using System;
using System.Web;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using EntraID.ASP.NETCore.Board.ViewModels;

namespace EntraID.ASP.NETCore.Board.Controllers
{
	public class MsalAuthenticationController : BoardBaseController
	{
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
