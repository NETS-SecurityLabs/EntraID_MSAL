using EntraID.ASP.NETCore.Board.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
				// 이미 인증되어 있기 때문에, 전달된 returnUrl로 페이지를 이동한다.
				return Redirect(returnUrl);
			}

			// MSAL SDK를 적용하여 사용자를 인증하도록 변경하기 위해서 MsalAuthenticationController의 SignIn 액션으로
			// 리다이렉트한다.
			return RedirectToAction("SignIn", "MsalAuthentication", new { returnUrl = returnUrl });
		}
	}
}
