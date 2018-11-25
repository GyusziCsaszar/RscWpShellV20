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
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;

using Ressive.Utils;
using Ressive.Theme;
using Ressive.FrameWork;

namespace RscDC
{
	
    public partial class RscDC_CamcorderV11 : PhoneApplicationPage
    {
		
		RscTheme m_Theme = null;
		ImageSource m_isChbOn = null;
		ImageSource m_isChbOff = null;
				
		RscIconButton m_btnRecStart;
		RscIconButton m_btnRecStop;
		
		RscIconButton m_btnFldr;
	
		class RscOptItemDesc
		{
			
			public RscOptItemDesc m_grp;
			public List<RscOptItemDesc> m_aOpts = new List<RscOptItemDesc>();
			
			public string m_strTitle;
			public bool m_bCurrent;
			
			public RscIconButton m_imgChb;
			
			public object Tag;
			
			public RscOptItemDesc()
			{
				m_grp = null;
				
				m_strTitle = "N/A";
				m_bCurrent = false;
				
				m_imgChb = null;
				
				Tag = null;
			}
			
			public RscOptItemDesc(RscOptItemDesc grp)
			{
				m_grp = grp;
				m_grp.m_aOpts.Add( this );
				
				m_strTitle = "N/A";
				m_bCurrent = false;
				
				m_imgChb = null;
				
				Tag = null;
			}
			
			public bool Group
			{
				get{ return (m_grp == null); }
			}
			
		}
		
		// Viewfinder for capturing video.
		private VideoBrush videoRecorderBrush;
		
		// Source and device for capturing video.
		private CaptureSource captureSource;
		private VideoCaptureDevice videoCaptureDevice;
		
		// File details for storing the recording.        
		//private Iso latedStorageFileStream isoVideoFile;
		private FileSink fileSink;
		
		// For managing button and application state.
		private enum ButtonState { Initialized, Ready, Recording, Playback, Paused, NoChange, CameraNotSupported };
		private ButtonState currentAppState;
		
		bool m_bAlloweToInit = false;
		bool m_bForceCloseOnInit = false;
		DispatcherTimer m_tmrLaterInit;
		
        public RscDC_CamcorderV11()
        {
            InitializeComponent();
 			
			//MemUsage Optimization...
			Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			m_Theme = (RscTheme) GlobalDILholder.Tag;
			//m_dil = new RscDefaultedImageList( "Theme", "Current", "Default" );
			
			m_isChbOn = m_Theme.GetImage("Images/CheckOn.jpg");
			m_isChbOff = m_Theme.GetImage("Images/CheckOff.jpg");
			
			m_btnRecStart = new RscIconButton(shotGrid, Grid.ColumnProperty, 0, 50, 50, Rsc.Collapsed);
			m_btnRecStart.Image.Source = m_Theme.GetImage("Images/Btn001_Rec.jpg");
			m_btnRecStart.Click += new System.Windows.RoutedEventHandler(m_btnRecStart_Click);
			
			m_btnRecStop = new RscIconButton(shotGrid, Grid.ColumnProperty, 2, 50, 50, Rsc.Collapsed);
			m_btnRecStop.Image.Source = m_Theme.GetImage("Images/Btn001_RecStop.jpg");
			m_btnRecStop.Click += new System.Windows.RoutedEventHandler(m_btnRecStop_Click);
			
			m_btnFldr = new RscIconButton(toolsGrid, Grid.ColumnProperty, 3, 50, 50, Rsc.Visible);
			m_btnFldr.Image.Source = m_Theme.GetImage("Images/Ico001_VideoGallery.jpg");
			m_btnFldr.Click += new System.Windows.RoutedEventHandler(m_btnFldr_Click);
			
			toolsGrid.Visibility = Rsc.Visible;
			
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
       }
		
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			
			if( !m_bAlloweToInit ) return;
		
			// Initialize the video recorder.
			InitializeVideoRecorder();
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
			InitializeVideoRecorder();
		}
		
		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			// Dispose of camera and media objects.
			DisposeVideoPlayer();
			DisposeVideoRecorder();
		
			base.OnNavigatedFrom(e);
		}
		
		// Update the buttons and text on the UI thread based on app state.
		private void UpdateUI(ButtonState currentButtonState, string statusMessage)
		{
			// Run code on the UI thread.
			Dispatcher.BeginInvoke(delegate
			{
		
				switch (currentButtonState)
				{
					// When the camera is not supported by the phone.
					case ButtonState.CameraNotSupported:
						m_btnRecStart.Visibility = Rsc.Collapsed;
						m_btnRecStop.Visibility = Rsc.Collapsed;
						/*
						StartPlayback.IsEnabled = false;
						PausePlayback.IsEnabled = false;
						*/
						break;
		
					// First launch of the application, so no video is available.
					case ButtonState.Initialized:
						m_btnRecStart.Visibility = Rsc.Visible;
						m_btnRecStop.Visibility = Rsc.Collapsed;
						/*
						StartPlayback.IsEnabled = false;
						PausePlayback.IsEnabled = false;
						*/
						break;
		
					// Ready to record, so video is available for viewing.
					case ButtonState.Ready:
						m_btnRecStart.Visibility = Rsc.Visible;
						m_btnRecStop.Visibility = Rsc.Collapsed;
						/*
						StartPlayback.IsEnabled = true;
						PausePlayback.IsEnabled = false;
						*/
						break;
		
					// Video recording is in progress.
					case ButtonState.Recording:
						m_btnRecStart.Visibility = Rsc.Collapsed;
						m_btnRecStop.Visibility = Rsc.Visible;
						/*
						StartPlayback.IsEnabled = false;
						PausePlayback.IsEnabled = false;
						*/
						break;
		
					// Video playback is in progress.
					case ButtonState.Playback:
						m_btnRecStart.Visibility = Rsc.Collapsed;
						m_btnRecStop.Visibility = Rsc.Visible;
						/*
						StartPlayback.IsEnabled = false;
						PausePlayback.IsEnabled = true;
						*/
						break;
		
					// Video playback has been paused.
					case ButtonState.Paused:
						m_btnRecStart.Visibility = Rsc.Collapsed;
						m_btnRecStop.Visibility = Rsc.Visible;
						/*
						StartPlayback.IsEnabled = true;
						PausePlayback.IsEnabled = false;
						*/
						break;
		
					default:
						break;
				}
		
				/*
				// Display a message.
				txtDebug.Text = statusMessage;
				*/
		
				// Note the current application state.
				currentAppState = currentButtonState;
			});
		}
		
		public void InitializeVideoRecorder()
		{
			if (captureSource == null)
			{
				// Create the VideoRecorder objects.
				captureSource = new CaptureSource();
				fileSink = new FileSink();
		
				videoCaptureDevice = CaptureDeviceConfiguration.GetDefaultVideoCaptureDevice();
			
				// //
				//
				
				spOpts.Children.Clear();
				
				RscOptItemDesc itHead;
				RscOptItemDesc it;
				
				itHead = new RscOptItemDesc();
				itHead.m_strTitle = "Resolution";
				AddOpt(itHead);
				int iCnt = 0;
				foreach( VideoFormat vf in videoCaptureDevice.SupportedFormats )
				{
					iCnt++;
					
					/*
					//On WP7x no VideoCaptureDevice.DesiredFormat setting!!! Limit...
					if( iCnt > 1 ) break;
					*/
					
					//On Win10Mo unable change DesiredFormat...
					if( iCnt > 1 ) break;
					
					//Duplicated values...
					if( iCnt % 2 == 0 ) continue;
					
					it = new RscOptItemDesc(itHead);
					it.m_strTitle = vf.FramesPerSecond.ToString() + "fps" + "\r\n" + vf.PixelWidth.ToString() + "x" + vf.PixelHeight.ToString();
					it.m_bCurrent = (iCnt == 1); //(videoCaptureDevice.DesiredFormat == vf);
					it.Tag = vf;
					AddOpt(it);
				}
				
				//
				// //
		
				// Add eventhandlers for captureSource.
				captureSource.CaptureFailed += new EventHandler<ExceptionRoutedEventArgs>(OnCaptureFailed);
		
				// Initialize the camera if it exists on the phone.
				if (videoCaptureDevice != null)
				{
					// Create the VideoBrush for the viewfinder.
					videoRecorderBrush = new VideoBrush();
					videoRecorderBrush.SetSource(captureSource);
		
					// Display the viewfinder image on the rectangle.
					viewfinderRectangle.Fill = videoRecorderBrush;
		
					// Start video capture and display it on the viewfinder.
					captureSource.Start();
		
					// Set the button state and the message.
					UpdateUI(ButtonState.Initialized, "Tap record to start recording...");
				}
				else
				{
					// Disable buttons when the camera is not supported by the phone.
					UpdateUI(ButtonState.CameraNotSupported, "A camera is not supported on this phone.");
				}
			}
		}
		
		// Set recording state: start recording.
		private void StartVideoRecording()
		{
			try
			{
				// Connect fileSink to captureSource.
				if (captureSource.VideoCaptureDevice != null
					&& captureSource.State == CaptureState.Started)
				{
					captureSource.Stop();
			
					DateTime dNow = DateTime.Now;
			
					string sVidName = dNow.Year.ToString() + RscUtils.pad60(dNow.Month) 
						+ RscUtils.pad60(dNow.Day) + "_" + RscUtils.pad60(dNow.Hour) + RscUtils.pad60(dNow.Minute) 
						+ RscUtils.pad60(dNow.Second);
		
					// Connect the input and output of fileSink.
					fileSink.CaptureSource = captureSource;
					fileSink.IsolatedStorageFileName = (RscKnownFolders.GetMediaPath("DCVID") + "\\" + sVidName + ".mp4").Substring( 3 );
				}
		
				// Begin recording.
				if (captureSource.VideoCaptureDevice != null
					&& captureSource.State == CaptureState.Stopped)
				{
					captureSource.Start();
				}
		
				// Set the button states and the message.
				UpdateUI(ButtonState.Recording, "Recording...");
				
				BtnBk.Fill = new SolidColorBrush( Colors.Green );
			}
		
			// If recording fails, display an error.
			catch (Exception /*e*/)
			{
				this.Dispatcher.BeginInvoke(delegate()
				{
					//txtDebug.Text = "ERROR: " + e.Message.ToString();
				
					BtnBk.Fill = new SolidColorBrush( Colors.Red );
				});
			}
		}
		
		// Set the recording state: stop recording.
		private void StopVideoRecording()
		{
			try
			{
				// Stop recording.
				if (captureSource.VideoCaptureDevice != null
				&& captureSource.State == CaptureState.Started)
				{
					captureSource.Stop();
		
					// Disconnect fileSink.
					fileSink.CaptureSource = null;
					fileSink.IsolatedStorageFileName = null;
		
					// Set the button states and the message.
					UpdateUI(ButtonState.NoChange, "Preparing viewfinder...");
		
					StartVideoPreview();
				
					BtnBk.Fill = new SolidColorBrush( Colors.Green );
				}
			}
			// If stop fails, display an error.
			catch (Exception /*e*/)
			{
				this.Dispatcher.BeginInvoke(delegate()
				{
					//txtDebug.Text = "ERROR: " + e.Message.ToString();
				
					BtnBk.Fill = new SolidColorBrush( Colors.Red );
				});
			}
		}
		
		// Set the recording state: display the video on the viewfinder.
		private void StartVideoPreview()
		{
			try
			{
				// Display the video on the viewfinder.
				if (captureSource.VideoCaptureDevice != null
				&& captureSource.State == CaptureState.Stopped)
				{
					// Add captureSource to videoBrush.
					videoRecorderBrush.SetSource(captureSource);
		
					// Add videoBrush to the visual tree.
					viewfinderRectangle.Fill = videoRecorderBrush;
		
					captureSource.Start();
		
					// Set the button states and the message.
					UpdateUI(ButtonState.Ready, "Ready to record.");
				}
			}
			// If preview fails, display an error.
			catch (Exception /*e*/)
			{
				this.Dispatcher.BeginInvoke(delegate()
				{
					//txtDebug.Text = "ERROR: " + e.Message.ToString();
				});
			}
		}
		
		private void DisposeVideoPlayer()
		{
			/*
			if (VideoPlayer != null)
			{
				// Stop the VideoPlayer MediaElement.
				VideoPlayer.Stop();
		
				// Remove playback objects.
				VideoPlayer.Source = null;
				isoVideoFile = null;
		
				// Remove the event handler.
				VideoPlayer.MediaEnded -= VideoPlayerMediaEnded;
			}
			*/
		}

		private void DisposeVideoRecorder()
		{
			if (captureSource != null)
			{
				// Stop captureSource if it is running.
				if (captureSource.VideoCaptureDevice != null
					&& captureSource.State == CaptureState.Started)
				{
					captureSource.Stop();
				}
		
				// Remove the event handler for captureSource.
				captureSource.CaptureFailed -= OnCaptureFailed;
		
				// Remove the video recording objects.
				captureSource = null;
				videoCaptureDevice = null;
				fileSink = null;
				videoRecorderBrush = null;
			}
		}
		
		// If recording fails, display an error message.
		private void OnCaptureFailed(object sender, ExceptionRoutedEventArgs e)
		{
			this.Dispatcher.BeginInvoke(delegate()
			{
				//txtDebug.Text = "ERROR: " + e.ErrorException.Message.ToString();
			});
		}
		
       	protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			//e.Cancel = true;
		}
		
		private void StartRecording_Click(object sender, EventArgs e)
		{
			StartVideoRecording();
		}
		
		private void StopRecording_Click(object sender, EventArgs e)
		{
			StopVideoRecording();
		}
		
		private void m_btnRecStart_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			StartVideoRecording();
		}
		
		private void m_btnRecStop_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			StopVideoRecording();
		}
		
		private void AddOpt(RscOptItemDesc it)
		{
			int idx = spOpts.Children.Count + 1;
			
			Grid grdOut = new Grid();
			grdOut.Name = "grdOut_" + idx.ToString();
			if( it.Group && idx > 1 )
			{
				grdOut.Margin = new Thickness(0, 12, 0, 4 );
			}
			else
			{
				grdOut.Margin = new Thickness(0, 0, 0, 4 );
			}
			ColumnDefinition cd;
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdOut.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); grdOut.ColumnDefinitions.Add(cd);
			spOpts.Children.Add(grdOut);
			
			if( !it.Group )
			{	
				it.m_imgChb = new RscIconButton(grdOut, Grid.ColumnProperty, 0, 20, 20, Rsc.Visible);
				if( it.m_bCurrent )
				{
					it.m_imgChb.Image.Source = m_isChbOn;
				}
				else
				{
					it.m_imgChb.Image.Source = m_isChbOff;
				}
				it.m_imgChb.Click += new System.Windows.RoutedEventHandler(Item_Click);
				
				it.m_imgChb.Tag = it;
			}
			
			Rectangle rc;
			rc = new Rectangle();
			if( it.Group )
			{
				rc.Fill = new SolidColorBrush( m_Theme.ThemeColors.TreeContainerBack ); //Colors.Orange);
			}
			else
			{
				rc.Fill = new SolidColorBrush(m_Theme.ThemeColors.TreeLeafBack ); //Colors.Blue);
			}
			rc.Opacity = 0.5;
			rc.SetValue(Grid.ColumnProperty, 1);
			grdOut.Children.Add(rc);
	
			Button btnMore = new Button();
			btnMore.Name = "btnMore_" + idx.ToString();
			btnMore.Content = it.m_strTitle;
			btnMore.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
			btnMore.BorderThickness = new Thickness(0);
			if( it.Group )
			{
				btnMore.FontSize = 20;
				btnMore.Foreground = new SolidColorBrush( m_Theme.ThemeColors.TreeContainerFore ); //Colors.White);
			}
			else
			{
				btnMore.FontSize = 16;
				btnMore.Foreground = new SolidColorBrush( m_Theme.ThemeColors.TreeLeafFore ); //Colors.White);
			}
			btnMore.Foreground = new SolidColorBrush(Colors.White);
			btnMore.Margin = new Thickness(-12,-10,-12,-12);
			btnMore.SetValue(Grid.ColumnProperty, 1);
			grdOut.Children.Add(btnMore);
			
			if( !it.Group )
			{
				btnMore.Tag = it;
				btnMore.Click += new System.Windows.RoutedEventHandler(Item_Click);
			}
		}

		private void Item_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			RscOptItemDesc it;
			it = (RscOptItemDesc) btn.Tag;
			
			if( it.m_grp == null ) return;
			
			foreach( RscOptItemDesc itChb in it.m_grp.m_aOpts )
			{
				if( itChb.m_imgChb != null )
				{
					itChb.m_imgChb.Image.Source = m_isChbOff;
				}
			}
			
			switch( it.m_grp.m_strTitle )
			{
					
				case "Resolution" :
				{
					
					videoCaptureDevice.DesiredFormat = (VideoFormat) it.Tag;
			
					it.m_imgChb.Image.Source = m_isChbOn;
					
					break;
				}
					
			}
		}

		private void m_btnFldr_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sUri = "/Lib_RscViewers;component/" + "RscViewer_VideoFolderV10" + ".xaml";
			NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}
		
    }
	
}
