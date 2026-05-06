using EntraID.ASP.NETCore.Board.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EntraID.ASP.NETCore.Board.Controllers
{
	public class BoardBaseController : Controller
	{
		protected SignInManager signInManager;

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			signInManager = new SignInManager();
			signInManager.CheckSignIn(Request);
		}
	}
}
