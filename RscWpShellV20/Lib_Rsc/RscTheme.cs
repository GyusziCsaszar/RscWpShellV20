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

using System.Xml.Linq;

using Ressive.Utils;
using Ressive.Store;

namespace Ressive.Theme
{
	
	public class RscThemeColors
	{
		
		const string csFn = "ThemeColors.xml";
		
		public Color ListZebraBack1 = Color.FromArgb( 255, 195, 195, 195 );
		public Color ListZebraBack2 = Color.FromArgb( 255, 145, 145, 145 );
		public Color ListZebraFore = Color.FromArgb( 255, 255, 255, 255 );
		
		public Color TreeDescBack = Color.FromArgb( 255, 236, 244, 178 );
		public Color TreeDescFore = Color.FromArgb( 255, 0, 0, 0 );
		public Color TreeLeafBack = Color.FromArgb( 255, 195, 195, 195 );
		public Color TreeLeafFore = Color.FromArgb( 255, 255, 255, 255 );
		public Color TreeContainerBack = Color.FromArgb( 255, 98, 98, 98 );
		public Color TreeContainerFore = Color.FromArgb( 255, 255, 255, 255 );
		
		public Color TextDarkBack = Color.FromArgb( 255, 31, 31, 31 );
		public Color TextDarkFore = Color.FromArgb( 255, 236, 244, 178 );
		public Color TextLightBack = Color.FromArgb( 255, 236, 244, 178 );
		public Color TextLightFore = Color.FromArgb( 255, 31, 31, 31 );
		
		public Color GalleryItemBack = Color.FromArgb( 255, 127, 127, 127 );
		public Color GalleryItemBorder = Color.FromArgb( 255, 31, 31, 31 );
		public Color GalleryItemFore = Color.FromArgb( 255, 255, 255, 255 );
		
		public Color DialogLightBack = Color.FromArgb( 255, 195, 195, 195 );
		
		public Color ToolBarLightBack = Color.FromArgb( 255, 195, 195, 195 );
		public Color ToolBarLightFore = Color.FromArgb( 255, 0, 0, 0 );
		
		public Color AppTitleBack = Color.FromArgb( 255, 127, 127, 127 );
		public Color AppTitleFore = Color.FromArgb( 255, 255, 255, 255 );
		
		public Color AppStatusBack = Color.FromArgb( 255, 127, 127, 127 );
		public Color AppStatusFore = Color.FromArgb( 255, 0, 0, 0 );
		
		public Color IconBack = Color.FromArgb( 255, 31, 31, 31 );
		public Color IconBorder = Color.FromArgb( 255, 127, 127, 127 );
		public Color IconFore = Color.FromArgb( 255, 195, 195, 195 );
		
		public Color ThemeBack = Color.FromArgb( 255, 31, 31, 31 );
		
		public RscThemeColors()
		{
		}
		
		public void Load( string strSysFolder, string strSysSubFolder1, string strSysSubFolder2 = "" )
		{
			
			string sSysFle1 = RscKnownFolders.GetSystemPath( strSysFolder, strSysSubFolder1 ) + "\\" + csFn;
			string sSysFle2 = "";
			if( strSysSubFolder2.Length > 0 ) sSysFle2 = RscKnownFolders.GetSystemPath( strSysFolder, strSysSubFolder2 ) + "\\" + csFn;
			
			RscStore store = new RscStore();
			
			if( !store.FileExists(sSysFle1) )
			{
				//Save current xml...
				SaveToXml( sSysFle1 );
				
				if( sSysFle2.Length > 0 )
				{
					if( !store.FileExists( sSysFle2 ) )
					{
						//Save default xml...
						SaveToXml( sSysFle2 );
					}
				}
			}
			else
			{
				LoadFromXml( sSysFle1 );
			}
		}
		
		protected void LoadFromXml( string path )
		{
			RscStore store = new RscStore();
			System.IO.Stream strm = store.GetReaderStream( path );
			
			XDocument xDoc = XDocument.Load( strm, LoadOptions.None );
			
			strm.Close();
			
			try
			{
				XElement xRoot = xDoc.Element( "ThemeColors" );
				
				ListZebraBack1 = RscDecode.HexaStringToColor( xRoot.Element( "ListZebraBack1" ).Value );
				ListZebraBack2 = RscDecode.HexaStringToColor( xRoot.Element( "ListZebraBack2" ).Value );
				ListZebraFore = RscDecode.HexaStringToColor( xRoot.Element( "ListZebraFore" ).Value );
				
				TreeDescBack = RscDecode.HexaStringToColor( xRoot.Element( "TreeDescBack" ).Value );
				TreeDescFore = RscDecode.HexaStringToColor( xRoot.Element( "TreeDescFore" ).Value );
				TreeLeafBack = RscDecode.HexaStringToColor( xRoot.Element( "TreeLeafBack" ).Value );
				TreeLeafFore = RscDecode.HexaStringToColor( xRoot.Element( "TreeLeafFore" ).Value );
				TreeContainerBack = RscDecode.HexaStringToColor( xRoot.Element( "TreeContainerBack" ).Value );
				TreeContainerFore = RscDecode.HexaStringToColor( xRoot.Element( "TreeContainerFore" ).Value );
				
				TextDarkBack = RscDecode.HexaStringToColor( xRoot.Element( "TextDarkBack" ).Value );
				TextDarkFore = RscDecode.HexaStringToColor( xRoot.Element( "TextDarkFore" ).Value );
				TextLightBack = RscDecode.HexaStringToColor( xRoot.Element( "TextLightBack" ).Value );
				TextLightFore = RscDecode.HexaStringToColor( xRoot.Element( "TextLightFore" ).Value );
				
				GalleryItemBack = RscDecode.HexaStringToColor( xRoot.Element( "GalleryItemBack" ).Value );
				GalleryItemBorder = RscDecode.HexaStringToColor( xRoot.Element( "GalleryItemBorder" ).Value );
				GalleryItemFore = RscDecode.HexaStringToColor( xRoot.Element( "GalleryItemFore" ).Value );
				
				DialogLightBack = RscDecode.HexaStringToColor( xRoot.Element( "DialogLightBack" ).Value );
				
				ToolBarLightBack = RscDecode.HexaStringToColor( xRoot.Element( "ToolBarLightBack" ).Value );
				ToolBarLightFore = RscDecode.HexaStringToColor( xRoot.Element( "ToolBarLightFore" ).Value );
				
				AppTitleBack = RscDecode.HexaStringToColor( xRoot.Element( "AppTitleBack" ).Value );
				AppTitleFore = RscDecode.HexaStringToColor( xRoot.Element( "AppTitleFore" ).Value );
				
				AppStatusBack = RscDecode.HexaStringToColor( xRoot.Element( "AppStatusBack" ).Value );
				AppStatusFore = RscDecode.HexaStringToColor( xRoot.Element( "AppStatusFore" ).Value );
				
				IconBack = RscDecode.HexaStringToColor( xRoot.Element( "IconBack" ).Value );
				IconBorder = RscDecode.HexaStringToColor( xRoot.Element( "IconBorder" ).Value );
				IconFore = RscDecode.HexaStringToColor( xRoot.Element( "IconFore" ).Value );
				
				ThemeBack = RscDecode.HexaStringToColor( xRoot.Element( "ThemeBack" ).Value );				
			}
			catch( Exception ex )
			{
				MessageBox.Show( ex.Message );
			}
		}
		
		protected void SaveToXml( string path )
		{
			XDocument xDoc = new XDocument();
			
			XElement xRoot = new XElement("ThemeColors");
			
			// //
			//
			
			xRoot.Add( new XElement( "ListZebraBack1", ListZebraBack1 ) );
			xRoot.Add( new XElement( "ListZebraBack2", ListZebraBack2 ) );
			xRoot.Add( new XElement( "ListZebraFore", ListZebraFore ) );
			
			xRoot.Add( new XElement( "TreeDescBack", TreeDescBack ) );
			xRoot.Add( new XElement( "TreeDescFore", TreeDescFore ) );
			xRoot.Add( new XElement( "TreeLeafBack", TreeLeafBack ) );
			xRoot.Add( new XElement( "TreeLeafFore", TreeLeafFore ) );
			xRoot.Add( new XElement( "TreeContainerBack", TreeContainerBack ) );
			xRoot.Add( new XElement( "TreeContainerFore", TreeContainerFore ) );
			
			xRoot.Add( new XElement( "TextDarkBack", TextDarkBack ) );
			xRoot.Add( new XElement( "TextDarkFore", TextDarkFore ) );
			xRoot.Add( new XElement( "TextLightBack", TextLightBack ) );
			xRoot.Add( new XElement( "TextLightFore", TextLightFore ) );
			
			xRoot.Add( new XElement( "GalleryItemBack", GalleryItemBack ) );
			xRoot.Add( new XElement( "GalleryItemBorder", GalleryItemBorder ) );
			xRoot.Add( new XElement( "GalleryItemFore", GalleryItemFore ) );
			
			xRoot.Add( new XElement( "DialogLightBack", DialogLightBack ) );
			
			xRoot.Add( new XElement( "ToolBarLightBack", ToolBarLightBack ) );
			xRoot.Add( new XElement( "ToolBarLightFore", ToolBarLightFore ) );
			
			xRoot.Add( new XElement( "AppTitleBack", AppTitleBack ) );
			xRoot.Add( new XElement( "AppTitleFore", AppTitleFore ) );
			
			xRoot.Add( new XElement( "AppStatusBack", AppStatusBack ) );
			xRoot.Add( new XElement( "AppStatusFore", AppStatusFore ) );
			
			xRoot.Add( new XElement( "IconBack", IconBack ) );
			xRoot.Add( new XElement( "IconBorder", IconBorder ) );
			xRoot.Add( new XElement( "IconFore", IconFore ) );
			
			xRoot.Add( new XElement( "ThemeBack", ThemeBack ) );
			
			//
			// //
			//
			
			xDoc.Add(xRoot);
			
			RscStore store = new RscStore();
			System.IO.Stream strm = store.CreateFile( path );
			
			xDoc.Save( strm );
			
			strm.Close();
			
			//
			// //
		}
		
	}
	
	public class RscTheme
	{
		
		const string csAssyName = "/Lib_Rsc;component/";

		class RscDefaultedImage
		{
			
			public string Name = "";
		
			string m_sPathToSave1 = "";
			string m_sPathToSave2 = "";
			
			public ImageSource ims;
			
			public RscDefaultedImage( string sName, ImageSource imsIn )
			{
				Name = sName;
				
				m_sPathToSave1 = "";
				m_sPathToSave2 = "";
					
				ims = imsIn;
			}
			
			public RscDefaultedImage( string sName, Uri uri, string sPathToSave1 = "", string sPathToSave2 = "" )
			{
				Name = sName;
				
				m_sPathToSave1 = sPathToSave1;
				m_sPathToSave2 = sPathToSave2;
					
				BitmapImage bmp = new BitmapImage();
				
				ims = bmp;
				
				if( m_sPathToSave1.Length > 0 || m_sPathToSave2.Length > 0 )
				{
					bmp.CreateOptions = BitmapCreateOptions.None;
					bmp.ImageOpened += BitmapImage_OpenedTrue;
				}
				
				bmp.UriSource = uri;
			}
		
			void BitmapImage_OpenedTrue(object sender, RoutedEventArgs e)
			{
				BitmapImage bImage = (BitmapImage) sender;
			
				WriteableBitmap wbmp= new WriteableBitmap(bImage);
				
				RscStore store = new RscStore();
			
				if( m_sPathToSave1.Length > 0 )
				{
					System.IO.Stream stream = store.CreateFile(m_sPathToSave1);
				
					System.Windows.Media.Imaging.
					Extensions.SaveJpeg(wbmp, stream,
					wbmp.PixelWidth, wbmp.PixelHeight,
					0, 100);
				
					stream.Close();		
				}
			
				if( m_sPathToSave2.Length > 0 )
				{
					System.IO.Stream stream = store.CreateFile(m_sPathToSave2);
				
					System.Windows.Media.Imaging.
					Extensions.SaveJpeg(wbmp, stream,
					wbmp.PixelWidth, wbmp.PixelHeight,
					0, 100);
				
					stream.Close();		
				}
			}
			
		}
		
		string m_strSysFolder;
		string m_strSysSubFolder1;
		string m_strSysSubFolder2;
		
		List<RscDefaultedImage> m_a = new List<RscDefaultedImage>();
		
		RscThemeColors m_ThemeColors = new RscThemeColors();
		
		public RscTheme(bool bReserved, string strSysFolder, string strSysSubFolder1, string strSysSubFolder2 = "")
		{
			m_strSysFolder = strSysFolder;
			m_strSysSubFolder1 = strSysSubFolder1;
			m_strSysSubFolder2 = strSysSubFolder2;
			
			m_ThemeColors.Load( m_strSysFolder, m_strSysSubFolder1, m_strSysSubFolder2 );
		}
			
		public RscThemeColors ThemeColors { get{ return m_ThemeColors; } }
		
		public ImageSource GetImage( string strResName )
		{
			
			strResName = csAssyName + strResName;
			
			string sFn;
			int iPos = strResName.LastIndexOf('/');
			if( iPos >= 0 )
				sFn = strResName.Substring(iPos + 1);
			else
				sFn = strResName;
			iPos = sFn.LastIndexOf('.');
			if( iPos >= 0 ) sFn = sFn.Substring(0, iPos );
			
			RscDefaultedImage rdi = _GetByName( sFn );
			if( rdi != null ) return rdi.ims;
			
			string strPath1 = "";
			string strPath2 = "";
			ImageSource ims = _ChkFile( sFn, out strPath1, out strPath2 );
			if( ims != null )
				rdi = new RscDefaultedImage( sFn, ims );
			else
				rdi = new RscDefaultedImage( sFn, new Uri(strResName, UriKind.Relative), strPath1, strPath2 );
			
			//TODO: BinarySearch...
			m_a.Add( rdi );
			return rdi.ims;
		}
		
		RscDefaultedImage _GetByName( string sName )
		{
			//TODO: BinarySearch...
			foreach( RscDefaultedImage rdi in m_a )
			{
				if( rdi.Name == sName ) return rdi;
			}		
			return null;
		}
		
		ImageSource _ChkFile( string strName, out string strPath1, out string strPath2 )
		{
			strPath1 = "";
			strPath2 = "";
			
			string sSysFldr1 = RscKnownFolders.GetSystemPath( m_strSysFolder, m_strSysSubFolder1 ) + "\\" + strName;
			string sSysFldr2 = "";
			if( m_strSysSubFolder2.Length > 0 ) sSysFldr2 = RscKnownFolders.GetSystemPath( m_strSysFolder, m_strSysSubFolder2 ) + "\\" + strName;
			
			RscStore store = new RscStore();
			
			string strExt = "";
			if( store.FileExists(sSysFldr1 + ".png") ) strExt = ".png";
			if( strExt.Length == 0 && store.FileExists(sSysFldr1 + ".jpg") ) strExt = ".jpg";
			
			if( strExt.Length > 0 )
			{
				try
				{
					//FIX: Keeps file locked!!!
					string strAccDeniPath = RscKnownFolders.GetTempPath( "DefaultedImageList", m_strSysFolder ) + "\\" + strName + strExt;
					store.CopyFile(sSysFldr1 + strExt, strAccDeniPath, true);
					
					System.IO.Stream stream = store.GetReaderStream(strAccDeniPath);
					
					//BUG: Not working...
					{
						BitmapImage bmp = new BitmapImage();
						bmp.SetSource(stream);
						
						stream.Close();
						
						return bmp;
					}
					
					//BUG: Not working...
					/*
					BitmapImage bmp = new BitmapImage(new Uri(strResName, UriKind.Relative));
					
					WriteableBitmap wbmp= new WriteableBitmap(bmp.PixelWidth, bmp.PixelHeight);
					
					System.Windows.Media.Imaging.
						Extensions.LoadJpeg(wbmp, stream);
					
					stream.Close();
					
					return wbmp;
					*/
				}
				catch( Exception )
				{
					//NOP...
				}
			}
			
			strPath1 = sSysFldr1 + ".jpg";
			if( sSysFldr2.Length > 0 ) strPath2 = sSysFldr2 + ".jpg";
			
			return null;
		}
		
	}
	
}