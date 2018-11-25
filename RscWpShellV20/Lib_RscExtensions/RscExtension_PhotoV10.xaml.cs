using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;

using Ressive.Utils;
using Ressive.Store;
using Ressive.Theme;
using Ressive.FrameWork;

namespace Lib_RscExtensions
{
	
    public partial class RscExtension_PhotoV10 : PhoneApplicationPage
    {
		
		RscTheme m_Theme = null;
		//ImageSource m_isDummy = null;
		
		TextBoxDenieEdit m_txtName;
		TextBoxDenieEdit m_txtInfo;
		RscIconButton m_btnSave;
		
		MediaLibrary m_media = null;
		
		Picture m_pic = null;
		
		string m_sGUID = "";
		string m_sPicFileName = "";
		string m_sPhoneFolder = "";
		string m_sAppFolder = "";
		string m_sAppPath = "";
		string m_sInf = "";
		
        public RscExtension_PhotoV10()
        {
            InitializeComponent();
			
			//MemUsage Optimization...
			Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			m_Theme = (RscTheme) GlobalDILholder.Tag;
			//m_dil = new RscDefaultedImageList( "Theme", "Current", "Default" );
			// ///////////////
			//m_isDummy = m_dil.GetImage("Images/Img001_Dummy.jpg");
			
			m_txtName = new TextBoxDenieEdit(true, true, ToolBar, Grid.ColumnProperty, 0);
			m_txtName.Background = new SolidColorBrush(Colors.Black);
			m_txtName.Foreground = new SolidColorBrush(Colors.White);
			m_txtName.FontSize = 18;
			m_txtName.Text = "          ";
			
			m_txtInfo = new TextBoxDenieEdit(true, true, ToolBar, Grid.ColumnProperty, 1);
			m_txtInfo.Background = new SolidColorBrush(Colors.Black);
			m_txtInfo.Foreground = new SolidColorBrush(Colors.White);
			m_txtInfo.FontSize = 18;
			m_txtInfo.Text = "loading...\r\n\r\n\r\n";
			
			m_btnSave = new RscIconButton(ButtonBar, Grid.RowProperty, 1, 100, 100, Rsc.Collapsed);
			m_btnSave.Image.Source = m_Theme.GetImage("Images/Btn001_Save.jpg");
			m_btnSave.Click += new System.Windows.RoutedEventHandler(m_btnSave_Click);
        }
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			//Handle ExitOnBack=True arg...
			RscUtils.OnNavigatedTo_ExitOnBack(this.NavigationContext.QueryString);
			
			if( m_media != null ) return;
			
			m_media = new MediaLibrary();
			m_pic = null;
			
			IDictionary<string, string> parameters = this.NavigationContext.QueryString;
			if( parameters.ContainsKey( "token" ) )
			{
				m_sGUID = parameters["token"];
			}
			else if( parameters.ContainsKey( "FileId" ) )
			{
				m_sGUID = parameters["FileId"];
			}
			else
			{
				return;
			}
			
			m_pic = m_media.GetPictureFromToken( m_sGUID );
			
			if( m_pic != null )
			{
			
				string sFName = m_pic.Name;
				string sFExt = "";
				int iPos = sFName.LastIndexOf('.');
				if( iPos >= 0 )
				{
					sFExt = sFName.Substring( iPos );
					sFName = sFName.Substring( 0, iPos );
				}
			
				m_sPicFileName = m_sGUID + sFExt;
				
				m_sPhoneFolder = "";
				PictureAlbum pa = m_pic.Album;
				for(;;)
				{
					if( pa.Name.ToString().Length == 0 ) break;
					
					if( m_sPhoneFolder.Length > 0 ) m_sPhoneFolder = "\\" + m_sPhoneFolder;
					m_sPhoneFolder = pa.Name + m_sPhoneFolder;
					
					pa = pa.Parent;
					if( pa == null ) break;
				}
				
				m_sAppFolder = RscKnownFolders.GetMediaPath("Zune");
				if( m_sPhoneFolder.Length > 0 )
					m_sAppFolder += "\\" + m_sPhoneFolder;
				
				m_sAppPath = m_sAppFolder + "\\" + m_sPicFileName;
				
				string sNames = "";
				sNames += "Name";
				sNames += "\r\n";
				sNames += "Time";
				sNames += "\r\n";
				sNames += "Size";
				sNames += "\r\n";
				sNames += "Folder";	
				m_txtName.Text = sNames;
				
				string sInf = "";
				sInf += m_pic.Name;
				sInf += "\r\n";
				sInf += m_pic.Date.ToShortDateString() + " " + m_pic.Date.ToShortTimeString();
				sInf += "\r\n";
				sInf += m_pic.Width.ToString() + " x " + m_pic.Height.ToString();
				sInf += "\r\n";
				sInf += m_sPhoneFolder;
				m_txtInfo.Text = sInf;
				
				m_sInf = "";
				m_sInf += m_pic.Name;
				m_sInf += "\r\n";
				m_sInf += m_sGUID;
				m_sInf += "\r\n";
				m_sInf += m_pic.Date.ToShortDateString() + " | " + m_pic.Date.ToShortTimeString();
				m_sInf += "\r\n";
				m_sInf += m_pic.Width.ToString() + " x " + m_pic.Height.ToString();
				m_sInf += "\r\n";
				m_sInf += m_sPhoneFolder;
				
				AddSavedNote();
			
				/*
				if( bThumbnail )
				{
					System.Windows.Media.Imaging.WriteableBitmap wbmp = 
						Microsoft.Phone.PictureDecoder.DecodeJpeg(pic.GetThumbnail(), iCX, iCY );
				
					return wbmp;
				}
				else
				{
				*/
					BitmapImage bmp = new BitmapImage();
					bmp.SetSource(m_pic.GetImage());
				/*
					return bmp;
				}
				*/
				
				imgFull.Source = bmp;
			}
			/*
			else
			{
			
				imgFull.Source = m_isDummy;
				
			}
			*/
		}
		
		private void AddSavedNote()
		{
			RscStore store = new RscStore();
			
			if( store.FileExists( m_sAppPath ) )
			{
				string sInf = "";
				sInf += "\r\n";
				sInf += "\r\n";
				sInf += "Picture stored in app storage:\r\n" + m_sAppPath;
				m_txtInfo.Text += sInf;
				
				m_btnSave.Visibility = Rsc.Collapsed;
			}
			else
			{
				m_btnSave.Visibility = Rsc.Visible;
			}
		}

		private void m_btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			RscStore store = new RscStore();
			store.CreateFolderPath( m_sAppFolder );
			
			SaveImage( true );
			SaveImage( false );
			
			AddSavedNote();
		}
		
		private void SaveImage( bool bThumbnail )
		{
			RscStore store = new RscStore();
			
			string sFolder = m_sAppFolder;
			
			if( bThumbnail )
			{
				sFolder += "\\" + "tn";
				if( !store.FolderExists( sFolder ) )
					store.CreateFolder( sFolder );
			}
			
			string sPreExt = "";
			if( bThumbnail )
				sPreExt = ".tn";
			
			string sFName = m_sPicFileName;
			string sFExt = "";
			int iPos = sFName.LastIndexOf('.');
			if( iPos >= 0 )
			{
				sFExt = sFName.Substring( iPos );
				sFName = sFName.Substring( 0, iPos );
			}
			
			string sPath = sFolder + "\\" + sFName + sPreExt + sFExt;
			/*
			iCnt = 0;
			for(;;)
			{
				if( !store.FileExists(sPath) ) break;
				iCnt++;
				sPath = sFolder + "\\" + sFName + "_" + iCnt.ToString() + sPreExt + sFExt;
			}
			*/
			
			System.IO.Stream strmSrc;
			
			if( bThumbnail )
				strmSrc = m_pic.GetThumbnail();
			else
				strmSrc = m_pic.GetImage();
			
			if( store.FileExists( sPath ) ) store.DeleteFile( sPath );
			
			System.IO.Stream stream = store.CreateFile(sPath);
            // Initialize the buffer for 4KB disk pages.
            byte[] readBuffer = new byte[4096];
            int bytesRead = -1;
            // Copy the image/thumbnail to the local folder. 
            while ((bytesRead = strmSrc.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                stream.Write(readBuffer, 0, bytesRead);
            }
			stream.Close();
			
			strmSrc.Close();
			
			if( !bThumbnail )
			{
				store.WriteTextFile( sFolder + "\\" + sFName + sPreExt + sFExt + ".inf", m_sInf, true );
			}
		}
		
    }
	
}