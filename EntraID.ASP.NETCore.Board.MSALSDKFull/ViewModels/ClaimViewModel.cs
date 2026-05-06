using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using EntraID.ASP.NETCore.Board.Models;

namespace EntraID.ASP.NETCore.Board.ViewModels
{
	public class ClaimViewModel : BaseViewModel
	{
		public ClaimViewModel(LoginSession loginSession)
			: base(loginSession)
		{

		}

		public Dictionary<string, string> Claims { get; set; }
	}
}