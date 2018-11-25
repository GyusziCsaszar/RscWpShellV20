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
using System.ComponentModel;
using System.Windows.Input;
using System.IO;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Ressive.Utils;
using Ressive.Store;
using Ressive.Theme;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

namespace Lib_RscViewers
{
	
    public partial class RscViewer_ImageV10 : PhoneApplicationPage
    {
		
		RscTheme m_Theme = null;
		ImageSource m_isDummy = null;
		ImageSource m_isSaveToPhone = null;
		
		string m_strInitErr = "";
			
		RscPageArgsRet appInput;
		
		int m_iIndex = 0;
		List<string> m_aImages = new List<string>();
		
		int m_iNaviState = 0;
		
		Point m_ptDragStart;
		
		RscIconButton m_btnExtOpen;
		RscIconButton m_btnSaveToPhone;
		
		int m_iImgWidth = 0;
		int m_iImgHeight = 0;
		
		//NOT WORKING!!!
		/*
		DispatcherTimer m_tmrGif;
		int m_iFrameGif = 0;
		*/
		
        public RscViewer_ImageV10()
        {
            InitializeComponent();
 			
			try
			{
				//MemUsage Optimization...
				Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
				m_Theme = (RscTheme) GlobalDILholder.Tag;
				//m_dil = new RscDefaultedImageList( "Theme", "Current", "Default" );
				
				AppIcon.Source = m_Theme.GetImage("Images/Ico001_Ressive.jpg");
				AppCloseIcon.Source = m_Theme.GetImage("Images/Btn001_Close.jpg");
				//AppNextIcon.Source = m_Theme.GetImage("Images/Btn001_Next.jpg");
				// ///////////////
				m_isDummy = m_Theme.GetImage("Images/Img001_Dummy.jpg");
				m_isSaveToPhone = m_Theme.GetImage("Images/Btn001_Save.jpg");
				// ///////////////
				imgPrev.Source = m_Theme.GetImage("Images/BtnDrk001_SkipPrev.jpg");
				imgNext.Source = m_Theme.GetImage("Images/BtnDrk001_SkipNext.jpg");
			
				m_btnExtOpen = new RscIconButton(RightBtns, Grid.RowProperty, 1, 54, 54, Rsc.Visible);
				m_btnExtOpen.Image.Source = m_Theme.GetImage("Images/BtnDrk001_Open.jpg");;
				m_btnExtOpen.Click += new System.Windows.RoutedEventHandler(m_btnExtOpen_Click);
			
				m_btnSaveToPhone = new RscIconButton(LeftBtns, Grid.RowProperty, 1, 54, 54, Rsc.Visible);
				m_btnSaveToPhone.Image.Source = m_isSaveToPhone;
				m_btnSaveToPhone.Click += new System.Windows.RoutedEventHandler(m_btnSaveToPhone_Click);
			
				//NOT WORKING!!!
				/*
				m_tmrGif = new DispatcherTimer();
				m_tmrGif.Interval = System.TimeSpan.FromSeconds(1);
				m_tmrGif.Tick +=new System.EventHandler(m_tmrGif_Tick);
				*/
				
			}
			catch( Exception e )
			{
				m_strInitErr = e.Message;
				txImgDetails.Text = m_strInitErr;
			}
			
			double dWidth = 480;
			double dHeight = 800;
			
			try
			{		
				RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
				appInput = appArgsMgr.GetInput( "RscViewer_ImageV10" );
				
				if( appInput != null )
				{
					
					ApplicationTitle.Text = appInput.CallerAppTitle;
					AppIcon.Source = m_Theme.GetImage(appInput.CallerAppIconRes);
					
					m_iIndex = 0;
					if( !Int32.TryParse( appInput.GetFlag(0), out m_iIndex ) ) return;
					
					if( !double.TryParse( appInput.GetFlag(1), out dWidth ) ) return;
					if( !double.TryParse( appInput.GetFlag(2), out dHeight ) ) return;
					
					m_aImages.Clear();
					for( int i = 0; i < appInput.DataCount; i++ )
					{
						string sPath = appInput.GetData( i );
						
						m_aImages.Add( sPath );
					}
					if( m_aImages.Count == 0 ) return;
					
					m_iIndex = Math.Min( m_iIndex, appInput.DataCount - 1);
					m_iIndex = Math.Max( m_iIndex, 0 );
				}
			}
			catch( Exception e )
			{
				txImgDetails.Text = e.Message;
			}
			
			if( m_aImages.Count == 0 )
			{
				imgFull.Stretch = Stretch.None;
				imgFull.Source = m_isDummy;
			}
			else
			{
				_ShowImage( m_aImages[ m_iIndex ], new Size(dWidth, dHeight) );
				_SetNextAndPrevBtn();
			}
		}
		
		//NEVER CALLED!!!
		/*
		private void ContentPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
		}
		*/
		
		private void AppClose_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			//NOT WORKING!!!
			//m_tmrGif.Stop();
			
			this.NavigationService.GoBack();
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			//NOT WORKING!!!
			//m_tmrGif.Stop();
			
			//e.Cancel = true;
		}
		
		void btnFull_ManipulationStarted(object sender, ManipulationStartedEventArgs args)
		{
			m_ptDragStart = args.ManipulationOrigin;
		}

		private void btnFull_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs args)
		{
			double dCX = args.ManipulationOrigin.X - m_ptDragStart.X;
			
			//Smaller movement...
			dCX = dCX / 5;
			
			imgFull.Margin = new Thickness(dCX, 0, -dCX, 0);
		}
		
		void btnFull_ManipulationCompleted(object sender, ManipulationCompletedEventArgs args)
		{
			//DeBug...
			/*
			txImgDetails.Text = "CX: " + (args.ManipulationOrigin.X - m_ptDragStart.X).ToString() +
				"\r\n" + "CY: " + (args.ManipulationOrigin.Y - m_ptDragStart.Y).ToString();
			*/
			
			imgFull.Margin = new Thickness(0);
			
			double dCX = args.ManipulationOrigin.X - m_ptDragStart.X;
			
			if( dCX > 100 )
			{
				if( _Prev() )
				{
					args.Handled = true;
				}
			}
			else if( dCX < -100 )
			{
				if( _Next() )
				{
					args.Handled = true;
				}
			}
			else
			{
				//NOP...
			}
		}
		
		private void btnFull_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			switch( m_iNaviState )
			{
				
				case 0 : //Off...
					NaviPanel.Opacity = 1;
					NaviPanel.Visibility = Rsc.Visible;
					m_iNaviState++;
					break;
					
				case 1 : //On...
					NaviPanel.Opacity = 0.5;
					m_iNaviState++;
					break;
					
				case 2 : //Intermediate 1...
					NaviPanel.Opacity = 0.25;
					m_iNaviState++;
					break;
					
				case 3 : //Intermediate 2...
					NaviPanel.Visibility = Rsc.Collapsed;
					m_iNaviState = 0;
					break;
					
			}
		}
			
		private void btnPrev_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_Prev();
		}
		
		private bool _Prev()
		{
			//NOT WORKING!!!
			//m_tmrGif.Stop();
			
			if( m_aImages.Count == 0 ) return false;
			
			if( m_iIndex > 0 ) m_iIndex--;
			
			_ShowImage( m_aImages[m_iIndex], new Size(LayoutRoot.ActualWidth, LayoutRoot.ActualHeight));
			_SetNextAndPrevBtn();
			
			//NaviPanel.Visibility = Rsc.Collapsed;
			
			return true;
		}
			
		private void btnNext_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_Next();
		}
		
		private bool _Next()
		{
			//NOT WORKING!!!
			//m_tmrGif.Stop();
			
			if( m_aImages.Count == 0 ) return false;
			
			if( m_iIndex < (m_aImages.Count - 1) ) m_iIndex++;
			
			_ShowImage( m_aImages[m_iIndex], new Size(LayoutRoot.ActualWidth, LayoutRoot.ActualHeight));
			_SetNextAndPrevBtn();
			
			//NaviPanel.Visibility = Rsc.Collapsed;
			
			return true;
		}
		
		private void _SetNextAndPrevBtn()
		{
			if( m_iIndex <= 0 )
			{
				imgPrev.Visibility = Rsc.Collapsed;
				//btnPrev.IsEnabled = false;
			}
			else
			{
				imgPrev.Visibility = Rsc.Visible;
				//btnPrev.IsEnabled = true;
			}
			
			if( m_iIndex >= (m_aImages.Count - 1) )
			{
				imgNext.Visibility = Rsc.Collapsed;
				//btnNext.IsEnabled = false;
			}
			else
			{
				imgNext.Visibility = Rsc.Visible;
				//btnNext.IsEnabled = true;	
			}
		}
		
		private void _ShowImage( string sPath, Size sz )
		{
			//NOT WORKING!!!
			//m_tmrGif.Stop();
			
			txPrevCnt.Text = m_iIndex.ToString();
			txNextCnt.Text = Math.Max(0, ((m_aImages.Count - 1) - m_iIndex)).ToString();
			
			string strDet = (m_iIndex + 1).ToString() + " / " + m_aImages.Count.ToString() + "\r\n";
			
			string strFn = sPath;
			int iPos = sPath.LastIndexOf( '\\' );
			if( iPos >= 0 )
				strFn = sPath.Substring(iPos + 1);
			strDet += "\r\n" + strFn;
			
			string strExt = "";
			int iPos2 = strFn.LastIndexOf('.');
			if( iPos2 >= 0 )
				strExt = strFn.Substring( iPos2 + 1 );
			
			//NOT WORKING!!!
			/*
			bool bGif = false;
			if( strExt.ToLower() == "gif" )
				bGif = true;
			*/
			
			string strFldr = "\\";
			if( iPos >= 0 )
				strFldr = sPath.Substring(0, iPos + 1);
			
			strDet += "\r\n" + "\r\n" + strFldr;
			txImgDetails.Text = strDet;
			
			long lFs = 0;
			m_iImgWidth = 0;
			m_iImgHeight = 0;
			try
			{
				RscStore store = new RscStore();
				
				//if( !store.FileExists(sPath) ) return;
				
				Stream stream = store.GetReaderStream( sPath, false );
				lFs = stream.Length;
				
				//TODO... ImageTools, strExt
				
				BitmapImage bmp = new BitmapImage();
				bmp.SetSource(stream);
				stream.Close();
				
				m_iImgWidth = bmp.PixelWidth;
				m_iImgHeight = bmp.PixelHeight;
				
				//NOT WORKING!!!
				/*
				if( bGif )
				{
					imgFull.Visibility = Rsc.Collapsed;
					
					//imgGif.Source = bmp;
					imgGif.Source = new Uri( "", UriKind.Absolute );
					
					canvGif.Visibility = Rsc.Visible;
					
					m_iFrameGif = 0;
					m_tmrGif.Start();
				}
				else
				{
					canvGif.Visibility = Rsc.Collapsed;
				*/
				
					//strDet += "\r\n" + "bmp( " + bmp.PixelWidth.ToString() + " ; " + bmp.PixelHeight.ToString() + " )";
					//strDet += "\r\n" + "sz( " + sz.Width.ToString() + " ; " + sz.Height.ToString() + " )";
					if( (bmp.PixelWidth <= sz.Width) && (bmp.PixelHeight <= sz.Height) )
					{
						imgFull.Stretch = Stretch.None;
					}
					else
					{
						imgFull.Stretch = Stretch.Uniform;
					}
				
					imgFull.Source = bmp;
				
				//NOT WORKING!!!
				/*
					imgFull.Visibility = Rsc.Visible;
				}
				*/
			}
			catch( Exception e )
			{
				strDet += "\r\n" + "\r\nERROR: " + e.Message;
				
				imgFull.Stretch = Stretch.None;
				imgFull.Source = m_isDummy;
			}
			
			if( lFs > 0 )
			{
				strDet += "\n\n" + lFs.ToString() + " B" + " ( " + RscUtils.toMBstr(lFs) + " ) ";
			}

			string sDim = "";
			if( m_iImgWidth > 0 )
				sDim += m_iImgWidth.ToString();
			else
				sDim += "-";
			sDim += " x ";
			if( m_iImgHeight > 0 )
				sDim += m_iImgHeight.ToString();
			else
				sDim += "-";
			strDet += "\n\n" + sDim;
			
			if( m_strInitErr.Length > 0 )
			{
				strDet += "\n\n" + "\r\nApp Init ERROR: " + m_strInitErr;
			}
			
			txImgDetails.Text = strDet;
		}
		
		//NOT WORKING!!!
		/*
		private void m_tmrGif_Tick(object sender, System.EventArgs e)
		{
			m_tmrGif.Stop();
			
			m_iFrameGif++;
			if( m_iFrameGif > 10 )
				m_iFrameGif = 0;
			
			imgGif.Clip = new RectangleGeometry {
                                       Rect =
                                           new Rect(
                                               m_iFrameGif * m_iImgWidth,
                                               0,
                                               m_iImgWidth,
                                               m_iImgHeight)
                                           };

			Canvas.SetLeft( imgGif, -1 * m_iImgWidth * m_iFrameGif);

			m_tmrGif.Start();
		}
		*/
		
		private void m_btnSaveToPhone_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_iIndex < 0 ) return;
			if( m_iIndex >= m_aImages.Count ) return;
			
			RscStore store = new RscStore();
			
			string sPath = m_aImages[m_iIndex];
			string sName = store.GetOriginalFileNameOfPath( sPath );
			
			//MessageBox.Show( sPath + "\r\n" + sName ); //sTnFolder + "\r\n" + sTnPath );
			
			Stream stream = store.GetReaderStream( sPath, false );
			
			MediaLibrary media = new MediaLibrary();
			media.SavePicture( sName, stream );
			
			stream.Close();

			MessageBox.Show( "Image (" + sName + ") saved to device's Saved Pictures folder." );
		}
		
		private void m_btnExtOpen_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_iIndex < 0 ) return;
			if( m_iIndex >= m_aImages.Count ) return;
			
			string sErr = "";
			
			if( !RscStore_Storage.LaunchFile( m_aImages[ m_iIndex ], out sErr ) )
			{
				if( sErr.Length > 0 )
					MessageBox.Show( sErr );
				else
					MessageBox.Show( "No app installed to open this file." );
			}
		}

    }
	
}
