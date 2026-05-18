# 🚀 HOL Step-by-Step

### ✅ Step 1: Entra ID 정보 확인 및 App Registration 등록 (1/7)


### ✅ Step 2: Microsoft.Identity.Web 패키지 추가 (2/7)
1. 다음 내용을 복사합니다.
```xml
    <PackageReference Include="Microsoft.Identity.Web" Version="4.8.0" />
```
2. 아래 링크를 클릭하여 붙여넣기 합니다.
- [/EntraID.ASP.NETCore.Board.csproj(#9)](./EntraID.ASP.NETCore.Board.csproj#L9)
<br/>
3. 변경 후 ✔️ 
```diff
8: <ItemGroup>
+9:     <PackageReference Include="Microsoft.Identity.Web" Version="4.8.0" />
10: </ItemGroup>
```

### ✅ Step 3: MSAL Entra ID 연결 정보 추가  (3/7)
1. 다음 내용을 복사합니다.
```json
,"EntraID": {
    "ClientID": "{ClientID}",
    "TenantID": "{TenantID}",
    "Instance": "https://login.microsoftonline.com/"
  }
```
2. 아래 링크를 클릭하여 붙여넣기 합니다. Demo 진행과정에서 확인된 Entra ID 테넌트 ID와 App Registration의 Client ID로 교체합니다.
- [/appsettings.json(#12)](appsettings.json#L12)
<br/>

3. 변경 후 ✔️ 
```diff
{
        ...
9:	"AllowedHosts": "*",
10:	"UserInfo": "UserInfos.json",
11:	"BoardData": "BoardData.json"
+12:    , "EntraID": {
+13:		"ClientID": "{ClientID}",
+14:		"TenantID": "{TenantID}",
+15:		"Instance": "https://login.microsoftonline.com/"
+16:	}
} 
```

### ✅ Step 4: 컨테이너에 MSAL 인증 서비스 등록  (4/7)
1. 다음 내용을 복사합니다.
```csharp
using Microsoft.Identity.Web;
```
2. 아래 링크를 클릭하여 붙여넣기 합니다.
- [/Startup.cs(#7)](Startup.cs#L7)
<br/>

3. 변경 후 ✔️ 
```diff
 6: using Microsoft.AspNetCore.HttpOverrides;
+7: using Microsoft.Identity.Web;
 8: 
 9: namespace EntraID.ASP.NETCore.Board
```

4. 다음 내용을 복사합니다.
```csharp
     		services
      			.AddAuthentication(Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme)
      			.AddMicrosoftIdentityWebApp(idOptions => 
      			{
          			idOptions.TenantId = Configuration.GetValue<string>("EntraID:TenantID");
          			idOptions.ClientId = Configuration.GetValue<string>("EntraID:ClientID");   
          			idOptions.Instance = Configuration.GetValue<string>("EntraID:Instance");
      			});
```

5. 아래 링크를 클릭하여 붙여넣기 합니다.
- [/Startup.cs(#23)](Startup.cs#L23)
<br/>

6. 변경 후 ✔️ 
```diff
 27: public void ConfigureServices(IServiceCollection services)
 28: {
+29:     services
+30:        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
+31:        .AddMicrosoftIdentityWebApp(idOptions => 
+32:        {
+33:            idOptions.TenantId = Configuration.GetValue<string>("EntraID:TenantID");
+34:            idOptions.ClientId = Configuration.GetValue<string>("EntraID:ClientID");   
+35:            idOptions.Instance = Configuration.GetValue<string>("EntraID:Instance");
+36:        });
 37:    services.AddControllersWithViews();
 38:    // 앱의 게시판 기능 추가
 39:    services.AddSingleton<BoardManager>(new BoardManager(Configuration.GetValue<string>("BoardData")));
 40: }
```

### ✅ Step 5: MSAL 인증 흐름 시작 & 흐름 완료 코드 추가  (5/7)
1. 다음 내용을 복사합니다.
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
		public IActionResult SignIn(string returnUrl)
		{
			if(string.IsNullOrWhiteSpace(returnUrl))
				returnUrl = Url.Action("Index", "Board");
			if(signInManager.IsAuthenticated)
				return Redirect(returnUrl);
			else
				return Challenge( new AuthenticationProperties { RedirectUri = Url.Action(nameof(SignInCompleted), new { returnUrl = HttpUtility.UrlEncode(returnUrl) }) }, OpenIdConnectDefaults.AuthenticationScheme);
		}

		public IActionResult SignInCompleted(string returnUrl)
		{
			if(!User.Identity.IsAuthenticated)
				throw new Exception("MSAL 인증이 완료되었지만, ASP.NET Core 인증이 되지 않았습니다.");
			else
			{
				if(!signInManager.Login(User, Response))
					throw new Exception("MSAL 인증은 완료되었지만, 인증을 완료한 사용자는 없는 사용자 입니다.");

				if (string.IsNullOrWhiteSpace(returnUrl))
					returnUrl = Url.Action("Index", "Board");

				return Redirect(HttpUtility.UrlDecode(returnUrl));
			}
		}
	}
}
```
2. 아래 링크를 클릭하여 붙여넣기 합니다.
- [/Controllers/MsalAuthenticationController.cs(#1)](./Controllers/MsalAuthenticationController.cs#L1)
<br/>

3. 변경 후 ✔️ 
```diff
+ (빈 파일에서 복사한 내용 전체 추가)
```
### ✅ Step 6: MSAL 인증 완료 확인  (6/7)
1. 다음 내용을 복사합니다.
```csharp
        public bool Login(IPrincipal userPrincipal, HttpResponse response)
		{
			if(!userPrincipal.Identity.IsAuthenticated)
			{
				setNoAuthn();
				return false;
			}

			var claimsIdentity = (userPrincipal.Identity as ClaimsIdentity);
			var userName = claimsIdentity.FindFirst(Microsoft.Identity.Web.ClaimConstants.PreferredUserName).Value; 
			UserInfo userInfo = UserManager.GetUserInfoByEmail(userName);

			if(userInfo == null)
			{
				setNoAuthn();
				return false;
			}
			
			saveSignIn(userInfo.DisplayName, userInfo.UserName, true, DateTime.Now, response);
			return true;
		}
```
2. 아래 링크를 클릭하여 붙여넣기 합니다.
- [/Business/SignInManager.cs(#100)](./Business/SignInManager.cs#L100)
<br/>

3. 변경 후 ✔️ 
```diff
+100:  public bool Login(IPrincipal userPrincipal, HttpResponse response)
+101:	{
+102:		if(!userPrincipal.Identity.IsAuthenticated)
+103:		{
+104:			setNoAuthn();
+105:			return false;
+106:		}
+107:
+108:		var claimsIdentity = (userPrincipal.Identity as ClaimsIdentity);
+109:		var userName = claimsIdentity.FindFirst(Microsoft.Identity.Web.ClaimConstants.PreferredUserName).Value; 
+110:		UserInfo userInfo = UserManager.GetUserInfoByEmail(userName);
+111:
+112:		if(userInfo == null)
+113:		{
+114:			setNoAuthn();
+115:			return false;
+116:		}
+117:		
+118:		saveSignIn(userInfo.DisplayName, userInfo.UserName, true, DateTime.Now, response);
+119:		return true;
+120:	}
```
### ✅ Step 7: 인증 진입점 및 MSAL 로그아웃 적용  (7/7)
1. 다음 내용을 복사합니다.
```csharp
     		return RedirectToAction("SignIn", "MsalAuthentication", new { returnUrl = returnUrl });
```
2. 아래 링크를 클릭하여 붙여넣기 합니다. AS-IS 로그인 페이지로 이동하는 19행은 //를 코드 앞에 붙여 주석처리를 한 후 20행에 새로운 내용을 붙여넣기 합니다.
- [/Controllers//HomeController.cs(#19)](./Controllers//HomeController.cs#L19)
<br/>

3. 변경 후 ✔️ 
```diff
-19: return View(new LoginViewModel(signInManager.LoginSession) { InvalidCredential = false, ReturnUrl = returnUrl})
+19: //return View(new LoginViewModel(signInManager.LoginSession) { InvalidCredential = false, ReturnUrl = returnUrl})
+20: return RedirectToAction("SignIn", "MsalAuthentication", new { returnUrl = returnUrl });
```

4. 다음 내용을 복사합니다.
```csharp
			return SignOut(
				new Microsoft.AspNetCore.Authentication.AuthenticationProperties {RedirectUri = Url.Action("Index", "Board")},
				Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme,
				Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
```

5. 아래 링크를 클릭하여 붙여넣기 합니다.
- [/Controllers//HomeController.cs(#28)](./Controllers//HomeController.cs#L28)
<br/>

6. 변경 후 ✔️ 
```diff
-28: return RedirectToAction("Index", "Board");
+28: //return RedirectToAction("Index", "Board");
+29: return SignOut(
+30:	new Microsoft.AspNetCore.Authentication.AuthenticationProperties {RedirectUri = Url.Action("Index", "Board")},
+31:     	Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme,
+32:		Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
```