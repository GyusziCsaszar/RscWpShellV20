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

using Ressive.Utils;
using Ressive.FrameWork;

namespace Lib_RscSettings
{
	
    public partial class RscFileTypesV10 : PhoneApplicationPage
    {
		
		RscAppFrame m_AppFrame;
		RscIconButton m_btnExpandAll;
		RscIconButton m_btnCollapseAll;
		
		Size m_sContentPanel = new Size(100, 100);
		
		RscTreeLbItemList m_aTI = null;
								
        public RscFileTypesV10()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "File Types 1.0", "Images/IcoSm001_FileTypes.jpg"
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
			
			StringArrayHelper sah = new StringArrayHelper();
			
			string [] asKeys = RscRegistry.GetKeys( HKEY.HKEY_CLASSES_ROOT, "" );
			for( int i = 0; i < asKeys.Length; i++ )
			{
				string sExt = asKeys[ i ];
				if( sExt.IndexOf( "()" ) != 0 ) continue;
				sExt = sExt.Substring( 2 );
				if( sExt.Length == 0 ) continue;
				
				string sGroup = RscRegistry.ReadString( HKEY.HKEY_CLASSES_ROOT,
					"()" + sExt, "Group", "" );
				
				if( sGroup.Length == 0 ) continue;
				
				sah.Add( sGroup );
			}
			
			for( int i = 0; i < sah.m_a.Count; i++ )
			{
				
				TreeLbItem ti = new TreeLbItem( m_aTI, null, "Group", sah.m_a[ i ] );
				m_aTI.Add( ti );
				
			}
			
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
				tiCurrent.Expand();
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
								
				//VERY SLOW!!!
				//m_aTI.PreRefresh();		
											
				PreInserts();

				switch( ContainerID )
				{
					
					case "ViewerApp" :
					{
						string sGroup = Parent.Title;
						
						string sValue;
						TreeLbItem ti;
						
						sValue = RscRegistry.ReadString( HKEY.HKEY_CLASSES_ROOT,
							"Groups\\" + sGroup, "ViewerAppPageName", "<none>" );
						ti = new TreeLbItem( Holder, this, "", "Page Name", sValue );
						Insert( ti );
						
						sValue = RscRegistry.ReadString( HKEY.HKEY_CLASSES_ROOT,
							"Groups\\" + sGroup, "ViewerAppAssyName", "<none>" );
						ti = new TreeLbItem( Holder, this, "", "Assy Name", sValue );
						Insert( ti );
						
						sValue = "No";
						if( RscRegistry.ReadBool( HKEY.HKEY_CLASSES_ROOT,
							"Groups\\" + sGroup, "ViewerAppAllowList", false ) )
							sValue = "Yes";
						ti = new TreeLbItem( Holder, this, "", "Allow List", sValue );
						Insert( ti );
						
						sValue = "No";
						if( RscRegistry.ReadBool( HKEY.HKEY_CLASSES_ROOT,
							"Groups\\" + sGroup, "ViewerAppSendContent", false ) )
							sValue = "Yes";
						ti = new TreeLbItem( Holder, this, "", "Send Content", sValue );
						Insert( ti );
						
						break;
					}
					
					case "Extensions" :
					{
						StringArrayHelper sah = new StringArrayHelper();
						string [] asKeys = RscRegistry.GetKeys( HKEY.HKEY_CLASSES_ROOT, "" );
						for( int i = 0; i < asKeys.Length; i++ )
						{
							string sExt = asKeys[ i ];
							if( sExt.IndexOf( "()" ) != 0 ) continue;
							sExt = sExt.Substring( 2 );
							if( sExt.Length == 0 ) continue;
							
							string sGroup = RscRegistry.ReadString( HKEY.HKEY_CLASSES_ROOT,
								"()" + sExt, "Group", "" );
							
							if( sGroup.Length == 0 ) continue;
							
							if( sGroup.ToUpper().CompareTo( Parent.Title.ToUpper() ) != 0 ) continue;
							
							sah.Add( sExt );
						}
						
						for( int i = 0; i < sah.m_a.Count; i++ )
						{
							TreeLbItem ti = new TreeLbItem( Holder, this, "", "", sah.m_a[ i ] );
							Insert( ti );
						}
						
						break;
					}
					
					case "Group" :
					{
						TreeLbItem ti = new TreeLbItem( Holder, this, "Extensions", "File Extension" );
						Insert( ti );
						
						ti = new TreeLbItem( Holder, this, "ViewerApp", "Viewer App" );
						Insert( ti );
				
						break;
					}
					
				}
				
				base.Expand();
			}
			
		}
		
		class StringArrayHelper
		{
			
			public List<string> m_a = new List<string>();
			
			public StringArrayHelper()
			{
			}
			
			public void Add( string sValue )
			{
				int iInsAt = m_a.Count;
				
				for( int i = 0; i < m_a.Count; i++ )
				{
					int iCmp = m_a[ i ].ToUpper().CompareTo( sValue.ToUpper() );
					if( iCmp == 0 ) return; //Already exists...
					if( iCmp < 0 ) continue;
					iInsAt = i;
					break;
				}
				
				m_a.Insert( iInsAt, sValue );
			}
			
		}
		
    }
	
}
