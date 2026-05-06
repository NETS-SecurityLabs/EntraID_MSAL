using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.Json;
using System.IO;
using System.Configuration;
namespace EntraID.ASP.NETCore.Board.Business
{
	public class UserInfo
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string DisplayName { get; set; }
	}

	public class UserManager
	{
		public static List<UserInfo> Users { get; set; }

		/// <summary>
		/// Startup.Configure에서 호출됨
		/// </summary>
		/// <param name="userInfoFilePath"></param>
		public static void Initialize(string userInfoFilePath)
		{
			if(File.Exists(userInfoFilePath))
			{
				using var reader = new StreamReader(userInfoFilePath);
				var json = reader.ReadToEnd();
				Users = JsonSerializer.Deserialize<List<UserInfo>>(json);
			}
			else
			{
				throw new Exception("사용자 정보 파일이 존재하지 않습니다. appsettings.json 파일을 확인하세요.");
			}
		}

		public static bool Authenticate(string userName, string password) =>
			Users.Any(u => u.UserName == userName && u.Password == password);


		public static UserInfo GetUserInfo(string userName) => Users.FirstOrDefault(u => u.UserName == userName);

		public static UserInfo GetUserInfoByEmail(string email) => Users.FirstOrDefault(u => u.Email == email);
	}
}