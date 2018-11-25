using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Media;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;

using Ressive.Utils;
using Ressive.Store;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

using Ressive.FTP;

namespace RscFtpClients
{
	
    public partial class RscFtp_UpLoadV11 : PhoneApplicationPage
    {
		
		const string csDlgsAssy = "Lib_RscIPgC_Dlgs";
		
		const int ciLogCChMax = 64;
		const int ciLogMaxLnCnt = 256;
		
		RscAppFrame m_AppFrame;
	
		RscPageArgsRetManager m_AppArgs;
		
		RscFtpClient m_ftpc = null;
		
		RscSimpleLbItemList m_aTodo = null;
		int m_iActiveToDo = -1;
		
		string m_strSessionFolderDefault = "";
		
		string m_strServerRoot = "";
		string m_strSessionFolder = "";
		
		bool m_bOverWriteExisting = false;
		
		bool m_bChkBack = false;
		int m_iPhase = 0;
		int m_iPhaseCYC = 1;
		int m_iPhaseMAX = 0;
		
		int m_iStatAllFiles = 0;
		int m_iStatAllFolders = 0;
		int m_iStatDoneFolder = 0;
		int m_iStatSkipFile = 0;
		int m_iStatDoneFile = 0;
		int m_iStatFailFile = 0;
		
		bool m_bLogItems = false;
		bool m_bUpHidden = false;
		
		bool m_bUserStop = false;
		
		RscIconButton m_btnUpFldrBrowse;
		TextBoxDenieEdit m_txtUpFldr;
		RscIconButton m_btnUpFldrDelete;
		DispatcherTimer m_tmrBrowse;
		
		string m_sUpFolder = "";
		
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
					sDet += m_iStatDoneFile.ToString() + " write";
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
			if( m_iPhaseCYC > 1 ) sDone += "| " + m_iPhaseCYC.ToString() + " cycle";
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
			todoResDone.Text = sDone;
			if( sDone.Length > 0 )
				todoResDonePanel.Visibility = Rsc.Visible;
			else
				todoResDonePanel.Visibility = Rsc.Collapsed;
			
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
			todoRes.Text = sToDo;		
			if( (m_iStatAllFiles + m_iStatAllFolders) == 0 )
				todoRes.Text = " ";
		}
		
        public RscFtp_UpLoadV11()
        {
            InitializeComponent();
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "FTP Upload 1.1", "Images/IcoSm001_FtpUpLoad.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			// ///////////////
			imgInput.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Open.jpg");
			imgIpUpIco.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Inc.jpg");
			imgIpDnIco.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Dec.jpg");
			
			m_strSessionFolderDefault = RscUtils.GetDeviceName() + " (BackUp)";

			txSvrIP.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11", "LastSvrIP", "192.168.0.0" );
			LoadFromReg( );
			
			m_btnUpFldrBrowse = new RscIconButton(upFldrGrid, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible);
			m_btnUpFldrBrowse.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_(dir).jpg");
			m_btnUpFldrBrowse.Click += new System.Windows.RoutedEventHandler(m_btnUpFldrBrowse_Click);
			
			m_txtUpFldr = new TextBoxDenieEdit(true, true, upFldrGrid, Grid.ColumnProperty, 1);
			m_txtUpFldr.Background = new SolidColorBrush(Colors.LightGray);
			m_txtUpFldr.Foreground = new SolidColorBrush(Colors.Black);
			m_txtUpFldr.FontSize = 16;
			m_txtUpFldr.MarginOffset = new Thickness( 10, 7, 10, 7 );
			m_txtUpFldr.Text = "";
			
			m_btnUpFldrDelete = new RscIconButton(upFldrGrid, Grid.ColumnProperty, 3, 50, 50, Rsc.Visible);
			m_btnUpFldrDelete.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Delete.jpg");
			m_btnUpFldrDelete.Click +=new System.Windows.RoutedEventHandler(m_btnUpFldrDelete_Click);
			
			m_AppArgs = new RscPageArgsRetManager();
			
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
			
			m_tmrBrowse = new DispatcherTimer();
			m_tmrBrowse.Interval = new TimeSpan(500);
			m_tmrBrowse.Tick += new System.EventHandler(m_tmrBrowse_Tick);
			
			m_tmrInput = new DispatcherTimer();
			m_tmrInput.Interval = new TimeSpan(500);
			m_tmrInput.Tick += new System.EventHandler(m_tmrInput_Tick);
			
			m_logs.ListBoxAsteriskWidth = 100;
			lbLogs.ItemsSource = m_logs;
			lbLogs.SizeChanged += new System.Windows.SizeChangedEventHandler(lbLogs_SizeChanged);
			
			m_aTodo = new RscSimpleLbItemList( lbItems, m_AppFrame.Theme );
			
			this.Unloaded += new System.Windows.RoutedEventHandler(RscFtpUpLoadV10_Unloaded);
			this.Loaded +=new System.Windows.RoutedEventHandler(RscFtpUpLoadV10_Loaded);
			
			bool bOw = RscRegistry.ReadBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11", "Overwrite", true );
			if( bOw )
			{
				rbOverAll.IsChecked = true;
				grdAtt.Visibility = Rsc.Collapsed;
			}
			else
			{
				rbAddNews.IsChecked = true;
				grdAtt.Visibility = Rsc.Visible;
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

		private void RscFtpUpLoadV10_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			m_AppFrame.SetNoSleep( true );
		}		

		private void RscFtpUpLoadV10_Unloaded(object sender, System.Windows.RoutedEventArgs e)
		{
			m_AppFrame.SetNoSleep( false );
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
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
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
						
						case "UpFldrPath" :
							if( appOutput.GetFlag(0) == "Ok" )
							{
								m_txtUpFldr.Text = appOutput.GetData(0);
							}
							else
							{
								//NOP...
							}
							break;
							
					}
				}
				
				m_AppArgs.Clear();
			}
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
		
		private void rbOverAll_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			grdAtt.Visibility = Rsc.Collapsed;
		}
		private void rbAddNews_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			grdAtt.Visibility = Rsc.Visible;
		}
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			if( parPanel.Visibility == Rsc.Visible )
			{
				
				if( !DeviceNetworkInformation.IsNetworkAvailable )
				{
					ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
					connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.WiFi;
					connectionSettingsTask.Show();
					
					return;
				}
				if( m_txtUpFldr.Text.Length == 0 )
				{
					if( MessageBoxResult.OK != MessageBox.Show( "Do you really want to upload of all files and folders of all storages?\n\n(Press Back to cancel...)" ) )
						return;
				}
				
				parPanel.Visibility = Rsc.Collapsed;
				
				lbLogs.Visibility = Rsc.Visible;
				m_AppFrame.ShowButtonNext(false);
				
				m_bOverWriteExisting = false;
				if( rbOverAll.IsChecked.Value )
				{
					m_bOverWriteExisting = true;
				}
				
				m_bChkBack = false;
				if( chbChkBack.IsChecked.Value )
				{
					m_bChkBack = true;
				}
				m_iPhase = 0;
				m_iPhaseCYC = 1;
				m_iPhaseMAX = 0;
				
				m_bLogItems = false;
				if( chbItemLog.IsChecked.Value )
				{
					m_bLogItems = true;
				}
				
				m_bUpHidden = false;
				if( chbUpHidden.IsChecked.Value )
				{
					m_bUpHidden = true;
				}
				
				m_sUpFolder = m_txtUpFldr.Text;
				
				//WP81 FIX
				//m_ftpc.SetFastConnection( chbFastConn.IsChecked.Value, chbFastConnEx.IsChecked.Value );
				
				m_bUserStop = false;
			
				sSAVE_Status = "";
				scSAVE = StatusColoring.Normal;
				
				m_dtStart = DateTime.Now;
				m_bStarted = true;
				m_dtEnd = m_dtStart;
				m_bEnded = false;
				
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
				prsBarData.Visibility = Rsc.Collapsed;
				
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
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11" + "\\" + txSvrIP.Text,
				"Port", 2221.ToString( ) );
			
			txUsr.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11" + "\\" + txSvrIP.Text,
				"Usr", "usr" );
			
			//Do not store...
			txPwd.Text = "";
			
			//WP81 FIX
			/*
			chbFastConn.IsChecked = RscRegistry.ReadBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11" + "\\" + txSvrIP.Text,
				"FastConn", false );
			
			chbFastConnEx.IsChecked = RscRegistry.ReadBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11" + "\\" + txSvrIP.Text,
				"FastConnEx", false );
			*/
		}
		
		private void SaveToReg( )
		{
			RscRegistry.WriteBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11", "Overwrite", rbOverAll.IsChecked.Value );			
			
			RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11", "LastSvrIP", txSvrIP.Text );			
			
			RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11" + "\\" + txSvrIP.Text,
				"Port", txSvrPort.Text );
			
			RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11" + "\\" + txSvrIP.Text,
				"Usr", txUsr.Text );
			
			//Do not store...
			//txPwd.Text = "";
			
			//WP81 FIX
			/*
			RscRegistry.WriteBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11" + "\\" + txSvrIP.Text,
				"FastConn", chbFastConn.IsChecked.Value );
			
			RscRegistry.WriteBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_UpLoadV11" + "\\" + txSvrIP.Text,
				"FastConnEx", chbFastConnEx.IsChecked.Value );
			*/
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( m_AppFrame.CancelTimer() )
			{
				e.Cancel = true;
				return;
			}
			
			if( todoPanel.Visibility == Rsc.Visible )
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
				
				bool bUserDoesNotStarted = (m_strSessionFolder.Length == 0);
				
				_Log("21", "Disconnect...");
				
				m_ftpc.CloseAllSockets();
				
				todoPanel.Visibility = Rsc.Collapsed;
				
				prsBar.Visibility = Rsc.Collapsed;
				prsBarData.Visibility = Rsc.Collapsed;
				
				btnStartStop.Visibility = Rsc.Collapsed;
				btnStartStop.Content = "Start";
				btnStartStop.Background = new SolidColorBrush(Colors.Green);

				m_logs.Clear();
				
				m_aTodo.Clear();
				m_iActiveToDo = -1;
				
				m_strServerRoot = "";
				m_strSessionFolder = "";
				
				m_bOverWriteExisting = false;
				
				m_bChkBack = false;
				m_iPhase = 0;
				m_iPhaseCYC = 1;
				m_iPhaseMAX = 0;
				
				m_bUserStop = false;
				
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
			
			RscFtp_UpLoad_SimpleLbItem it = null;
			
			if( m_iActiveToDo > -1 )
			{
				it = m_aTodo[m_iActiveToDo] as RscFtp_UpLoad_SimpleLbItem;
				
				//it.tbTit.Text = it.GetStateTitle(m_bLogItems);
				//SLOW!!!
				/*
				lbItems.ItemsSource = null;
				lbItems.ItemsSource = m_aTodo;
				*/
				
				if( it.bFolder && (m_iPhase == 0 && m_iPhaseMAX == 0) )
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
							m_iStatDoneFile++;
							_UpdateRes();
							
							it.Acked = true;
						}
						
						if( it.itOwner != null ) 
						{
							//Removing parent folder if RefCount gone to 0...
							//Parent folder is always upper in list!!!
							it = it.itOwner;
							it.Release();
							if( it.RefCount == 0 )
							{
								it.Done = true;
								
								//MUST NOT!!!
								/*
								spFiles.Children.Remove(it.grdOut);
								m_todo.Remove(it);
								m_iActiveToDo--;
								*/
							}
							
							//it.tbTit.Text = it.GetStateTitle(m_bLogItems);
							//SLOW!!!
							/*
							lbItems.ItemsSource = null;
							lbItems.ItemsSource = m_aTodo;
							*/
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
			
			m_iActiveToDo++;
			if( m_iActiveToDo >= m_aTodo.Count ) return false;
			it = m_aTodo[m_iActiveToDo] as RscFtp_UpLoad_SimpleLbItem;
			
			if( it.bFolder )
			{
				if( it.Title == "." )
				{
					if( (m_iPhase == 1 && it.Done) || (m_iPhase == 0 && it.Created) )
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
						
						//Get FTP server's user's current server work dir once perc session...
						if( m_strServerRoot.Length == 0 ) m_strServerRoot = m_ftpc.WorkingDirectory;
						
						m_strSessionFolder = m_strSessionFolderDefault;
						
						string sRemoteFullPath = m_strServerRoot + m_strSessionFolder;
						
						//ROLLED BACK!!!
						//UpLoad-ChkBack-BUG...
						/*
						if( m_sUpFolder.Length > 0 )
						{
							sRemoteFullPath += m_ftpc.BackSlashInPath + m_sUpFolder;
						}
						*/
						
						RscFtpClientCommand cmdRoot = new RscFtpClientCommand(1, "CWD", sRemoteFullPath + m_ftpc.BackSlashInPath );
						
						RscFtpClientCommand cmd = null;
						
						if( m_iPhase == 0 ) cmd = RscFtpClientCommand.CreateFolder(sRemoteFullPath);
				
						RscFtpClientCommand cmdList = RscFtpClientCommand.ListFilesAndFolders( "", m_iActiveToDo);
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
				else
				{
					if( m_strSessionFolder.Length == 0 ) return false;
					
					if( (m_iPhase == 1 && it.Done) || (m_iPhase == 0 && it.Created) )
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
						if( strWrkPath[ 1 ] == ':' )
						{
							//A:\... will be A_\... on remote...
							strWrkPath = strWrkPath.Substring( 0, 1 ) + "_" + strWrkPath.Substring( 2 );
						}
						
						strWrkPath = strWrkPath.Replace("\\", m_ftpc.BackSlashInPath);
						string sRemoteFullPath = m_strServerRoot + m_strSessionFolder + m_ftpc.BackSlashInPath + strWrkPath;
						
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
				
				string strWrkPath = it.GetPath();
				if( strWrkPath[ 1 ] == ':' )
				{
					//A:\... will be A_\... on remote...
					strWrkPath = strWrkPath.Substring( 0, 1 ) + "_" + strWrkPath.Substring( 2 );
				}
						
				strWrkPath = strWrkPath.Replace("\\", m_ftpc.BackSlashInPath);
				string sRemoteFullPath = m_strServerRoot + m_strSessionFolder + m_ftpc.BackSlashInPath + strWrkPath;
				
				// //
				//
				
				bool bExists = false;
				bool bForceDeleteRemote = false;
				
				if( m_iPhase != 1 )
				{
					if( it.itOwner != null )
					{
						if( it.itOwner.ServerData != null )
						{
							
							//DeBug...
							//_Log("", "Remote file check (iPhase=" + m_iPhase.ToString() + "): " + it.GetTitle());
							
							int iIdx = it.itOwner.ServerData.GetIndexByTitle( it.Title );
							if( iIdx >= 0 )
							{
								if( m_bLogItems )
								{
									it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") Remote file exists!";
							
									//SLOW!!!
									/*
									lbItems.ItemsSource = null;
									lbItems.ItemsSource = m_aTodo;
									*/
								}
								
								bExists = true;
								//_Log("", "Item EXISTS (iPhase=" + m_iPhase.ToString() + "): " + it.GetTitle());
								
								//Size check...
								/*
								if( m_iPhaseMAX > 0 )
								{
								*/
								
								RscFtpServerDataItemFileInfo fi = (RscFtpServerDataItemFileInfo) it.itOwner.ServerData.GetItem(iIdx);
								
								long lSize = -1;
								if( fi.m_sSize.Length > 0)
								{
									if( !long.TryParse(fi.m_sSize, out lSize) )
										lSize = -1;
								}
								it.RemoteFileSize = lSize;
								
								if( !m_bOverWriteExisting )
								{
									if( it.FileSize < 0 )
									{
										RscStore store = new RscStore();
										
										System.IO.Stream stream = store.GetReaderStream( it.GetPath() );
										
										it.FileSize = stream.Length;
										
										stream.Close();
									}
									
									//_Log("", "local size: " + it.FileSize.ToString() + " vs. remote size: " + it.RemoteFileSize.ToString());
									
									if( it.FileSize != it.RemoteFileSize )
									{
										bExists = false;
										bForceDeleteRemote = true;
									}
								}
								
								if( m_iPhaseMAX > 0 )
								{
									//it.tbTit.Text = it.GetStateTitle(m_bLogItems);
									
									//SLOW!!!
									/*
									lbItems.ItemsSource = null;
									lbItems.ItemsSource = m_aTodo;
									*/
								}
							}
							/*
							else
							{
								_Log("", "Item ERROR: Not found! (iPhase=" + m_iPhase.ToString() + "): " + it.GetTitle());
							}
							*/
						}
					}
					
					if( m_iPhaseMAX > 0 )
					{
						if( !it.Done )
						{
							if( m_bLogItems )
							{
								it.Details = it.Details + "\r\n(" + m_iPhase.ToString() +
									") The local(" + it.FileSize.ToString() + 
									") vs. remote(" + it.RemoteFileSize.ToString() + ") size check...";
							
								//SLOW!!!
								/*
								lbItems.ItemsSource = null;
								lbItems.ItemsSource = m_aTodo;
								*/
							}
							
							//If yes, upload succeeded!
							it.Done = (it.FileSize == it.RemoteFileSize);
							
							//DEBUG...
							//it.Done = (it.ChkBackFailCount > 1);
							
							if( it.Done )
							{
								m_iStatFailFile--;
							}
							else
							{
								it.ChkBackFailCount++;
								if( it.ChkBackFailCount == 1 ) //Report once...
								{
									m_iStatFailFile++;
									_UpdateRes();
								}
							}
						}
					}	
				}
				
				//
				// //
				//
				
				if( (bExists && (!m_bOverWriteExisting)) || it.Done || (m_iPhase == 1) )
				{
					if( bExists && m_iPhaseMAX == 0 )
					{
						m_iStatSkipFile++;
						_UpdateRes();
						
						it.Acked = true;
					}
					
					//FIX...
					//if( bExists && (!m_bOverWriteExisting) ) it.Done = true;
					if( bExists && (!m_bOverWriteExisting) && (m_iPhaseMAX == 0) ) it.Done = true;
					
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
					RscStore store = new RscStore();
					
					System.IO.Stream stream = store.GetReaderStream( it.GetPath() );
					
					//it.tbTit.Text = it.GetStateTitle(m_bLogItems);
					//SLOW!!!
					/*
					lbItems.ItemsSource = null;
					lbItems.ItemsSource = m_aTodo;
					*/

					it.FileSize = stream.Length;
					
					/*
					MemoryStream ms = new MemoryStream((int) stream.Length);
					stream.CopyTo(ms);
					stream.Close();
					ms.Seek(0, SeekOrigin.Begin);
					*/			
					//RscFtpClientCommand cmd = RscFtpClientCommand.UploadBin(sRemoteFullPath, ms);
					RscFtpClientCommand cmd = RscFtpClientCommand.UploadBin(sRemoteFullPath, null, stream);
					
					RscFtpClientCommand cmdPasv = RscFtpClientCommand.EnterPassiveMode();
					
					cmdPasv.Parent = cmd;
						
					//Vibrated...
					//prsBarData.Visibility = Rsc.Collapsed;
					prsBarData.Value = 0;
					
					if( bExists || bForceDeleteRemote || (m_iPhase == 1) )
					{
						if( !m_bChkBack ) it.Done = true;
						
						if( m_bLogItems )
						{
							it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") Delete + Uploading...";
							
							//SLOW!!!
							/*
							lbItems.ItemsSource = null;
							lbItems.ItemsSource = m_aTodo;
							*/
						}
						
						RscFtpClientCommand cmd2 = RscFtpClientCommand.DeleteFile(sRemoteFullPath);
						
						cmd2.Parent = cmdPasv;
						
						_Log("21", cmd2.ToString() + " + " + "(auto-PASV) " + cmd.ToString());
						
						m_ftpc.SendCommandToServer(cmd2);
					}
					else
					{
						if( !m_bChkBack ) it.Done = true;
						
						if( m_bLogItems )
						{
							it.Details = it.Details + "\r\n(" + m_iPhase.ToString() + ") Uploading...";
							
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
				
				//
				// //
			}
			
			return true;
		}
		
		private void btnStartStop_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_strSessionFolder.Length == 0 )
			{
				prsBar.Visibility = Rsc.Visible;
				prsBar.IsIndeterminate = false;
				prsBar.Minimum = 0;
				prsBar.Value = 0;
				prsBar.Maximum = m_aTodo.Count - 1;
				
				btnStartStop.Content = "Cancel";
				btnStartStop.Background = new SolidColorBrush(Colors.Red);
			
				_UpdateRes(true);
				
				bool bTmp = false;
				RaiseNextFtpClientCommand(out bTmp);
			}
			else
			{
				m_bUserStop = true;
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
					
					ListFiles();
					
					/*
					prsBar.Visibility = Rsc.Collapsed;
					prsBarData.Visibility = Rsc.Collapsed;
					
					btnStartStop.Visibility = Rsc.Visible;
					btnStartStop.Content = "Start";
					btnStartStop.Background = new SolidColorBrush(Colors.Green);
					
					string sAddr = m_ftpc.UserName;
					if( sAddr.Length > 0 ) sAddr += "@";
					sAddr += m_ftpc.IPAddress + ":" + m_ftpc.Port.ToString();
					svrAddr.Text = sAddr;
					svrTit.Text =  m_ftpc.ServerTitle;
					svrRoot.Text = m_strSessionFolderDefault;
					
					_UpdateRes(true);
					*/
				}
				else if( m_bUserStop )
				{
					
					RscFtp_UpLoad_SimpleLbItem it;
					
					m_aTodo.Clear();
							
					prsBar.Visibility = Rsc.Collapsed;
					prsBarData.Visibility = Rsc.Collapsed;
					btnStartStop.Visibility = Rsc.Collapsed;
					
					it = new RscFtp_UpLoad_SimpleLbItem( m_aTodo );	
					it.bFile = false;
					it.bFolder = false;
					it.bWalked = false;
					it.strOwner = "USER stopped operation! Some items were not uploaded (or checked) to server-folder: '" +
						m_strServerRoot + m_strSessionFolder + "'!";
					it.strName = "FAIL!";
					it.CustomBackColor = Colors.Red;
					it.CustomForeColor = Colors.White;
					
					it.m_bLogItems = m_bLogItems;
					m_aTodo.Add(it);
					
					m_bEnded = true;
				}
				else if( m_iActiveToDo < m_aTodo.Count )
				{
					if( RaiseNextFtpClientCommand(out bNoop, true) )
					{
						prsBar.Value += 1;
					}
					else
					{
						if( (m_iPhase == 0 && m_bChkBack) && (m_iPhaseMAX == 0) )
						{
							m_iPhase++;
							m_iPhaseMAX++;
							
							_UpdateRes();
							
							prsBar.Maximum += (m_aTodo.Count - 1);
							
							m_iActiveToDo = -1;
							RaiseNextFtpClientCommand(out bNoop, true);
						}
						else if( m_iPhase == 1 )
						{
							m_iPhase = 0;
							m_iPhaseCYC++;
							
							_UpdateRes();
							
							prsBar.Maximum += (m_aTodo.Count - 1);
							
							m_iActiveToDo = -1;
							RaiseNextFtpClientCommand(out bNoop, true);
						}
						else
						{
							bool bDone = true;
							
							RscFtp_UpLoad_SimpleLbItem it;
							int idx = -1;
							for(;;)
							{
								idx++;
								if( idx >= m_aTodo.Count ) break;
								it = m_aTodo[ idx ] as RscFtp_UpLoad_SimpleLbItem;
								
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
								if( (m_iPhase == 0 && m_bChkBack) && (m_iPhaseMAX > 0) )
								{
									m_iPhase++;
									
									_UpdateRes();
									
									prsBar.Maximum += (m_aTodo.Count - 1);
									
									m_iActiveToDo = -1;
									RaiseNextFtpClientCommand(out bNoop, true);
								}
								else
								{
									prsBar.Visibility = Rsc.Collapsed;
									prsBarData.Visibility = Rsc.Collapsed;
									btnStartStop.Visibility = Rsc.Collapsed;
									
									it = new RscFtp_UpLoad_SimpleLbItem( m_aTodo );	
									it.bFile = false;
									it.bFolder = false;
									it.bWalked = false;
									it.strOwner = "Some items were not uploaded (or checked) to server-folder: '" +
										m_strServerRoot + m_strSessionFolder + "'!";
									it.strName = "FAIL!";
									it.CustomBackColor = Colors.Red;
									it.CustomForeColor = Colors.White;
									
									it.m_bLogItems = m_bLogItems;
									m_aTodo.Add(it);
									
									m_bEnded = true;
								}
							}
							else
							{
								prsBar.Visibility = Rsc.Collapsed;
								prsBarData.Visibility = Rsc.Collapsed;
								btnStartStop.Visibility = Rsc.Collapsed;
								
								it = new RscFtp_UpLoad_SimpleLbItem( m_aTodo );	
								it.bFile = false;
								it.bFolder = false;
								it.bWalked = false;
								it.strOwner = "All files and folders has been uploaded (or checked) to server-folder: '" +
									m_strServerRoot + m_strSessionFolder + "'!";
								it.strName = "Success!";
								it.CustomBackColor = Colors.Green;
								it.CustomForeColor = Colors.White;
								
								it.m_bLogItems = m_bLogItems;
								m_aTodo.Add(it);
								
								m_bEnded = true;
							}
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

						int iCount;
						RscFtpServerDataItemFileInfo fi;
						
						iCount = e.ServerData.Count;
						
						//Store file+folder list for folder item...
						if( m_aTodo.Count > 0 )
						{
							int iIdx = (int) e.ClientCommand.Tag;
							RscFtp_UpLoad_SimpleLbItem it = m_aTodo[iIdx] as RscFtp_UpLoad_SimpleLbItem;
							it.ServerData = e.ServerData;
							if( m_bLogItems )
							{
								it.Details = it.Details + "\r\n" + "Direcory Listing received"
									+ " (cmd.Arg1=" + e.ClientCommand.Arg1 + ", Cnt=" + iCount.ToString() + ").";
							
								//SLOW!!!
								/*
								lbItems.ItemsSource = null;
								lbItems.ItemsSource = m_aTodo;
								*/
							}
						}
						
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
								
								//DEBUG...
								if( m_aTodo.Count > 0 )
								{
									int iIdx = (int) e.ClientCommand.Tag;
									RscFtp_UpLoad_SimpleLbItem it = m_aTodo[iIdx] as RscFtp_UpLoad_SimpleLbItem;
									if( m_bLogItems )
									{
										it.Details = it.Details + "\r\n"
											+ fi.GetItemTitle( );
							
										//SLOW!!!
										/*
										lbItems.ItemsSource = null;
										lbItems.ItemsSource = m_aTodo;
										*/
									}
								}
							}
						
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

		private void ListFiles()
		{
			
			m_aTodo.Clear();
			m_iActiveToDo = -1;
			
			//string sTRACE = "";
						
			string [] asUpFolder = m_sUpFolder.Split('\\');
			RscFtp_UpLoad_SimpleLbItem itPrev = null;
			//foreach( string sFolder in asUpFolder )
			for( int i = -1; i < asUpFolder.Length; i++ )
			{
				string sFolder;
				if( i == -1 )
				{
					sFolder = "";
				}
				else
				{
					sFolder = asUpFolder[ i ];
					if( sFolder == "" && i == asUpFolder.Length - 1 )
						break;
				}
				
				if( itPrev != null )
				{
					itPrev.bWalked = true; //Do not list subfolders and files...
				}
			
				RscFtp_UpLoad_SimpleLbItem it = new RscFtp_UpLoad_SimpleLbItem( m_aTodo );
				it.bFile = false;
				it.bFolder = true;
				it.bWalked = false;
				
				if( itPrev != null )
					it.strOwner = itPrev.GetPath();
				else
					it.strOwner = "";
				
				it.strName = sFolder;
				
				it.m_bLogItems = m_bLogItems;
				m_aTodo.Add(it);
				
				itPrev = it;
				
				//sTRACE += it.GetPath() + "\r\n";
			}
			
			//m_AppFrame.TRACE = sTRACE;
			
			_SetStatusText( "Listing items..." );
			m_AppFrame.StartTimer( "list files", LayoutRoot, 1, m_aTodo.Count - 1, true, m_aTodo.Count - 1 );			
		}
		
		private void m_AppFrame_OnTimer(object sender, RscAppFrameTimerEventArgs e)
		{
			switch( e.Reason )
			{
				
				case "list files_Cancel" :
				{
					m_aTodo.Clear();
					m_iActiveToDo = -1;
					
					_SetStatusText( "User canceled operation!", StatusColoring.Error );
					break;
				}
				
				case "list files" :
				{
					
					_RefreshStatusText();
			
					RscFtp_UpLoad_SimpleLbItem it;
					RscFtp_UpLoad_SimpleLbItem itCurrent;
					
					RscStore store = new RscStore();
					
					itCurrent = m_aTodo[ e.Pos ] as RscFtp_UpLoad_SimpleLbItem;

					if( !itCurrent.bWalked )
					{
		
						string[] fldrs = store.GetFolderNames( itCurrent.GetPath(), "*.*", m_bUpHidden );
						foreach(string node in fldrs)
						{
							it = new RscFtp_UpLoad_SimpleLbItem( m_aTodo );
							
							it.bFile = false;
							it.bFolder = true;
							it.bWalked = false;
							
							it.strOwner = itCurrent.GetPath();
							it.strName = node;
							
							e.Max++;
							it.m_bLogItems = m_bLogItems;
							m_aTodo.Add(it);
							
						}
						
						itCurrent.bWalked = true;
						
						if( e.Pos == e.Max )
						{
							e.Pos = 0;
						}	
					}
					else
					{
						if( m_sUpFolder.Length == 0 || itCurrent.GetPath().Length >= m_sUpFolder.Length )
						{
							string[] fles = store.GetFileNames( itCurrent.GetPath(), "*.*", m_bUpHidden );
							foreach(string node in fles)
							{					
								it = new RscFtp_UpLoad_SimpleLbItem( m_aTodo );
								
								it.bFile = true;
								it.bFolder = false;
								it.bWalked = false;
								
								it.strOwner = itCurrent.GetPath();
								it.strName = node;
								
								it.itOwner = itCurrent;
								itCurrent.AddRef();
								
								it.m_bLogItems = m_bLogItems;
								m_aTodo.Add(it);
							}
						}
						
						if( e.Pos == e.Max )
						{
							//ReQuery...
							//SLOW!!!
							/*
							lbItems.ItemsSource = null;
							lbItems.ItemsSource = m_aTodo;
							*/
			
							m_iStatAllFiles = 0;
							m_iStatAllFolders = 0;
							foreach(RscFtp_UpLoad_SimpleLbItem ti in m_aTodo)
							{
								if( ti.bFile )
									m_iStatAllFiles++;
								else if( ti.bFolder )
									m_iStatAllFolders++;
							}
							
							prsBar.Visibility = Rsc.Collapsed;
							prsBarData.Visibility = Rsc.Collapsed;
							
							btnStartStop.Visibility = Rsc.Visible;
							btnStartStop.Content = "Start";
							btnStartStop.Background = new SolidColorBrush(Colors.Green);
							
							string sAddr = m_ftpc.UserName;
							if( sAddr.Length > 0 ) sAddr += "@";
							sAddr += m_ftpc.IPAddress + ":" + m_ftpc.Port.ToString();
							svrAddr.Text = sAddr;
							svrTit.Text =  m_ftpc.ServerTitle;
							svrRoot.Text = m_strSessionFolderDefault;
							
							_UpdateRes(true);
							
							//AUTO CLICK...
							m_AppFrame.AutoClick( btnStartStop, new System.Windows.RoutedEventHandler(btnStartStop_Click) );
						}
					}
					
					break;
				}
				
			}
		}

		private void m_btnUpFldrDelete_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_txtUpFldr.Text = "";
		}

		private void m_btnUpFldrBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_tmrBrowse.Start();
		}

		private void m_tmrBrowse_Tick(object sender, System.EventArgs e)
		{
			m_tmrBrowse.Stop();

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "UpFldrPath" );
			appInput.SetFlag( 0, "local folder path to upload" );
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, m_txtUpFldr.Text );
			appInput.SetInput( "RscDlg_FolderInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csDlgsAssy ) );
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
					/*+ " (" + m_ftpc.StatUpCmd.Seconds.ToString() + ")"*/
				+ " |"
				+ " usr: " + RscUtils.toMBstr(m_ftpc.StatUpDat.ByteCount)
					/*+ " (" + m_ftpc.StatUpDat.Seconds.ToString() + ")"*/;
			
			sPlus += "\r\n" + "DN <-"
				+ " sys: " + RscUtils.toMBstr(m_ftpc.StatDnCmd.ByteCount)
					/*+ " (" + m_ftpc.StatDnCmd.Seconds.ToString() + ")"*/
				+ " |"
				+ " usr: " + RscUtils.toMBstr(m_ftpc.StatDnDat.ByteCount)
					/*+ " (" + m_ftpc.StatDnDat.Seconds.ToString() + ")"*/;
			
			bool bSpeed = false;
			long lUp = m_ftpc.BytesPerSecUp;
			//long lDn = m_ftpc.BytesPerSecDn;
			if( lUp > 0 /*|| lDn > 0*/ )
			{
				bSpeed = true;
				
				sPlus += "\r\nSpeed ";
				/*if( lUp > 0 )
				{*/
					sPlus += "UP: " + RscUtils.toMBstr(lUp) + "/s";
				/*}
				if( lDn > 0 )
				{
					if( lUp > 0 ) sPlus += " | ";
					sPlus += "DN: " + RscUtils.toMBstr(lDn) + "/s";
				}*/
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
		
		private void SimpleLbItem_BtnCust1_Click(object sender, System.Windows.RoutedEventArgs e)
		{
		}
		
		private void SimpleLbItem_BtnCust2_Click(object sender, System.Windows.RoutedEventArgs e)
		{
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
