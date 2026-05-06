param(
    [string]$ProjectPath = ".\EntraID.ASP.NETCore.Board.csproj",
    [string]$PublishProfile = "Properties/PublishProfiles/VSCode-AppService.pubxml",
    [string]$Configuration = "Release",
    [string]$MsDeployProfilePath = "Properties/PublishProfiles/MSAL-DEMO-TOBE - Web Deploy.pubxml",
    [string]$AppNameSuffix = "",
    [string]$AzureLocation = "koreacentral",
    [string[]]$AppServicePlanSkus = @("B1", "S1", "F1", "D1"),
    [int]$DeployTimeoutMs = 900000,
    [int]$DeployRetryCount = 2,
    [string]$OutputDir = ".\bin\Release\net10.0\publish",
    [string]$ZipPath = ".\bin\Release\net10.0\publish\appservice.zip",
    [switch]$SkipDeploy
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function ConvertTo-AppServiceNameSegment {
    param(
        [AllowEmptyString()]
        [string]$Value
    )

    $normalizedValue = $Value.ToLowerInvariant()
    $normalizedValue = $normalizedValue -replace "[^a-z0-9-]", "-"
    $normalizedValue = $normalizedValue -replace "-+", "-"
    return $normalizedValue.Trim("-")
}

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $projectRoot

$resolvedMsDeployProfilePath = Join-Path $projectRoot $MsDeployProfilePath
if (-not (Test-Path $resolvedMsDeployProfilePath)) {
    throw "MSDeploy 프로필 파일을 찾을 수 없습니다: $MsDeployProfilePath"
}

[xml]$msDeployXml = Get-Content -Path $resolvedMsDeployProfilePath -Raw
$ResourceGroup = [string]$msDeployXml.Project.PropertyGroup.ResourceGroup
$AppName = [string]$msDeployXml.Project.PropertyGroup.DeployIisAppPath
$ResourceId = [string]$msDeployXml.Project.PropertyGroup.ResourceId

if ([string]::IsNullOrWhiteSpace($AppName) -and -not [string]::IsNullOrWhiteSpace($ResourceId)) {
    if ($ResourceId -match "/sites/([^/]+)$") {
        $AppName = $Matches[1]
    }
}

if ([string]::IsNullOrWhiteSpace($ResourceGroup) -or [string]::IsNullOrWhiteSpace($AppName)) {
    throw "MSDeploy 프로필에서 ResourceGroup 또는 AppName을 읽지 못했습니다: $MsDeployProfilePath"
}

Write-Host "[1/4] dotnet publish 실행"
dotnet publish $ProjectPath -c $Configuration /p:PublishProfile="$PublishProfile"

if (-not (Test-Path $OutputDir)) {
    $fallbackOutputDir = ".\bin\$Configuration\net10.0\publish"
    if (Test-Path $fallbackOutputDir) {
        $OutputDir = $fallbackOutputDir
    }
    else {
        throw "게시 출력 폴더를 찾을 수 없습니다: $OutputDir"
    }
}

Write-Host "[2/4] 기존 zip 정리"
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

Write-Host "[3/4] zip 생성"
Compress-Archive -Path (Join-Path $OutputDir "*") -DestinationPath $ZipPath -Force

Write-Host "[4/4] Azure 리소스 확인 및 생성"

# Azure CLI 설치 확인
Write-Host "  - Azure CLI 확인..."
$azPath = Get-Command az -ErrorAction SilentlyContinue
if (-not $azPath) {
    Write-Host ""
    Write-Host "오류: Azure CLI가 설치되지 않았습니다."
    Write-Host ""
    Write-Host "해결 방법:"
    Write-Host "1. https://learn.microsoft.com/ko-kr/cli/azure/install-azure-cli에서 Azure CLI 설치"
    Write-Host "2. 설치 후 PowerShell 재시작"
    Write-Host "3. 다시 이 스크립트 실행"
    Write-Host ""
    throw "Azure CLI 필수"
}
Write-Host "    Azure CLI 설치됨"

# 로그인 상태 확인
Write-Host "  - Azure 로그인 상태 확인..."
$currentUser = az account show --query user.name -o tsv 2>$null
if ([string]::IsNullOrWhiteSpace($currentUser)) {
    throw "Azure에 로그인하지 않았습니다. 'az login'을 실행하세요."
}
Write-Host "    로그인 사용자: $currentUser"

$baseAppName = ConvertTo-AppServiceNameSegment $AppName
if ([string]::IsNullOrWhiteSpace($baseAppName)) {
    throw "기본 AppName을 Azure Web App 이름 형식으로 변환하지 못했습니다: $AppName"
}

$effectiveAppNameSuffix = $AppNameSuffix
if ([string]::IsNullOrWhiteSpace($effectiveAppNameSuffix)) {
    $suffixSource = $currentUser
    if ($currentUser -match "^([^@]+)@") {
        $suffixSource = $Matches[1]
    }

    $effectiveAppNameSuffix = ConvertTo-AppServiceNameSegment $suffixSource
}

if ([string]::IsNullOrWhiteSpace($effectiveAppNameSuffix)) {
    throw "AppNameSuffix를 자동 생성하지 못했습니다. -AppNameSuffix 파라미터로 직접 지정하세요."
}

$maxAppNameLength = 60
$AppName = "$baseAppName-$effectiveAppNameSuffix"
if ($AppName.Length -gt $maxAppNameLength) {
    $AppName = $AppName.Substring(0, $maxAppNameLength).TrimEnd('-')
}

Write-Host "    배포 대상 앱 이름: $AppName"

# 리소스 그룹 확인 및 생성
Write-Host "  - 리소스 그룹 확인: $ResourceGroup"
$rgExists = az group exists --name $ResourceGroup
if ($rgExists -eq "false") {
    Write-Host "    리소스 그룹 생성 중: $ResourceGroup"
    az group create --name $ResourceGroup --location $AzureLocation | Out-Null
    Write-Host "    리소스 그룹 생성 완료"
}
else {
    Write-Host "    리소스 그룹 이미 존재"
}

# App Service Plan 확인 및 생성
$AppServicePlanName = "easm-msal-demo-plan"
Write-Host "  - App Service Plan 확인: $AppServicePlanName"
$planExists = az appservice plan list --resource-group $ResourceGroup --query "[?name=='$AppServicePlanName'].name" -o tsv
if ([string]::IsNullOrWhiteSpace($planExists)) {
    $normalizedPlanSkus = $AppServicePlanSkus | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    if (-not $normalizedPlanSkus -or $normalizedPlanSkus.Count -eq 0) {
        throw "AppServicePlanSkus 파라미터가 비어 있습니다. 예: -AppServicePlanSkus B1,S1"
    }

    Write-Host "    App Service Plan 생성 중: $AppServicePlanName"
    $planCreateErrors = New-Object System.Collections.Generic.List[string]
    $planCreated = $false

    foreach ($planSku in $normalizedPlanSkus) {
        Write-Host "    - SKU 시도: $planSku"
        $planCreateOutput = az appservice plan create --name $AppServicePlanName --resource-group $ResourceGroup --location $AzureLocation --sku $planSku 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "    App Service Plan 생성 완료 (SKU: $planSku)"
            $planCreated = $true
            break
        }

        $planCreateErrors.Add("[SKU=$planSku] $($planCreateOutput -join " ")")
        Write-Host "    SKU 실패: $planSku"
    }

    if (-not $planCreated) {
        $combinedPlanError = $planCreateErrors -join "`n"
        if ($combinedPlanError -match "without additional quota") {
            throw "App Service Plan 생성 실패(쿼터 부족): $combinedPlanError`n조치: 1) Azure Portal에서 Korea Central App Service 쿼터 증가 요청 2) 임시로 다른 리전 지정(-AzureLocation japaneast)"
        }

        throw "App Service Plan 생성 실패: $combinedPlanError"
    }
}
else {
    Write-Host "    App Service Plan 이미 존재"
}

$planExistsAfterCreate = az appservice plan show --name $AppServicePlanName --resource-group $ResourceGroup --query name -o tsv 2>$null
if ([string]::IsNullOrWhiteSpace($planExistsAfterCreate)) {
    throw "App Service Plan을 확인할 수 없습니다. ResourceGroup=$ResourceGroup, PlanName=$AppServicePlanName"
}

# Web App 확인 및 생성
Write-Host "  - Web App 확인: $AppName"
$appExists = az webapp show --resource-group $ResourceGroup --name $AppName --query name -o tsv 2>$null
if ([string]::IsNullOrWhiteSpace($appExists)) {
    Write-Host "    Web App 생성 중: $AppName"
    $createOutput = az webapp create --resource-group $ResourceGroup --plan $AppServicePlanName --name $AppName 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Web App 생성 실패: $($createOutput -join "`n")"
    }
    Write-Host "    Web App 생성 완료"
}
else {
    Write-Host "    Web App 이미 존재"
}

$appExistsAfterCreate = az webapp show --resource-group $ResourceGroup --name $AppName --query name -o tsv 2>$null
if ([string]::IsNullOrWhiteSpace($appExistsAfterCreate)) {
    throw "Web App을 확인할 수 없습니다. ResourceGroup=$ResourceGroup, AppName=$AppName"
}

# App Service 배포
Write-Host "  - 배포 시작"

if ($SkipDeploy) {
    Write-Host "  배포 건너뜀 (-SkipDeploy 지정)"
    Write-Host "  리소스 준비 완료"
    Write-Host "  생성된 zip: $ZipPath"
    exit 0
}

$maxAttempt = [Math]::Max(1, $DeployRetryCount)
$deploySucceeded = $false
$deployErrors = New-Object System.Collections.Generic.List[string]

for ($attempt = 1; $attempt -le $maxAttempt; $attempt++) {
    Write-Host "  - 배포 시도: $attempt/$maxAttempt (timeout=${DeployTimeoutMs}ms)"
    $deployOutput = az webapp deploy --resource-group $ResourceGroup --name $AppName --src-path $ZipPath --type zip --timeout $DeployTimeoutMs --only-show-errors 2>&1
    if ($LASTEXITCODE -eq 0) {
        $deploySucceeded = $true
        break
    }

    $deployText = $deployOutput -join "`n"
    $deployErrors.Add("[attempt=$attempt] $deployText")

    if ($deployText -notmatch "Status Code:\s*504") {
        break
    }

    Write-Host "  - 배포 API 타임아웃(504) 감지, 재시도합니다."
}

if (-not $deploySucceeded) {
    throw "배포 실패: $($deployErrors -join "`n")"
}

Write-Host "완료: $AppName 배포 성공"
Write-Host "앱 URL: https://$AppName.azurewebsites.net"
