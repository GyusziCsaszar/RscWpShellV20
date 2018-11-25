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
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Input;

using Ressive.Utils;
using Ressive.Store;
using Ressive.Theme;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

namespace Lib_RscViewers
{
	
    public partial class RscViewer_MediaV10 : PhoneApplicationPage
    {
		
		RscPageArgsRetManager m_AppArgs;
		
		RscTheme m_Theme = null;
		ImageSource m_isDummy = null;
		ImageSource m_isSaveToPhone = null;
		ImageSource m_isMuteOff = null;
		ImageSource m_isMuteOn = null;
		
		string m_strInitErr = "";
			
		RscPageArgsRet appInput;
		
		int m_iIndex = 0;
		List<string> m_aVideos = new List<string>();
		
		int m_iNaviState = 0;
		
		Point m_ptDragStart;
		
		DispatcherTimer m_currentPosition = new DispatcherTimer();
		
		string m_sVidInf;
		
		bool m_bAlloweToInit = false;
		bool m_bForceCloseOnInit = false;
		DispatcherTimer m_tmrLaterInit;
		
		RscIconButton m_btnMuteOnOff;
		RscIconButton m_btnExtOpen;
		//RscIconButton m_btnSaveToPhone;
		
        public RscViewer_MediaV10()
        {
            InitializeComponent();
 			
			m_AppArgs = new RscPageArgsRetManager();
			
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
				m_isMuteOff = m_Theme.GetImage("Images/Btn001_MuteOff.jpg");
				m_isMuteOn = m_Theme.GetImage("Images/Btn001_MuteOn.jpg");
				// ///////////////
				imgPrev.Source = m_Theme.GetImage("Images/BtnDrk001_SkipPrev.jpg");
				imgNext.Source = m_Theme.GetImage("Images/BtnDrk001_SkipNext.jpg");
				// ///////////////
				imgPlay.Source = m_Theme.GetImage("Images/Btn001_Play.jpg");
				imgPause.Source = m_Theme.GetImage("Images/Btn001_Pause.jpg");
				imgStop.Source = m_Theme.GetImage("Images/Btn001_Stop.jpg");
			
				m_btnMuteOnOff = new RscIconButton(RightBtns, Grid.RowProperty, 2, 54, 54, Rsc.Collapsed);
				m_btnMuteOnOff.Image.Source = m_isSaveToPhone;
				m_btnMuteOnOff.Click += new System.Windows.RoutedEventHandler(m_btnMuteOnOff_Click);
			
				m_btnExtOpen = new RscIconButton(LeftBtns, Grid.RowProperty, 2, 54, 54, Rsc.Visible);
				m_btnExtOpen.Image.Source = m_Theme.GetImage("Images/BtnDrk001_Open.jpg");;
				m_btnExtOpen.Click += new System.Windows.RoutedEventHandler(m_btnExtOpen_Click);
			
				//WP81 FIX...
				/*
				m_btnSaveToPhone = new RscIconButton(LeftBtns, Grid.RowProperty, 0, 54, 54, Rsc.Collapsed);
				m_btnSaveToPhone.Image.Source = m_isSaveToPhone;
				m_btnSaveToPhone.Click += new System.Windows.RoutedEventHandler(m_btnSaveToPhone_Click);
				*/
			}
			catch( Exception e )
			{
				m_strInitErr = e.Message;
				txImgDetails.Text = m_strInitErr;
			}
			
			/*
				BUGFIX: Unable to start camera, while playing music (Sound Player app)!!!
			*/
			MediaElement me = Application.Current.Resources["GlobalMedia"] as MediaElement;
			switch( me.CurrentState )
			{
				case MediaElementState.Closed :
				case MediaElementState.Stopped :
				{
					m_bAlloweToInit = true;
					break;
				}
				default :
				{
					if( MessageBoxResult.OK == MessageBox.Show( "To start app, internal music player must be stopped!\r\n\r\n(press Back to cancel)" ) )
					{
						Ressive.MediaLib.RscMediaLib.StopMusic();
					}
					else
					{
						m_bForceCloseOnInit = true;
					}

					m_tmrLaterInit = new DispatcherTimer();
					m_tmrLaterInit.Interval = new TimeSpan(1000);
					m_tmrLaterInit.Tick +=new System.EventHandler(m_tmrLaterInit_Tick);
					m_tmrLaterInit.Start();
					break;
				}
			}
			
			this.Loaded += new System.Windows.RoutedEventHandler(RscVideoGalleryV10_Loaded);
       }
		
		//NEVER CALLED!!!
		/*
		private void ContentPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
		}
		*/
		
		private void AppClose_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.NavigationService.GoBack();
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			//e.Cancel = true;
		}

		private void RscVideoGalleryV10_Loaded(object sender, System.Windows.RoutedEventArgs args)
		{
			if( !m_bAlloweToInit ) return;
			
			DoLoad();
		}
		
		private void m_tmrLaterInit_Tick(object sender, System.EventArgs e)
		{
			m_tmrLaterInit.Stop();
			
			if( m_bForceCloseOnInit )
			{
				NavigationService.GoBack();
				return;
			}
			
			m_bAlloweToInit = true;
			DoLoad();
		}
		
		private double GetVol(bool bSet = false)
		{
			int iMuteOn;
			
			if( bSet )
			{
				iMuteOn = 1; //True...
				if( vidFull.Volume > 0 )
					iMuteOn = 0; //False...
				
				RscRegistry.WriteDWORD(HKEY.HKEY_CURRENT_USER, "Software\\Ressive.Hu\\RscViewer_MediaV10", "MuteOn", iMuteOn );
			}
			else
			{
				iMuteOn = RscRegistry.ReadDWORD(HKEY.HKEY_CURRENT_USER, "Software\\Ressive.Hu\\RscViewer_MediaV10", "MuteOn", 0 );
			}
			
			if( iMuteOn != 0 )
			{
				m_btnMuteOnOff.Image.Source = m_isMuteOn;
				return 0;
			}
			else
			{
				m_btnMuteOnOff.Image.Source = m_isMuteOff;
				return 1;
			}
		}
		
		private void DoLoad()
		{
			
			vidFull.MediaFailed += new System.EventHandler<System.Windows.ExceptionRoutedEventArgs>(vidFull_MediaFailed);
			vidFull.MediaOpened += new RoutedEventHandler(vidFull_MediaOpened);
			vidFull.CurrentStateChanged += new System.Windows.RoutedEventHandler(vidFull_CurrentStateChanged);
			vidFull.MediaEnded += new System.Windows.RoutedEventHandler(vidFull_MediaEnded);
			
			m_currentPosition.Tick += new System.EventHandler(m_currentPosition_Tick);
			
			vidFull.Volume = GetVol();
			m_btnMuteOnOff.Visibility = Rsc.Visible;
			
			try
			{		
				RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
				appInput = appArgsMgr.GetInput( "RscViewer_MediaV10" );
				
				if( appInput != null )
				{
					
					ApplicationTitle.Text = appInput.CallerAppTitle;
					AppIcon.Source = m_Theme.GetImage(appInput.CallerAppIconRes);
					
					m_iIndex = 0;
					if( !Int32.TryParse( appInput.GetFlag(0), out m_iIndex ) ) return;
					
					//NOT NEEDED...
					/*
					if( !double.TryParse( appInput.GetFlag(1), out dWidth ) ) return;
					if( !double.TryParse( appInput.GetFlag(2), out dHeight ) ) return;
					*/
					
					m_aVideos.Clear();
					for( int i = 0; i < appInput.DataCount; i++ )
					{
						string sPath = appInput.GetData( i );
						
						m_aVideos.Add( sPath );
					}
					if( m_aVideos.Count == 0 ) return;
					
					m_iIndex = Math.Min( m_iIndex, appInput.DataCount - 1);
					m_iIndex = Math.Max( m_iIndex, 0 );
				}
			}
			catch( Exception e )
			{
				txImgDetails.Text = e.Message;
			}
			
			if( m_aVideos.Count != 0 )
			{
				_ShowVideo( m_aVideos[ m_iIndex ] );
				_SetNextAndPrevBtn();
			}
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
			
			vidFull.Margin = new Thickness(dCX, 0, -dCX, 0);
		}
		
		void btnFull_ManipulationCompleted(object sender, ManipulationCompletedEventArgs args)
		{
			//DeBug...
			/*
			txImgDetails.Text = "CX: " + (args.ManipulationOrigin.X - m_ptDragStart.X).ToString() +
				"\r\n" + "CY: " + (args.ManipulationOrigin.Y - m_ptDragStart.Y).ToString();
			*/
			
			vidFull.Margin = new Thickness(0);
			
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
			if( m_aVideos.Count == 0 ) return false;
			
			if( m_iIndex > 0 ) m_iIndex--;
			
			_ShowVideo( m_aVideos[m_iIndex]);
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
			if( m_aVideos.Count == 0 ) return false;
			
			if( m_iIndex < (m_aVideos.Count - 1) ) m_iIndex++;
			
			_ShowVideo( m_aVideos[m_iIndex]);
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
			
			if( m_iIndex >= (m_aVideos.Count - 1) )
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
		
		private void _ShowVideo( string sPath )
		{
			txPrevCnt.Text = m_iIndex.ToString();
			txNextCnt.Text = Math.Max(0, ((m_aVideos.Count - 1) - m_iIndex)).ToString();
			
			string strDet = (m_iIndex + 1).ToString() + " / " + m_aVideos.Count.ToString() + "\r\n";
			
			string strFn = sPath;
			int iPos = sPath.LastIndexOf( '\\' );
			if( iPos >= 0 )
				strFn = sPath.Substring(iPos + 1);
			strDet += "\r\n" + strFn;
			
			string strFldr = "\\";
			if( iPos >= 0 )
				strFldr = sPath.Substring(0, iPos + 1);
			
			strDet += "\r\n" + "\r\n" + strFldr;
			
			m_sVidInf = strDet;
			txImgDetails.Text = strDet;
			
			long lFs = 0;
			
			try
			{
				if( RscStore.IsIsoStorePath( sPath ) )
				{
					RscStore store = new RscStore();
	
					System.IO.Stream stream = store.GetReaderStream( sPath, false );
					lFs = stream.Length;
					
					vidFull.SetSource(stream);
					
					stream.Close();
				}
				else
				{
					RscStore store = new RscStore();
					lFs = store.GetFileLength( sPath );
					
					vidFull.Source = new Uri( "file:///" + sPath, UriKind.Absolute );
				}
				
			}
			catch( Exception e )
			{
				strDet += "\r\n" + "\r\nERROR: " + e.Message;
			}
			
			if( lFs > 0 )
			{
				strDet += "\r\n" + lFs.ToString() + " B" + " ( " + RscUtils.toMBstr(lFs) + " ) ";
			}
			
			if( m_strInitErr.Length > 0 )
			{
				strDet += "\r\n" + "\r\nApp Init ERROR: " + m_strInitErr;
			}
			
			m_sVidInf = strDet;
			txImgDetails.Text = strDet;
		}

		private void vidFull_MediaFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
		{
			PlayPanel.Visibility = Rsc.Collapsed;
			txImgDetails.Text = txImgDetails.Text + "\r\n" + "\r\nERROR: " + e.ErrorException.Message;
		}
		
		void vidFull_MediaOpened(object sender, RoutedEventArgs e)
		{
			imgPlay.Visibility = Rsc.Collapsed;
			btnPlay.Visibility = Rsc.Collapsed;
			//
			imgPause.Visibility = Rsc.Collapsed;
			btnPause.Visibility = Rsc.Collapsed;
			//
			imgStop.Visibility = Rsc.Collapsed;
			btnStop.Visibility = Rsc.Collapsed;
			
			PlayPanel.Visibility = Rsc.Visible;
			
			prsBarLen.Minimum = 0;
			prsBarLen.Maximum = (int) vidFull.NaturalDuration.TimeSpan.TotalMilliseconds;
			
			string strDet;
			strDet = "\r\n\r\n";
			strDet += vidFull.NaturalVideoWidth.ToString() + " x " + vidFull.NaturalVideoHeight.ToString();
			
			m_sVidInf += strDet;
			txImgDetails.Text = m_sVidInf;
			
			vidFull.Play( );
		}

		private void vidFull_CurrentStateChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			if (vidFull.CurrentState == MediaElementState.Playing)
			{
				m_currentPosition.Start();
				
				imgPlay.Visibility = Rsc.Collapsed;
				btnPlay.Visibility = Rsc.Collapsed;
				//
				imgPause.Visibility = Rsc.Visible;
				btnPause.Visibility = Rsc.Visible;
				//
				imgStop.Visibility = Rsc.Visible;
				btnStop.Visibility = Rsc.Visible;
				
				//m_btnSaveToPhone.Visibility = Rsc.Collapsed;
			}
			//If video runs its end, on Lumia800, seems to become in Paused state (no SaveToPhone button)
			/*
			else if (vidFull.CurrentState == MediaElementState.Paused)
			{
				m_currentPosition.Stop();
				
				imgPlay.Visibility = Rsc.Visible;
				btnPlay.Visibility = Rsc.Visible;
				//
				imgPause.Visibility = Rsc.Collapsed;
				btnPause.Visibility = Rsc.Collapsed;
				//
				imgStop.Visibility = Rsc.Visible;
				btnStop.Visibility = Rsc.Visible;
				
				m_btnSaveToPhone.Visibility = Rsc.Collapsed;
			}
			*/
			else
			{
				m_currentPosition.Stop();
				
				imgPlay.Visibility = Rsc.Visible;
				btnPlay.Visibility = Rsc.Visible;
				//
				imgPause.Visibility = Rsc.Collapsed;
				btnPause.Visibility = Rsc.Collapsed;
				//
				imgStop.Visibility = Rsc.Collapsed;
				btnStop.Visibility = Rsc.Collapsed;
				
				//m_btnSaveToPhone.Visibility = Rsc.Visible;
			}
		}

		private void m_currentPosition_Tick(object sender, System.EventArgs e)
		{
			if( vidFull.Position.TotalMilliseconds > vidFull.NaturalDuration.TimeSpan.TotalMilliseconds )
			{
				//Can happen...
				prsBarLen.Maximum = (int) vidFull.Position.TotalMilliseconds;
			}
			
			prsBarLen.Value = (int) vidFull.Position.TotalMilliseconds;
			
			string strDet;
			strDet = "\r\n";
			strDet += Math.Round(vidFull.RenderedFramesPerSecond, 0).ToString() + " fps" + "\r\n" +
				vidFull.Position.ToString() + " / " + vidFull.NaturalDuration.ToString();
			
			txImgDetails.Text = m_sVidInf + strDet;
		}

		private void vidFull_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
		{
			vidFull.Stop();
		}
			
		private void btnPlay_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			vidFull.Play();
		}
			
		private void btnPause_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			vidFull.Pause();
		}
			
		private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			vidFull.Stop();
		}
		
		private void m_btnSaveToPhone_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_iIndex < 0 ) return;
			if( m_iIndex >= m_aVideos.Count ) return;
			
			string sPath = m_aVideos[m_iIndex];
			
			// //
			//
			
			string sTnPath = "";
			int iPos = sPath.LastIndexOf('\\');
			if( iPos < 0 )
				iPos = 0;
			else
				iPos++;
			int iPos2 = sPath.LastIndexOf('.');
			if( iPos2 >= 0)
			{
				if( iPos > 0 ) sTnPath += sPath.Substring( 0, iPos );
				sTnPath += "tn\\";
				sTnPath += sPath.Substring( iPos, iPos2 - iPos );
				sTnPath += ".tn";
				sTnPath += ".jpg"; //sPath.Substring( iPos2 );
			}
			
			//
			// //
			
			RscStore store = new RscStore();
			if( !store.FileExists( sTnPath ) )
				sTnPath = "";
			
			//MessageBox.Show( sPath + "\r\n" + sTnPath );
			
			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				ApplicationTitle.Text, "Images/Ico001_Ressive.jpg", "SaveToPhone" );
			
			appInput.SetData( 0, sPath );
			appInput.SetData( 1, sTnPath );
			
			appInput.SetInput( "RscDC_FlashV10" );
						
			this.NavigationService.Navigate( appInput.GetNavigateUri( "RscDC" ) );
		}
		
		private void m_btnMuteOnOff_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( vidFull.Volume > 0 )
				vidFull.Volume = 0;
			else
				vidFull.Volume = 1;
			
			GetVol( true );
		}
		
		private void m_btnExtOpen_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_iIndex < 0 ) return;
			if( m_iIndex >= m_aVideos.Count ) return;
			
			string sErr = "";
			
			if( !RscStore_Storage.LaunchFile( m_aVideos[ m_iIndex ], out sErr ) )
			{
				if( sErr.Length > 0 )
					MessageBox.Show( sErr );
				else
					MessageBox.Show( "No app installed to open this file." );
			}
		}
		
		/*
		private void ScreenShot()
		{
			DateTime dNow = DateTime.Now;
			string fName = dNow.Year.ToString() +
					"_" + RscUtils.pad60(dNow.Month) + "_" +
					RscUtils.pad60(dNow.Day) + "_" + RscUtils.pad60(dNow.Hour) +
					"_" + RscUtils.pad60(dNow.Minute) + "_" +
					RscUtils.pad60(dNow.Second) + ".jpg";
			
			string fPath = RscFs.GetRscMediaFolderPath("Screen Shot") + "\\" + fName;
			
			SaveControlShot(LayoutRoot, fPath);
			//SaveControlShot(vidFull, fPath);
		}
		
		private void SaveControlShot(UIElement ctrl, string fName)
		{
			WriteableBitmap wbmp = new WriteableBitmap(ctrl,
				new System.Windows.Media.MatrixTransform());
			
			SaveWBmp(wbmp, fName);
		}
		
		private void SaveWBmp(WriteableBitmap wbmp, string fName)
		{
			RscStore store = new RscStore();
		
			if( store.FileExists(fName) ) store.DeleteFile(fName);
		
			System.IO.Stream stream = store.CreateFile(fName);
		
			System.Windows.Media.Imaging.
			Extensions.SaveJpeg(wbmp, stream,
			wbmp.PixelWidth, wbmp.PixelHeight,
			0, 100);
		
			stream.Close();
		}
		*/

    }
	
}
