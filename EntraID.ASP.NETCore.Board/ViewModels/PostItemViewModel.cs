using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EntraID.ASP.NETCore.Board.Models;

namespace EntraID.ASP.NETCore.Board.ViewModels
{
	public class PostItemViewModel : BaseViewModel
	{
		public PostItemViewModel(LoginSession loginSession) : base(loginSession) { }
		public PostItem PostItem { get; set; }
		public string WriterDisplayName { get; set; }
	}
}