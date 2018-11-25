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
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Xna.Framework.Media;

using Ressive.Utils;
using Ressive.Store;
using Ressive.FrameWork;

namespace RscXtests
{
	
    public partial class RscMedLibPicturesV10 : PhoneApplicationPage
    {
		
		RscAppFrame m_AppFrame;
		
		class RscMediaLibItemDesc
		{
			
			public bool bFolder = false;
			
			public string sIndiciesToItem = "";
			
			public string sFolderPath = "N/A";
			public string sName = "N/A";
			
			public string sDetails = "<none>";
			
			public RscMediaLibItemDesc()
			{
			}
			
		}
		
		RscIconButton m_btnContinue;
		TextBoxDenieEdit m_txtTitle;
		RscIconButton m_btnThumbnail;
		RscIconButton m_btnImage;
		
		class StackItem
		{
			
			public PictureAlbum pa;
			public int Index;
			
			public StackItem(PictureAlbum oPa, int iIndex)
			{
				pa = oPa;
				Index = iIndex;
			}
		}
		
		MediaLibrary m_media = null;
		List<StackItem> m_stck = null;
		PictureAlbum m_pa = null;
		int m_iIdx = -1;
		PictureAlbum m_paPic = null;
		int m_iIdxPic = -1;
		
        public RscMedLibPicturesV10()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Media Library - Pictures 1.0", "Images/Ico001_Ressive.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			
			m_btnContinue = new RscIconButton(TitlePanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Collapsed);
			m_btnContinue.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Continue.jpg");
			m_btnContinue.Click += new System.Windows.RoutedEventHandler(m_btnContinue_Click);
			
			m_txtTitle = new TextBoxDenieEdit(true, true, TitlePanel, Grid.ColumnProperty, 1);
			m_txtTitle.Background = new SolidColorBrush(Colors.LightGray);
			m_txtTitle.Foreground = new SolidColorBrush(Colors.Black);
			m_txtTitle.FontSize = 16;
			m_txtTitle.Text = "TODO: press button Next to list." + "\r\n"
				+ "NOTE #1: clk = thumbnail, dblclk = image" + "\r\n"
				+ "NOTE #2: thumb_clk = save_thumb, img_clk = save_img";
			
			m_btnThumbnail = new RscIconButton(TitlePanel, Grid.ColumnProperty, 2, 48, 80, Rsc.Collapsed);
			//m_btnThumbnail.Image.Source = m_AppFrame.DefaultedImageList.GetImage("Images/Btn001_Continue.jpg");
			m_btnThumbnail.Click += new System.Windows.RoutedEventHandler(m_btnThumbnail_Click);
			
			m_btnImage = new RscIconButton(TitlePanel, Grid.ColumnProperty, 3, 96, 160, Rsc.Collapsed);
			//m_btnImage.Image.Source = m_AppFrame.DefaultedImageList.GetImage("Images/Btn001_Continue.jpg");
			m_btnImage.Click += new System.Windows.RoutedEventHandler(m_btnImage_Click);
			
			this.Unloaded += new System.Windows.RoutedEventHandler(RscFtpDownLoadV10_Unloaded);
       }
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			m_AppFrame.SetNoSleep( true );
		}

		private void RscFtpDownLoadV10_Unloaded(object sender, System.Windows.RoutedEventArgs e)
		{
			m_AppFrame.SetNoSleep( false );
		}
		
		private void m_AppFrame_OnExit(object sender, EventArgs e)
		{
			this.NavigationService.GoBack();
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( m_AppFrame.CancelTimer() )
				e.Cancel = true;
			
			//e.Cancel = true;
        }
		
		private void m_btnContinue_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			spFiles.Children.Clear();
			
			//MUST NOT!!! To continue...
			//ReSet();
			
			m_AppFrame.StartTimer( "load", LayoutRoot, 1, 0, true );
		}
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			spFiles.Children.Clear();
			
			ReSet();
			
			m_AppFrame.StartTimer( "load", LayoutRoot, 1, 0, true );
		}
		
		private void ReSet()
		{
			m_media = new MediaLibrary();
			m_stck = new List<StackItem>();
			m_pa = m_media.RootPictureAlbum;
			m_iIdx = -1;
			m_paPic = null;
			m_iIdxPic = -1;
		}
		
		private void m_AppFrame_OnTimer(object sender, RscAppFrameTimerEventArgs e)
		{
			switch( e.Reason )
			{
				
				case "load_Cancel" :
				{
					m_AppFrame.SetStatusText( "CANCELED by user!" );
					
					m_btnContinue.Visibility = Rsc.Visible;

					break;
				}
				
				case "load" :
				{
					bool bPicturesJustDone = false;
					if( m_paPic != null )
					{						
						m_iIdxPic++;
						if( m_iIdxPic >= m_paPic.Pictures.Count )
						{
							m_paPic = null;
							m_iIdxPic = 0;
							
							bPicturesJustDone = true;
						}
						else
						{
							e.Max++;
							ReportAlbumPictures( m_paPic, m_iIdxPic );
							m_AppFrame.SetStatusText( "Loaded: " + (e.Max + 1).ToString());
						}
					}
					
					if( m_paPic == null )
					{
						m_iIdx++;
						e.Max++;
						
						if( m_iIdx == 0 )
						{
							ReportAlbum( m_pa );
							m_AppFrame.SetStatusText( "Loaded: " + (e.Max + 1).ToString());
						}
						
						if( m_iIdx >= m_pa.Albums.Count )
						{
							if( (!bPicturesJustDone) && (m_pa.Pictures.Count > 0) )
							{
								m_paPic = m_pa;
								m_iIdxPic = 0;
								
								ReportAlbumPictures( m_paPic, m_iIdxPic );											
								m_AppFrame.SetStatusText( "Loaded: " + (e.Max + 1).ToString());
								
								return;
							}
						}
						
						if( m_iIdx >= m_pa.Albums.Count )
						{
							if( m_stck.Count > 0 )
							{
								m_pa = m_stck[ m_stck.Count - 1 ].pa;
								m_iIdx = m_stck[ m_stck.Count - 1 ].Index;
								
								m_stck.RemoveAt( m_stck.Count - 1 );
								
								return;
							}
							
							e.Completed = true;
							
							m_AppFrame.SetStatusText( "Load done!" );
							
							ReSet();
							m_btnContinue.Visibility = Rsc.Collapsed;
							
							return;
						}
						
						m_stck.Add( new StackItem( m_pa, m_iIdx ) );
						m_pa = m_pa.Albums[ m_iIdx ];
						m_iIdx = -1;
					}
				
					break;
				}
				
			}
		}
		
		private void ReportAlbum( PictureAlbum pa )
		{
			string sIndiciesToItem = "";
			string sFolderPath = "";
			foreach( StackItem si in m_stck )
			{
				if( sIndiciesToItem.Length > 0 ) sIndiciesToItem += "|";
				sIndiciesToItem += si.Index.ToString();
				
				if( sFolderPath.Length > 0 ) sFolderPath += "\\";
				sFolderPath += si.pa.Name;
			}
			/*
			if( sFolderPath.Length > 0 ) sFolderPath += "\\";
			sFolderPath += pa.Name;
			*/
			
			RscMediaLibItemDesc it = new RscMediaLibItemDesc();
			
			it.bFolder = true;
			
			it.sIndiciesToItem = sIndiciesToItem;
			it.sFolderPath = sFolderPath;
			
			it.sName = pa.Name;
			
			it.sDetails = "";
			it.sDetails += sFolderPath;
			it.sDetails += "\r\n";
			it.sDetails += sIndiciesToItem;
			it.sDetails += "\r\n";
			it.sDetails += "Album count: " + pa.Albums.Count.ToString();
			it.sDetails += "\r\n";
			it.sDetails += "Picture count: " + pa.Pictures.Count.ToString();
			
			AddFile(it);
		}
		
		private void ReportAlbumPictures( PictureAlbum pa, int iPic )
		{	
			string sIndiciesToItem = "";
			string sFolderPath = "";
			foreach( StackItem si in m_stck )
			{
				if( sIndiciesToItem.Length > 0 ) sIndiciesToItem += "|";
				sIndiciesToItem += si.Index.ToString();
				
				if( sFolderPath.Length > 0 ) sFolderPath += "\\";
				sFolderPath += si.pa.Name;
			}
			/*
			if( sIndiciesToItem.Length > 0 ) sIndiciesToItem += "|";
			sIndiciesToItem += m_iIdx.ToString();
			*/
			if( sIndiciesToItem.Length > 0 ) sIndiciesToItem += "|";
			sIndiciesToItem += iPic.ToString();
			if( sFolderPath.Length > 0 ) sFolderPath += "\\";
			sFolderPath += pa.Name;

			Picture pic = pa.Pictures[ iPic ];
			
			RscMediaLibItemDesc it = new RscMediaLibItemDesc();
			
			it.bFolder = false;
			
			it.sIndiciesToItem = sIndiciesToItem;
			it.sFolderPath = sFolderPath;
			
			it.sName = pic.Name;
			
			it.sDetails = "";
			it.sDetails += sFolderPath;
			it.sDetails += "\r\n";
			it.sDetails += sIndiciesToItem;
			it.sDetails += "\r\n";
			it.sDetails += "DateTime: " + pic.Date.ToShortDateString() + " " + pic.Date.ToShortTimeString();
			it.sDetails += "\r\n";
			it.sDetails += "Dimenstion: " + pic.Width.ToString() + " x " + pic.Height.ToString();
			
			AddFile(it);
		}
		
		private void AddFile(RscMediaLibItemDesc it)
		{
			
			int idx = spFiles.Children.Count + 1;
			
			Grid grdOut = new Grid();
			grdOut.Name = "grdOut_" + idx.ToString();
			grdOut.Margin = new Thickness(0, 0, 0, 4 );
			RowDefinition rd;
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdOut.RowDefinitions.Add(rd);
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdOut.RowDefinitions.Add(rd);
			spFiles.Children.Add(grdOut);
			
			Rectangle rc;
			rc = new Rectangle();
			if( it.bFolder )
			{
				rc.Fill = new SolidColorBrush(Colors.Orange);
			}
			else
			{
				rc.Fill = new SolidColorBrush(Colors.Blue);
			}
			rc.Opacity = 0.5;
			rc.SetValue(Grid.RowProperty, 0);
			grdOut.Children.Add(rc);
	
			Button btnMore = new Button();
			btnMore.Name = "btnOpen_" + idx.ToString();
			btnMore.Content = it.sName;
			btnMore.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
			btnMore.BorderThickness = new Thickness(0);
			btnMore.FontSize = 20;
			btnMore.Foreground = new SolidColorBrush(Colors.White); //.Blue);
			btnMore.Margin = new Thickness(-12,-10,-12,-12);
			//btnMore.Tag = it;
			//btnMore.Opacity = 0.5;
			btnMore.SetValue(Grid.RowProperty, 0);
			grdOut.Children.Add(btnMore);
			
			Grid grdTit = new Grid();
			grdTit.Name = "grdTit_" + idx.ToString();
			grdTit.Margin = new Thickness(12, 0, 0, 0);
			//RowDefinition rd;
			rd = new RowDefinition(); grdTit.RowDefinitions.Add(rd);
			grdTit.SetValue(Grid.RowProperty, 1);
			grdOut.Children.Add(grdTit);
			
			TextBox tbDetails = new TextBox();
			tbDetails.Name = "tbDet_" + idx.ToString();
			tbDetails.FontSize = 16;
			tbDetails.Text = it.sDetails;
			tbDetails.Background = new SolidColorBrush(Colors.Gray);
			tbDetails.Foreground = new SolidColorBrush(Colors.White);
			tbDetails.Margin = new Thickness(-11, -12, -12, -12);
			tbDetails.BorderThickness = new Thickness(0, 0, 0, 0);
			tbDetails.AcceptsReturn = true;
			tbDetails.TextWrapping = TextWrapping.Wrap;
			tbDetails.SetValue(Grid.RowProperty, 0);
			grdTit.Children.Add(tbDetails);
		
			Button btn = new Button();
			btn.Name = "btnTit_" + idx.ToString();
			btn.Content = "";
			btn.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
			btn.BorderThickness = new Thickness(0);
			btn.Foreground = new SolidColorBrush(Colors.White); //.Blue);
			btn.Margin = new Thickness(-12,-10,-12,-12);
			//btn.Tag = it;
			btn.Opacity = 0.5;
			btn.SetValue(Grid.RowProperty, 1);
			grdOut.Children.Add(btn);
			
			//it.tbTit = tbTit;
			//it.btn = btn;
			
			btnMore.Tag = it;
			btn.Tag = it;
			
			btnMore.Tap += new System.EventHandler<System.Windows.Input.GestureEventArgs>(btn_Tap);
			btn.Tap += new System.EventHandler<System.Windows.Input.GestureEventArgs>(btn_Tap);
			
			/*
			btnMore.DoubleTap += new System.EventHandler<System.Windows.Input.GestureEventArgs>(btn_DoubleTap);
			btn.DoubleTap += new System.EventHandler<System.Windows.Input.GestureEventArgs>(btn_DoubleTap);
			*/
			
		}

		private void btn_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			RscMediaLibItemDesc it;
			it = (RscMediaLibItemDesc) btn.Tag;
			
			if( it.bFolder )
			{
				//m_path.Push( it.sFn );
				
				//ListFiles();
			}
			else
			{
				m_txtTitle.Text = "medlib:\\" + it.sFolderPath + "\\" + it.sName;
				
				m_btnThumbnail.Visibility = Rsc.Visible;
				m_btnThumbnail.Tag = it.sIndiciesToItem;
				m_btnThumbnail.Image.Source = LoadImage( it, true, 48, 80 );
				
				/*
				m_btnImage.Visibility = Rsc.Collapsed;
				m_btnImage.Tag = null;
				m_btnImage.Image.Source = null;
				*/
								
				m_btnImage.Visibility = Rsc.Visible;
				m_btnImage.Tag = it.sIndiciesToItem;
				m_btnImage.Image.Source = LoadImage( it, false, 96, 160 );

			}
		}

		/*
		private void btn_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			RscMediaLibItemDesc it;
			it = (RscMediaLibItemDesc) btn.Tag;
			
			if( it.bFolder )
			{
				//m_path.Push( it.sFn );
				
				//ListFiles();
			}
			else
			{
				//m_txtTitle.Text = "medlib:\\" + it.sFolderPath + "\\" + it.sName;
				
				m_btnImage.Visibility = Rsc.Visible;
				m_btnImage.Tag = it.sIndiciesToItem;
				m_btnImage.Image.Source = LoadImage( it, false, 96, 160 );
			}
		}
		*/
		
		private ImageSource LoadImage( RscMediaLibItemDesc it, bool bThumbnail, int iCX, int iCY )
		{
			if( it.bFolder ) return m_AppFrame.Theme.GetImage("Images/Ico001_Ressive.jpg");
			
			// //
			//
			
			PictureAlbum pa = m_media.RootPictureAlbum;
			Picture pic = null;
			string [] asInd = it.sIndiciesToItem.Split('|');
			int iCnt = asInd.Length;
			for( int i = 0; i < iCnt; i++ )
			{
				if( i == iCnt - 1 )
				{
					pic = pa.Pictures[ Int32.Parse( asInd[ i ] ) ];
				}
				else
				{
					pa = pa.Albums[ Int32.Parse( asInd[ i ] ) ];
				}
			}
			
			//
			// //
			
			if( bThumbnail )
			{
				System.Windows.Media.Imaging.WriteableBitmap wbmp = 
					Microsoft.Phone.PictureDecoder.DecodeJpeg(pic.GetThumbnail(), iCX, iCY );
			
				return wbmp;
			}
			else
			{
				BitmapImage bmp = new BitmapImage();
				bmp.SetSource(pic.GetImage());
				return bmp;
			}
		}
		
		private void m_btnThumbnail_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn = (Button) sender;
			if( btn.Tag == null ) return;
			string sIndiciesToItem = (string) btn.Tag;
			if( sIndiciesToItem.Length == 0 ) return;
			
			SaveImage( sIndiciesToItem, true );
			SaveImage( sIndiciesToItem, false );
		}
		
		private void m_btnImage_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn = (Button) sender;
			if( btn.Tag == null ) return;
			string sIndiciesToItem = (string) btn.Tag;
			if( sIndiciesToItem.Length == 0 ) return;
			
			SaveImage( sIndiciesToItem, true );
			SaveImage( sIndiciesToItem, false );
			
			m_AppFrame.SetStatusText( "Save done!" );
		}
		
		private void SaveImage( string sIndiciesToItem, bool bThumbnail )
		{
			
			RscStore store = new RscStore();
			
			//string sFolder = RscKnownFolders.GetMediaPath("Zune");
			string sFolder = RscKnownFolders.GetMediaPath("Media Library");
			
			// //
			//
			
			PictureAlbum pa = m_media.RootPictureAlbum;
			Picture pic = null;
			string [] asInd = sIndiciesToItem.Split('|');
			int iCnt = asInd.Length;
			for( int i = 0; i < iCnt; i++ )
			{
				if( i == iCnt - 1 )
				{
					sFolder += "\\" + pa.Name;
					if( !store.FolderExists( sFolder ) )
						store.CreateFolder( sFolder );
					
					pic = pa.Pictures[ Int32.Parse( asInd[ i ] ) ];
				}
				else
				{
					sFolder += "\\" + pa.Name;
					if( !store.FolderExists( sFolder ) )
						store.CreateFolder( sFolder );
					
					pa = pa.Albums[ Int32.Parse( asInd[ i ] ) ];
				}
			}
			
			if( bThumbnail )
			{
				sFolder += "\\" + "tn";
				if( !store.FolderExists( sFolder ) )
					store.CreateFolder( sFolder );
			}
			
			//
			// //
			
			string sPreExt = "";
			if( bThumbnail )
				sPreExt = ".tn";
			
			string sFName = pic.Name;
			string sFExt = "";
			int iPos = sFName.LastIndexOf('.');
			if( iPos >= 0 )
			{
				sFExt = sFName.Substring( iPos );
				sFName = sFName.Substring( 0, iPos );
			}
			
			string sPath = sFolder + "\\" + sFName + sPreExt + sFExt;
			iCnt = 0;
			for(;;)
			{
				if( !store.FileExists(sPath) ) break;
				iCnt++;
				sPath = sFolder + "\\" + sFName + "_" + iCnt.ToString() + sPreExt + sFExt;
			}
			
			System.IO.Stream strmSrc;
			if( bThumbnail )
				strmSrc = pic.GetThumbnail();
			else
				strmSrc = pic.GetImage();
			
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
		}
		
    }
	
}