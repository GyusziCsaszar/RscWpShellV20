using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Lib_RscViewers.Resources;

using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.ComponentModel;

using Ressive.FrameWork;

namespace Lib_RscViewers
{
	
    public partial class MainPage : PhoneApplicationPage
    {
		
		const string csAssyName = "/Lib_RscViewers;component/";
		
		private void AddApps( )
		{
			// //
			//
			
			AddApp( "RscViewer_JSonV10", "JSon Viewer v1.0",
				"INFO: JSon Viewer." );
			
			AddApp( "RscViewer_HexaV10", "Hexa Viewer v1.0",
				"INFO: Hexa Viewer for unknown files." );
			
			AddApp( "RscViewer_MedLibV11", "Media Library v1.1",
				"INFO: Media Library." );
			
			AddApp( "RscViewer_SoundV11", "Sound Player v1.1",
				"INFO: Sound Player." );
			
			AddApp( "RscViewer_QueryStringV10", "Query String Viewer v1.0",
				"INFO: Page navigation Uri's Query String Viewer.",
				"?ExitOnBack=True" );
			
			AddApp( "RscViewer_VideoFolderV10", "Video Gallery v1.0",
				"INFO: View a video folder (with \\tn\\ support)." );
			
			AddApp( "RscViewer_PhotoFolderV10", "Photo Gallery v1.0",
				"INFO: View a photo folder (with \\tn\\ support)." );
			
			AddApp( "RscViewer_MediaV10", "Media Viewer v1.0",
				"INFO: View one or more video or media file." );
			
			AddApp( "RscViewer_TextV10", "Text Viewer v1.0",
				"INFO: View one or more text file." );
			
			AddApp( "RscViewer_ImageV10", "Image Viewer v1.0",
				"INFO: View one or more image." );
			
			AddApp( "RscViewer_FsV12", "FileSystem Viewer v1.2",
				"INFO: Browse for folders and files." );
			
			AddApp( "RscViewer_FindFilesV12", "Find Files v1.2",
				"INFO: Filter file list and manage files." );
			
			//
			// //
		}
		
		RscAppFrame m_AppFrame;
		
		MyVirtualList m_apps = new MyVirtualList();
		
        public MainPage()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "RscLib Viewers", "Images/Ico001_Ressive.jpg"
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
		
		private void AddApp( string sName, string sDesc, string sChReason, string sArgs = "" )
		{
			AppInfo app = new AppInfo() { Name = sName, Description = sDesc,
				ChangeReason = sChReason, Args = sArgs };
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
			
			string sUri = csAssyName + ai.Name + ".xaml" + ai.Args;
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
		public string Args { get; set; }
		
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
