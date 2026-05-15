using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using EntraID.ASP.NETCore.Board.Models;

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
				setNoAuthn();
			else
			{
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
				SameSite = SameSiteMode.None,
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
			if (!UserManager.Authenticate(username, password))
			{
				setNoAuthn();
				return false;
			}

			var userInfo = UserManager.GetUserInfo(username);
			if (userInfo == null)
			{
				setNoAuthn();
				return false;
			}
			
			saveSignIn(userInfo.DisplayName, userInfo.UserName, true, DateTime.Now, response);
			return true;
		}

		public void Logout(HttpResponse response)
		{
			response.Cookies.Delete(SignInCookieName);
			setNoAuthn();
		}
		
		
	}
}