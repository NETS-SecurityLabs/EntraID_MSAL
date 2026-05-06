using EntraID.ASP.NETCore.Board.Models;

namespace EntraID.ASP.NETCore.Board.ViewModels
{
	public class BaseViewModel
	{
		public LoginSession LoginSession { get; private set; }

		public BaseViewModel(LoginSession session)
		{
			LoginSession = session;
		}
	}
}
