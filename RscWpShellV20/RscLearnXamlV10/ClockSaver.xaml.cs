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
using Microsoft.Phone.Net.NetworkInformation;
using System.Windows.Shapes;
using System.Windows.Media;

using Ressive.Utils;
using Ressive.Store;

namespace RscLearnXamlV10
{
	
    public partial class ClockSaver : PhoneApplicationPage
    {
			
		int iBandHeight = 0;
		
		private Grid m_grid = null;
		private TextBlock m_tbClk = null;
		private TextBlock m_tbDt = null;
		private TextBlock m_tbFs = null;
		
		private DispatcherTimer m_tmr;
		private int m_iSecTime = 0;
		private int m_iSecDate = 0;
		private int m_iSecFree = 0;
		private int m_iSecMove = 0;
		
        public ClockSaver()
        {
            InitializeComponent();
 			
			PhoneApplicationService phoneAppService = PhoneApplicationService.Current;
			phoneAppService.UserIdleDetectionMode = IdleDetectionMode.Disabled;
			
			m_tmr = new DispatcherTimer();
			m_tmr.Interval = System.TimeSpan.FromSeconds(1);
			m_tmr.Tick +=new System.EventHandler(m_tmr_Tick);
			m_tmr.Start();
			
			this.Unloaded += new System.Windows.RoutedEventHandler(RscClockSaverV10_Unloaded);
       }

		private void RscClockSaverV10_Unloaded(object sender, System.Windows.RoutedEventArgs e)
		{
			m_tmr.Stop();
			
			PhoneApplicationService phoneAppService = PhoneApplicationService.Current;
			phoneAppService.UserIdleDetectionMode = IdleDetectionMode.Enabled;
		}
		
		private void m_tmr_Tick(object sender, System.EventArgs e)
		{
			m_iSecTime++;
			m_iSecDate++;
			m_iSecFree++;
			m_iSecMove++;
			
			UpdateContents(ContentPanel.ActualHeight);
		}
		
		private void UpdateContents(double dCY, bool bIgnoreSec = false)
		{
			
			if( m_tbClk != null )
			{
				if( m_iSecTime >= 1 || bIgnoreSec )
				{
					m_iSecTime = 0;
					
					DateTime dNow = DateTime.Now;
					m_tbClk.Text = RscUtils.pad60(dNow.Hour) +
						":" + RscUtils.pad60(dNow.Minute) + ":" +
						RscUtils.pad60(dNow.Second);
				}
			}
			
			if( m_tbDt != null )
			{
				if( m_iSecDate >= 1 || bIgnoreSec )
				{
					m_iSecDate = 0;
					
					DateTime dNow = DateTime.Now;
					string sCnt = "";
						
					sCnt = dNow.Year.ToString() +
						". " + RscUtils.pad60(dNow.Month) + ". " +
						RscUtils.pad60(dNow.Day) + ".";
					
					sCnt += " ";
					switch( dNow.DayOfWeek )
					{
						case DayOfWeek.Monday: sCnt += "Hétfő"; break;
						case DayOfWeek.Tuesday: sCnt += "Kedd"; break;
						case DayOfWeek.Wednesday: sCnt += "Szerda"; break;
						case DayOfWeek.Thursday: sCnt += "Csütörtök"; break;
						case DayOfWeek.Friday: sCnt += "Péntek"; break;
						case DayOfWeek.Saturday: sCnt += "Szombat"; break;
						case DayOfWeek.Sunday: sCnt += "Vasárnap"; break;
					}
						
					int iWeek;
					iWeek = dNow.DayOfYear / 7;
					if( dNow.DayOfYear % 7 > 0 ) iWeek++;
					sCnt += ", " + RscUtils.pad60(iWeek) + ". hét";
					
					sCnt += ", ";
					switch( dNow.Month )
					{
						case 1 : sCnt += "Január"; break;
						case 2 : sCnt += "Február"; break;
						case 3 : sCnt += "Március"; break;
						case 4 : sCnt += "Április"; break;
						case 5 : sCnt += "Május"; break;
						case 6 : sCnt += "Június"; break;
						case 7 : sCnt += "Július"; break;
						case 8 : sCnt += "Augusztus"; break;
						case 9 : sCnt += "Szeptember"; break;
						case 10 : sCnt += "Október"; break;
						case 11 : sCnt += "November"; break;
						case 12 : sCnt += "December"; break;
					}
					
					m_tbDt.Text = sCnt;
				}
			}
			
			if( m_tbFs != null )
			{
				if( m_iSecFree >= 10 || bIgnoreSec )
				{
					m_iSecFree = 0;
					
					string sIsoStoreDrive = "";
					string sCnt = "";
					sCnt = RscUtils.toMBstr(RscStore.AvailableFreeSpace( out sIsoStoreDrive ));
					if( sIsoStoreDrive.Length > 0 )
					{
						sCnt = sIsoStoreDrive + " " + sCnt;
					}
					
					sCnt += " | " + Windows.Phone.Devices.Power.Battery.GetDefault().RemainingChargePercent.ToString() + " %";
					if( Microsoft.Phone.Info.DeviceStatus.PowerSource == Microsoft.Phone.Info.PowerSource.Battery )
						sCnt += ""; //" (batt)";
					else
						sCnt += " (chrg)";
					
					if( DeviceNetworkInformation.IsWiFiEnabled )
					{
						sCnt += " | " + "WiFi (";
						
						NetworkInterfaceList nil;
						nil = new NetworkInterfaceList();
						foreach(NetworkInterfaceInfo ni in nil)
						{
							if( ni.InterfaceState == ConnectState.Connected )
							{
								if( ni.InterfaceSubtype == NetworkInterfaceSubType.WiFi )
								{
									sCnt += ni.Description;
									break;
								}
							}
						}
						
						sCnt += ")";
					}
					
					m_tbFs.Text = sCnt;
				}
			}
			
			if( m_iSecMove >= 5 || bIgnoreSec )
			{
				m_iSecMove = 0;
				
				Random rnd;
				rnd = new Random();
				
				int iTop;
				iTop = rnd.Next(((int) dCY) - iBandHeight);
				
				m_grid.Margin = new Thickness(0, iTop, 0, 0);
			}
		}
		
		private void ContentPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			
			//Defided in xaml as shell:SystemTray.IsVisible="False"
			//Microsoft.Phone.Shell.SystemTray.IsVisible = false;
			
			int iClkWidth;
			double dFsSm;
			double dFsLg;
			double dClkTop;
			
			if( e.NewSize.Width < e.NewSize.Height )
			{
				iClkWidth = 110;
				iBandHeight = 50;
				dFsLg = 25;
				dFsSm = 16;
				dClkTop = 5;
			}
			else
			{
				iClkWidth = 165;
				iBandHeight = 80;
				dFsLg = 40;
				dFsSm = 25;
				dClkTop = 10;
			}
			
			m_iSecTime = 0;
			m_iSecDate = 0;
			m_iSecFree = 0;
			m_iSecMove = 0;
			
			if( m_grid != null )
			{
				ContentPanel.Children.Remove(m_grid);
				m_grid = null;
				m_tbClk = null;
				m_tbDt = null;
				m_tbFs = null;
			}
			
			RowDefinition rd;
			ColumnDefinition cd;
			TextBlock tb;
			Rectangle rc;
			Grid grd;
			
			m_grid = new Grid();
			m_grid.Name = "grd";
			//m_grid.Background = new SolidColorBrush(Colors.Green);
			m_grid.Opacity = 0.5;
			rd = new RowDefinition(); rd.Height = GridLength.Auto;
			m_grid.RowDefinitions.Add(rd);
			rd = new RowDefinition(); rd.Height = new GridLength(1, GridUnitType.Star);
			m_grid.RowDefinitions.Add(rd);
			cd = new ColumnDefinition(); //cd.Width = new GridLength(1, GridUnitType.Star);
			m_grid.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto;
			m_grid.ColumnDefinitions.Add(cd);
			ContentPanel.Children.Add(m_grid);
			
			grd = new Grid();
			grd.SetValue(Grid.RowProperty, 0);
			grd.SetValue(Grid.ColumnProperty, 0);
			rd = new RowDefinition(); rd.Height = GridLength.Auto;
			grd.RowDefinitions.Add(rd);
			rd = new RowDefinition(); rd.Height = GridLength.Auto;
			grd.RowDefinitions.Add(rd);
			m_grid.Children.Add(grd);
			
			rc = new Rectangle();
			rc.SetValue(Grid.RowProperty, 0);
			rc.SetValue(Grid.ColumnProperty, 0);
			rc.Fill = new SolidColorBrush(Colors.Blue);
			rc.Width = e.NewSize.Width - iClkWidth;
			rc.Height = iBandHeight / 2;
			grd.Children.Add(rc);
			//
			tb = new TextBlock();
			tb.SetValue(Grid.RowProperty, 0);
			tb.SetValue(Grid.ColumnProperty, 0);
			tb.Margin = new Thickness(10, 2, 0, 0);
			tb.Width = (e.NewSize.Width - iClkWidth) - 10;
			tb.Height = (iBandHeight / 2) - 2;
			tb.FontSize = dFsSm;
			tb.Foreground = new SolidColorBrush(Colors.White);
			grd.Children.Add(tb);
			m_tbDt = tb;
			
			rc = new Rectangle();
			rc.SetValue(Grid.RowProperty, 1);
			rc.SetValue(Grid.ColumnProperty, 0);
			rc.Fill = new SolidColorBrush(Colors.LightGray);
			rc.Width = e.NewSize.Width - iClkWidth;
			rc.Height = iBandHeight / 2;
			grd.Children.Add(rc);
			//
			tb = new TextBlock();
			tb.SetValue(Grid.RowProperty, 1);
			tb.SetValue(Grid.ColumnProperty, 0);
			tb.Margin = new Thickness(10, 2, 0, 0);
			tb.Width = (e.NewSize.Width - iClkWidth) - 10;
			tb.Height = (iBandHeight / 2) - 2;
			tb.FontSize = dFsSm;
			tb.Foreground = new SolidColorBrush(Colors.Black);
			grd.Children.Add(tb);
			m_tbFs = tb;
			
			rc = new Rectangle();
			rc.SetValue(Grid.RowProperty, 0);
			rc.SetValue(Grid.ColumnProperty, 1);
			rc.Fill = new SolidColorBrush(Colors.LightGray);
			rc.Width = iClkWidth;
			rc.Height = iBandHeight;
			m_grid.Children.Add(rc);
			//
			tb = new TextBlock();
			tb.SetValue(Grid.RowProperty, 0);
			tb.SetValue(Grid.ColumnProperty, 1);
			tb.Margin = new Thickness(10, dClkTop, 0, 0);
			tb.Width = iClkWidth - 10;
			tb.Height = iBandHeight - dClkTop;
			tb.FontSize = dFsLg;
			tb.Foreground = new SolidColorBrush(Colors.Black);
			m_grid.Children.Add(tb);
			m_tbClk = tb;
			
			UpdateContents(e.NewSize.Height, true);
		}
		
    }
}
