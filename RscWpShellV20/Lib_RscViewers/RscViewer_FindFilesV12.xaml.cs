using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using System.ComponentModel;

using Ressive.Utils;
using Ressive.Theme;
using Ressive.Store;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

namespace Lib_RscViewers
{
	
    public partial class RscViewer_FindFilesV12 : PhoneApplicationPage
    {
		
		const string csDlgsAssy = "Lib_RscIPgC_Dlgs";
		
		RscAppFrame m_AppFrame;
	
		RscPageArgsRetManager m_AppArgs;
		
		RscIconButton m_btnRootFldrBrowse;
		TextBoxDenieEdit m_txtRootFldr;
		RscIconButton m_btnRootFldrDelete;
		DispatcherTimer m_tmrBrowse;
		
		DispatcherTimer m_tmrRename;
		
		RscIconButton m_btnCycle;
		RscIconButton m_btnDelete;
			
		RscPageArgsRet m_AppInput;
		
		MyVirtualList_FindFilesV12 m_folders = new MyVirtualList_FindFilesV12();
		MyVirtualList_FindFilesV12 m_files = new MyVirtualList_FindFilesV12();
		
		bool m_bListOnLoad = false;
		
		string m_sCopyMove = "";
		DispatcherTimer m_tmrCopyMove;
		string m_sCopyMoveDest = "";
		
		Size m_sContentPanel = new Size(100, 100);
		
		string [] m_asAutoOpPathes = null;
		int m_iAutoOpPath = -1;
		string m_sAutoOperation = "";
		
        public RscViewer_FindFilesV12()
        {
            InitializeComponent();
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Find Files 1.2", "Images/IcoSm001_FindFiles.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			
			// //
			
			m_btnRootFldrBrowse = new RscIconButton(rootFldrGrid, Grid.ColumnProperty, 1, 50, 50, Rsc.Visible);
			m_btnRootFldrBrowse.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_(dir).jpg");
			m_btnRootFldrBrowse.Click += new System.Windows.RoutedEventHandler(m_btnRootFldrBrowse_Click);
			
			m_txtRootFldr = new TextBoxDenieEdit(true, true, rootFldrGrid, Grid.ColumnProperty, 2);
			m_txtRootFldr.Background = new SolidColorBrush(Colors.LightGray);
			m_txtRootFldr.Foreground = new SolidColorBrush(Colors.Black);
			m_txtRootFldr.FontSize = 16;
			m_txtRootFldr.MarginOffset = new Thickness( 10, 7, 10, 7 );
			m_txtRootFldr.Text = "";
			
			m_btnRootFldrDelete = new RscIconButton(rootFldrGrid, Grid.ColumnProperty, 3, 50, 50, Rsc.Visible);
			m_btnRootFldrDelete.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Delete.jpg");
			m_btnRootFldrDelete.Click +=new System.Windows.RoutedEventHandler(m_btnRootFldrDelete_Click);
			
			// //
			
			ActionPanel.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack );
			
			// //
			
			imgRename.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			
			imgCopy.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			
			imgMove.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			
			m_btnDelete = new RscIconButton(ActionPanel, Grid.ColumnProperty, 6, 50, 50, Rsc.Visible);
			m_btnDelete.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Delete.jpg");
			m_btnDelete.Click +=new System.Windows.RoutedEventHandler(m_btnDelete_Click);
			
			//To cycle AutoOperation...
			m_btnCycle = new RscIconButton(ActionPanel, Grid.ColumnProperty, 7, 50, 50, Rsc.Collapsed);
			//m_btnCycle.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Delete.jpg");
			m_btnCycle.Click +=new System.Windows.RoutedEventHandler(m_btnCycle_Click);
			
			// //
			
			m_AppArgs = new RscPageArgsRetManager();
			
			m_files.ListBoxAsteriskWidth = 100;
			lbFiles.ItemsSource = m_files;
			lbFiles.SizeChanged += new System.Windows.SizeChangedEventHandler(lbFiles_SizeChanged);
			
			m_tmrBrowse = new DispatcherTimer();
			m_tmrBrowse.Interval = new TimeSpan(500);
			m_tmrBrowse.Tick += new System.EventHandler(m_tmrBrowse_Tick);
			
			m_tmrRename = new DispatcherTimer();
			m_tmrRename.Interval = new TimeSpan(500);
			m_tmrRename.Tick += new System.EventHandler(m_tmrRename_Tick);
				
			m_tmrCopyMove = new DispatcherTimer();
			m_tmrCopyMove.Interval = new TimeSpan(500);
			m_tmrCopyMove.Tick += new System.EventHandler(m_tmrCopyMove_Tick);
		
			txFilter.Text = "*.*";
			chbRecurse.IsChecked = true;
			
			RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
			m_AppInput = appArgsMgr.GetInput( "RscViewer_FindFilesV12" );
			
			if( m_AppInput != null )
			{
				
				m_AppFrame.AppTitle = m_AppInput.CallerAppTitle;
				m_AppFrame.AppIconRes = m_AppInput.CallerAppIconRes;
				
				string sInput = m_AppInput.GetData(0);
				txFilter.Text = m_AppInput.GetData(1);
				if( txFilter.Text.Length == 0 ) txFilter.Text = "*.*";
				chbRecurse.IsChecked = (m_AppInput.GetData(2) == "recurse");
				chbShowHidden.IsChecked = (m_AppInput.GetData(3) == "showHidden");
				
				m_sAutoOperation = m_AppInput.GetData(4);
				if( m_sAutoOperation.Length > 0 )
				{
					m_asAutoOpPathes = sInput.Split( ';' );
					m_iAutoOpPath = 0;
					m_txtRootFldr.Text = m_asAutoOpPathes[ m_iAutoOpPath ];

					filterPanel.Visibility = Rsc.Collapsed;
					ActionPanel.Visibility = Rsc.Collapsed;
				}
				else
				{
					m_txtRootFldr.Text = sInput;
				}
				
				m_bListOnLoad = true;
			}
			
			this.Loaded +=new System.Windows.RoutedEventHandler(RscFindFilesV10_Loaded);
			
			txFilter.TextChanged += new System.Windows.Controls.TextChangedEventHandler(txFilter_TextChanged);
			chbRecurse.Checked += new System.Windows.RoutedEventHandler(chbRecurse_Checked);
			
			ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);
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

		private void RscFindFilesV10_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			//Page Navigation Back calls this...
			
			/*
			if( m_tasks.Count == 0 )
			{
				ListFiles();
			}
			*/
			
			if( m_bListOnLoad )
			{
				m_bListOnLoad = false;
				
				ListFiles();
			}
		}		
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if( m_AppArgs.Waiting )
			{
				RscPageArgsRet appOutput = m_AppArgs.GetOutput();
				if( appOutput != null )
				{
					switch( appOutput.ID )
					{
						
						case "RootFldrPath" :
						{
							if( appOutput.GetFlag(0) == "Ok" )
							{
								m_txtRootFldr.Text = appOutput.GetData(0);
								ListFiles();
							}
							else
							{
								//NOP...
							}
							break;
						}
						
						case "CopyMove" :
						{
							if( appOutput.GetFlag(0) == "Ok" )
							{
								m_sCopyMoveDest = appOutput.GetData(0);
								
								if( chbRecurse.IsChecked.Value && (m_txtRootFldr.Text.Length == 0 || m_sCopyMoveDest.ToUpper().IndexOf( m_txtRootFldr.Text.ToUpper() ) == 0 ))
								{
									MessageBox.Show( "Unable to " + m_sCopyMove + " from parent folder to its' child folder." );
								}
								else
								{
									m_AppFrame.SetStatusText( m_sCopyMove + "..." );
									m_AppFrame.StartTimer( "copy move files", LayoutRoot, 1, 0, true );			
								}
							}
							else
							{
								//NOP...
							}
							break;
						}
						
						case "txRenameTo" :
						{
							if( appOutput.GetFlag(0) == "Ok" )
							{
								RscFileItemDesc it = m_files[ 0 ];
								
								string sNewName = appOutput.GetData(0);
								
								if( it.strFileName.ToUpper().CompareTo( sNewName.ToUpper() ) != 0 )
								{
									
									RscStore store = new RscStore();
									
									string sNewPath = it.strParent;
									if( sNewPath.Length > 0 ) sNewPath += "\\";
									sNewPath += sNewName;
									
									if( store.FileExists( sNewPath ) )
									{
										MessageBox.Show( "Name already in use!\r\n\r\n" + sNewPath );
									}
									else
									{
										if( RscStore.ExtensionOfPath( sNewPath ).ToUpper().CompareTo( RscStore.ExtensionOfPath( it.Path ).ToUpper() ) != 0 )
										{
											if( MessageBoxResult.OK != MessageBox.Show( "Do you really want to change type from " + RscStore.ExtensionOfPath( it.Path )
												+ " to " + RscStore.ExtensionOfPath( sNewPath ) + "?\r\n\r\n(press Back to cancel)" ) )
												return;
										}
										
										try
										{
											store.MoveFile( it.Path, sNewPath );
										}
										catch( Exception exc )
										{
											MessageBox.Show( "Rename failed!\r\n\r\n" + exc.Message );
											return;
										}
										
										if( txFilter.Text == it.strFileName )
											txFilter.Text = sNewName;
										it.strFileName = sNewName;
										
										//ReQuery...
										lbFiles.ItemsSource = null;
										lbFiles.ItemsSource = m_files;
									}
								}
							}
							else
							{
								//NOP...
							}
							break;
						}
							
					}
				}
				
				m_AppArgs.Clear();
			}
		}

		private void lbFiles_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			int iListBoxAsteriskWidth = (int) (e.NewSize.Width - 10);
			//ATT!!! - Otherwise slowdown...
			if( m_files.ListBoxAsteriskWidth != iListBoxAsteriskWidth )
			{
				m_files.ListBoxAsteriskWidth = iListBoxAsteriskWidth;
				
				if( m_files.Count > 0 )
				{
					//ReQuery...
					lbFiles.ItemsSource = null;
					lbFiles.ItemsSource = m_files;
				}
			}
		}
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			ListFiles();
		}
 
		private void m_AppFrame_OnExit(object sender, System.EventArgs e)
		{
			PrepareExit();
			
			NavigationService.GoBack();
		}
		
       	protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( m_AppFrame.CancelTimer() )
			{
				e.Cancel = true;
				return;
			}
			
			PrepareExit();
		}
		
		private void PrepareExit()
		{
			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Ok" );
				appOutput.SetOutput();
			}
		}

		private void ListFiles()
		{
			
			ActionPanel.Visibility = Rsc.Collapsed;
			
			//problems with txFilter_TextChanged!!
			/*
			if( txFilter.Text.Length == 0 )
				txFilter.Text = "*.*";
			*/
			
			RscFileItemDesc itCurrent;
			
			m_folders.Clear();
			m_files.Clear();
			
			itCurrent = new RscFileItemDesc( m_AppFrame.Theme );
			itCurrent.bFolder = true;
			itCurrent.bWalked = false;
			itCurrent.strParent = "";
			itCurrent.strFileName = m_txtRootFldr.Text;
			itCurrent.Parent = m_folders;
			
			RscStore store = new RscStore();
			if( !store.FolderExists( itCurrent.Path ) )
			{
				itCurrent.bWalked = true;
				itCurrent.LastError = "Not exist!";
			}
			
			m_folders.Add(itCurrent);
			
			m_AppFrame.SetStatusText( "Listing files..." );
			m_AppFrame.StartTimer( "list files", LayoutRoot, 1, 0, true );			
		}
		
		private void m_AppFrame_OnTimer(object sender, RscAppFrameTimerEventArgs e)
		{
			switch( e.Reason )
			{
				
				case "list files_Cancel" :
				{
					m_folders.Clear();
					m_files.Clear();
					m_AppFrame.SetStatusText( "User canceled operation!", StatusColoring.Error );
					break;
				}
				
				case "list files" :
				{
					
					string sFileFilter = txFilter.Text;
					if( sFileFilter.Length == 0 )
						sFileFilter = "*.*";
					sFileFilter = sFileFilter.Replace("\\", "");
					sFileFilter = sFileFilter.Replace("/", "");
					sFileFilter = sFileFilter.Replace(":", "");
					sFileFilter = sFileFilter.Replace(('"').ToString(), "");
					sFileFilter = sFileFilter.Replace("<", "");
					sFileFilter = sFileFilter.Replace(">", "");
					sFileFilter = sFileFilter.Replace("|", "");
					
					RscFileItemDesc itCurrent;
					RscFileItemDesc it;
					
					RscStore store = new RscStore();
					
					itCurrent = m_folders[ e.Pos ];
					
					if( !itCurrent.bWalked )
					{
						if( chbRecurse.IsChecked.Value )
						{
							string[] fldrs = RscSort.OrderBy(store.GetFolderNames(itCurrent.Path, "*.*", chbShowHidden.IsChecked.Value));
							foreach(string node in fldrs)
							{
								it = new RscFileItemDesc( m_AppFrame.Theme );
								
								it.bFolder = true;
								it.bWalked = false;
								
								it.strParent = itCurrent.Path;
								it.strFileName = node;
								
								e.Max++;
								it.Parent = m_folders;
								m_folders.Add(it);				
							}
						}
						
						itCurrent.bWalked = true;
						
						if( e.Pos == e.Max )
						{
							e.Pos = 0;
						}
					}
					else
					{
						if( itCurrent.LastError.Length == 0 ) //Otherwise Not Exist!!!
						{
							string[] fles = RscSort.OrderBy(store.GetFileNames(itCurrent.Path, sFileFilter, chbShowHidden.IsChecked.Value));
							foreach(string node in fles)
							{								
								it = new RscFileItemDesc( m_AppFrame.Theme );
								
								it.bFolder = false;
								it.bWalked = false;
								
								it.strParent = itCurrent.Path;
								it.strFileName = node;
								
								it.Parent = m_files;
								m_files.Add(it);
							}
						}
						
						if( e.Pos == e.Max )
						{
							//ReQuery...
							lbFiles.ItemsSource = null;
							lbFiles.ItemsSource = m_files;
							
							btnRename.Visibility = Rsc.ConditionalVisibility( m_files.Count == 1 && m_folders.Count == 1 );
							imgRename.Visibility = btnRename.Visibility;
							//
							ActionPanel.Visibility = Rsc.ConditionalVisibility( 
								(m_files.Count > 0 || m_txtRootFldr.Text.Length > 0) && m_folders[0].LastError.Length == 0 );
							m_btnDelete.Visibility = Rsc.ConditionalVisibility( 
								(m_files.Count > 0 || m_folders.Count > 0 /*|| m_txtRootFldr.Text.Length > 0*/) && m_folders[0].LastError.Length == 0 );
							//
							btnCopy.Visibility = Rsc.ConditionalVisibility( 
								(m_files.Count > 0 /*|| m_txtRootFldr.Text.Length > 0*/) && m_folders[0].LastError.Length == 0 );
							imgCopy.Visibility = btnCopy.Visibility;
							//
							btnMove.Visibility = Rsc.ConditionalVisibility( 
								(m_files.Count > 0 /*|| m_txtRootFldr.Text.Length > 0*/) && m_folders[0].LastError.Length == 0 );
							imgMove.Visibility = btnMove.Visibility;
							
							m_AppFrame.SetStatusText(m_files.Count.ToString() + " file(s) in "
								+ m_folders.Count.ToString() + " folder(s) listed" );
			
							// //
							//
							
							if( m_sAutoOperation.Length > 0 )
							{
								ActionPanel.Visibility = Rsc.Collapsed;
								
								switch( m_sAutoOperation )
								{
									case "AutoDelete" :
										m_AppFrame.AutoClick( m_btnDelete, new System.Windows.RoutedEventHandler(m_btnDelete_Click) );
										break;
								}
							}
							
							//
							// //
						}
					}
					
					break;
				}
				
				default :
					m_AppFrame_OnTimer_Sub1( sender, e );
					break;
				
			}
		}

		private void btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			RscFileItemDesc it;
			it = (RscFileItemDesc) btn.Tag;
			
			int idx = m_files.IndexOf( it );
			if( lbFiles.SelectedIndex != idx )
				lbFiles.SelectedIndex = idx;
			
			if( it.bFolder )
			{
				//m_path.Push( it.sFn );
				
				//ListFiles();
			}
		}

		private void m_btnRootFldrDelete_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_txtRootFldr.Text = "";
			
			//problems with txFilter_TextChanged!!!
			//ListFiles();
		}

		private void m_btnRootFldrBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_tmrBrowse.Start();
		}

		private void m_tmrBrowse_Tick(object sender, System.EventArgs e)
		{
			m_tmrBrowse.Stop();

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "RootFldrPath" );
			appInput.SetFlag( 0, "root folder path" );
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, m_txtRootFldr.Text );
			appInput.SetInput( "RscDlg_FolderInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csDlgsAssy ) );
		}

		private void txFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			//BUG-CAUSER!!!
			/*
			if( m_folders.Count > 0 )
				m_folders.Clear();
			
			if( m_files.Count > 0 )
				m_files.Clear();
			*/
			
			ActionPanel.Visibility = Rsc.Collapsed;
		}

		private void chbRecurse_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			//BUG-CAUSER!!!
			/*
			if( m_folders.Count > 0 )
				m_folders.Clear();
			
			if( m_files.Count > 0 )
				m_files.Clear();
			*/
			
			ActionPanel.Visibility = Rsc.Collapsed;
		}
		
		private void btnRename_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_tmrRename.Start();
		}

		private void m_tmrRename_Tick(object sender, System.EventArgs e)
		{
			m_tmrRename.Stop();
			
			if( m_files.Count != 1 ) return;
			RscFileItemDesc it = m_files[0];

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "txRenameTo" );
			appInput.SetFlag( 0, it.strFileName );
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetFlag( 3, "no history" );
			appInput.SetData( 0, it.strFileName );
			appInput.SetInput( "RscDlg_TxtInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csDlgsAssy ) );
		}
		
		
		private void m_btnCycle_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			//AutoOperation Cycle...
			ListFiles();
		}
		
		private void m_btnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_sAutoOperation.Length == 0 )
			{
				if( MessageBoxResult.OK != MessageBox.Show( "Really want to delete files?\r\n\r\n(press Back to cancel)" ) )
					return;
			}
			
			if( m_files.Count == 0 && m_txtRootFldr.Text.Length == 0 ) return;
			
			m_AppFrame.SetStatusText( "Deleting files..." );
			m_AppFrame.StartTimer( "delete files", LayoutRoot, 1, 0, true );			
		}
		
		private void btnCopy_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_sCopyMove = "copy";
			m_tmrCopyMove.Start();
		}
		
		private void btnMove_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_sCopyMove = "move";
			m_tmrCopyMove.Start();
		}

		private void m_tmrCopyMove_Tick(object sender, System.EventArgs e)
		{
			m_tmrCopyMove.Stop();

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "CopyMove" );
			appInput.SetFlag( 0, m_sCopyMove + " to destination folder" );
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, "" );
			appInput.SetInput( "RscDlg_FolderInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csDlgsAssy ) );
		}
				
		private void m_AppFrame_OnTimer_Sub1(object sender, RscAppFrameTimerEventArgs e)
		{
			switch( e.Reason )
			{
				
				case "copy move files_Cancel" :
				{
					m_AppFrame.SetStatusText( "User canceled operation!", StatusColoring.Error );
					break;
				}
				
				case "copy move files" :
				{
					int iRootLen = m_txtRootFldr.Text.Length;
						
					if( e.Pos == 0 )
					{
						foreach( RscFileItemDesc itFldr in m_folders )
						{
							string sPart = itFldr.Path.Substring( iRootLen );
							if( sPart.Length > 0 )
							{
								string sNewFolder = m_sCopyMoveDest;
								if( iRootLen == 0 ) sNewFolder += "\\";
								sNewFolder += sPart;
								
								try
								{
									RscStore store = new RscStore();
									store.CreateFolder( sNewFolder );
								}
								catch( Exception /*exc*/ )
								{
									m_AppFrame.SetStatusText( "ERR: Unable to copy folder stru near: " + sNewFolder, StatusColoring.Error );
									e.Completed = true;
									return;
								}
							}
						}
						
						e.Max = 1;
					}
					else
					{
						RscFileItemDesc it = m_files[e.Pos - 1];
						
						string sErr = "";
						try
						{
							string sCopyMoveTo = m_sCopyMoveDest;
							if( iRootLen == 0 ) sCopyMoveTo += "\\";
							sCopyMoveTo += it.Path.Substring( iRootLen );
							
							RscStore store = new RscStore();
							
							if( m_sCopyMove == "move" )
								store.MoveFileForce( it.Path, sCopyMoveTo );
							else
								store.CopyFileForce( it.Path, sCopyMoveTo );
						}
						catch( Exception exc )
						{
							sErr = "Unable to " + m_sCopyMove + " file.\r\n" + exc.Message;
						}
						
						if( sErr.Length == 0 )
						{
							m_files.RemoveAt( e.Pos - 1 );
							
							if( (e.Pos - 1) < m_files.Count )
							{
								int iPos = e.Pos;
								e.Pos = iPos; //Denie subsys to increment...
							}
							else
								e.Completed = true;
						}
						else
						{
							it.LastError = sErr;
							
							//ReQuery...
							lbFiles.ItemsSource = null;
							lbFiles.ItemsSource = m_files;
							
							if( (e.Pos - 1) < (m_files.Count - 1) )
								e.Max++;
							else
								e.Completed = true;
						}
						
						if( e.Completed )
						{
							if( m_sCopyMove == "move" )
							{
								if( m_files.Count == 0 ) //Otherwise error occured...
								{
									if( chbRecurse.IsChecked.Value ) //Otherwise no sub-tree deletion...
									{
										int iIdx = (m_folders.Count - 1) + 1;
										for(;;)
										{
											iIdx--;
											if( iIdx < 0 ) break;
											
											try
											{
												if( m_folders[ iIdx ].Path.Substring(iRootLen).Length > 0 )
												{
													RscStore store = new RscStore();
													store.DeleteFolder( m_folders[ iIdx ].Path );
												}
												
												m_folders.RemoveAt( iIdx );
											}
											catch( Exception /*exc*/ )
											{
											}
										}
									}
								}
						
								if( m_files.Count == 0 )
									m_AppFrame.SetStatusText("Move succeeded!", StatusColoring.Success);
								else
									m_AppFrame.SetStatusText("Move failed for some item(s)!", StatusColoring.Error);
							}
							else
							{
								if( m_files.Count == 0 )
									m_AppFrame.SetStatusText("Copy succeeded!", StatusColoring.Success);
								else
									m_AppFrame.SetStatusText("Copy failed for some item(s)!", StatusColoring.Error);
							}
							
							//Causes exception...
							//ListFiles();
							
							ActionPanel.Visibility = Rsc.Collapsed;
						}
					}
					
					break;
				}
				
				case "delete files_Cancel" :
				{
					m_AppFrame.SetStatusText( "User canceled operation!", StatusColoring.Error );
					break;
				}
				
				case "delete files" :
				{
					
					if( e.Pos >= m_files.Count )
					{
						e.Completed = true;
					}
					else
					{
						RscFileItemDesc it = m_files[e.Pos];
						
						string sErr = "";
						try
						{
							RscStore store = new RscStore();
							store.DeleteFile( it.Path );
						}
						catch( Exception exc )
						{
							sErr = "Unable to delete file.\r\n" + exc.Message;
						}
						
						if( sErr.Length == 0 )
						{
							m_files.RemoveAt( e.Pos );
							
							if( e.Pos < m_files.Count )
							{
								int iPos = e.Pos;
								e.Pos = iPos; //Denie subsys to increment...
							}
							else
								e.Completed = true;
						}
						else
						{
							it.LastError = sErr;
							
							//ReQuery...
							lbFiles.ItemsSource = null;
							lbFiles.ItemsSource = m_files;
							
							if( e.Pos < (m_files.Count - 1) )
								e.Max++;
							else
								e.Completed = true;
						}
					}
					
					if( e.Completed )
					{
						if( m_files.Count == 0 ) //Otherwise error occured...
						{
							if( chbRecurse.IsChecked.Value ) //Otherwise no sub-tree deletion...
							{
								int iIdx = (m_folders.Count - 1) + 1;
								for(;;)
								{
									iIdx--;
									if( iIdx < 0 ) break;
									
									try
									{
										if( m_folders[ iIdx ].Path.Length > 0 )
										{
											RscStore store = new RscStore();
											store.DeleteFolder( m_folders[ iIdx ].Path );
										}
										
										m_folders.RemoveAt( iIdx );
									}
									catch( Exception /*exc*/ )
									{
									}
								}
							}
						}
						
						if( m_files.Count == 0 )
						{
							m_AppFrame.SetStatusText("Delete succeeded!", StatusColoring.Success);
							
							// //
							//
							
							if( m_sAutoOperation.Length > 0 )
							{
								if( m_iAutoOpPath + 1 < m_asAutoOpPathes.Length )
								{
									m_iAutoOpPath++;
									m_txtRootFldr.Text = m_asAutoOpPathes[ m_iAutoOpPath ];
									m_AppFrame.AutoClick( m_btnCycle, new System.Windows.RoutedEventHandler(m_btnCycle_Click) );
								}
								else
								{
									PrepareExit();
									NavigationService.GoBack();
								}
							}
							
							//
							// //
						}
						else
						{
							m_AppFrame.SetStatusText("Delete failed for some item(s)!", StatusColoring.Error);
						}
						
						//Causes exception...
						//ListFiles();
						
						ActionPanel.Visibility = Rsc.Collapsed;
					}
					
					break;
				}
				
			}
		}
		
    }
	
    public class MyVirtualList_FindFilesV12 : ObservableCollection<RscFileItemDesc>, IList<RscFileItemDesc>, IList
    {
		public int ListBoxAsteriskWidth { set; get; }
	}

	public class RscFileItemDesc
	{
		
		public bool bFolder;
		public bool bWalked; //Folder is walked...
		
		public string strParent;
		public string strFileName;
		
		public string m_sLastError = "";
		
		public RscFileItemDesc This { get{ return this; } }
		public MyVirtualList_FindFilesV12 Parent { set; get; }
		public int ListBoxAsteriskWidth { get{ return Parent.ListBoxAsteriskWidth; } }
		
		public string LastError { get{ return m_sLastError; } set{ m_sLastError = value; } }
		
		public Brush StateBackBrush
		{
			get
			{
				if( LastError.Length > 0 )
				{
					return new SolidColorBrush(Colors.Red);
				}
				else
				{
					return new SolidColorBrush( m_Theme.ThemeColors.TreeDescBack );
				}
			}
		}
		
		public Brush StateForeBrush
		{
			get
			{
				if( LastError.Length > 0 )
				{
					return new SolidColorBrush(Colors.White);
				}
				else
				{
					return new SolidColorBrush( m_Theme.ThemeColors.TreeDescFore );
				}
			}
		}
		
		public Brush DescBackBrush
		{
			get
			{
				return new SolidColorBrush( m_Theme.ThemeColors.TreeDescBack );
			}
		}
		
		public Brush DescForeBrush
		{
			get
			{
				return new SolidColorBrush( m_Theme.ThemeColors.TreeDescFore );
			}
		}
		
		public Brush BackBrush
		{
			get
			{
				if( bFolder )
				{
					return new SolidColorBrush( m_Theme.ThemeColors.TreeContainerBack );
				}
				else
				{
					return new SolidColorBrush(m_Theme.ThemeColors.TreeLeafBack );
				}
			}
		}
		
		public Brush ForeBrush
		{
			get
			{
				if( bFolder )
				{
					return new SolidColorBrush( m_Theme.ThemeColors.TreeContainerFore );
				}
				else
				{
					return new SolidColorBrush(m_Theme.ThemeColors.TreeLeafFore );
				}
			}
		}
		
		public string FileTitle
		{
			get
			{
				if( strFileName.Length == 0 ) return ".";
				return strFileName;
			}
		}
		
		public string Path
		{
			get
			{
				string strPath;
				
				strPath = strParent;
				
				if( strFileName.Length > 0 )
				{
					if( strParent.Length > 0 ) strPath += "\\";
					
					strPath += strFileName;
				}
				
				return strPath;
			}
		}
		
		public string StateTitle
		{
			get
			{
				string strSt = "";
				
				if( bFolder )
				{
					strSt = "folder";
				
					if( !bWalked ) strSt += " (NOT WALKED!!!)";
				}
				else
				{
					strSt = "file";
				}
				
				if( LastError.Length > 0 )
				{
					strSt += "\r\nERROR: ";
					strSt += LastError;
				}
				
				return strSt;
			}
		}
		
		protected RscTheme m_Theme;
		
		public RscFileItemDesc( RscTheme Theme )
		{
			m_Theme = Theme;
		}
		
	}
	
}
