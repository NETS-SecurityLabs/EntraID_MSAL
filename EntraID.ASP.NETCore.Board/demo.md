# 🚀 Demo Step-by-Step

### ✅ Step 1: Microsoft.Identity.Web 패키지 추가 (1/7)
- ./EntraID.ASP.NETCore.Board.csproj
```xml
<ItemGroup>
  <PackageReference
    Include="Microsoft.Identity.Web"
    Version="4.8.0" />
</ItemGroup>
```
### ✅ Step 2: MSAL 연결 정보 추가  (2/7)
- ./appsettings.json
```json
//Demo 진행과정에서 생성한 App Registration의 Client ID로 교체합니다.
,"EntraID": {
    "ClientID": "a1168e5b-47ca-4ad9-8a7a-ae170c420546",
    "TenantID": "625438f2-aa00-4fcf-a457-90348974057a",
    "Instance": "https://login.microsoftonline.com/"
  }
```
### ✅ Step 3: 컨테이너에 MSAL 인증 서비스 등록  (3/7)
- ./Startup.cs
```csharp
     services
      .AddAuthentication
       (OpenIdConnectDefaults.AuthenticationScheme)
      .AddMicrosoftIdentityWebApp(idOptions =>
       {
          idOptions.TenantId =
             Configuration.GetValue<string>("EntraID:TenantID");
          idOptions.ClientId =  
             Configuration.GetValue<string>("EntraID:ClientID");   
          idOptions.Instance = 
             Configuration.GetValue<string>("EntraID:Instance");
         });
```
### ✅ Step 4: MSAL 인증 수행 신규 파일 생성  (4/7)
- ./Controllers/MsalAuthenticationController.cs
```csharp
    using System;
    using System.Web;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    
    namespace EntraID.ASP.NETCore.Board.Controllers
    {
      public class MsalAuthenticationController : BoardBaseController
      {
        //OIDC 인증 시작 요청을 보내는 메서드를 추가합니다.
        //인증 완료 후 후속 처리를 위해 이후에 구현할 SignInCompleted 메서드로 이동하도록 코드에서 지정하고 있습니다.        
        public IActionResult SignIn(string returnUrl)
        {
            if(string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Url.Action("Index", "Board");
            }
            if(signInManager.IsAuthenticated)
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Challenge(new AuthenticationProperties {
                RedirectUri =Url.Action("SignInCompleted", new { returnUrl = HttpUtility.UrlEncode(returnUrl) })
            }, OpenIdConnectDefaults.AuthenticationScheme);
            }
        }
          
        //인증 흐름 완료 후 인증에 실패했거나 인증이 완료된 사용자가 자체 저장소에 존재하지 않는 사용자인지 검사하는 코드를 호출합니다.
        public IActionResult SignInCompleted(string returnUrl)
            {
        if(!User.Identity.IsAuthenticated)  {
        throw new Exception(@"MSAL 인증이 완료되었지만,
        ASP.NET Core 인증이 되지 않았습니다.");
        }
        else  {
        if(!signInManager.Login(User, Response)) {
            throw new Exception(@"MSAL 인증은 완료되었지만,
        인증을 완료한 사용자는 없는 사용자 입니다.");
        }
        
        if (string.IsNullOrWhiteSpace(returnUrl)) {
            returnUrl = Url.Action("Index", "Board");
        }
    
        return Redirect(HttpUtility.UrlDecode(returnUrl));
        }
            
          
      }
    }
```