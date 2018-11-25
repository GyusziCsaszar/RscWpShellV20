using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.ComponentModel;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;
using System.Windows.Media;

using Ressive.Utils;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

namespace Lib_RscIPgC_Dlgs
{
	
    public partial class RscDlg_FtpHostInputV10 : PhoneApplicationPage
    {
		
		const string csClsName = "RscDlg_FtpHostInputV10";
		
		RscAppFrame m_AppFrame;
	
		RscPageArgsRet m_AppInput;
		
		RscRegistryValueList m_RegHistory;
		
        public RscDlg_FtpHostInputV10()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "FTP Host Input 1.0", "Images/Ico001_Ressive.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			// ///////////////
			imgIpUpIco.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Inc.jpg");
			imgIpDnIco.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Dec.jpg");
			
			txSvrIP.Text = "";
			
			RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
			m_AppInput = appArgsMgr.GetInput( csClsName );
			if( m_AppInput != null )
			{
				
				m_AppFrame.AppTitle = m_AppInput.CallerAppTitle;
				m_AppFrame.AppIconRes = m_AppInput.CallerAppIconRes;
				
				//lbStr.Text = m_AppInput.GetFlag(0);
				//txSvrIP.Text = m_AppInput.GetData(0);
				
				if( txSvrIP.Text.Length == 0 )
				{
					string sPath = "Software\\Ressive.Hu\\" + csClsName + "\\History";
					sPath += "\\" + m_AppInput.CallerAppTitle;
					sPath += "\\" + m_AppInput.ID;
					txSvrIP.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER, sPath, "LastOk", "" );
				}
				
				_LoadHistory();
			}
       }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Back" );
				appOutput.SetOutput();
				
				//Maybe user deleted some items...
				m_RegHistory.Flush();
			}
			
			//e.Cancel = true;
        }
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			OnOK();
		}
				
		private void btnOk_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			OnOK();
		}
		
		private void OnOK()
		{
			bool bGoBack = true;
			
			if( !DeviceNetworkInformation.IsNetworkAvailable )
			{
				ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
				connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.WiFi;
				connectionSettingsTask.Show();
				
				return;
			}
			
			if( m_AppInput != null )
			{
				string strRes = txSvrIP.Text;
				
				//NoEmpty...
				if( m_AppInput.GetFlag(1).Length > 0 )
				{
					strRes = strRes.Trim();
					
					if( strRes.Length == 0 )
					{
						MessageBox.Show("Value must not be empty!");
						return;
					}
				}
				
				/*
				//FileName...
				if( m_AppInput.GetFlag(1).Length > 0 )
				{
					strRes = strRes.Trim();
					
					string strChk = "\\/:*?\"<>|";
					foreach( char cChk in strChk )
					{
						if( strRes.IndexOf( cChk ) >= 0 )
						{
							MessageBox.Show("Value must not contain characters of '" + strChk + "'!");
							return;
						}
					}
				}
				*/
				
				bGoBack = false;
				
				if( RscUtils.IsIpAddress( strRes ) )
				{
					/*
					MessageBox.Show("IP Address is not allowed here!");
					return;
					*/
					
					DoOk();
				}
				else
				{
					prsBar.Visibility = Rsc.Visible;
					
					var endpoint = new DnsEndPoint(strRes, 0);
					DeviceNetworkInformation.ResolveHostNameAsync(endpoint, OnHostResolved, null);
				}
			}

			if( bGoBack )
			{
				this.NavigationService.GoBack();
			}
		}
		
		private void OnHostResolved(NameResolutionResult result)
		{
			IPEndPoint[] endpoints = result.IPEndPoints;
			
			System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				prsBar.Visibility = Rsc.Collapsed;
					
				if( endpoints == null )
				{
					MessageBox.Show("Host can not be resolved!\n\nTIP: Even 4-5 tries may be required...");
				}
				else
				{
					string sCustRes = "";
					
					foreach( IPEndPoint ipep in endpoints )
					{
						byte[] ipa = ipep.Address.GetAddressBytes();
						
						sCustRes = ipa[0].ToString() + "." + ipa[ 1 ].ToString() + "."
							+ ipa[2].ToString() + "." + ipa[3].ToString();
						
						//LAST value is the good one...
						//break;
					}
					
					DoOk( sCustRes );
				}
			});
		}		
		
		private void DoOk( string sCustRes = "" )
		{	
			string sPath = "Software\\Ressive.Hu\\" + csClsName + "\\History";
			sPath += "\\" + m_AppInput.CallerAppTitle;
			sPath += "\\" + m_AppInput.ID;
			RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER, sPath, "LastOk", txSvrIP.Text );
			
			m_RegHistory.Add( txSvrIP.Text );
			m_RegHistory.Flush();
			
			if( sCustRes.Length == 0 )
				sCustRes = txSvrIP.Text;
			
			RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
			appOutput.SetFlag( 0, "Ok" );
			appOutput.SetData( 0, sCustRes ); //txSvrIP.Text );
			appOutput.SetOutput();
			
			this.NavigationService.GoBack();
		}
		
		private void m_AppFrame_OnExit(object sender, EventArgs e)
		{
			OnCancel();
		}
				
		private void btnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			OnCancel();
		}
		
		private void OnCancel()
		{
			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Cancel" );
				appOutput.SetOutput();
				
				//Maybe user deleted some items...
				m_RegHistory.Flush();
			}

			this.NavigationService.GoBack();
		}
		
		private void _LoadHistory()
		{
			spHis.Children.Clear();
			
			string sPath = "Software\\Ressive.Hu\\" + csClsName + "\\History";
			sPath += "\\" + m_AppInput.CallerAppTitle;
			sPath += "\\" + m_AppInput.ID;
			
			m_RegHistory = new RscRegistryValueList(HKEY.HKEY_CURRENT_USER, sPath);
			
			for( int i = 0; i < m_RegHistory.Count; i++ )
			{
				_AddHis( m_RegHistory.Get( i ), i );
			}
			if( m_RegHistory.Count == 0 )
			{
				_AddHis( "<history is empty>", 0, true );
			}
		}
		
		private void _AddHis(string sValue, int iIndex, bool bDummyItem = false)
		{
			
			int idx = spHis.Children.Count + 1;
			
			Grid grdOut = new Grid();
			grdOut.Name = "grdOut_" + idx.ToString();
			grdOut.Margin = new Thickness(0, 0, 0, 4 );
			ColumnDefinition cd;
			cd = new ColumnDefinition(); cd.Width = new GridLength(1, GridUnitType.Star); grdOut.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdOut.ColumnDefinitions.Add(cd);
			spHis.Children.Add(grdOut);
			grdOut.Tag = iIndex;
			
			TextBoxDenieEdit txtItem = new TextBoxDenieEdit(true, true, grdOut, Grid.ColumnProperty, 0);
			txtItem.Background = new SolidColorBrush(Colors.Gray);
			txtItem.Foreground = new SolidColorBrush(Colors.White);
			//txtItem.FontSize = 16;
			txtItem.Text = sValue;
			
			if( bDummyItem ) return;
			
			Button btn = txtItem.ButtonShield;
			btn.Click += new System.Windows.RoutedEventHandler(btn_Click);
			
			RscIconButton btnRemove = new RscIconButton(grdOut, Grid.ColumnProperty, 1, 36, 36, Rsc.Visible);
			btnRemove.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Remove.jpg");
			btnRemove.Tag = grdOut;
			btnRemove.Click += new System.Windows.RoutedEventHandler(btnRemove_Click);
		
		}

		private void btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn = (Button) sender;
			TextBoxDenieEdit txtItem = (TextBoxDenieEdit) btn.Tag;
			
			txSvrIP.Text = txtItem.Text;
			
			m_AppFrame.AutoClick( btnOk, btnOk_Click );
		}

		private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btnRemove = (Button) sender;
			Grid grdOut = (Grid) btnRemove.Tag;
			int iIndex = (int) grdOut.Tag;
			
			m_RegHistory.Delete( iIndex );
			spHis.Children.Remove(grdOut);
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
		
    }
	
}
