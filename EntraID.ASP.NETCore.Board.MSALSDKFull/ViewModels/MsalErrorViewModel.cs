using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EntraID.ASP.NETCore.Board.Models;
using EntraID.ASP.NETCore.Board.ViewModels;

namespace EntraID.ASP.NETCore.Board.ViewModels
{
	public class MsalErrorViewModel : BaseViewModel
	{
		public MsalErrorViewModel(LoginSession session) : base(session)
		{
		}

		public string ErrorDescription { get; set; }
		public string Error { get; set; }
	}
}