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
			// 샘플 앱의 모든 Controller의 액션 메소드가 호출되기 전에 실행되는 이벤트 핸들러로
			// 샘플 앱의 사용자 인증 관리 객체를 생성한 후에, 인증 상태를 검사한다.
			signInManager = new SignInManager();
			signInManager.CheckSignIn(Request);
		}
	}
}
