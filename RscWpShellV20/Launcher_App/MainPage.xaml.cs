using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Launcher_App.Resources;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;
//using Microsoft.Phone.Scheduler;
using System.Windows.Input;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;

using Ressive.Utils;
using Ressive.Store;
using Ressive.Theme;

using Ressive.ShellTiles;

using Launcher_Lib;

namespace Launcher_App
{
	
    public partial class MainPage : PhoneApplicationPage
    {
		
		const string csTaskName = "RscWpShell_80_Task";

		private void InitializeViewElements()
		{
			RscStore store = new RscStore();
			
			//Register all file-type associations...
			RscFileTypes.RegisterAll();
			
			//Extension handlers:
			m_sExtension_Unknown = "/Lib_RscViewers;component/RscViewer_QueryStringV10";
			m_sExtension_EditPhoto = "/Lib_RscExtensions;component/RscExtension_PhotoV10";
			
			int iPageIndex = 0;
			
			iPageIndex = -3;
			//Ressive.Hu, Apps, Tools
			Add_App_Title( iPageIndex, "RscLearnXamlV10", Colors.Black, "Learn\nXAML\nApps", "Learn XAML Apps" );
			Add_App_WithArgs_Icon( iPageIndex, "RscIeV10", "?uri=www.ressive.hu", "Images/Ico001_Ressive.jpg", "Ressive.Hu" );
			//Add_App_TitleOverIcon( iPageIndex, "Lib_RscIPgC_Dlgs", "Ressive\nLib\nDlgs", "RscLib Dialogs", "Images/Ico001_Ressive.jpg" );
			Add_App_Title( iPageIndex, "Lib_RscIPgC_Dlgs", Colors.Black, "Ressive\nLib\nDlgs", "RscLib Dialogs" );
			Add_App_Title( iPageIndex, "Lib_RscViewers", Colors.Black, "Ressive\nLib\nViewers", "RscLib Viewers" );
			Add_App_Title( iPageIndex, "Lib_RscExtensions", Colors.Black, "Ressive\nLib\nExts", "RscLib Extensions" );
			Add_App_Title( iPageIndex, "RscXobsolotes", Colors.Black, "Ressive\nObsolote\nApps", "Obsolote Apps" );
			Add_App_Title( iPageIndex, "RscXtests", Colors.Black, "Ressive\nTest\nApps", "Test Apps" );
			
			iPageIndex = -2;
			//Ressive.Hu, Grouped Apps
			Add_App_Icon( iPageIndex, "Launcher_AppMini", "Images/Ico001_LauncherMini.jpg", "Launcher (Mini)" );
			Add_App_Icon( iPageIndex, "/RscDC;component/RscDC_CameraV11", "Images/Ico001_PhotoCamera.jpg", "Photo Camera" );
			Add_App_Icon( iPageIndex, "/RscDC;component/RscDC_CamcorderV11", "Images/Ico001_Camcorder.jpg", "Video Camera" );
			Add_App_Icon( iPageIndex, "/RscFtpClients;component/RscFtp_ExplorerV11", "Images/Ico001_FtpTest.jpg", "FTP Explorer" );
			Add_App_Icon( iPageIndex, "/RscFtpClients;component/RscFtp_UpLoadV11", "Images/Ico001_FtpUpLoad.jpg", "FTP Upload" );
			Add_App_Icon( iPageIndex, "/RscFtpClients;component/RscFtp_DownLoadV11", "Images/Ico001_FtpDownLoad.jpg", "FTP Download" );
			
			iPageIndex = -1;
			//Ressive.Hu, Internet Links
			{
				Add_App_Icon( iPageIndex, "RscIeV10", "Images/Ico001_IE.jpg", "Internet Explorer" );
				
				//Contact Desktop Icons
				if( store.FolderExists( "A:\\Desktop" ) )
				{
					string sFolder = "A:\\Desktop\\Internet Bookmarks";
					
					if( store.FolderExists( sFolder ) )
					{
						string [] asFns = RscSort.OrderBy(store.GetFileNames( sFolder, "*.txt" ));
						foreach( string sFn in asFns )
						{
							bool bTmp;
							string sUri = store.ReadTextFile( sFolder + "\\" + sFn, "", out bTmp );
							if( sUri.Length > 0 )
							{
								sUri = sUri.Replace("#", "(HASH)");
								
								string sPathTn = sFolder + "\\tn\\" + sFn + ".jpg";
								if( store.FileExists( sPathTn ) )
								{
									Add_App_WithArgs_Icon( iPageIndex, "RscIeV10",
										"?uri=" + sUri,
										"isostore:\\" + sPathTn,
										"Link (" + RscStore.FileNameOfPath( sFn ) + ")" );
								}
								else
								{
									Add_App_TitleOverIcon( iPageIndex,
										"RscIeV10",
										RscStore.FileNameOfPath( sFn ).Replace( ' ', '\n' ),
										"Link (" + RscStore.FileNameOfPath( sFn ) + ")",
										"Images/Ico001_IE.jpg",
										"?uri=" + sUri);
								}
							}
						}
					}
				}
			}
			
			iPageIndex = 0;
			//Ressive.Hu, Apps, Main
			Add_SystemIcon_Updatable( iPageIndex, "sysDtFull", 1, "Date"  );
			Add_SystemIcon_Updatable( iPageIndex, "sysTm", 1, "Time"  );
			Add_SystemIcon_Updatable( iPageIndex, "sysDtDay", 1, "Day of week, Week of year" );
			Add_SystemIcon_Updatable( iPageIndex, "sysFsFree", 10, "Free Space" );
			// ///////////
			Add_SystemIcon_Updatable( iPageIndex, "sysCellNet", 10, "Celluar network information"  );
			Add_SystemIcon_Updatable( iPageIndex, "sysCnt_Note", 3600, "Note Count"  );
			Add_SystemIcon_Updatable( iPageIndex, "sysCnt_Anni", 60, "Anniversary Alert"  );
			Add_SystemIcon_Updatable( iPageIndex, "sysCnt_WebDog", 60, "Web Dog"  );
			// ///////////
			Add_SystemIcon_Updatable( iPageIndex, "sysCnt_Event", 10, "Event Count"  );
			Add_SystemIcon( iPageIndex, "sysWiFi", "Images/Ico001_WiFi.jpg", "WiFi Settings" );
			Add_SystemIcon( iPageIndex, "sysBT", "Images/Ico001_BlueTooth.jpg", "BlueTooth Settings" );
			Add_SystemIcon_Updatable( iPageIndex, "sysBatPow", 10, "Battery Power" );
			//Add_SystemIcon( iPageIndex, "sysBatt", "Images/Ico001_Battery.jpg", "Battery Info" );
			// ///////////
			Add_App_Icon( iPageIndex, "/Lib_RscViewers;component/RscViewer_PhotoFolderV10", "Images/Ico001_Gallery.jpg", "Photo Gallery" );
			Add_App_Icon( iPageIndex, "/RscLearnXamlV10;component/ClockSaver", "Images/Ico001_ScreenSaver.jpg", "Nokia Screen Saver" );
			Add_App_Icon( iPageIndex, "RscDC", "Images/Ico001_PhotoCamera.jpg", "Digital Camera" );
			Add_App_Icon( iPageIndex, "/Lib_RscViewers;component/RscViewer_VideoFolderV10", "Images/Ico001_VideoGallery.jpg", "Video Gallery" );
			// ///////////
			Add_App_Icon( iPageIndex, "/Lib_RscViewers;component/RscViewer_SoundV11", "Images/Ico001_SoundPlayer.jpg", "Sound Player" );
			Add_SystemIcon( iPageIndex, "sysSm", "Images/Ico001_StopMusic.jpg", "Stop Music" );
			Add_App_Icon( iPageIndex, "RscFtpClients", "Images/Ico001_FtpTest.jpg", "FTP Cloud" );
			Add_SystemIcon_Updatable( iPageIndex, "sysCnt_PerDay", 10, "Amount per day"  );
			Add_App_Icon( iPageIndex, "/Lib_RscViewers;component/RscViewer_FsV12", "Images/Ico001_Explorer4.jpg", "File Explorer v1.2" );
			// ///////////
			Add_App_Title( iPageIndex, "/Lib_RscViewers;component/RscViewer_MedLibV11", Colors.Black, "Ressive\nMedLib\nv11", "Media Library" );
			Add_App_Title( iPageIndex, "/RscXtests;component/RscAssemblyBrowserV10", Colors.Black, "Ressive\nAssyBrwsr\nv10", "Assembly Browser" );
			
			iPageIndex = 1;
			//Ressive.Hu, Apps, Tools
			Add_App_Icon( iPageIndex, "Lib_RscSettings", "Images/Ico001_Settings.jpg", "System Settings" );
			Add_App_Icon( iPageIndex, "/Lib_RscViewers;component/RscViewer_FindFilesV12", "Images/Ico001_FindFiles.jpg", "Find Files v1.2" );
			Add_App_Icon( iPageIndex, "/RscLearnXamlV10;component/ColorMatchGame", "Images/Ico001_ColorMatch.jpg", "Color Match Game" );
			// ///////////
			{
				Add_App_WithArgs_Icon( iPageIndex, "/Lib_RscViewers;component/RscViewer_TextV10", "?folder=A:\\Contacts", "Images/Ico001_Contacts.jpg", "Contacts (Contacts\\*.vcf)" );
				
				//Contact Desktop Icons
				if( store.FolderExists( "A:\\Desktop" ) )
				{
					string sFolder = "A:\\Desktop\\Contacts";
					
					if( store.FolderExists( sFolder ) )
					{
						string [] asFns = RscSort.OrderBy(store.GetFileNames( sFolder, "*.txt" ));
						foreach( string sFn in asFns )
						{
							bool bTmp;
							string sPath = store.ReadTextFile( sFolder + "\\" + sFn, "", out bTmp );
							
							//Backward Compat...
							if( sPath[ 1 ] != ':' ) sPath = "A:\\" + sPath;
							
							if( sPath.Length > 0 )
							{
								if( store.FileExists( sPath ) )
								{
									string sPathTn = sFolder + "\\tn\\" + sFn + ".jpg";
									if( store.FileExists( sPathTn ) )
									{
										Add_App_WithArgs_Icon( iPageIndex, "/Lib_RscViewers;component/RscViewer_TextV10",
											"?file=" + sPath,
											"isostore:\\" + sPathTn,
											"Contact (" + RscStore.FileNameOfPath( sFn ) + ")" );
									}
									else
									{
										Add_App_TitleOverIcon( iPageIndex,
											"/Lib_RscViewers;component/RscViewer_TextV10",
											RscStore.FileNameOfPath( sFn ).Replace( ' ', '\n' ),
											"Contact (" + RscStore.FileNameOfPath( sFn ) + ")",
											"Images/Ico001_Contacts.jpg",
											"?file=" + sPath);
									}
								}
							}
						}
					}
				}
			}
			
		}
		
		private void Add_SystemIcon_Updatable( int iPageIndex, string sID, int iIntervallSeconds, string sLongDescription )
		{
			_AddViewElement( sLongDescription, iPageIndex, sID, iIntervallSeconds );
		}
		private void Add_SystemIcon( int iPageIndex, string sID, string sImageResName, string sLongDescription )
		{
			_AddViewElement( sLongDescription, iPageIndex, sID, sImageResName );
		}
		private void Add_App_WithArgs_Icon( int iPageIndex, string sPageName, string sPageArguments, string sImageResName, string sLongDescription )
		{
			_AddViewElement( sLongDescription, iPageIndex, "sysPg", sImageResName, sPageName, sPageArguments );
		}
		private void Add_App_Icon( int iPageIndex, string sPageName, string sImageResName, string sLongDescription )
		{
			_AddViewElement( sLongDescription, iPageIndex, "sysPg", sImageResName, sPageName );
		}
		private void Add_App_TitleOverIcon( int iPageIndex, string sPageName, string s3LineTitle, string sLongDescription, string sImageResName, string sPageArguments = "" )
		{
			_AddViewElement( sLongDescription, iPageIndex, "sysPg", sImageResName, sPageName, sPageArguments, s3LineTitle );
		}
		private void Add_App_Title( int iPageIndex, string sPageName, Color BkColor, string s3LineTitle, string sLongDescription )
		{
			_AddViewElement( sLongDescription, iPageIndex, "sysPg", sPageName, s3LineTitle, BkColor );
		}
			
		int iIconDimension = 80;
		int iIconChbDim = 30; //24;
		int iBtnDimension = 80 + 24;
		
		List< ViewElement > m_ve;
		
		List< Button > m_tmrtrg;
		DispatcherTimer m_tmr;
		
		private Grid m_grid = null;
		
		Point m_ptTouchDown;
		
		int m_iPgIdxMin = 0;
		int m_iPgIdxMax = 0;
		int m_iPgIdxCur = 0;
		
		DispatcherTimer m_tmrSavePanelShot;
		
		DispatcherTimer m_tmrStatAutoHide;
		
		bool m_bInThisApp = true;
		
		RscTheme m_Theme = null;
		
		DispatcherTimer m_tmrQSV;
		string m_sNavUriQSV = "";
		
		bool m_bNavToCalledWithNoArg = false;
		Size m_sScreenToDo = new Size(0, 0);
		
		string m_sExtension_EditPhoto = "";
		//string m_sExtension_SharePhoto = "";
		string m_sExtension_Unknown = "";
		
		string m_sNavApp = "";
		DispatcherTimer m_tmrExitOnBack;
		
		DateTime m_dtTileEnter;
		int m_iIdxTileEnter;

		public MainPage()
        {
            InitializeComponent();
			
			try
			{
				
				//Shell Tiles...
				{
				
					RscShellTileManager.InitShellTask( csTaskName );
						
					// DO NOT! SLOW!
					// //Update Live Tiles if any...
					//SysTiles.DoUpdate();
				
					// DO NOT! SLOW!
					//CleanUpShellTileData( );
				}
				
				m_tmrtrg = new List< Button >();			
				m_ve = new List< ViewElement >();
	
				Touch.FrameReported += new System.Windows.Input.TouchFrameEventHandler(Touch_FrameReported);
				m_ptTouchDown = new Point(0,0);
				
				Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
				GlobalDILholder.Tag = new RscTheme( true, "Theme", "Current", "Default" );
				m_Theme = (RscTheme) GlobalDILholder.Tag;
				//m_dil = new RscDefaultedImageList( "Theme", "Current", "Default" );
				
				InitializeViewElements();
				
				m_tmr = new DispatcherTimer();
				m_tmr.Interval = System.TimeSpan.FromSeconds(1);
				m_tmr.Tick +=new System.EventHandler(m_tmr_Tick);
				m_tmr.Start();
				
				m_tmrSavePanelShot = new DispatcherTimer();
				m_tmrSavePanelShot.Interval = TimeSpan.FromMilliseconds(500);
				m_tmrSavePanelShot.Tick +=new System.EventHandler(m_tmrSavePanelShot_Tick);
				
				m_tmrStatAutoHide = new DispatcherTimer();
				m_tmrStatAutoHide.Interval = TimeSpan.FromSeconds(2);
				m_tmrStatAutoHide.Tick +=new System.EventHandler(m_tmrStatAutoHide_Tick);
				
				this.Unloaded += new System.Windows.RoutedEventHandler(MainPage_Unloaded);
				
				m_tmrQSV = new DispatcherTimer();
				m_tmrQSV.Interval = TimeSpan.FromMilliseconds(500);
				m_tmrQSV.Tick += new System.EventHandler(m_tmrQSV_Tick);
				
				m_tmrExitOnBack = new DispatcherTimer();
				m_tmrExitOnBack.Interval = TimeSpan.FromMilliseconds(500);
				m_tmrExitOnBack.Tick += new System.EventHandler(m_tmrExitOnBack_Tick);
				
				m_dtTileEnter = DateTime.Now;
				m_iIdxTileEnter = -1;
				
				// //
				//
				// XNA XNA XNA
	
				/*
				// Timer to run the XNA internals (MediaPlayer is from XNA)
				DispatcherTimer dt = new DispatcherTimer();
				dt.Interval = TimeSpan.FromMilliseconds(33);
				dt.Tick += delegate { try { Microsoft.Xna.Framework.FrameworkDispatcher.Update(); } catch { } };
				dt.Start();
				*/
				
				//
				// //
			}
			catch( Exception e )
			{
				string sException = "Message:\n" + e.Message +
					"\n\nStack Trace:\n" + e.StackTrace;
			
				MessageBox.Show(sException);
			}
       }
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			m_bInThisApp = true;
			
			bool bDenieLoad = false;
			
			IDictionary<string, string> pars = this.NavigationContext.QueryString;
			if( pars.Count > 0 )
			{
				//ATT!!! Handle once!!!
				if( m_sNavUriQSV.Length == 0 )
				{
					//lblStatus.Text = e.Uri.ToString();
					
					//string sDbg = "Cnt=" + pars.Count.ToString() + " | " + "action=" + pars["Action"];
					//MessageBox.Show( sDbg );
					
					bool bIgnorePars = false;
					
					if( pars.Count == 2 && pars.ContainsKey("Action") && m_sExtension_EditPhoto.Length > 0 )
					{
						switch( pars["Action"] )
						{
							case "ShareContent" :
							case "EditPhotoContent" :
								m_sNavApp = m_sExtension_EditPhoto;
								break;
						}
					}
					else
					{
						if( pars.Count == 1 && pars.ContainsKey("token") && m_sExtension_EditPhoto.Length > 0 )
						{
							m_sNavApp = m_sExtension_EditPhoto;
						}
						else if( pars.Count == 2 && pars.ContainsKey("IcoGd") && pars.ContainsKey("IcoId") )
						{
							//SysTile...
							bIgnorePars = true;
						}
						else if( pars.Count == 1 && pars.ContainsKey("IcoGd") )
						{
							m_sNavApp = "X" + pars["IcoGd"];
						}
					}
					
					if( !bIgnorePars )
					{
						if( m_sNavApp.Length == 0 ) m_sNavApp = m_sExtension_Unknown;
						
						if( m_sNavApp.Length > 0 )
						{
							m_sNavUriQSV = e.Uri.ToString();
							m_tmrQSV.Start();
							
							//RscFs.WriteTextFile( "NavigationUri.txt", e.Uri.ToString(), true );
							
							bDenieLoad = true;
						}
					}
				}
				else
				{
					string sPathNotExitOnBack = RscKnownFolders.GetTempPath( "Launcher" ) + "\\" + "NotExitOnBack.txt";
					
					RscStore store = new RscStore();
					
					if( !store.FileExists( sPathNotExitOnBack ) )
					{
						bDenieLoad = true;
						
						//Update Live Tiles if any...
						SysTiles st = new SysTiles();
						st.DoUpdate( false, null );
						
						m_tmrExitOnBack.Start();
					}
				}
			}
			else
			{
				//TEST ONLY!!!
				/*
				string sPathNotExitOnBack = RscFs.GetRscTempPath( "Launcher" ) + "\\" + "NotExitOnBack.txt";
				RscStore store = new RscStore();
				if( !store.FileExists( sPathNotExitOnBack ) )
				{
					bDenieLoad = true;
						
					m_tmrExitOnBack.Start();
				}
				*/
			}
			if( !bDenieLoad )
			{
				m_bNavToCalledWithNoArg = true;
				if( m_sScreenToDo.Height > 0 && m_sScreenToDo.Width > 0 )
				{
					BuildPage(m_sScreenToDo);
					m_sScreenToDo = new Size(0, 0);
				}
			}
			
			// //
			//
			// XNA XNA XNA
			
			/*
           	MediaLibrary library = new MediaLibrary();
			
			const String _playSongKey = "playSong";
			
			if (NavigationContext.QueryString.ContainsKey(_playSongKey))
			{
				MessageBox.Show( NavigationContext.QueryString[_playSongKey] );
			}
			else if (MediaPlayer.State == MediaState.Playing)
            {
                // A song was already playing when we started.
                MessageBox.Show( MediaPlayer.Queue.ActiveSong.Name );
            }
			*/
			
			//
			// //
		}

		private void m_tmrQSV_Tick(object sender, System.EventArgs e)
		{
			m_tmrQSV.Stop();
			
			if( m_sNavApp.Length == 0 ) return;
			
			string sUri = "";
			
			string sPathNotExitOnBack = RscKnownFolders.GetTempPath( "Launcher" ) + "\\" + "NotExitOnBack.txt";
			
			RscStore store = new RscStore();
			
			if( m_sNavApp[0] == 'X' )
			{
				store.DeleteFile( sPathNotExitOnBack );
				
				string gdStr = m_sNavApp.Substring(1);
					
				string sStFldr = RscKnownFolders.GetTempPath("ShellTiles", "" );
				bool bTmp;
				string sTxt = store.ReadTextFile( sStFldr + "\\" + gdStr + ".txt", "", out bTmp );
				if( sTxt.Length > 0 )
				{
					string [] astr = sTxt.Split( new String [] {"\r\n"}, StringSplitOptions.None);
					string sNavTo = astr[0];
					string sArgs = "";
					if( astr.Length > 1 )
						sArgs = astr[1];
					
					NavToPage( sNavTo, sArgs, false );
				}
			}
			else
			{
				
				if( m_sNavApp.IndexOf( ";component/" ) >= 0 )
				{
					sUri = m_sNavUriQSV.Replace( "/MainPage.xaml?",
						m_sNavApp + ".xaml?ExitOnBack=True&" );
				}
				else
				{
					sUri = m_sNavUriQSV.Replace( "/MainPage.xaml?",
						"/" + m_sNavApp + ";component/" + "MainPage" + ".xaml?ExitOnBack=True&" );
				}
				
				store.WriteTextFile( sPathNotExitOnBack, sUri, true );
			
				//RscFs.WriteTextFile( "NavigationUri2.txt", sUri, true );
				//MessageBox.Show( sUri );
				
				this.NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
			}
		}

		private void m_tmrExitOnBack_Tick(object sender, System.EventArgs e)
		{
			m_tmrExitOnBack.Stop();
			
			this.NavigationService.GoBack();
		}
		
       	protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( m_iPgIdxCur != 0 )
			{
				m_iPgIdxCur = 0;
				ReBuildCurrentPage(ContentPanel.ActualWidth, ContentPanel.ActualHeight);
				
				e.Cancel = true;
				
				return;
			}
									
			//Update Live Tiles if any...
			SysTiles st = new SysTiles();
			st.DoUpdate( false, null );

		}

		private void MainPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
		{
			m_tmr.Stop();
		}
		
		protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
    	{
			m_bInThisApp = false;
		}
		
		private void _AddViewElement(string sStatus, int iPageIndex, string sID, int iSecTick = 0)
		{
			m_iPgIdxMin = Math.Min(m_iPgIdxMin, iPageIndex);
			m_iPgIdxMax = Math.Max(m_iPgIdxMax, iPageIndex);
			
			m_ve.Add( new ViewElement() { PageIndex = iPageIndex, ID = sID, Image = "", PageNavTo = "",
				Title = "", BkColor = m_Theme.ThemeColors.IconBack, SecTick = iSecTick, SecCntr = 0, Status = sStatus } );
		}
		private void _AddViewElement(string sStatus, int iPageIndex, string sID, string sImage = "", string sPageNavTo = "", string sPageArgs = "", string sTitle = "")
		{
			m_iPgIdxMin = Math.Min(m_iPgIdxMin, iPageIndex);
			m_iPgIdxMax = Math.Max(m_iPgIdxMax, iPageIndex);
			
			m_ve.Add( new ViewElement() { PageIndex = iPageIndex, ID = sID, Image = sImage, PageNavTo = sPageNavTo, PageArgs = sPageArgs,
				Title = sTitle, BkColor = m_Theme.ThemeColors.IconBack, SecTick = 0, SecCntr = 0, Status = sStatus } );
		}
		private void _AddViewElement(string sStatus, int iPageIndex, string sID, string sPageNavTo, string sTitle, System.Windows.Media.Color bkClr)
		{
			m_iPgIdxMin = Math.Min(m_iPgIdxMin, iPageIndex);
			m_iPgIdxMax = Math.Max(m_iPgIdxMax, iPageIndex);
			
			m_ve.Add( new ViewElement() { PageIndex = iPageIndex, ID = sID, Image = "", PageNavTo = sPageNavTo,
				Title = sTitle, BkColor = bkClr, SecTick = 0, SecCntr = 0, Status = sStatus } );
		}
		
		private void ReInitStatus()
		{
			
			int iIdx;
			string sName;
			Image img;
			string fName;
			
			RscStore store = new RscStore();
			
			panelPreView.Children.Clear();
			
			for( iIdx = m_iPgIdxMin; iIdx <= m_iPgIdxMax; iIdx++ )
			{
				sName = "imgPre_" + iIdx.ToString();
				
				//<Image x:Name="imgPre0" Margin="0,3,3,3" Width="30" Height="30" Source="Bk001_portrait.jpg" Stretch="Fill"/>
				
				img = new Image();
				img.Name = sName;
				img.Margin = new Thickness(0, 3, 3, 3);
				img.Width = 48;
				img.Height = 80;
				img.Stretch = Stretch.Fill;
				if( iIdx != m_iPgIdxCur )
				{
					img.Opacity = 0.5;
				}
				panelPreView.Children.Add(img);
				
				fName = RscKnownFolders.GetTempPath( "Launcher" ) + "\\" + "Page_" + iIdx.ToString() + ".tn.jpg";
				if( store.FileExists(fName) )
				{
			
					System.IO.Stream stream = store.GetReaderStream( fName );
				
					BitmapImage bmp = new BitmapImage();
					bmp.SetSource(stream);
					stream.Close();
				
					img.Source = bmp;
					img.Width = bmp.PixelWidth / 10;
					img.Height = bmp.PixelHeight / 10;
				}
				else
				{
					img.Source = m_Theme.GetImage("Images/Bk001_portrait.jpg");
				}
				
			}
			
			m_tmrStatAutoHide.Stop();
			if( gridStatus.Visibility == Rsc.Collapsed )
			{
				gridStatus.Visibility = Rsc.Visible;
			}
			m_tmrStatAutoHide.Start();
		}

		private void m_tmrStatAutoHide_Tick(object sender, System.EventArgs e)
		{
			m_tmrStatAutoHide.Stop();
			
			if( gridStatus.Visibility == Rsc.Visible )
			{
				gridStatus.Visibility = Rsc.Collapsed;
				
				lblStatus.Text = "Ready...";
			}
		}
		
		private void SetStatus( string sStatus )
		{
			m_tmrStatAutoHide.Stop();
			lblStatus.Text = sStatus;
			if( gridStatus.Visibility ==Rsc.Collapsed )
			{
				gridStatus.Visibility = Rsc.Visible;
			}
			m_tmrStatAutoHide.Start();
		}
		
		private void Touch_FrameReported(object sender, System.Windows.Input.TouchFrameEventArgs e)
        {
			if( !m_bInThisApp ) return;
			
			TouchPoint primaryTouchPoint = e.GetPrimaryTouchPoint(null);
			if( primaryTouchPoint == null ) return;
			
			switch( primaryTouchPoint.Action )
			{
				
				case TouchAction.Down :
					m_ptTouchDown = primaryTouchPoint.Position;
					break;
					
				case TouchAction.Move :
					if( m_grid != null )
					{
						double dCX = 0;
						
						if( ContentPanel.ActualWidth < ContentPanel.ActualHeight )
							dCX = primaryTouchPoint.Position.X - m_ptTouchDown.X;
						else
							dCX = primaryTouchPoint.Position.Y - m_ptTouchDown.Y;
						
						//Smaller movement...
						dCX = dCX / 5;
							
						m_grid.Margin = new Thickness(dCX, 0, -dCX, 0);
						imgBk.Margin = new Thickness(dCX, 0, -dCX, 0);
					}
					
					break;
					
				case TouchAction.Up :
					
					if( m_grid != null )
					{
						m_grid.Margin = new Thickness(0);
						imgBk.Margin = new Thickness(0);
					}
					
					int iDistance = 0;
					int iDistanceOther = 0;
					
					if( ContentPanel.ActualWidth < ContentPanel.ActualHeight )
					{
						iDistance = (int) (primaryTouchPoint.Position.X - m_ptTouchDown.X);
						iDistanceOther = (int) (primaryTouchPoint.Position.Y - m_ptTouchDown.Y);
					}
					else
					{
						iDistance = (int) (primaryTouchPoint.Position.Y - m_ptTouchDown.Y);
						iDistanceOther = (int) (primaryTouchPoint.Position.X - m_ptTouchDown.X);
					}
					if( iDistanceOther < 0 ) iDistanceOther *= -1;
					
					//SetStatus(iDistanceOther.ToString());
					
					if( iDistance > 50 && iDistanceOther < 200 )
					{
						//lblStatus.Text = "...to the Right (" + iDistance.ToString() + ")...";
						if( m_iPgIdxCur > m_iPgIdxMin )
						{
							m_iPgIdxCur--;
							ReBuildCurrentPage(ContentPanel.ActualWidth, ContentPanel.ActualHeight);
						}
					}
					else if( iDistance < -50 && iDistanceOther < 200 )
					{
						//lblStatus.Text = "...to the Left (" + iDistance.ToString() + ")...";
						if( m_iPgIdxCur < m_iPgIdxMax )
						{
							m_iPgIdxCur++;
							ReBuildCurrentPage(ContentPanel.ActualWidth, ContentPanel.ActualHeight);
						}
					}
					break;
			}			
		}

		private void ContentPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			
			//Defided in xaml as shell:SystemTray.IsVisible="False"
			//Microsoft.Phone.Shell.SystemTray.IsVisible = false;
			
			if( !m_tmr.IsEnabled ) m_tmr.Start();
			
			if( !m_bNavToCalledWithNoArg )
			{
				m_sScreenToDo = e.NewSize;
				
				if( e.NewSize.Width < e.NewSize.Height )
					imgBk.Source = new BitmapImage(new Uri("SplashScreenImage.jpg", UriKind.Relative));
				else
					imgBk.Source = new BitmapImage(new Uri("SplashScreenImage_landscape.jpg", UriKind.Relative));
			
				return;
			}
			
			m_sScreenToDo = new Size(0, 0);
			
			BuildPage( e.NewSize );
		}
		
		private void BuildPage( Size s )
		{
			if( s.Width < s.Height )
				imgBk.Source = m_Theme.GetImage("Images/Bk001_portrait.jpg");
			else
				imgBk.Source = m_Theme.GetImage("Images/Bk001_landscape.jpg");
			
			ReBuildCurrentPage(s.Width, s.Height);
		}
		
		private void ReBuildCurrentPage(double dWidth, double dHeight)
		{
			m_tmrtrg.Clear();
			
			if( m_grid != null )
			{
				ContentPanel.Children.Remove(m_grid);
				m_grid = null;
			}
				
			RowDefinition rd;
			ColumnDefinition cd;
			Button btn;
			Image img, img2;
			int iX, iY;
			int iCX, iCY;
			double dGapX, dGapY;
			int iVeIdx;
			
			iCX = (int) dWidth / iBtnDimension;
			dGapX = (dWidth - (iCX * iBtnDimension)) / (iCX + 1);
			
			iCY = (int) dHeight / iBtnDimension;
			dGapY = (dHeight - (iCY * iBtnDimension)) / (iCY + 1);
			
			m_grid = new Grid();
			m_grid.Name = "grd";
			for( iY = 0; iY < iCY; iY++ )
			{
				rd = new RowDefinition(); rd.Height = GridLength.Auto;
				m_grid.RowDefinitions.Add(rd);
			}
			for( iX = 0; iX < iCX; iX++ )
			{
				cd = new ColumnDefinition(); cd.Width = GridLength.Auto;
				m_grid.ColumnDefinitions.Add(cd);
			}
			//
			ContentPanel.Children.Add(m_grid);
			
			iVeIdx = -1;
			iY = 0;
			iX = -1;
			for(;;)
			{
				iVeIdx++;
				if( iVeIdx >= m_ve.Count ) break;
				
				if( m_ve[iVeIdx].PageIndex != m_iPgIdxCur ) continue;
				
				iX++;
				if( iX >= iCX )
				{
					iX = 0;
					iY++;
					if( iY >= iCY ) break;
				}
				
				btn = new Button();
				if( m_ve[iVeIdx].Image != "" )
				{
					img = new Image();
					img.Margin = new Thickness(dGapX, dGapY, 0, 0);
					
					if( m_ve[iVeIdx].Image.IndexOf( "isostore:\\" ) == 0 )
					{
						
						string sPath = m_ve[iVeIdx].Image.Substring(10);
						
						RscStore store = new RscStore();
						
						if( store.FileExists(sPath) )
						{
							System.IO.Stream stream = store.GetReaderStream( sPath );
							
							BitmapImage bmp = new BitmapImage();
							bmp.SetSource(stream);
							stream.Close();
							
							img.Source = bmp;
						}
						else
						{
							img.Source = m_Theme.GetImage("Images/Img001_Dummy.jpg");
						}
					}
					else
					{
						img.Source = m_Theme.GetImage(m_ve[iVeIdx].Image);
					}
					
					img.Width = iIconDimension;
					img.Height = iIconDimension;
					img.SetValue(Grid.RowProperty, iY);
					img.SetValue(Grid.ColumnProperty, iX);
					m_grid.Children.Add(img);
					
					m_ve[iVeIdx].tmpImg = img;
					
					switch( m_ve[iVeIdx].ID )
					{
						case "sysWiFi" :
						case "sysLt" :
						//case "sysBT" :
						{
							img2 = new Image();
							img2.Margin = new Thickness(dGapX + (iIconDimension - iIconChbDim) - 3,
								dGapY + (iIconDimension - iIconChbDim) - 3, 0, 0);
							img2.Width = iIconChbDim;
							img2.Height = iIconChbDim;
							img2.SetValue(Grid.RowProperty, iY);
							img2.SetValue(Grid.ColumnProperty, iX);
							m_grid.Children.Add(img2);
							
							m_ve[iVeIdx].tmpImgChb = img2;
							
							m_ve[iVeIdx].SecCntr = 0;
							m_ve[iVeIdx].SecTick = 1;
							m_tmrtrg.Add(btn);
							
							break;
						}
					}
					
					
				}
				//
				if( m_ve[iVeIdx].Image != "" )
				{
					btn.Opacity = 0.5;
					btn.BorderThickness = new Thickness(0);
					
					if( m_ve[iVeIdx].ID == "sysPg" )
					{
						if( m_ve[iVeIdx].Title.Length > 0 )
						{
							btn.Content = m_ve[iVeIdx].Title;
							
							//btn.Background = new SolidColorBrush(Colors.White);
							
							//btn.Foreground = new SolidColorBrush(Colors.Black);
							
							//btn.BorderBrush = new SolidColorBrush(Colors.Black);
							//btn.BorderThickness = new Thickness(1);
							
							btn.FontSize = 14;
							
							btn.Opacity = 1; //0.8;
							btn.Foreground = new SolidColorBrush( Colors.Blue );
							
							m_ve[iVeIdx].tmpImg.Opacity = 0.5; //0.1; //0.5;
						}
					}
				}
				else
				{	
					
					Brush brBk = new SolidColorBrush(m_ve[iVeIdx].BkColor);
					
					double dFs = 0;
					if( m_ve[iVeIdx].ID == "sysPg" )
						dFs = 14;
					else
						dFs = 18;
					
					Brush brBkCust = null;
					Brush brForeCust = null;
					double dFontSize = 0;
					btn.Content = GetVeContentEx(iVeIdx, out brBkCust, out brForeCust, out dFontSize);
					if( brBkCust != null ) brBk = brBkCust;
					if( brForeCust != null )
						btn.Foreground = brForeCust;
					else
						btn.Foreground = new SolidColorBrush( m_Theme.ThemeColors.IconFore );
					if( dFontSize > 0 ) dFs = dFontSize;
					
					btn.BorderThickness = new Thickness(1);
					btn.BorderBrush = new SolidColorBrush( m_Theme.ThemeColors.IconBorder );
					
					btn.Background = brBk; //new SolidColorBrush(m_ve[iVeIdx].BkColor);
					
					btn.FontSize = dFs;
					
					btn.Opacity = 0.75;
					//btn.IsEnabled = false;
					
					switch( m_ve[iVeIdx].ID )
					{
						case "sysTm" :
						case "sysDtFull" :
						case "sysDtDay" :
						case "sysCellNet" :
						case "sysCnt_Note" :
						case "sysCnt_Anni" :
						case "sysCnt_WebDog" :
						case "sysCnt_Event" :
						case "sysCnt_PerDay" :
						{
							m_ve[iVeIdx].SecCntr = 0;
							m_tmrtrg.Add(btn);
							break;
						}
					}
				}
				btn.Tag = iVeIdx;
				btn.Margin = new Thickness(dGapX, dGapY, 0, 0);
				btn.Click += new System.Windows.RoutedEventHandler(btn_Click);
				/*
				btn.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(btn_MouseLeftButtonDown);
				*/
				btn.MouseEnter += new System.Windows.Input.MouseEventHandler(btn_MouseEnter);
				//btn.MouseLeave += new System.Windows.Input.MouseEventHandler(btn_MouseLeave);
				btn.Width = iBtnDimension;
				btn.Height = iBtnDimension;
					
				btn.SetValue(Grid.RowProperty, iY);
				btn.SetValue(Grid.ColumnProperty, iX);
				m_grid.Children.Add(btn);
			}
			
			m_tmrSavePanelShot.Start();
		}

		private void m_tmrSavePanelShot_Tick(object sender, System.EventArgs e)
		{
			m_tmrSavePanelShot.Stop();
			
			if( m_grid == null ) return;
			
			string fName;
			fName = RscKnownFolders.GetTempPath( "Launcher" ) + "\\" + "Page_" + m_iPgIdxCur.ToString() + ".tn.jpg";
			SaveControlShot(m_grid, fName);
			
			ReInitStatus();
		}

		private void m_tmr_Tick(object sender, System.EventArgs e)
		{
			if( !m_bInThisApp ) return;
			
			for( int iIdx = 0; iIdx < m_tmrtrg.Count; iIdx++ )
			{
				Button btn = m_tmrtrg[ iIdx ];
				
				int iVeIdx;
				iVeIdx = (int) btn.Tag;
				
				m_ve[iVeIdx].SecCntr++;
				if( m_ve[iVeIdx].SecCntr < m_ve[iVeIdx].SecTick ) continue;
				m_ve[iVeIdx].SecCntr = 0;
				
				string sImgChb = "Images/CheckOff.jpg";
				double dChbOpa = 1; //0.5;
				
				//DEBUG...
				//RscFs.WriteTextFile( m_ve[iVeIdx].ID + ".txt", m_tmrtrg.Count.ToString() + " | " + DateTime.Now.ToString(), true );
				
				switch( m_ve[iVeIdx].ID )
				{
					case "sysWiFi" :
					{						
						if( DeviceNetworkInformation.IsWiFiEnabled )
						{
							sImgChb = "Images/CheckOn.jpg";
							dChbOpa = 1;
						}
							
						m_ve[iVeIdx].tmpImgChb.Source = m_Theme.GetImage(sImgChb);
						m_ve[iVeIdx].tmpImgChb.Opacity = dChbOpa;
						break;
					}
					
					// VERY-VERY SLOW!!!
					/*
					case "sysBT" :
					{
						bool bBT = false;
						
						//bBT = Windows.Devices.Bluetooth.PeerFinder.AllowBluetooth;
						//bBT = Windows.Networking.Proximity.PeerFinder.AllowBluetooth;
						bBT = Ressive.BlueTooth.RscBlueTooth.Enabled;
						
						if( bBT )
						{
							sImgChb = "Images/CheckOn.jpg";
							dChbOpa = 1;
						}
						
						m_ve[iVeIdx].tmpImgChb.Source = m_dil.GetImage(sImgChb);
						m_ve[iVeIdx].tmpImgChb.Opacity = dChbOpa;
						break;
					}
					*/
					
					default :
					{
						Brush brBkCust = null;
						Brush brForeCust = null;
						double dFontSize = 0;
						btn.Content = GetVeContentEx( iVeIdx, out brBkCust, out brForeCust, out dFontSize );
						if( brBkCust != null ) btn.Background = brBkCust;
						if( brForeCust != null ) btn.Foreground = brForeCust;
						if( dFontSize > 0 ) btn.FontSize = dFontSize;
						break;
					}
				}
				
			}
		}
		
		private string GetVeContentEx( int iVeIdx, out Brush brBk, out Brush brFore, out double dFontSize )
		{
			
			brBk = null;
			brFore = null;
			dFontSize = 0;
			string sErr = "";
			string sNotiTitle = "";
			string sNotiContent = "";
			string sNotiSound = "";
			
			string sInfoToChngChk = "";
			
			string sCnt = "";
			
			switch( m_ve[ iVeIdx ].ID )
			{
				case "sysPg" :
				{
					sCnt += m_ve[iVeIdx].Title;
					return sCnt;
				}
			}
			
			SysTiles st = new SysTiles();
			return st.GetInfo( false, m_ve[iVeIdx].ID, out brBk, out brFore, out dFontSize, out sErr,
				out sNotiTitle, out sNotiContent, out sNotiSound, false, null, out sInfoToChngChk );
		}
		
		private void NavToPage( string sPageName, string sPageArgs, bool bNotExitOnBack = true )
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

			if( bNotExitOnBack )
			{
				string sPathNotExitOnBack = RscKnownFolders.GetTempPath( "Launcher" ) + "\\" + "NotExitOnBack.txt";
				
				RscStore store = new RscStore();

				store.WriteTextFile( sPathNotExitOnBack, sUri, true );
			}
			
			this.NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}
		
		private bool PinTile( int iVeIdx )
		{
			switch( m_ve[ iVeIdx ].ID )
			{
				
				case "sysPg" :
				{
					
					RscShellTileManager.CleanUpShellTileData( );
					
					SysTiles st = new SysTiles();
					st.Create( m_ve[ iVeIdx ].PageNavTo, m_ve[ iVeIdx ].PageArgs,
						m_ve[ iVeIdx ].Image, m_ve[ iVeIdx ].Status );

					return true;
					//break;
				}
				
				case "sysTm" :
				case "sysDtFull" :
				case "sysDtDay" :
				case "sysFsFree" :
				case "sysCellNet" :
				case "sysCnt_Note" :
				case "sysCnt_Anni" :
				case "sysCnt_WebDog" :
				case "sysCnt_Event" :
				case "sysCnt_PerDay" :
				case "sysBatPow" :
				{
					RscShellTileManager.InitShellTask( csTaskName, false );
					
					RscShellTileManager.CleanUpShellTileData( );
					
					SysTiles st = new SysTiles();
					st.Create( m_ve[ iVeIdx ].PageNavTo, m_ve[ iVeIdx ].PageArgs,
						m_ve[ iVeIdx ].Image, m_ve[ iVeIdx ].Status, m_ve[ iVeIdx ].ID );

					return true;
					//break;
				}
				
			}
			
			return false;
		}

		private void btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			
			Button btn;
			int iVeIdx;
			
			btn = ((Button) sender);
			iVeIdx = (int) btn.Tag;
			
			if( iVeIdx == m_iIdxTileEnter )
			{
				TimeSpan ts = DateTime.Now - m_dtTileEnter;
				if( ts.TotalMilliseconds > 1500 )
				{
					if( MessageBoxResult.OK == MessageBox.Show( "Do you want to Pin tile to Start Screen?\r\n\r\n(Press Back to Cancel...)" ) )
					{
						if( !PinTile( iVeIdx ) )
							MessageBox.Show( "Unable to Pin Tile (" + m_ve[ iVeIdx ].ID + ")!" );
					}
					return;
				}
			}
			
			switch( m_ve[ iVeIdx ].ID )
			{
				
				case "sysPg" :
				{
					NavToPage( m_ve[ iVeIdx ].PageNavTo, m_ve[ iVeIdx ].PageArgs );
					break;
				}
				
				case "sysWiFi" :
				{
			
					ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
					connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.WiFi;
					connectionSettingsTask.Show();
					
					break;
				}
				
				case "sysBT" :
				{
			
					ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
					connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.Bluetooth;
					connectionSettingsTask.Show();
					
					break;
				}
				
				case "sysSm" :
				{
					Ressive.MediaLib.RscMediaLib.StopMusic();
					
					break;
				}
				
				case "sysSs" :
				{
					
					DateTime dNow = DateTime.Now;
					string fName = dNow.Year.ToString() +
							"_" + RscUtils.pad60(dNow.Month) + "_" +
							RscUtils.pad60(dNow.Day) + "_" + RscUtils.pad60(dNow.Hour) +
							"_" + RscUtils.pad60(dNow.Minute) + "_" +
							RscUtils.pad60(dNow.Second) + ".jpg";
					
					string fPath = RscKnownFolders.GetMediaPath("Screen Shot") + "\\" + fName;
					
					SaveControlShot(ContentPanel, fPath);
	
					break;
				}
				
				case "sysBatt" :
				{
					MessageBox.Show( "(TIP: Swipe vertically downward on System Tray above to show battery info.)" );
					break;
				}
				
				case "sysCellNet" :
				{
			
					ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
					connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.Cellular;
					connectionSettingsTask.Show();
					
					break;
				}
				
				case "sysCnt_Note" :
				{
					NavToPage( "/RscLearnXamlV10;component/NoteEditor", "" );
					break;
				}
				
				case "sysCnt_Anni" :
				{
					NavToPage( "/RscLearnXamlV10;component/Anniversary", "" );
					break;
				}
				
				case "sysCnt_WebDog" :
				{
					RscStore store = new RscStore();
					
					if( store.FileExists( "A:\\Documents\\WebDogUri.txt" ) )
					{
						bool bTmp = false;
						string sUri = store.ReadTextFile( "A:\\Documents\\WebDogUri.txt", "", out bTmp );
						if( sUri.Length > 0 )
						{
							NavToPage( "/RscLearnXamlV10;component/FaviconViewer", "?uri=" + sUri );
						}
					}
					else
					{
						NavToPage( "/RscLearnXamlV10;component/FaviconFinder", "" );
					}
					break;
				}
				
				case "sysCnt_Event" :
				{
					NavToPage( "/Lib_RscViewers;component/RscViewer_TextV10", "?folder=A:\\System\\Events" );
					break;
				}
				
				case "sysCnt_PerDay" :
				{
					NavToPage( "/RscLearnXamlV10;component/PerDay", "" );
					break;
				}
				
				default :
				{
					if( RscShellTileManager.HasShellTask( csTaskName ) )
					{
						if( MessageBoxResult.OK == MessageBox.Show( "Do you want to Update Start Screen Tiles immediately?\n\n(press Back to Cancel...)" ) )
						{					
							SysTiles st = new SysTiles();
							st.DoUpdate( false, null );
						}
					}
					else
					{
						MessageBox.Show( "TIP: To place an icon to Start Screen as Live Tile tap it for about 1.5 seconds." ); //"Unable to found ScheduledActionService named: " + sTaskName );
					}

					break;
				}
				
			}
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

		private void btn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			
			Button btn;
			int iVeIdx;
			
			btn = ((Button) sender);
			iVeIdx = (int) btn.Tag;
			
			/*
			if( m_iIdxTileEnter != iVeIdx )
			{
			*/
				m_dtTileEnter = DateTime.Now;
				m_iIdxTileEnter = iVeIdx;
			/*
			}
			*/
			
			SetStatus( m_ve[ iVeIdx ].Status );
		}

		/*
		private void btn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			m_iIdxTileEnter = -1;
		}
		*/
		
    }
	
	public class ViewElement
	{
		public int PageIndex {get; set;}
		public string ID { get; set; }
		public string Image { get; set; }
		public string PageNavTo { get; set; }
		public string PageArgs { get; set; }
		public string Title { get; set; }
		public System.Windows.Media.Color BkColor { get; set; }
		public int SecTick { get; set; }
		public int SecCntr { get; set; }
		public Image tmpImgChb { get; set; }
		public Image tmpImg { get; set; }
		public string Status { get; set; }
	}	
	
}
