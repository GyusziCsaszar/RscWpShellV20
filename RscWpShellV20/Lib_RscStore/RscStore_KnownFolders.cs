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

using Windows.Storage.FileProperties;

namespace Ressive.Store
{
	
	public class RscStore_KnownFolders : RscStore_Storage
	{
		
		public RscStore_KnownFolders()
		{
		}
		
		// //
		//
		
		public override RscStore_Base GetSpecialStorage( string path )
		{
			string pathLo = path.ToLower();
			
			if( pathLo == "media library" || pathLo == "\\media library"
				|| pathLo.IndexOf( "media library\\" ) == 0 || pathLo.IndexOf( "\\media library\\" ) == 0 )
			{
				return new RscStore_MediaLibrary();
			}
			
			return this;
		}
		
		//
		// //
		//
		
		public override string [] ListRootFolders( bool bIncludeHidden, bool bStartWithInfChr )
		{
			string [] astr;
			if( bIncludeHidden )
				astr = new string [4];
			else
				astr = new string [2];
			
			int iIdx = 0;
			
			if( bIncludeHidden )
			{
				if( bStartWithInfChr ) astr[ iIdx ] = "r"; else astr[ iIdx ] = "";
				astr[ iIdx ] += "Installed Location"; iIdx++;
				
				if( bStartWithInfChr ) astr[ iIdx ] = "r"; else astr[ iIdx ] = "";
				astr[ iIdx ] += "Local Folder"; iIdx++;
				
				// NOT IMPLEMENTED //astr[ iIdx ] = "Roaming Folder"; iIdx++;
				// NOT IMPLEMENTED //astr[ iIdx ] = "Temporary Folder"; iIdx++;
			}
			
			if( bStartWithInfChr ) astr[ iIdx ] = "n"; else astr[ iIdx ] = "";
			astr[ iIdx ] += "Media Library"; iIdx++;
			
			//astr[ iIdx ] = KnownFolders.AppCaptures.Name; iIdx++;
			// EMPTY //astr[ iIdx ] = KnownFolders.CameraRoll.Name; iIdx++;
			// NOT IMPLEMENTED //astr[ iIdx ] = KnownFolders.DocumentsLibrary.Name; iIdx++;
			// NOT IMPLEMENTED //astr[ iIdx ] = KnownFolders.HomeGroup.Name; iIdx++;
			// NOT IMPLEMENTED //astr[ iIdx ] = KnownFolders.MediaServerDevices.Name; iIdx++;
			// NOT IMPLEMENTED //astr[ iIdx ] = KnownFolders.MusicLibrary.Name; iIdx++;
			//astr[ iIdx ] = KnownFolders.Objects3D.Name; iIdx++;
			
			if( bStartWithInfChr ) astr[ iIdx ] = "n"; else astr[ iIdx ] = "";
			astr[ iIdx ] += Windows.Storage.KnownFolders.PicturesLibrary.Name; iIdx++;
			
			//astr[ iIdx ] = KnownFolders.Playlists.Name; iIdx++;
			//astr[ iIdx ] = KnownFolders.RecordedCalls.Name; iIdx++;
			// NOT IMPLEMENTED //astr[ iIdx ] = KnownFolders.RemovableDevices.Name; iIdx++;
			// EMPTY //astr[ iIdx ] = KnownFolders.SavedPictures.Name; iIdx++;
			// NOT IMPLEMENTED //astr[ iIdx ] = KnownFolders.VideosLibrary.Name; iIdx++;
			
			return astr;
		}
		
		public override Windows.Storage.StorageFolder GetRootFolderByNameLower( string sNameLower )
		{
			if( sNameLower.CompareTo( "installed location" ) == 0 )
				return Windows.ApplicationModel.Package.Current.InstalledLocation;
			if( sNameLower.CompareTo( "local folder" ) == 0 )
				return Windows.Storage.ApplicationData.Current.LocalFolder;
			
			if( sNameLower.CompareTo( "media library" ) == 0 )
				return null;
			
			// NOT IMPLEMENTED //if( sNameLower.CompareTo( "roaming folder" ) == 0 )
			// NOT IMPLEMENTED //	return Windows.Storage.ApplicationData.Current.RoamingFolder;
			// NOT IMPLEMENTED //if( sNameLower.CompareTo( "temporary folder" ) == 0 )
			// NOT IMPLEMENTED //	return Windows.Storage.ApplicationData.Current.TemporaryFolder;
			//if( sNameLower.CompareTo( KnownFolders.AppCaptures.Name.ToLower() ) == 0 )
			//	return KnownFolders.AppCaptures;
			// EMPTY //if( sNameLower.CompareTo( KnownFolders.CameraRoll.Name.ToLower() ) == 0 )
			// EMPTY //	return KnownFolders.CameraRoll;
			// NOT IMPLEMENTED //if( sNameLower.CompareTo( KnownFolders.DocumentsLibrary.Name.ToLower() ) == 0 )
			// NOT IMPLEMENTED //	return KnownFolders.DocumentsLibrary;
			// NOT IMPLEMENTED //if( sNameLower.CompareTo( KnownFolders.HomeGroup.Name.ToLower() ) == 0 )
			// NOT IMPLEMENTED //	return KnownFolders.HomeGroup;
			// NOT IMPLEMENTED //if( sNameLower.CompareTo( KnownFolders.MediaServerDevices.Name.ToLower() ) == 0 )
			// NOT IMPLEMENTED //	return KnownFolders.MediaServerDevices;
			// NOT IMPLEMENTED //if( sNameLower.CompareTo( KnownFolders.MusicLibrary.Name.ToLower() ) == 0 )
			// NOT IMPLEMENTED //	return KnownFolders.MusicLibrary;
			//if( sNameLower.CompareTo( KnownFolders.Objects3D.Name.ToLower() ) == 0 )
			//	return KnownFolders.Objects3D;
			if( sNameLower.CompareTo( Windows.Storage.KnownFolders.PicturesLibrary.Name.ToLower() ) == 0 )
				return Windows.Storage.KnownFolders.PicturesLibrary;
			//if( sNameLower.CompareTo( KnownFolders.Playlists.Name.ToLower() ) == 0 )
			//	return KnownFolders.Playlists;
			//if( sNameLower.CompareTo( KnownFolders.RecordedCalls.Name.ToLower() ) == 0 )
			//	return KnownFolders.RecordedCalls;
			// NOT IMPLEMENTED //if( sNameLower.CompareTo( KnownFolders.RemovableDevices.Name.ToLower() ) == 0 )
			// NOT IMPLEMENTED //	return KnownFolders.RemovableDevices;
			// EMPTY //if( sNameLower.CompareTo( KnownFolders.SavedPictures.Name.ToLower() ) == 0 )
			// EMPTY //	return KnownFolders.SavedPictures;
			// NOT IMPLEMENTED //if( sNameLower.CompareTo( KnownFolders.VideosLibrary.Name.ToLower() ) == 0 )
			// NOT IMPLEMENTED //	return KnownFolders.VideosLibrary;
			
			return null;
		}
		
		//
		// //
		
	}
	
}