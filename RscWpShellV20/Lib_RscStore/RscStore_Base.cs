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

namespace Ressive.Store
{
	
	public class RscStore_Base
	{
		
		public static string [] PathParts( string path, out int length /*, out string wildcard*/ )
		{
			length = 0;
			
			/*
			wildcard = "";
			*/
			
			if( path.Length == 0 ) return new String [] {};
			if( path[ 0 ] == '\\' ) path = path.Substring(1);
			if( path.Length == 0 ) return new String [] {};
			if( path[ path.Length - 1 ] == '\\' ) path = path.Substring( 0, path.Length - 1 );
			if( path.Length == 0 ) return new String [] {};
			
			string [] astr = path.Split( '\\' );
			if( astr.Length == 0 ) return astr;
			
			length = astr.Length;
			
			/*
			if( astr[ length - 1 ].IndexOf( '*' ) >= 0 || astr[ length - 1 ].IndexOf( '?' ) >= 0 )
			{
				wildcard = astr[ length - 1 ];
				length--;
			}
			*/
			
			return astr;
		}
		
		public RscStore_Base()
		{
		}
		
		// //
		//
		
		public virtual RscStore_Base GetSpecialStorage( string path )
		{
			return this;
		}
		
		public virtual bool IsFolderImageOnly( string path )
		{
			return false;
		}
		
		public virtual string GetOriginalFileNameOfPath( string path )
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
		
		public virtual void DeleteFolder( string path )
		{
			throw new Exception( "Not implemented (rsc): Delete Folder" );
		}
		
		public virtual void CreateFolder( string path )
		{
			throw new Exception( "Not implemented (rsc): Create Folder" );
		}
		
		public virtual void DeleteFile( string path )
		{
			throw new Exception( "Not implemented (rsc): Delete File" );
		}
		
		public virtual System.IO.Stream CreateFile( string path )
		{
			throw new Exception( "Not implemented (rsc): Create File" );
		}
		
		/*
		public virtual Stream GetWriterStream( string path )
		{
			throw new Exception( "Not implemented (rsc): Get Writer Stream" );
			return null;
		}
		*/
		
		public virtual System.IO.Stream GetReaderStream( string path, bool bForPreview )
		{
			throw new Exception( "Not implemented (rsc): Get Reader Stream" );
		}
		
		public virtual bool FileExists( string path )
		{
			throw new Exception( "Not implemented (rsc): File Exists" );
		}
		
		public virtual bool FolderExists( string path )
		{
			throw new Exception( "Not implemented (rsc): Folder Exists" );
		}
		
		public virtual RscStoreProperties GetFileProperties( string path )
		{
			return new RscStoreProperties();
		}
		
		public virtual RscStoreProperties GetFolderProperties( string path )
		{
			return new RscStoreProperties();
		}
		
		public virtual long GetFileLength( string path )
		{
			return -1;
		}
		
		public virtual DateTimeOffset GetFileCreationTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public virtual DateTimeOffset GetFileLastWriteTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public virtual DateTimeOffset GetFileLastAccessTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public virtual DateTimeOffset GetFolderCreationTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public virtual DateTimeOffset GetFolderLastWriteTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public virtual DateTimeOffset GetFolderLastAccessTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public virtual string [] GetFileNames( string path, string wildcard, bool bIncludeHidden, bool bStartWithInfChr )
		{
			return new String [] {};
		}
		
		public virtual string [] GetFolderNames( string path, string wildcard, bool bIncludeHidden, bool bStartWithInfChr )
		{
			return new String [] {};
		}
		
		//
		// //
		
	}
	
}