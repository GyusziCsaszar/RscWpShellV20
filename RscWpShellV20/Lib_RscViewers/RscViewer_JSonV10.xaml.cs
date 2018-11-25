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
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Ressive.Utils;
using Ressive.Store;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;
using Ressive.Formats;

namespace Lib_RscViewers
{
	
    public partial class RscViewer_JSonV10 : PhoneApplicationPage
    {
		
		RscAppFrame m_AppFrame;
	
		RscPageArgsRet m_AppInput;
		
		TextBoxDenieEdit m_txtPath;
		
		int m_iIndex = 0;
		List<string> m_aPathes = new List<string>();
			
		//RscIconButton m_btnPrev;
		RscIconButton m_btnExpandAll;
		RscIconButton m_btnCollapseAll;
		RscIconButton m_btnExtOpen;
		RscIconButton m_btnShare;
		ImageSource m_isInfErrOn;
		ImageSource m_isInfErrOff;
		RscIconButton m_btnErrsOnOff;
		//RscIconButton m_btnDelete;
		//RscIconButton m_btnNext;
		
		/*
		Point m_ptTouchDown;
		bool m_bInThisApp = true;
		bool m_bIsInSwipe = false;
		*/
		
		bool m_bFirstLoad = true;
		
		RscTreeLbItemList m_aTI = null;
		
		string m_sContent_TEMP = "";
		
		RscTextLbItemList m_aLines = null;
		
        public RscViewer_JSonV10()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "JSon Viewer 1.0", "Images/Ico001_Ressive.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			
			TitlePanel.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack );
			
			/*
			m_btnPrev = new RscIconButton(TitlePanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible);
			m_btnPrev.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipPrev.jpg");
			m_btnPrev.Click += new System.Windows.RoutedEventHandler(m_btnPrev_Click);
			*/
			
			m_btnExpandAll = new RscIconButton(TitlePanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible );
			m_btnExpandAll.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_TreeExpand.jpg");
			m_btnExpandAll.Click += new System.Windows.RoutedEventHandler(m_btnExpandAll_Click);
			
			m_btnCollapseAll = new RscIconButton(TitlePanel, Grid.ColumnProperty, 1, 50, 50, Rsc.Visible );
			m_btnCollapseAll.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_TreeCollapse.jpg");
			m_btnCollapseAll.Click += new System.Windows.RoutedEventHandler(m_btnCollapseAll_Click);
			
			m_txtPath = new TextBoxDenieEdit(true, true, TitlePanel, Grid.ColumnProperty, 2);
			m_txtPath.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack ); //Colors.LightGray);
			m_txtPath.Foreground = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightFore ); //Colors.Black);
			m_txtPath.FontSize = 16;
			m_txtPath.Text = "N/A";
			
			m_btnExtOpen = new RscIconButton(TitlePanel, Grid.ColumnProperty, 3, 50, 50, Rsc.Visible);
			m_btnExtOpen.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Open.jpg");
			m_btnExtOpen.Click += new System.Windows.RoutedEventHandler(m_btnExtOpen_Click);
			
			m_isInfErrOn = m_AppFrame.Theme.GetImage("Images/Btn001_InfErrOn.jpg");
			m_isInfErrOff = m_AppFrame.Theme.GetImage("Images/Btn001_InfErrOff.jpg");
			m_btnErrsOnOff = new RscIconButton(TitlePanel, Grid.ColumnProperty, 4, 50, 50, Rsc.Collapsed );
			m_btnErrsOnOff.Image.Source = m_isInfErrOn;
			m_btnErrsOnOff.Click += new System.Windows.RoutedEventHandler(m_btnErrsOnOff_Click);
			
			/*
			m_btnDelete = new RscIconButton(TitlePanel, Grid.ColumnProperty, 4, 50, 50, Rsc.Visible);
			m_btnDelete.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Delete.jpg");
			m_btnDelete.Click += new System.Windows.RoutedEventHandler(m_btnDelete_Click);
			*/
			
			m_btnShare = new RscIconButton(TitlePanel, Grid.ColumnProperty, 5, 50, 50, Rsc.Visible);
			m_btnShare.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Share.jpg");
			m_btnShare.Click += new System.Windows.RoutedEventHandler(m_btnShare_Click);
			
			/*
			m_btnNext = new RscIconButton(TitlePanel, Grid.ColumnProperty, 6, 50, 50, Rsc.Visible);
			m_btnNext.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipNext.jpg");
			m_btnNext.Click += new System.Windows.RoutedEventHandler(m_btnNext_Click);
			*/
			
			lbLines.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.TextDarkBack );
			lbLines.Foreground = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.TextDarkFore );
			
			m_aLines = new RscTextLbItemList( lbLines, m_AppFrame.Theme );
			
			m_AppFrame.ShowButtonNext( false );

			/*
			Touch.FrameReported += new System.Windows.Input.TouchFrameEventHandler(Touch_FrameReported);
			m_ptTouchDown = new Point(0,0);
			*/
			
			m_aTI = new RscTreeLbItemList( lbTree, m_AppFrame.Theme, "Images/Btn001_TreeExpand.jpg", "Images/Btn001_TreeCollapse.jpg");
       	}
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			//m_bInThisApp = true;
			
			if( !m_bFirstLoad ) return;
			m_bFirstLoad = false;
			
			RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
			m_AppInput = appArgsMgr.GetInput( "RscViewer_JSonV10" );
			if( m_AppInput != null )
			{
				
				m_AppFrame.AppTitle = m_AppInput.CallerAppTitle;
				m_AppFrame.AppIconRes = m_AppInput.CallerAppIconRes;
				
				m_iIndex = 0;
				if( !Int32.TryParse( m_AppInput.GetFlag(0), out m_iIndex ) ) return;
					
				m_aPathes.Clear();
				for( int i = 0; i < m_AppInput.DataCount; i++ )
				{
					string sPath = m_AppInput.GetData( i );
					
					m_aPathes.Add( sPath );
				}
				if( m_aPathes.Count == 0 ) return;
					
				m_iIndex = Math.Min( m_iIndex, m_AppInput.DataCount - 1);
				m_iIndex = Math.Max( m_iIndex, 0 );
				
				//appArgsMgr.Vipe();
				
				_ReadTextFile();
			}
		}
		
		protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
    	{
			//m_bInThisApp = false;
		}
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Next" );
				appOutput.SetOutput();
			}
			
			this.NavigationService.GoBack();
		}
		
		private void m_AppFrame_OnExit(object sender, EventArgs e)
		{
			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Exit" );
				appOutput.SetOutput();
			}
			
			this.NavigationService.GoBack();
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Back" );
				appOutput.SetOutput();
			}
			
			//e.Cancel = true;
        }
		
		/*
		private void Touch_FrameReported(object sender, System.Windows.Input.TouchFrameEventArgs e)
        {
			if( !m_bInThisApp ) return;
			
			TouchPoint primaryTouchPoint = e.GetPrimaryTouchPoint(null);
			if( primaryTouchPoint == null ) return;
			
			switch( primaryTouchPoint.Action )
			{
				
				case TouchAction.Down :
					m_ptTouchDown = primaryTouchPoint.Position;
					m_bIsInSwipe = false;
					break;
					
				case TouchAction.Move :
					
					m_bIsInSwipe = IsInSwipe( primaryTouchPoint.Position ) != 0;
					
					if( scrl != null )
					{
						double dCX = 0;
						
						if( ContentPanel.ActualWidth < ContentPanel.ActualHeight )
							dCX = primaryTouchPoint.Position.X - m_ptTouchDown.X;
						else
							dCX = primaryTouchPoint.Position.Y - m_ptTouchDown.Y;
						
						//Smaller movement...
						dCX = dCX / 5;
							
						scrl.Margin = new Thickness(dCX, 0, -dCX, 0);
					}
					
					break;
					
				case TouchAction.Up :
					
					if( scrl != null )
					{
						scrl.Margin = new Thickness(0);
					}
					
					int iSwipe = IsInSwipe( primaryTouchPoint.Position );
					
					if( iSwipe < 0 )
						DoPrev();
					else if( iSwipe > 0 )
						DoNext();
					
					break;
			}			
		}
		
		private int IsInSwipe(Point ptPosition)
		{
			
			int iDistance = 0;
			int iDistanceOther = 0;
			
			if( ContentPanel.ActualWidth < ContentPanel.ActualHeight )
			{
				iDistance = (int) (ptPosition.X - m_ptTouchDown.X);
				iDistanceOther = (int) (ptPosition.Y - m_ptTouchDown.Y);
			}
			else
			{
				iDistance = (int) (ptPosition.Y - m_ptTouchDown.Y);
				iDistanceOther = (int) (ptPosition.X - m_ptTouchDown.X);
			}
			if( iDistanceOther < 0 ) iDistanceOther *= -1;
			
			//SetStatus(iDistanceOther.ToString());
			
			if( iDistance > 50 && iDistanceOther < 200 )
			{
				//lblStatus.Text = "...to the Right (" + iDistance.ToString() + ")...";
				//DoPrev();
				return -1;
			}
			else if( iDistance < -50 && iDistanceOther < 200 )
			{
				//lblStatus.Text = "...to the Left (" + iDistance.ToString() + ")...";
				//DoNext();
				return 1;
			}
			
			return 0;
		}
			
		private void m_btnPrev_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoPrev();
		}
		
		private void DoPrev()
		{
			if( m_aPathes.Count == 0 ) return;
			
			if( m_iIndex > 0 ) m_iIndex--;
			
			_ReadTextFile();
		}
			
		private void m_btnNext_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoNext();
		}
		
		private void DoNext()
		{
			if( m_aPathes.Count == 0 ) return;
			
			if( m_iIndex < (m_aPathes.Count - 1) ) m_iIndex++;
			
			_ReadTextFile();
		}
		*/
		
		private void m_btnExpandAll_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_AppFrame.StartTimer( "expand_all", LayoutRoot, 1, 0, false );
		}
		
		private void m_btnCollapseAll_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_aTI.CollapseAll( false );
			
			m_AppFrame.StatusText = ""; //To refresh mem info...
		}
		
		private void _ReadTextFile()
		{
			/*
			if( m_iIndex == 0 )
				m_btnPrev.Visibility = Rsc.Collapsed;
			else
				m_btnPrev.Visibility = Rsc.Visible;
			
			if( m_iIndex >= (m_aPathes.Count - 1) )
				m_btnNext.Visibility = Rsc.Collapsed;
			else
				m_btnNext.Visibility = Rsc.Visible;
			*/
			
			m_btnShare.Visibility = Rsc.ConditionalVisibility( m_aPathes.Count > 0 );
			
			if( m_aPathes.Count == 0 )
			{
				m_AppFrame.StatusText = "0 of 0";
				m_txtPath.Text = "";
				
				m_aLines.Clear();
				
				return;
			}
			
			m_AppFrame.StatusText = (m_iIndex + 1).ToString() + " of " + m_aPathes.Count.ToString();
			
			string sPath = m_aPathes[ m_iIndex ];
			
			m_txtPath.Text = sPath;
			
			RscStore store = new RscStore();
			
			bool bNotExist = false;
			m_sContent_TEMP = store.ReadTextFile( sPath, "", out bNotExist );
			
			if( m_sContent_TEMP.Length == 0 )
				return;
			
			m_aLines.Clear();
			
			m_AppFrame.StartTimer( "load", LayoutRoot, 1, 0, false );			
		}
		
		private void m_AppFrame_OnTimer(object sender, RscAppFrameTimerEventArgs e)
		{
			switch( e.Reason )
			{
				case "load" :
				{
					if( e.Pos == e.Max )
					{
						LoadContent( );
					}
					
					break;
				}
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
		
		private void LoadContent( )
		{
			string sContent = m_sContent_TEMP;
			m_sContent_TEMP = "";
			
			m_aLines.Text = sContent;
			
			string sErr = "";
			RscJSonItem json = RscJSon.FromResponseContetn( sContent, out sErr );
			if( sErr.Length > 0 )
			{
				LogError( sErr );
			}
				
			RscViewer_JSon_TreeLbItem ti = new RscViewer_JSon_TreeLbItem( m_aTI, null );
			m_aTI.Add( ti );
			
			ti.SetJSon( json );
		}
			
		private void m_btnShare_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_aPathes.Count == 0 ) return;
			
			Microsoft.Phone.Tasks.EmailComposeTask eml = new Microsoft.Phone.Tasks.EmailComposeTask();
		
			eml.Subject = m_aPathes[ m_iIndex ];
			eml.Body = m_aLines.Text;
		
			eml.Show();
		}
			
		/*
		private void m_btnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_aPathes.Count == 0 ) return;
			
			if( MessageBoxResult.OK == MessageBox.Show( "Do you really want to delete file?\n\n(Press Back to Cancel...)" ) )
			{
				RscStore store = new RscStore();
				
				store.DeleteFile( m_aPathes[ m_iIndex ] );
				
				m_aPathes.RemoveAt( m_iIndex );
				if( m_iIndex >= m_aPathes.Count )
					m_iIndex = Math.Max( 0, m_iIndex - 1 );
				
				_ReadTextFile();
			}
		}
		*/
		
		private void m_btnExtOpen_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_aPathes.Count == 0 ) return;
			
			string sErr = "";
			
			if( !RscStore_Storage.LaunchFile( m_aPathes[ m_iIndex ], out sErr ) )
			{
				if( sErr.Length > 0 )
					MessageBox.Show( sErr );
				else
					MessageBox.Show( "No app installed to open this file." );
			}
		}
		
		private void TreeLbItem_BtnExpCol_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoExpColl( sender );
			
			m_AppFrame.StatusText = ""; //To refresh mem info...
		}
		private void TreeLbItem_Title_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoExpColl( sender );
			
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
		
		private void DoExpColl( object sender )
		{
			Button btn = (Button) sender;	
			RscViewer_JSon_TreeLbItem ti = (RscViewer_JSon_TreeLbItem) btn.Tag;
			
			if( ti.Expanded )
			{
				ti.Collapse();
			}
			else
			{
				if( ti.HasJSon )
				{
					ti.Expand();
				}
			}
		}
		
		private void m_btnErrsOnOff_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( logGrid.Visibility != Rsc.Visible )
			{
				m_btnErrsOnOff.Visibility = Rsc.Visible;
				m_btnErrsOnOff.Image.Source = m_isInfErrOn;
				
				logGrid.Visibility = Rsc.Visible;
			}
			else
			{
				//m_btnErrsOnOff.Visibility = Rsc.Collapsed;
				m_btnErrsOnOff.Image.Source = m_isInfErrOff;
				
				logGrid.Visibility = Rsc.Collapsed;
			}
		}
		
		private void LogError( string sEvent )
		{
			if( logGrid.Visibility != Visibility.Visible )
			{
				m_btnErrsOnOff.Visibility = Rsc.Visible;
				m_btnErrsOnOff.Image.Source = m_isInfErrOn;
				
				logGrid.Visibility = Rsc.Visible;
			}
			
			string [] astr = sEvent.Split('\n');
			
			int iInsert = 0;
			foreach( string s in astr )
			{
				string sA = s;
				string sB = "";
				if( sA.Length > 40 )
				{
					sB = " ..." + sA.Substring( 40 );
					sA = sA.Substring( 0, 40 ) + "...";
				}
				
				lstLog.Items.Insert(iInsert, sA);
				if( sB.Length > 0 )
				{
					lstLog.Items.Insert( iInsert + 1, sB );
					iInsert++;
				}
				
				iInsert++;
			}
			
			lstLog.Items.Insert( 0, "-----------------------" );
		}
		
		private void TextLbItem_BtnLine_Click(object sender, System.Windows.RoutedEventArgs e)
		{
		}
		
    }
	
}
