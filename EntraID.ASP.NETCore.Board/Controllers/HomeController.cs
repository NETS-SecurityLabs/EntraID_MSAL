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

			return View(new LoginViewModel(signInManager.LoginSession) { InvalidCredential = false, ReturnUrl = returnUrl});

		}

		public ActionResult Logout()
		{
			signInManager.Logout(Response);
			
			return RedirectToAction("Index", "Board");

		}

		[HttpPost]
		public ActionResult Login(string username, string password, string returnUrl)
		{
			if (!signInManager.Login(username, password, Response))
			{
				LoginViewModel model = new LoginViewModel(signInManager.LoginSession);
				model.InvalidCredential = true;
				model.ReturnUrl = returnUrl;
				return View(model);
			}
			else
				return Redirect(returnUrl);
		}
	}
}
