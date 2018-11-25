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

using Ressive.Utils;
using Ressive.FrameWork;

namespace Lib_RscSettings
{
	
    public partial class RscDeviceInfoV11 : PhoneApplicationPage
    {
		
		RscAppFrame m_AppFrame;
		RscIconButton m_btnExpandAll;
		RscIconButton m_btnCollapseAll;
		
		Size m_sContentPanel = new Size(100, 100);
		
		RscTreeLbItemList m_aTI = null;
		
        public RscDeviceInfoV11()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Device Info 1.1", "Images/IcoSm001_DeviceInfo.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			
			ToolBarPanel.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack );
			
			m_btnExpandAll = new RscIconButton(ToolBarPanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible );
			m_btnExpandAll.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_TreeExpand.jpg");
			m_btnExpandAll.Click += new System.Windows.RoutedEventHandler(m_btnExpandAll_Click);
			
			m_btnCollapseAll = new RscIconButton(ToolBarPanel, Grid.ColumnProperty, 1, 50, 50, Rsc.Visible );
			m_btnCollapseAll.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_TreeCollapse.jpg");
			m_btnCollapseAll.Click += new System.Windows.RoutedEventHandler(m_btnCollapseAll_Click);
			
			m_AppFrame.ShowButtonNext( false );
			
			m_aTI = new RscTreeLbItemList( lbTree, m_AppFrame.Theme, "Images/Btn001_TreeExpand.jpg", "Images/Btn001_TreeCollapse.jpg");
			
			ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);
			
			// //
			//
			
			TreeLbItem ti;
	
			ti = new TreeLbItem( m_aTI, null, "IE", "IE" );
			m_aTI.Add( ti );
			
			ti = new TreeLbItem( m_aTI, null, "Device", "Device" );
			m_aTI.Add( ti );
			
			ti = new TreeLbItem( m_aTI, null, "Network", "Network" );
			m_aTI.Add( ti );
			
			//
			// //
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
			this.NavigationService.GoBack();
		}
		
		private void m_btnExpandAll_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_AppFrame.StartTimer( "expand_all", LayoutRoot, 1, 0, false );
		}
		
		private void m_AppFrame_OnTimer(object sender, RscAppFrameTimerEventArgs e)
		{
			switch( e.Reason )
			{
				case "expand_all" :
				{
					if( e.Pos == e.Max )
					{
						m_aTI.ExpandAll();
						
						m_AppFrame.StatusText = ""; //To refresh mem info...
					}
					
					break;
				}
			}
		}
		
		private void m_btnCollapseAll_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_aTI.CollapseAll( false );
			
			m_AppFrame.StatusText = ""; //To refresh mem info...
		}
		
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
			TreeLbItem tiCurrent = (TreeLbItem) btn.Tag;
			
			if( tiCurrent.Expanded )
			{
				tiCurrent.Collapse();
			}
			else
			{
				if( tiCurrent.ContainerID != "IE" )
				{
					tiCurrent.Expand();
				}
				else
				{
					if( m_tiLoading != null )
					{
						MessageBox.Show( "Load in progress!\n\nPlease wait..." );
						return;
					}
					
					m_tiLoading = tiCurrent;
					m_tiLoading.Loading = true;
					
					UserAgentHelper.GetUserAgent(
						iePanel,
						userAgent =>
							{
								if( m_tiLoading != null )
								{
									m_tiLoading.Loading = false;
									
									m_tiLoading.PreInserts();
									
									TreeLbItem ti;
							
									ti = new TreeLbItem( m_tiLoading.Holder, m_tiLoading, "UserAgent",
										"User Agent",
										userAgent );
									m_tiLoading.Insert( ti );
									
									m_tiLoading.Expand_Base();
									
									m_tiLoading = null;
								}
							});
					
				}
			}
		}
	
		public static class UserAgentHelper
		{
			
			private const string Html =
				@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"">
		
				<html>
				<head>
				<script language=""javascript"" type=""text/javascript"">
					function notifyUA() {
					window.external.notify(navigator.userAgent);
					}
				</script>
				</head>
				<body onload=""notifyUA();""></body>
				</html>";
		
			public static void GetUserAgent(Panel rootElement, Action<string> callback)
			{
				var browser = new Microsoft.Phone.Controls.WebBrowser();
				
				browser.IsScriptEnabled = true;
				browser.Visibility = Visibility.Collapsed;
				browser.Loaded += (sender, args) => browser.NavigateToString(Html);
				
				browser.ScriptNotify += (sender, args) =>
											{
												string userAgent = args.Value;
												rootElement.Children.Remove(browser);
												callback(userAgent);
											};
											
				rootElement.Children.Add(browser);
			}
			
		}
		
		class TreeLbItem : RscTreeLbItemBase
		{
			
			protected string m_sContainerID = "";
			
			public TreeLbItem( RscTreeLbItemList oHolder, RscTreeLbItemBase oParent, string sContainerID, string sTitle, string sDetails = "" )
			: base( oHolder, oParent )
			{
				m_sContainerID = sContainerID;
				
				Title = sTitle;
				
				if( sDetails.Length > 0 )
				{
					if( sTitle.Length > 0 )
					{
						DetailsOfTitle = sDetails;
						IsLeaf = true;
					}
					else
					{
						DetailsOnly = sDetails;
					}
				}
			}
			
			public string ContainerID { get{ return m_sContainerID; } }
			
			override public void Expand()
			{
				if( ContainerID.Length == 0 ) return;
				
				// ???
				/*
				if( tiCurrent.Children.Count > 0 )
				{
					//TODO...
					return;
				}
				*/
								
				//VERY SLOW!!!
				//m_aTI.PreRefresh();		
											
				PreInserts();
				
				switch( ContainerID )
				{
					
					default :
					{
						Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceList nil
							= new Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceList();
						foreach( Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceInfo nii in nil )
						{
							if( ContainerID == nii.InterfaceName )
							{
								TreeLbItem ti;
					
								ti = new TreeLbItem( Holder, this, "",
									"Interface Name",
									nii.InterfaceName );
								Insert( ti );
					
								ti = new TreeLbItem( Holder, this, "",
									"Description",
									nii.Description + " " );
								Insert( ti );
					
								ti = new TreeLbItem( Holder, this, "",
									"Interface State",
									nii.InterfaceState.ToString() );
								Insert( ti );
					
								ti = new TreeLbItem( Holder, this, "",
									"Interface Type",
									nii.InterfaceType.ToString() );
								Insert( ti );
					
								ti = new TreeLbItem( Holder, this, "",
									"Characteristics",
									nii.Characteristics.ToString() );
								Insert( ti );
					
								ti = new TreeLbItem( Holder, this, "",
									"Bandwidth",
									nii.Bandwidth.ToString() );
								Insert( ti );
								
								break;
							}
						}
						
						break;
					}
					
					case "Network Interface List" :
					{
						Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceList nil
							= new Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceList();
						foreach( Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceInfo nii in nil )
						{
							TreeLbItem ti = new TreeLbItem( Holder, this, nii.InterfaceName, nii.InterfaceName );
							Insert( ti );
						}
						
						break;
					}
					
					case "Device Network Information" :
					{
						string sValue;
						TreeLbItem ti;
				
						ti = new TreeLbItem( Holder, this, "",
							"Cellular Mobile Operator",
							Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.CellularMobileOperator + " " );
						Insert( ti );
				
						sValue = "No";
						if( Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsCellularDataEnabled ) sValue = "Yes";
						ti = new TreeLbItem( Holder, this, "",
							"Is Cellular Data Enabled",
							sValue );
						Insert( ti );
				
						sValue = "No";
						if( Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsCellularDataRoamingEnabled ) sValue = "Yes";
						ti = new TreeLbItem( Holder, this, "",
							"Is Cellular Data Roaming Enabled",
							sValue );
						Insert( ti );
				
						sValue = "No";
						if( Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsNetworkAvailable ) sValue = "Yes";
						ti = new TreeLbItem( Holder, this, "",
							"Is Network Available",
							sValue );
						Insert( ti );
					
						sValue = "No";
						if( Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsWiFiEnabled ) sValue = "Yes";
						ti = new TreeLbItem( Holder, this, "",
							"Is WiFi Enabled",
							sValue );
						Insert( ti );
					
						break;
					}
					
					case "Network" :
					{
						TreeLbItem ti;
				
						ti = new TreeLbItem( Holder, this, "Device Network Information", "Device Network Information" );
						Insert( ti );
				
						ti = new TreeLbItem( Holder, this, "Network Interface List", "Network Interface List" );
						Insert( ti );
						
						break;
					}
					
					case "Device.Rsc" :
					{
						TreeLbItem ti;
				
						ti = new TreeLbItem( Holder, this, "",
							"RscUtils.GetDeviceName",
							"\"" + RscUtils.GetDeviceName() + "\"" + "\n\n"
							+ "Networking.Proximity.PeerFinder.DisplayName =\n"
							+ Windows.Networking.Proximity.PeerFinder.DisplayName );
						Insert( ti );
				
						break;
					}
					
					case "Device.DeviceStatus" :
					{
						string sValue;
						TreeLbItem ti;
				
						ti = new TreeLbItem( Holder, this, "",
							"Device Name",
							Microsoft.Phone.Info.DeviceStatus.DeviceName );
						Insert( ti );
				
						ti = new TreeLbItem( Holder, this, "",
							"Device Manufacturer",
							Microsoft.Phone.Info.DeviceStatus.DeviceManufacturer );
						Insert( ti );
				
						ti = new TreeLbItem( Holder, this, "",
							"Device Total Memory",
							RscUtils.toMBstr( Microsoft.Phone.Info.DeviceStatus.DeviceTotalMemory, false ) );
						Insert( ti );
				
						sValue = "No";
						if( Microsoft.Phone.Info.DeviceStatus.IsKeyboardDeployed ) sValue = "Yes";
						ti = new TreeLbItem( Holder, this, "",
							"Is Keyboard Deployed",
							sValue );
						Insert( ti );
				
						ti = new TreeLbItem( Holder, this, "",
							"Power Source",
							Microsoft.Phone.Info.DeviceStatus.PowerSource.ToString() );
						Insert( ti );
				
						break;
					}
					
					case "Device.Environment" :
					{
						TreeLbItem ti;
				
						ti = new TreeLbItem( Holder, this, "",
							"Device Type",
							Microsoft.Devices.Environment.DeviceType.ToString() );
						Insert( ti );
				
						break;
					}
					
					case "Device.ExtendedProperties" :
					{
						TreeLbItem ti;
				
						ti = new TreeLbItem( Holder, this, "",
							"Device Name",
							Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceName").ToString() );
						Insert( ti );
				
						ti = new TreeLbItem( Holder, this, "",
							"Device Manufacturer",
							Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceManufacturer").ToString() );
						Insert( ti );
				
						ti = new TreeLbItem( Holder, this, "",
							"Device Firmware Version",
							Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceFirmwareVersion").ToString() );
						Insert( ti );
				
						ti = new TreeLbItem( Holder, this, "",
							"Device Hardware Version",
							Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceHardwareVersion").ToString() );
						Insert( ti );
				
						break;
					}
					
					case "Device" :
					{
						TreeLbItem ti;
				
						ti = new TreeLbItem( Holder, this, "Device.Rsc", "Rsc" );
						Insert( ti );
				
						ti = new TreeLbItem( Holder, this, "Device.DeviceStatus", "Device Status" );
						Insert( ti );
				
						ti = new TreeLbItem( Holder, this, "Device.Environment", "Device Environment" );
						Insert( ti );
				
						ti = new TreeLbItem( Holder, this, "Device.ExtendedProperties", "Device Extended Properties" );
						Insert( ti );
						
						break;
					}
					
				}
				
				base.Expand();
			}
			
		}
		
    }
	
}