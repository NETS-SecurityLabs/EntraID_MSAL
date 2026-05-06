using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EntraID.ASP.NETCore.Board.Models;

namespace EntraID.ASP.NETCore.Board.ViewModels
{
	public class BoardListViewModel : BaseViewModel
	{
		public List<PostItem> PostItems { get; set; }
		public BoardListViewModel(LoginSession session) : base(session)
		{
		}
	}
}