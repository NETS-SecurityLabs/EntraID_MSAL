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