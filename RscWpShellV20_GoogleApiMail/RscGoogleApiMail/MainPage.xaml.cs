using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using RscGoogleApiMail.Resources;

using System.Windows.Media;
using System.Text;
using System.ComponentModel;

using Ressive.Utils;
using Ressive.Store;
using Ressive.Theme;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

using Ressive.Formats;

using Ressive.GoogleApi;

using Ressive.ShellTiles;

using RscGoogleApiMail_Lib;

namespace RscGoogleApiMail
{
	
    public partial class MainPage : PhoneApplicationPage
    {
		
		const string csTaskName = "RscGoogleApiMail_80_Task";
		
		const string csVerUp = "version_upgrade";
		
		const string csViewersAssy = "Lib_RscViewers";
		
		const int ciMaxAtOnce = 20;
		const double cdFontSize = 22;
		
		const string csMoreItems = "more_items";
		
		RscAppFrame m_AppFrame;
		RscIconButton m_btnInit;
		
		/*
		RscIconButton m_btnExpandAll;
		RscIconButton m_btnCollapseAll;
		*/
		
		RscIconButton m_btnAddTile;
		ImageSource m_isInfErrOn;
		ImageSource m_isInfErrOff;
		RscIconButton m_btnErrsOnOff;
		RscIconButton m_btnCleanUp;
		RscIconButton m_btnLogOut;
		
		ImageSource m_isCheckOn;
		//ImageSource m_isCheckOff;
		
		RscPageArgsRetManager m_AppArgs;
		
		Size m_sContentPanel = new Size(100, 100);
		
		RscGoogleAuth m_gAuth = null;
		
		RscTreeLbItemList m_aTI = null;
		
		string m_sUserIDlast = "";
		
		TreeLbItem m_tiSum = null;
		
        public MainPage()
        {
            InitializeComponent();
			
			
 			//StandAlone app...
			bool bStandAloneApp = false;
			Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			if( GlobalDILholder.Tag == null )
			{
				bStandAloneApp = true;
				GlobalDILholder.Tag = new RscTheme( true, "Theme", "Current", "Default" );
			}
			//StandAlone app...
			
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", ".G Mail 1.0", "Images/Ico001_GoogleApiMail.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			// ///////////////
			
			ToolBarPanel.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack );
			
			m_btnInit = new RscIconButton(ToolBarPanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible );
			m_btnInit.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_LogOn.jpg");
			m_btnInit.Click += new System.Windows.RoutedEventHandler(m_btnInit_Click);
			
			/*
			m_btnExpandAll = new RscIconButton(ToolBarPanel, Grid.ColumnProperty, 1, 50, 50, Rsc.Collapsed );
			m_btnExpandAll.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_TreeExpand.jpg");
			m_btnExpandAll.Click += new System.Windows.RoutedEventHandler(m_btnExpandAll_Click);
			
			m_btnCollapseAll = new RscIconButton(ToolBarPanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Collapsed );
			m_btnCollapseAll.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_TreeCollapse.jpg");
			m_btnCollapseAll.Click += new System.Windows.RoutedEventHandler(m_btnCollapseAll_Click);
			*/
			m_btnAddTile = new RscIconButton(ToolBarPanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Collapsed );
			m_btnAddTile.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_AddTile.jpg");
			m_btnAddTile.Click += new System.Windows.RoutedEventHandler(m_btnAddTile_Click);
			
			m_isInfErrOn = m_AppFrame.Theme.GetImage("Images/Btn001_InfErrOn.jpg");
			m_isInfErrOff = m_AppFrame.Theme.GetImage("Images/Btn001_InfErrOff.jpg");
			m_btnErrsOnOff = new RscIconButton(ToolBarPanel, Grid.ColumnProperty, 4, 50, 50, Rsc.Collapsed );
			m_btnErrsOnOff.Image.Source = m_isInfErrOn;
			m_btnErrsOnOff.Click += new System.Windows.RoutedEventHandler(m_btnErrsOnOff_Click);
			
			m_btnCleanUp = new RscIconButton(ToolBarPanel, Grid.ColumnProperty, 5, 50, 50, Rsc.Collapsed );
			m_btnCleanUp.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_ClearCache.jpg");
			m_btnCleanUp.Click += new System.Windows.RoutedEventHandler(m_btnCleanUp_Click);
			
			m_btnLogOut = new RscIconButton(ToolBarPanel, Grid.ColumnProperty, 6, 50, 50, Rsc.Collapsed );
			m_btnLogOut.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_LogOut.jpg");
			m_btnLogOut.Click += new System.Windows.RoutedEventHandler(m_btnLogOut_Click);
			
			m_isCheckOn = m_AppFrame.Theme.GetImage("Images/CheckOn.jpg");
			//m_isCheckOff = m_AppFrame.Theme.GetImage("Images/CheckOff.jpg");
			
			/*
			m_txtPath = new TextBoxDenieEdit(true, true, ToolBarPanel, Grid.ColumnProperty, 1);
			m_txtPath.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack ); //Colors.LightGray);
			m_txtPath.Foreground = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightFore ); //Colors.Black);
			m_txtPath.FontSize = 16;
			m_txtPath.Text = "N/A";
			*/
			
			m_AppArgs = new RscPageArgsRetManager();
			
			//Used to DeBug...
			//m_AppFrame.ShowButtonNext( false );
			
			if( bStandAloneApp )
			{
				m_AppFrame.ShowButtonTools( true );
			}
			
			m_aTI = new RscTreeLbItemList( lbTree, m_AppFrame.Theme, "Images/Btn001_TreeExpand.jpg", "Images/Btn001_TreeCollapse.jpg");
			
			ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);
			
			m_AppFrame.AutoClick( m_btnInit, new System.Windows.RoutedEventHandler(m_btnInit_Click) );
        }
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if( m_AppArgs.Waiting )
			{				
				RscStore store = new RscStore();
				
				RscPageArgsRet appOutput = m_AppArgs.GetOutput();
				if( appOutput != null )
				{
					switch( appOutput.ID )
					{
						
						case "CleanUp" :
							
							m_AppArgs.Vipe();
							
							//Tools called...
							
							m_AppArgs.Clear();
							
							if( store.FolderExists( AppLogic.csSecretsFolder + "\\" + m_sUserIDlast ) )
							{
								MessageBox.Show( "Unable to delete files!");
							}
							else
							{
								m_btnCleanUp.Visibility = Rsc.Collapsed;
							}
							
							break;
						
						case "LogOut" :
							
							m_AppArgs.Vipe();
							
							//Tools called...
							
							m_AppArgs.Clear();
							
							if( store.FolderExists( AppLogic.csSecretsFolder + "\\" + m_sUserIDlast ) )
							{
								MessageBox.Show( "Unable to delete files!");
							}
							/*
							else
							{
							*/
								m_btnCleanUp.Visibility = Rsc.Collapsed;
								m_btnLogOut.Visibility = Rsc.Collapsed;
							/*
							}
							*/
							
							break;
							
					}
				}
			}
		}

		private void ContentPanel_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			bool bNoChng = (m_sContentPanel.Width == e.NewSize.Width && m_sContentPanel.Height == e.NewSize.Height);
			m_sContentPanel = e.NewSize;
			
			if( !bNoChng )
			{
				if( e.NewSize.Width < e.NewSize.Height )
					imgBk.Source = m_AppFrame.Theme.GetImage("Images/Bk001_portrait.jpg");
				else
					imgBk.Source = m_AppFrame.Theme.GetImage("Images/Bk001_landscape.jpg");
			}
		}
 
		private void m_AppFrame_OnExit(object sender, System.EventArgs e)
		{
			NavigationService.GoBack();
		}
		
       	protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
									
			//Update Live Tiles if any...
			SysTiles stm = new SysTiles();
			stm.DoUpdate( false, null );
		}
		
		private void m_btnInit_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_gAuth == null )
			{
				
				RscStore store = new RscStore();
				store.CreateFolderPath( AppLogic.csSecretsFolder );
				
				string sPath = AppLogic.csSecretsFolder + "\\" + "client_secret.json";
				bool bNotExists = false;
				string sJSon = store.ReadTextFile( sPath, "", out bNotExists );
				if( bNotExists )
				{
					MessageBox.Show( "File " + sPath + " does not exists!"
						+ "\n\n(NOTE: Drive A: reffers to Isolated Storage.)"
						+ "\n\n(NOTE: You can download missing file from FTP Cloud. Press Tools button in Caption Bar.)" );
					return;
				}
				
				string sErr = "";
				RscJSonItem json = RscJSon.FromResponseContetn( sJSon, out sErr );
				if( sErr.Length > 0 )
				{
					LogError( sErr );
					
					//Try to get values needed...
					//return;
				}
				
				//To see content of json...
				/*
				TreeLbItem tiJSon;				
				tiJSon = new TreeLbItem( m_aTI, null );
				tiJSon.Title = "client_secret.json";
				m_aTI.Add( tiJSon );				
				tiJSon.SetResponse( json, false );
				*/
				
				m_sUserIDlast = store.ReadTextFile( AppLogic.csSecretsFolder + "\\" + "UserIDlast.txt", "" );
				
				
				// //
				//
				
				m_gAuth = new RscGoogleAuth( json,
						RscGoogleScopes.UserinfoEmail
						+ " " + RscGoogleScopes.UserinfoProfile
						+ " " + RscGoogleScopes.Gmail,
						webBrowser1 );
				
				m_gAuth.Authenticated += new EventHandler(m_gAuth_Authenticated);
				m_gAuth.AuthenticationFailed += new EventHandler(m_gAuth_AuthenticationFailed);
				m_gAuth.ShowAuthPage += new EventHandler(m_gAuth_ShowAuthPage);
				m_gAuth.ResponseReceived += new Ressive.GoogleApi.RscGoogleAuth.ResponseReceived_EventHandler(m_gAuth_ResponseReceived);
				
				//bool bNotExists;
				m_gAuth.AuthResult = AppLogic.LoadAuthResult( out bNotExists );
			
				if( !bNotExists )
				{
					//ATT: Logged ON!!!
					m_btnLogOut.Visibility = Rsc.Visible;
				}

				//
				// //
				
				
				m_btnInit.Visibility = Rsc.Collapsed;
				
				/*
				m_btnExpandAll.Visibility = Rsc.Visible;
				m_btnCollapseAll.Visibility = Rsc.Visible;
				*/
				
				m_btnAddTile.Visibility = Rsc.Visible;
				
				//DO NOT!!!
				//m_btnLogOut.Visibility = Rsc.Visible;
				
				m_AppFrame.StatusText = "Initialized...";
				
				// //
				//
				
				TreeLbItem ti;
				
				ti = new TreeLbItem( m_aTI, null );
				ti.Title = "User Profile";
				ti.gr = GoogleRequest.UserInfo;
				m_aTI.Add( ti );
				
				if( m_sUserIDlast.Length > 0 )
				{
					AddRootContainers();
				}
				
				//
				// //
			}
		}
		
		private void AddRootContainers()
		{
			TreeLbItem ti;
			
			// //
			//
			
			if( !AppLogic.VersionUpgrade( m_sUserIDlast, true ) )
			{
				
				ti = new TreeLbItem( m_aTI, null );
				ti.Title = "Local Data - Version Upgrade needed...";
				ti.sID = csVerUp;
				ti.CustomBackColor = Colors.Red;
				ti.CustomForeColor = Colors.Yellow;
				m_aTI.Add( ti );
				
				return;
			}

			//
			// //
				
			/*
			ti = new TreeLbItem( m_aTI, null );
			ti.gr = GoogleRequest.GMail_Messages;
			m_aTI.Add( ti );
				
			ti = new TreeLbItem( m_aTI, null );
			ti.gr = GoogleRequest.GMail_Labels;
			m_aTI.Add( ti );
			*/
				
			ti = new TreeLbItem( m_aTI, null );
			ti.Title = "Reload Google-API threads...";
			ti.gr = GoogleRequest.GMail_Threads;
			m_aTI.Add( ti );
			
			DoExpColItem( ti );
			
			/*
			ti = new TreeLbItem( m_aTI, null );
			ti.gr = GoogleRequest.GMail_History;
			m_aTI.Add( ti );
				
			ti = new TreeLbItem( m_aTI, null );
			ti.gr = GoogleRequest.GMail_Drafts;
			m_aTI.Add( ti );
			*/
		}
		
        void m_gAuth_Authenticated(object sender, EventArgs e)
        {
			Deployment.Current.Dispatcher.BeginInvoke(() =>
            {			
				//ATT: Logged ON!!!
				m_btnLogOut.Visibility = Rsc.Visible;
			});
			
			AppLogic.SaveAuthResult( m_gAuth.AuthResult );
        }

        void m_gAuth_AuthenticationFailed(object sender, EventArgs e)
        {
        	MessageBox.Show("Please try again", "Login failed", MessageBoxButton.OK);
        }
		
		void m_gAuth_ShowAuthPage(object sender, EventArgs e)
		{
			/*
			Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
			*/
			
				webBrowser1.Visibility = Rsc.Visible;
				webBrowser1.Source = m_gAuth.AuthUri;
			
			/*
			});
			*/
		}
		
		void m_gAuth_ResponseReceived(object sender, RscGoogleAuthEventArgs e)
		{	
			bool bAddRootContainers;
			DoLoad( m_tiLoading, e.Content,
				e.Uri, e.ContentType, true, out bAddRootContainers );
						
			m_tiLoading.Loading = false;
			
			//VERY SLOW!!!
			//m_aTI.Refresh();
			
			m_tiLoading = null;
			
			if( bAddRootContainers )
				AddRootContainers();
		}
		
		private void DoLoad( TreeLbItem ti, string sContent, string sID, string sDetails, bool bSaveJSon, out bool bAddRootContainers )
		{
			bAddRootContainers = false;
			
			string sErr = "";
			RscJSonItem jsonRoot = null;	
			/*
			jsonRoot = new RscJSonItem();
			jsonRoot.ID = response.ResponseUri.ToString();
			jsonRoot.Name = "IRestResponse<Object>";
			jsonRoot.AddProperty( "Response Status", response.ResponseStatus.ToString() );
			jsonRoot.AddProperty( "Response Uri", response.ResponseUri.ToString() );
			jsonRoot.AddProperty( "Content Length", response.ContentLength.ToString() );
			*/
			jsonRoot = RscJSon.FromResponseContetn( jsonRoot, sContent, out sErr, sID, sDetails );
			if( sErr.Length > 0 )
			{
				LogError( sErr );
			}
			
			if( jsonRoot != null )
			{
				// //
				//
				
				string sErrorCode = "";
				if( jsonRoot.ChildCount > 0 )
				{
					if( jsonRoot.GetChild( 0 ).Name == "error" )
					{
						//For example: Required Scope not specified while LogOn!!!
						
						sErrorCode = jsonRoot.GetChildPropertyValue( 0, "code" );
						string sErrorMessage = jsonRoot.GetChildPropertyValue( 0, "message" );
						
						LogError( "Error response:\ncode: " + sErrorCode + "\nmessage: " + sErrorMessage );
					}
				}
				
				//
				// //
				
				//Show Error JSon!!!
				//if( sErrorCode.Length == 0 )
				{
					//Try to load result as is...
					GoogleRequest gr = GoogleUtils.GoogleRequestFromUrl( jsonRoot.ID );
					
					switch( gr )
					{
						
						case GoogleRequest.UserInfo :
						case GoogleRequest.GMail_Messages :
						case GoogleRequest.GMail_Message_Details :
						case GoogleRequest.GMail_Labels :
						case GoogleRequest.GMail_Threads :
						case GoogleRequest.GMail_History :
						case GoogleRequest.GMail_Drafts :
						{
							if( sErr.Length > 0 || (gr != GoogleRequest.GMail_Threads) )
							{
								ti.SetResponse( jsonRoot );
							}
							
							RscStore store = new RscStore();
							
							if( gr == GoogleRequest.UserInfo )
							{
								string sUserID = jsonRoot.GetPropertyValue( "id" );
								
								if( m_sUserIDlast.Length == 0 || m_sUserIDlast != sUserID )
								{
									if( sUserID.Length > 0 )
									{
										m_sUserIDlast = sUserID;
										store.WriteTextFile( AppLogic.csSecretsFolder + "\\" + "UserIDlast.txt", m_sUserIDlast, true );
										
										bAddRootContainers = true;
									}
								}
							}
							
							string sPath = "";
							string sFn = "";
							if( m_sUserIDlast.Length > 0 )
							{
								sPath = AppLogic.csSecretsFolder + "\\" + m_sUserIDlast;
								sFn = Uri2FileName( jsonRoot.ID );
							}
							if( bSaveJSon )
							{
								if( m_sUserIDlast.Length > 0 )
								{
									
									
									//ATT: We need up to date data always!!!
									if( gr == GoogleRequest.UserInfo )
									{
										store.CreateFolderPath( sPath );
										
										store.WriteTextFile( sPath + "\\" + sFn + ".json", sContent, true );
										
										m_btnCleanUp.Visibility = Rsc.Visible;
									}
									//ATT: We need up to date data always!!!
									
									
									if( gr == GoogleRequest.GMail_Threads )
									{
										if( sErr.Length == 0 )
										{
											
											//Saving threads to have older thread content...
											AppLogic.SaveThreadData( false, sPath, jsonRoot );
										
											m_btnCleanUp.Visibility = Rsc.Visible;
											
											ShowSavedThreadData( sPath, 0 );
											
										}
									}
									
									
								}
							}
							
							if( sErr.Length == 0 )
							{
								if( sFn.Length > 0 )
								{
									if( bSaveJSon )
										m_AppFrame.StatusText = "Downloaded..."; // + sPath + "\\" + sFn + ".json";
									else
										m_AppFrame.StatusText = "Loaded..."; // + sPath + "\\" + sFn + ".json";
								}
								else
								{
									if( bSaveJSon )
										m_AppFrame.StatusText = "Downloaded...";
									else
										m_AppFrame.StatusText = "Loaded...";
								}
							}
							
							break;
						}
						
						default :
						{
							//Unexpected...
							LogError( jsonRoot.ToDecoratedString() );
							break;
						}
						
					}
				}
			}
        }
		
		private void m_btnCleanUp_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( MessageBoxResult.OK != MessageBox.Show( "Do you really want to delete all saved response file (.json)?\n\n(Press Back to Cancel...)" ) )
				return;
			
			// //
			//
			
			logGrid.Visibility = Rsc.Collapsed;
			
			m_aTI.CollapseAll( true );
			
			//
			// //
			//
			
			if( m_sUserIDlast.Length > 0 )
			{
				RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
					m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "CleanUp" );
				
				appInput.SetData( 0, AppLogic.csSecretsFolder + "\\" + m_sUserIDlast );
				appInput.SetData( 1, "*.json" );
				appInput.SetData( 2, "recurse" );
				//appInput.SetData( 2, "" );
				
				appInput.SetData( 3, "showHidden" );
				//appInput.SetData( 3, "" );
				
				appInput.SetData( 4, "AutoDelete" );
				
				appInput.SetInput( "RscViewer_FindFilesV12" );
							
				this.NavigationService.Navigate( appInput.GetNavigateUri( csViewersAssy ) );
			}
			
			//
			// //
		}
		
		private void m_btnLogOut_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( MessageBoxResult.OK != MessageBox.Show( "Do you really want to log out\nand to delete all saved response files (.json)\nand browser chache?\n\n(Press Back to Cancel...)" ) )
				return;
			
			// //
			//
			
			logGrid.Visibility = Rsc.Collapsed;
			
			m_aTI.CollapseAll( true );
			
			//
			// //
			//
			
			m_gAuth.Logout();
			
			RscStore store = new RscStore();
			store.CreateFolderPath( AppLogic.csSecretsFolder );
			//Otherwise able to logon on next run...
			store.DeleteFile( AppLogic.csSecretsFolder + "\\" + "AUTH.xml" );
			
			//
			// //
			//
			
			//NOTE: Deleting all IE folders WILL NOT force the user to reenter eMail + Pw!!!
			
			string sIeCacheFolders = ";" + RscStore_IsoStore.Get_IE_Cache_FolderList( ';' );
			sIeCacheFolders = sIeCacheFolders.Replace( ";", ";A:\\" );
			
			//
			// //
			//
			
			if( m_sUserIDlast.Length > 0 )
			{
				RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
					m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "LogOut" );
				
				appInput.SetData( 0, AppLogic.csSecretsFolder + "\\" + m_sUserIDlast + sIeCacheFolders );
				appInput.SetData( 1, "*.*" );
				appInput.SetData( 2, "recurse" );
				//appInput.SetData( 2, "" );
				
				appInput.SetData( 3, "showHidden" );
				//appInput.SetData( 3, "" );
				
				appInput.SetData( 4, "AutoDelete" );
				
				appInput.SetInput( "RscViewer_FindFilesV12" );
							
				this.NavigationService.Navigate( appInput.GetNavigateUri( csViewersAssy ) );
			}
			
			//
			// //
		}
		
		/*
		private void m_btnExpandAll_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_AppFrame.StartTimer( "expand_all", LayoutRoot, 1, 0, false );
		}
		*/
		
		private void m_AppFrame_OnTimer(object sender, RscAppFrameTimerEventArgs e)
		{
			switch( e.Reason )
			{
				
				case csVerUp :
				{
					if( e.Pos == e.Max )
					{
						bool bDone = false;
						
						if( AppLogic.VersionUpgrade( m_sUserIDlast, false ) )
						{
							m_tiLoading.Title = "Local Data - Version Upgrade DONE!";
							m_tiLoading.CustomBackColor = Colors.Green;
							m_tiLoading.IsLeaf = true;
							
							bDone = true;
						}
							
						m_tiLoading.Loading = false;
						m_tiLoading = null;
						
						if( bDone )
						{
							AddRootContainers();
						}
						
						m_AppFrame.StatusText = ""; //To refresh mem info...
					}
					
					break;
				}
				
				/*
				case "expand_all" :
				{
					if( e.Pos == e.Max )
					{
						m_aTI.ExpandAll();
						
						m_AppFrame.StatusText = ""; //To refresh mem info...
					}
					
					break;
				}
				*/
				
			}
		}
		
		/*
		private void m_btnCollapseAll_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_aTI.CollapseAll( false );
			
			m_AppFrame.StatusText = ""; //To refresh mem info...
		}
		*/
		
		private void TreeLbItem_BtnExpCol_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoExpCol(sender);
			
			m_AppFrame.StatusText = ""; //To refresh mem info...
		}
		private void TreeLbItem_Title_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoExpCol(sender);
			
			m_AppFrame.StatusText = ""; //To refresh mem info...
		}
		
		private void TreeLbItem_BtnCustom1_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn = (Button) sender;
			TreeLbItem ti = (TreeLbItem) btn.Tag;
			
			DoAck( ti );
		}
		private void TreeLbItem_BtnCustom2_Click(object sender, System.Windows.RoutedEventArgs e)
		{
		}
		private void TreeLbItem_BtnCustom3_Click(object sender, System.Windows.RoutedEventArgs e)
		{
		}
		private void TreeLbItem_BtnCustom4_Click(object sender, System.Windows.RoutedEventArgs e)
		{
		}
		
		TreeLbItem m_tiLoading = null;
		private void DoExpCol(object sender)
		{			
			Button btn = (Button) sender;
			TreeLbItem ti = (TreeLbItem) btn.Tag;
			
			DoExpColItem( ti );
		}
		
		private void DoExpColItem( TreeLbItem ti )
		{
			
			if( ti.gr == GoogleRequest.None && (!ti.HasResponse) )
			{
				if( ti.Expanded )
				{
					ti.Collapse();
				}
				else if( !ti.IsLeaf && !ti.IsDetailsOnly )
				{
					if( ti.sID == csVerUp )
					{
						if( m_tiLoading != null )
						{
							MessageBox.Show( "Load in progress!\n\nPlease wait..." );
							return;
						}
						
						m_tiLoading = ti;
						m_tiLoading.Loading = true;
						
						m_AppFrame.StartTimer( csVerUp, LayoutRoot, 1, 0, false );
					}
					else if( ti.sID == csMoreItems )
					{
						int iStartIndex = Int32.Parse( ti.sHistoryID );
						
						string sPath = AppLogic.csSecretsFolder + "\\" + m_sUserIDlast;
						ShowSavedThreadData( sPath, iStartIndex, ti );
					}
					else
					{
						ti.Expand();
					}
				}
				
				return; //Nothing to load...
			}
			
			if( ti.Expanded )
			{
				ti.Collapse();
			}
			else
			{
				if( ti.HasResponse )
				{
					ti.Expand();
				}
				else
				{	
					if( m_tiLoading != null )
					{
						MessageBox.Show( "Load in progress!\n\nPlease wait..." );
						return;
					}
					
					string id = "";
					if( ti.Parameters != null )
					{
						id = ti.Parameters.GetPropertyValue( "id" );
					}				
					string sUriResource = "";
					string sBaseUrl = GoogleUtils.GoogleRequestBaseUrl( ti.gr, out sUriResource, "me", id );
					if( sBaseUrl.Length == 0 )
					{
						MessageBox.Show( "No Request Url found!" );
						return;
					}
						
					bool bLoadedFromFile = false;
					if( m_sUserIDlast.Length > 0 )
					{
						if( ti.gr != GoogleRequest.GMail_Threads )
						{
							string sPath = AppLogic.csSecretsFolder + "\\" + m_sUserIDlast;
							
							RscStore store = new RscStore();
							store.CreateFolderPath( sPath );
							
							string sFn = Uri2FileName( sBaseUrl + sUriResource );
							bool bTmp;
							string sJSon = store.ReadTextFile( sPath + "\\" + sFn + ".json", "", out bTmp );
							if( sJSon.Length > 0 )
							{
								bool bTmp2;
								DoLoad( ti, sJSon,
									sBaseUrl + sUriResource, "", false, out bTmp2 );
								
								m_btnCleanUp.Visibility = Rsc.Visible;
								
								bLoadedFromFile = true;
							}
						}
					}
					
					if( !bLoadedFromFile )
					{
						if( ti.gr == GoogleRequest.GMail_Threads )
						{
							m_tiSum = null;
							m_aTI.RemoveAll( ti );
						}
						
						m_tiLoading = ti;
						m_tiLoading.Loading = true;
						
						//VERY SLOW!!!
						//m_aTI.Refresh();
						
						m_gAuth.SendRequest( sBaseUrl, sUriResource );
					}
				}
			}
		}
		
		private void m_btnErrsOnOff_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( logGrid.Visibility != Rsc.Visible )
			{
				m_btnErrsOnOff.Visibility = Rsc.Visible;
				m_btnErrsOnOff.Image.Source = m_isInfErrOn;
				
				logGrid.Visibility = Rsc.Visible;
			}
			else
			{
				//m_btnErrsOnOff.Visibility = Rsc.Collapsed;
				m_btnErrsOnOff.Image.Source = m_isInfErrOff;
				
				logGrid.Visibility = Rsc.Collapsed;
			}
		}
		
		private void LogError( string sEvent )
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
				if( logGrid.Visibility != Rsc.Visible )
				{
					m_btnErrsOnOff.Visibility = Rsc.Visible;
					m_btnErrsOnOff.Image.Source = m_isInfErrOn;
					
					logGrid.Visibility = Rsc.Visible;
				}
				
				m_AppFrame.StatusText = "Error occured...";
				
				string [] astr = sEvent.Split('\n');
				
				int iInsert = 0;
				foreach( string s in astr )
				{
					string sA = s;
					string sB = "";
					if( sA.Length > 40 )
					{
						sB = " ..." + sA.Substring( 40 );
						sA = sA.Substring( 0, 40 ) + "...";
					}
					
					lstLog.Items.Insert(iInsert, sA);
					if( sB.Length > 0 )
					{
						lstLog.Items.Insert( iInsert + 1, sB );
						iInsert++;
					}
					
					iInsert++;
				}
				
				lstLog.Items.Insert( 0, "-----------------------" );
			});
		}
		
		private string Uri2FileName( string sUri )
		{
			string sFn = sUri;
			sFn = sFn.Replace( ':', '_' );
			sFn = sFn.Replace( '/', '_' );
			return sFn;
		}
		
		// //
		//
		
		private void UpdateSumItem(int iCount_NonAckAll, int iCountAll)
		{
			if( m_tiSum == null )
				return;
			
			m_tiSum.DetailsOnly = "NON-acked messages: " + iCount_NonAckAll.ToString() + " of " + iCountAll.ToString();
		}
		
		private void ShowSavedThreadData( string sPath, int iStartIndex, TreeLbItem tiHead = null )
		{
			RscStore store = new RscStore();
			
			sPath += "\\Threads";
			if( !store.FolderExists( sPath ) )
				return;
			
			int iCountAll = 0;
			iCountAll = store.ReadXmlDataFile( sPath + "\\" + "Count" + ".xml", iCountAll );
			
			int iCount_NonAckAll = 0;
			iCount_NonAckAll = store.ReadXmlDataFile( sPath + "\\" + "Count_NonAck" + ".xml", iCount_NonAckAll );
			
			if( iStartIndex == 0 )
			{
				m_tiSum = new TreeLbItem( m_aTI, null );
				m_tiSum.DetailsFontSize = cdFontSize;
				UpdateSumItem( iCount_NonAckAll, iCountAll );
				m_aTI.Add( m_tiSum );
				
				tiHead = new TreeLbItem( m_aTI, null );
				//tiHead.DetailsFontSize = cdFontSize;
				tiHead.DetailsOnly = "...";
				m_aTI.Add( tiHead );
			}
			
			string sThreadIdOrder = "";
			sThreadIdOrder = store.ReadTextFile( sPath + "\\" + "IdOrder" + ".txt", sThreadIdOrder );
			string [] asThreadIdOrders = sThreadIdOrder.Split( '|' );
			
			bool bMoreItems = false;
			int iThCnt = asThreadIdOrders.Length;
			for( int iTh = iStartIndex; iTh < iThCnt; iTh++ )
			{
				
				//NOT all...
				if( (iTh - iStartIndex) >= ciMaxAtOnce )
				{
					bMoreItems = true;
					break;
				}
				
				string sID = asThreadIdOrders[ iTh ];
				
				if( sID.Length == 0 )
					continue;
				
				if( !store.FolderExists( sPath + "\\" + sID ) )
					continue;
				
				string sIdOrder = "";
				sIdOrder = store.ReadTextFile( sPath + "\\" + sID + "\\" + "IdOrder" + ".txt", sIdOrder );
				string [] asIdOrders = sIdOrder.Split( '|' );
				
				string sHistoryID = asIdOrders[ 0 ];
				
				if( sHistoryID.Length == 0 )
					continue;
				
				if( !store.FileExists( sPath + "\\" + sID + "\\" + sHistoryID + ".xml" ) )
					continue;
				
				MyThread2 th = new MyThread2();
				th = store.ReadXmlDataFile( sPath + "\\" + sID + "\\" + sHistoryID + ".xml", th );
				if( th.ID.Length == 0 )
					continue;
			
				TreeLbItem ti = new TreeLbItem( m_aTI, null );
				//
				ti.DetailsFontSize = cdFontSize;
				ti.DetailsBackColor = m_AppFrame.Theme.ThemeColors.TextDarkBack;
				ti.DetailsForeColor = m_AppFrame.Theme.ThemeColors.TextDarkFore;
				if( !th.Acknowledged )
				{
					ti.CustomBackColor = Colors.Orange;
					
					ti.BtnCustom1Visibility = Rsc.Visible;
				}
				ti.BtnCustom1Image = m_isCheckOn; //Off;
				//
				string sTitle = "";
				sTitle += RscUtils.toDateDiff( th.DateSaved );
				if( asIdOrders.Length > 1 )
					sTitle += " ( +" + (asIdOrders.Length - 1).ToString() + " )";
				ti.Title = sTitle;
				ti.DetailsOfTitle = TreeLbItemThread.DecorateSnippet( th );
				ti.IsLeaf = (asIdOrders.Length <= 1);
				m_aTI.Add( ti );
					
				ti.sID = sID;
				ti.sHistoryID = sHistoryID;
				
				foreach( string sId in asIdOrders )
				{
					if( sId.Length == 0 )
						continue;
					if( sId == sHistoryID )
						continue;
				
					MyThread2 thSub = new MyThread2();
					thSub = store.ReadXmlDataFile( sPath + "\\" + sID + "\\" + sId + ".xml", thSub );
					if( thSub.ID.Length == 0 )
						continue;
			
					//DO NOT!!!
					/*
					TreeLbItem tiSub = new TreeLbItem( m_aTI, ti );
					tiSub.Title = RscUtils.toDateDiff( thSub.DateSaved ) + "retrived...";
					tiSub.DetailsOfTitle = thSub.Snippet;
					tiSub.IsLeaf = true;
					m_aTI.Add( tiSub );
					*/
					
					ti.m_a.Add( thSub );
				}
			}
			
			if( tiHead != null )
			{
				string sTit = "Threads " + (iStartIndex + 1).ToString() + " - ";
				if( bMoreItems )
					sTit += (iStartIndex + ciMaxAtOnce).ToString();
				else
					sTit += iThCnt.ToString();
				
				//tiHead.DetailsFontSize = cdFontSize;
				tiHead.DetailsOnly = sTit;
			}
			
			if( bMoreItems )
			{
				TreeLbItem ti = new TreeLbItem( m_aTI, null );
				ti.Title = "Threads " + (iStartIndex + ciMaxAtOnce + 1).ToString() + " - ... (press to list)";
				ti.sID = csMoreItems;
				ti.sHistoryID = (iStartIndex + ciMaxAtOnce).ToString();
				ti.CustomBackColor = m_AppFrame.Theme.ThemeColors.TreeDescBack;
				ti.CustomForeColor = m_AppFrame.Theme.ThemeColors.TreeDescFore;
				m_aTI.Add( ti );
			}
		}
		
		//
		// //
		
		protected void DoAck( TreeLbItem ti )
		{
			if( MessageBoxResult.OK != MessageBox.Show( "Do you really want to acknowledge message?\n\n(press Back to Cancel)" ) )
				return;
			
			if( m_sUserIDlast.Length == 0 )
				return;
			
			string sPath = AppLogic.csSecretsFolder + "\\" + m_sUserIDlast;
			
			RscStore store = new RscStore();
			
			sPath += "\\Threads";
			if( !store.FolderExists( sPath ) )
				return;
					
			MyThread2 th = new MyThread2();
			th = store.ReadXmlDataFile( sPath + "\\" + ti.sID + "\\" + ti.sHistoryID + ".xml", th );
			if( th.ID.Length == 0 )
				return; //FAIL!!!
			
			th.DateAcked = DateTime.Now;
			
			store.WriteXmlDataFile( sPath + "\\" + ti.sID + "\\" + ti.sHistoryID + ".xml", th, true );
			
			ti.BtnCustom1Visibility = Rsc.Collapsed;
			ti.DetailsOfTitle = TreeLbItemThread.DecorateSnippet( th );
			ti.ClearCustomBackColor();
			
			int iCount_NonAckAll = 0;
			iCount_NonAckAll = store.ReadXmlDataFile( sPath + "\\" + "Count_NonAck" + ".xml", iCount_NonAckAll );
			iCount_NonAckAll = Math.Max( 0, iCount_NonAckAll - 1 );
			store.WriteXmlDataFile( sPath + "\\" + "Count_NonAck" + ".xml", iCount_NonAckAll, true );			
			
			int iCountAll = 0;
			iCountAll = store.ReadXmlDataFile( sPath + "\\" + "Count" + ".xml", iCountAll );
			UpdateSumItem( iCount_NonAckAll, iCountAll );
			
			m_AppFrame.StatusText = ""; //To refresh mem info...
		}
		
		private void m_btnAddTile_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( MessageBoxResult.OK != MessageBox.Show( "Do you really want to create Shell Tile?\n\n(press Back to Cancel)" ) )
				return;
			
			RscShellTileManager.InitShellTask( csTaskName, false );
			
			RscShellTileManager.CleanUpShellTileData( );
			
			SysTiles stm = new SysTiles();
			stm.Create( "/RscGoogleApiMail;component/MainPage.xaml", "",
				"", "", SysTiles.csTileID );
			
			m_AppFrame.StatusText = ""; //To refresh mem info...
		}
 
		private void m_AppFrame_OnNext(object sender, System.EventArgs e)
		{
			if( MessageBoxResult.OK != MessageBox.Show( "Do you really want to Test TileUpdate logic?\n\n(press Back to Cancel)" ) )
				return;
			
			try
			{
				AppLogicTest alt = new AppLogicTest();
				int iNew = alt.ReadThreadData( );
				
				if( iNew < 0 )
				{
					MessageBox.Show( "new: " + "N/A" + " (" + iNew.ToString() + ")" );
				}
			}
			catch( Exception ex )
			{
				MessageBox.Show( ex.Message + "\n\n" + ex.StackTrace );
			}
		}
		
    }
	
	class AppLogicTest : AppLogic
	{
		protected override void OnDone( int iNew )
		{
			if( iNew < 0 )
			{
				MessageBox.Show( "new: " + "N/A" + " (" + iNew.ToString() + ")" );
			}
			else
			{
				MessageBox.Show( "new: " + iNew.ToString() );
			}
		}
	}
}
