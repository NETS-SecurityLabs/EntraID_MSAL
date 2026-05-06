using EntraID.ASP.NETCore.Board.Models;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Web;

namespace EntraID.ASP.NETCore.Board.Business
{
	public class BoardManager
	{
		private List<PostItem> postItems = null;
		private string boardFilePath = string.Empty;
		public BoardManager(string boardDataFilePath)
		{
			boardFilePath = boardDataFilePath;	
			
			if (File.Exists(boardFilePath))
			{
				using (var reader = new StreamReader(boardFilePath))
				{
					var json = reader.ReadToEnd();
					try
					{
						postItems = JsonSerializer.Deserialize<List<PostItem>>(json);
					}
					catch
					{
						postItems = new List<PostItem>();
					}
				}
			}
			else
			{
				postItems = new List<PostItem>();
			}
		}

		public List<PostItem> PostItems => postItems;

		public void AddPostItem(PostItem postItem)
		{
			postItems.Add(postItem);
			var postItemsJson = JsonSerializer.Serialize<List<PostItem>>(postItems);
			
			using (var fStream = new FileStream(boardFilePath, FileMode.OpenOrCreate, FileAccess.Write))
			{
				var postItemBytes = Encoding.UTF8.GetBytes(postItemsJson);
				fStream.Write(postItemBytes, 0, postItemBytes.Length);
			}
		}

		public void saveAll()
		{
			var postItemsJson = JsonSerializer.Serialize<List<PostItem>>(postItems, new JsonSerializerOptions
			{
				WriteIndented = true
			});
			
			using (var fStream = new FileStream(boardFilePath, FileMode.OpenOrCreate, FileAccess.Write))
			{
				var postItemBytes = Encoding.UTF8.GetBytes(postItemsJson);
				fStream.Write(postItemBytes, 0, postItemBytes.Length);
			}
		}
		public PostItem GetPostItem(string id)
		{
			var item = postItems.FirstOrDefault(x => x.Id == id);
			if (item == null)
			{
				return null;
			}
			else
			{
				item.ReadCount++;
				saveAll();
				return item;
			}
		}
	}
}