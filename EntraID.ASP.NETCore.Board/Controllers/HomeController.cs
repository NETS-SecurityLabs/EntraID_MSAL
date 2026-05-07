using System;
using Microsoft.AspNetCore.Mvc;

using EntraID.ASP.NETCore.Board.Models;
using EntraID.ASP.NETCore.Board.ViewModels;

namespace EntraID.ASP.NETCore.Board.Controllers
{
	public class HomeController : BoardBaseController
	{
		[HttpGet]
		public IActionResult Login(string returnUrl)
		{
			// 로그인 완료 후에 최종적으로 되돌아갈 URL이 전달되지 않았을 경우에
			// 게시판 목록 조회 페이지로 기본 설정한다.
			if (string.IsNullOrWhiteSpace(returnUrl))
			{
				returnUrl = Url.Action("Index", "Board");
			}

			if (signInManager.IsAuthenticated)
			{
				// 이미 인증되어 있기 때문에, 게시판목록 페이지로 이동한다.
				return Redirect(returnUrl);
			}

			LoginViewModel model = new LoginViewModel(signInManager.LoginSession);
			model.InvalidCredential = false;
			model.ReturnUrl = returnUrl;

			return View(model);
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
			{
				return Redirect(returnUrl);
			}
		}

		public ActionResult Logout()
		{
			signInManager.Logout(Response);
			return RedirectToAction("Index", "Board");
		}
	}
}
