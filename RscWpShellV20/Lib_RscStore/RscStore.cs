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

using System.Runtime.Serialization;

namespace Ressive.Store
{
	
	public class RscStore
	{
		// //
		//
		
		public static string [] GetRoots( bool bIncludeHidden, bool bStartWithInfChr )
		{
			//return new String [] {"A:", "B:", "C:", "D:"};
			
			RscStore_Storage store = new RscStore_Storage();
			
			string [] astrDrvs = new String [26];
			object oFldr;
			
			int iCnt = 0;
			for( char cDrv = 'C'; cDrv <= 'Z'; cDrv++ )
			{
				oFldr = store.GetRootFolderByNameLower( (cDrv.ToString() + ":").ToLower() );
				if( oFldr != null )
				{
					//Drive exits...
					astrDrvs[ iCnt ] = cDrv.ToString() + ":"; iCnt++;
				}
			}
			
			string [] astrRes = new String [iCnt + 2];
			if( bStartWithInfChr )
			{
				astrRes[ 0 ] = "nA:"; //Normal..
				astrRes[ 1 ] = "nB:"; //Normal..
			}
			else
			{
				astrRes[ 0 ] = "A:";
				astrRes[ 1 ] = "B:";
			}
			for( int i = 0; i < iCnt; i++ )
			{
				if( bStartWithInfChr )
					astrRes[ 2 + i ] = "n"; //Normal...
				else
					astrRes[ 2 + i ] = "";
				
				astrRes[ 2 + i ] += astrDrvs[ i ];
			}
			return astrRes;
		}
		
		public static bool IsIsoStorePath( string path )
		{
			if( path.Length < 2 ) return false;
			if( path[1] != ':' ) return false;
			switch( path[ 0 ] )
			{
				case 'A' :
				case 'a' :
					return true;
			}
			return false;
		}
		
		public static string GetRootDescription( string sRoot )
		{
			if( sRoot.Length != 2 ) return "";
			if( sRoot[ 1 ] != ':' ) return "";
			switch( sRoot[ 0 ] )
			{
				case 'A' :
				case 'a' :
					return "Isolated Storage";
				case 'B' :
				case 'b' :
					return "Known Folders";
				default :
					return "Storage";
			}
			//return "";
		}
		
		public bool HasWriteAccess( string path )
		{
			//
			// V3
			//
			if( IsIsoStorePath( path ) )
				return true;
			
			return false;
			
			//
			// V2
			//
			/*
			try
			{
				if( path[ path.Length - 1 ] != '\\' )
					path += '\\';
				
				path += "_write_test_";
				
				if( !WriteTextFile( path, "", true ) )
					return false;
				
				DeleteFile( path );
			}
			catch( Exception e )
			{
				MessageBox.Show( e.Message );
				return false;
			}
			return true;
			*/
			
			//
			// V1
			//
			/*
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.HasWriteAccess( GetStorePath( path ) );
			*/
		}
		
		//
		// //
		//
		
		public RscStore()
		{
		}
		
		protected RscStore_Base GetStore( string sPath )
		{
			if( sPath.Length < 2 ) return null;
			if( sPath[ 1 ] != ':' ) return null;
			
			RscStore_Base store = null;
			
			switch( sPath[ 0 ] )
			{
				case 'A' :
				case 'a' :
					store = new RscStore_IsoStore();
					break;
				case 'B' :
				case 'b' :
					store = new RscStore_KnownFolders();
					break;
				default :
					store = new RscStore_Storage();
					break;
			}
			
			store = store.GetSpecialStorage( GetStorePath( sPath ) );
			
			return store;
		}
		
		protected string GetStorePath( string sPath )
		{
			if( sPath.Length < 2 ) return sPath;
			if( sPath[ 1 ] != ':' ) return sPath;
			
			switch( sPath[ 0 ] )
			{
				case 'A' :
				case 'a' :
					return sPath.Substring( 2 );
				case 'B' :
				case 'b' :
					return sPath.Substring( 2 );
				default :
					return sPath;
			}
			
			//return sPath;
		}
		
		//
		// //
		//
		
		public bool IsFolderImageOnly( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.IsFolderImageOnly( GetStorePath( path ) );
		}
		
		public string GetOriginalFileNameOfPath( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetOriginalFileNameOfPath( GetStorePath( path ) );
		}
		
		//
		// //
		//
		
		public void DeleteFolder( string path )
		{			
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			string sStorePath = GetStorePath( path );
			
			if( store.FolderExists( sStorePath ) ) //Do not delete this... ...backward compatibility.
			{
				store.DeleteFolder( sStorePath );
			}
		}
		
		public void CreateFolder( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			string sStorePath = GetStorePath( path );
			
			if( !store.FolderExists( sStorePath ) ) //Do not delete this... ...backward compatibility.
			{
				store.CreateFolder( sStorePath );
			}
		}
		
		public void DeleteFile( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			store.DeleteFile( GetStorePath( path ) );
		}
		
		public System.IO.Stream CreateFile( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.CreateFile( GetStorePath( path ) );
		}
		
		public System.IO.Stream GetReaderStream( string path, bool bForPreview = false )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetReaderStream( GetStorePath( path ), bForPreview );
		}
		
		public bool FileExists( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.FileExists( GetStorePath( path ) );
		}
		
		public bool FolderExists( string path )
		{
			//Can happen...
			if( path.Trim().Length == 0 ) return true; //Holder of drives exists and walkable...
			
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.FolderExists( GetStorePath( path ) );
		}
		
		public RscStoreProperties GetFileProperties( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetFileProperties( GetStorePath( path ) );
		}
		
		public RscStoreProperties GetFolderProperties( string path )
		{
			//Can happen...
			path = path.Trim();
			
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );		
			
			if( path.Length == 2 )
			{
				if( path[ 1 ] == ':' ) return new RscStoreProperties();
			}
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetFolderProperties( GetStorePath( path ) );
		}
		
		public long GetFileLength( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) return 0;
			
			return store.GetFileLength( GetStorePath( path ) );
		}
		
		public DateTimeOffset GetFileCreationTime( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetFileCreationTime( GetStorePath( path ) );
		}
		
		public DateTimeOffset GetFileLastWriteTime( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetFileLastWriteTime( GetStorePath( path ) );
		}
		
		public DateTimeOffset GetFileLastAccessTime( string path )
		{
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetFileLastAccessTime( GetStorePath( path ) );
		}
		
		public DateTimeOffset GetFolderCreationTime( string path )
		{
			//Can happen...
			path = path.Trim();
			
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			if( path.Length == 2 )
			{
				if( path[ 1 ] == ':' ) return new DateTimeOffset();
			}
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetFolderCreationTime( GetStorePath( path ) );
		}
		
		public DateTimeOffset GetFolderLastWriteTime( string path )
		{
			//Can happen...
			path = path.Trim();
			
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			if( path.Length == 2 )
			{
				if( path[ 1 ] == ':' ) return new DateTimeOffset();
			}
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetFolderLastWriteTime( GetStorePath( path ) );
		}
		
		public DateTimeOffset GetFolderLastAccessTime( string path )
		{
			//Can happen...
			path = path.Trim();
			
			if( path[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + path );
			
			if( path.Length == 2 )
			{
				if( path[ 1 ] == ':' ) return new DateTimeOffset();
			}
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetFolderLastAccessTime( GetStorePath( path ) );
		}
		
		public string [] GetFileNames( string path, string wildcard, bool bIncludeHidden = true, bool bStartWithInfChr = false )
		{
			//Can happen...
			path = path.Trim();
			
			//MUST NOT!!! Called to list drives...
			//if( searchPattern[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + searchPattern );
			
			if( path.Length == 0 )
			{
				return new String [] {};
			}
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetFileNames( GetStorePath( path ), wildcard, bIncludeHidden, bStartWithInfChr );
		}
		
		public string [] GetFolderNames( string path, string wildcard, bool bIncludeHidden = true, bool bStartWithInfChr = false )
		{
			//Can happen...
			path = path.Trim();
			
			//MUST NOT!!! Called to list drives...
			//if( searchPattern[ 1 ] != ':' ) throw new Exception( "Not absolute path: " + searchPattern );
			
			if( path.Length == 0 )
			{
				return GetRoots( bIncludeHidden, bStartWithInfChr );
			}
			
			RscStore_Base store = GetStore( path );
			if( store == null ) throw new Exception( "Unknown store: " + path );
			
			return store.GetFolderNames( GetStorePath( path ), wildcard, bIncludeHidden, bStartWithInfChr );
		}
		
		//
		// //
		//
		
		public static string ExtensionOfPath( string sPath )
		{
			int iPos = sPath.LastIndexOf('.');
			if( iPos >= 0 )
				return sPath.Substring( iPos );
			return "";
		}
		
		public static string FileNameOfPath( string sPath )
		{
			int iPos = sPath.LastIndexOf('\\');
			if( iPos < 0 )
				iPos = 0;
			else
				iPos++;
			int iPos2 = sPath.LastIndexOf('.');
			if( iPos2 < 0 ) iPos2 = sPath.Length;
			return sPath.Substring( iPos, iPos2 - iPos );
		}
		
		public static string FileOfPath( string sPath )
		{
			int iPos = sPath.LastIndexOf('\\');
			if( iPos >= 0 )
				return sPath.Substring( iPos + 1 );
			return sPath;
		}
		
		public static string PathOfPath( string sPath )
		{
			int iPos = sPath.LastIndexOf('\\');
			if( iPos >= 0 )
				return sPath.Substring( 0, iPos );
			return sPath;
		}
		
		//
		// //
		//
		
		public void MoveFileForce( string sPath, string sPathDest )
		{
			CopyFile( sPath, sPathDest, true );
			DeleteFile( sPath );
		}
		
		public void CopyFileForce( string sPath, string sPathDest )
		{
			CopyFile( sPath, sPathDest, true );
		}
		
		public void MoveFile( string pathSrc, string pathTrg )
		{
			CopyFile( pathSrc, pathTrg, false );
			DeleteFile( pathSrc );
		}
		
		public void CopyFile( string pathSrc, string pathTrg, bool bOverWrite = false )
		{
			
			System.IO.Stream streamSrc = GetReaderStream( pathSrc, false );
			
			if( FileExists( pathTrg ) )
			{
				if( bOverWrite )
					DeleteFile( pathTrg );
				else
					throw new Exception( "File already exits: " + pathTrg );
			}
			
			System.IO.Stream streamTrg = CreateFile( pathTrg );
			
            // Initialize the buffer for 4KB disk pages.
            byte[] readBuffer = new byte[4096];
            int bytesRead = -1;
            while ((bytesRead = streamSrc.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                streamTrg.Write(readBuffer, 0, bytesRead);
            }
			streamTrg.Close();
			
			streamSrc.Close();
		}
		
		public string ReadTextFile( string sPath, string sDefaultContent )
		{
			bool bNotExists;
			return ReadTextFile( sPath, sDefaultContent, out bNotExists );
		}
		public string ReadTextFile( string sPath, string sDefaultContent, out bool bNotExist, int iCch = -1 )
		{
			bNotExist = false;
			
			if( !FileExists(sPath) )
			{
				bNotExist = true;
				return sDefaultContent;
			}
			
			System.IO.Stream stream = GetReaderStream( sPath, false );
			System.IO.StreamReader sr = new System.IO.StreamReader( stream );
			
			string strText = "";
			
			if( iCch <= 0 )
			{
				strText = sr.ReadToEnd();
			}
			else
			{
				char [] ac = new char [iCch];
				int iLen = sr.Read(ac, 0, iCch );
				if( iLen > 0 ) strText = new String(ac, 0, iLen );
			}
			
			sr.Close();
			stream.Close();
			
			return strText;
		}
		
		public bool WriteTextFile( string sPath, string sText, bool bOverWrite )
		{
			if( FileExists(sPath) )
			{
				if( !bOverWrite ) return false;
				DeleteFile(sPath);
			}
			
			System.IO.Stream stream = CreateFile(sPath);
			
			System.IO.StreamWriter sw = new System.IO.StreamWriter( stream );
			sw.Write( sText );
			sw.Close();
			
			stream.Close();
			
			return true;
		}
		
		public T ReadXmlDataFile<T>( string sPath, T oDefaultContent )
		{
			bool bNotExists;
			return ReadXmlDataFile( sPath, oDefaultContent, out bNotExists );
		}
		public T ReadXmlDataFile<T>( string sPath, T oDefaultContent, out bool bNotExist )
		{
			bNotExist = false;
			
			if( !FileExists(sPath) )
			{
				bNotExist = true;
				return oDefaultContent;
			}
			
			System.IO.Stream stream = GetReaderStream( sPath, false );
			
			T oRet = oDefaultContent;
			try
			{
				DataContractSerializer dcs = new DataContractSerializer(typeof(T));
			
				oRet = (T)dcs.ReadObject(stream);
			}
			catch( Exception )
			{
			}
			
			stream.Close();
			
			return oRet;
		}
		
		public bool WriteXmlDataFile( string sPath, object oData, bool bOverWrite )
		{
			if( FileExists(sPath) )
			{
				if( !bOverWrite ) return false;
				DeleteFile(sPath);
			}
			
			System.IO.Stream stream = CreateFile(sPath);
			
			DataContractSerializer dcs = new DataContractSerializer(oData.GetType());
			dcs.WriteObject(stream, oData);
			
			stream.Close();
			
			return true;
		}
		
		public void CreateFolderPath( string sPath )
		{
			
			string sToCre = "";
			int iStart = 0;
			int iPos;
			
			for(;;)
			{
				if( sToCre.Length > 0 ) sToCre += "\\";
				
				iPos = sPath.IndexOf('\\', iStart);
				if( iPos >= 0 )
					sToCre += sPath.Substring(iStart, iPos - iStart);
				else
					sToCre += sPath.Substring(iStart);
				
				if( !FolderExists( sToCre ) )
				{
					CreateFolder( sToCre );
				}
				
				if( iPos < 0 ) break;
				iStart = iPos + 1;
			}
		}
		
		//
		// //
		//
		
		public static long AvailableFreeSpace(out string sDrive)
		{
			sDrive = RscStore_Storage.IsoStoreDrive();
			return RscStore_IsoStore.AvailableFreeSpace();
		}
		
		public static void AddSysEvent( Exception e, string sCustTimeStampPart = "" )
		{
			string sException = "RootFrame_NavigationFailed...\n\nMessage:\n" + e.Message +
				"\n\nStack Trace:\n" + e.StackTrace;
			
			RscStore_IsoStore.AddSysEvent( sException, true, sCustTimeStampPart );
		}
		
		public static void AddSysEvent( string sText, bool bErr = false, string sCustTimeStampPart = "" )
		{
			RscStore_IsoStore.AddSysEvent( sText, bErr, sCustTimeStampPart );
		}
		
		//
		// //
		
	}
	
}