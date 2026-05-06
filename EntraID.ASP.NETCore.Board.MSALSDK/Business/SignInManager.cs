using EntraID.ASP.NETCore.Board.Models;

using Microsoft.AspNetCore.Http;

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace EntraID.ASP.NETCore.Board.Business
{
	public class SignInManager
	{
		public SignInManager()
		{
			LoginSession = new LoginSession()
			{
				DisplayName = string.Empty,
				IsAuthenticated = false,
				UserName = string.Empty,
				WhenLogin = DateTime.Now,
			};
		}
		public bool IsAuthenticated => LoginSession.IsAuthenticated;
		
		public LoginSession LoginSession{ get; private set; }

		private const string SignInCookieName = "SignInfo";
		public void CheckSignIn(HttpRequest request)
		{
			var signInCookie = request.Cookies[SignInCookieName];
			if (signInCookie == null)
			{
				// 사용자 인증 쿠키가 존재하지 않기 때문에, 미 인증 상태를 설정한다.
				setNoAuthn();
			}
			else
			{
				// 사용자 인증 쿠키가 존재하기 때문에, 이 쿠키로 부터 사용자의 인증 세션 정보를
				// 복원한다.
				var sessionInfoJson = Convert.FromBase64String(signInCookie);
				LoginSession = JsonSerializer.Deserialize<LoginSession>(sessionInfoJson);
			}
		}

		private void saveSignIn(string displayName, string userName, bool isAuthenticated, DateTime whenLogin, HttpResponse response)
		{
			if (LoginSession == null)
			{
				LoginSession = new LoginSession();
			}

			LoginSession.DisplayName = displayName;
			LoginSession.IsAuthenticated = isAuthenticated;
			LoginSession.UserName = userName;
			LoginSession.WhenLogin = whenLogin;
			var sessionCookie = JsonSerializer.Serialize<LoginSession>(LoginSession);
			var persistCookie = Convert.ToBase64String(Encoding.UTF8.GetBytes(sessionCookie));

			response.Cookies.Append(SignInCookieName, persistCookie, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None,	// SameSite 설정 주의
				Path = "/"
			});
		}

		private void setNoAuthn()
		{
			if (LoginSession == null)
			{
				LoginSession = new LoginSession();
			}

			LoginSession.DisplayName = string.Empty;
			LoginSession.IsAuthenticated = false;
			LoginSession.UserName = string.Empty;
			LoginSession.WhenLogin = DateTime.Now;
		}

		public bool Login(string username, string password, HttpResponse response)
		{
			if (UserManager.Authenticate(username, password))
			{
				var userInfo = UserManager.GetUserInfo(username);
				if (userInfo != null)
				{
					saveSignIn(userInfo.DisplayName, userInfo.UserName, true, DateTime.Now, response);
				}
				return true;
			}
			else
			{
				setNoAuthn();
				return false;
			}
		}

		/// <summary>
		/// MSAL SDK를 이용해서 사용자 인증을 처리하는 메소드이다.
		/// </summary>
		/// <param name="userPrincipal">
		/// MSAL SDK가 제공하는 사용자 인증 정보
		/// </param>
		/// <param name="response">
		/// 샘플 앱 고유의 인증 정보를 쿠키로 발행하기 위해서 사용.
		/// </param>
		/// <returns></returns>
		public bool Login(IPrincipal userPrincipal, HttpResponse response)
		{
			if(!userPrincipal.Identity.IsAuthenticated)
			{
				return false;
			}

			var claimsIdentity = (userPrincipal.Identity as ClaimsIdentity);
			//var displayName = claimsIdentity.FindFirst("name").Value;
			//var userName = claimsIdentity.FindFirst("preferred_username").Value;

			// 샘플 앱은 원래 사용자 ID를 이용해서 인증을 수행했다.
			// Entra ID로 인증을 받은 경우에는 사용자 ID가 아니라, Entra ID에 등록된 사용자의 UPN이 사용자를 위한 인증 식별자로 사용된다.
			// 샘플 앱에서 사용하는 사용자 정보(userinfos.json)에는 사용자 정보 중에 UPN이 존재하지 않는다. 
			// 샘플 앱 사용자 정보와 Entra ID 인증 정보에 공통으로 포함되어 있는 것은 이메일이다. 
			// 샘플 앱은 이메일을 이용해서 사용자를 매칭하도록 한다.
			var email = claimsIdentity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
			UserInfo userInfo = UserManager.GetUserInfoByEmail(email);
			if(userInfo == null)
			{
				// Entra ID에서 반환한 인증 정보의 이메일에 해당하는 샘플 앱 사용자 정보가 존재하지 않기 때문에, 샘플 앱 입장에서는
				// 사용자 인증에 실패한 경우가 된다. 
				return false;
			}
			saveSignIn(userInfo.DisplayName, userInfo.UserName, true, DateTime.Now, response);
			return true;
		}

		public void Logout(HttpResponse response)
		{
			response.Cookies.Delete(SignInCookieName, new CookieOptions
			{
				SameSite = SameSiteMode.None, // SameSite 설정 주의
				HttpOnly = true,
				Secure = true,
				Path = "/",
				Expires = new DateTime(1970, 1, 1)
			});
			setNoAuthn();
		}
	}
}