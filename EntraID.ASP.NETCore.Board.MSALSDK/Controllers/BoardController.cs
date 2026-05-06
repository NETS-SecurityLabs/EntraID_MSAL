using EntraID.ASP.NETCore.Board.Business;
using EntraID.ASP.NETCore.Board.Models;
using EntraID.ASP.NETCore.Board.ViewModels;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

using System;

namespace EntraID.ASP.NETCore.Board.Controllers
{
	public class BoardController : BoardBaseController
	{
		private BoardManager boardManager;
		public BoardController(BoardManager boardManager)
		{
			this.boardManager = boardManager;
		}

		// 게시글 목록 조회 액션 메소드는 사용자 인증을 요구하지 않기 때문에,
		// 사용자의 인증 상태를 체크하지 않는다.
		public IActionResult Index()
		{
			BoardListViewModel model = new BoardListViewModel(signInManager.LoginSession);
			model.PostItems = boardManager.PostItems;
			return View(model);
		}

		[HttpGet]
		public IActionResult Write()
		{
			if(!signInManager.IsAuthenticated)
			{
				// 로그인 상태가 아니기 때문에, 로그인 페이지로 이동시킨다.
				return RedirectToAction("Login", "Home", new { returnUrl = Request.GetEncodedUrl() });

			}

			PostItemViewModel model = new PostItemViewModel(signInManager.LoginSession);
			return View(model);			
		}

		[HttpPost]
		public IActionResult Write(string subject, string content)
		{
			if (!signInManager.IsAuthenticated)
			{
				// 로그인 상태가 아니기 때문에, 로그인 페이지로 이동시킨다.
				return RedirectToAction("Login", "Home", new {returnUrl = Request.GetEncodedUrl()});

			}

			boardManager.AddPostItem(new PostItem
			{
				Subject = subject,
				Content = content,
				Id = Guid.NewGuid().ToString(),
				ReadCount = 0,
				WhenWrited = DateTime.Now,
				Writer = signInManager.LoginSession.UserName
			});
			return RedirectToAction(nameof(Index));
		}

		// 게시글 조회 페이지는 인증을 요구하지 않기 때문에, 사용자의 인증 상태를 검사하지 않는다.
		public IActionResult Read(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
			{
				return RedirectToAction(nameof(Index));
			}

			var item = boardManager.GetPostItem(id);
			PostItemViewModel model = new PostItemViewModel(signInManager.LoginSession);
			model.PostItem = item;
			model.WriterDisplayName = UserManager.GetUserInfo(item.Writer).DisplayName;
			return View(model);
		}
	}
}
