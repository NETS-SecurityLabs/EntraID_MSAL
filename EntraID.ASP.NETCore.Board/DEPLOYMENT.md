# Azure App Service 배포 가이드

이 문서는 EntraID.ASP.NETCore.Board 프로젝트를 Azure App Service에 배포하는 방법을 설명합니다.

## 목차
1. [필수 요구사항](#필수-요구사항)
2. [빠른 시작](#빠른-시작)
3. [단계별 배포](#단계별-배포)
4. [배포 옵션](#배포-옵션)
5. [문제 해결](#문제-해결)

---

## 필수 요구사항

### 1. Azure CLI 설치
- [Azure CLI 설치 페이지](https://learn.microsoft.com/ko-kr/cli/azure/install-azure-cli)에서 설치
``` powershell 을 통한 x64 azureCLI 설치 스크립트 (위 경로에서 직접 설치하거나 아래 스크립트를 통해 설치진행)
$ProgressPreference = 'SilentlyContinue'
Invoke-WebRequest -Uri https://aka.ms/installazurecliwindowsx64 -OutFile .\AzureCLI.msi
Start-Process msiexec.exe -Wait -ArgumentList '/I', 'AzureCLI.msi', '/quiet'
Remove-Item .\AzureCLI.msi
```
- 설치 후 PowerShell/터미널 재시작


설치 확인:
```powershell
az --version
```

### 2. Azure 계정
- 활성 Azure 구독
- App Service를 만들 수 있는 권한

### 3. 로컬 개발 환경
- .NET 10.0 SDK
- Visual Studio Code 또는 선호하는 에디터

---

## 빠른 시작
**VS Code 사용자:**
1. VS Code 탐색기에서 프로젝트 폴더(이 DEPLOYMENT.md가 있는 폴더) 선택
2. 우클릭 → **"터미널에서 열기"** (또는 "Open in Integrated Terminal")
3. 아래 명령 실행


### 1단계: Azure 로그인
```powershell
az login
```
브라우저에서 Azure 계정으로 로그인합니다.

### 2단계: 프로젝트 루트에서 배포 실행
```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\deploy-appservice.ps1
```

**기타 터미널 사용자:**
자신의 소스 경로에 있는 프로젝트 루트로 이동 후 위 명령을 실행하세요.

끝입니다! 배포가 자동으로 진행됩니다.

기본 동작:
- Resource Group 신규 생성 위치: koreacentral
- App Service Plan SKU 후보: B1, S1, F1, D1 순서로 시도
- App Service 이름: 기본 앱 이름 + 로그인 사용자 UPN alias suffix

---

## 단계별 배포

### Step 1: Azure CLI 설치 및 로그인

**Windows 사용자:**
1. [Azure CLI 설치 페이지](https://learn.microsoft.com/ko-kr/cli/azure/install-azure-cli)에서 MSI 다운로드
2. 설치 프로그램 실행
3. PowerShell 재시작

**설치 확인:**
```powershell
az --version
```

**Azure 로그인:**
```powershell
az login
```
브라우저 창이 열리고 Azure 계정으로 로그인하면 됩니다.

### Step 2: 프로젝트 루트에서 배포 스크립트 실행

VS Code 탐색기에서 프로젝트 루트 폴더를 우클릭 → **"터미널에서 열기"**

그 후 다음 명령을 실행합니다:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\deploy-appservice.ps1
```

**배포 스크립트가 자동으로 수행하는 작업:**
1. ✅ dotnet publish 실행 (Release 모드)
2. ✅ 배포 zip 파일 생성
3. ✅ 리소스 그룹 확인/생성 (필요시, 기본 위치: koreacentral)
4. ✅ App Service Plan 확인/생성 (필요시, SKU 후보: B1/S1/F1/D1)
5. ✅ Web App 확인/생성 (필요시)
6. ✅ Azure App Service에 배포

### Step 3: 배포 확인

배포가 완료되면 다음 출력이 나타납니다:
```
배포 대상 앱 이름: msal-demo-asis-<alias>
완료: msal-demo-asis-<alias> 배포 성공
```

배포된 앱에 접속:
- https://msal-demo-asis-<alias>.azurewebsites.net

예시:
- 로그인 사용자: user@example.com
- 자동 생성 alias: user
- 최종 App Service 이름: msal-demo-asis-user

---

## 배포 옵션

### 옵션 1: 패키지만 생성 (배포 제외)

리소스 준비 상태를 먼저 확인하고 싶을 때:
```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\deploy-appservice.ps1 -SkipDeploy
```

**출력 예시:**
```
[4/4] Azure 리소스 확인 및 생성
  - Azure CLI 확인...
    Azure CLI 설치됨
  - Azure 로그인 상태 확인...
    로그인 사용자: user@example.com
    배포 대상 앱 이름: msal-demo-asis-user
  - 리소스 그룹 확인: easm-msal-demo
    리소스 그룹 이미 존재
  - App Service Plan 확인: easm-msal-demo-plan
    App Service Plan 이미 존재
  - Web App 확인: msal-demo-asis-user
    Web App 이미 존재
  배포 건너뜀 (-SkipDeploy 지정)
  리소스 준비 완료
  생성된 zip: .\bin\Release\net10.0\publish\appservice.zip
```

### 옵션 2: App Service 이름 suffix 수동 지정

기본적으로 App Service 이름은 로그인한 Azure 사용자의 UPN alias를 suffix로 사용합니다.

예:
- 로그인 사용자: user@example.com
- 기본 앱 이름: MSAL-DEMO-ASIS
- 최종 앱 이름: msal-demo-asis-user

필요하면 suffix를 직접 지정할 수 있습니다:
```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\deploy-appservice.ps1 -AppNameSuffix demo1
```

이 경우 최종 앱 이름 예시는 다음과 같습니다:
- msal-demo-asis-demo1

### 옵션 3: App Service Plan SKU/리전 지정

기본적으로 리소스 그룹은 koreacentral에 생성되고, App Service Plan은 B1, S1, F1, D1 순서로 생성 시도를 합니다.

필요하면 직접 지정할 수 있습니다:
```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\deploy-appservice.ps1 -AzureLocation koreacentral -AppServicePlanSkus S1
```

### 옵션 4: VS Code Tasks 사용

VS Code에서 Command Palette 실행: `Ctrl+Shift+P`

```
Tasks: Run Task
```

선택 가능한 Task:
- **Deploy: AppService (Zip)** - 전체 배포 (권장)
- **Package Only: AppService Zip** - 패키지만 생성
- **Publish: AppService Profile** - dotnet publish만 실행

---

## 배포 설정 상세

### 게시 프로필 정보

**파일:** `Properties/PublishProfiles/VSCode-AppService.pubxml`

이 프로필은:
- 로컬 Release 폴더에 앱 게시
- 모든 파일 포함
- 불필요한 파일 제외

### 배포 대상 자동 감지

배포 스크립트는 기존 MSDeploy 프로필에서 자동으로 다음 정보를 읽습니다:
- **리소스 그룹:** `easm-msal-demo`
- **기본 App 이름:** `MSAL-DEMO-ASIS`

이는 [MSDeploy 프로필](Properties/PublishProfiles/MSAL-DEMO-ASIS%20-%20Web%20Deploy.pubxml)에서 읽어옵니다.

실제 생성/사용되는 App Service 이름은 다음 규칙을 따릅니다:
- 기본 App 이름을 소문자/하이픈 형식으로 정규화
- 로그인한 Azure 계정의 UPN alias를 suffix로 추가
- 최종 형식: `msal-demo-asis-<alias>`
- 필요 시 `-AppNameSuffix`로 수동 지정 가능

---

## 문제 해결

### 문제 1: "Azure CLI가 설치되지 않았습니다"

**원인:** Azure CLI가 설치되지 않았거나 PATH에 없음

**해결책:**
1. [Azure CLI 설치](https://learn.microsoft.com/ko-kr/cli/azure/install-azure-cli)
2. PowerShell 재시작
3. 다시 배포 스크립트 실행

```powershell
# 설치 확인
az --version
```

### 문제 2: "Azure에 로그인하지 않았습니다"

**원인:** `az login`을 실행하지 않음

**해결책:**
```powershell
az login
```

브라우저에서 Azure 계정으로 로그인합니다.

### 문제 3: "리소스 그룹을 생성할 수 없습니다"

**원인:** Azure 구독 권한 부족

**해결책:**
1. Azure 포털에서 해당 리소스 그룹이 있는지 확인
2. 권한이 있는 구독에 로그인했는지 확인
```powershell
az account list
az account set --subscription "구독 ID"
```

### 문제 4: "배포에 실패했습니다"

**일반적인 원인:**
- 네트워크 연결 문제
- App Service 리소스 부족
- 디스크 공간 부족
- App Service Plan SKU 쿼터 부족

**해결책:**
```powershell
# 1. 로그인 상태 재확인
az login

# 2. 구독 확인
az account show

# 3. 리소스 그룹 상태 확인
az group show --name easm-msal-demo

# 4. App Service 상태 확인
az webapp show --resource-group easm-msal-demo --name msal-demo-asis-<alias>
```

**SKU 쿼터 부족 시:**
```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\deploy-appservice.ps1 -AppServicePlanSkus S1
```

또는 다른 리전으로 테스트:
```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\deploy-appservice.ps1 -AzureLocation japaneast
```

### 문제 5: PowerShell 실행 정책 오류

**오류:** "실행 정책을 확인할 수 없습니다"

**해결책:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

## 추가 정보

### 배포되는 파일들
- `bin/Release/net10.0/publish/` - 컴파일된 앱
- `bin/Release/net10.0/publish/appservice.zip` - 배포용 압축 파일

### 배포 대상 리소스
| 리소스 | 이름 | 위치 |
|--------|------|------|
| Resource Group | easm-msal-demo | koreacentral (기본값) |
| App Service Plan | easm-msal-demo-plan | koreacentral (기본값, SKU 후보: B1/S1/F1/D1) |
| Web App | msal-demo-asis-<alias> | - |

### 환경 설정

배포 후 App Service 설정:
1. Azure Portal에서 App Service 이동
2. **설정** > **구성**
3. **애플리케이션 설정** 탭에서 필요한 환경변수 추가

필요한 설정:
- `EntraID:TenantID`
- `EntraID:ClientID`
- `EntraID:Instance`
- `EntraID:CallbackPath`
- `EntraID:PostSignoutCallbackPath`

---

## 문의 및 지원

문제가 발생하면:
1. 위 **문제 해결** 섹션 확인
2. [Azure CLI 설명서](https://learn.microsoft.com/ko-kr/cli/azure/)
3. [App Service 문서](https://learn.microsoft.com/ko-kr/azure/app-service/)
