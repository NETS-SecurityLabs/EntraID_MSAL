using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EntraID.ASP.NETCore.Board.Models;

namespace EntraID.ASP.NETCore.Board.ViewModels
{
	public class LoginViewModel : BaseViewModel
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string ReturnUrl { get; set; }
		public bool InvalidCredential { get; set; }
		public LoginViewModel(LoginSession session) : base(session)
		{
		}
	}
}