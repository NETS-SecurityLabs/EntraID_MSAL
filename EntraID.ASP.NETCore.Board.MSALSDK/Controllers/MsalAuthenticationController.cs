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

		internal static Task OnAuthenticationFailed(AuthenticationFailedContext context)
		{
			// 오류가 발생했기 때문에, 미들웨어 체인 실행을 중단한다.
			context.HandleResponse();

			// 오류 페이지로 이동하기 위해서 URL을 구성하며, QueryString으로 오류 식별자와 오류 설명을 함께 전송한다.
			var errorUrl = string.Format("{0}://{1}{2}/MsalAuthentication/Error",
				context.Request.Scheme, context.Request.Host.Value, context.Request.PathBase);
			var errorViewUrl = string.Format("{0}?error={1}&errorDesc={2}",
				errorUrl, context.ProtocolMessage.Error, context.ProtocolMessage.ErrorDescription);
			context.Response.Redirect(errorViewUrl);
			return Task.CompletedTask;
		}

		public IActionResult SignIn(string? returnUrl)
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

		public new IActionResult SignOut()
		{
			// 샘플 앱의 인증 세션 정보를 삭제한다.
			signInManager.Logout(Response);

			// MSAL 인증 정보를 삭제하고, Entra ID로 로그아웃 요청을 보내기 위해서 SignOut을 호출한다.
			return SignOut(
				new AuthenticationProperties
				{
					// 모든 로그아웃 처리 과정이 완료된 후에, 최종적으로 되돌아 갈 URL을 설정한다.
					// 샘플 앱은 게시판 목록 페이지로 이동하도록 설정.
					RedirectUri = Url.Action("Index", "Board")
				},
				OpenIdConnectDefaults.AuthenticationScheme,
				CookieAuthenticationDefaults.AuthenticationScheme);
		}

		/// <summary>
		/// 이 액션 메소드는 하기의 두가지 경우에 호출된다.
		/// 1. 샘플 앱에서 로그아웃을 요청했을때.
		/// 2. 샘플 앱이 Entra ID 인증을 받은 후에, 다른 Entra ID 인증 대상 앱에서 인증을 받고, 
		///		샘플 앱이 아닌 다른 대상 앱에서 로그아웃을 요청했을 때.
		///	두번째 경우에는 SignOut에서 RedirectUri를 설정한 Url로 최종적으로 돌아오지 않는다. 이 경우에는 
		///	로그아웃을 처음으로 요청한 대상 앱이 지정한 URL로 이동한다.
		/// </summary>
		/// <returns></returns>
		public IActionResult SignOutReceived()
		{
			if (signInManager.IsAuthenticated)
			{
				// 샘플 앱 자체 인증 정보가 존재한다면, 로그아웃을 수행한다.
				signInManager.Logout(Response);
			}

			if (User.Identity.IsAuthenticated)
			{
				// MSAL 인증이 존재한다면, 로그아웃을 수행한다.				
				return SignOut(OpenIdConnectDefaults.AuthenticationScheme,
					CookieAuthenticationDefaults.AuthenticationScheme);
			}
			else
			{
				return new OkResult();
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
		public IActionResult SignInCompleted(string? returnUrl)
		{
			// MSAL로 부터 제공받은 인증정보를 이용해서 ASP.NET Core 인증이 완료되어 SignInCompleted 액션 메소드가
			// 호출되었기 때문에, ASP.NET Core 인증을 먼저 확인한다.
			if(!User.Identity.IsAuthenticated)
			{
				// ASP.NET Core 인증이 되어 있지 않기 때문에 오류 페이지로 이동시킨다.
				MsalErrorViewModel model = new MsalErrorViewModel(signInManager.LoginSession);
				model.Error = "no_authentication";
				model.ErrorDescription = "MSAL 인증을 완료하였지만, 인정 정보가 없습니다.";
				return View(nameof(Error), model);
			}
			else
			{
				// 샘플 앱에 사용자 인증을 처리하는 SignInManager 클래스에 MSAL 인증 정보를 이용해서
				// 샘플 앱 만의 인증을 수행하도록 추가한 Login 메소드를 호출한다.
				if(!signInManager.Login(User, Response))
				{
					MsalErrorViewModel model = new MsalErrorViewModel(signInManager.LoginSession);
					model.Error = "no_such_user";
					model.ErrorDescription = "MSAL 인증을 완료한 사용자는 없는 사용자 입니다.";
					return View(nameof(Error), model);
				}

				if (string.IsNullOrWhiteSpace(returnUrl))
				{
					returnUrl = Url.Action("Index", "Board");
				}

				// 인증 처리 완료 후에 실제로 되돌아갈 URL로 이동한다.
				return Redirect(HttpUtility.UrlDecode(returnUrl));
			}
		}

		public IActionResult Error(string error, string errorDesc)
		{
			MsalErrorViewModel model = new MsalErrorViewModel(signInManager.LoginSession);
			model.Error = error;
			model.ErrorDescription = errorDesc;
			return View(model);
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
				if (claim.ValueType.Equals("http://www.w3.org/2001/XMLSchema#integer"))
				{
					model.Claims.Add(claim.Type, model.ToDateTime(Convert.ToDouble(claim.Value)).ToString());
				}
				else
				{
					model.Claims.Add(claim.Type, claim.Value);
				}
			}

			return View(model);			
		}		
	}
}
