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
using Microsoft.Xna.Framework.Media;
using Microsoft.Devices;

using Ressive.Utils;
using Ressive.Store;
using Ressive.Theme;
using Ressive.FrameWork;

namespace RscDC
{
	
    public partial class RscDC_CameraV11 : PhoneApplicationPage
    {
		
		RscTheme m_Theme = null;
		ImageSource m_isChbOn = null;
		ImageSource m_isChbOff = null;
		
		RscIconButton m_btnFocus;
		RscIconButton m_btnShutter;
		
		RscIconButton m_btnTools;
		RscIconButton m_btnVid;
		RscIconButton m_btnFldr;
		
		bool m_bSaveToMediaLibrary = false; //true;
	
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
				
		PhotoCamera cam;
		DispatcherTimer m_tmrInit;
		
		string m_sFnDtNext = "";
		
		bool m_bAlloweToInit = false;
		bool m_bForceCloseOnInit = false;
		DispatcherTimer m_tmrLaterInit;
		
		bool m_bStandAloneApp = false;
		
        public RscDC_CameraV11()
        {
            InitializeComponent();
			
 			//StandAlone app...
			Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			if( GlobalDILholder.Tag == null )
			{
				m_bStandAloneApp = true;
				GlobalDILholder.Tag = new RscTheme( true, "Theme", "Current", "Default" );
			}
			//StandAlone app...
			
			//MemUsage Optimization...
			//Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			m_Theme = (RscTheme) GlobalDILholder.Tag;
			//m_dil = new RscDefaultedImageList( "Theme", "Current", "Default" );
			
			m_isChbOn = m_Theme.GetImage("Images/CheckOn.jpg");
			m_isChbOff = m_Theme.GetImage("Images/CheckOff.jpg");
			
			m_btnFocus = new RscIconButton(shotGrid, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible);
			m_btnFocus.Image.Source = m_Theme.GetImage("Images/Btn001_ShotAF.jpg");
			m_btnFocus.Click += new System.Windows.RoutedEventHandler(m_btnFocus_Click);
			
			m_btnShutter = new RscIconButton(shotGrid, Grid.ColumnProperty, 2, 50, 50, Rsc.Visible);
			m_btnShutter.Image.Source = m_Theme.GetImage("Images/Btn001_Shot.jpg");
			m_btnShutter.Click += new System.Windows.RoutedEventHandler(m_btnShutter_Click);
			
			if( m_bStandAloneApp )
			{
				m_btnTools = new RscIconButton(toolsGrid, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible);
				m_btnTools.Image.Source = m_Theme.GetImage("Images/IcoSm001_LauncherMini.jpg");
				m_btnTools.Click += new System.Windows.RoutedEventHandler(m_btnTools_Click);
			}
				
			if( m_bStandAloneApp )
			{
				m_btnVid = new RscIconButton(toolsGrid, Grid.ColumnProperty, 2, 50, 50, Rsc.Visible);
				m_btnVid.Image.Source = m_Theme.GetImage("Images/Ico001_Camcorder.jpg");
				m_btnVid.Click += new System.Windows.RoutedEventHandler(m_btnVid_Click);
			}
			
			m_btnFldr = new RscIconButton(toolsGrid, Grid.ColumnProperty, 3, 50, 50, Rsc.Visible);
			m_btnFldr.Image.Source = m_Theme.GetImage("Images/Ico001_Gallery.jpg");
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
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			
			if( !m_bAlloweToInit ) return;
			
			DoInit();
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
			DoInit();
		}
		
		private void DoInit()
		{
			shotGrid.Visibility = Rsc.Visible;
			
			cam = new PhotoCamera();
			//BUG: Not called by sys...
			/*
			cam.Initialized += new System.EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);
			*/
			cam.CaptureCompleted +=new System.EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_CaptureCompleted);
			cam.CaptureThumbnailAvailable += new System.EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureThumbnailAvailable);
			cam.CaptureImageAvailable += new System.EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);
			cam.AutoFocusCompleted +=new System.EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_AutoFocusCompleted);

			videoBrush.SetSource(cam);
			
			m_tmrInit = new DispatcherTimer();
			m_tmrInit.Interval = new TimeSpan(1000);
			m_tmrInit.Tick +=new System.EventHandler(m_tmrInit_Tick);
			m_tmrInit.Start();
		}
		
		/*
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (cam != null)
            {
                // LandscapeRight rotation when camera is on back of phone.
                int landscapeRightRotation = 180;

                // Change LandscapeRight rotation for front-facing camera.
                if (cam.CameraType == CameraType.FrontFacing) landscapeRightRotation = -180;

                // Rotate video brush from camera.
                if (e.Orientation == PageOrientation.LandscapeRight)
                {
                    // Rotate for LandscapeRight orientation.
                    videoBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = landscapeRightRotation };
                }
                else
                {
                    // Rotate for standard landscape orientation.
                    videoBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 0 };
                }
            }
			
			/*
			if( cam != null )
			{
				Dispatcher.BeginInvoke (() =>
				{
					double rotation = cam.Orientation; 
					
					switch (this the Orientation)
					{
						case PageOrientation.LandscapeLeft: 
							rotation = cam.Orientation - 90; 
							break;
							
						 case PageOrientation.LandscapeRight: 
							rotation = cam.Orientation + 90; 
							break;
					}
				}
			}
			*

            base.OnOrientationChanged(e);
        }
		*/
		
		protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
    	{
			if( cam != null )
			{
				cam.Dispose();
			}
		}

		//BUG: Not called by sys...
		/*
		private void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
		{
			FillOpts();
		}
		*/
		private void m_tmrInit_Tick(object sender, System.EventArgs e)
		{
			try
			{
				//WP81 FIX...
				//cam.IsFlashModeSupported(FlashMode.Off);
				bool bTest = cam.FlashMode == FlashMode.Off;
			}
			catch( Exception )
			{
				return;
			}
			
			m_tmrInit.Stop();
			
			FillOpts();
		}

		private void cam_AutoFocusCompleted(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
		{
			cam.CaptureImage();
		}

		private void cam_CaptureCompleted(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
		{
		}

		private void cam_CaptureThumbnailAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
		{
			if( m_sFnDtNext.Length == 0 )
			{
				DateTime dNow = DateTime.Now;
				m_sFnDtNext = dNow.Year.ToString() + RscUtils.pad60(dNow.Month) 
					+ RscUtils.pad60(dNow.Day) + "_" + RscUtils.pad60(dNow.Hour) + RscUtils.pad60(dNow.Minute) 
					+ RscUtils.pad60(dNow.Second);
			}
			
			if( !m_bSaveToMediaLibrary )
			{
				RscStore store = new RscStore();
				
				string fName = RscKnownFolders.GetMediaPath("DCIM", "tn") + "\\" + m_sFnDtNext + ".tn" + ".jpg";
				int iCnt = 0;
				for(;;)
				{
					if( !store.FileExists(fName) ) break;
					iCnt++;
					fName = RscKnownFolders.GetMediaPath("DCIM", "tn") + "\\" + m_sFnDtNext + "_" + iCnt.ToString() + ".tn" + ".jpg";
				}
				
				System.IO.Stream stream = store.CreateFile(fName);
				// Initialize the buffer for 4KB disk pages.
				byte[] readBuffer = new byte[4096];
				int bytesRead = -1;
				// Copy the thumbnail to the local folder. 
				while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
				{
					stream.Write(readBuffer, 0, bytesRead);
				}
				stream.Close();
			}

		}

		private void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
		{
			if( m_sFnDtNext.Length == 0 )
			{
				DateTime dNow = DateTime.Now;
				m_sFnDtNext = dNow.Year.ToString() + RscUtils.pad60(dNow.Month) 
					+ RscUtils.pad60(dNow.Day) + "_" + RscUtils.pad60(dNow.Hour) + RscUtils.pad60(dNow.Minute) 
					+ RscUtils.pad60(dNow.Second);
			}
			
			if( m_bSaveToMediaLibrary )
			{
				MediaLibrary media = new MediaLibrary();
				media.SavePicture( m_sFnDtNext, e.ImageStream );
				
				e.ImageStream.Seek(0, System.IO.SeekOrigin.Begin);
			}
			else
			{
				RscStore store = new RscStore();
				
				string fName = RscKnownFolders.GetMediaPath("DCIM") + "\\" + m_sFnDtNext + ".jpg";
				int iCnt = 0;
				for(;;)
				{
					if( !store.FileExists(fName) ) break;
					iCnt++;
					fName = RscKnownFolders.GetMediaPath("DCIM") + "\\" + m_sFnDtNext + "_" + iCnt.ToString() + ".jpg";
				}
				
				System.IO.Stream stream = store.CreateFile(fName);
				// Initialize the buffer for 4KB disk pages.
				byte[] readBuffer = new byte[4096];
				int bytesRead = -1;
				// Copy the thumbnail to the local folder. 
				while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
				{
					stream.Write(readBuffer, 0, bytesRead);
				}
				stream.Close();
			}

		}
		
		private void m_btnFocus_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			try
			{
				cam.Focus();
				m_sFnDtNext = "";
				
				BtnBk.Fill = new SolidColorBrush( Colors.Green );
			}
			catch( Exception )
			{
				BtnBk.Fill = new SolidColorBrush( Colors.Red );
			}
		}
		
		private void m_btnShutter_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			try
			{
				cam.CaptureImage();
				m_sFnDtNext = "";
				
				BtnBk.Fill = new SolidColorBrush( Colors.Green );
			}
			catch( Exception )
			{
				BtnBk.Fill = new SolidColorBrush( Colors.Red );
			}
		}
		
		private void FillOpts()
		{
			
			RscOptItemDesc itHead;
			RscOptItemDesc it;
			
			// //
			//
				
			spOpts.Children.Clear();
			
			itHead = new RscOptItemDesc();
			itHead.m_strTitle = "Save to";
			AddOpt(itHead);
			
			{
				it = new RscOptItemDesc(itHead);
				it.m_strTitle = "Media Library";
				it.m_bCurrent = (m_bSaveToMediaLibrary);
				it.Tag = true;
				AddOpt(it);
			}
			{
				it = new RscOptItemDesc(itHead);
				it.m_strTitle = "Isolated Storage";
				it.m_bCurrent = (!m_bSaveToMediaLibrary);
				it.Tag = false;
				AddOpt(it);
			}
			
			//
			// //
			//
			
			itHead = new RscOptItemDesc();
			itHead.m_strTitle = "Flash";
			AddOpt(itHead);
			
			if( cam.IsFlashModeSupported(FlashMode.Off) )
			{
				it = new RscOptItemDesc(itHead);
				it.m_strTitle = "Off";
				it.m_bCurrent = (cam.FlashMode == FlashMode.Off);
				it.Tag = FlashMode.Off;
				AddOpt(it);
			}
			if( cam.IsFlashModeSupported(FlashMode.On) )
			{
				it = new RscOptItemDesc(itHead);
				it.m_strTitle = "On";
				it.m_bCurrent = (cam.FlashMode == FlashMode.On);
				it.Tag = FlashMode.On;
				AddOpt(it);
			}
			if( cam.IsFlashModeSupported(FlashMode.Auto) )
			{
				it = new RscOptItemDesc(itHead);
				it.m_strTitle = "Auto";
				it.m_bCurrent = (cam.FlashMode == FlashMode.Auto);
				it.Tag = FlashMode.Auto;
				AddOpt(it);
			}
			if( cam.IsFlashModeSupported(FlashMode.RedEyeReduction) )
			{
				it = new RscOptItemDesc(itHead);
				it.m_strTitle = "Red Eye";
				it.m_bCurrent = (cam.FlashMode == FlashMode.RedEyeReduction);
				it.Tag = FlashMode.RedEyeReduction;
				AddOpt(it);
			}
			
			//
			// //
			//
			
			itHead = new RscOptItemDesc();
			itHead.m_strTitle = "Resolution";
			AddOpt(itHead);
			
			var resolutions = cam.AvailableResolutions.ToList();
			for( int i = 0; i < resolutions.Count; i++ )
			{
				it = new RscOptItemDesc(itHead);
				it.m_strTitle = resolutions[i].Width.ToString() + "x" + resolutions[i].Height.ToString();
				it.m_bCurrent = (cam.Resolution == resolutions[i]);
				it.Tag = resolutions[i];
				AddOpt(it);
			}
			
			//
			// //
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
				
				case "Save to" :
				{
					m_bSaveToMediaLibrary = (bool) it.Tag;
			
					it.m_imgChb.Image.Source = m_isChbOn;
					
					break;
				}
				
				case "Flash" :
				{
					
					cam.FlashMode = (Microsoft.Devices.FlashMode) it.Tag;
			
					it.m_imgChb.Image.Source = m_isChbOn;
					
					break;
				}
					
				case "Resolution" :
				{
					
					cam.Resolution = (System.Windows.Size) it.Tag;
			
					it.m_imgChb.Image.Source = m_isChbOn;
					
					break;
				}
					
			}
		}

		private void m_btnTools_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sUri = "/Launcher_AppMini;component/" + "MainPage" + ".xaml"; //+ "?NoIe=True";
			NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}

		private void m_btnVid_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sUri = "/RscCamera;component/" + "RscDC_CamcorderV11" + ".xaml";
			NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}

		private void m_btnFldr_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sUri = "/Lib_RscViewers;component/" + "RscViewer_PhotoFolderV10" + ".xaml";
			NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}
		
    }
	
}
