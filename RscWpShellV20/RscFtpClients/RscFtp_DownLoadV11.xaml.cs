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
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;

using Ressive.Utils;
using Ressive.Store;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

using Ressive.FTP;

namespace RscFtpClients
{
	
    public partial class RscFtp_DownLoadV11 : PhoneApplicationPage
    {
		
		const string csDlgsAssy = "Lib_RscIPgC_Dlgs";
		
		RscAppFrame m_AppFrame;
		// //////////////////
		ImageSource m_isAdd = null;
		ImageSource m_isRemove = null;
		ImageSource m_isSkipPrev = null;
		ImageSource m_isSkipNext = null;
		ImageSource m_isDummy = null;
	
		RscPageArgsRetManager m_AppArgs;
		
		const int ciLogCChMax = 64;
		const int ciLogMaxLnCnt = 256;
		
		RscFtpClient m_ftpc = null;
		
		bool m_bRestoreBackUp = true;
		bool m_bRestoreBackUpUsrChoiche = true;
		
		RscSimpleLbItemList m_aTodo = null;
		int m_iActiveToDo = -1;
		
		string m_strSessionFolderRoot = "";
		string m_strSessionFolderRootMIN = "";
		string m_strSessionFolderRootDefault = "";
		
		string m_strServerRoot = "";
		string m_strSessionFolder = "";
		
		int m_iPhase = 0;
		
		int m_iStatAllFiles = 0;
		int m_iStatAllFolders = 0;
		int m_iStatDoneFolder = 0;
		int m_iStatSkipFile = 0;
		int m_iStatDoneFile = 0;
		int m_iStatFailFile = 0;
		
		bool m_bChkLocal = false;
		
		bool m_bLogItems = false;
		
		bool m_bUserStop = false;
		
		int m_TEMP_iTodoIdxToList = 0;
		
		DispatcherTimer m_tmrDownToFolder;
		
		MyLogItemList m_logs = new MyLogItemList();
		
		string sSAVE_Status = "";
		StatusColoring scSAVE = StatusColoring.Normal;
		
		DateTime m_dtStart = DateTime.Now;
		bool m_bStarted = false;
		DateTime m_dtEnd = DateTime.Now;
		bool m_bEnded = false;
		
		DispatcherTimer m_tmrInput;
		
		Size m_sContentPanel = new Size(100, 100);
		
		private void _UpdateRes(bool bInit = false)
		{
			if( bInit )
			{
				m_iStatDoneFolder = 0;
				m_iStatSkipFile = 0;
				m_iStatDoneFile = 0;
			}
			
			string sDet;
			
			int iRem = (m_iStatAllFiles + m_iStatAllFolders) - (m_iStatDoneFolder + m_iStatSkipFile + m_iStatDoneFile);
			int iDone = m_iStatDoneFolder + m_iStatSkipFile + m_iStatDoneFile;
			
			string sDone = "";
			if( iRem > 0 ) sDone += iRem.ToString() + " REM ";
			if( iDone > 0 )
			{
				if( iRem > 0 ) sDone += "| ";
				
				sDone += iDone.ToString() + " DONE ";
				sDone += "(";
				sDet = "";
				if( m_iStatDoneFolder > 0 )
				{
					if( sDet.Length > 0 ) sDet += " ";
					sDet += m_iStatDoneFolder.ToString() + " folders";
					if( m_iStatDoneFile > 0 || m_iStatSkipFile > 0 ) sDet += " |";
				}
				if( m_iStatDoneFile > 0 )
				{
					if( sDet.Length > 0 ) sDet += " ";
					sDet += m_iStatDoneFile.ToString() + " download";
				}
				if( m_iStatSkipFile > 0 )
				{
					if( sDet.Length > 0 ) sDet += " ";
					sDet += m_iStatSkipFile.ToString() + " skip";
				}
				sDone += sDet;
				sDone += ") ";
			}
			if( m_iStatFailFile > 0 ) sDone += "| " + m_iStatFailFile.ToString() + " NOK ";
			//if( m_iPhaseCYC > 1 ) sDone += "| " + m_iPhaseCYC.ToString() + " cycle";
			if( m_iStatFailFile > 0 )
			{
				todoResDonePanel.Background = new SolidColorBrush(Colors.Red);
				todoResDone.Foreground = new SolidColorBrush(Colors.White);
			}
			else
			{
				todoResDonePanel.Background = todoResPanel.Background;
				todoResDone.Foreground = todoRes.Foreground;
			}
			
			string sToDo = (m_iStatAllFiles + m_iStatAllFolders).ToString() + " TODO ";
			sToDo += "(";
			sDet = "";
			if( m_iStatAllFolders > 0 )
			{
				if( sDet.Length > 0 ) sDet += " ";
				sDet += m_iStatAllFolders.ToString() + " folders";
				if(m_iStatAllFiles > 0 ) sDet += " |";
			}
			if( m_iStatAllFiles > 0 )
			{
				if( sDet.Length > 0 ) sDet += " ";
				sDet += m_iStatAllFiles.ToString() + " files";
			}
			sToDo += sDet;
			sToDo += ") ";
			
			// ////
			
			if( iRem > 0 && iRem == (m_iStatAllFiles + m_iStatAllFolders) )
			{
				if( (m_iPhase == 0 || m_iPhase == 1) )
				{
					sDone = sToDo;
					sToDo = " ";
				}
			}
			
			todoRes.Text = sToDo;		
			if( (m_iStatAllFiles + m_iStatAllFolders) == 0 )
				todoRes.Text = " ";
			
			todoResDone.Text = sDone;
			if( sDone.Length > 0 )
				todoResDonePanel.Visibility = Rsc.Visible;
			else
				todoResDonePanel.Visibility = Rsc.Collapsed;
			
		}
		
        public RscFtp_DownLoadV11()
        {
            InitializeComponent();
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "FTP Download 1.1", "Images/IcoSm001_FtpDownLoad.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			// ///////////////
			m_isAdd = m_AppFrame.Theme.GetImage("Images/Btn001_Add.jpg");
			m_isRemove = m_AppFrame.Theme.GetImage("Images/Btn001_Remove.jpg");
			m_isSkipPrev = m_AppFrame.Theme.GetImage("Images/Btn001_SkipPrev.jpg");
			m_isSkipNext = m_AppFrame.Theme.GetImage("Images/Btn001_SkipNext.jpg");
			m_isDummy = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			// ///////////////
			imgInput.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Open.jpg");
			imgIpUpIco.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Inc.jpg");
			imgIpDnIco.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Dec.jpg");
			
			m_AppArgs = new RscPageArgsRetManager();
			
			m_strSessionFolderRootDefault = RscUtils.GetDeviceName() + " (BackUp)";
			m_bRestoreBackUp = true;
			
			txSvrIP.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11", "LastSvrIP", "192.168.0.0" );
			LoadFromReg();
			
			m_ftpc = new RscFtpClient();
			m_ftpc.CommandSocketConnectedAsync += new Ressive.FTP.RscFtpClient.CommandSocketConnectedAsync_EventHandler(m_ftpc_CommandSocketConnectedAsync);
			m_ftpc.ServerResponseAsync += new Ressive.FTP.RscFtpClient.ServerResponseAsync_EventHandler(m_ftpc_ServerResponseAsync);
			m_ftpc.CommandSentAsync += new Ressive.FTP.RscFtpClient.CommandSentAsync_EventHandler(m_ftpc_CommandSentAsync);
			m_ftpc.DataSocketConnectedAsync += new Ressive.FTP.RscFtpClient.DataSocketConnectedAsync_EventHandler(m_ftpc_DataSocketConnectedAsync);
			m_ftpc.ServerDataReceivedAsync += new Ressive.FTP.RscFtpClient.ServerDataReceivedAsync_EventHandler(m_ftpc_ServerDataReceivedAsync);
			m_ftpc.ServerDataSentAsync += new Ressive.FTP.RscFtpClient.ServerDataSentAsync_EventHandler(m_ftpc_ServerDataSentAsync);
			m_ftpc.DataSocketClosingAsync += new Ressive.FTP.RscFtpClient.DataSocketClosingAsync_EventHandler(m_ftpc_DataSocketClosingAsync);
			m_ftpc.CommandDoneAsync += new Ressive.FTP.RscFtpClient.CommandDoneAsync_EventHandler(m_ftpc_CommandDoneAsync);
			m_ftpc.LogAsync += new Ressive.FTP.RscFtpClient.LogAsync_EventHandler(m_ftpc_LogAsync);
			m_ftpc.ProgressAsync += new Ressive.FTP.RscFtpClient.ProgressAsync_EventHandler(m_ftpc_ProgressAsync);
			
			m_tmrDownToFolder = new DispatcherTimer();
			m_tmrDownToFolder.Interval = new TimeSpan(500);
			m_tmrDownToFolder.Tick += new System.EventHandler(m_tmrDownToFolder_Tick);
			
			m_tmrInput = new DispatcherTimer();
			m_tmrInput.Interval = new TimeSpan(500);
			m_tmrInput.Tick += new System.EventHandler(m_tmrInput_Tick);
			
			m_logs.ListBoxAsteriskWidth = 100;
			lbLogs.ItemsSource = m_logs;
			lbLogs.SizeChanged += new System.Windows.SizeChangedEventHandler(lbLogs_SizeChanged);
			
			m_aTodo = new RscSimpleLbItemList( lbItems, m_AppFrame.Theme );
			
			this.Unloaded += new System.Windows.RoutedEventHandler(RscFtpDownLoadV10_Unloaded);
			this.Loaded +=new System.Windows.RoutedEventHandler(RscFtpDownLoadV10_Loaded);
			
			bool bChk = RscRegistry.ReadBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11", "ChkLocal", false );
			if( bChk )
			{
				chbChkLocal.IsChecked = true;
				grdAtt.Visibility = Rsc.Visible;
			}
			else
			{
				chbChkLocal.IsChecked = false;
				grdAtt.Visibility = Rsc.Collapsed;
			}
			
			ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);
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
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			m_AppFrame.SetNoSleep( true );
			
			if( m_AppArgs.Waiting )
			{
				RscPageArgsRet appOutput = m_AppArgs.GetOutput();
				if( appOutput != null )
				{
					switch( appOutput.ID )
					{
						
						case "FtpHost" :
							txSvrIP.Text = appOutput.GetData(0);
							LoadFromReg();
							break;
						
						case "txDownToFolder" :
							if( appOutput.GetFlag(0) == "Ok" )
							{
								txDownToFolder.Text = appOutput.GetData(0);
							}
							else
							{
								txDownToFolder.Text = "NA";
							}
							break;
							
					}
				}
				
				m_AppArgs.Clear();
			}
		}

		private void RscFtpDownLoadV10_Unloaded(object sender, System.Windows.RoutedEventArgs e)
		{
			m_AppFrame.SetNoSleep( false );
		}

		private void RscFtpDownLoadV10_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
		}		
		
		private void lbLogs_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			int iListBoxAsteriskWidth = (int) (e.NewSize.Width - (50 + 10));
			//ATT!!! - Otherwise slowdown...
			if( m_logs.ListBoxAsteriskWidth != iListBoxAsteriskWidth )
			{
				m_logs.ListBoxAsteriskWidth = iListBoxAsteriskWidth;
				
				if( m_logs.Count > 0 )
				{
					//ReQuery...
					lbLogs.ItemsSource = null;
					lbLogs.ItemsSource = m_logs;
				}
			}
		}

		private void rbRestBuFull_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			txDownToFolder.Text = " ";
		}

		private void rbRestBuPart_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			txDownToFolder.Text = " ";
		}

		private void rbDown_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			//FIX: Without timer - focus problem...
			m_tmrDownToFolder.Start();
		}

		private void m_tmrDownToFolder_Tick(object sender, System.EventArgs e)
		{
			m_tmrDownToFolder.Stop();

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "txDownToFolder" );
			appInput.SetFlag( 0, "download to (subfolder of FTP)" );
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, txDownToFolder.Text.ToString().Trim() );
			appInput.SetInput( "RscDlg_TxtInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csDlgsAssy ) );
		}
		
		private void btnInput_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_tmrInput.Start();
		}

		private void m_tmrInput_Tick(object sender, System.EventArgs e)
		{
			m_tmrInput.Stop();

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				/*m_AppFrame.AppTitle*/ "FTP access", m_AppFrame.AppIconRes, "FtpHost" );
			appInput.SetFlag( 0, "FTP hostname" );
			appInput.SetFlag( 1, "NoEmpty" );
			//appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, txSvrIP.Text );
			appInput.SetInput( "RscDlg_FtpHostInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csDlgsAssy ) );
		}
		
		private void btnIpUp_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string strIp = txSvrIP.Text;
			if( strIp.Length == 0 ) return;
			string strPre = "";
			string strNum = "";
			int iPos = strIp.LastIndexOf('.');
			if( iPos < 0 )
			{
				strNum = strIp;
			}
			else
			{
				strPre = strIp.Substring(0, iPos + 1);
				strNum = strIp.Substring(iPos + 1);
			}
			int iNum = 0;
			if( !Int32.TryParse( strNum, out iNum ) ) return;
			if( iNum > 254 ) return;
			iNum++;
			txSvrIP.Text = strPre + iNum.ToString();
		}
		
		private void btnIpDn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string strIp = txSvrIP.Text;
			if( strIp.Length == 0 ) return;
			string strPre = "";
			string strNum = "";
			int iPos = strIp.LastIndexOf('.');
			if( iPos < 0 )
			{
				strNum = strIp;
			}
			else
			{
				strPre = strIp.Substring(0, iPos + 1);
				strNum = strIp.Substring(iPos + 1);
			}
			int iNum = 0;
			if( !Int32.TryParse( strNum, out iNum ) ) return;
			if( iNum <= 0 ) return;
			iNum--;
			txSvrIP.Text = strPre + iNum.ToString();
		}
		
		private void chbChkLocal_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( chbChkLocal.IsChecked.Value )
				grdAtt.Visibility = Rsc.Visible;
			else
				grdAtt.Visibility = Rsc.Collapsed;
		}
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			if( parPanel.Visibility == Rsc.Visible )
			{
				if( !DeviceNetworkInformation.IsNetworkAvailable )
				{
					if( !DeviceNetworkInformation.IsCellularDataEnabled )
					{
						ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
						connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.WiFi;
						connectionSettingsTask.Show();
						
						return;
					}
					else
					{
						MessageBox.Show( "ATT: For security reasons and high amount of possible data FTP connection through the Internet and Public WiFi networks is not recommended!" );
					}
				}
			
				if( !rbRestBuFull.IsChecked.Value && !rbRestBuPart.IsChecked.Value && !rbDown.IsChecked.Value )
				{
					MessageBox.Show("Select download option!");
					return;
				}
				
				if( rbRestBuFull.IsChecked.Value || rbRestBuPart.IsChecked.Value )
				{
					if( MessageBoxResult.OK != MessageBox.Show( "Local Drives other than A: are all Read-Only, you are unable to restore backup to them!"
						+ "\n\nContent of remote folder A_\\*.* will be restored to local drive A:\\*.*,"
						+ "\n\nwhile content of remote folder B_\\*.* - Z_\\*.* will be restored to local folder A:\\B_\\*.* - A:\\Z_\\*.*!"
						+ "\n\nDo you want to continue?"
						+ "\n\n(Press Back to cancel...)" ) )
						return;
				}
				
				parPanel.Visibility = Rsc.Collapsed;
				
				lbLogs.Visibility = Rsc.Visible;
				m_AppFrame.ShowButtonNext(false);
				
				m_iPhase = 0;
				
				m_bChkLocal = false;
				if( chbChkLocal.IsChecked.Value )
				{
					m_bChkLocal = true;
				}
				
				m_bLogItems = false;
				if( chbItemLog.IsChecked.Value )
				{
					m_bLogItems = true;
				}
				
				m_dtStart = DateTime.Now;
				m_bStarted = false;
				m_dtEnd = m_dtStart;
				m_bEnded = false;
				
				m_strSessionFolderRoot = m_strSessionFolderRootDefault;
				m_strSessionFolderRootMIN = "";
				m_bRestoreBackUp = true;
				m_bRestoreBackUpUsrChoiche = true;
				if( !rbRestBuFull.IsChecked.Value )
				{
					m_bRestoreBackUp = false;
					m_bRestoreBackUpUsrChoiche = false;
					
					if( rbRestBuPart.IsChecked.Value )
					{
						m_strSessionFolderRootMIN = m_strSessionFolderRoot;
					}
					else
					{
						m_strSessionFolderRoot = "";
					}				
				}
				else
				{
					m_bStarted = true;
					m_dtStart = DateTime.Now;
				}
				
				//WP81 FIX
				//m_ftpc.SetFastConnection( chbFastConn.IsChecked.Value, chbFastConnEx.IsChecked.Value );
				
				m_bUserStop = false;
			
				sSAVE_Status = "";
				scSAVE = StatusColoring.Normal;
				
				_Log("21", "Connecting...");
				
				SaveToReg();
			
				//unkLISTresLEN sign reset...
				m_AppFrame.TRACE = "";
				
				m_ftpc.CloseAllSockets();
				
				m_ftpc.IPAddress = txSvrIP.Text.ToString();
				m_ftpc.Port = Int32.Parse(txSvrPort.Text.ToString());
				m_ftpc.UserName = txUsr.Text.ToString();
				m_ftpc.PassWord = txPwd.Text.ToString();
				m_ftpc.AutoLogOn = true;
				m_ftpc.AutoPassiveMode = true;
				
				prsBar.IsIndeterminate = true;
				prsBar.Visibility = Rsc.Visible;
				
				btnStartStop.Visibility = Rsc.Collapsed;
				btnStartStop.Content = "Start";
				btnStartStop.Background = new SolidColorBrush(Colors.Green);
				
				m_ftpc.Connect();
			}
			else if( todoPanel.Visibility == Rsc.Visible )
			{
				todoPanel.Visibility = Rsc.Collapsed;
				
				lbLogs.Visibility = Rsc.Visible;
				m_AppFrame.ShowButtonNext(false);
			}
		}
		
		private void LoadFromReg( )
		{
			if( txSvrIP.Text.Length == 0 ) return;
			
			txSvrPort.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11" + "\\" + txSvrIP.Text,
				"Port", 2221.ToString( ) );
			
			txUsr.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11" + "\\" + txSvrIP.Text,
				"Usr", "usr" );
			
			//Do not store...
			txPwd.Text = "";
			
			//WP81 FIX
			/*
			chbFastConn.IsChecked = RscRegistry.ReadBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11" + "\\" + txSvrIP.Text,
				"FastConn", false );
			
			chbFastConnEx.IsChecked = RscRegistry.ReadBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11" + "\\" + txSvrIP.Text,
				"FastConnEx", false );
			*/
		}
		
		private void SaveToReg( )
		{
			RscRegistry.WriteBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11", "ChkLocal", chbChkLocal.IsChecked.Value );			
			
			RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11", "LastSvrIP", txSvrIP.Text );			
			
			RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11" + "\\" + txSvrIP.Text,
				"Port", txSvrPort.Text );
			
			RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11" + "\\" + txSvrIP.Text,
				"Usr", txUsr.Text );
			
			//Do not store...
			//txPwd.Text = "";
			
			//WP81 FIX
			/*
			RscRegistry.WriteBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11" + "\\" + txSvrIP.Text,
				"FastConn", chbFastConn.IsChecked.Value );
			
			RscRegistry.WriteBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_DownLoadV11" + "\\" + txSvrIP.Text,
				"FastConnEx", chbFastConnEx.IsChecked.Value );
			*/
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( m_AppFrame.CancelTimer( ) )
			{
				e.Cancel = true;
				return;
			}
			
			if( todoPanel.Visibility == Rsc.Visible )
			{
				bool bUserDoesNotStarted = (m_strSessionFolder.Length == 0) ||
					(m_iPhase == 0) || (m_iPhase == 1) || (m_iPhase == 1);
				
				_Log("21", "Disconnect...");
				
				m_ftpc.CloseAllSockets();
				
				todoPanel.Visibility = Rsc.Collapsed;
				
				prsBar.Visibility = Rsc.Collapsed;
				prsBarData.Visibility = Rsc.Collapsed;
				
				btnStartStop.Visibility = Rsc.Collapsed;
				btnStartStop.Content = "Start";
				btnStartStop.Background = new SolidColorBrush(Colors.Green);
				
				btnAddRemAll.Visibility = Rsc.Collapsed;

				m_logs.Clear();
				
				m_iStatAllFolders = 0;
				m_iStatAllFiles = 0;
				
				m_aTodo.Clear();
				m_iActiveToDo = -1;
				
				m_strServerRoot = "";
				m_strSessionFolder = "";
				
				m_bChkLocal = false;

				m_bLogItems = false;
				
				m_iPhase = 0;
				
				m_bUserStop = false;
				
				m_bRestoreBackUp = true;
				
				if( bUserDoesNotStarted )
				{
					_SetStatusText();
					
					parPanel.Visibility = Rsc.Visible;
					e.Cancel = true;
				}
			}
			else if( lbLogs.Visibility == Rsc.Visible )
			{
				lbLogs.Visibility = Rsc.Collapsed;
				m_AppFrame.ShowButtonNext(true);
				
				todoPanel.Visibility = Rsc.Visible;
				
				e.Cancel = true;
			}
        }
		
		//
		// //
		//
		
		private bool RaiseNextFtpClientCommand(out bool bNoop, bool bNoNoop = false)
		{
			bNoop = false;
			
			if( m_aTodo.Count == 0 ) return false;
			if( m_iActiveToDo >= m_aTodo.Count ) return false;
			
			RscFtp_DownLoad_SimpleLbItem it = null;
			
			if( m_iActiveToDo > -1 )
			{
				it = m_aTodo[m_iActiveToDo] as RscFtp_DownLoad_SimpleLbItem;
				
				//it.tbTit.Text = it.GetStateTitle(m_bLogItems);
				//SLOW!!!
				/*
				lbItems.ItemsSource = null;
				lbItems.ItemsSource = m_aTodo;
				*/
				
				if( it.bFolder && (m_iPhase == 0) )
				{
					if( !it.Acked )
					{
						m_iStatDoneFolder++;
						_UpdateRes();
						
						it.Acked = true;
					}
				}
				
				if( it.bFile )
				{
					if( !it.Done )
					{
						//TODO...
					}
					else
					{
						if( !it.Acked )
						{
							if( it.Skipped )
								m_iStatSkipFile++;
							else
								m_iStatDoneFile++;
							
							_UpdateRes();
							
							it.Acked = true;
						}
						
						//Do not...
						/*
						spFiles.Children.RemoveAt(m_iActiveToDo);
						m_todo.RemoveAt(m_iActiveToDo);
						m_iActiveToDo--;
						*/
					}
				}
			}
			
			if( m_iPhase == 0 && m_iActiveToDo == 0 && (!m_bRestoreBackUp) )
			{
				return false;
			}
			
			m_iActiveToDo++;
			if( m_iActiveToDo >= m_aTodo.Count ) return false;
			it = m_aTodo[m_iActiveToDo] as RscFtp_DownLoad_SimpleLbItem;
			
			if( it.bFolder )
			{
				if( it.Title == it.strNameEmpty )
				{
					//if( (m_iPhase == 1 && it.Done) || (m_iPhase == 0 && it.Created) )
					if( m_iPhase > 0 )
					{
						
						if( m_bLogItems )
						{
							it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") NOOP...";
							
							//SLOW!!!
							/*
							lbItems.ItemsSource = null;
							lbItems.ItemsSource = m_aTodo;
							*/
						}
						
						if( bNoNoop )
						{
							bNoop = true;
						}
						else
						{
							RscFtpClientCommand cmd = RscFtpClientCommand.NOOP();
						
							_Log("21", cmd.ToString());
						
							m_ftpc.SendCommandToServer(cmd);
						}
					}
					else
					{
						
						//Get FTP server's user's current server work dir once per session...
						if( m_strServerRoot.Length == 0 ) m_strServerRoot = m_ftpc.WorkingDirectory;
						
						if( m_strSessionFolderRoot.Length == 0 )
							m_strSessionFolder = m_ftpc.BackSlashInPath;
						else
							m_strSessionFolder = m_strSessionFolderRoot;						
						
						string sRemoteFullPath = m_strServerRoot + m_strSessionFolder;					
						// "//" or "/"...
						sRemoteFullPath = RscUtils.RemoveEnding(sRemoteFullPath, m_ftpc.BackSlashInPath);
						sRemoteFullPath = RscUtils.RemoveEnding(sRemoteFullPath, m_ftpc.BackSlashInPath);
						
						RscFtpClientCommand cmdRoot = new RscFtpClientCommand(1, "CWD", sRemoteFullPath + m_ftpc.BackSlashInPath );
						
						RscFtpClientCommand cmd = null;
						
						if( m_iPhase == 0 )
						{
							//m_AppFrame.TRACE = "'" + sRemoteFullPath + "'";
							if( sRemoteFullPath.Length > 0 )
								cmd = RscFtpClientCommand.CreateFolder(sRemoteFullPath);
						}
				
						RscFtpClientCommand cmdList = RscFtpClientCommand.ListFilesAndFolders( "", m_iActiveToDo);
						RscFtpClientCommand cmdPasv = RscFtpClientCommand.EnterPassiveMode();
						cmdPasv.Parent = cmdList;
						
						if( m_iPhase == 0 && cmd != null )
						{
							if( m_bLogItems )
							{
								it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") Creating folder...";
							
								//SLOW!!!
								/*
								lbItems.ItemsSource = null;
								lbItems.ItemsSource = m_aTodo;
								*/
							}
							
							_Log("21", cmd.ToString() + " + " + "(auto-PASV) " + cmdList.ToString());
						
							cmdRoot.Parent = cmdPasv;
							
							cmd.Parent = cmdRoot;
							
							it.Created = true;
							it.Done = (it.RefCount == 0);
					
							m_ftpc.SendCommandToServer(cmd);
						}
						else
						{
							if( m_bLogItems )
							{
								it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") Listing folder...";
							
								//SLOW!!!
								/*
								lbItems.ItemsSource = null;
								lbItems.ItemsSource = m_aTodo;
								*/
							}
							
							_Log("21", "(auto-PASV) " + cmdList.ToString());
						
							cmdRoot.Parent = cmdPasv;
					
							m_ftpc.SendCommandToServer(cmdRoot);
						}
					}
				}
				else
				{
					if( m_strSessionFolder.Length == 0 ) return false;
					
					//if( (m_iPhase == 1 && it.Done) || (m_iPhase == 0 && it.Created) )
					if( m_iPhase > 0 )
					{
						if( m_bLogItems )
						{
							it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") NOOP...";
							
							//SLOW!!!
							/*
							lbItems.ItemsSource = null;
							lbItems.ItemsSource = m_aTodo;
							*/
						}
						
						if( bNoNoop )
						{
							bNoop = true;
						}
						else
						{
							RscFtpClientCommand cmd = RscFtpClientCommand.NOOP();
							
							_Log("21", cmd.ToString());
							
							m_ftpc.SendCommandToServer(cmd);
						}
					}
					else
					{						
						string strWrkPath = it.GetPath();
						strWrkPath = strWrkPath.Replace("\\", m_ftpc.BackSlashInPath);
						string sRemoteFullPath = m_strServerRoot + m_strSessionFolder;
						sRemoteFullPath = RscUtils.RemoveEnding(sRemoteFullPath, m_ftpc.BackSlashInPath);
						sRemoteFullPath = RscUtils.RemoveEnding(sRemoteFullPath, m_ftpc.BackSlashInPath);
						sRemoteFullPath += m_ftpc.BackSlashInPath + strWrkPath;
						
						RscFtpClientCommand cmdRoot = new RscFtpClientCommand(1, "CWD", sRemoteFullPath + m_ftpc.BackSlashInPath );
						
						RscFtpClientCommand cmd = null;
						
						if( m_iPhase == 0 ) cmd = RscFtpClientCommand.CreateFolder(sRemoteFullPath);
				
						RscFtpClientCommand cmdList = RscFtpClientCommand.ListFilesAndFolders("", m_iActiveToDo);
						RscFtpClientCommand cmdPasv = RscFtpClientCommand.EnterPassiveMode();
						cmdPasv.Parent = cmdList;
						
						if( m_iPhase == 0 )
						{
							if( m_bLogItems )
							{
								it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") Creating folder...";
							
								//SLOW!!!
								/*
								lbItems.ItemsSource = null;
								lbItems.ItemsSource = m_aTodo;
								*/
							}
							
							_Log("21", cmd.ToString() + " + " + "(auto-PASV) " + cmdList.ToString());
					
							cmdRoot.Parent = cmdPasv;
						
							cmd.Parent = cmdRoot;
							
							it.Created = true;
							it.Done = (it.RefCount == 0);
							
							m_ftpc.SendCommandToServer(cmd);
						}
						else
						{
							if( m_bLogItems )
							{
								it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") Listing folder...";
							
								//SLOW!!!
								/*
								lbItems.ItemsSource = null;
								lbItems.ItemsSource = m_aTodo;
								*/
							}
							
							_Log("21", "(auto-PASV) " + cmdList.ToString());
						
							cmdRoot.Parent = cmdPasv;
							
							m_ftpc.SendCommandToServer(cmdRoot);
						}
					}
				}
			}
			else if( it.bFile )
			{
				if( m_strSessionFolder.Length == 0 ) return false;
				
				if( m_iPhase != 2 || (!it.Include) )
				{
					if( !it.Include ) it.Done = true;
					
					if( m_bLogItems )
					{
						it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") NOOP...";
							
						//SLOW!!!
						/*
						lbItems.ItemsSource = null;
						lbItems.ItemsSource = m_aTodo;
						*/
					}
					
					if( bNoNoop )
					{
						bNoop = true;
					}
					else
					{
						RscFtpClientCommand cmd = RscFtpClientCommand.NOOP();
						
						_Log("21", cmd.ToString());
						
						m_ftpc.SendCommandToServer(cmd);
					}
				}
				else
				{				
					string strWrkPath = it.GetPath();
					strWrkPath = strWrkPath.Replace("\\", m_ftpc.BackSlashInPath);
					
					string strRemoteFullPath = m_strServerRoot + m_strSessionFolder;
					strRemoteFullPath = RscUtils.RemoveEnding(strRemoteFullPath, m_ftpc.BackSlashInPath);
					strRemoteFullPath = RscUtils.RemoveEnding(strRemoteFullPath, m_ftpc.BackSlashInPath);
					strRemoteFullPath += m_ftpc.BackSlashInPath + strWrkPath;
					
					string strLocalFullPath = "";
					if( m_strSessionFolder == m_strSessionFolderRootDefault )
					{
						strLocalFullPath = it.GetPath();
					}
					else
					{
						
						string strDir;
						
						//_Log("21", "DEBUG: " + m_strSessionFolder);
						//_Log("21", "DEBUG: " + m_strSessionFolderRootDefault);
						
						if( RscUtils.FindStart( m_strSessionFolder, m_strSessionFolderRootDefault ) )
						{
							strLocalFullPath = "";
							
							strDir = m_strSessionFolder.Substring(m_strSessionFolderRootDefault.Length + 1);
						}
						else
						{
							strLocalFullPath = "FTP\\";
							
							string strSubFolder = txDownToFolder.Text.ToString().Trim();
							if( strSubFolder.Length == 0 )
							{
								string strSvrFldr = m_ftpc.ServerTitle;
								if( strSvrFldr.Length == 0 )
									strSvrFldr = "Unknown Server Title";
								int iPos = strSvrFldr.IndexOf(' ');
								if( iPos >= 0 )
									strSvrFldr = strSvrFldr.Substring(0, iPos);
								if( strSvrFldr.Length == 0 )
									strSvrFldr = "NA";
								
								strSubFolder = strSvrFldr;
							}
							strLocalFullPath += strSubFolder + "\\";
							
							strDir = m_strSessionFolder;
						}
						
						strDir = RscUtils.RemoveStarting(strDir, m_ftpc.BackSlashInPath);
						strDir = RscUtils.RemoveEnding(strDir, m_ftpc.BackSlashInPath);
						strDir = RscUtils.RemoveEnding(strDir, m_ftpc.BackSlashInPath);
						strDir = strDir.Replace(m_ftpc.BackSlashInPath, "\\");						
						
						strLocalFullPath += strDir + "\\";
						strLocalFullPath += it.GetPath();
					}
					
					//RscStore...
					if( strLocalFullPath.Substring(0, 3).ToUpper() == "A_\\" )
						strLocalFullPath = "A:\\" + strLocalFullPath.Substring(3);
					else
						strLocalFullPath = "A:\\" + strLocalFullPath;
					
					RscStore store = new RscStore();
					if( m_bChkLocal )
					{
						//m_AppFrame.TRACE = strLocalFullPath + "\r\nremote: " + it.RemoteFileSize.ToString();
						
						if( store.FileExists( strLocalFullPath ) )
						{
							System.IO.Stream streamChk = store.GetReaderStream( strLocalFullPath );
							
							//m_AppFrame.TRACE = strLocalFullPath + "\r\nremote: " + it.RemoteFileSize.ToString() +
							//	"\r\nlocal: " + streamChk.Length.ToString();
							
							if( it.RemoteFileSize == streamChk.Length )
							{
								
								it.Skipped = true;
								it.Done = true;
								
								bNoop = true;
						
								if( m_bLogItems )
								{
									it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") Local file exists with same size...";
										
									//SLOW!!!
									/*
									lbItems.ItemsSource = null;
									lbItems.ItemsSource = m_aTodo;
									*/
								}
							}
							streamChk.Close();
						}
					}
							
					if( !it.Done )
					{						
						{
							string sDirPath = "";
							int iPos = strLocalFullPath.LastIndexOf('\\');
							if( iPos >= 0 ) sDirPath = strLocalFullPath.Substring(0, iPos);
							if( sDirPath.Length > 0 )
							{
								store.CreateFolderPath( sDirPath );
							}
						}	
						if( store.FileExists( strLocalFullPath ) ) store.DeleteFile( strLocalFullPath );
						System.IO.Stream stream = store.CreateFile( strLocalFullPath );
						
						RscFtpClientCommand cmd = RscFtpClientCommand.DownloadBin( strRemoteFullPath,
							it.RemoteFileSize, "" /*strLocalFullPath*/, m_iActiveToDo, stream);
						
						RscFtpClientCommand cmdPasv = RscFtpClientCommand.EnterPassiveMode();
						
						cmdPasv.Parent = cmd;
						
						it.Done = true;
						
						if( m_bLogItems )
						{
							it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") Downloading...";
								
							//SLOW!!!
							/*
							lbItems.ItemsSource = null;
							lbItems.ItemsSource = m_aTodo;
							*/
						}
						
						_Log("21", "(auto-PASV) " + cmd.ToString());
				
						m_ftpc.SendCommandToServer(cmdPasv);
					}
				}
			}
			
			return true;
		}
		
		private void btnAddRemAll_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			foreach(RscFtp_DownLoad_SimpleLbItem it in m_aTodo)
			{
				if( !it.Include )
				{
					/*
					it.imgRemove.Source = m_isRemove;
					it.rc.Fill = new SolidColorBrush(it.GetColor());
					it.Include = true;
					
					if( it.bFolder )
					{
						m_iStatAllFolders++;
					}
					else if( it.bFile )
					{
						m_iStatAllFiles++;
					}
					*/
				}
				else
				{
					it.BtnCust2Img = m_isAdd;
					it.Include = false;
					
					if( it.bFolder )
					{
						m_iStatAllFolders--;
					}
					else if( it.bFile )
					{
						m_iStatAllFiles--;
					}
				}
				
				_UpdateRes(true);
			}		
			
			//SLOW!!!
			/*
			lbItems.ItemsSource = null;
			lbItems.ItemsSource = m_aTodo;
			*/
			
			if( m_iStatAllFolders + m_iStatAllFiles == 0 )
				btnStartStop.IsEnabled = false;
			else
				btnStartStop.IsEnabled = true;
		}
		
		private void _ListFolders(bool bItemClick)
		{
			bool bWalking = false;
			if( m_aTodo.Count > 0 )
			{
				bWalking = true;
				
				m_iStatAllFolders = 0;
				m_iStatAllFiles = 0;
				
				m_aTodo.Clear();
				m_iActiveToDo = -1;
			}
			
			RscFtp_DownLoad_SimpleLbItem it = new RscFtp_DownLoad_SimpleLbItem( m_aTodo );
			it.bFile = false;
			it.bFolder = true;
			it.bWalked = false;
			it.strName = "";
			if( bWalking )
			{
				/*
				string strTmp = m_strSessionFolderRoot.Substring(0, m_strSessionFolderRoot.Length - 1);
				it.strOwner = strTmp.Replace(m_ftpc.BackSlashInPath, "\\");
				*/
				//BUGFIX...
				it.strOwner = "";
				
				it.strNameEmpty = "..";
			}
			else
			{
				it.strOwner = "";
				it.strNameEmpty = ".";
			}
			AddTaskEx(it);
			
			prsBar.Visibility = Rsc.Visible;
			
			btnStartStop.Content = "Cancel";
			btnStartStop.Background = new SolidColorBrush(Colors.Red);
			
			m_iActiveToDo = -1;
			
			if( (bWalking) && (!bItemClick) )
			{
				it.strNameEmpty = ".";
				m_bRestoreBackUp = true;
			}
			
			bool bTmp = false;
			RaiseNextFtpClientCommand(out bTmp);
		}
		
		private void btnStartStop_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_iPhase == 0 ) //m_strSessionFolder.Length == 0 )
			{
				_ListFolders(false);
			}
			else if( m_iPhase == 1 )
			{
				
				prsBar.Visibility = Rsc.Visible;
				btnStartStop.Visibility = Rsc.Collapsed;
							
				btnAddRemAll.Visibility = Rsc.Collapsed;
			
				m_iStatAllFolders = 0;
				m_iStatAllFiles = 0;
				
				m_TEMP_iTodoIdxToList = 0;
				if( m_aTodo.Count > 0 ) (m_aTodo[ 0 ] as RscFtp_DownLoad_SimpleLbItem).m_TEMP_iFileInfoToList = -1;
				m_AppFrame.StartTimer( "list remote files", LayoutRoot, 1, 0, true );
			}
			else if( m_iPhase == 2 )
			{
											
				btnAddRemAll.Visibility = Rsc.Collapsed;

				m_iStatAllFolders = 0;
				m_iStatAllFiles = 0;
				foreach( RscFtp_DownLoad_SimpleLbItem it in m_aTodo)
				{
					//if( it.bFolder && it.Include ) m_iStatAllFolders++;
					if( it.bFile )
					{
						if( it.Include ) m_iStatAllFiles++;
						it.BtnCust2Vis = Rsc.Collapsed;
					}
				}
				_UpdateRes(true);
				//SLOW!!!
				/*
				lbItems.ItemsSource = null;
				lbItems.ItemsSource = m_aTodo;
				*/
				
				prsBar.Visibility = Rsc.Visible;
				prsBar.IsIndeterminate = false;
				prsBar.Minimum = 0;
				prsBar.Value = 0;
				prsBar.Maximum = m_aTodo.Count - 1;
				
				btnStartStop.Content = "Cancel";
				btnStartStop.Background = new SolidColorBrush(Colors.Red);
				
				m_iActiveToDo = -1;
				
				bool bTmp = false;
				RaiseNextFtpClientCommand(out bTmp);
			}
			else
			{
				m_bUserStop = true;
			}
		}
		
		private void m_AppFrame_OnTimer(object sender, RscAppFrameTimerEventArgs e)
		{
			switch( e.Reason )
			{
				
				case "list remote files_Cancel" :
				{
					prsBar.Visibility = Rsc.Collapsed;
					prsBarData.Visibility = Rsc.Collapsed;
					
					_SetStatusText( "User canceled operation!", StatusColoring.Error );
					
					break;
				}
				
				case "list remote files" :
				{
					
					_RefreshStatusText();
									
					RscFtp_DownLoad_SimpleLbItem itFolder = m_aTodo[m_TEMP_iTodoIdxToList] as RscFtp_DownLoad_SimpleLbItem;
					if( itFolder.bFolder && itFolder.ServerData != null && itFolder.Include )
					{
						//m_iStatAllFolders++;
						
						RscFtpServerDataItemFileInfo fi;
						
						itFolder.m_TEMP_iFileInfoToList++;
						
						if( itFolder.m_TEMP_iFileInfoToList < itFolder.ServerData.Count )
						{
							if( itFolder.m_TEMP_iFileInfoToList == 0 )
							{
								e.Max += itFolder.ServerData.Count;
							}
							
							fi = (RscFtpServerDataItemFileInfo) itFolder.ServerData.GetItem(itFolder.m_TEMP_iFileInfoToList);
							
							if( !fi.IsFolder )
							{
								m_iStatAllFiles++;
								_UpdateRes(true);
								
								RscFtp_DownLoad_SimpleLbItem it = new RscFtp_DownLoad_SimpleLbItem( m_aTodo );
								
								it.bFile = true;
								it.bFolder = false;
								it.bWalked = false;
								
								it.strOwner = itFolder.GetPath();
								it.strName = fi.GetItemTitle();
								
								it.itOwner = itFolder;
								
								long lSize = 0;
								if( !long.TryParse(fi.m_sSize, out lSize) )
									lSize = 0;
								it.RemoteFileSize = lSize;
								
								AddTaskEx(it, true);
							}
							
							//e.Max++;
							return;
						}
						
						//BUG: fails!!!
						/*
						spTodo.Children.Remove(itFolder.grdOut);
						m_todo.RemoveAt(iIdx);
						iIdx--;
						iFldrCnt--;
						*/
						
					}
					
					itFolder.BtnCust2Vis = Rsc.Collapsed;
					//SLOW!!!
					/*
					lbItems.ItemsSource = null;
					lbItems.ItemsSource = m_todo;
					*/
					
					bool bGoNext = false;
					if( m_TEMP_iTodoIdxToList < m_aTodo.Count - 1 )
					{
						//Folder items are at the begining...
						if( (m_aTodo[m_TEMP_iTodoIdxToList + 1] as RscFtp_DownLoad_SimpleLbItem).bFolder )
						{
							(m_aTodo[m_TEMP_iTodoIdxToList + 1] as RscFtp_DownLoad_SimpleLbItem).m_TEMP_iFileInfoToList = -1;
							bGoNext = true;
						}
					}
		
					if( bGoNext )
					{
						m_TEMP_iTodoIdxToList++;
						
						e.Max++;
					}
					else
					{					
						prsBar.Visibility = Rsc.Collapsed;
						prsBarData.Visibility = Rsc.Collapsed;
									
						btnStartStop.Visibility = Rsc.Visible;
						btnStartStop.Content = "Download Remote Files";
						btnStartStop.Background = new SolidColorBrush(Colors.Green);
						
						m_iPhase++;
						
						if( !m_bRestoreBackUpUsrChoiche )
						{
							m_bStarted = true;
							m_dtStart = DateTime.Now;
						}
					
						if( m_bRestoreBackUpUsrChoiche )
						{
							//AUTO CLICK...
							m_AppFrame.AutoClick( btnStartStop, new System.Windows.RoutedEventHandler(btnStartStop_Click) );
						}
					}
					
					break;
				}
				
			}
		}
		
		private void OnFtpClientCommandDone()
		{
			for(;;)
			{
				bool bNoop = false;
				
				if( m_strSessionFolder.Length == 0 )
				{
					lbLogs.Visibility = Rsc.Collapsed;
					m_AppFrame.ShowButtonNext(true);
					
					todoPanel.Visibility = Rsc.Visible;
					
					prsBar.Visibility = Rsc.Collapsed;
					prsBarData.Visibility = Rsc.Collapsed;
					
					btnStartStop.Visibility = Rsc.Visible;
					if( m_bRestoreBackUp )
					{
						btnStartStop.Content = "List all Remote subFolders recursively";
					}
					else
					{
						if( !m_bRestoreBackUpUsrChoiche && m_strSessionFolderRootMIN.Length > 0 )
							btnStartStop.Content = "List Folders in Remote Backup Root";
						else
							btnStartStop.Content = "List Folders in Remote User Root";
					}
					btnStartStop.Background = new SolidColorBrush(Colors.Green);
					
					string sAddr = m_ftpc.UserName;
					if( sAddr.Length > 0 ) sAddr += "@";
					sAddr += m_ftpc.IPAddress + ":" + m_ftpc.Port.ToString();
					svrAddr.Text = sAddr;
					svrTit.Text =  m_ftpc.ServerTitle;
					if( m_strSessionFolderRoot.Length == 0 )
						svrRoot.Text = m_ftpc.BackSlashInPath;
					else
						svrRoot.Text = m_strSessionFolderRoot;
					
					_UpdateRes(true);
					
					if( m_bRestoreBackUpUsrChoiche )
					{
						//AUTO CLICK...
						m_AppFrame.AutoClick( btnStartStop, new System.Windows.RoutedEventHandler(btnStartStop_Click) );
					}
				}
				else if( m_bUserStop )
				{
					
					RscFtp_DownLoad_SimpleLbItem it;
					
					/*
					int idx = -1;
					for(;;)
					{
						idx++;
						if( idx >= m_todo.Count ) break;
						it = m_todo[ idx ];
						
						spTodo.Children.Remove(it.grdOut);
						m_todo.RemoveAt(idx);
						idx--;
					}
					*/
					m_aTodo.Clear();
							
					prsBar.Visibility = Rsc.Collapsed;
					prsBarData.Visibility = Rsc.Collapsed;
					btnStartStop.Visibility = Rsc.Collapsed;
					
					it = new RscFtp_DownLoad_SimpleLbItem( m_aTodo );	
					it.bFile = false;
					it.bFolder = false;
					it.bWalked = false;
					it.strOwner = "USER stopped operation! Some items were not downloaded from server-folder: '" +
						m_strServerRoot + m_strSessionFolder + "'!";
					it.strName = "FAIL!";
					it.CustomBackColor = Colors.Red;
					it.CustomForeColor = Colors.White;
					
					AddTaskEx(it);
					
					m_bEnded = true;
				}
				else if( m_iActiveToDo < m_aTodo.Count )
				{
					if( RaiseNextFtpClientCommand(out bNoop, true) )
					{
						if( m_iPhase == 2 ) prsBar.Value += 1;
					}
					else
					{
						if( m_iPhase == 0 )
						{
							prsBar.Visibility = Rsc.Collapsed;
							prsBarData.Visibility = Rsc.Collapsed;
							
							m_iStatAllFiles = 0;
							m_iStatAllFolders = m_aTodo.Count;
							_UpdateRes(true);
							
							if( m_bRestoreBackUp )
							{
								foreach(RscFtp_DownLoad_SimpleLbItem it in m_aTodo)
								{
									it.BtnCust2Vis = Rsc.Visible;
								}
								lbItems.ItemsSource = null;
								lbItems.ItemsSource = m_aTodo;
								
								btnStartStop.Visibility = Rsc.Visible;
								btnStartStop.Content = "List Remote Files";
								btnStartStop.Background = new SolidColorBrush(Colors.Green);
								
								btnAddRemAll.Content = "Exclude all Folder";
								btnAddRemAll.Visibility = Rsc.Visible;
								
								m_iPhase++;
					
								if( m_bRestoreBackUpUsrChoiche )
								{
									//AUTO CLICK...
									m_AppFrame.AutoClick( btnStartStop, new System.Windows.RoutedEventHandler(btnStartStop_Click) );
								}
							}
							else
							{
								foreach(RscFtp_DownLoad_SimpleLbItem it in m_aTodo)
								{
									if( it.strName.Length == 0 )
										it.BtnCust1Img = m_isSkipPrev;
									else
										it.BtnCust1Img = m_isSkipNext;
									
									it.BtnCust1Vis = Rsc.Visible;
								}
								//SLOW!!!
								/*
								lbItems.ItemsSource = null;
								lbItems.ItemsSource = m_aTodo;
								*/
								
								btnStartStop.Visibility = Rsc.Visible;
								btnStartStop.Content = "List all Remote subFolders recursively";
								btnStartStop.Background = new SolidColorBrush(Colors.Green);
								
								btnAddRemAll.Visibility = Rsc.Collapsed;
							}
						}
						else //if( m_iPhase == 2 )
						{
							bool bDone = true;
							
							RscFtp_DownLoad_SimpleLbItem it;
							int idx = -1;
							for(;;)
							{
								idx++;
								if( idx >= m_aTodo.Count ) break;
								it = m_aTodo[ idx ] as RscFtp_DownLoad_SimpleLbItem;
								
								if( it.Done )
								{
									m_aTodo.RemoveAt(idx);
									idx--;
								}
								else
								{
									bDone = false;
								}
							}
							
							if( !bDone )
							{
								/*
								if( (m_iPhase == 0 && m_bChkBack) && (m_iPhaseMAX > 0) )
								{
									m_iPhase++;
									
									_UpdateRes();
									
									prsBar.Maximum += (m_todo.Count - 1);
									
									m_iActiveToDo = -1;
									RaiseNextFtpClientCommand(out bNoop, true);
								}
								else
								{
								*/
									prsBar.Visibility = Rsc.Collapsed;
									prsBarData.Visibility = Rsc.Collapsed;
									btnStartStop.Visibility = Rsc.Collapsed;
									
									it = new RscFtp_DownLoad_SimpleLbItem( m_aTodo );	
									it.bFile = false;
									it.bFolder = false;
									it.bWalked = false;
									it.strOwner = "Some items were not downloaded from server-folder: '" +
										m_strServerRoot + m_strSessionFolder + "'!";
									it.strName = "FAIL!";
									it.CustomBackColor = Colors.Red;
									it.CustomForeColor = Colors.White;
								
									AddTaskEx(it);
								
									m_bEnded = true;
								
								/*
								}
								*/
							}
							else
							{
								prsBar.Visibility = Rsc.Collapsed;
								prsBarData.Visibility = Rsc.Collapsed;
								btnStartStop.Visibility = Rsc.Collapsed;
								
								it = new RscFtp_DownLoad_SimpleLbItem( m_aTodo );	
								it.bFile = false;
								it.bFolder = false;
								it.bWalked = false;
								it.strOwner = "All selected files and folders has been downloaded from server-folder: '" +
									m_strServerRoot + m_strSessionFolder + "'!";
								it.strName = "Success!";
								it.CustomBackColor = Colors.Green;
								it.CustomForeColor = Colors.White;
								
								AddTaskEx(it);
								
								m_bEnded = true;
							}
							
							m_iPhase++; //To make Back button to exit app!
						}
					}
				}
				
				if( !bNoop ) break;
			}
		}
		
		//
		// //
		//

		private void m_ftpc_CommandDoneAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_CommandDoneSYNC( sender, e ); });
		}
		private void m_ftpc_CommandDoneSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{	
			string sLog = "";
			if( e.ClientCommand != null )
			{
				sLog = "DONE! - " + e.ClientCommand.ToString() + " ("
					+ e.ClientCommand.ResponseCountArrived.ToString( ) + " / "+ e.ClientCommand.ResponseCount.ToString( ) + ")";
			}
			
			//Last command executed (with error / success)...
			_Log("", sLog, false);
			
			OnFtpClientCommandDone();
		}

		private void m_ftpc_CommandSocketConnectedAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_CommandSocketConnectedSYNC( sender, e ); });
		}
		private void m_ftpc_CommandSocketConnectedSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			_Log("21", "...connected!", false);			
			m_ftpc.ReadServerResponse( e.ClientCommand );
		}

		private void m_ftpc_ServerResponseAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_ServerResponseSYNC( sender, e ); });
		}
		private void m_ftpc_ServerResponseSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			_Log("21", e.ServerResponse);
			
			m_ftpc.DispatchResponse( e.ClientCommand, e.ServerResponse );
		}

		private void m_ftpc_CommandSentAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_CommandSentSYNC( sender, e ); });
		}
		private void m_ftpc_CommandSentSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			m_ftpc.ReadServerResponse( e.ClientCommand );
		}

		private void m_ftpc_DataSocketConnectedAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_DataSocketConnectedSYNC( sender, e ); });
		}
		private void m_ftpc_DataSocketConnectedSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			_Log("20", "...connected! (" + e.ClientCommand.ToString() + ")", false);
			
			//MUST NOT!!!
			//this.Dispatcher.BeginInvoke(() => { ReceiveData20( ); });
		}

		private void m_ftpc_ServerDataReceivedAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_ServerDataReceivedSYNC( sender, e ); });
		}
		private void m_ftpc_ServerDataReceivedSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			if( !e.ServerData.ParseOk )
			{
				_Log("20", "<DATA ERROR> " + e.ClientCommand.ToString( ), false);
			}
			else
			{
				switch( e.ClientCommand.Command )
				{
					
					case "LIST" :
					{
						prsBarData.Visibility = Rsc.Collapsed;
						
						//Store file+folder list for folder item...
						RscFtp_DownLoad_SimpleLbItem itCurrent = null;
						if( m_aTodo.Count > 0 )
						{
							int iIdx = (int) e.ClientCommand.Tag;
							itCurrent = m_aTodo[iIdx] as RscFtp_DownLoad_SimpleLbItem;
							itCurrent.ServerData = e.ServerData;
							if( m_bLogItems )
							{
								//itCurrent.tbDetails.Text = itCurrent.tbDetails.Text + "\r\n" + "Direcory Listing received (" 
								//	+ e.ClientCommand.Arg1 + ").";
								itCurrent.Details = itCurrent.Details + "\r\n" + "Direcory Listing received (" 
									+ e.ClientCommand.Arg1 + ").";
								
								//SLOW!!!
								/*
								lbItems.ItemsSource = null;
								lbItems.ItemsSource = m_todo;
								*/
							}
						}

						int iCount;
						RscFtpServerDataItemFileInfo fi;
						
						iCount = e.ServerData.Count;
						
						if( iCount == 0 )
						{
							_Log("20", e.ClientCommand.Command + " returned 0 item.", false, true);
						}
						else
						{
						
							for( int i = 0; i < iCount; i++ )
							{
								fi = (RscFtpServerDataItemFileInfo) e.ServerData.GetItem(i);
								
								_Log("20", fi.GetItemTitle( ), false, true, fi);
								
								if( fi.IsFolder )
								{
									RscFtp_DownLoad_SimpleLbItem it = new RscFtp_DownLoad_SimpleLbItem( m_aTodo );
									
									it.bFile = false;
									it.bFolder = true;
									it.bWalked = false;
									
									it.strOwner = itCurrent.GetPath();
									it.strName = fi.GetItemTitle( );
									
									AddTaskEx(it);
								}
							}
						
						}
						
						break;
					}
					
					case "RETR" :
					{
						prsBarData.Visibility = Rsc.Collapsed;

						//Vibrated...
						//prsBarData.Visibility = Rsc.Collapsed;
						prsBarData.Value = 0;
						
						//Store file+folder list for folder item...
						RscFtp_DownLoad_SimpleLbItem itCurrent = null;
						if( m_aTodo.Count > 0 )
						{
							int iIdx = (int) e.ClientCommand.Tag;
							itCurrent = m_aTodo[iIdx] as RscFtp_DownLoad_SimpleLbItem;
							if( m_bLogItems )
							{
								//itCurrent.tbDetails.Text = itCurrent.tbDetails.Text + "\r\n" + 
								//	"Downloaded " + e.ClientCommand.StreamDone.ToString() + " bytes" + ".";
								itCurrent.Details = itCurrent.Details + "\r\n" + 
									"Downloaded " + e.ClientCommand.StreamDone.ToString() + " bytes" + ".";
								
								//SLOW!!!
								/*
								lbItems.ItemsSource = null;
								lbItems.ItemsSource = m_aTodo;
								*/
							}
						}
						
						_Log("20", e.ClientCommand.Command + " Downloaded " + 
							e.ClientCommand.StreamDone.ToString() + " bytes.", false, true);
						
						if( e.ClientCommand.HasMemStream )
						{
							
							string sPath = e.ClientCommand.UserData;
							
							_Log("20", e.ClientCommand.Command + " saving local file '" + 
								sPath + "'.", false, true);
							
							RscStore store = new RscStore();
							
							string sDirPath = "";
							int iPos = sPath.LastIndexOf('\\');
							if( iPos >= 0 ) sDirPath = sPath.Substring(0, iPos);
							if( sDirPath.Length > 0 )
							{
								store.CreateFolderPath( sDirPath );
							}
							
							if( store.FileExists( sPath ) ) store.DeleteFile( sPath );
							System.IO.Stream stream = store.CreateFile(sPath);
							e.ServerData.MemStream.WriteTo(stream);
							e.ServerData.MemStream.Close();
							stream.Close();
							
						}
						
						break;
					}
					
				}
			}
		}

		private void m_ftpc_ServerDataSentAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_ServerDataSentSYNC( sender, e ); });
		}
		private void m_ftpc_ServerDataSentSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			_Log("20", "...sent!", false);
			
			prsBarData.Visibility = Rsc.Collapsed;
			
			/*
			 * For WM6.5|Mocha FTP Server end of sent data has been lost!
			 * Possible FIX!!!
			 *
			 * THIS IS!!!
			 */
			System.Threading.Thread.Sleep(1000);
			m_ftpc.CloseDataSocket( e.ClientCommand );
		}

		private void m_ftpc_DataSocketClosingAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_DataSocketClosingSYNC( sender, e ); });
		}
		private void m_ftpc_DataSocketClosingSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			_Log("20", "Closing...", false);				
		}

		private void m_ftpc_LogAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_LogSYNC( sender, e ); });
		}
		private void m_ftpc_LogSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			//KNOWN BUG!!!
			if( e.Message == "unkLISTresLEN" )
			{
				m_AppFrame.TRACE = "file\r\nLIST\r\nloss";
			}
			
			string sLog = "";
			if( e.ClientCommand != null )
			{
				sLog = e.Message + "\r\n" + e.ClientCommand.ToString() + " ("
					+ e.ClientCommand.ResponseCountArrived.ToString( ) + " / " + e.ClientCommand.ResponseCount.ToString( ) + ")";
			}
			
			//Last command executed (with error / success)...
			_Log("", "Log: " + sLog, false);
		}

		private void m_ftpc_ProgressAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_ProgressSYNC( sender, e ); });
		}
		private void m_ftpc_ProgressSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			if( e.ProgressPos >= e.ProgressMax ) return;
			
			prsBarData.Maximum = e.ProgressMax;
			prsBarData.Minimum = 0;
			prsBarData.Value = e.ProgressPos;
			
			prsBarData.Visibility = Rsc.Visible;
			
			m_AppFrame.UpdateAppMemInfo();
			
			_RefreshStatusText();
		}
		
		//
		// //
		//
		
		private void _Log( string sCh, RscFtpServerResponse resp )
		{
			StatusColoring sc;
			
			switch( resp.Type )
			{
				
				case RscFtpServerResponse.ServerResponseType.Success :
					sc = StatusColoring.Success;
					break;
					
				case RscFtpServerResponse.ServerResponseType.Data :
					sc = StatusColoring.Normal;
					break;
					
				case RscFtpServerResponse.ServerResponseType.Warning :
					sc = StatusColoring.Warning;
					break;
					
				//case RscFtpServerResponse.ServerResponseType.Unknown :
				default :
					sc = StatusColoring.Error;
					break;
					
			}
			
			string sResp = ""; //resp.GetTypeAsString( );
			sResp += "(";
			sResp += resp.Code.ToString();
			sResp += ") ";
			sResp += resp.Message;
			_SetStatusText( sResp, sc );

			_Log(sCh, resp.ToString(), false);
		}
		
		private void _Log( string sCh, string sLog, bool bSent = true, bool bDataItem = false, object oTag = null )
		{
			MyLogItem li = new MyLogItem();
			li.Parent = m_logs;
			
			li.bFullEmpty = false;
			if( sCh.Length == 0 && sLog.Length == 0 ) li.bFullEmpty = true;
			
			li.bSent = bSent;
			
			li.sCh = sCh;
			li.sLog = sLog;
			
			li.bDataItem = bDataItem;
			li.oTag = oTag;
			
			m_logs.Insert(0, li );
			
			while(m_logs.Count > ciLogMaxLnCnt)
			{
				m_logs.RemoveAt(m_logs.Count - 1);
			}
	
			_RefreshStatusText();
			
			/*
			string strItem = "";
			if( bSent )
				strItem = "--> ";
			else
				strItem = "<-- ";
			strItem += sCh + "|";
			strItem += sLog;
			
			if( strItem.Length > ciLogCChMax )
			{
				string sPre = strItem.Substring(0, ciLogCChMax) + "...";
				lbLogs.Items.Insert(0, sPre);
				
				string sPst = strItem.Substring(ciLogCChMax);
				if( sPst.Length > ciLogCChMax ) sPst = sPst.Substring(sPst.Length - ciLogCChMax, ciLogCChMax);
				lbLogs.Items.Insert(1, "..." + sPst);
				
				lbLogs.Items.Insert(2, " ");
			}
			else
			{
				lbLogs.Items.Insert(0, strItem);
			}
			
			while(lbLogs.Items.Count > ciLogMaxLnCnt)
			{
				lbLogs.Items.RemoveAt(lbLogs.Items.Count - 1);
			}
			*/
		}

		//
		// //
		//
		
		private void AddTaskEx(RscFtp_DownLoad_SimpleLbItem it, bool bRemoveVisible = false)
		{
			it.BtnCust1Img = m_isDummy;
			it.BtnCust1Vis = Rsc.Collapsed;
			
			it.BtnCust2Img = m_isRemove;
			it.BtnCust2Vis = Rsc.ConditionalVisibility( bRemoveVisible );
			
			//it.Parent = m_aTodo;
			m_aTodo.Add( it );
		}

		private void SimpleLbItem_BtnCust2_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			RscFtp_DownLoad_SimpleLbItem it;
			it = (RscFtp_DownLoad_SimpleLbItem) btn.Tag;
			
			if( !it.Include )
			{
				it.BtnCust2Img = m_isRemove;
				it.Include = true;
				
				if( it.bFolder )
				{
					m_iStatAllFolders++;
				}
				else if( it.bFile )
				{
					m_iStatAllFiles++;
				}
				_UpdateRes(true);
			}
			else
			{
				it.BtnCust2Img = m_isAdd;
				it.Include = false;
				
				if( it.bFolder )
				{
					m_iStatAllFolders--;
				}
				else if( it.bFile )
				{
					m_iStatAllFiles--;
				}
				_UpdateRes(true);
			}
			//SLOW!!!
			/*
			lbItems.ItemsSource = null;
			lbItems.ItemsSource = m_aTodo;
			*/
			
			if( m_iStatAllFolders + m_iStatAllFiles == 0 )
				btnStartStop.IsEnabled = false;
			else
				btnStartStop.IsEnabled = true;
		}

		private void SimpleLbItem_BtnCust1_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_bRestoreBackUp ) return;
			
			Button btn;
			btn = ((Button) sender);
			RscFtp_DownLoad_SimpleLbItem it;
			it = (RscFtp_DownLoad_SimpleLbItem) btn.Tag;
			
			if( !it.bFolder ) return;
			
			string strTit = it.Title;
			bool bGoBack = (strTit == it.strNameEmpty);
			
			if( m_strSessionFolderRoot.Length < 2 ) // "" or "\"
			{
				if( bGoBack ) return;
				m_strSessionFolderRoot = strTit + m_ftpc.BackSlashInPath;
			}
			else
			{
				if( bGoBack )
				{
					string sRoot = RscUtils.RemoveEnding( m_strSessionFolderRoot, m_ftpc.BackSlashInPath );
					
					//FIX...
					if( sRoot.CompareTo( m_strSessionFolderRootMIN ) == 0 )
						return;
					
					int iPos = sRoot.LastIndexOf(m_ftpc.BackSlashInPath);
					if( iPos >= 0 )
						m_strSessionFolderRoot = sRoot.Substring(0, iPos + 1);
					else
						m_strSessionFolderRoot = m_ftpc.BackSlashInPath;
				}
				else
				{
					//FIX...
					m_strSessionFolderRoot = RscUtils.RemoveEnding( m_strSessionFolderRoot, m_ftpc.BackSlashInPath );
					if( m_strSessionFolderRoot.Length > 0 ) m_strSessionFolderRoot += m_ftpc.BackSlashInPath;
					
					m_strSessionFolderRoot += strTit + m_ftpc.BackSlashInPath;
				}
			}
				
			//m_AppFrame.TRACE = m_strSessionFolderRoot;
			
			svrRoot.Text = m_strSessionFolderRoot;
			
			//m_iPhase = 0;
			
			_ListFolders(true);
		}
		
		private void _SetStatusText( string sStatus = "", StatusColoring sc = StatusColoring.Normal )
		{
			sSAVE_Status = sStatus;
			scSAVE = sc;
			
			_RefreshStatusText();
		}
		
		private void _RefreshStatusText()
		{
			string sPlus = "";
			
			sPlus += "\r\n" + "UP ->"
				+ " sys: " + RscUtils.toMBstr(m_ftpc.StatUpCmd.ByteCount)
					/* + " (" + m_ftpc.StatUpCmd.Seconds.ToString() + ")" */
				+ " |"
				+ " usr: " + RscUtils.toMBstr(m_ftpc.StatUpDat.ByteCount)
					/*+ " (" + m_ftpc.StatUpDat.Seconds.ToString() + ")"*/ ;
			
			sPlus += "\r\n" + "DN <-"
				+ " sys: " + RscUtils.toMBstr(m_ftpc.StatDnCmd.ByteCount)
					/*+ " (" + m_ftpc.StatDnCmd.Seconds.ToString() + ")"*/
				+ " |"
				+ " usr: " + RscUtils.toMBstr(m_ftpc.StatDnDat.ByteCount)
					/*+ " (" + m_ftpc.StatDnDat.Seconds.ToString() + ")"*/;
			
			
			bool bSpeed = false;
			//long lUp = m_ftpc.BytesPerSecUp;
			long lDn = m_ftpc.BytesPerSecDn;
			if( /*lUp > 0 ||*/ lDn > 0 )
			{
				bSpeed = true;
				
				sPlus += "\r\nSpeed ";
				/*if( lUp > 0 )
				{
					sPlus += "UP: " + RscUtils.toMBstr(lUp) + "/s";
				}
				if( lDn > 0 )
				{
					if( lUp > 0 ) sPlus += " | ";*/
					sPlus += /*"DN: " +*/ RscUtils.toMBstr(lDn) + "/s";
				//}
			}
			
			if( m_bStarted )
			{
				if( bSpeed )
					sPlus += " | ";
				else
					sPlus += "\r\n";
					
				if( !m_bEnded ) m_dtEnd = DateTime.Now;
				
				TimeSpan ts = m_dtEnd - m_dtStart;
				sPlus += RscUtils.toDurationStr(ts.ToString());
			}
			
			m_AppFrame.SetStatusText( sSAVE_Status + sPlus, scSAVE );
		}
		
		private void SimpleLbItem_BtnCust3_Click(object sender, System.Windows.RoutedEventArgs e)
		{
		}
		
		private void SimpleLbItem_BtnCust4_Click(object sender, System.Windows.RoutedEventArgs e)
		{
		}
		
		private void SimpleLbItem_Btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
		}
		
    }
	
}
