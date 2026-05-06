using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntraID.ASP.NETCore.Board.Models
{
	public class PostItem
	{
		public string Id { get; set; }
		public string Subject { get; set; }
		public string Content { get; set; }
		public string Writer { get; set; }
		public int ReadCount { get; set; }
		public DateTime WhenWrited { get; set; }

	}
}