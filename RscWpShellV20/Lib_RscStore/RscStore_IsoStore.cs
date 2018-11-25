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

using System.IO;
using System.IO.IsolatedStorage;

namespace Ressive.Store
{
	
	public class RscStore_IsoStore : RscStore_Base
	{
		
		public RscStore_IsoStore()
		{
		}
		
		// //
		//
		
		public override bool IsFolderImageOnly( string path )
		{
			return false;
		}
		
		public override string GetOriginalFileNameOfPath( string path )
		{
			int iPos = path.LastIndexOf('\\');
			if( iPos < 0 )
				iPos = 0;
			else
				iPos++;
			int iPos2 = path.LastIndexOf('.');
			if( iPos2 < 0 ) iPos2 = path.Length;
			return path.Substring( iPos, iPos2 - iPos );
		}
		
		//
		// //
		//
		
		public override void DeleteFolder( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			if( !store.DirectoryExists( path ) ) return;
			store.DeleteDirectory( path );
		}
		
		public override void CreateFolder( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			store.CreateDirectory(path);
		}
		
		public override void DeleteFile( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			if( !store.FileExists( path ) ) return;
			store.DeleteFile( path );
		}
		
		public override Stream CreateFile( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			Stream stream = store.CreateFile(path);
			return stream;
		}
		
		public override Stream GetReaderStream( string path, bool bForPreview )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			IsolatedStorageFileStream stream = store.OpenFile(path, FileMode.Open, FileAccess.Read);
			return stream;
		}
		
		public override bool FileExists( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			return store.FileExists( path );
		}
		
		public override bool FolderExists( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			return store.DirectoryExists( path );
		}
		
		public override RscStoreProperties GetFileProperties( string path )
		{
			RscStoreProperties sp = new RscStoreProperties();
			
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			
			sp.CreationTime = store.GetCreationTime( path ).DateTime;
			sp.LastWriteTime = store.GetLastWriteTime( path ).DateTime;
			sp.LastAccessTime = store.GetLastAccessTime( path ).DateTime;
			
			IsolatedStorageFileStream stream = store.OpenFile(path, FileMode.Open, FileAccess.Read);
			sp.Length = stream.Length;
			stream.Close();
			
			return sp;
		}
		
		public override RscStoreProperties GetFolderProperties( string path )
		{
			RscStoreProperties sp = new RscStoreProperties();
			
			
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			
			sp.CreationTime = store.GetCreationTime( path ).DateTime;
			sp.LastWriteTime = store.GetLastWriteTime( path ).DateTime;
			sp.LastAccessTime = store.GetLastAccessTime( path ).DateTime;
			
			return sp;
		}
		
		public override long GetFileLength( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			IsolatedStorageFileStream stream = store.OpenFile(path, FileMode.Open, FileAccess.Read);
			long lLen = stream.Length;
			stream.Close();
			return lLen;
		}
		
		public override DateTimeOffset GetFileCreationTime( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			return store.GetCreationTime( path );
		}
		
		public override DateTimeOffset GetFileLastWriteTime( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			return store.GetLastWriteTime( path );
		}
		
		public override DateTimeOffset GetFileLastAccessTime( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			return store.GetLastAccessTime( path );
		}
		
		public override DateTimeOffset GetFolderCreationTime( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			return store.GetCreationTime( path );
		}
		
		public override DateTimeOffset GetFolderLastWriteTime( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			return store.GetLastWriteTime( path );
		}
		
		public override DateTimeOffset GetFolderLastAccessTime( string path )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			return store.GetLastAccessTime( path );
		}
		
		public override string [] GetFileNames( string path, string wildcard, bool bIncludeHidden, bool bStartWithInfChr )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			
			string [] astr = store.GetFileNames( path + "\\" + wildcard );
			
			if( bStartWithInfChr )
			{
				for( int i = 0; i < astr.Length; i++ )
				{
					//Normal...
					astr[ i ] = "n" + astr[i];
				}
			}
			
			return astr;
		}
		
		public override string [] GetFolderNames( string path, string wildcard, bool bIncludeHidden, bool bStartWithInfChr )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			
			string [] astr = store.GetDirectoryNames( path + "\\" + wildcard );
			
			if( (!bIncludeHidden) || bStartWithInfChr )
			{
				int iCntNormal = 0;
				string [] astrNormal = null;
				
				for( int iCyc = 0; iCyc < 2; iCyc++ )
				{
					for( int i = 0; i < astr.Length; i++ )
					{
						if( path == "" || path == "\\" ) //Iso root...
						{
							string sLo = astr[ i ].ToLower();
							
							if( RscStore_IsoStore.Is_IE_Cache_Folder( sLo ) )
							{
								if( bIncludeHidden && (iCyc == 0) && bStartWithInfChr ) astr[ i ] = "w" + astr[i];
							}
							else
							{
								switch( sLo )
								{
									
									case "shared" : //WP7
									{
										if( bIncludeHidden && (iCyc == 0) && bStartWithInfChr ) astr[ i ] = "w" + astr[i];
										break;
									}
									
									case "temp" : //Ressive.Hu
									{
										if( bIncludeHidden && (iCyc == 0) && bStartWithInfChr ) astr[ i ] = "r" + astr[i];
										break;
									}
									
									default :
									{
										//Normal...
										if( iCyc > 0 ) astrNormal[ iCntNormal ] = astr[ i ];
										iCntNormal++;
										if( (iCyc == 0) && bStartWithInfChr ) astr[ i ] = "n" + astr[i];
										break;
									}
								}
							}
						}
						else
						{
							//Normal...
							if( iCyc > 0 ) astrNormal[ iCntNormal ] = astr[ i ];
							iCntNormal++;
							if( (iCyc == 0) && bStartWithInfChr ) astr[ i ] = "n" + astr[i];
						}
					}
					
					if( iCyc == 0 )
					{
						if( bIncludeHidden ) break; //Include all files...
						
						if( iCntNormal == astr.Length ) break; //No hidden files...
						
						astrNormal = new String[ iCntNormal ];
						iCntNormal = 0;
					}
					else
					{
						astr = astrNormal;
						break;
					}
				}
			}
			
			return astr;
		}
		
		//
		// //
		
		// //
		// Extras {
		
		public static string Get_IE_Cache_FolderList( char cSep = ';' )
		{
			string sRes = "";
			
			if( sRes.Length > 0 ) sRes += cSep.ToString();
			sRes += "IECompatCache"; //WP7
			
			if( sRes.Length > 0 ) sRes += cSep.ToString();
			sRes += "IECompatUaCache"; //WP7
				
			if( sRes.Length > 0 ) sRes += cSep.ToString();
			sRes += "AppCache"; //WP8
				
			if( sRes.Length > 0 ) sRes += cSep.ToString();
			sRes += "Microsoft"; //Win10Mo
			
			if( sRes.Length > 0 ) sRes += cSep.ToString();
			sRes += "PlatformData"; //Win10Mo
			
			return sRes;
		}
		
		public static bool Is_IE_Cache_Folder( string sFolderName )
		{
			string sList = Get_IE_Cache_FolderList( ';' ) + ";";
			sList = sList.ToLower();
			
			int iHit = sList.IndexOf( sFolderName.ToLower() + ";" );
			if( iHit < 0 )
				return false;
			
			/*
			switch( sFolderName.ToLower() )
			{
				
				case "iecompatcache" : //WP7
				case "iecompatuacache" : //WP7
				
				case "appcache" : //WP8
				
				case "microsoft" : //Win10Mo
				case "platformdata" : //Win10Mo
					
					return true;
			}
			*/
			
			return true;
		}
		
		public static long AvailableFreeSpace()
		{
			IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
			return iso.AvailableFreeSpace;
		}
		
		public static void AddSysEvent( string sText, bool bErr = false, string sCustTimeStampPart = "" )
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
			
			string sPath = "System";
			if( !store.DirectoryExists( sPath ) ) store.CreateDirectory( sPath );
			sPath += "\\Events";
			if( !store.DirectoryExists( sPath ) ) store.CreateDirectory( sPath );
			
			sPath += "\\";
			if( bErr )
				sPath += "ERR_";
			else
				sPath += "INF_";
			
			if( sCustTimeStampPart.Length == 0 )
			{
				DateTime dNow = DateTime.Now;
				sPath += dNow.Year.ToString() + "-";
				if( dNow.Month < 10 ) sPath += "0";
				sPath += dNow.Month.ToString() + "-";
				if( dNow.Day < 10 ) sPath += "0";
				sPath += dNow.Day.ToString();
				sPath += "_";
				if( dNow.Hour < 10 ) sPath += "0";
				sPath += dNow.Hour.ToString() + "-";
				if( dNow.Minute < 10 ) sPath += "0";
				sPath += dNow.Minute.ToString() + "-";
				if( dNow.Second < 10 ) sPath += "0";
				sPath += dNow.Second.ToString();
			}
			else
			{
				sPath += sCustTimeStampPart;
			}
			
			string sExt;
			if( bErr )
				sExt = ".error";
			else
				sExt = ".info";
			
			string sPathWrk = sPath;
			if( sCustTimeStampPart.Length == 0 )
			{
				int i = 0;
				for(;;)
				{
					sPathWrk = sPath + "_" + i.ToString();
					if( !store.FileExists(sPathWrk + sExt) ) break;
					i++;
				}
			}
			sPath = sPathWrk + sExt;			
			IsolatedStorageFileStream stream = store.CreateFile(sPath);
			StreamWriter sw = new StreamWriter( stream );
			sw.Write( sText );
			sw.Close();
			stream.Close();
		}
		
		// } Extras
		// //

	}
	
}