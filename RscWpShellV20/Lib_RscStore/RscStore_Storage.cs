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
using System.Threading.Tasks;

namespace Ressive.Store
{
	
	public class RscStore_Storage : RscStore_Base
	{
		
		public RscStore_Storage()
		{
		}
		
		// //
		//
		
		public virtual string [] ListRootFolders( bool bIncludeHidden, bool bStartWithInfChr )
		{
			//To support RscStore_KnownFolders class...
			return new String [] {};
		}
		
		public virtual Windows.Storage.StorageFolder GetRootFolderByNameLower( string sNameLower )
		{
			Windows.Storage.StorageFolder fldr = null;
			
			try
			{
				string sPath;
				
				//sPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
				//sPath += @"\Assets";
				
				sPath = sNameLower.ToUpper(); //"d:"
					
				var taskHlp = Task.Run(async () => { fldr = await GetFolderFromPathAsync(sPath); });
				taskHlp.Wait();
			}
			catch( Exception )
			{
			}
			
			return fldr;
		}
		
		//
		// //
		//
		
		public override bool IsFolderImageOnly( string path )
		{
			//Additional files may occure, desktop.ini for example...
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
		
		public override System.IO.Stream CreateFile( string path )
		{
			int iPos = path.LastIndexOf( '\\' );
			if( iPos < 0 ) throw new Exception( "Invalid path: " + path );
			
			string sPathFldr = path.Substring( 0, iPos );
			string sPathItem = path.Substring( iPos + 1 );
			
			Windows.Storage.StorageFolder fldr = GetFolderObject( sPathFldr );
			if( fldr == null )
				throw new Exception( "Folder does not exist: " + sPathFldr );
			
			Windows.Storage.StorageFile fle = null;
			
			var taskHlp1 = Task.Run(async () => { fle = await CreateFileAsync(fldr, sPathItem); });
			taskHlp1.Wait();	
			
			if( fle == null )
				throw new Exception( "Can not create file: " + path );
			
			System.IO.Stream strm = null;
			
			var taskHlp2 = Task.Run(async () => { strm = await GetFileWriterStreamAsync(fle); });
			taskHlp2.Wait();
			
			return strm;
		}
		
		/*
		public override Stream GetWriterStream( string path )
		{
			return null;
			
			/*
			byte[] data = Encoding.Unicode.GetBytes(content);
		
			var folder = ApplicationData.Current.LocalFolder;
			var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
		
			using (var s = await file.OpenStreamForWriteAsync())
			{
				await s.WriteAsync(data, 0, data.Length);
			}
			*
		}
		*/
		
		public override System.IO.Stream GetReaderStream( string path, bool bForPreview )
		{
			//FAILS...
			/*
			if( bForPreview )
			{
				MessageBox.Show( path );
				
				StorageFile fle = GetFileObject( path );
				if( fle == null )
				{
					MessageBox.Show( "fle is null" );
					return null;
				}
				
				StorageItemThumbnail tn = null;
				
				//protected async static Task<StorageItemThumbnail> GetFileThumbnailAsync( StorageFile fle )
				
				MessageBox.Show( "Before call." );
				
				var taskHlp1 = Task.Run(async () => { tn = await GetFileThumbnailAsync(fle); });
				taskHlp1.Wait();
				
				if( tn == null )
				{
					MessageBox.Show( "tn is null" );
					return null;
				}
				
				MessageBox.Show( "Before AsStream." );
				
				Stream strm = tn.AsStream();
				if( strm == null )
				{
					MessageBox.Show( "strm is null" );
					return null;
				}
				
				MessageBox.Show( "Stream length: " + strm.Length.ToString() );
				
				return strm;
				
				/*
				InMemoryRandomAccessStream win8Stream = GetData(); // Get a data stream from somewhere.
				System.IO.Stream inputStream = win8Stream.AsStream()
				
				For converting from .NET streams -> WinRT streams:
				
				Windows.Storage.Streams.IInputStream inStream = stream.AsInputStream();
				Windows.Storage.Streams.IOutputStream outStream = stream.AsOutputStream();
				
				//
				// //
				//
				StorageFile storageFile =
				await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
			
				var randomAccessStream = await storageFile.OpenReadAsync();
				Stream stream = randomAccessStream.AsStreamForRead();
				*
				
				//return tn.AsStream();
				//return tn.GetInputStreamAt( 0 ).AsStreamForRead();
			}
			else
			*/
			{
				
				//WORKS FINE!!!
				/*
				int iPos = path.LastIndexOf( '\\' );
				if( iPos < 0 )
					return null;
				
				StorageFolder fldr = GetFolderObject( path.Substring( 0, iPos ) );
				if( fldr == null )
					return null;
				
				Stream strm = null;
				
				var taskHlp1 = Task.Run(async () => { strm = await GetFileReaderStreamAsync(fldr, path.Substring( iPos + 1 )); });
				taskHlp1.Wait();
	
				return strm;
				*/
				
				Windows.Storage.StorageFile fle = GetFileObject( path );
				if( fle == null )
				{
					MessageBox.Show( "fle is null" );
					return null;
				}
				
				Windows.Storage.Streams.IRandomAccessStreamWithContentType strm = null;
				
				var taskHlp1 = Task.Run(async () => { strm = await GetFileReaderStreamAsync(fle); });
				taskHlp1.Wait();
				
				if( strm == null )
					return null;
				
				return strm.AsStreamForRead();
			}
		}
		
		public override bool FileExists( string path )
		{
			Windows.Storage.StorageFile fle = GetFileObject( path );
			if( fle == null )
				return false;
			
			return true;
		}
		
		public override bool FolderExists( string path )
		{
			Windows.Storage.StorageFolder fldr = GetFolderObject( path );
			if( fldr == null )
				return false;
			
			return true;
		}
		
		public override RscStoreProperties GetFileProperties( string path )
		{
			RscStoreProperties sp = new RscStoreProperties();
			
			Windows.Storage.StorageFile fle = GetFileObject( path );
			if( fle == null )
				return sp;
			
			sp.CreationTime = fle.DateCreated.DateTime;
			
			Windows.Storage.FileProperties.BasicProperties bp = null;
					
			var taskHlp1 = Task.Run(async () => { bp = await GetFileBasicPropertiesAsync(fle); });
			taskHlp1.Wait();
			
			sp.LastWriteTime = bp.DateModified.DateTime;
			sp.Length = (long) bp.Size;

			//FAILS...
			/*
			readonly string dateAccessedProperty = "System.DateAccessed";
			readonly string fileOwnerProperty = "System.FileOwner";
			List<string> propertiesName = new List<string>();
			propertiesName.Add(dateAccessedProperty);
			//propertiesName.Add(fileOwnerProperty);
			
			// Get the specified properties through StorageFile.Properties
			IDictionary<string, object> extraProperties = null;
			
			var taskHlp2 = Task.Run(async () => { extraProperties = await GetFileProperties(fle, propertiesName); });
			taskHlp2.Wait();
			
			var propValue = extraProperties[dateAccessedProperty];
			if (propValue != null)
			{
				outputText.AppendLine("Date accessed: " + propValue);
			}
			propValue = extraProperties[fileOwnerProperty];
			if (propValue != null)
			{
				outputText.AppendLine("File owner: " + propValue);
			}
			*/
			
			return sp;
		}
		
		public override RscStoreProperties GetFolderProperties( string path )
		{
			RscStoreProperties sp = new RscStoreProperties();
			
			Windows.Storage.StorageFolder fldr = null;
			
			try
			{
				fldr = GetFolderObject( path );
			}
			catch( Exception )
			{
			}
			
			if( fldr == null )
				return sp;
			
			try
			{
				sp.FullPath = fldr.Path;
			}
			catch( Exception )
			{
			}
			
			try
			{
				sp.DisplayName = fldr.DisplayName;
			}
			catch( Exception )
			{
			}
			
			sp.CreationTime = fldr.DateCreated.DateTime;
			
			/*
			const string freeSpaceProperty = "System.FreeSpace";
			IDictionary<string, object> extraProperties = null;
			try
			{
				MessageBox.Show( "Start..." );
				var taskHlp2 = Task.Run(async () => { extraProperties = await GetFolderPropertiesAsync(fldr, new string[] { freeSpaceProperty }); });
				taskHlp2.Wait();
				MessageBox.Show( "Stop..." );
			}
			catch( Exception )
			{
				MessageBox.Show( "Err..." );
			}
			if( extraProperties != null )
			{
				var propValue = extraProperties[freeSpaceProperty];
				if (propValue != null)
				{
					sp.DisplayName = propValue.ToString();
				}
			}
			*/
			
			//FAILS...
			/*
			UInt64 fs = 0;
			try
			{
				var taskHlp2 = Task.Run(async () => { fs = await GetFolderFreeSpaceAsync(fldr); });
				taskHlp2.Wait();
			}
			catch( Exception )
			{
			}
			sp.DisplayName = "free space: " + fs.ToString();
			*/
			
			return sp;
		}
		
		public override long GetFileLength( string path )
		{
			Windows.Storage.StorageFile fle = GetFileObject( path );
			if( fle == null )
				return -1;
			
			Windows.Storage.FileProperties.BasicProperties bp = null;
					
			var taskHlp1 = Task.Run(async () => { bp = await GetFileBasicPropertiesAsync(fle); });
			taskHlp1.Wait();
			
			return (long) bp.Size;
		}
		
		public override DateTimeOffset GetFileCreationTime( string path )
		{
			Windows.Storage.StorageFile fle = GetFileObject( path );
			if( fle == null )
				return new DateTimeOffset();
			
			return fle.DateCreated;
		}
		
		public override DateTimeOffset GetFileLastWriteTime( string path )
		{
			Windows.Storage.StorageFile fle = GetFileObject( path );
			if( fle == null )
				return new DateTimeOffset();
			
			Windows.Storage.FileProperties.BasicProperties bp = null;
					
			var taskHlp1 = Task.Run(async () => { bp = await GetFileBasicPropertiesAsync(fle); });
			taskHlp1.Wait();
			
			return bp.DateModified;
		}
		
		public override DateTimeOffset GetFileLastAccessTime( string path )
		{
			return new DateTimeOffset();
		}
		
		public override DateTimeOffset GetFolderCreationTime( string path )
		{
			Windows.Storage.StorageFolder fldr = GetFolderObject( path );
			if( fldr == null )
				return new DateTimeOffset();
			
			return fldr.DateCreated;
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
				Windows.Storage.StorageFolder fldr = GetRootFolderByNameLower( aPath[ 0 ].ToLower() );
				
				if( length > 1 && fldr != null )
				{
					string sSubFolderPath = "";
					for( int iFldr = 1; iFldr < length; iFldr++ )
					{
						if( sSubFolderPath.Length > 0 ) sSubFolderPath += "\\";
						sSubFolderPath += aPath[ iFldr ];
					}
					if( sSubFolderPath.Length > 0 )
					{
						var taskHlpSf = Task.Run(async () => { fldr = await GetSubFolderBySubPathAsync(fldr, sSubFolderPath); });
						taskHlpSf.Wait();
						
						//FREEZ!!!
						/*
						Task<StorageFolder> tskSf = GetSubFolderBySubPathAsync(fldr, sSubFolderPath);
						tskSf.Wait();
						//if( tskSf.Wait( TimeSpan.FromSeconds( ciTimeoutSeconds ) ) )
							fldr = tskSf.Result;
						//else
						//	return new String [] {};
						*/
						
						if( fldr == null )
							return new String [] {};
					}
					
					string [] astrRes = new String [] {};
					
					try
					{
						var taskHlp = Task.Run(async () => { astrRes = await GetFileNamesAsync(fldr, wildcard); });
						taskHlp.Wait();
					}
					catch( Exception )
					{
					}
					
					if( bStartWithInfChr )
					{
						for( int i = 0; i < astrRes.Length; i++ )
							astrRes[ i ] = "n" + astrRes[ i ]; //Normal...
					}
					
					return astrRes;
					
					//FREEZ!!!
					/*
					Task<string []> tsk = GetFileNamesAsync(fldr);
					tsk.Wait();
					//if( tsk.Wait( TimeSpan.FromSeconds( ciTimeoutSeconds ) ) )
						return tsk.Result;
					//return new String [] {};
					*/
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
			
			if( length == 0 )
			{
				return ListRootFolders( bIncludeHidden, bStartWithInfChr );
			}
			else
			{
				Windows.Storage.StorageFolder fldr = GetRootFolderByNameLower( aPath[ 0 ].ToLower() );
				
				if( fldr != null )
				{
					string sSubFolderPath = "";
					for( int iFldr = 1; iFldr < length; iFldr++ )
					{
						if( sSubFolderPath.Length > 0 ) sSubFolderPath += "\\";
						sSubFolderPath += aPath[ iFldr ];
					}
					if( sSubFolderPath.Length > 0 )
					{
						var taskHlpSf = Task.Run(async () => { fldr = await GetSubFolderBySubPathAsync(fldr, sSubFolderPath); });
						taskHlpSf.Wait();
						
						//FREEZ!!!
						/*
						Task<StorageFolder> tskSf = GetSubFolderBySubPathAsync(fldr, sSubFolderPath);
						tskSf.Wait();
						//if( tskSf.Wait( TimeSpan.FromSeconds( ciTimeoutSeconds ) ) )
							fldr = tskSf.Result;
						//else
						//	return new String [] {};
						*/
						
						if( fldr == null )
							return new String [] {};
					}
					
					string [] astr = new String [] {};
					
					try
					{
						var taskHlp = Task.Run(async () => { astr = await GetDirectoryNamesAsync(fldr, wildcard); });
						taskHlp.Wait();
					}
					catch( Exception )
					{
					}
			
					if( (!bIncludeHidden) || bStartWithInfChr )
					{
						int iCntNormal = 0;
						string [] astrNormal = null;
						
						for( int iCyc = 0; iCyc < 2; iCyc++ )
						{
							for( int i = 0; i < astr.Length; i++ )
							{
								bool bIsRoot = false;
								if( path.Length == 2 )
								{
									if( path[ 1 ] == ':' )
										bIsRoot = true;
								}
								if( path.Length == 3 )
								{
									if( path[ 1 ] == ':' && path[ 2 ] == '\\' )
										bIsRoot = true;
								}
								
								if( bIsRoot )
								{
									string sLo = astr[ i ].ToLower();
									switch( sLo )
									{
										
										case "system volume information" : //WP
										case "wpsystem" : //WP
										{
											if( bIncludeHidden && (iCyc == 0) && bStartWithInfChr ) astr[ i ] = "w" + astr[i];
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
					
					//FREEZ!!!
					/*
					Task<string []> tsk = GetDirectoryNamesAsync(fldr);
					tsk.Wait();
					//if( tsk.Wait( TimeSpan.FromSeconds( ciTimeoutSeconds ) ) )
						return tsk.Result;
					//return new String [] {};
					*/
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
		
		private Windows.Storage.StorageFile GetFileObject( string path )
		{
			
			int length = 0;
			string [] aPath = PathParts( path, out length );
			
			if( length == 0 )
			{
				return null;
			}
			else
			{
				Windows.Storage.StorageFolder fldr = GetRootFolderByNameLower( aPath[ 0 ].ToLower() );
				
				if( length > 1 && fldr != null )
				{
					string sSubFolderPath = "";
					for( int iFldr = 1; iFldr < length; iFldr++ )
					{
						if( sSubFolderPath.Length > 0 ) sSubFolderPath += "\\";
						sSubFolderPath += aPath[ iFldr ];
					}
					if( sSubFolderPath.Length > 0 )
					{
						Windows.Storage.StorageFile fle = null;
						
						var taskHlpSf = Task.Run(async () => { fle = await GetFileBySubPathAsync(fldr, sSubFolderPath); });
						taskHlpSf.Wait();
						
						return fle;
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
		
		public Windows.Storage.StorageFolder GetFolderObject( string path )
		{
			
			int length = 0;
			string [] aPath = PathParts( path, out length );
			
			if( length == 0 )
			{
				return null;
			}
			else
			{
				Windows.Storage.StorageFolder fldr = GetRootFolderByNameLower( aPath[ 0 ].ToLower() );
				
				if( length > 1 && fldr != null )
				{
					string sSubFolderPath = "";
					for( int iFldr = 1; iFldr < length; iFldr++ )
					{
						if( sSubFolderPath.Length > 0 ) sSubFolderPath += "\\";
						sSubFolderPath += aPath[ iFldr ];
					}
					if( sSubFolderPath.Length > 0 )
					{
						var taskHlpSf = Task.Run(async () => { fldr = await GetSubFolderBySubPathAsync(fldr, sSubFolderPath); });
						taskHlpSf.Wait();
					}
					
					return fldr;
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
		//
		
		//FAILS...
		/*
		protected async static Task<StorageItemThumbnail> GetFileThumbnailAsync( StorageFile fle )
		{
			StorageItemThumbnail tn = null;
			
			try
			{
				tn = await fle.GetThumbnailAsync( ThumbnailMode.SingleItem ); //.PicturesView );
			}
			catch( System.IO.FileNotFoundException e )
			{
				//NOT EXIST...
			}
			
			return tn;
		}
		*/
		
		protected async static Task<Windows.Storage.StorageFile> CreateFileAsync( Windows.Storage.StorageFolder fldr, string sFileName )
		{
			Windows.Storage.StorageFile fle = null;
			
			//
			// FAILS!!! - Not implemented.
			//
			fle = await fldr.CreateFileAsync(sFileName, Windows.Storage.CreationCollisionOption.FailIfExists ); //CreationCollisionOption.ReplaceExisting);
			
			return fle;
		}
		
		protected async static Task<System.IO.Stream> GetFileWriterStreamAsync( Windows.Storage.StorageFile fle )
		{
			System.IO.Stream strm = null;
			
			strm = await fle.OpenStreamForWriteAsync();
			
			return strm;
		}
		
		protected async static Task<Windows.Storage.Streams.IRandomAccessStreamWithContentType> GetFileReaderStreamAsync( Windows.Storage.StorageFile fle )
		{
			Windows.Storage.Streams.IRandomAccessStreamWithContentType strm = null;
			
            strm = await fle.OpenReadAsync();
			
			return strm;
		}
		
		protected async static Task<Windows.Storage.StorageFolder> GetFolderFromPathAsync(string sFolderPath )
		{
			Windows.Storage.StorageFolder fldrSub = null;
			
			try
			{
                fldrSub = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(sFolderPath);
			}
			catch( System.IO.FileNotFoundException )
			{
				//NOT EXIST...
			}
			
			return fldrSub;
		}
		
		//FAILS...
		/*
		protected async static Task<UInt64> GetFolderFreeSpaceAsync( StorageFolder fldr )
		{
			var retrivedProperties = await fldr.Properties.RetrievePropertiesAsync(new string[] { "System.FreeSpace" });
			return (UInt64)retrivedProperties["System.FreeSpace"];
		}
		
		protected async static Task<IDictionary<string, object>> GetFolderPropertiesAsync( StorageFolder fldr, string [] propertiesName )
		{
			return await fldr.Properties.RetrievePropertiesAsync(propertiesName);
		}
		*/
		
		protected async static Task<IDictionary<string, object>> GetFilePropertiesAsync( Windows.Storage.StorageFile fle, List<string> propertiesName )
		{
			return await fle.Properties.RetrievePropertiesAsync(propertiesName);
		}
		
		protected async static Task<Windows.Storage.FileProperties.BasicProperties> GetFileBasicPropertiesAsync( Windows.Storage.StorageFile fle )
		{
            return await fle.GetBasicPropertiesAsync();
		}
		
		protected async static Task<Windows.Storage.StorageFile> GetFileBySubPathAsync(Windows.Storage.StorageFolder fldr, string sSubFolderPath )
		{
			Windows.Storage.StorageFile fle = null;
			
			try
			{
                fle = await fldr.GetFileAsync(sSubFolderPath);
			}
			catch( System.IO.FileNotFoundException )
			{
				//NOT EXIST...
			}
			
			return fle;
		}
		
		protected async static Task<Windows.Storage.StorageFolder> GetSubFolderBySubPathAsync(Windows.Storage.StorageFolder fldr, string sSubFolderPath )
		{
			Windows.Storage.StorageFolder fldrSub = null;
			
			try
			{
                fldrSub = await fldr.GetFolderAsync(sSubFolderPath);
			}
			catch( System.IO.FileNotFoundException )
			{
				//NOT EXIST...
			}
			
			return fldrSub;
		}
		
		protected async static Task<string []> GetFileNamesAsync(Windows.Storage.StorageFolder fldr, string wildcard )
		{			
			//
			// NOT WORKING!!!
			//
			/*
			else
			{
				using Windows.Storage.Search;
			
				if( !fldr.IsCommonFileQuerySupported( CommonFileQuery.DefaultQuery ) )
					throw new Exception( "Unable to query!" );
				
				/*
				string sTypeFilter = "*";
				if( sSearchPattern.IndexOf( "*." ) == 0 )
					sTypeFilter = sSearchPattern.Substring( 1 );
				else
					sTypeFilter = sSearchPattern; //This mustn't working...
				*
				
				List<string> lstTypeFilter = new List<string>();
				lstTypeFilter.Add("*");
				
				//QueryOptions queryOptions = new QueryOptions(); //CommonFileQuery.DefaultQuery); //, lstTypeFilter);
				//queryOptions.UserSearchFilter = sSearchPattern;
				
				QueryOptions queryOptions = new QueryOptions();
				queryOptions.FolderDepth = Windows.Storage.Search.FolderDepth.Deep; 
				queryOptions.IndexerOption = Windows.Storage.Search.IndexerOption.UseIndexerWhenAvailable; 
				//queryOptions.UserSearchFilter = "'" + sSearchPattern + "'";
				
				//"'" + Begriff + @"' folder:" + Ordner
				//"last quarter" author:(theresa OR lee) folder:MyDocs
				
				StorageFileQueryResult queryResult = fldr.CreateFileQueryWithOptions(queryOptions);
				
				IReadOnlyList<StorageFile> subfiles = await queryResult.GetFilesAsync();
				
				astr = new String[ subfiles.Count ];
				int i = 0;
				foreach (StorageFile file in subfiles)
				{
					astr[ i ] = file.Name;
					i++;
				}
			}
			*/
	
			IReadOnlyList <Windows.Storage.StorageFile> subfiles =
					await fldr.GetFilesAsync();		
			
			RscWildCard wc = new RscWildCard( wildcard );

			string [] astr = new String[ subfiles.Count ];
			
			int iLenSave = astr.Length;
			int i = 0;
			foreach (Windows.Storage.StorageFile file in subfiles)
			{
				if( wc.Wanted( file.Name ) )
				{
					astr[ i ] = file.Name;
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
		
		protected async static Task<string []> GetDirectoryNamesAsync(Windows.Storage.StorageFolder fldr, string wildcard)
		{
			IReadOnlyList <Windows.Storage.StorageFolder> subfolders =
                     await fldr.GetFoldersAsync();
			
			RscWildCard wc = new RscWildCard( wildcard );
			
			string [] astr = new String[ subfolders.Count ];
			
			int iLenSave = astr.Length;
			int i = 0;
            foreach (Windows.Storage.StorageFolder folder in subfolders)
            {
				if( wc.Wanted( folder.Name ) )
				{
					astr[ i ] = folder.Name;
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
		
		//
		// //
		
		// //
		// Extras {
		
		public static string IsoStoreDrive( )
		{
			string sPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
			return sPath.Substring( 0, 2 );
		}
		
		public static string IsoStoreDevicePath( )
		{
			return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
		}
		
		public static Windows.Storage.StorageFile GetStorageFileObject( string path, out string err )
		{
			err = "";
			
			string sPath = "";
			
			switch( path.Substring(0, 2).ToUpper() )
			{
				
				case "A:" :
					sPath = IsoStoreDevicePath() + path.Substring(2);
					break;
					
				case "B:" :
					
					//sPath = "C:" + path.Substring(2);
					
					err = "Content of virtual drive B: is not accessible through physical drive!";
					break;
					
				default :
					sPath = path;
					break;
					
			}
			
			RscStore_Storage strg = new RscStore_Storage();
			return strg.GetFileObject( sPath );
		}
		
		public static bool LaunchFile( string path, out string err )
		{
			err = "";
			
			Windows.Storage.StorageFile fle = RscStore_Storage.GetStorageFileObject( path, out err );
			if( fle == null )
				return false;
			
			bool bSucc = false;
			try
			{
				//NO!
				/*
				var options = new Windows.System.LauncherOptions();
      			options.DisplayApplicationPicker = false;
				*/
				
				var taskHlp = Task.Run(async () => { bSucc = await Windows.System.Launcher.LaunchFileAsync(fle /*, options*/); });
				taskHlp.Wait();
			}
			catch( Exception )
			{
			}
			
			return bSucc;
		}
		
		public static bool LaunchUri( Uri uri )
		{
			bool bSucc = false;
			try
			{
				//NO!
				/*
				var options = new Windows.System.LauncherOptions();
      			options.DisplayApplicationPicker = false;
				*/
				
				var taskHlp = Task.Run(async () => { bSucc = await Windows.System.Launcher.LaunchUriAsync(uri /*, options*/); });
				taskHlp.Wait();
			}
			catch( Exception )
			{
			}
			
			return bSucc;
		}
		
		// } Extras
		// //

	}
	
}