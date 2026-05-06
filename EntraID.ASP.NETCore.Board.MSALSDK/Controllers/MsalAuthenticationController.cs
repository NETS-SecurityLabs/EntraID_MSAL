using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Web;
using EntraID.ASP.NETCore.Board.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EntraID.ASP.NETCore.Board.Controllers
{
	public class MsalAuthenticationController : BoardBaseController
	{
		private readonly IConfiguration configuration;
		public MsalAuthenticationController(IConfiguration config)
		{
			configuration = config;
		}

		public IActionResult SignIn(string returnUrl)
		{
			if(string.IsNullOrWhiteSpace(returnUrl))
			{
				// 인증을 완료한 후에 되돌아 갈 URL이 전달되지 않을 경우에
				// 게시판 목록 조회 페이지로 이동할 수 있도록 한다.
				returnUrl = Url.Action("Index", "Board");
			}
			if(signInManager.IsAuthenticated)
			{
				// 이미 인증되어 있기 때문에, 전달된 returnUrl로 이동한다.
				return Redirect(returnUrl);
			}
			else
			{
				// MSAL SDK를 통해서 인증 요청을 하기 위해서  Challenge 메소드를 호출한다.
				// AuthenticationProperties의 RedirectUri 속성을 설정하는 목적은
				// MSAL SDK가 사용자 인증을 완료한 후에, 호출할 액션 메소드를 지정하는 것이다.
				// RedirectUri에 설정한 SignInCompleted 액션 메소드에서는 샘플 웹 고유의 인증 정보를 
				// 설정하기 위해서 사용된다.
				return Challenge(new AuthenticationProperties
				{
					RedirectUri = Url.Action(nameof(SignInCompleted), new { returnUrl = HttpUtility.UrlEncode(returnUrl) })
				}, OpenIdConnectDefaults.AuthenticationScheme);
			}
		}

		/// <summary>
		/// MSAL SDK가 Entra ID로 부터 받은 인증 정보를 처리한 후에 호출하는 액션 메소드이다.
		/// 이 액션 메소드는 SignIn 액션 메소드에서 MSAL SDK의 Challenge 메소드를 호출할 때 설정된다.
		/// </summary>
		/// <param name="returnUrl">
		/// MSAL SDK의 Challenge 메소드에서 함께 설정되 returnUrl QueryString이며, 이 URL이 모든 인증을 완료한 후에
		/// 최종적으로 사용자에게 제공될 웹 페이지 URL이다.
		/// </param>
		/// <returns>
		/// </returns>
		public IActionResult SignInCompleted(string returnUrl)
		{
			// MSAL로 부터 제공받은 인증정보를 이용해서 ASP.NET Core 인증이 완료되어 SignInCompleted 액션 메소드가
			// 호출되었기 때문에, ASP.NET Core 인증을 먼저 확인한다.
			if(!User.Identity.IsAuthenticated)
			{
				// ASP.NET Core 인증이 되어 있지 않기 때문에 오류 페이지로 이동시킨다.
				throw new Exception("MSAL 인증이 완료되었지만, ASP.NET Core 인증이 되지 않았습니다.");
			}
			else
			{
				// 샘플 앱에 사용자 인증을 처리하는 SignInManager 클래스에 MSAL 인증 정보를 이용해서
				// 샘플 앱 만의 인증을 수행하도록 추가한 Login 메소드를 호출한다.
				if(!signInManager.Login(User, Response))
				{
					throw new Exception("MSAL 인증은 완료되었지만, 인증을 완료한 사용자는 없는 사용자 입니다.");
				}

				if (string.IsNullOrWhiteSpace(returnUrl))
				{
					returnUrl = Url.Action("Index", "Board");
				}

				// 인증 처리 완료 후에 실제로 되돌아갈 URL로 이동한다.
				return Redirect(HttpUtility.UrlDecode(returnUrl));
			}
		}

		public IActionResult ClaimView()
		{
			if(!signInManager.IsAuthenticated)
			{
				return RedirectToAction("Login", "Home", new {returnUrl = Url.Action(nameof(ClaimView))});
			}

			var model = new ClaimViewModel(signInManager.LoginSession);
			model.Claims = new Dictionary<string, string>();
			foreach (var claim in (User.Identity as ClaimsIdentity).Claims)
			{
				model.Claims.Add(claim.Type, claim.Value);
			}

			return View(model);			
		}		
	}
}
