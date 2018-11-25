using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Phone.Controls;
using System.Windows.Threading;
using Microsoft.Phone.Shell;

using Ressive.Utils;
using Ressive.Theme;

namespace Ressive.FrameWork
{
		
	public enum StatusColoring
	{
		Normal = 0,
		Success = 1,
		Warning = 2,
		Error = 3
	}
	
	public class RscAppFrame
	{
		
		const string csStatusDefault = "Ready...";
		
		RscTheme m_Theme = null;
		
		string m_sCompany = "NA";
		string m_sAppName = "N/A";
		string m_sAppIconRes = "Images/Img001_Dummy.jpg";
		
		PhoneApplicationPage m_AppPage = null;		
		Grid m_AppTitleBar = null;
		Grid m_AppStatusBar = null;
		
		RscIcon m_imgAppIcon = null;
		TextBlock m_tbTitle = null;
		RscIconButton m_btnNoSleep = null;
		RscIconButton m_btnTools = null;
		TextBoxDenieEdit m_txtSysInf = null;
		TextBoxDenieEdit m_txtTRACE = null;
		TextBoxDenieEdit m_txtStatus = null;
		RscIconButton m_btnNext = null;
		
		public delegate void OnExit_EventHandler(object sender, EventArgs e);
		public event OnExit_EventHandler OnExit;
		
		public delegate void OnNext_EventHandler(object sender, EventArgs e);
		public event OnNext_EventHandler OnNext;
		
		public delegate void OnTimer_EventHandler(object sender, RscAppFrameTimerEventArgs e);
		public event OnTimer_EventHandler OnTimer;
		
		DispatcherTimer m_tmrApp = null;
		ImageSource m_isTmrWc = null;
		string m_sTmrReason = "";
		int m_iTmrMax = -1;
		int m_iTmrPos = 0;
		int m_iTmrReTry = 0;
		Grid m_grdLayoutRootTmr;
		Canvas m_cnvTmr;
		ProgressBar m_prsTmr;
		List<Rectangle> m_aRcTmr = null;
		bool m_bTmrCanCancel;
		bool m_bTmrCancel;
		
		DateTime m_dtTraceStart = DateTime.Now;
		long m_lTraceMemUsage = 0;
		
		DispatcherTimer m_tmrAutoClick = null;
		Button m_btnAutoClick = null;
		event RoutedEventHandler m_ehOnAutoClick;
		
		string m_sToolsArgs = "";
		
		public RscAppFrame(string sCompany, string sAppName, string sAppIconRes, PhoneApplicationPage AppPage,
			Grid AppTitleBar = null, Grid AppStatusBar = null)
		{
			
			//MemUsage Optimization...
			Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			m_Theme = (RscTheme) GlobalDILholder.Tag;
			//m_DefaultedImageList = new RscDefaultedImageList( "Theme", "Current", "Default" );
			
			m_sCompany = sCompany;
			m_sAppName = sAppName;
			m_sAppIconRes = sAppIconRes;
			
			m_AppPage = AppPage;
			m_AppTitleBar = AppTitleBar;
			m_AppStatusBar = AppStatusBar;
			
			/*
			foreach( UIElement uie in m_AppTitleBar.Children)
			{
				switch( uie.GetValue(FrameworkElement.NameProperty).ToString() )
				{
					
					case "AppIcon" :
						((Image) uie).Source = m_DefaultedImageList.GetImage("Images/Ico001_Explorer4.jpg");
						break;
						
				}
			}
			*/
			
			_BuildAppTitleBar();
			_BuildAppStatusBar();
			
			m_isTmrWc = m_Theme.GetImage("Images/Img001_WaitCursor.jpg");
			m_tmrApp = new DispatcherTimer();
			m_tmrApp.Tick += new System.EventHandler(m_tmrApp_Tick);
			m_iTmrMax = -1;
			m_iTmrPos = 0;
			
			m_tmrAutoClick = new DispatcherTimer();
			m_tmrAutoClick.Tick +=new System.EventHandler(m_tmrAutoClick_Tick);
		}
		
		public string AppTitle
		{
			get
			{
				//FIX: App1 calls App2, App2 calls App3 (in App3 not Title of App1 is visible)
				//Explorer -> FindFiles -> TxtInputDlg...
				//return m_sCompany + " - " + m_sAppName;
				
				if( m_tbTitle == null ) return m_sCompany + " - " + m_sAppName;
				if( m_tbTitle.Text.Length == 0 ) return m_sCompany + " - " + m_sAppName;
				return m_tbTitle.Text;
			}
			set
			{
				if( m_tbTitle == null ) return;
				m_tbTitle.Text = value;
			}
		}
		
		public string AppIconRes
		{
			get{ return m_sAppIconRes; }
			set
			{
				if( m_imgAppIcon == null ) return;
				m_imgAppIcon.Image.Source = m_Theme.GetImage(value);
			}
		}
		
		public RscTheme Theme
		{
			get{ return m_Theme; }
		}
		
		public void UpdateAppMemInfo()
		{
			m_txtSysInf.Text = _GetAppMemInfo();
		}
		
		public string StatusText
		{
			set
			{
				if( m_txtStatus == null ) return;
				
				if( value.Length == 0 )
					m_txtStatus.Text = csStatusDefault;
				else
					m_txtStatus.Text = value;
				
				UpdateAppMemInfo();
			}
		}
		
		public void SetStatusText( string sStatus = csStatusDefault, StatusColoring sc = StatusColoring.Normal)
		{
			if( m_txtStatus == null ) return;
			
			if( sStatus.Length == 0 )
				m_txtStatus.Text = csStatusDefault;
			else
				m_txtStatus.Text = sStatus;
				
			UpdateAppMemInfo();
			
			switch( sc )
			{
				
				case StatusColoring.Success :
					m_txtStatus.Background = new SolidColorBrush(Colors.Green);
					m_txtStatus.Foreground = new SolidColorBrush(Colors.White);
					break;
				
				case StatusColoring.Warning :
					m_txtStatus.Background = new SolidColorBrush(Colors.Yellow);
					m_txtStatus.Foreground = new SolidColorBrush(Colors.Red);
					break;
					
				case StatusColoring.Error :
					m_txtStatus.Background = new SolidColorBrush(Colors.Red);
					m_txtStatus.Foreground = new SolidColorBrush(Colors.Yellow);
					break;
				
				//case StatusColoring.Normal :
				default :
					m_txtStatus.Background = new SolidColorBrush(m_Theme.ThemeColors.AppStatusBack);
					m_txtStatus.Foreground = new SolidColorBrush(m_Theme.ThemeColors.AppStatusFore);
					break;
					
			}
		}
		
		public void ShowButtonNext( bool bShow )
		{
			if( m_btnNext == null ) return;
			if( bShow )
				m_btnNext.Visibility = Rsc.Visible;
			else
				m_btnNext.Visibility = Rsc.Collapsed;
		}
		
		public void ShowButtonTools( bool bShow, string sToolsArgs = "" )
		{
			if( m_btnTools == null ) return;
			if( bShow )
				m_btnTools.Visibility = Rsc.Visible;
			else
				m_btnTools.Visibility = Rsc.Collapsed;
			
			m_sToolsArgs = sToolsArgs;
		}
		
		public void SetNoSleep( bool bSet )
		{
			if( bSet )
			{
				PhoneApplicationService phoneAppService = PhoneApplicationService.Current;
				phoneAppService.UserIdleDetectionMode = IdleDetectionMode.Disabled;
				
				m_btnNoSleep.Visibility = Rsc.Visible;
			}
			else
			{
				PhoneApplicationService phoneAppService = PhoneApplicationService.Current;
				phoneAppService.UserIdleDetectionMode = IdleDetectionMode.Enabled;
				
				m_btnNoSleep.Image.Visibility = Rsc.Collapsed;
				m_btnNoSleep.Visibility = Rsc.Collapsed;
			}
		}
		
		public string TRACE
		{
			set
			{
				if( m_txtTRACE == null ) return;
				
				if( value.Length == 0 )
					m_txtTRACE.Visibility = Rsc.Collapsed;
				else
					m_txtTRACE.Visibility = Rsc.Visible;
				
				m_txtTRACE.Text = value;
			}
		}
		
		public void TRACE_Start()
		{
			if( m_txtTRACE == null ) return;
			
			m_dtTraceStart = DateTime.Now;
			m_lTraceMemUsage = Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage;
		}
		
		public void TRACE_Stop()
		{
			if( m_txtTRACE == null ) return;
			
			string sTrace = "";
			
			TimeSpan ts = DateTime.Now - m_dtTraceStart;
			sTrace += ts.ToString();
			for(;;)
			{
				if( sTrace.Length == 0 ) break;
				if( sTrace[ sTrace.Length - 1 ] != '0' ) break;
				sTrace = sTrace.Substring(0, sTrace.Length - 1);
			}
			for(;;)
			{
				if( sTrace.Length == 0 ) break;
				if( sTrace[ 0 ] != '0' &&  sTrace[ 0 ] != ':' ) break;
				sTrace = sTrace.Substring(1, sTrace.Length - 1);
			}
			sTrace += " sec\r\n";
			
			long lMemUsage = Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage - m_lTraceMemUsage;
			{
				//FIX... ...toMBstr fails with negative number...
				if( lMemUsage < 0 ) sTrace += "-";
				if( lMemUsage < 0 ) lMemUsage = lMemUsage * -1;
				
				sTrace += RscUtils.toMBstr(lMemUsage);
			}
			
			TRACE = sTrace;
		}
		
		protected void _BuildAppTitleBar()
		{
			if( m_AppTitleBar == null ) return;
			
			//m_AppTitleBar.Margin = new Thickness(0, 0, 0, 4);
			m_AppTitleBar.Background = new SolidColorBrush(m_Theme.ThemeColors.AppTitleBack);
			
			ColumnDefinition cd;
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; m_AppTitleBar.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; m_AppTitleBar.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; m_AppTitleBar.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = new GridLength(1, GridUnitType.Star); m_AppTitleBar.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; m_AppTitleBar.ColumnDefinitions.Add(cd);
			
			m_imgAppIcon = new RscIcon(m_AppTitleBar, Grid.ColumnProperty, 0, 36, 36, Rsc.Visible);
			m_imgAppIcon.Image.Source = m_Theme.GetImage(m_sAppIconRes);
			
			m_btnNoSleep = new RscIconButton(m_AppTitleBar, Grid.ColumnProperty, 1, 36, 36, Rsc.Collapsed);
			m_btnNoSleep.Image.Source = m_Theme.GetImage("Images/IcoSm001_ScreenSaver.jpg");
			m_btnNoSleep.Click += new System.Windows.RoutedEventHandler(btnNoSleep_Click);
			
			m_btnTools = new RscIconButton(m_AppTitleBar, Grid.ColumnProperty, 2, 36, 36, Rsc.Collapsed);
			m_btnTools.Image.Source = m_Theme.GetImage("Images/IcoSm001_LauncherMini.jpg");
			m_btnTools.Click += new System.Windows.RoutedEventHandler(m_btnTools_Click);
			
			m_tbTitle = new TextBlock();
			m_tbTitle.Text = AppTitle;
			m_tbTitle.Foreground = new SolidColorBrush(m_Theme.ThemeColors.AppTitleFore);
			m_tbTitle.Margin = new Thickness(5);
			m_tbTitle.TextAlignment = TextAlignment.Center;
			m_tbTitle.SetValue(Grid.ColumnProperty, 3);
			m_AppTitleBar.Children.Add(m_tbTitle);
			
			RscIconButton btnClose = new RscIconButton(m_AppTitleBar, Grid.ColumnProperty, 4, 36, 36, Rsc.Visible);
			btnClose.Image.Source = m_Theme.GetImage("Images/Btn001_Close.jpg");
			btnClose.Click += new System.Windows.RoutedEventHandler(btnClose_Click);
		}

		private void btnNoSleep_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			MessageBox.Show("While this application run, automatic sleep is turned off!");
		}

		private void m_btnTools_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sUri = "/Launcher_AppMini;component/" + "MainPage" + ".xaml";
			
			if( m_sToolsArgs.Length > 0 )
			{
				sUri += "?" + m_sToolsArgs;
			}
			
			m_AppPage.NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}

		private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( OnExit == null )
			{
				MessageBoxResult res = MessageBox.Show( "Forceing app to exit!\r\nUnsaved data will be lost...\r\n\r\n(to cancel press Back)");
				if( res != MessageBoxResult.OK ) return;
			
				m_AppPage.NavigationService.GoBack();
			}
			else
			{
				OnExit( this, new EventArgs() );
			}
		}
		
		private void _BuildAppStatusBar()
		{
			if( m_AppStatusBar == null ) return;
			
			//FIX...
			m_AppStatusBar.Margin = new Thickness(0, 2, -2, 0);
			
			m_AppStatusBar.Background = new SolidColorBrush(m_Theme.ThemeColors.AppStatusBack);
			
			ColumnDefinition cd;
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; m_AppStatusBar.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = new GridLength(2, GridUnitType.Pixel); m_AppStatusBar.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; m_AppStatusBar.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = new GridLength(1, GridUnitType.Star); m_AppStatusBar.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; m_AppStatusBar.ColumnDefinitions.Add(cd);
			
			m_txtSysInf = new TextBoxDenieEdit(true, true, m_AppStatusBar, Grid.ColumnProperty, 0);
			m_txtSysInf.Background = new SolidColorBrush(m_Theme.ThemeColors.AppStatusBack);
			m_txtSysInf.Foreground = new SolidColorBrush(m_Theme.ThemeColors.AppStatusFore);
			m_txtSysInf.FontSize = 16;
			m_txtSysInf.Text = _GetAppMemInfo();
			
			Rectangle rcDiv;
			rcDiv = new Rectangle();
			rcDiv.Fill = new SolidColorBrush(Colors.Black);
			rcDiv.SetValue(Grid.ColumnProperty, 1);
			m_AppStatusBar.Children.Add(rcDiv);
			
			m_txtTRACE = new TextBoxDenieEdit(true, true, m_AppStatusBar, Grid.ColumnProperty, 2);
			m_txtTRACE.Visibility = Rsc.Collapsed;
			m_txtTRACE.Background = new SolidColorBrush(Colors.Brown);
			m_txtTRACE.Foreground = new SolidColorBrush(Colors.Yellow);
			m_txtTRACE.FontSize = 16;
			m_txtTRACE.Text = "";
			
			m_txtStatus = new TextBoxDenieEdit(true, true, m_AppStatusBar, Grid.ColumnProperty, 3);
			m_txtStatus.Background = new SolidColorBrush(m_Theme.ThemeColors.AppStatusBack);
			m_txtStatus.Foreground = new SolidColorBrush(m_Theme.ThemeColors.AppStatusFore);
			m_txtStatus.FontSize = 16;
			m_txtStatus.Text = csStatusDefault;
			
			m_btnNext = new RscIconButton(m_AppStatusBar, Grid.ColumnProperty, 4, 36, 36, Rsc.Visible);
			m_btnNext.Image.Source = m_Theme.GetImage("Images/Btn001_Next.jpg");
			m_btnNext.Click += new System.Windows.RoutedEventHandler(btnNext_Click);
		}

		private void btnNext_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( OnNext == null ) return;
			OnNext( this, new EventArgs() );
		}
		
		private string _GetAppMemInfo()
		{
			string strMem = "";
			
			strMem += "Curr: " + RscUtils.toMBstr(Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
			
			strMem += "\r\n";
			
			strMem += "Max: " + RscUtils.toMBstr(Microsoft.Phone.Info.DeviceStatus.ApplicationMemoryUsageLimit);
			
			strMem += "\r\n";
			
			strMem += "Peek: " + RscUtils.toMBstr(Microsoft.Phone.Info.DeviceStatus.ApplicationPeakMemoryUsage);
			
			return strMem;
		}
		
		public void StartTimer(string sReason, Grid LayoutRoot, int iGridRow, int iMax, bool bCanCancel = false, int iPosStart = 0)
		{
			m_iTmrMax = iMax;
			m_iTmrPos = iPosStart;
			m_sTmrReason = sReason;
			
			m_bTmrCanCancel = bCanCancel;
			m_bTmrCancel = false;
			
			//BUG: Unable to press cancel...
			//this.m_AppPage.IsEnabled = false;
			m_aRcTmr = new List<Rectangle>();
			int iRd = -1;
			foreach( RowDefinition rdTmp in LayoutRoot.RowDefinitions )
			{
				iRd++;
				
				Rectangle rc = new Rectangle();
				rc.Fill = new SolidColorBrush( Colors.Red );
				rc.Opacity = 0.1;
				rc.SetValue(Grid.RowProperty, iRd );
				LayoutRoot.Children.Add( rc );
				
				m_aRcTmr.Add( rc );
			}
			
			m_cnvTmr = new Canvas();
			m_cnvTmr.Width = 100;
			m_cnvTmr.Height = 100;
			m_cnvTmr.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			m_cnvTmr.VerticalAlignment = System.Windows.VerticalAlignment.Center;
			//m_cnvTmr.Background = new SolidColorBrush(Colors.Red);
			m_cnvTmr.SetValue(Grid.RowProperty, iGridRow );
			
			m_grdLayoutRootTmr = LayoutRoot;
			m_grdLayoutRootTmr.Children.Add( m_cnvTmr );
			
			Grid grd = new Grid();
			grd.Width = 100;
			grd.Height = 100;
			grd.Background = new SolidColorBrush(Colors.White);
			RowDefinition rd;
			rd = new RowDefinition(); grd.RowDefinitions.Add( rd );
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grd.RowDefinitions.Add( rd );
			m_cnvTmr.Children.Add( grd );
			
			Image img = new Image();
			//img.Width = 100;
			//img.Height = 100;
			img.Source = m_isTmrWc;
			img.SetValue(Grid.RowProperty, 0 );
			grd.Children.Add( img );
			
			if( bCanCancel )
			{
				Image imgTmrCancel = new Image();
				imgTmrCancel.Width = 36;
				imgTmrCancel.Height = 36;
				imgTmrCancel.Margin = new Thickness(64 + 2, -48 + 2, 2, 2);
				imgTmrCancel.Source = m_Theme.GetImage("Images/Btn001_Close.jpg");
				imgTmrCancel.SetValue(Grid.RowProperty, 0 );
				grd.Children.Add( imgTmrCancel );
				
				//RscIconButton btnTmrCancel = new RscIconButton(grd, Grid.RowProperty, 0, 36, 36, Rsc.Visible, 64, -48);
				RscIconButton btnTmrCancel = new RscIconButton(grd, Grid.RowProperty, 0, 90, 90, Rsc.Visible);
				btnTmrCancel.IsEnabled = true;
				//btnTmrCancel.Image.Source = m_DefaultedImageList.GetImage("Images/Btn001_Close.jpg");
				btnTmrCancel.Click += new System.Windows.RoutedEventHandler(btnTmrCancel_Click);
			}
			
			m_prsTmr = new ProgressBar();
			m_prsTmr.Height = 10;
			m_prsTmr.Margin = new Thickness(-12, 0, -12, 6);
			m_prsTmr.RenderTransformOrigin = new Point(0.5, 0.5);
			CompositeTransform ct = new CompositeTransform();
			ct.ScaleY = -2;
			m_prsTmr.RenderTransform = ct;
			m_prsTmr.Minimum = 0;
			m_prsTmr.Maximum = iMax;
			m_prsTmr.Value = iPosStart;
			m_prsTmr.Width = 100;
			m_prsTmr.SetValue(Grid.RowProperty, 1 );
			grd.Children.Add( m_prsTmr );
			
			//m_tmrApp.Interval = new TimeSpan(1000);
			m_tmrApp.Start();
		}

		private void btnTmrCancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_bTmrCanCancel )
			{
				//TRACE = "Button:\r\nTmrCancel";
				m_bTmrCancel = true;
			}
		}
		
		public bool CancelTimer()
		{
			if( m_cnvTmr == null ) return false;
			
			if( m_bTmrCanCancel )
			{
				//TRACE = "BackKey:\r\nTmrCancel";
				m_bTmrCancel = true;
			}
			
			//MUST RET true, to denie app to close even if non-cancelable-timer!!!
			return true;
		}

		private void m_tmrApp_Tick(object sender, System.EventArgs e)
		{
			m_tmrApp.Stop();
			
			RscAppFrameTimerEventArgs args = new RscAppFrameTimerEventArgs();
			args.Max = m_iTmrMax;
			args.m_iPos = m_iTmrPos;
			args.m_iTmrReTry = m_iTmrReTry;
			args.m_bPosModified = false;
			args.Reason = m_sTmrReason;
			if( m_bTmrCancel )
				args.Reason += "_" + "Cancel";
			args.Completed = false;
			
			m_prsTmr.Maximum = m_iTmrMax + m_iTmrReTry;
			m_prsTmr.Value = m_iTmrPos + m_iTmrReTry;
			
			if( OnTimer == null )
			{
				args.Completed = true;
			}
			else
			{
				OnTimer( this, args );
			}
			
			//OnTimer can modify values...
			m_iTmrMax = args.Max;
			m_iTmrReTry = args.m_iTmrReTry;
			if( args.Pos == m_iTmrPos && !args.m_bPosModified )
			{
				m_iTmrPos++;
			}
			else
			{
				m_iTmrPos = args.Pos;
			}
			
			//Possibly changed...
			m_prsTmr.Maximum = m_iTmrMax + m_iTmrReTry;
			m_prsTmr.Value = m_iTmrPos + m_iTmrReTry;
			
			if( args.Completed || (m_iTmrPos > m_iTmrMax) || m_bTmrCancel )
			{
				
				m_grdLayoutRootTmr.Children.Remove( m_cnvTmr );
				
				//BUG: Unable to press cancel...
				//this.m_AppPage.IsEnabled = true;
				for( int iRc = 0; iRc < m_aRcTmr.Count; iRc++ )
				{
					m_grdLayoutRootTmr.Children.Remove( m_aRcTmr[ iRc ] );
				}
				m_aRcTmr.Clear();
				m_aRcTmr = null;
				
				m_prsTmr = null;
				m_cnvTmr = null;
				m_grdLayoutRootTmr = null;
				
				return;
			}
			
			m_tmrApp.Start();
		}
		
		public void AutoClick( Button btn, RoutedEventHandler eh )
		{
			m_btnAutoClick = btn;
			m_ehOnAutoClick = eh;
			
			m_tmrAutoClick.Start();
		}

		private void m_tmrAutoClick_Tick(object sender, System.EventArgs e)
		{
			m_tmrAutoClick.Stop();
			
			if( m_ehOnAutoClick != null )
			{
				m_ehOnAutoClick( m_btnAutoClick, new RoutedEventArgs() );
			}
		}
		
	}
	
	public class RscAppFrameTimerEventArgs : EventArgs
    {
		
		public int Max;
		
		public int m_iPos;
		public bool m_bPosModified;
		
		public int m_iTmrReTry;
		
		public string Reason;
		
		public bool Completed;
		
		public RscAppFrameTimerEventArgs()
		{
			Max = -1;
			
			m_iPos = 0;
			m_bPosModified = false;
			
			Completed = false;
		}
		
		public int Pos
		{
			set
			{
				if( value < m_iPos )
				{
					//Pos can be set back, to redo part or all of items...
					//Let ProgressBar not to restart progress indication...
					m_iTmrReTry += (m_iPos - value);
				}
				
				m_iPos = value;
				
				m_bPosModified = true;
			}
			get
			{
				return m_iPos;
			}
		}
		
	}
	
}