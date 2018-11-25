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
using System.Windows.Media.Imaging;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;

using Ressive.Utils;
using Ressive.Store;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

using Ressive.FTP;

namespace RscFtpClients
{
	
    public partial class RscFtp_ExplorerV11 : PhoneApplicationPage
    {
		
		const string csViewersAssy = "Lib_RscViewers";
		const string csDlgsAssy = "Lib_RscIPgC_Dlgs";
		
		RscAppFrame m_AppFrame;
	
		RscPageArgsRetManager m_AppArgs;
		
		RscFtpClient m_ftpc = null;
		
		TextBoxDenieEdit m_txtRemotePath;
		
		string NationalChrsToTestFileNames = "";
		
		TextBoxDenieEdit m_txtLastFile;
		string m_sLastFile = "";
		bool m_bLastFileIsFolder = false;
		long m_lLastFileSize = -1;
		RscIconButton m_btnRemoteEnter;
		RscIconButton m_btnDownloadMem;
		RscIconButton m_btnDownloadFile;
		RscIconButton m_btnRemoteDel;
		RscIconButton m_btnRemoteDateTime;
		RscIconButton m_btnRemoteList;
		
		DispatcherTimer m_tmrSend;
		string m_sSendID = "";
		
		MyLogItemList m_logs = new MyLogItemList();
		
		DispatcherTimer m_tmrFolder;
		
		string sSAVE_Status = "";
		StatusColoring scSAVE = StatusColoring.Normal;
		
		DispatcherTimer m_tmrInput;
		
        public RscFtp_ExplorerV11()
        {
            InitializeComponent();
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "FTP Explorer 1.1", "Images/IcoSm001_FtpTest.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			// ///////////////
			imgInput.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Open.jpg");
			imgIpUpIco.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Inc.jpg");
			imgIpDnIco.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Dec.jpg");
			
			m_txtRemotePath = new TextBoxDenieEdit(true, true, pathPanel, Grid.ColumnProperty, 0);
			m_txtRemotePath.Background = new SolidColorBrush(Colors.LightGray);
			m_txtRemotePath.Foreground = new SolidColorBrush(Colors.Black);
			m_txtRemotePath.FontSize = 16;
			m_txtRemotePath.MarginOffset = new Thickness( 0, 10, 0, 10 );
			m_txtRemotePath.Text = "";
			
			m_txtLastFile = new TextBoxDenieEdit(true, true, lastFilePanel, Grid.ColumnProperty, 0);
			m_txtLastFile.Background = new SolidColorBrush(Colors.LightGray);
			m_txtLastFile.Foreground = new SolidColorBrush(Colors.Black);
			m_txtLastFile.FontSize = 16;
			m_txtLastFile.MarginOffset = new Thickness( 0, 0, 12, 0 );
			m_txtLastFile.Text = "";
			
			m_btnRemoteEnter = new RscIconButton(lastFilePanel, Grid.ColumnProperty, 1, 36, 36, Rsc.Visible);
			m_btnRemoteEnter.Image.Source = m_AppFrame.Theme.GetImage("Images/BtnDrk001_(dir).jpg");
			m_btnRemoteEnter.MarginOffset = new Thickness( 0, 0, 10, 0 );
			m_btnRemoteEnter.Click += new System.Windows.RoutedEventHandler(m_btnRemoteEnter_Click);
			
			m_btnDownloadMem = new RscIconButton(lastFilePanel, Grid.ColumnProperty, 2, 36, 36, Rsc.Visible, 0, 0, "Down\r\nMem");
			m_btnDownloadMem.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			m_btnDownloadMem.MarginOffset = new Thickness( 0, 0, 10, 0 );
			m_btnDownloadMem.Click += new System.Windows.RoutedEventHandler(m_btnDownloadMem_Click);
			
			m_btnDownloadFile = new RscIconButton(lastFilePanel, Grid.ColumnProperty, 3, 36, 36, Rsc.Visible, 0, 0, "Down\r\nFile");
			m_btnDownloadFile.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			m_btnDownloadFile.MarginOffset = new Thickness( 0, 0, 10, 0 );
			m_btnDownloadFile.Click += new System.Windows.RoutedEventHandler(m_btnDownloadFile_Click);
			
			m_btnRemoteDel = new RscIconButton(lastFilePanel, Grid.ColumnProperty, 4, 36, 36, Rsc.Visible);
			m_btnRemoteDel.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Delete.jpg");
			m_btnRemoteDel.MarginOffset = new Thickness( 0, 0, 10, 0 );
			m_btnRemoteDel.Click += new System.Windows.RoutedEventHandler(m_btnRemoteDel_Click);
			
			m_btnRemoteDateTime = new RscIconButton(lastFilePanel, Grid.ColumnProperty, 5, 36, 36, Rsc.Visible, 0, 0, "Date\r\nTime");
			m_btnRemoteDateTime.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			m_btnRemoteDateTime.MarginOffset = new Thickness( 0, 0, 10, 0 );
			m_btnRemoteDateTime.Click += new System.Windows.RoutedEventHandler(m_btnRemoteDateTime_Click);
			
			m_btnRemoteList = new RscIconButton(lastFilePanel, Grid.ColumnProperty, 6, 36, 36, Rsc.Visible, 0, 0, "List");
			m_btnRemoteList.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			m_btnRemoteList.MarginOffset = new Thickness( 0, 0, 10, 0 );
			m_btnRemoteList.Click += new System.Windows.RoutedEventHandler(m_btnRemoteList_Click);
			
			btnNop.FontSize = 14;
			btnNopSup.FontSize = 14;
			btnMkD.Content = "Mk Cnst Dir";
			btnMkD.FontSize = 14;
			btnPasv.FontSize = 14;
			btnList.FontSize = 14;
			btnSendTxt.Content = "Snd Cnst Txt";
			btnSendTxt.FontSize = 14;
			btnSendJpg.Content = "Snd Cnst Jpg";
			btnSendJpg.FontSize = 14;
			btnSendMem.Content = "Snd MEM Fle...";
			btnSendMem.FontSize = 14;
			btnSendFile.Content = "Snd FLE Fle...";
			btnSendFile.FontSize = 14;
			btnAddFolder.Content = "Mk Dir...";
			btnAddFolder.FontSize = 14;
			
			m_tmrSend = new DispatcherTimer();
			m_tmrSend.Interval = new TimeSpan(500);
			m_tmrSend.Tick += new System.EventHandler(m_tmrSend_Tick);
			
			m_AppArgs = new RscPageArgsRetManager();
			
			txSvrIP.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_ExplorerV11", "LastSvrIP", "192.168.0.0" );
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
			
			m_tmrFolder = new DispatcherTimer();
			m_tmrFolder.Interval = new TimeSpan(500);
			m_tmrFolder.Tick += new System.EventHandler(m_tmrFolder_Tick);
			
			m_tmrInput = new DispatcherTimer();
			m_tmrInput.Interval = new TimeSpan(500);
			m_tmrInput.Tick += new System.EventHandler(m_tmrInput_Tick);
			
			m_logs.ListBoxAsteriskWidth = 100;
			lbLogs.ItemsSource = m_logs;
			lbLogs.SizeChanged += new System.Windows.SizeChangedEventHandler(lbLogs_SizeChanged);
			
			this.Loaded += new System.Windows.RoutedEventHandler(RscFtpTestV10_Loaded);
 			this.Unloaded += new System.Windows.RoutedEventHandler(RscFtpTestV10_Unloaded);
        }

		private void RscFtpTestV10_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			m_AppFrame.SetNoSleep( true );
		}

		private void RscFtpTestV10_Unloaded(object sender, System.Windows.RoutedEventArgs e)
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
						
						case "txNewFolder" :
						{
							if( appOutput.GetFlag(0) == "Ok" )
							{
								m_bLastFileIsFolder = true;
								string sFile = appOutput.GetData(0);
								SetLastFile( sFile, true );
								
								RscFtpClientCommand cmd = RscFtpClientCommand.CreateFolder(sFile);
								
								_Log("21", cmd.ToString());
								
								m_ftpc.SendCommandToServer(cmd);
							}
							break;
						}
						
						case "BrowseForFile_MEM" :
						{
							if( appOutput.GetFlag(0) == "Ok" )
							{
								
								string sFilePath = appOutput.GetData(0);
								
								string sFile = RscStore.FileOfPath( sFilePath );
								
								RscStore store = new RscStore();
								
								System.IO.Stream stream = store.GetReaderStream( sFilePath );
								
								SetLastFile( sFile, false, stream.Length );
								System.IO.MemoryStream ms = new System.IO.MemoryStream((int) stream.Length);
								stream.CopyTo(ms);
								stream.Close();
								ms.Seek(0, System.IO.SeekOrigin.Begin);
			
								RscFtpClientCommand cmd = RscFtpClientCommand.UploadBin(sFile, ms);
								
								_SendAutoPASV(cmd);

							}
							else
							{
								//NOP...
							}
							
							m_AppArgs.Vipe();
							
							break;
						}
						
						case "BrowseForFile_FLE" :
						{
							if( appOutput.GetFlag(0) == "Ok" )
							{
								
								string sFilePath = appOutput.GetData(0);
								
								string sFile = RscStore.FileOfPath( sFilePath );
								
								RscStore store = new RscStore();
								
								System.IO.Stream stream = store.GetReaderStream( sFilePath );
								
								SetLastFile( sFile, false, stream.Length );
								
								/*
								MemoryStream ms = new MemoryStream((int) stream.Length);
								stream.CopyTo(ms);
								stream.Close();
								ms.Seek(0, SeekOrigin.Begin);
								*/
			
								RscFtpClientCommand cmd = RscFtpClientCommand.UploadBin(sFile, null, stream);
								
								_SendAutoPASV(cmd);

							}
							else
							{
								//NOP...
							}
							
							m_AppArgs.Vipe();

							break;
						}
							
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
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
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
			
			m_AppFrame.ShowButtonNext( false );
			
			parPanel.Visibility = Rsc.Collapsed;
			
			if( chbAutoLogOn.IsChecked.Value )
			{
				btnLogUsr.Visibility = Rsc.Collapsed;
				btnLogPw.Visibility = Rsc.Collapsed;
			}
			
			if( chbAutoPasv.IsChecked.Value )
			{
				btnPasv.Visibility = Rsc.Collapsed;
			}
			
			//lastFilePanel.Visibility = Rsc.Collapsed; //Rsc.Visible;
			SetLastFile( "", false );
			
			pathPanel.Visibility = Rsc.Visible;
			custCmdPanel.Visibility = Rsc.Visible;
			logPanel.Visibility = Rsc.Visible;
			
			svrAddr.Text = " ";
			svrTit.Text = " ";
			svrPanel.Visibility = Rsc.Visible;
				
			//WP81 FIX
			//m_ftpc.SetFastConnection( chbFastConn.IsChecked.Value, chbFastConnEx.IsChecked.Value );
			
			NationalChrsToTestFileNames = "";
			if( chbNatFsNames.IsChecked.Value )
			{
				NationalChrsToTestFileNames = " (ÁáÉéÍíÓóÖöŐőÚúÜüŰű)";
			}
			
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
			m_ftpc.AutoLogOn = chbAutoLogOn.IsChecked.Value;
			m_ftpc.AutoPassiveMode = chbAutoPasv.IsChecked.Value;
			
			m_ftpc.Connect();
		}
		
		private void LoadFromReg( )
		{
			if( txSvrIP.Text.Length == 0 ) return;
			
			txSvrPort.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_ExplorerV11" + "\\" + txSvrIP.Text,
				"Port", 2221.ToString( ) );
			
			txUsr.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_ExplorerV11" + "\\" + txSvrIP.Text,
				"Usr", "usr" );
			
			//Do not store...
			txPwd.Text = "";
			
			//WP81 FIX
			/*
			chbFastConn.IsChecked = RscRegistry.ReadBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_ExplorerV11" + "\\" + txSvrIP.Text,
				"FastConn", false );
			
			chbFastConnEx.IsChecked = RscRegistry.ReadBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_ExplorerV11" + "\\" + txSvrIP.Text,
				"FastConnEx", false );
			*/
		}
		
		private void SaveToReg()
		{
			RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_ExplorerV11", "LastSvrIP", txSvrIP.Text );			
			
			RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_ExplorerV11" + "\\" + txSvrIP.Text,
				"Port", txSvrPort.Text );
			
			RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_ExplorerV11" + "\\" + txSvrIP.Text,
				"Usr", txUsr.Text );
			
			//Do not store...
			//txPwd.Text = "";
			
			//WP81 FIX
			/*
			RscRegistry.WriteBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_ExplorerV11" + "\\" + txSvrIP.Text,
				"FastConn", chbFastConn.IsChecked.Value );
			
			RscRegistry.WriteBool( HKEY.HKEY_CURRENT_USER,
				"Software\\Ressive.Hu\\RscFtp_ExplorerV11" + "\\" + txSvrIP.Text,
				"FastConnEx", chbFastConnEx.IsChecked.Value );
			*/
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( parPanel.Visibility == Rsc.Collapsed )
			{
				_Log("21", "Disconnect...");
				
				m_ftpc.CloseAllSockets();
				
				svrAddr.Text = " ";
				svrTit.Text = " ";
				svrPanel.Visibility = Rsc.Collapsed;
				
				logPanel.Visibility = Rsc.Collapsed;
				custCmdPanel.Visibility = Rsc.Collapsed;
				pathPanel.Visibility = Rsc.Collapsed;
				
				//lastFilePanel.Visibility = Rsc.Collapsed;
				SetLastFile( "", false );

				m_logs.Clear();
				
				_SetStatusText( );
				
				parPanel.Visibility = Rsc.Visible;
				
				m_AppFrame.ShowButtonNext( true );
				
				e.Cancel = true;
			}
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
					
			if( svrPanel.Visibility == Rsc.Collapsed )
			{
				
				svrPanel.Visibility = Rsc.Visible;
			}
			
			if( svrAddr.Text.ToString().Trim().Length == 0 )
			{
				string sAddr = m_ftpc.UserName;
				if( sAddr.Length > 0 ) sAddr += "@";
				sAddr += m_ftpc.IPAddress + ":" + m_ftpc.Port.ToString();
				svrAddr.Text = sAddr;
			}
			if( svrTit.Text.ToString().Trim().Length == 0 )
			{
				svrTit.Text =  m_ftpc.ServerTitle;
			}
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
							}
						
						}
						
						break;
					}
					
					case "RETR" :
					{
						prsBarData.Visibility = Rsc.Collapsed;
						
						_Log("20", e.ClientCommand.Command + " returned " + 
							e.ClientCommand.DataSize.ToString() + " bytes.", false, true);
						
						if( e.ClientCommand.HasMemStream )
						{
							string sPath = e.ClientCommand.Arg1;
							int iPos = sPath.LastIndexOf(m_ftpc.BackSlashInPath);
							string sFn = sPath;
							if( iPos >= 0 ) sFn = sPath.Substring(iPos + m_ftpc.BackSlashInPath.Length);
							
							_Log("20", e.ClientCommand.Command + " saving local file '" + 
								sFn + "'.", false, true);
							
							RscStore store = new RscStore();
							
							string sLocalPath = "A:\\FTP";
							if( !store.FolderExists( sLocalPath ) ) store.CreateFolder( sLocalPath );
							sLocalPath += "\\" + sFn;
							if( store.FileExists( sLocalPath ) ) store.DeleteFile( sLocalPath );
							
							System.IO.Stream stream = store.CreateFile(sLocalPath);
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
			 * On Wm6.5 end of sent data has been lost!
			 * Possible FIX!!!
			 *
			 * THIS IS!!!
			 */
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

		private void m_ftpc_CommandDoneAsync(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			this.Dispatcher.BeginInvoke(() => { m_ftpc_CommandDoneSYNC( sender, e ); });
		}
		private void m_ftpc_CommandDoneSYNC(object sender, Ressive.FTP.RscFtpCommandEventArgs e)
		{
			m_txtRemotePath.Text = m_ftpc.WorkingDirectory;
			
			string sLog = "";
			if( e.ClientCommand != null )
			{
				sLog = "DONE! - " + e.ClientCommand.ToString() + " ("
					+ e.ClientCommand.ResponseCountArrived.ToString( ) + " / "+ e.ClientCommand.ResponseCount.ToString( ) + ")";
			}
			
			//Last command executed (with error / success)...
			_Log("", sLog, false);
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
				
		private void btnSnd_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sCmd = txCmd.Text.ToString( );
			if( sCmd.Length == 0 ) return;
			txCmd.Text = "";
			
			RscFtpClientCommand cmd = new RscFtpClientCommand(1, sCmd);
			
			_Log("21", "custom command: " + cmd.ToString());
			
			m_ftpc.SendCommandToServer(cmd);
		}
				
		private void btnCwdBack_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sPath = m_txtRemotePath.Text;
			if( sPath.Length == 0 ) return;
			
			sPath = sPath.Substring(0, sPath.Length - 1);
			if( sPath.Length == 0 ) return;
			
			int iPos = sPath.LastIndexOf(m_ftpc.BackSlashInPath);
			if( iPos < 0 ) return;
			
			SetLastFile( "", false );
			
			sPath = sPath.Substring(0, iPos + 1);
			
			RscFtpClientCommand cmd = new RscFtpClientCommand(1, "CWD", sPath );
			
			_Log("21", cmd.ToString() );
			
			m_ftpc.SendCommandToServer(cmd);
		}
				
		private void btnLogUsr_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			RscFtpClientCommand cmd = RscFtpClientCommand.LogInUser(m_ftpc.UserName);
			
			_Log("21", cmd.ToString());
			
			m_ftpc.SendCommandToServer(cmd);
		}
				
		private void btnLogPw_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			RscFtpClientCommand cmd = RscFtpClientCommand.LogInPassWord(m_ftpc.PassWord);
			
			_Log("21", cmd.ToString());
			
			m_ftpc.SendCommandToServer(cmd);
		}
				
		private void btnPasv_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			RscFtpClientCommand cmd = RscFtpClientCommand.EnterPassiveMode();
			
			_Log("21", cmd.ToString());
			
			m_ftpc.SendCommandToServer(cmd);
		}
				
		private void btnNoop_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			RscFtpClientCommand cmd = RscFtpClientCommand.NOOP();
			
			_Log("21", cmd.ToString());
			
			m_ftpc.SendCommandToServer(cmd);
		}
				
		private void btnNoopSup_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			RscFtpClientCommand cmd = RscFtpClientCommand.NOOP(true);
			
			//_Log("21", "NOOP - with supressing (200) Ok response");
			_Log("", "");
			
			m_ftpc.SendCommandToServer(cmd);
		}
				
		private void btnMkD_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DateTime dNow = DateTime.Now;
			
			m_bLastFileIsFolder = true;
			string sFile = dNow.Year.ToString() +
					"_" + RscUtils.pad60(dNow.Month) + "_" +
					RscUtils.pad60(dNow.Day) + "_" + RscUtils.pad60(dNow.Hour) +
					"_" + RscUtils.pad60(dNow.Minute) + "_" +
					RscUtils.pad60(dNow.Second) + NationalChrsToTestFileNames;
			SetLastFile( sFile, true );
			
			RscFtpClientCommand cmd = RscFtpClientCommand.CreateFolder(sFile);
			
			_Log("21", cmd.ToString());
			
			m_ftpc.SendCommandToServer(cmd);
		}
				
		private void btnList_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			RscFtpClientCommand cmd = RscFtpClientCommand.ListFilesAndFolders();
			
			_SendAutoPASV(cmd);
		}
				
		private void btnSendTxt_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DateTime dNow = DateTime.Now;
			
			m_bLastFileIsFolder = false;
			string sFile = dNow.Year.ToString() +
					"_" + RscUtils.pad60(dNow.Month) + "_" +
					RscUtils.pad60(dNow.Day) + "_" + RscUtils.pad60(dNow.Hour) +
					"_" + RscUtils.pad60(dNow.Minute) + "_" +
					RscUtils.pad60(dNow.Second) + NationalChrsToTestFileNames + ".txt";
			SetLastFile( sFile, false );
			
			string sTxt = dNow.ToLongDateString( ) +
				"\r\n" + dNow.ToLongTimeString( ) +
				"\r\n" + "Áá Éé Íí Óó Öö Őő Úú Üü Űű";

			RscFtpClientCommand cmd = RscFtpClientCommand.UploadTxt(sFile, sTxt);
			
			_SendAutoPASV(cmd);
		}
				
		private void btnSendJpg_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DateTime dNow = DateTime.Now;
			
			m_bLastFileIsFolder = false;
			string sFile = dNow.Year.ToString() +
					"_" + RscUtils.pad60(dNow.Month) + "_" +
					RscUtils.pad60(dNow.Day) + "_" + RscUtils.pad60(dNow.Hour) +
					"_" + RscUtils.pad60(dNow.Minute) + "_" +
					RscUtils.pad60(dNow.Second) + NationalChrsToTestFileNames + ".jpg";
			SetLastFile( sFile, false );
			
			WriteableBitmap wbmp = new WriteableBitmap(ContentPanel,
				new System.Windows.Media.MatrixTransform());
			
			System.IO.MemoryStream ms = new System.IO.MemoryStream(4096);
			
			System.Windows.Media.Imaging.
			Extensions.SaveJpeg(wbmp, ms,
				wbmp.PixelWidth, wbmp.PixelHeight,
				0, 100);
			ms.Seek(0, System.IO.SeekOrigin.Begin);
			
			RscFtpClientCommand cmd = RscFtpClientCommand.UploadBin(sFile, ms);
			
			_SendAutoPASV(cmd);
		}
		
		private void btnSendMem_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_sSendID = "BrowseForFile_MEM";
			m_tmrSend.Start();
		}
		
		private void btnSendFile_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_sSendID = "BrowseForFile_FLE";
			m_tmrSend.Start();
		}

		private void m_tmrSend_Tick(object sender, System.EventArgs e)
		{
			m_tmrSend.Stop();
			
			string strCallerAppTitle = m_AppFrame.AppTitle;
			string strCallerAppIconRes = m_AppFrame.AppIconRes;

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				strCallerAppTitle, strCallerAppIconRes, m_sSendID );
			
			appInput.SetFlag( 0, "*" ); //FileType Filter (if empty, folder path browsed)
			
			/*
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, m_Root.FullPath );
			*/
			appInput.SetInput( "RscViewer_FsV12" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csViewersAssy ) );
		}
		
		private void btnAddFolder_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_tmrFolder.Start();
		}

		private void m_tmrFolder_Tick(object sender, System.EventArgs e)
		{
			m_tmrFolder.Stop();

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "txNewFolder" );
			appInput.SetFlag( 0, "new directory name" );
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, "" );
			appInput.SetInput( "RscDlg_TxtInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csDlgsAssy ) );
		}
		
		//
		// //
		//
		
		private void _SendAutoPASV(RscFtpClientCommand cmd)
		{
			if( m_ftpc.AutoPassiveMode )
			{
				RscFtpClientCommand cmdPasv = RscFtpClientCommand.EnterPassiveMode();
				
				cmdPasv.Parent = cmd;
				
				_Log("21", "(auto-PASV) " + cmd.ToString());
			
				m_ftpc.SendCommandToServer(cmdPasv);
			}
			else
			{
				_Log("21", cmd.ToString());
			
				m_ftpc.SendCommandToServer(cmd);
			}
		}
		
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
	
			_RefreshStatusText();
		}
		
		private void btnLogItem_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn = (Button) sender;
			if( btn.Tag != null )
			{
				MyLogItem li = (MyLogItem) btn.Tag;
				if( li != null )
				{
					if( li.oTag != null )
					{
						RscFtpServerDataItemFileInfo fi = (RscFtpServerDataItemFileInfo) li.oTag;
						
						if( fi.IsFolder )
						{
							if( m_ftpc.WorkingDirectory.Length == 0 ) return;
							
							SetLastFile( fi.GetItemTitle(), true );
						}
						else
						{									
							SetLastFile( fi.GetItemTitle(), false, long.Parse(fi.m_sSize) );
						}
						
						return;
					}
				}
			}
			
			SetLastFile( "", false );
		}
		
		//
		// //
		//
		
		private void SetLastFile( string sLastFile, bool bIsFolder, long lSize = -1 )
		{
			m_sLastFile = sLastFile;
			m_bLastFileIsFolder = bIsFolder;
			m_lLastFileSize = lSize;
			
			bool bHas = m_sLastFile.Length > 0;
			
			lastFilePanel.Visibility = Rsc.ConditionalVisibility( bHas );
			
			m_btnRemoteEnter.Visibility = Rsc.ConditionalVisibility( bHas && bIsFolder );
			
			m_btnDownloadMem.Visibility = Rsc.ConditionalVisibility( bHas && !bIsFolder && lSize >= 0 );
			m_btnDownloadFile.Visibility = Rsc.ConditionalVisibility( bHas && !bIsFolder && lSize >= 0 );
			
			m_btnRemoteDateTime.Visibility = Rsc.ConditionalVisibility( bHas );
			
			m_btnRemoteList.Visibility = Rsc.ConditionalVisibility( bHas && bIsFolder );
			
			m_txtLastFile.Text = m_sLastFile;
		}

		private void m_btnRemoteEnter_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_sLastFile.Length == 0 ) return;
			
			if( !m_bLastFileIsFolder ) return;
			
			if( m_ftpc.WorkingDirectory.Length == 0 ) return;
			
			string sPath = m_ftpc.WorkingDirectory;
			sPath += m_sLastFile;
			sPath += m_ftpc.BackSlashInPath;
			
			SetLastFile( "", false );
			
			RscFtpClientCommand cmd = RscFtpClientCommand.ChangeWorkingDirecory( sPath );
			
			_Log("21", cmd.ToString() );
			
			m_ftpc.SendCommandToServer(cmd);
		}

		private void m_btnDownloadMem_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_sLastFile.Length == 0 ) return;
			
			if( m_bLastFileIsFolder ) return;
			
			if( m_lLastFileSize < 0 ) return;
			
			if( m_ftpc.WorkingDirectory.Length == 0 ) return;
			
			string sPath = m_ftpc.WorkingDirectory;
			sPath += m_sLastFile;
			
			RscFtpClientCommand cmd = RscFtpClientCommand.DownloadBin( sPath, m_lLastFileSize );
			
			_SendAutoPASV(cmd);
		}

		private void m_btnDownloadFile_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_sLastFile.Length == 0 ) return;
			
			if( m_bLastFileIsFolder ) return;
			
			if( m_lLastFileSize < 0 ) return;
			
			if( m_ftpc.WorkingDirectory.Length == 0 ) return;
			
			string sPath = m_ftpc.WorkingDirectory;
			sPath += m_sLastFile;
								
			RscStore store = new RscStore();
			
			string sLocalPath = "A:\\FTP";
			
			if( !store.FolderExists( sLocalPath ) ) store.CreateFolder( sLocalPath );
			sLocalPath += "\\" + m_sLastFile;
			if( store.FileExists( sLocalPath ) ) store.DeleteFile( sLocalPath );
			System.IO.Stream stream = store.CreateFile(sLocalPath);
			
			RscFtpClientCommand cmd = RscFtpClientCommand.DownloadBin( sPath, m_lLastFileSize, "", null, stream );
			
			_SendAutoPASV(cmd);
		}
				
		private void m_btnRemoteDel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_sLastFile.Length == 0 ) return;
			
			RscFtpClientCommand cmd;
			
			if( m_bLastFileIsFolder )
				cmd = RscFtpClientCommand.DeleteFolder(m_sLastFile);
			else
				cmd = RscFtpClientCommand.DeleteFile(m_sLastFile);
			
			SetLastFile( "", false );
			
			_Log("21", cmd.ToString());
			
			m_ftpc.SendCommandToServer(cmd);
		}
				
		private void m_btnRemoteDateTime_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_sLastFile.Length == 0 ) return;
			
			RscFtpClientCommand cmd;
			
			cmd = RscFtpClientCommand.GetLastModifiedFileDateTime(m_sLastFile);
			
			_Log("21", cmd.ToString());
			
			m_ftpc.SendCommandToServer(cmd);
		}
				
		private void m_btnRemoteList_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_sLastFile.Length == 0 ) return;
			
			if( !m_bLastFileIsFolder ) return;
			
			RscFtpClientCommand cmd = RscFtpClientCommand.ListFilesAndFolders(
				m_sLastFile + m_ftpc.BackSlashInPath );
			
			_SendAutoPASV(cmd);
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
					+ " (" + m_ftpc.StatUpCmd.Seconds.ToString() + ")"
				+ " |"
				+ " usr: " + RscUtils.toMBstr(m_ftpc.StatUpDat.ByteCount)
					+ " (" + m_ftpc.StatUpDat.Seconds.ToString() + ")";
			
			sPlus += "\r\n" + "DN <-"
				+ " sys: " + RscUtils.toMBstr(m_ftpc.StatDnCmd.ByteCount)
					+ " (" + m_ftpc.StatDnCmd.Seconds.ToString() + ")"
				+ " |"
				+ " usr: " + RscUtils.toMBstr(m_ftpc.StatDnDat.ByteCount)
					+ " (" + m_ftpc.StatDnDat.Seconds.ToString() + ")";
			
			long lUp = m_ftpc.BytesPerSecUp;
			long lDn = m_ftpc.BytesPerSecDn;
			if( lUp > 0 || lDn > 0 )
			{
				sPlus += "\r\nSpeed ";
				if( lUp > 0 )
				{
					sPlus += "UP: " + RscUtils.toMBstr(lUp) + "/s";
				}
				if( lDn > 0 )
				{
					if( lUp > 0 ) sPlus += " | ";
					sPlus += "DN: " + RscUtils.toMBstr(lDn) + "/s";
				}
			}
			
			m_AppFrame.SetStatusText( sSAVE_Status + sPlus, scSAVE );
		}
		
    }
	
}
