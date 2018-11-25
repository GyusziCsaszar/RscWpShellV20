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

namespace RscXtests
{
	
    public partial class RscDeviceInfoV10 : PhoneApplicationPage
    {
		
		RscAppFrame m_AppFrame;
		
		TextBoxDenieEdit m_txtTitle;
		
		class NavToPar
		{
			
			public string Key;
			public string Value;
			
			public NavToPar(string sKey, string sValue)
			{
				Key = sKey;
				Value = sValue;
			}
			
		}
		
        public RscDeviceInfoV10()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Device Info 1.0", "Images/Ico001_Settings.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			
			m_txtTitle = new TextBoxDenieEdit(true, true, TitlePanel, Grid.ColumnProperty, 1);
			m_txtTitle.Background = new SolidColorBrush(Colors.LightGray);
			m_txtTitle.Foreground = new SolidColorBrush(Colors.Black);
			m_txtTitle.FontSize = 16;
			m_txtTitle.Text = "(none)";
        }
		
		protected override void OnNavigatedTo(NavigationEventArgs args)
		{
			//Handle ExitOnBack=True arg...
			RscUtils.OnNavigatedTo_ExitOnBack(this.NavigationContext.QueryString);
			
			spItems.Children.Clear();
			
			List<NavToPar> ap = new List<NavToPar>();
			
			NavToPar np;
			string sBool;
			
			np = new NavToPar( "RscUtils.GetDeviceName",
				"\"" + RscUtils.GetDeviceName() + "\"" );
			ap.Add( np );
			
			np = new NavToPar( "(Networking.Proximity.PeerFinder) Display Name",
				Windows.Networking.Proximity.PeerFinder.DisplayName );
			ap.Add( np );
			
			/*
			var deviceInformation = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
			console.log(deviceInformation.systemSku);
			np = new NavToPar( "(ExchangeActiveSyncProvisioning) systemSku",
				deviceInformation.systemSku );
			ap.Add( np );
			*/
			
			np = new NavToPar( "(DeviceStatus) Device Name",
				Microsoft.Phone.Info.DeviceStatus.DeviceName );
			ap.Add( np );
			
			np = new NavToPar( "(Extended Properties) Device Name",
				Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceName").ToString() );
			ap.Add( np );
			
			np = new NavToPar( "(DeviceStatus) Device Manufacturer",
				Microsoft.Phone.Info.DeviceStatus.DeviceManufacturer );
			ap.Add( np );
			
			np = new NavToPar( "(DeviceStatus) Device Hardware Version",
				Microsoft.Phone.Info.DeviceStatus.DeviceHardwareVersion );
			ap.Add( np );
			
			np = new NavToPar( "(DeviceStatus) Device Total Memory",
				RscUtils.toMBstr( Microsoft.Phone.Info.DeviceStatus.DeviceTotalMemory, false ) );
			ap.Add( np );
			
			sBool = "No";
			if( Microsoft.Phone.Info.DeviceStatus.IsKeyboardDeployed ) sBool = "Yes";
			np = new NavToPar( "(DeviceStatus) Is Keyboard Deployed",
				sBool );
			ap.Add( np );
			
			np = new NavToPar( "(DeviceStatus) Power Source",
				Microsoft.Phone.Info.DeviceStatus.PowerSource.ToString() );
			ap.Add( np );
			
			np = new NavToPar( "(Environment) Device Type",
				Microsoft.Devices.Environment.DeviceType.ToString() );
			ap.Add( np );
			
			np = new NavToPar( "(Extended Properties)Device Firmware Version",
				Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceFirmwareVersion").ToString() );
			ap.Add( np );
			
			np = new NavToPar( "(Extended Properties)Device Hardware Version",
				Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceHardwareVersion").ToString() );
			ap.Add( np );
			
			np = new NavToPar( "(Extended Properties)Device Manufacturer",
				Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceManufacturer").ToString() );
			ap.Add( np );
				
			foreach( NavToPar p in ap )
			{
				AddItem( false, p.Key, p.Value );
			}
			
			base.OnNavigatedTo(args);
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
		
    }
	
}