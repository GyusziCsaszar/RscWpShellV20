using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Lib_RscIPgC_Dlgs.Resources;

using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.ComponentModel;

using Ressive.FrameWork;

namespace Lib_RscIPgC_Dlgs
{
	
    public partial class MainPage : PhoneApplicationPage
    {
		
		const string csAssyName = "/Lib_RscIPgC_Dlgs;component/";

		private void AddApps( )
		{
			// //
			//
			
			AddApp( "RscDlg_BigInputV10", "Big Input Dlg v1.0",
				"INFO: Dlg for multi line Text Input." );
			
			AddApp( "RscDlg_FtpHostInputV10", "FTP Host Input Dlg v1.0",
				"INFO: Dlg for FTP Host Input." );
			
			AddApp( "RscDlg_FolderInputV10", "Folder Input Dlg v1.0",
				"INFO: Dlg for Folder Path Input." );
			
			AddApp( "RscDlg_TxtInputV10", "Txt Input Dlg v1.0",
				"INFO: Dlg for single line Text Input." );
			
			AddApp( "RscDlg_MsgBoxV10", "MessageBox Dlg v1.0",
				"INFO: Dlg for Messages." );
			
			AddApp( "RscDlg_TestAppV10", "RscDlg Tester App v1.0",
				"INFO: App to Test Dlgs." );
			
			//
			// //
		}
		
		RscAppFrame m_AppFrame;
		
		MyVirtualList m_apps = new MyVirtualList();
		
		public MainPage()
        {
            InitializeComponent();
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "RscLib Dlgs", "Images/Ico001_Ressive.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			
			m_apps.ListBoxAsteriskWidth = 100;
			lbApps.ItemsSource = m_apps;
			lbApps.SizeChanged += new System.Windows.SizeChangedEventHandler(lbApps_SizeChanged);
			
			m_AppFrame.ShowButtonNext( false );
			
			this.Loaded +=new System.Windows.RoutedEventHandler(RscFindFilesV10_Loaded);
		}

		private void RscFindFilesV10_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			//Page Navigation Back calls this...
			
			m_apps.Clear( );
			AddApps( );
		}

		private void lbApps_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			int iListBoxAsteriskWidth = (int) (e.NewSize.Width - 10);
			//ATT!!! - Otherwise slowdown...
			if( m_apps.ListBoxAsteriskWidth != iListBoxAsteriskWidth )
			{
				m_apps.ListBoxAsteriskWidth = iListBoxAsteriskWidth;
				
				if( m_apps.Count > 0 )
				{
					//ReQuery...
					lbApps.ItemsSource = null;
					lbApps.ItemsSource = m_apps;
				}
			}
		}
		
		private void AddApp( string sName, string sDesc, string sChReason )
		{
			AppInfo app = new AppInfo() { Name = sName, Description = sDesc, ChangeReason = sChReason };
			app.Parent = m_apps;
			m_apps.Add( app );
			
			if( m_apps.Count % 2 > 0 )
				app.BkColor = m_AppFrame.Theme.ThemeColors.ListZebraBack1;
			else
				app.BkColor = m_AppFrame.Theme.ThemeColors.ListZebraBack2;
		}
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			NavigationService.GoBack();
		}
 
		private void m_AppFrame_OnExit(object sender, System.EventArgs e)
		{
			NavigationService.GoBack();
		}
		
       	protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			//e.Cancel = true;
		}

		private void btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			AppInfo ai;
			ai = (AppInfo) btn.Tag;
			
			int idx = m_apps.IndexOf( ai );
			if( lbApps.SelectedIndex != idx )
				lbApps.SelectedIndex = idx;
			
			string sUri = csAssyName + ai.Name + ".xaml";
			this.NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}
		
    }
	
	public class AppInfo
	{
		
		public AppInfo This { get { return this; } }
		public MyVirtualList Parent { set; get; }
		public int ListBoxAsteriskWidth { get{ return Parent.ListBoxAsteriskWidth; } }
		
		public Color BkColor{ get; set; }
		
		public string Name { get; set; }
		public string Description { get; set; }
		public string ChangeReason { get; set; }
		
		public Brush BkBrush
		{
			get{ return new SolidColorBrush( BkColor ); }
		}
	}
	
    public class MyVirtualList : ObservableCollection<AppInfo>, IList<AppInfo>, IList
    {
		public int ListBoxAsteriskWidth { set; get; }
	}
	
}
