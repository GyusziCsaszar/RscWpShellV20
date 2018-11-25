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
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Net.NetworkInformation;

using Ressive.Utils;
using Ressive.FrameWork;

namespace RscXtests
{
    public partial class RscListNetConnV10 : PhoneApplicationPage
    {
		
		RscAppFrame m_AppFrame;
		
		TextBoxDenieEdit m_txtTitle;
		
        public RscListNetConnV10()
        {
            InitializeComponent();
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "List Network Connections 1.0", "Images/Ico001_Ressive.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			
			m_txtTitle = new TextBoxDenieEdit(true, true, TitlePanel, Grid.ColumnProperty, 1);
			m_txtTitle.Background = new SolidColorBrush(Colors.LightGray);
			m_txtTitle.Foreground = new SolidColorBrush(Colors.Black);
			m_txtTitle.FontSize = 16;
			m_txtTitle.Text = "TODO: press button Next to list.";
			
			txHostName.Text = "localhost";
			txIP.Text = "192.168.0.100";
        }
		
		private void m_AppFrame_OnExit(object sender, EventArgs e)
		{
			this.NavigationService.GoBack();
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( m_AppFrame.CancelTimer() )
				e.Cancel = true;
			
			//e.Cancel = true;
        }
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			spItems.Children.Clear();
			
			string s;
			
			s = "";
			//
			s += "Cellular Mobile Operator: ";
			s += DeviceNetworkInformation.CellularMobileOperator;
			//
			s += "\r\n";
			s += "Is Cellular Data Enabled: ";
			if( DeviceNetworkInformation.IsCellularDataEnabled )
				s += "Yes";
			else
				s += "No";
			//
			s += "\r\n";
			s += "Is Cellular Data Roaming Enabled: ";
			if( DeviceNetworkInformation.IsCellularDataRoamingEnabled )
				s += "Yes";
			else
				s += "No";
			//
			s += "\r\n";
			s += "Is Network Available: ";
			if( DeviceNetworkInformation.IsNetworkAvailable )
				s += "Yes";
			else
				s += "No";
			//
			s += "\r\n";
			s += "Is WiFi Enabled: ";
			if( DeviceNetworkInformation.IsWiFiEnabled )
				s += "Yes";
			else
				s += "No";
			//
			AddItem( true, "Device Network Information", s );
			
			NetworkInterfaceList nil = new NetworkInterfaceList();
			AddItem( true, "Network Interface List", "" );
			foreach( NetworkInterfaceInfo nii in nil )
			{
				s = "";
				//
				s += "Interface Name: ";
				s += nii.InterfaceName;
				//
				s += "\r\n";
				s += "Description: ";
				s += nii.Description;
				//
				s += "\r\n";
				s += "Interface State: ";
				s += nii.InterfaceState.ToString();
				//
				s += "\r\n";
				s += "Interface Type: ";
				s += nii.InterfaceType.ToString();
				//
				//
				s += "\r\n";
				s += "Characteristics: ";
				s += nii.Characteristics.ToString();
				//
				s += "\r\n";
				s += "Bandwidth: ";
				s += nii.Bandwidth.ToString();
				//
				AddItem( false, nii.InterfaceName, s );
			}
			
		}
		
		private void AddItem(bool bContainer, string sTitle, string sDetails)
		{
			
			int idx = spItems.Children.Count + 1;
			
			Grid grdOut = new Grid();
			grdOut.Name = "grdOut_" + idx.ToString();
			grdOut.Margin = new Thickness(0, 0, 0, 4 );
			RowDefinition rd;
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdOut.RowDefinitions.Add(rd);
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdOut.RowDefinitions.Add(rd);
			spItems.Children.Add(grdOut);
			
			Rectangle rc;
			rc = new Rectangle();
			if( bContainer )
			{
				rc.Fill = new SolidColorBrush(Colors.Orange);
			}
			else
			{
				rc.Fill = new SolidColorBrush(Colors.Blue);
			}
			rc.Opacity = 0.5;
			rc.SetValue(Grid.RowProperty, 0);
			grdOut.Children.Add(rc);
	
			Button btnMore = new Button();
			btnMore.Name = "btnOpen_" + idx.ToString();
			btnMore.Content = sTitle;
			btnMore.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
			btnMore.BorderThickness = new Thickness(0);
			btnMore.FontSize = 20;
			btnMore.Foreground = new SolidColorBrush(Colors.White); //.Blue);
			btnMore.Margin = new Thickness(-12,-10,-12,-12);
			//btnMore.Tag = it;
			//btnMore.Opacity = 0.5;
			btnMore.SetValue(Grid.RowProperty, 0);
			grdOut.Children.Add(btnMore);
			
			Grid grdTit = new Grid();
			grdTit.Name = "grdTit_" + idx.ToString();
			grdTit.Margin = new Thickness(12, 0, 0, 0);
			//RowDefinition rd;
			rd = new RowDefinition(); grdTit.RowDefinitions.Add(rd);
			grdTit.SetValue(Grid.RowProperty, 1);
			grdOut.Children.Add(grdTit);
			
			TextBox tbDetails = new TextBox();
			tbDetails.Name = "tbDet_" + idx.ToString();
			tbDetails.FontSize = 16;
			tbDetails.Text = sDetails;
			tbDetails.Background = new SolidColorBrush(Colors.Gray);
			tbDetails.Foreground = new SolidColorBrush(Colors.White);
			tbDetails.Margin = new Thickness(-11, -12, -12, -12);
			tbDetails.BorderThickness = new Thickness(0, 0, 0, 0);
			tbDetails.AcceptsReturn = true;
			tbDetails.TextWrapping = TextWrapping.Wrap;
			tbDetails.SetValue(Grid.RowProperty, 0);
			grdTit.Children.Add(tbDetails);
		
			Button btn = new Button();
			btn.Name = "btnTit_" + idx.ToString();
			btn.Content = "";
			btn.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
			btn.BorderThickness = new Thickness(0);
			btn.Foreground = new SolidColorBrush(Colors.White); //.Blue);
			btn.Margin = new Thickness(-12,-10,-12,-12);
			//btn.Tag = it;
			btn.Opacity = 0.5;
			btn.SetValue(Grid.RowProperty, 1);
			grdOut.Children.Add(btn);
			
			//it.tbTit = tbTit;
			//it.btn = btn;
			
			//btnMore.Tag = it;
			//btn.Tag = it;
			
			/*
			btnMore.Tap += new System.EventHandler<System.Windows.Input.GestureEventArgs>(btn_Tap);
			btn.Tap += new System.EventHandler<System.Windows.Input.GestureEventArgs>(btn_Tap);
			*/
			
			/*
			btnMore.DoubleTap += new System.EventHandler<System.Windows.Input.GestureEventArgs>(btn_DoubleTap);
			btn.DoubleTap += new System.EventHandler<System.Windows.Input.GestureEventArgs>(btn_DoubleTap);
			*/
			
		}
		
		private void btnHostResolve_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			txIP.Text = "";
			
			if( RscUtils.IsIpAddress( txHostName.Text ) )
			{
				txIP.Text = "ERR: IP can not be resolved!";
			}
			else
			{
				var endpoint = new DnsEndPoint(txHostName.Text, 0);
				DeviceNetworkInformation.ResolveHostNameAsync(endpoint, OnHostResolved, null);
			}
		}
		
		private void OnHostResolved(NameResolutionResult result)
		{
			string sRes = "";
			
			IPEndPoint[] endpoints = result.IPEndPoints;
			if( endpoints == null )
			{
				sRes = "NOT FOUND";
			}
			else
			{
				foreach( IPEndPoint ipep in endpoints )
				{
					byte[] ipa = ipep.Address.GetAddressBytes();
					sRes += "(" + ipa[0].ToString() + "." + ipa[ 1 ].ToString() + "."
						+ ipa[2].ToString() + "." + ipa[3].ToString() + ")";
				}
			}
			
			System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				txIP.Text = sRes;
				
				//MessageBox.Show("OnHostResolved called!", "Warning", MessageBoxButton.OK);
			});
		}		
		
		/*
		private void btnIPResolve_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			txHostName.Text = "";
			
			var endpoint = new DnsEndPoint(txIP.Text, 0);
			//var endpoint = new IPEndPoint( IPAddress.Parse( txIP.Text), 0 );
			
			DeviceNetworkInformation.ResolveHostNameAsync(endpoint, OnIPResolved, null);
		}
		
		private void OnIPResolved(NameResolutionResult result)
		{
			IPEndPoint[] endpoints = result.IPEndPoints;
			
			System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				
				txHostName.Text = result.HostName;
				
				foreach( IPEndPoint ipep in endpoints )
				{
					byte[] ipa = ipep.Address.GetAddressBytes();
					txHostName.Text += "(" + ipa[0].ToString() + "." + ipa[ 1 ].ToString() + "."
						+ ipa[2].ToString() + "." + ipa[3].ToString() + ")";
				}
				
				//MessageBox.Show("OnIPResolved called!", "Warning", MessageBoxButton.OK);
				
			});
		}*/
		
    }
	
}