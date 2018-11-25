using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

//Win10Mo DENIES!!!
/*
using System.Runtime.InteropServices;
*/

namespace RscIeV10
{
	
	public static class RscUserAgents
	{
			
		//Win10Mo DENIES!!!
		/*
		//SRC: http://www.lukepaynesoftware.com/articles/programming-tutorials/changing-the-user-agent-in-a-web-browser-control/
		[DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
		private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);
		const int URLMON_OPTION_USERAGENT = 0x10000001;
		public void ChangeUserAgent(string Agent)
		{
			UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, Agent, Agent.Length, 0);
		}
		*/
		
		const string csUserAgent_Op98  = "Opera/9.80 (Windows Phone; Opera Mini/9.0.0/37.7206; " /*+ "U; hu; "*/ + "Presto/2.12.423 Version/12.16)";
		const string csUserAgent_Wp75  = "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0)"; //; NOKIA; Lumia 620)";
		const string csUserAgent_Wp80  = "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0)"; //; ARM; Touch)"; //; NOKIA; Lumia 620)";
		const string csUserAgent_Wp100 = "Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1" /*+ "; NOKIA; Lumia 620"*/ + ") AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/547.36 Edge/13.1";
		const string csUserAgent_iOS4  = "Mozilla/5.0 (iPhone; " /*+ "U; "*/ + "CPU iPhone OS 4_1 like Mac OS X" /*+ "; en-us"*/ + ") AppleWebKit/532.9 (KHTML, like Gecko) Version/4.0.5 Mobile/8B117 Safari/6531.22.7";
		const string csUserAgent_iOS9  = "Mozilla/5.0 (iPhone; CPU iPhone OS 9_0 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13A342 Safari/601.1";
		const string csUserAgent_Sym94 = "Mozilla/5.0 (SymbianOS/9.4; Series60/5.0" /*+ " NokiaC5-03/22.0.007"*/ + "; Profile/MIDP-2.1 Configuration/CLDC-1.1) AppleWebKit/533.4 (KHTML, like Gecko) NokiaBrowser/7.3.1.33 Mobile Safari/533.4 3gpp-gba";
		
		public static int Count { get{ return 8; } }
		
		public static string Title( int iIndex )
		{ 
			switch( iIndex )
			{
				default : return "(default)";
				case 1 : return "Opera 9.8";
				case 2 : return "Windows Phone 7.5";
				case 3 : return "Windows Phone 8.0";
				case 4 : return "Windows Phone 10.0";
				case 5 : return "Apple iOS 4";
				case 6 : return "Apple iOS 9";
				case 7 : return "Symbian 9.4";
			}
		}
		
		public static string ID( int iIndex )
		{
			//ATT: Do not change, this is stored, remembered...
			switch( iIndex )
			{
				default : return "Default";
				case 1 : return "Op98";
				case 2 : return "WP75";
				case 3 : return "WP80";
				case 4 : return "WP100";
				case 5 : return "iOS4";
				case 6 : return "iOS9";
				case 7 : return "Sym94";
			}
		}
		
		public static string UserAgentFromID( string sID, bool bDecorated )
		{ 
			switch( sID )
			{
				case "Op98" : return csUserAgent_Op98;
				case "WP75" : return csUserAgent_Wp75;
				case "WP80" : return csUserAgent_Wp80;
				case "WP100" : return csUserAgent_Wp100;
				case "iOS4" : return csUserAgent_iOS4;
				case "iOS9" : return csUserAgent_iOS9;
				case "Sym94" : return csUserAgent_Sym94;
			}
			
			if( bDecorated )
				return "<empty>";
			else
				return "";
		}

		public static string DecorateUserAgentID( string sID, bool bCrLf )
		{
			string sRes = "";
			
			switch( sID )
			{
				case "Op98" : sRes += "Opera"; break;
				case "WP75" : sRes += "Windows"; break;
				case "WP80" : sRes += "Windows"; break;
				case "WP100" : sRes += "Windows"; break;
				case "iOS4" : sRes += "iOS"; break;
				case "iOS9" : sRes += "iOS"; break;
				case "Sym94" : sRes += "Symbian"; break;
				default : return "(default)";
			}
			
			if( bCrLf )
				sRes += "\n";
			else
				sRes += " ";
			
			switch( sID )
			{
				case "Op98" : sRes += "9.8"; break;
				case "WP75" : sRes += "Phone 7.5"; break;
				case "WP80" : sRes += "Phone 8.0"; break;
				case "WP100" : sRes += "Phone 10.0"; break;
				case "iOS4" : sRes += "4.1"; break;
				case "iOS9" : sRes += "9.0"; break;
				case "Sym94" : sRes += "9.4"; break;
				default : return "";
			}
			
			return sRes;
		}
		
		public static string UserAgent( int iIndex, bool bDecorated )
		{ 
			switch( iIndex )
			{
				case 1 : return csUserAgent_Op98;
				case 2 : return csUserAgent_Wp75;
				case 3 : return csUserAgent_Wp80;
				case 4 : return csUserAgent_Wp100;
				case 5 : return csUserAgent_iOS4;
				case 6 : return csUserAgent_iOS9;
				case 7 : return csUserAgent_Sym94;
			}
			
			if( bDecorated )
				return "<empty>";
			else
				return "";
		}
		
	}
	
}