using System;

namespace EntraID.ASP.NETCore.Board.Models
{
	public class LoginSession
	{
		public string UserName { get; set; } = string.Empty;
		public bool IsAuthenticated { get; set; } = false;
		public DateTime WhenLogin { get; set; } = DateTime.Now;
		public string DisplayName { get; set; } = string.Empty;
	}
}
