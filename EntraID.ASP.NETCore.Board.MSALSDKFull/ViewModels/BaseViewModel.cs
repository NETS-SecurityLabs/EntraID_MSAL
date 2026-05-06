using EntraID.ASP.NETCore.Board.Models;

using System;

namespace EntraID.ASP.NETCore.Board.ViewModels
{
	public class BaseViewModel
	{
		public LoginSession LoginSession { get; private set; }

		public BaseViewModel(LoginSession session)
		{
			LoginSession = session;
		}

		public DateTime ToDateTime(double unixTimestamp)
		{
			DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dt = dt.AddSeconds(unixTimestamp);
			return dt.ToLocalTime();
		}
	}
}
