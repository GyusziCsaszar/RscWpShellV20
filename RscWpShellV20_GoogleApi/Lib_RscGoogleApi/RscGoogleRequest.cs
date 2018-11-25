
namespace Ressive.GoogleApi
{
	
	public enum GoogleRequest
	{
		None = 0,
		
		UserInfo = 1,
		
		GMail_Messages = 2,
		GMail_Message_Details = 3,
		
		GMail_Labels = 4,
		
		GMail_Threads = 5,
		
		GMail_History = 6,
		
		GMail_Drafts = 7
		
	}
	
	public static class GoogleUtils
	{
		
		public static string GoogleRequestTitle( GoogleRequest gr )
		{
			switch( gr )
			{
				case GoogleRequest.None : return "<none>";
				case GoogleRequest.UserInfo : return "User Info";
				case GoogleRequest.GMail_Messages : return "GMail - Messages";
				case GoogleRequest.GMail_Message_Details : return "GMail - Message Details";
				case GoogleRequest.GMail_Labels : return "GMail - Labels";
				case GoogleRequest.GMail_Threads : return "GMail - Threads";
				case GoogleRequest.GMail_History : return "GMail - History";
				case GoogleRequest.GMail_Drafts : return "GMail - Drafts";
			}
			return "<unknown>";
		}
		
		public static string GoogleRequestBaseUrl( GoogleRequest gr, out string sUriResource, string UserId, string id )
		{
			sUriResource = "";
			switch( gr )
			{
				
				case GoogleRequest.UserInfo :
					sUriResource = "/oauth2/v1/userinfo";
					return "https://www.googleapis.com";
					
				case GoogleRequest.GMail_Messages :
					if( UserId.Length == 0 ) return "";
					sUriResource = "/" + UserId + "/messages";
					return "https://www.googleapis.com/gmail/v1/users";
					
				case GoogleRequest.GMail_Message_Details :
					if( UserId.Length == 0 ) return "";
					if( id.Length == 0 ) return "";
					sUriResource = "/" + UserId + "/messages/" + id;
					return "https://www.googleapis.com/gmail/v1/users";
					
				case GoogleRequest.GMail_Labels :
					if( UserId.Length == 0 ) return "";
					sUriResource = "/" + UserId + "/labels";
					return "https://www.googleapis.com/gmail/v1/users";
					
				case GoogleRequest.GMail_Threads :
					if( UserId.Length == 0 ) return "";
					sUriResource = "/" + UserId + "/threads";
					return "https://www.googleapis.com/gmail/v1/users";
					
				case GoogleRequest.GMail_History :
					if( UserId.Length == 0 ) return "";
					sUriResource = "/" + UserId + "/history";
					return "https://www.googleapis.com/gmail/v1/users";
					
				case GoogleRequest.GMail_Drafts :
					if( UserId.Length == 0 ) return "";
					sUriResource = "/" + UserId + "/drafts";
					return "https://www.googleapis.com/gmail/v1/users";
					
			}
			return "";
		}
		
		public static GoogleRequest GoogleRequestFromUrl( string sUrl )
		{
			
			//ATT!!! ASSUMED: UserID = me
			
			switch( sUrl )
			{
				case "https://www.googleapis.com/oauth2/v1/userinfo"			: return GoogleRequest.UserInfo;
				case "https://www.googleapis.com/gmail/v1/users/me/messages"	: return GoogleRequest.GMail_Messages;
				case "https://www.googleapis.com/gmail/v1/users/me/labels"		: return GoogleRequest.GMail_Labels;
				case "https://www.googleapis.com/gmail/v1/users/me/threads"		: return GoogleRequest.GMail_Threads;
				case "https://www.googleapis.com/gmail/v1/users/me/history"		: return GoogleRequest.GMail_History;
				case "https://www.googleapis.com/gmail/v1/users/me/drafts"		: return GoogleRequest.GMail_Drafts;
			}
			
			if( sUrl.IndexOf( "https://www.googleapis.com/gmail/v1/users/me/messages/" ) == 0 )
				return GoogleRequest.GMail_Message_Details;
			
			return GoogleRequest.None;
		}
		
	}
	
}