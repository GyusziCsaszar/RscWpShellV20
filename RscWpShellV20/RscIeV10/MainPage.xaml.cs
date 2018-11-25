using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using RscIeV10.Resources;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Threading;

using Ressive.Utils;
using Ressive.Store;
using Ressive.FrameWork;
using Ressive.Theme;
using Ressive.InterPageCommunication;

using Ressive.ShellTiles;

namespace RscIeV10
{
		
    public partial class MainPage : PhoneApplicationPage
    {
		
		const string csAppTitle = "IE";
		
		const string csViewersAssy = "Lib_RscViewers";
		const string csIPgC_DlgsAssy = "Lib_RscIPgC_Dlgs";
		
		const string csClsName = "RscIeV10";
		
		const string csUaMark = "UA=";
		
		const double cdFsText = 20;
		const double cdFsTx = 16;
		
        Stack<Uri> _navigationStack = new Stack<Uri>();
		
		RscTheme m_Theme = null;
		// //
		ImageSource m_isIe = null;
		ImageSource m_isDelete = null;
		ImageSource m_isAddTile = null;
		ImageSource m_isDelTile = null;
		ImageSource m_isOn = null;
		ImageSource m_isOff = null;
		ImageSource m_isWallPort = null;
		ImageSource m_isWallLand = null;
		// //
		RscIconButton m_btnTools = null;
		// //
		TextBoxDenieEdit m_txtStatus;
		// //
		ImageSource m_isU = null;
		ImageSource m_isD = null;
		RscIconButton m_btnUd = null;
		// //
		ImageSource m_isDownload = null;
		ImageSource m_isDownloadCancel = null;
		RscIconButton m_btnDownload = null;
		RscIconButton m_btnDownFolder = null;
		TextBoxDenieEdit m_txtDownPrs;
		TextBoxDenieEdit m_txtDownload;
		// //
		TextBoxDenieEdit m_txtFreeMem;
		RscIconButton m_btnExit = null;
		TextBoxDenieEdit m_txtUaID;
		RscIconButton m_btnStop = null;
		RscIconButton m_btnTriBack = null;
		RscIconButton m_btnDblBack = null;
		RscIconButton m_btnBack = null;
		// //
		RscIconButton m_btnSaveBm = null;
		// //
		RscIconButton m_btnGo = null;
		RscIconButton m_btnBm = null;
		// //
		TextBoxDenieEdit m_txtLog;
		RscIconButton m_btnHome = null;
		RscIconButton m_btnHis = null;
		RscIconButton m_btnClearCache = null;
		RscIconButton m_btnUserAgent = null;
		RscIconButton m_btnCopyLast = null;
		ImageSource m_isEmptyOff = null;
		ImageSource m_isEmptyOn = null;
		RscIconButton m_btnNight = null;
		RscIconButton m_btnBms = null;
		
		RscPageArgsRetManager m_AppArgs;
		
		bool m_bPopOnBack = true;
		
		bool m_bAppStarted = false;
				
		System.Net.WebClient m_client = null;
		
		bool m_bStandAloneApp = false;
		
		DispatcherTimer m_FreeMemTmr;
		
		RscBrowserBroker m_bb = null;
		
		bool m_BookmarksVisible = false;
		bool m_HistoryVisible = false;
		
		Size m_sContentPanel = new Size(100, 100);
		
		Uri m_UriAutoStart = null;
		
		DispatcherTimer m_tmrLoadBookmarks;
		
		RscSimpleLbItemList m_aRows = null;

        public MainPage()
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
			
						
			//Win10Mo DENIES!!!
			//ChangeUserAgent( csUserAgent_Wp71 );
 			
			bool bNightMode = RscRegistry.ReadBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\" + csClsName + "\\Settings",
				"NightMode", false );

			//MemUsage Optimization...
			//Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			m_Theme = (RscTheme) GlobalDILholder.Tag;
			//m_dil = new RscDefaultedImageList( "Theme", "Current", "Default" );
			// ///////////////
			m_isIe = m_Theme.GetImage("Images/Ico001_IE.jpg");
			m_isDelete = m_Theme.GetImage("Images/Btn001_Delete.jpg");
			m_isAddTile = m_Theme.GetImage("Images/Btn001_AddTile.jpg");
			m_isDelTile = m_Theme.GetImage("Images/Btn001_DelTile.jpg");
			m_isOn = m_Theme.GetImage("Images/CheckOn.jpg");
			m_isOff = m_Theme.GetImage("Images/CheckOff.jpg");
			//
			//NightMode requires solid Black bk!!!
			//
			//m_isWallPort = m_Theme.GetImage("Images/Bk001_portrait.jpg");
			//m_isWallLand = m_Theme.GetImage("Images/Bk001_landscape.jpg");
			m_isWallPort = m_Theme.GetImage("Images/BlackBlank.png");
			m_isWallLand = m_Theme.GetImage("Images/BlackBlank.png");
			
			m_AppArgs = new RscPageArgsRetManager();
			
			if( m_bStandAloneApp )
			{		
				m_btnTools = new RscIconButton(ExPanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible);
				m_btnTools.Image.Source = m_Theme.GetImage("Images/IcoSm001_LauncherMini.jpg");
				m_btnTools.Click += new System.Windows.RoutedEventHandler(m_btnTools_Click);
			}
			
			m_isU = m_Theme.GetImage("Images/Btn001_UpLess.jpg");
			m_isD = m_Theme.GetImage("Images/Btn001_DownMore.jpg");
			m_btnUd = new RscIconButton(UdPanel, Grid.ColumnProperty, 1, 50, 50, Rsc.Visible);
			m_btnUd.Image.Source = m_isU;
			m_btnUd.Click += new System.Windows.RoutedEventHandler(m_btnUd_Click);
			
			m_txtStatus = new TextBoxDenieEdit(true, true, StatusPanel, Grid.RowProperty, 0);
			m_txtStatus.Background = new SolidColorBrush(Colors.Gray);
			m_txtStatus.Foreground = new SolidColorBrush(Colors.White);
			m_txtStatus.FontSize = cdFsTx;
			m_txtStatus.Opacity = 0.8;
			m_txtStatus.MarginOffset = new Thickness( 6 );
			m_txtStatus.Text = "Ready...";
			
			m_isDownload = m_Theme.GetImage("Images/Btn001_Download.jpg");
			m_isDownloadCancel = m_Theme.GetImage("Images/Btn001_Delete.jpg");
			m_btnDownload = new RscIconButton(DnPanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible);
			m_btnDownload.Image.Source = m_isDownload;
			m_btnDownload.Click += new System.Windows.RoutedEventHandler(m_btnDownload_Click);
			
			m_btnDownFolder = new RscIconButton(DnPanel, Grid.ColumnProperty, 1, 50, 50, Rsc.Visible,
				4 );
			m_btnDownFolder.Image.Source = m_Theme.GetImage("Images/Btn001_(dir).jpg");
			m_btnDownFolder.Click += new System.Windows.RoutedEventHandler(m_btnDownFolder_Click);
			
			m_txtDownPrs = new TextBoxDenieEdit(true, true, DnPanel, Grid.ColumnProperty, 2);
			m_txtDownPrs.Background = new SolidColorBrush(m_Theme.ThemeColors.TextDarkBack);
			m_txtDownPrs.Foreground = new SolidColorBrush(m_Theme.ThemeColors.TextDarkFore);
			m_txtDownPrs.FontSize = cdFsTx;
			m_txtDownPrs.Opacity = 0.9;
			m_txtDownPrs.MarginOffset = new Thickness( 6 );
			m_txtDownPrs.Visibility = Rsc.Collapsed;
			m_txtDownPrs.Text = "";
			
			m_txtDownload = new TextBoxDenieEdit(true, true, DnPanel, Grid.ColumnProperty, 3);
			m_txtDownload.Background = new SolidColorBrush(m_Theme.ThemeColors.TextDarkBack);
			m_txtDownload.Foreground = new SolidColorBrush(m_Theme.ThemeColors.TextDarkFore);
			m_txtDownload.FontSize = cdFsTx;
			m_txtDownload.Opacity = 0.9;
			m_txtDownload.MarginOffset = new Thickness( 6 );
			m_txtDownload.Visibility = Rsc.Collapsed;
			m_txtDownload.Text = "";
			
			m_txtFreeMem = new TextBoxDenieEdit(true, true, ExPanel, Grid.ColumnProperty, 1);
			m_txtFreeMem.Background = new SolidColorBrush(m_Theme.ThemeColors.TextDarkBack);
			m_txtFreeMem.Foreground = new SolidColorBrush(m_Theme.ThemeColors.TextDarkFore);
			m_txtFreeMem.FontSize = cdFsTx;
			m_txtFreeMem.Opacity = 0.9;
			m_txtFreeMem.MarginOffset = new Thickness( 6 );
			m_txtFreeMem.Visibility = Rsc.Collapsed;
			m_txtFreeMem.Text = "";
			
			m_btnExit = new RscIconButton(ExPanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Visible);
			m_btnExit.Image.Source = m_Theme.GetImage("Images/Btn001_Close.jpg");
			m_btnExit.Click += new System.Windows.RoutedEventHandler(m_btnExit_Click);
			
			m_txtUaID = new TextBoxDenieEdit(true, true, ExPanel, Grid.ColumnProperty, 3);
			m_txtUaID.Background = new SolidColorBrush(m_Theme.ThemeColors.TextDarkBack);
			m_txtUaID.Foreground = new SolidColorBrush(m_Theme.ThemeColors.TextDarkFore);
			m_txtUaID.FontSize = cdFsTx;
			m_txtUaID.Opacity = 0.9;
			m_txtUaID.MarginOffset = new Thickness( 6 );
			m_txtUaID.Visibility = Rsc.Collapsed;
			m_txtUaID.Text = "";
			
			m_btnStop = new RscIconButton(ExPanel, Grid.ColumnProperty, 5, 50, 50, Rsc.Collapsed);
			m_btnStop.Image.Source = m_Theme.GetImage("Images/Btn001_Delete.jpg");
			m_btnStop.Click += new System.Windows.RoutedEventHandler(m_btnStop_Click);
			
			m_btnTriBack = new RscIconButton(ExPanel, Grid.ColumnProperty, 6, 50, 50, Rsc.Collapsed);
			m_btnTriBack.Image.Source = m_Theme.GetImage("Images/Btn001_Back.jpg");
			m_btnTriBack.Click += new System.Windows.RoutedEventHandler(m_btnTriBack_Click);
			
			m_btnDblBack = new RscIconButton(ExPanel, Grid.ColumnProperty, 7, 50, 50, Rsc.Collapsed);
			m_btnDblBack.Image.Source = m_Theme.GetImage("Images/Btn001_Back.jpg");
			m_btnDblBack.Click += new System.Windows.RoutedEventHandler(m_btnDblBack_Click);
			
			m_btnBack = new RscIconButton(ExPanel, Grid.ColumnProperty, 8, 50, 50, Rsc.Collapsed);
			m_btnBack.Image.Source = m_Theme.GetImage("Images/Btn001_Back.jpg");
			m_btnBack.Click += new System.Windows.RoutedEventHandler(m_btnBack_Click);
			
			m_btnSaveBm = new RscIconButton(BmPanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Visible);
			m_btnSaveBm.Image.Source = m_Theme.GetImage("Images/Btn001_Save.jpg");
			m_btnSaveBm.Click += new System.Windows.RoutedEventHandler(m_btnSaveBm_Click);
			
			m_btnGo = new RscIconButton(UriPanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Visible);
			m_btnGo.Image.Source = m_Theme.GetImage("Images/Btn001_Next.jpg");
			m_btnGo.Click += new System.Windows.RoutedEventHandler(m_btnGo_Click);
			
			m_btnBm = new RscIconButton(UriPanel, Grid.ColumnProperty, 3, 50, 50, Rsc.Collapsed);
			m_btnBm.Image.Source = m_Theme.GetImage("Images/Btn001_BookMark.jpg");
			m_btnBm.Click += new System.Windows.RoutedEventHandler(m_btnBm_Click);
			
			m_txtLog = new TextBoxDenieEdit(true, true, GoPanel, Grid.RowProperty, 0);
			m_txtLog.Background = new SolidColorBrush(Colors.Gray);
			m_txtLog.Foreground = new SolidColorBrush(Colors.White);
			m_txtLog.FontSize = cdFsTx;
			m_txtLog.Opacity = 0.9;
			m_txtLog.MarginOffset = new Thickness( 12 );
			m_txtLog.Text = "Ready...";
			
			m_btnHome = new RscIconButton(MorePanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible);
			m_btnHome.Image.Source = m_Theme.GetImage("Images/Btn001_Home.jpg");
			m_btnHome.Click += new System.Windows.RoutedEventHandler(m_btnHome_Click);
			
			m_btnHis = new RscIconButton(MorePanel, Grid.ColumnProperty, 1, 50, 50, Rsc.Visible,
				0, 0, "History" );
			m_btnHis.Image.Source = m_Theme.GetImage("Images/Btn001_Empty.jpg");
			m_btnHis.Click += new System.Windows.RoutedEventHandler(m_btnHis_Click);
			
			m_btnClearCache = new RscIconButton(MorePanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Visible);
			m_btnClearCache.Image.Source = m_Theme.GetImage("Images/Btn001_ClearCache.jpg");
			m_btnClearCache.Click += new System.Windows.RoutedEventHandler(m_btnClearCache_Click);
			
			m_btnUserAgent = new RscIconButton(MorePanel, Grid.ColumnProperty, 3, 50, 50, Rsc.Visible,
				0, 0, "User Agent" );
			m_btnUserAgent.Image.Source = m_Theme.GetImage("Images/Btn001_Empty.jpg");
			m_btnUserAgent.Click += new System.Windows.RoutedEventHandler(m_btnUserAgent_Click);
			
			m_btnCopyLast = new RscIconButton(MorePanel, Grid.ColumnProperty, 4, 50, 50, Rsc.Visible);
			m_btnCopyLast.Image.Source = m_Theme.GetImage("Images/Btn001_Copy.jpg");
			m_btnCopyLast.Click += new System.Windows.RoutedEventHandler(m_btnCopyLast_Click);
			
			m_isEmptyOff = m_Theme.GetImage("Images/Btn001_EmptyOff.jpg");
			m_isEmptyOn = m_Theme.GetImage("Images/Btn001_EmptyOn.jpg");
			m_btnNight = new RscIconButton(MorePanel, Grid.ColumnProperty, 5, 50, 50, Rsc.Visible,
				0, 0, "Night" );
			if( bNightMode )
				m_btnNight.Image.Source = m_isEmptyOn;
			else
				m_btnNight.Image.Source = m_isEmptyOff;
			m_btnNight.Click += new System.Windows.RoutedEventHandler(m_btnNight_Click);
			
			m_btnBms = new RscIconButton(MorePanel, Grid.ColumnProperty, 6, 50, 50, Rsc.Visible);
			m_btnBms.Image.Source = m_Theme.GetImage("Images/Btn001_BookMark.jpg");
			m_btnBms.Click += new System.Windows.RoutedEventHandler(m_btnBms_Click);
			
			{
				wbc1.ScriptNotify += new EventHandler<NotifyEventArgs>(JavaScriptNotify);
				wbc1.Navigating += new EventHandler<NavigatingEventArgs>(WebControlNavigating);
				wbc1.NavigationFailed += new NavigationFailedEventHandler(WebControlNavigationFailed);
				wbc1.Navigated += new EventHandler<NavigationEventArgs>(WebControlNavigated);
				
				wbc1.IsScriptEnabled = true;
				wbc1.IsGeolocationEnabled = true;
				
				//NOT WORKING...
				//wbc1.Loaded += new RoutedEventHandler(browser_Loaded);
			}
			{
				wbc2.ScriptNotify += new EventHandler<NotifyEventArgs>(JavaScriptNotify);
				wbc2.Navigating += new EventHandler<NavigatingEventArgs>(WebControlNavigating);
				wbc2.NavigationFailed += new NavigationFailedEventHandler(WebControlNavigationFailed);
				wbc2.Navigated += new EventHandler<NavigationEventArgs>(WebControlNavigated);
				
				wbc2.IsScriptEnabled = true;
				wbc2.IsGeolocationEnabled = true;
				
				//NOT WORKING...
				//wbc2.Loaded += new RoutedEventHandler(browser_Loaded);
			}
			
			m_bb = new RscBrowserBroker( wbc1, wbc2 );
			m_bb.NightMode = bNightMode;
			m_bb.ApplyNightMode( wbc1 );
			m_bb.ApplyNightMode( wbc2 );

            //MyNavigate(new Uri("http://m.facebook.com", UriKind.Absolute));
			//UriPanel.Visibility = Rsc.Collapsed;
			
			m_bPopOnBack = true;
			
			UpdateFreeMemInfo();
			
			m_FreeMemTmr = new DispatcherTimer();
			m_FreeMemTmr.Interval = System.TimeSpan.FromSeconds(2);
			m_FreeMemTmr.Tick += new System.EventHandler(m_FreeMemTmr_Tick);
			m_FreeMemTmr.Start();
			
			m_tmrLoadBookmarks = new DispatcherTimer();
			m_tmrLoadBookmarks.Tick += new System.EventHandler(m_tmrLoadBookmarks_Tick);
			
 			LayoutRoot.SizeChanged += new System.Windows.SizeChangedEventHandler(LayoutRoot_SizeChanged);
			
			m_aRows = new RscSimpleLbItemList( lbRows, m_Theme );
      	}
		
		private void m_FreeMemTmr_Tick(object sender, System.EventArgs e)
		{
			UpdateFreeMemInfo();
		}

		private void LayoutRoot_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			LoadWallImg( e.NewSize );
		}
		
		private void LoadWallImg( Size sz, bool bForce = false )
		{
			bool bNoChng = (m_sContentPanel.Width == sz.Width && m_sContentPanel.Height == sz.Height);
			m_sContentPanel = sz;
			
			if( !bNoChng || bForce )
			{
				if( sz.Width < sz.Height )
					imgBk.Source = m_isWallPort;
				else
					imgBk.Source = m_isWallLand;
			}
		}
		
		protected override void OnNavigatedTo(NavigationEventArgs args)
		{
			if( m_AppArgs.Waiting )
			{				
				//RscStore store = new RscStore();
				
				RscPageArgsRet appOutput = m_AppArgs.GetOutput();
				if( appOutput != null )
				{
					switch( appOutput.ID )
					{
						
						case "ClearCache" :
						{
							
							m_AppArgs.Vipe();
							
							//Tools called...
							
							m_AppArgs.Clear();

							MessageBox.Show( "Viping browser cache done!" );
							
							break;
						}
							
						case "txDownFn" :
						{
							string sPathOk = m_sDownloadedPath;
							
							if( appOutput.GetFlag(0) == "Ok" )
							{
								string sNewFn = appOutput.GetData(0);
								
								RscStore store = new RscStore();
								
								string sNewPath = RscStore.PathOfPath( m_sDownloadedPath );
								sNewPath += "\\" + RscStore.FileNameOfPath( m_sDownloadedPath );
								sNewPath += "_" + sNewFn;
								
								if( RscStore.ExtensionOfPath( sNewPath ).Length == 0 )
								{
									sNewPath += RscStore.ExtensionOfPath( m_sDownloadedPath );
								}
								
								store.MoveFileForce( m_sDownloadedPath, sNewPath );
								
								sPathOk = sNewPath;
							}
							
							m_AppArgs.Clear();
							
							MessageBox.Show( "Download succeeded!\n\nFile: " + sPathOk );
							
							break;
						}
							
					}
				}
			}
			
			IDictionary<string, string> parameters = this.NavigationContext.QueryString;
			
			if( parameters.ContainsKey( "uri" ) )
			{
				if( !m_bAppStarted ) //FIX: Returning from ThumbStuning, Dlgs issues...
				{
					string sUri;
					
					sUri = parameters["uri"];
					
					sUri = sUri.Replace("(HASH)", "#");
					
					if( sUri.IndexOf( "://" ) < 0 )
						sUri = "http://" + sUri;
					
					m_UriAutoStart = new Uri(sUri, UriKind.Absolute);
					ShowUserAgentList( m_UriAutoStart.DnsSafeHost );
					//m_bb.Navigate(new Uri(sUri, UriKind.Absolute));
				}
			}
			else if( parameters.ContainsKey( "IcoGd" ) )
			{
				if( !m_bAppStarted ) //FIX: Returning from ThumbStuning, Dlgs issues...
				{
					string gdStr = parameters["IcoGd"];
						
					string sStFldr = RscKnownFolders.GetTempPath("ShellTiles", "" );
					bool bTmp;
					RscStore store = new RscStore();
					string sTxt = store.ReadTextFile( sStFldr + "\\" + gdStr + ".txt", "", out bTmp );
					if( sTxt.Length > 0 )
					{
						string [] astr = sTxt.Split( new String [] {"\r\n"}, StringSplitOptions.None);
						string sUri = "";
						if( astr.Length > 1 )
							sUri = astr[1];
						
						if( sUri.IndexOf( "://" ) < 0 )
							sUri = "http://" + sUri;
						
						m_UriAutoStart = new Uri(sUri, UriKind.Absolute);
						ShowUserAgentList( m_UriAutoStart.DnsSafeHost );
						//m_bb.Navigate(new Uri(sUri, UriKind.Absolute));
					}
				}
			}
			else
			{
				RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
				RscPageArgsRet appInput = appArgsMgr.GetInput( "MainPage" );
				if( appInput != null )
				{
					
					//ApplicationTitle.Text = appInput.CallerAppTitle;
					//AppIcon.Source = m_dil.GetImage(appInput.CallerAppIconRes);
					
					string sUri = appInput.GetData( 0 );
					
					//FAILS!!!
					/*
					if( sUri[ 1 ] == ':' ) //Local file...
					{
						/*
						sUri = "file:///" + sUri.Replace( '\\', '/' );
						
						sUri = sUri.Replace( "A:", RscStore_Storage.IsoStoreDevicePath().Replace( '\\', '/' ) );
						sUri = sUri.Replace( "a:", RscStore_Storage.IsoStoreDevicePath().Replace( '\\', '/' ) );
						
						MessageBox.Show( sUri );
						
						//wbc.NavigateToString( sUri );
						//wbc.NavigateToString( "<A HREF=\"" + sUri + "\">" + sUri + "</A>" );
						Microsoft.Phone.Tasks.WebBrowserTask wbt = new Microsoft.Phone.Tasks.WebBrowserTask();
						//wbt.URL = sUri;
						wbt.Uri = new Uri(sUri, UriKind.Absolute);
						wbt.Show();
						*
						
						if( sUri.Substring(0, 2 ) == "A:" )
						{
							sUri = sUri.Substring( 3 );
							sUri = sUri.Replace( '\\', '/' );
							
							//MessageBox.Show( sUri );
							
							MyNavigate(new Uri(sUri, UriKind.Relative));
							
							//FAILED...
							/*
							Microsoft.Phone.Tasks.WebBrowserTask wbt = new Microsoft.Phone.Tasks.WebBrowserTask();
							//wbt.URL = sUri;
							wbt.Uri = new Uri(sUri, UriKind.Relative);
							wbt.Show();
							*
						}
					}
					else
					*/
					{
						if( sUri.IndexOf( "://" ) < 0 )
							sUri = "http://" + sUri;
						
						m_UriAutoStart = new Uri(sUri, UriKind.Absolute);
						ShowUserAgentList( m_UriAutoStart.DnsSafeHost );
						//m_bb.Navigate(new Uri(sUri, UriKind.Absolute));
					}
					
					
					appArgsMgr.Vipe();
				}
				else
				{
					if( !m_bAppStarted ) //FIX: Returning from ThumbStuning, Dlgs issues...
					{
						//Debug...
						//_navigationStack.Push( new Uri("http://google.com", UriKind.Absolute) );
						//_navigationStack.Push( new Uri("http://cnn.com", UriKind.Absolute) );
						//_navigationStack.Push( new Uri("http://mno.com", UriKind.Absolute) );
						
						UriPanel.Visibility = Rsc.Visible;
						GoPanel.Visibility = Rsc.Visible;
					}
				}
			}
			
			if( m_bAppStarted ) //FIX: Refresh Bookmark list...
			{
				if( scrl.Visibility == Rsc.Visible )
				{
					if( m_BookmarksVisible )
					{
						ShowBookmarkList( );
					}
					if( m_HistoryVisible )
					{
						ShowHistoryList();
					}
				}
			}
			
			base.OnNavigatedTo(args);
			
			m_bAppStarted = true;
		}

        void JavaScriptNotify(Object sender, NotifyEventArgs notifyArgs)
        {
            m_txtLog.Text = "JavaScriptNotify: " + notifyArgs.Value;
        }

		private void WebControlNavigating(object sender, NavigatingEventArgs navArgs)
		{
			if( RscBrowserSettings.MarkedOpenExternal( navArgs.Uri ) )
			{
				//MessageBox.Show( navArgs.Uri.DnsSafeHost );
				
				navArgs.Cancel = true;
				
				DoStop();
				
				if( !RscStore_Storage.LaunchUri( navArgs.Uri ) )
				{
					MessageBox.Show( "Unable to launch uri " + navArgs.Uri.ToString() + " !!!" );
				}
				
				return;
			}
			
			//User Agent switching on the go...
			string sUaID = "x";
			sUaID = RscBrowserSettings.LoadUserAgentID( navArgs.Uri, sUaID );
			if( sUaID != "x" )
			{
				m_txtUaID.Visibility = Rsc.Visible;
				m_txtUaID.Text = RscUserAgents.DecorateUserAgentID( sUaID, true );
				
				m_bb.SetUserAgentID( sUaID );
			}
			else
			{
				if( m_bb.UserAgentID.Length == 0 )
				{
					m_txtUaID.Visibility = Rsc.Collapsed;
				}
				else
				{
					m_txtUaID.Visibility = Rsc.Visible;
					m_txtUaID.Text = RscUserAgents.DecorateUserAgentID( m_bb.UserAgentID, true ) + " (inh.)";
				}
			}
			
			bool bCancel;
			if( !m_bb.OnNavigating( sender as WebBrowser, navArgs.Uri, out bCancel ) )
			{
				if( bCancel )
				{
					navArgs.Cancel = true;
					
					if( m_bb.HasUserAgent )
					{
						WriteableBitmap wbmp = new WriteableBitmap(480, 800);
						wbmp.Render(sender as WebBrowser, null);
						wbmp.Invalidate();

						imgBk.Source = wbmp;
					}
				}
				
				return;
			}
			
			//txLog.Text = "Nav Started: " + navArgs.Uri.ToString();
			m_txtStatus.Text = "Loading...";
			StatusPanel.Visibility = Rsc.Visible;
			
			m_btnStop.Visibility = Rsc.Visible;
			
			//txBmName.Text = "";
			
			m_txtLog.Text = "Loading | " + navArgs.Uri.ToString();
			//GoPanel.Visibility = Rsc.Visible;
			
			UpdateFreeMemInfo();	
		}

		private void WebControlNavigationFailed(object sender, NavigationFailedEventArgs navArgs)
		{
			if( !m_bb.OnNavigated( sender as WebBrowser, navArgs.Uri, true ) )
				return;
			
			if( m_bb.HasUserAgent )
				LoadWallImg( m_sContentPanel, true );
			
			m_btnStop.Visibility = Rsc.Collapsed;
			
			StatusPanel.Visibility = Rsc.Collapsed;
			
			txBmName.Text = "";
			
			m_bPopOnBack = false;
			
			m_txtLog.Text = "Failed | " + navArgs.Uri.ToString();
			GoPanel.Visibility = Rsc.Visible;
			
			UpdateFreeMemInfo();	
		}

        void WebControlNavigated(Object sender, NavigationEventArgs navArgs)
        {
			if( !m_bb.OnNavigated( sender as WebBrowser, navArgs.Uri, false ) )
				return;
			
			if( m_bb.HasUserAgent )
				LoadWallImg( m_sContentPanel, true );
			
			m_btnStop.Visibility = Rsc.Collapsed;
			
			StatusPanel.Visibility = Rsc.Collapsed;
			
			txBmName.Text = "";
			
			if( navArgs.Uri.ToString().Length == 0 || navArgs.Uri.ToString() == "about:blank" ) //Stop button pressed...
			{
				//Stop button pressed...
				string s = m_txtLog.Text;
				int iPos = s.IndexOf( '|' );
				if( iPos >= 0 )
				{
					m_txtLog.Text = "Stopped " + s.Substring( iPos );
					
					m_bPopOnBack = true;
					His_Add( new Uri(s.Substring( iPos + 2 ), UriKind.Absolute ));
				}
			}
			else
			{
				m_bPopOnBack = true;
				
				txUri.Text = navArgs.Uri.ToString();
				
				m_txtLog.Text = "Completed | " + navArgs.Uri.ToString();
				His_Add(navArgs.Uri);
			}
			
			//Not working...
			//TurnOnNightMode(sender);
				
			if( _navigationStack.Count > 3 )
			{
				m_btnTriBack.Visibility = Rsc.Visible;
				m_btnDblBack.Visibility = Rsc.Visible;
				m_btnBack.Visibility = Rsc.Visible;
			}
			else if( _navigationStack.Count > 2 )
			{
				m_btnTriBack.Visibility = Rsc.Collapsed;
				m_btnDblBack.Visibility = Rsc.Visible;
				m_btnBack.Visibility = Rsc.Visible;
			}
			else if( _navigationStack.Count > 1 )
			{
				m_btnTriBack.Visibility = Rsc.Collapsed;
				m_btnDblBack.Visibility = Rsc.Collapsed;
				m_btnBack.Visibility = Rsc.Visible;
			}
			else
			{
				m_btnTriBack.Visibility = Rsc.Collapsed;
				m_btnDblBack.Visibility = Rsc.Collapsed;
				m_btnBack.Visibility = Rsc.Collapsed;
			}
			
			m_btnBm.Visibility = Rsc.Visible;
				
			UpdateFreeMemInfo();	
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( scrl.Visibility == Rsc.Visible )
			{
				scrl.Visibility = Rsc.Collapsed;
				m_bb.Visibility = Rsc.Visible;
				
				if( m_UriAutoStart != null )
				{
					Uri uri = m_UriAutoStart;
					m_UriAutoStart = null;
					
					if( m_sUaAlertDomains_TMP.Length > 0 )
					{
						string sLst = m_sUaAlertDomains_TMP;
						m_sUaAlertDomains_TMP = "";
						
						if( sLst.IndexOf( ";" + uri.DnsSafeHost.ToLower() ) >= 0 )
							MessageBox.Show( "NOTE: User Agent selection may not be applied to pages visited in this session, app restart required!" );
					}
				
					RscBrowserSettings.StoreUserAgentID( uri, /*sUaID*/ "" );

					m_bb.Navigate( uri );
				}
				
				e.Cancel = true;
			}
			else
			{

			//if( _navigationStack.Count() >= 2 )
            //{
				if( GoPanel.Visibility == Rsc.Collapsed )
				{
					UriPanel.Visibility = Rsc.Visible;
					GoPanel.Visibility = Rsc.Visible;
				}
				else
				{
					UriPanel.Visibility = Rsc.Collapsed;
					BmPanel.Visibility = Rsc.Collapsed;
					GoPanel.Visibility = Rsc.Collapsed;
				}
				
				e.Cancel = true;
            //}
				
			}
        }
				
		private void m_btnExit_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_client != null )
			{
				MessageBox.Show( "To exit, cancel downlad first!" );
			}
			else
			{
				this.NavigationService.GoBack();
			}
		}

		private void m_btnTools_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sUri = "/Launcher_AppMini;component/" + "MainPage" + ".xaml?NoIe=True";
			NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}
				
		private void m_btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoStop();
		}
		
		private void DoStop()
		{
			m_bPopOnBack = false;
			
			m_bb.NavigateToNull();
		}
				
		private void m_btnHome_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( _navigationStack.Count() > 0 )
			{
				txUri.Text = _navigationStack.ElementAt(_navigationStack.Count() - 1).AbsoluteUri;
			}
			
			UriPanel.Visibility = Rsc.Visible;
		}
				
		private void m_btnTriBack_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoBack(2);
		}
		private void m_btnDblBack_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoBack(1);
		}
		private void m_btnBack_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoBack(0);
		}
		private void DoBack( int iPlus )
		{
            if (_navigationStack.Count() > (1 + iPlus) )
            {
				if( m_bPopOnBack )
				{
					His_Back( (1 + iPlus) );
				}
				else
				{
					His_Back( (0 + iPlus) );
				}
			
				UriPanel.Visibility = Rsc.Collapsed;
				BmPanel.Visibility = Rsc.Collapsed;
				GoPanel.Visibility = Rsc.Collapsed;
				
				m_btnTriBack.Visibility = Rsc.Collapsed;
				m_btnDblBack.Visibility = Rsc.Collapsed;
				m_btnBack.Visibility = Rsc.Collapsed;
            }
		}
		
		private void m_btnBm_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( BmPanel.Visibility == Rsc.Visible )
				BmPanel.Visibility = Rsc.Collapsed;
			else
				BmPanel.Visibility = Rsc.Visible;
		}
		
		private void m_btnSaveBm_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			
			string sFn = txBmName.Text;
			if( sFn.Length == 0 )
			{
				MessageBox.Show( "No bookmark name specified!" );
				return;
			}				
			string strChk = "\\/:*?\"<>|";
			foreach( char cChk in strChk )
			{
				if( sFn.IndexOf( cChk ) >= 0 )
				{
					MessageBox.Show("Name must not contain characters of '" + strChk + "'!");
					return;
				}
			}
			
			string sUri = txUri.Text;
			if( sUri.Length == 0 ) return;
			if( sUri.IndexOf("://") < 0 )
				sUri = "http://" + sUri;
			
			RscStore store = new RscStore();
			
			string sPath = "A:\\Internet\\Bookmarks";
			store.CreateFolderPath( sPath );
			sPath += "\\" + sFn + ".ilnk";
			
			if( store.FileExists( sPath ) )
			{
				MessageBox.Show("Bookmark name already exists '" + sFn + "' in 'A:\\Internet\\Bookmarks' folder!");
				return;
			}
			
			store.WriteTextFile( sPath, sUri, true );
			
			MessageBox.Show( "Bookmark named '" + sFn + "' saved successfuly in folder 'A:\\Internet\\Bookmarks'!" );
			
			txBmName.Text = "";
			BmPanel.Visibility = Rsc.Collapsed;
		}
				
		private void m_btnGo_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			
			string sUri;
			
			txBmName.Text = "";
			
			sUri = txUri.Text;
			if( sUri != "" )
			{
				if( sUri.IndexOf("://") < 0 )
					sUri = "http://" + sUri;
				
				His_Clear();
				
				//Debug - History
				//His_Add( new Uri(sUri, UriKind.Absolute) );
				
				Uri uri = null;
				try{ uri = new Uri(sUri, UriKind.Absolute); }
				catch( Exception ) { MessageBox.Show( "Bad URI format!" ); return; }
				
				m_bb.Navigate(uri);
			}
			
			UriPanel.Visibility = Rsc.Collapsed;
			BmPanel.Visibility = Rsc.Collapsed;
			GoPanel.Visibility = Rsc.Collapsed;
			
			m_btnTriBack.Visibility = Rsc.Collapsed;
			m_btnDblBack.Visibility = Rsc.Collapsed;
			m_btnBack.Visibility = Rsc.Collapsed;
			
			m_btnBm.Visibility = Rsc.Collapsed;
		}
		
		private void ShowHistoryList()
		{
			m_aRows.Clear();
			
			List<Uri> aUris = new List<Uri>();
			foreach( Uri uri in _navigationStack )
			{
				aUris.Insert(0, uri);
			}
			foreach( Uri uri in aUris )
			{
				AddToUriList( uri.DnsSafeHost, uri.AbsoluteUri, uri.DnsSafeHost, null );
			}
			
			if( m_aRows.Count == 0 )
			{
				MessageBox.Show( "No history to list!" );
			}
			else
			{
				m_BookmarksVisible = false;
				
				m_HistoryVisible = true;
				
				if( m_bb.HasUserAgent )
					LoadWallImg( m_sContentPanel, true );
				
				UriPanel.Visibility = Rsc.Collapsed;
				BmPanel.Visibility = Rsc.Collapsed;
				GoPanel.Visibility = Rsc.Collapsed;
				
				m_bb.Visibility = Rsc.Collapsed;
				
				scrlTitle.Text = "History";
				scrl.Visibility = Rsc.Visible;
			}
		}
				
		private void m_btnHis_Click(object sender, System.Windows.RoutedEventArgs e)
		{			
			if( scrl.Visibility == Rsc.Collapsed )
			{
				ShowHistoryList();
			}
			else
			{
				m_HistoryVisible = false;
				
				UriPanel.Visibility = Rsc.Collapsed;
				BmPanel.Visibility = Rsc.Collapsed;
				GoPanel.Visibility = Rsc.Collapsed;
			
				scrl.Visibility = Rsc.Collapsed;
				m_bb.Visibility = Rsc.Visible;
			}
		}
		
		const string csBmFolder = "A:\\Internet\\Bookmarks";
		private string [] m_asFns_TMP = null;
		private bool RefreshBookmarks( bool bCallTimer )
		{
			RscStore store = new RscStore();
			
			bool bFound = false;
			if( store.FolderExists( csBmFolder ) )
			{
				m_asFns_TMP = RscSort.OrderBy(store.GetFileNames( csBmFolder, "*.ilnk" ));
				if( m_asFns_TMP.Length > 0 )
					bFound = true;
			}
			
			if( bFound || bCallTimer )
			{
				if( bCallTimer )
				{
					scrlTitle.Text = "Bookmarks";

					prsScrl.Visibility = Rsc.Visible;
					
					//Can be slow...
					m_tmrLoadBookmarks.Start();
				}
			}
			
			return bFound;
		}
		private void ShowBookmarkList( )
		{
			m_aRows.Clear();
			
			bool bFound = RefreshBookmarks( false );
			
			if( !bFound )
			{
				MessageBox.Show( "No bookmark to list!" );
			}
			else
			{
				m_HistoryVisible = false;
				
				m_BookmarksVisible = true;
				
				if( m_bb.HasUserAgent )
					LoadWallImg( m_sContentPanel, true );
			
				UriPanel.Visibility = Rsc.Collapsed;
				BmPanel.Visibility = Rsc.Collapsed;
				GoPanel.Visibility = Rsc.Collapsed;
				
				m_bb.Visibility = Rsc.Collapsed;
				
				scrlTitle.Text = "Bookmarks";
				
				prsScrl.Visibility = Rsc.Visible;
				
				scrl.Visibility = Rsc.Visible;
				
				//Can be slow...
				m_tmrLoadBookmarks.Start();
			}
		}
		
		private void m_tmrLoadBookmarks_Tick(object sender, System.EventArgs e)
		{
			m_tmrLoadBookmarks.Stop();
			
			string [] asFns = m_asFns_TMP;
			m_asFns_TMP = null;
			
			RscStore store = new RscStore();
			
			m_aRows.Clear();
			
			foreach( string sFn in asFns )
			{
				bool bTmp;
				string sUri = store.ReadTextFile( csBmFolder + "\\" + sFn, "", out bTmp );
				if( sUri.Length > 0 )
				{
					Uri uri = new Uri(sUri, UriKind.Absolute );
					AddToUriList( RscStore.FileNameOfPath( sFn ), sUri, uri.DnsSafeHost, sUri );
				}
			}
			
			scrlTitle.Text = "Bookmarks";
			prsScrl.Visibility = Rsc.Collapsed;
		}
				
		private void m_btnBms_Click(object sender, System.Windows.RoutedEventArgs e)
		{			
			if( scrl.Visibility == Rsc.Collapsed )
			{				
				ShowBookmarkList();
			}
			else
			{
				m_BookmarksVisible = false;
				
				UriPanel.Visibility = Rsc.Collapsed;
				BmPanel.Visibility = Rsc.Collapsed;
				GoPanel.Visibility = Rsc.Collapsed;
				
				scrl.Visibility = Rsc.Collapsed;
				m_bb.Visibility = Rsc.Visible;
			}
		}
		
		private void His_Back( int iCnt )
		{
			for( int i = 0; i < iCnt; i++ )
			{
                _navigationStack.Pop();
			}
			
			scrl.Visibility = Rsc.Collapsed;
			m_bb.Visibility = Rsc.Visible;
			
			m_bb.Navigate(_navigationStack.Pop());
		}
		
		private void His_Clear()
		{
			_navigationStack.Clear();
		}
		
		private void His_Add(Uri uri)
		{
			_navigationStack.Push(uri);
		}
		
		private void AddToUriList( string sTitle, string sUri, string sDomain, object oTag, bool bHighLight = false )
		{
			// //
			//
			
			int idx = m_aRows.Count;
			
			if( oTag == null )
			{
				oTag = idx;
			}
			
			bool bHistory = (oTag.GetType().ToString() != "System.String");
			
			bool bUserAgent = (sDomain.Length == 0);
			
			//
			// //
			//
			
			MySimpleLbItem it = new MySimpleLbItem( m_aRows );
			
			if( bHighLight )
			{
				it.CustomBackColor = Colors.Orange;
			}
			
			it.oTag = oTag;
			
			string sImgPath = "";
			if( !bUserAgent )
			{
				string sImgPathSm = "";
				bool bFound = false;
				/*Sm Path if any...*/ RscBrowserSettings.FaviconForDomain( sDomain, out bFound, out sImgPathSm, m_isIe );
				it.BtnCust4Img =      RscBrowserSettings.FaviconForDomain( sDomain, out bFound, out sImgPath, m_isIe, false );
				
				it.BtnCust4Vis = Rsc.Visible;
				
				if( !bHistory )
				{					
					//Add/Del tile/icon...
					it.IcoInfo = sTitle + ";" + sUri + ";" + sImgPathSm; //sImgPath;
					it.BtnCust1Vis = Rsc.Visible;
					if( HasDesktopIcon( sTitle ) )
					{
						it.BtnCust1Img = m_isDelTile;
					}
					else
					{
						it.BtnCust1Img = m_isAddTile;
					}
					
					//Marked open External
					if( RscBrowserSettings.MarkedOpenExternal( sDomain ) )
					{
						it.BtnCust2Img = m_isOn;
					}
					else
					{
						it.BtnCust2Img = m_isOff;
					}
					it.BtnCust2Vis = Rsc.Visible;
					
					//Del bookmark...
					it.BtnCust3Img = m_isDelete;
					it.BtnCust3Vis = Rsc.Visible;					
				}
			}
			
			it.Title = sTitle;
			
			it.Desc1 = sDomain;
			
			it.Desc2 = sUri;
			
			m_aRows.Insert( 0, it );
			
			//
			// //
			
			// // // //////////////////////////////////////
			
			/*
			string strDns;
			//strDns = uri.Host;
			strDns = uri.DnsSafeHost;
			*/
			
			/*
			WriteableBitmap wbmp = new WriteableBitmap(LayoutRoot,
				new System.Windows.Media.MatrixTransform());
			*/
			/*
			WriteableBitmap wbmp = new WriteableBitmap(480, 800);
			wbmp.Render(wbc, null);
			wbmp.Invalidate();
			*/
			
			Grid grdOut = new Grid();
			grdOut.Name = "grdOut_" + idx.ToString();
			grdOut.Margin = new Thickness(0, 0, 0, 4 );
			RowDefinition rd;
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdOut.RowDefinitions.Add(rd);
			if( !bUserAgent )
			{
				rd = new RowDefinition(); rd.Height = GridLength.Auto; grdOut.RowDefinitions.Add(rd);
			}
			//spRows.Children.Insert(0, grdOut);
			
			Grid grdPic = new Grid();
			grdPic.Name = "grdPic_" + idx.ToString();
			ColumnDefinition cd;
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdPic.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); grdPic.ColumnDefinitions.Add(cd);
			grdPic.SetValue(Grid.RowProperty, 0);
			grdOut.Children.Add(grdPic);
			
			//string sImgPath = "";
			if( !bUserAgent )
			{
				Image img;
				img = new Image();
				img.Margin = new Thickness(0, 0, 0, 0);
				img.Stretch = Stretch.Uniform;
				//img.Source = new BitmapImage(new Uri("Images/Bk001_portrait.jpg", UriKind.Relative));
				/*
				img.Source = wbmp;
				*/
				img.Stretch = Stretch.None;
				bool bFound = false;
				img.Source = RscBrowserSettings.FaviconForDomain( sDomain, out bFound, out sImgPath, m_isIe );
				if( !bFound )
				{
					img.Stretch = Stretch.UniformToFill;
					img.Width = 50; //120;
					img.Height = 50; //200;
				}
				img.SetValue(Grid.ColumnProperty, 0);
				grdPic.Children.Add(img);
			}
			
			Grid grdTit = new Grid();
			grdTit.Name = "grdTit_" + idx.ToString();
			//RowDefinition rd;
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdTit.RowDefinitions.Add(rd);
			rd = new RowDefinition(); grdTit.RowDefinitions.Add(rd);
			grdTit.SetValue(Grid.ColumnProperty, 1);
			grdPic.Children.Add(grdTit);
			
			TextBox tbSite = new TextBox();
			tbSite.Name = "tbSte_" + idx.ToString();
			tbSite.FontSize = cdFsText;
			tbSite.Text = sTitle;
			if( bHighLight )
			{
				tbSite.Background = new SolidColorBrush(Colors.Green);
				tbSite.Foreground = new SolidColorBrush(Colors.Yellow);
			}
			else
			{
				tbSite.Background = new SolidColorBrush(Colors.Gray);
				tbSite.Foreground = new SolidColorBrush(Colors.White);
			}
			tbSite.Margin = new Thickness(-11, -12, -12, -11);
			tbSite.BorderThickness = new Thickness(0, 0, 0, 0);
			tbSite.SetValue(Grid.RowProperty, 0);
			grdTit.Children.Add(tbSite);
			
			TextBox tbDetails = new TextBox();
			tbDetails.Name = "tbDet_" + idx.ToString();
			tbDetails.FontSize = cdFsText;
			tbDetails.Text = sUri;
			tbDetails.Background = new SolidColorBrush(Colors.DarkGray);
			tbDetails.Foreground = new SolidColorBrush(Colors.Black);
			tbDetails.Margin = new Thickness(-11, -12, -12, -12);
			tbDetails.BorderThickness = new Thickness(0, 0, 0, 0);
			tbDetails.AcceptsReturn = true;
			tbDetails.TextWrapping = TextWrapping.Wrap;
			tbDetails.SetValue(Grid.RowProperty, 1);
			grdTit.Children.Add(tbDetails);
		
			Button btn = new Button();
			btn.Name = "btnTit_" + idx.ToString();
			btn.Content = "";
			btn.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
			btn.BorderThickness = new Thickness(0);
			btn.Foreground = new SolidColorBrush(Colors.White); //.Blue);
			btn.Margin = new Thickness(-12,-10,-12,-12);
			btn.Tag = oTag;
			btn.Opacity = 0.5;
			btn.SetValue(Grid.RowProperty, 0);
			grdOut.Children.Add(btn);
			
			//btn.Click += new System.Windows.RoutedEventHandler(btn_Click);
			
			if( !bUserAgent )
			{
				Grid grdAddRem = new Grid();
				grdAddRem.Name = "grdAddRem_" + idx.ToString();
				//ColumnDefinition cd;
				cd = new ColumnDefinition(); grdAddRem.ColumnDefinitions.Add(cd);
				cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdAddRem.ColumnDefinitions.Add(cd);
				cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdAddRem.ColumnDefinitions.Add(cd);
				cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdAddRem.ColumnDefinitions.Add(cd);
				grdAddRem.SetValue(Grid.RowProperty, 1);
				grdOut.Children.Add(grdAddRem);
			
				if( !bHistory )
				{
			
					btn = new Button();
					btn.Name = "btnDel_" + idx.ToString();
					btn.Content = "Delete...";
					btn.Background = new SolidColorBrush(Colors.Red);
					btn.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
					btn.BorderThickness = new Thickness(0);
					btn.Foreground = new SolidColorBrush(Colors.White); //.Blue);
					btn.Margin = new Thickness(-12,-10,-12,-12);
					btn.Tag = sTitle;
					//btn.Opacity = 0.5;
					btn.SetValue(Grid.ColumnProperty, 1);
					grdAddRem.Children.Add(btn);
					
					//btn.Click += new System.Windows.RoutedEventHandler(btn_ClickDel);
					
					btn = new Button();
					btn.Name = "btnAddRem_" + idx.ToString();
					if( HasDesktopIcon( sTitle ) )
					{
						btn.Content = "Remove from Desktop";
						btn.Background = new SolidColorBrush(Colors.Red);
					}
					else
					{
						if( m_bStandAloneApp )
							btn.Content = "Add to Start Screen";
						else
							btn.Content = "Add to Desktop";
							
						btn.Background = new SolidColorBrush(Colors.Green);
					}
					btn.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
					btn.BorderThickness = new Thickness(0);
					btn.Foreground = new SolidColorBrush(Colors.White); //.Blue);
					btn.Margin = new Thickness(-12,-10,-12,-12);
					string sDesc = sTitle + ";" + sUri + ";" + sImgPath;
					btn.Tag = sDesc;
					//btn.Opacity = 0.5;
					btn.SetValue(Grid.ColumnProperty, 2);
					grdAddRem.Children.Add(btn);
					
					//btn.Click += new System.Windows.RoutedEventHandler(btn_ClickAddRem);
				}
			
				btn = new Button();
				btn.Name = "btnImg_" + idx.ToString();
				btn.Content = "FavIcon...";
				btn.Background = new SolidColorBrush(Colors.Blue);
				btn.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
				btn.BorderThickness = new Thickness(0);
				btn.Foreground = new SolidColorBrush(Colors.White); //.Blue);
				btn.Margin = new Thickness(-12,-10,-12,-12);
				btn.Tag = sDomain;
				//btn.Opacity = 0.5;
				btn.SetValue(Grid.ColumnProperty, 3);
				grdAddRem.Children.Add(btn);
				
				//btn.Click += new System.Windows.RoutedEventHandler(btn_ClickImage);
			}
			
			UpdateFreeMemInfo();	
		}
		
		private void m_btnCopyLast_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sUri = UriFromLog();
			if( sUri.Length == 0 ) return;
			
			System.Windows.Clipboard.SetText( sUri );
			
			MessageBox.Show("Uri\n\n" + sUri + "\n\ncopied to clipboard!");
		}
		
		private string m_sFreeMemLast = "";
		private void UpdateFreeMemInfo()
		{
			if( m_txtFreeMem.Visibility == Rsc.Collapsed )
				m_txtFreeMem.Visibility = Rsc.Visible;
			
			string sFreeMem = RscUtils.toMBstr(
				Microsoft.Phone.Info.DeviceStatus.ApplicationMemoryUsageLimit
				- Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage, false );
			sFreeMem = sFreeMem.Replace( " MB", "" ); //MB is default... ...less = faulire, more = heaven...
			
			DateTime dt = DateTime.Now;
			sFreeMem += "\n" + RscUtils.pad60( dt.Hour ) + ":" + RscUtils.pad60( dt.Minute );
			
			if( sFreeMem == m_sFreeMemLast )
				return;
			
			m_sFreeMemLast = sFreeMem;
			
			m_txtFreeMem.Text = m_sFreeMemLast;
		}
		
		private void m_btnDownload_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_client != null )
			{
				if( MessageBoxResult.OK != MessageBox.Show("Do you realy want to Cancel download?\n\n(press Back to cancel)") )
					return;
				
				m_client.CancelAsync();
			}
			else
			{
				string sUri = UriFromLog();
				if( sUri.Length == 0 ) return;
					
				if( StatusPanel.Visibility == Rsc.Visible )
				{
					DoStop();
				}
				
				if( MessageBoxResult.OK != MessageBox.Show("Do you want to download file?\n\n" + sUri + "\n\n(press Back to cancel)") )
					return;
				
				DoDownLoad( sUri );
			}
		}
		
		private string m_sDownloadFnOrig = "";
		private string m_sDownloadedPath = "";
		private DateTime m_dtDownloadStart;
		private void DoDownLoad( string sUri )
		{
			m_txtDownload.Text = sUri;
			m_txtDownload.Visibility = Rsc.Visible;
			m_btnDownload.Image.Source = m_isDownloadCancel;
			m_btnDownFolder.Visibility = Rsc.Collapsed;
			
			m_client = new System.Net.WebClient();
			m_client.DownloadProgressChanged+=new System.Net.DownloadProgressChangedEventHandler(m_client_DownloadProgressChanged);
			m_client.OpenReadCompleted += new System.Net.OpenReadCompletedEventHandler(client_OpenReadCompleted);
		
			Uri uri = new Uri(sUri, UriKind.Absolute);
			
			m_sDownloadFnOrig = uri.LocalPath;
			if( m_sDownloadFnOrig.Length > 0 )
			{
				if( m_sDownloadFnOrig[ 0 ] == '/' )
					m_sDownloadFnOrig = m_sDownloadFnOrig.Substring( 1 );
				m_sDownloadFnOrig = m_sDownloadFnOrig.Replace('/', '_');
			}
			
			m_sDownloadedPath = "";
			
			//NoSleep - On
			PhoneApplicationService phoneAppService = PhoneApplicationService.Current;
			phoneAppService.UserIdleDetectionMode = IdleDetectionMode.Disabled;
			
			m_dtDownloadStart = DateTime.Now;
			
			try
			{
				m_client.OpenReadAsync(uri);
			}
			catch( Exception )
			{
				DownloadEnd( true );
			}
		}

		private void m_client_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
		{
			string s = e.ProgressPercentage.ToString() + " %" + "\n" +
				RscUtils.toMBstr( e.BytesReceived, false ); 
			
			if( e.TotalBytesToReceive >= 0 )
			{
				s += " of " + "\n" + RscUtils.toMBstr( e.TotalBytesToReceive, false );
			}
			
			TimeSpan ds = DateTime.Now - m_dtDownloadStart;
			double dSec = ds.TotalSeconds;
			if( dSec > 0 )
			{
				double dSiz = ((double) e.BytesReceived);
				
				double dRes = dSiz / dSec;
				dRes = Math.Round( dRes, 0 );
				
				s += "\n" + RscUtils.toMBstr( ((long) dRes), false ) + "/s";
			}
			
			//NOT NEEDED!!!
			/*
			s += "\nmem avail: " + RscUtils.toMBstr(
				Microsoft.Phone.Info.DeviceStatus.ApplicationMemoryUsageLimit
				- Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage );
			*/
			
			m_txtDownPrs.Text = s;
			m_txtDownPrs.Visibility = Rsc.Visible;
		}
		
		private void client_OpenReadCompleted(object sender, System.Net.OpenReadCompletedEventArgs e)
		{
			if (e.Error == null)
			{				
				string sFolder = "A:\\Downloads";
				
				RscStore store = new RscStore();
				
				store.CreateFolderPath( sFolder );
				
				DateTime dNow = DateTime.Now;
				string sFn = dNow.Year.ToString() + RscUtils.pad60(dNow.Month) 
					+ RscUtils.pad60(dNow.Day) + "_" + RscUtils.pad60(dNow.Hour) + RscUtils.pad60(dNow.Minute) 
					+ RscUtils.pad60(dNow.Second);
				
				//Allow the user to rename...
				/*
				if( m_sDownloadFnOrig.Length > 0 )
				{
					sFn = sFn + "_" + m_sDownloadFn;
				}
				else
				*/
				{
					sFn += ".dat";
				}
				
				m_sDownloadedPath = sFolder + "\\" + sFn;
				
				System.IO.Stream stream = null;
				
				if( store.FileExists(m_sDownloadedPath) ) store.DeleteFile(m_sDownloadedPath);
				
				stream = store.CreateFile(m_sDownloadedPath);
				
				int iCbBuff = 10 * 4096;
				byte [] ay = new byte [iCbBuff];
				long lPos = 0;
				for(;;)
				{
					int iToCopy = iCbBuff;
					
					if( (lPos + iToCopy) > e.Result.Length )
						iToCopy = (int) (e.Result.Length - lPos);
					
					if( iToCopy <= 0 ) break;
					
					e.Result.Read( ay, 0, iToCopy );
					stream.Write( ay, 0, iToCopy );
					
					lPos = lPos + iToCopy;
				}
				
				stream.Close();

				//Allowe the user to rename...
				/*
				MessageBox.Show( "Downloaded successfully to:\n\n" + m_sDownloadedPath );
				*/
				
				DownloadEnd( false );
			}
			else
			{
				DownloadEnd( true );
			}
		}
		
		private void DownloadEnd( bool bError )
		{
			if( bError )
			{
				MessageBox.Show( "Download failed!" );
			}
				
			m_txtDownPrs.Text = "";
			m_txtDownPrs.Visibility = Rsc.Collapsed;
			
			m_txtDownload.Text = "";
			m_txtDownload.Visibility = Rsc.Collapsed;
			
			m_btnDownload.Image.Source = m_isDownload;
			
			m_btnDownFolder.Visibility = Rsc.Visible;
			
			m_client = null;
			
			//NoSleep - Off
			PhoneApplicationService phoneAppService = PhoneApplicationService.Current;
			phoneAppService.UserIdleDetectionMode = IdleDetectionMode.Enabled;
			
			if( !bError )
			{
				RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
					csAppTitle, "Images/Ico001_IE.jpg", "txDownFn" );
				
				appInput.SetFlag( 0, "downloaded file name" );
				appInput.SetFlag( 1, "NoEmpty" );
				appInput.SetFlag( 2, "FileName" );
				appInput.SetData( 0, m_sDownloadFnOrig );
				appInput.SetInput( "RscDlg_BigInputV10" ); //"RscDlg_TxtInputV10" );
				
				this.NavigationService.Navigate( appInput.GetNavigateUri( csIPgC_DlgsAssy ) );
			}
		}
		
		private void ShowUserAgentList( string sDomain )
		{
			m_aRows.Clear();
			
			string sUaIDCurSel = "x"; //...N/A
			if( m_UriAutoStart != null )
			{
				sUaIDCurSel = RscBrowserSettings.LoadUserAgentID( m_UriAutoStart, sUaIDCurSel );
			}
			
			for( int iCyc = 0; iCyc < 2; iCyc++ )
			{
				for( int i = RscUserAgents.Count - 1; i >= 0; i-- )
				{
					string sTitle = RscUserAgents.Title( i );
					bool bHighLight = false;
					if( sUaIDCurSel != "x" )
					{
						if( sUaIDCurSel == RscUserAgents.ID( i ) )
						{
							bHighLight = true;
							sTitle += " (previously selected)";
						}
					}
					
					if( iCyc == 0 )
					{
						if( !bHighLight )
							AddToUriList( sTitle, RscUserAgents.UserAgent( i, true ), "", csUaMark + RscUserAgents.ID( i ), bHighLight );
					}
					else
					{
						if( bHighLight )
							AddToUriList( sTitle, RscUserAgents.UserAgent( i, true ), "", csUaMark + RscUserAgents.ID( i ), bHighLight );
					}		
				}
			}
			
			if( m_aRows.Count == 0 )
			{
				MessageBox.Show( "No UserAgent to list!" );
			}
			else
			{
				m_BookmarksVisible = false;
				
				m_HistoryVisible = false;
				
				if( m_bb.HasUserAgent )
					LoadWallImg( m_sContentPanel, true );
				
				UriPanel.Visibility = Rsc.Collapsed;
				BmPanel.Visibility = Rsc.Collapsed;
				GoPanel.Visibility = Rsc.Collapsed;
				
				m_bb.Visibility = Rsc.Collapsed;
				
				scrlTitle.Text = "User Agents";
				if( sDomain.Length > 0 )
				{
					scrlTitle.Text = scrlTitle.Text + " (" + sDomain + ")";
				}
 
				scrl.Visibility = Rsc.Visible;
			}
		}
		
		private string UriFromLog()
		{
			string str = m_txtLog.Text.ToString();
			if( str.Length == 0 ) return "";
			
			int iPos = str.IndexOf(" | ");
			if( iPos < 0 ) return "";
			
			string sUri = str.Substring( iPos + 3 );
			if( sUri.Length == 0 ) return "";
			
			return sUri;
		}
				
		private void m_btnUserAgent_Click(object sender, System.Windows.RoutedEventArgs e)
		{			
			if( scrl.Visibility == Rsc.Collapsed )
			{
				string sDomain = "";
				string sUri = UriFromLog();
				if( sUri.Length > 0 )
				{
					Uri uri = new Uri( sUri, UriKind.Absolute );
					sDomain = uri.DnsSafeHost;
				}
				
				ShowUserAgentList( sDomain );
			}
			else
			{
				UriPanel.Visibility = Rsc.Collapsed;
				BmPanel.Visibility = Rsc.Collapsed;
				GoPanel.Visibility = Rsc.Collapsed;
				
				scrl.Visibility = Rsc.Collapsed;
				m_bb.Visibility = Rsc.Visible;
			}
		}
		
		private void Purge( string sUri = "" )
		{
			_navigationStack.Clear();
			
			m_txtStatus.Text = "";
			m_txtLog.Text = "Ready...";
			
			txUri.Text = sUri;
			
			txBmName.Text = "";
			
			m_btnTriBack.Visibility = Rsc.Collapsed;
			m_btnDblBack.Visibility = Rsc.Collapsed;
			m_btnBack.Visibility = Rsc.Collapsed;
			
			m_btnBm.Visibility = Rsc.Collapsed;
			
			m_bb.Purge();
			
			UriPanel.Visibility = Rsc.Visible;
			GoPanel.Visibility = Rsc.Visible;
		}
		
		private void SetUserAgent( string sUaID )
		{
			string sDomain = "";
			string sUri = UriFromLog();
			if( sUri.Length > 0 )
			{
				Uri uri = new Uri( sUri, UriKind.Absolute );
				sDomain = uri.DnsSafeHost;
			}
			if( sDomain.Length > 0 )
			{
				RscBrowserSettings.StoreUserAgentID( sDomain, sUaID );
			}
				
			m_bb.SetUserAgentID( sUaID );
			
			scrl.Visibility = Rsc.Collapsed;
			m_bb.Visibility = Rsc.Visible;
			
			if( m_UriAutoStart != null )
			{
				Uri uri = m_UriAutoStart;
				m_UriAutoStart = null;
				
				RscBrowserSettings.StoreUserAgentID( uri, sUaID );
				
				m_bb.Navigate( uri );
			}
			else
			{
				Purge( sUri );
				
				MessageBox.Show( "User Agent successfuly changed!\n\n" +
					"NOTE: May not be applied to pages visited in this session, app restart required!" );
			}
		}
		
		private void m_btnClearCache_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( MessageBoxResult.OK != MessageBox.Show( "Do you realy want to delete browser cache?\n\nNOTE: Remembered passwords WILL BE KEPT!!! To delete passwords uninstall app and reinstall it!!!\n\n(Press Back to Cancel...)" ) )
				return;
			
			Purge();
			
			//NOTE: Deleting all IE folders WILL NOT force the user to reenter eMail + Pw!!!		
			string sIeCacheFolders = ";" + RscStore_IsoStore.Get_IE_Cache_FolderList( ';' );
			sIeCacheFolders = sIeCacheFolders.Replace( ";", ";A:\\" );
			sIeCacheFolders = sIeCacheFolders.Substring(1);
			
			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				csAppTitle, "Images/Ico001_IE.jpg", "ClearCache" );
			
			appInput.SetData( 0, sIeCacheFolders );
			appInput.SetData( 1, "*.*" );
			appInput.SetData( 2, "recurse" );
			//appInput.SetData( 2, "" );
			
			appInput.SetData( 3, "showHidden" );
			//appInput.SetData( 3, "" );
			
			appInput.SetData( 4, "AutoDelete" );
			
			appInput.SetInput( "RscViewer_FindFilesV12" );
						
			this.NavigationService.Navigate( appInput.GetNavigateUri( csViewersAssy ) );
		}
		
		private void m_btnNight_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_bb.NightMode = !m_bb.NightMode;
			
			if( m_bb.NightMode )
			{
				m_btnNight.Image.Source = m_isEmptyOn;
			}
			else
			{
				m_btnNight.Image.Source = m_isEmptyOff;
			}
			
			RscRegistry.WriteBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\" + csClsName + "\\Settings",
				"NightMode", m_bb.NightMode );
			
			m_bb.ApplyNightMode( wbc1 );
			m_bb.ApplyNightMode( wbc2 );
		}
		
		private void m_btnUd_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( BottomBar.Visibility == Rsc.Collapsed )
			{
				BottomBar.Visibility = Rsc.Visible;
				m_btnUd.Image.Source = m_isD;
			}
			else
			{
				BottomBar.Visibility = Rsc.Collapsed;
				m_btnUd.Image.Source = m_isU;
			}
		}
		
		private void m_btnDownFolder_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sFolder = "A:\\Downloads";
			
			string sUri = "/" + csViewersAssy + ";component/RscViewer_FsV12" + ".xaml" +
				"?root=" + sFolder
				+ "&" + "app_title=" + csAppTitle + " - " + "Downloads"
				+ "&" + "app_icores=" + "Images/Ico001_IE.jpg";
			
			this.NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}
		
		private void NavToPage( string sPageName, string sPageArgs )
		{
			if( sPageName.Length == 0 ) return;
			
			string sUri = "";
			
			if( sPageName.IndexOf( ";component/" ) >= 0 )
			{
				sUri = sPageName + ".xaml" + sPageArgs;
			}
			else
			{
				sUri = "/" + sPageName + ";component/"
					+ "MainPage" + ".xaml" + sPageArgs;
			}
			
			this.NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}
		
		private bool HasDesktopIcon( string sTitle )
		{
			if( sTitle.Length == 0 ) return false;
			
			if( m_bStandAloneApp )
			{
				return false;
			}
			else
			{
				string sPath = "A:\\Desktop";
				
				RscStore store = new RscStore();
				if( !store.FolderExists( sPath ) ) return false;
				
				sPath += "\\" + "Internet Bookmarks";
				if( !store.FolderExists( sPath ) ) return false;
				
				sPath += "\\" + sTitle + ".txt";
				return store.FileExists( sPath );
			}
		}
		
		private void SimpleLbItem_BtnCust1_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			MySimpleLbItem it;
			it = (MySimpleLbItem) btn.Tag;
			
			string sDesc = it.IcoInfo;
			int iPos = sDesc.IndexOf(';');
			if( iPos < 0 ) return;
			int iPos2 = sDesc.LastIndexOf(';');
			if( iPos2 < 0 ) return;
			if( iPos2 == iPos ) return;
			
			string sTitle = sDesc.Substring(0, iPos);
			string sUri = sDesc.Substring(iPos + 1, (iPos2 - iPos) - 1);
			if( sUri.IndexOf("://") < 0 )
				sUri = "http://" + sUri;
			string sImgPath = sDesc.Substring(iPos2 + 1);
			
			if( m_bStandAloneApp )
			{
				
				RscShellTileManager stm = new RscShellTileManager();
				stm.Create( "na", sUri, sImgPath, sTitle );
				
			}
			else
			{
				/*
				if( sImgPath.Length == 0 )
				{
					MessageBox.Show( "No image for BookMark!" );
					return;
				}
				*/
				
				//Debug...
				//MessageBox.Show( sTitle + "\r\n" + sUri + "\r\n" + sImgPath );
				//return;
				
				string sPath = "A:\\Desktop\\Internet Bookmarks";
				
				RscStore store = new RscStore();
				
				if( HasDesktopIcon( sTitle ) )
				{
					store.DeleteFile( sPath + "\\" + sTitle + ".txt" );
					
					sPath += "\\" + "tn";
					if( store.FolderExists( sPath ) )
					{
						store.DeleteFile( sPath + "\\" + sTitle + ".txt.jpg" );
					}
					
					it.BtnCust1Img = m_isAddTile;
				}
				else
				{
					store.CreateFolderPath(sPath);
					
					store.WriteTextFile(sPath + "\\" + sTitle + ".txt", sUri, true );
					
					if( sImgPath.Length > 0 )
					{
						sPath += "\\" + "tn";
						store.CreateFolderPath(sPath);
						
						store.CopyFileForce(sImgPath, sPath + "\\" + sTitle + ".txt.jpg");
					}
					
					it.BtnCust1Img = m_isDelTile;
				}
						
				MessageBox.Show( "NOTE: To take into effect, restart application!" );
			}
		}
		
		private void SimpleLbItem_BtnCust2_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			MySimpleLbItem it;
			it = (MySimpleLbItem) btn.Tag;
			
			string sDomain = it.Desc1;
			
			if( RscBrowserSettings.MarkedOpenExternal( sDomain ) )
			{
				if( MessageBoxResult.OK != MessageBox.Show( "Do you realy want to UN-mark domain " + sDomain + " to open in external browser?\n\n(press Back to Cancel)" ) )
					return;
				
				RscBrowserSettings.MarkOpenExternal( sDomain, false );
				
				it.BtnCust2Img = m_isOff;
			}
			else
			{
				if( MessageBoxResult.OK != MessageBox.Show( "Do you realy want to mark domain " + sDomain + " to open in external browser?\n\n(press Back to Cancel)" ) )
					return;
				
				RscBrowserSettings.MarkOpenExternal( sDomain, true );
				
				it.BtnCust2Img = m_isOn;
			}
		}
		
		private void SimpleLbItem_BtnCust3_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			MySimpleLbItem it;
			it = (MySimpleLbItem) btn.Tag;
			
			string sTitle = it.Title;
			
			if( MessageBoxResult.OK != MessageBox.Show( "Do you realy want to delete bookmark\n" + sTitle + " ?\n\n(press Back to Cancel)" ) )
				return;
			
			RscStore store = new RscStore();
			
			string sFolder = "A:\\Internet\\Bookmarks";
			if( !store.FolderExists( sFolder ) ) return;
			
			store.DeleteFile( sFolder + "\\" + sTitle + ".ilnk" );
			
			RefreshBookmarks( true );
		}
		
		private void SimpleLbItem_BtnCust4_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			MySimpleLbItem it;
			it = (MySimpleLbItem) btn.Tag;
			
			string sDomain = it.Desc1;
			
			NavToPage( "/RscLearnXamlV10;component/FaviconFinder", "?dns=" + sDomain );
		}
		
		string m_sUaAlertDomains_TMP = "";
		private void SimpleLbItem_Btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			MySimpleLbItem it;
			it = (MySimpleLbItem) btn.Tag;
			
			if( it.oTag.GetType().ToString() == "System.String" )
			{
				string sTag = it.oTag.ToString();				
				if( sTag != "" )
				{
					if( sTag.Substring( 0, 3 ) == csUaMark )
					{
						
						string sUaID = sTag.Substring( 3 );
				
						if( m_UriAutoStart != null )
						{
							if( m_sUaAlertDomains_TMP.Length > 0 )
							{
								string sLst = m_sUaAlertDomains_TMP;
								m_sUaAlertDomains_TMP = "";
								
								if( sLst.IndexOf( ";" + m_UriAutoStart.DnsSafeHost.ToLower() ) >= 0 )
									MessageBox.Show( "NOTE: User Agent selection may not be applied to pages visited in this session, app restart required!" );
							}
						}
						
						SetUserAgent( sUaID );
					}
					else
					{
						string sUri = sTag;
						
						if( sUri.IndexOf("://") < 0 )
							sUri = "http://" + sUri;
						
						m_sUaAlertDomains_TMP = "";				
						for( int i = _navigationStack.Count() - 1; i >= 0; i-- )
							m_sUaAlertDomains_TMP += ";" + _navigationStack.ElementAt(i).DnsSafeHost.ToLower();
						
						His_Clear();
				
						scrl.Visibility = Rsc.Collapsed;
						m_bb.Visibility = Rsc.Visible;
						
						//Debug - History
						//His_Add( new Uri(sUri, UriKind.Absolute) );
						
						m_UriAutoStart = new Uri(sUri, UriKind.Absolute);
						ShowUserAgentList( m_UriAutoStart.DnsSafeHost );
						//m_bb.Navigate(new Uri(sUri, UriKind.Absolute));
					}
				}
			}
			else
			{
				int idx = (int) it.oTag;
				
				//Invert Index...
				idx = (m_aRows.Count - 1) - idx;
				//txLog.Text = idx.ToString();
				
				His_Back( idx );
			}
			
		}
		
    }
	
}
