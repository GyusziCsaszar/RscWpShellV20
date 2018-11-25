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

using Ressive.Store;

namespace RscIeV10
{
	
	public static class RscBrowserSettings
	{
		
		public static bool MarkedOpenExternal( Uri uri )
		{
			return MarkedOpenExternal( uri.DnsSafeHost );
		}
		
		public static bool MarkedOpenExternal( string sDomain )
		{
			RscStore store = new RscStore();
			
			string sFolder = "A:\\Internet\\UserAgents";
			if( !store.FolderExists( sFolder ) ) return false;
			
			if( !store.FileExists(  sFolder + "\\" + sDomain + "_OpenExternal" + ".txt" ) ) return false;
			
			return true;
		}
		
		public static void MarkOpenExternal( Uri uri, bool bMark )
		{
			MarkOpenExternal( uri.DnsSafeHost, bMark );
		}
		
		public static void MarkOpenExternal( string sDomain, bool bMark )
		{
			RscStore store = new RscStore();
			
			string sFolder = "A:\\Internet\\UserAgents";
			store.CreateFolderPath( sFolder );
			
			if( bMark )
			{
				store.WriteTextFile( sFolder + "\\" + sDomain + "_OpenExternal" + ".txt", "", true );
			}
			else
			{
				if( store.FileExists(  sFolder + "\\" + sDomain + "_OpenExternal" + ".txt" ) )
				{
					store.DeleteFile(  sFolder + "\\" + sDomain + "_OpenExternal" + ".txt" );
				}
			}
		}
		
		public static void StoreUserAgentID( Uri uri, string sUaID )
		{
			StoreUserAgentID( uri.DnsSafeHost, sUaID );
		}
		
		public static string LoadUserAgentID( Uri uri, string sDefaultRetVal )
		{
			return LoadUserAgentID( uri.DnsSafeHost, sDefaultRetVal );
		}
		
		public static void StoreUserAgentID( string sDomain, string sUaID )
		{
			RscStore store = new RscStore();
			
			string sFolder = "A:\\Internet\\UserAgents";
			store.CreateFolderPath( sFolder );
			
			store.WriteTextFile( sFolder + "\\" + sDomain + ".txt", sUaID, true );
		}
		
		public static string LoadUserAgentID( string sDomain, string sDefaultRetVal )
		{
			RscStore store = new RscStore();
			
			string sFolder = "A:\\Internet\\UserAgents";
			if( !store.FolderExists( sFolder ) )
				return sDefaultRetVal;
			
			List<string> asDomainLst = SplitDomain( sDomain );
			
			foreach( string sDn in asDomainLst )
			{
				string sPath;
				
				sPath = sFolder + "\\" + sDn + ".txt";
				if( store.FileExists( sPath ) )
				{
					string sUaID = store.ReadTextFile( sPath, "" );
					if( sUaID.Length > 0 )
						return sUaID;
				}
			}
			
			return sDefaultRetVal;
		}
		
		public static ImageSource FaviconForDomain( string sDomain, out bool bFound, out string sPathOut, ImageSource isDefault, bool bSmall = true )
		{
			bFound = false;
			sPathOut = "";
			
			// //
			//
			
			RscStore store = new RscStore();
			
			string sFolder = "A:\\Internet\\Favicons";
			if( !store.FolderExists( sFolder ) )
				return isDefault;
			
			List<string> asDomainLst = SplitDomain( sDomain );
			
			string strImgPath = "";
			
			foreach( string sDn in asDomainLst )
			{
				//MessageBox.Show( sDn );
				
				string sPath;
				
				if( bSmall )
				{
					sPath = sFolder + "\\" + sDn + ".ico.sm.jpg";
					if( store.FileExists( sPath ) )
					{
						strImgPath = sPath;
						break;
					}
				}
				
				sPath = sFolder + "\\" + sDn + ".ico.jpg";
				if( store.FileExists( sPath ) )
				{
					strImgPath = sPath;
					break;
				}
			}
			
			if( strImgPath.Length == 0 ) return isDefault;
			
			//
			// //
			//
			
			try
			{
				System.IO.Stream stream = store.GetReaderStream( strImgPath );
				
				BitmapImage bmp = new BitmapImage();
				bmp.SetSource(stream);
				stream.Close();
				
				bFound = true;
				sPathOut = strImgPath;
				
				return bmp;
			}
			catch( Exception /*e*/ )
			{
				return isDefault;
			}
			
			//
			// //
		}
		
		public static List<string> SplitDomain( string sDomain )
		{
			List<string> asDomainLst = new List<string>();
			
			string sTmp = sDomain;
			int iPos = sTmp.LastIndexOf('.');
			if( iPos >= 0 )
			{
				for(;;)
				{
					asDomainLst.Add( sTmp );
					
					iPos = sTmp.IndexOf( '.' );
					if( iPos < 0 )
						break;
					
					if( iPos == sTmp.LastIndexOf('.') )
					{
						asDomainLst.Add( sTmp.Substring( 0, iPos ));
						break;
					}
					
					sTmp = sTmp.Substring( iPos + 1 );
				}
			}
			else
			{
				asDomainLst.Add( sDomain );
			}
			
			return asDomainLst;
		}
		
	}
	
}