using System;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using EntraID.ASP.NETCore.Board.ViewModels;

namespace EntraID.ASP.NETCore.Board.Controllers
{
	public class HomeController : BoardBaseController
	{
		[HttpGet]
		public IActionResult Login(string returnUrl)
		{
			if (string.IsNullOrWhiteSpace(returnUrl))
				returnUrl = Url.Action("Index", "Board");

			if (signInManager.IsAuthenticated)
				return Redirect(returnUrl);
			
			return RedirectToAction("SignIn", "MsalAuthentication", new { returnUrl = returnUrl });
		}

		public ActionResult Logout()
		{
			signInManager.Logout(Response);

			return SignOut(
				new Microsoft.AspNetCore.Authentication.AuthenticationProperties {RedirectUri = Url.Action("Index", "Board")},
				Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme,
				Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
		}
	}
}
