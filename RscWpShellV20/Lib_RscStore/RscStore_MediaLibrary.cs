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

using Microsoft.Xna.Framework.Media;

namespace Ressive.Store
{
	
	public class RscStore_MediaLibrary : RscStore_Base
	{
		
		public RscStore_MediaLibrary()
		{
		}
		
		// //
		//
		
		public override bool IsFolderImageOnly( string path )
		{
			string path_lo = path.ToLower();
			
			if( path_lo.IndexOf( "pictures\\" ) == 0 )
				return true;
			if( path_lo.IndexOf( "\\pictures\\" ) == 0 )
				return true;
			
			//Integrated into RscStore_KnownFolders...
			if( path_lo.IndexOf( "media library\\pictures\\" ) == 0 )
				return true;
			if( path_lo.IndexOf( "\\media library\\pictures\\" ) == 0 )
				return true;
			
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
			
			string sName = path.Substring( iPos, iPos2 - iPos );
			
			//Removing leading INDEX value...
			int iPos3 = sName.IndexOf( ')' );
			if( iPos3 > 0 )
				sName = sName.Substring( iPos3 + 2 );
			
			return sName;
		}
		
		//
		// //
		//
		
		public override System.IO.Stream GetReaderStream( string path, bool bForPreview )
		{
			//MUST NOT!!!
			//if( path.IndexOf( '.' ) < 0 ) return null; //No length for Albums...
			
			Picture p = GetPictureObject( path );
			if( p == null ) return null; //Not found...
			
			if( bForPreview )
				return p.GetThumbnail();
			
			return p.GetImage();
		}
		
		public override bool FileExists( string path )
		{
			//MUST NOT!!!
			//if( path.IndexOf( '.' ) < 0 ) return false; //No exists for Albums...
			
			Picture p = GetPictureObject( path );
			if( p == null ) return false; //Not found...
			
			return true;
		}
		
		public override bool FolderExists( string path )
		{
			//MUST NOT!!!
			//if( path.IndexOf( '.' ) < 0 ) return false; //No exists for Albums...
			
			PictureAlbum pa = GetPictureAlbumObject( path );
			if( pa == null ) return false; //Not found...
			
			return true;
		}
		
		public override RscStoreProperties GetFileProperties( string path )
		{
			RscStoreProperties sp = new RscStoreProperties();
			
			Picture p = GetPictureObject( path );
			if( p == null ) return sp;
			
			sp.CreationTime = p.Date;
			
			sp.Length = p.GetImage().Length;
			
			return sp;
		}
		
		public override RscStoreProperties GetFolderProperties( string path )
		{
			return new RscStoreProperties();
		}
		
		public override long GetFileLength( string path )
		{
			Picture p = GetPictureObject( path );
			if( p == null ) return -1; //Not found...
			
			return p.GetImage().Length;
		}
		
		public override DateTimeOffset GetFileCreationTime( string path )
		{
			Picture p = GetPictureObject( path );
			if( p == null ) return new DateTimeOffset(); //Not found...
			
			return new DateTimeOffset( p.Date );
		}
		
		public override DateTimeOffset GetFileLastWriteTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public override DateTimeOffset GetFileLastAccessTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public override DateTimeOffset GetFolderCreationTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public override DateTimeOffset GetFolderLastWriteTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public override DateTimeOffset GetFolderLastAccessTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public override string [] GetFileNames( string path, string wildcard, bool bIncludeHidden, bool bStartWithInfChr )
		{
			int length = 0;
			string [] aPath = PathParts( path, out length );
			
			if( length == 0 )
			{
				return new String [] {};
			}
			else
			{
				bool bPictures = false;
				int iLevelStart = 0;
				if( length > 0 )
				{
					if( aPath[ 0 ].ToLower().CompareTo( "pictures" ) == 0 )
					{
						bPictures = true;
						iLevelStart = 1;
					}
				}
				if( length > 1 )
				{
					//Known Folder integration...
					if( aPath[ 0 ].ToLower().CompareTo( "media library" ) == 0 )
					{
						if( aPath[ 1 ].ToLower().CompareTo( "pictures" ) == 0 )
						{
							bPictures = true;
							iLevelStart = 2;
						}
					}
				}
				
				if( bPictures )
				{
					MediaLibrary MedLib = new MediaLibrary();
					PictureAlbum pa = MedLib.RootPictureAlbum;
					
					bool bHit = false;
					for( int iPos = iLevelStart; iPos < length; iPos++ )
					{
						foreach( PictureAlbum paHit in pa.Albums )
						{
							if( paHit.Name.ToLower().CompareTo( aPath[ iPos ].ToLower() ) == 0 )
							{
								bHit = true;
								pa = paHit;
								break;
							}
						}
						if( !bHit ) return new String [] {}; //Album not found...
					}
					
					if( pa.Pictures.Count == 0 ) return new String [] {}; //No Pictures...
					
					RscWildCard wc = new RscWildCard( wildcard );
					
					string [] astr = new String[ pa.Pictures.Count ];
					
					int iLenSave = astr.Length;
					int i = 0;
					foreach (Picture pHit in pa.Pictures)
					{
						/*
						string sTit = "";
						if( iIdx < 1000 ) sTit += "0";
						if( iIdx < 100 ) sTit += "0";
						if( iIdx < 10 ) sTit += "0";
						sTit = sTit + iIdx.ToString() + ") " + pHit.Name;
						
						if( wc.Wanted( sTit ) )
						*/
						
						if( wc.Wanted( pHit.Name ) )
						{
							int iIdx = i + 1;
							string sTit = "";
							if( iIdx < 1000 ) sTit += "0";
							if( iIdx < 100 ) sTit += "0";
							if( iIdx < 10 ) sTit += "0";
							sTit = sTit + iIdx.ToString() + ") " + pHit.Name;
							
							if( bStartWithInfChr )
								astr[ i ] = "n"; //Normal...
							else
								astr[ i ] = "";
							astr[ i ] += sTit;
							
							i++;
						}
					}
					
					if( i == 0 ) return new String [] {}; //All filtered out...
					if( i == iLenSave ) return astr; //All wanted...
					
					//Not so nice... :(
					string [] astr2 = new String[ i ];
					for( int j = 0; j < i; j++ )
					{
						astr2[ j ] = astr[ j ];
					}
					
					return astr2;
				}
				else
				{
					return new String [] {};
				}
			}
			
			//return new String [] {};
		}
		
		public override string [] GetFolderNames( string path, string wildcard, bool bIncludeHidden, bool bStartWithInfChr )
		{
			int length = 0;
			string [] aPath = PathParts( path, out length );
			
			if( length == 0 && (wildcard.Length == 0 || wildcard == "*.*") )
			{
				if( bStartWithInfChr )
					return new String [] {"nPictures"}; //Normal...
				else
					return new String [] {"Pictures"};
			}
			else if( length == 1 && aPath[ 0 ].ToLower().CompareTo( "media library" ) == 0 && (wildcard.Length == 0 || wildcard == "*.*") )
			{
				//Known Folder integration...
				if( bStartWithInfChr )
					return new String [] {"nPictures"}; //Normal...
				else
					return new String [] {"Pictures"};
			}
			else
			{
				bool bPictures = false;
				int iLevelStart = 0;
				if( length > 0 )
				{
					if( aPath[ 0 ].ToLower().CompareTo( "pictures" ) == 0 )
					{
						bPictures = true;
						iLevelStart = 1;
					}
				}
				if( length > 1 )
				{
					//Known Folder integration...
					if( aPath[ 0 ].ToLower().CompareTo( "media library" ) == 0 )
					{
						if( aPath[ 1 ].ToLower().CompareTo( "pictures" ) == 0 )
						{
							bPictures = true;
							iLevelStart = 2;
						}
					}
				}
				
				if( bPictures )
				{
					MediaLibrary MedLib = new MediaLibrary();
					PictureAlbum pa = MedLib.RootPictureAlbum;
					
					bool bHit = false;
					for( int iPos = iLevelStart; iPos < length; iPos++ )
					{
						foreach( PictureAlbum paHit in pa.Albums )
						{
							if( paHit.Name.ToLower().CompareTo( aPath[ iPos ].ToLower() ) == 0 )
							{
								bHit = true;
								pa = paHit;
								break;
							}
						}
						if( !bHit ) return new String [] {}; //Album not found...
					}
					
					if( pa.Albums.Count == 0 ) return new String [] {}; //No Sub Album...
					
					RscWildCard wc = new RscWildCard( wildcard );
					
					string [] astr = new String[ pa.Albums.Count ];
					
					int iLenSave = astr.Length;
					int i = 0;
					foreach (PictureAlbum paHit in pa.Albums)
					{
						if( wc.Wanted( paHit.Name ) )
						{
							if( bStartWithInfChr )
								astr[ i ] = "n"; //Normal...
							else
								astr[ i ] = "";
							astr[ i ] += paHit.Name;
							
							i++;
						}
					}
					
					if( i == 0 ) return new String [] {}; //All filtered out...
					if( i == iLenSave ) return astr; //All wanted...
					
					//Not so nice... :(
					string [] astr2 = new String[ i ];
					for( int j = 0; j < i; j++ )
					{
						astr2[ j ] = astr[ j ];
					}
					
					return astr2;
				}
				else
				{
					return new String [] {};
				}
			}
			
			//return new String [] {};
		}
		
		//
		// //
		//
		
		private Picture GetPictureObject( string path )
		{
			int length = 0;
			string [] aPath = PathParts( path, out length );
			
			if( length == 0 )
			{
				return null;
			}
			else
			{
				bool bPictures = false;
				int iLevelStart = 0;
				if( length > 0 )
				{
					if( aPath[ 0 ].ToLower().CompareTo( "pictures" ) == 0 )
					{
						bPictures = true;
						iLevelStart = 1;
					}
				}
				if( length > 1 )
				{
					//Known Folder integration...
					if( aPath[ 0 ].ToLower().CompareTo( "media library" ) == 0 )
					{
						if( aPath[ 1 ].ToLower().CompareTo( "pictures" ) == 0 )
						{
							bPictures = true;
							iLevelStart = 2;
						}
					}
				}
				
				if( bPictures )
				{
					MediaLibrary MedLib = new MediaLibrary();
					PictureAlbum pa = MedLib.RootPictureAlbum;
					
					bool bHit = false;
					for( int iPos = iLevelStart; iPos < (length - 1); iPos++ )
					{
						foreach( PictureAlbum paHit in pa.Albums )
						{
							if( paHit.Name.ToLower().CompareTo( aPath[ iPos ].ToLower() ) == 0 )
							{
								bHit = true;
								pa = paHit;
								break;
							}
						}
						if( !bHit ) return null; //Album not found...
					}
					
					if( pa.Pictures.Count == 0 ) return null; //No Pictures...
					
					int iPInd = -1;
					string sFn = aPath[ length - 1 ];
					int iDot = sFn.IndexOf(')');
					if( iDot >= 0 )
					{
						iPInd = Int32.Parse( sFn.Substring(0, iDot ) );
						sFn = sFn.Substring(iDot + 2);
					}
					sFn = sFn.ToLower();
					
					if( iPInd >= 0 && iPInd < pa.Pictures.Count )
					{
						if( pa.Pictures[ iPInd ].Name.ToLower().CompareTo( sFn ) == 0 )
							return pa.Pictures[ iPInd ];
					}
						
					foreach( Picture pHit in pa.Pictures )
					{
						if( pHit.Name.ToLower().CompareTo( sFn ) == 0 )
							return pHit;
					}
					
					return null;
				}
				else
				{
					return null;
				}
			}
			
			//return null;
		}
		
		private PictureAlbum GetPictureAlbumObject( string path )
		{
			int length = 0;
			string [] aPath = PathParts( path, out length );
			
			if( length == 0 )
			{
				return null;
			}
			else
			{
				bool bPictures = false;
				int iLevelStart = 0;
				if( length > 0 )
				{
					if( aPath[ 0 ].ToLower().CompareTo( "pictures" ) == 0 )
					{
						bPictures = true;
						iLevelStart = 1;
					}
				}
				if( length > 1 )
				{
					//Known Folder integration...
					if( aPath[ 0 ].ToLower().CompareTo( "media library" ) == 0 )
					{
						if( aPath[ 1 ].ToLower().CompareTo( "pictures" ) == 0 )
						{
							bPictures = true;
							iLevelStart = 2;
						}
					}
				}
				
				if( bPictures )
				{
					MediaLibrary MedLib = new MediaLibrary();
					PictureAlbum pa = MedLib.RootPictureAlbum;
					
					bool bHit = false;
					for( int iPos = iLevelStart; iPos < length; iPos++ )
					{
						foreach( PictureAlbum paHit in pa.Albums )
						{
							if( paHit.Name.ToLower().CompareTo( aPath[ iPos ].ToLower() ) == 0 )
							{
								bHit = true;
								pa = paHit;
								break;
							}
						}
						if( !bHit ) return null; //Album not found...
					}
					
					return pa;
				}
				else
				{
					return null;
				}
			}
			
			//return null;
		}

		//
		// //
		
	}
	
}