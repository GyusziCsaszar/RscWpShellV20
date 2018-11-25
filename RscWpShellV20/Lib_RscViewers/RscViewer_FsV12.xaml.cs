using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Input;

using Ressive.Utils;
using Ressive.Store;
using Ressive.Theme;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

namespace Lib_RscViewers
{
	
    public partial class RscViewer_FsV12 : PhoneApplicationPage
    {
		
		const string csViewersAssy = "Lib_RscViewers";
		const string csIPgC_DlgsAssy = "Lib_RscIPgC_Dlgs";
		
		RscAppFrame m_AppFrame;
		// //////////////////
		ImageSource m_isDriveLg = null;
		ImageSource m_isDriveSm = null;
		ImageSource m_isFolderLg = null;
		ImageSource m_isFolderSm = null;
		ImageSource m_isFolderHiddenLg = null;
		ImageSource m_isFolderHiddenSm = null;
		ImageSource m_isFileLg = null;
		ImageSource m_isFileSm = null;
		ImageSource m_isFileHiddenLg = null;
		ImageSource m_isFileHiddenSm = null;
		// //////////////////
		ImageSource m_isMore = null;
		ImageSource m_isLess = null;
		ImageSource m_isTools = null;
		ImageSource m_isOpen = null;
		//ImageSource m_isDummy = null;
	
		RscPageArgsRetManager m_AppArgs;
		RscPageArgsRet m_AppInput;
		string m_sReturnFileType = "";
		string m_sReturnFileName = "";
	
		class RscFileItemDesc
		{
			
			public int idx;
			
			public bool bFolder;
			public string sFn;
			public string sExt;
			
			public char cInfChr;
			
			public Grid grdDetails;
			public TextBox tbDetails;
			public Button btn;
			public RscIconButton btnMore;
			public Image img;
			public TextBox txt;
			
		}
		
		const int ciLoadAtOnce_Add = 20;
		const int ciLoadAtOnce_ExpAll = 10;
		
        Stack<string> m_path = new Stack<string>();
        List<RscFileItemDesc> m_a = new List<RscFileItemDesc>();
		
		string[] m_TEMP_asFolders;
		int m_TEMP_iCntFolders;
		int m_TEMP_iCntFiles;
		string[] m_TEMP_asFiles;
		string m_TEMP_sSearchPath;
		string m_TEMP_sSearchWildCard;
		int m_TEMP_iIdx = 0;
		
		DispatcherTimer m_tmrEnumExpAll;
		
		DispatcherTimer m_tmrAddFldr;
		
		Size m_sContentPanel = new Size(100, 100);
		
		bool m_bIncludeHidden = false;
		
		bool m_bAppStarted = false;
		
		int m_iArgRootLen = 0;
				
        public RscViewer_FsV12()
        {
            InitializeComponent();
			
 			//StandAlone app...
			bool bStandAloneApp = false;
			Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			if( GlobalDILholder.Tag == null )
			{
				bStandAloneApp = true;
				GlobalDILholder.Tag = new RscTheme( true, "Theme", "Current", "Default" );
			}
			//StandAlone app...
			
			//Register all file-type associations...
			RscFileTypes.RegisterAll();
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Explorer 1.2", "Images/IcoSm001_Explorer4.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			// ///////////////
			
			//Text Button Sample...
			/*
			m_btnCleanUp = new RscIconButton(ToolBarPanel, Grid.ColumnProperty, 4, 50, 50, Rsc.Visible,
				0, 0, "Clean Up" );
			m_btnCleanUp.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			m_btnCleanUp.Click += new System.Windows.RoutedEventHandler(m_btnCleanUp_Click);
			*/
			
			m_isDriveLg = m_AppFrame.Theme.GetImage("Images/Type001_(drv).jpg");
			m_isDriveSm = m_AppFrame.Theme.GetImage("Images/Btn001_(drv).jpg");
			//
			m_isFolderLg = m_AppFrame.Theme.GetImage("Images/Type001_(dir).jpg");
			m_isFolderSm = m_AppFrame.Theme.GetImage("Images/Btn001_(dir).jpg");
			//
			m_isFolderHiddenLg = m_AppFrame.Theme.GetImage("Images/Type001_(dir)_hidden.jpg");
			m_isFolderHiddenSm = m_AppFrame.Theme.GetImage("Images/Btn001_(dir)_hidden.jpg");
			//
			m_isFileLg = m_AppFrame.Theme.GetImage("Images/Type001_().jpg");
			m_isFileSm = m_AppFrame.Theme.GetImage("Images/Btn001_().jpg");
			//
			m_isFileHiddenLg = m_AppFrame.Theme.GetImage("Images/Type001_()_hidden.jpg");
			m_isFileHiddenSm = m_AppFrame.Theme.GetImage("Images/Btn001_()_hidden.jpg");
			// ///////////////
			m_isMore = m_AppFrame.Theme.GetImage("Images/Btn001_DownMore.jpg");
			m_isLess = m_AppFrame.Theme.GetImage("Images/Btn001_UpLess.jpg");
			m_isTools = m_AppFrame.Theme.GetImage("Images/Btn001_Tools.jpg");
			m_isOpen = m_AppFrame.Theme.GetImage("Images/Btn001_Open.jpg");
			//m_isDummy = m_AppFrame.Theme.GetImage("Images/Img001_Dummy.jpg");
			// ///////////////
			imgRootIco.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			imgUpIco.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			imgUsrAdm.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			btnUsrAdm.Content = " adm ";
			m_bIncludeHidden = false;
			imgAddFldr.Source = m_AppFrame.Theme.GetImage("Images/Btn001_NewFolder.jpg");;
			imgExpAll.Source = m_isMore;
			
			ToolBar.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack );
			
			if( bStandAloneApp )
			{
				m_AppFrame.ShowButtonTools( true, "NoExplorer=True" );
			}
			
			this.Loaded +=new System.Windows.RoutedEventHandler(RscExplorerV11_Loaded);
			
			m_tmrEnumExpAll = new DispatcherTimer();
			m_tmrEnumExpAll.Interval = new TimeSpan(100);
			m_tmrEnumExpAll.Tick +=new System.EventHandler(m_tmrEnumExpAll_Tick);
			
			m_tmrAddFldr = new DispatcherTimer();
			m_tmrAddFldr.Interval = new TimeSpan(100);
			m_tmrAddFldr.Tick +=new System.EventHandler(m_tmrAddFldr_Tick);
			
			m_AppArgs = new RscPageArgsRetManager();
			
			m_AppInput = m_AppArgs.GetInput( "RscViewer_FsV12" );
			if( m_AppInput != null )
			{
				m_AppFrame.AppTitle = m_AppInput.CallerAppTitle;
				m_AppFrame.AppIconRes = m_AppInput.CallerAppIconRes;
				
				m_sReturnFileType = m_AppInput.GetFlag(0);
			}
			
			m_AppFrame.ShowButtonNext( false );
			
			ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);
        }

		private void RscExplorerV11_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			/*
			//Page Navigation Back calls this...
			if( m_a.Count == 0 )
			{
				ListFiles();
			}
			*/
		}		
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if( m_AppArgs.Waiting )
			{
				RscStore store = new RscStore();
				
				RscPageArgsRet appOutput = m_AppArgs.GetOutput();
				if( appOutput != null )
				{
					switch( appOutput.ID )
					{
						
						case "Tools" :
							
							m_AppArgs.Vipe();
							
							//Tools called...
							
							m_AppArgs.Clear();
							
							if( !store.FolderExists( CurrentPath() ) )
							{
								if( m_path.Count > 0 )
									m_path.Pop();
							}
							
							ListFiles();
							
							break;
							
						case "txNewFolder" :
						{
							if( appOutput.GetFlag(0) == "Ok" )
							{
								string sNewFldr = CurrentPath(appOutput.GetData(0));
								
								if( !store.FolderExists( sNewFldr ) )
								{
									store.CreateFolder( sNewFldr );
								}
							
								ListFiles();
							}
							
							m_AppArgs.Clear();
							
							break;
						}
							
					}
				}
			}
			
			if( !m_bAppStarted )
			{
				bool bDone = false;
				
				IDictionary<string, string> parameters = this.NavigationContext.QueryString;
				
				if( parameters.ContainsKey( "app_title" ) )
				{
					m_AppFrame.AppTitle = parameters["app_title"];
				}
				
				if( parameters.ContainsKey( "app_icores" ) )
				{
					m_AppFrame.AppIconRes = parameters["app_icores"];
				}
				
				if( parameters.ContainsKey( "root" ) )
				{
					string sFolder = parameters["root"];
					
					RscStore store = new RscStore();
					
					try
					{
						store.CreateFolderPath( sFolder );
						
						string [] asParts = sFolder.Split( '\\' );
							
						foreach( string sPart in asParts )
						{
							m_path.Push( sPart );
						}
						m_iArgRootLen = m_path.Count;
							
						ListFiles();
							
						bDone = true;
					}
					catch( Exception )
					{
						//Wrong Drive, etc...
						m_iArgRootLen = 0;
						m_path.Clear();
					}
					
				}
				
				if( !bDone )
				{
					ListFiles();
				}
			}
			
			m_bAppStarted = true;
		}
		
		private void ContentPanel_SizeChanged(object sender, SizeChangedEventArgs e)
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
 
		private void m_AppFrame_OnNext(object sender, System.EventArgs e)
		{
			if( m_AppInput != null )
			{
				string sPath = "";
				
				if( m_sReturnFileType == "" )
				{
					if( m_path.Count == 0 ) return;
					sPath = CurrentPath();
				}
				else
				{
					if( m_sReturnFileName.Length == 0 ) return;
					sPath = CurrentPath(m_sReturnFileName);
				}
				
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Ok" );
				appOutput.SetData( 0, sPath );
				appOutput.SetOutput();
				
				NavigationService.GoBack();
			}
		}
 
		private void m_AppFrame_OnExit(object sender, System.EventArgs e)
		{
			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Cancel" );
				appOutput.SetOutput();
			}

			NavigationService.GoBack();
		}
		
       	protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( m_path.Count > m_iArgRootLen )
			{
				m_path.Pop();
				
				if( m_AppInput != null )
				{
					m_sReturnFileName = "";
					m_AppFrame.ShowButtonNext( m_path.Count > 0 && m_sReturnFileType.Length == 0 );
				}
			
				ListFiles();
				
				e.Cancel = true;
			}
			else
			{
				if( m_AppInput != null )
				{
					RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
					appOutput.SetFlag( 0, "Back" );
					appOutput.SetOutput();
				}
			}
		}
			
		private void btnRoot_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_iArgRootLen = 0;
			if( m_path.Count > 0 )
			{
				ReSet();
			}
		}
		
		private void ReSet( bool bNoClear = false )
		{
			if( !bNoClear )
				m_path.Clear();
				
			m_sReturnFileName = "";
			m_AppFrame.ShowButtonNext(false);
			
			ListFiles();
		}
		
		private void btnUp_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_path.Count <= m_iArgRootLen )
				m_iArgRootLen = 0;
			
			if( m_path.Count > 0 )
			{
				m_path.Pop();
				
				if( m_AppInput != null )
				{
					m_sReturnFileName = "";
					m_AppFrame.ShowButtonNext(m_path.Count > 0 && m_sReturnFileType.Length == 0);
				}
			
				ListFiles();
			}
		}
		
		private string CurrentPath(string sFileName = "")
		{
			string sPath = "";
			foreach( string sFldr in m_path )
			{
				if( sPath.Length != 0 ) sPath = "\\" + sPath;
				sPath = sFldr + sPath;
			}
			
			if( sFileName.Length != 0 )
			{
				if( sPath.Length != 0 ) sPath = sPath + "\\";
				sPath = sPath + sFileName;
			}
			
			return sPath;
		}

		private void ListFiles()
		{			
			string sPath = CurrentPath(" "); //The " " is to force \ at the end...
			
			m_AppFrame.StatusText = sPath;
			
			spFiles.Children.Clear();
			m_a.Clear();
			
			m_TEMP_asFolders = null;
			m_TEMP_asFiles = null;
			m_TEMP_iCntFolders = 0;
			m_TEMP_iCntFiles = 0;
			m_TEMP_iIdx = 0; //BugFix... -1;
			m_TEMP_sSearchPath = sPath.Trim();
			m_TEMP_sSearchWildCard = "*.*";
			
			prsBar1.Minimum = 0;
			prsBar1.Maximum = 1;
			prsBar1.Value = 0;
			
			prsBar2.Minimum = 0;
			prsBar2.Maximum = 1;
			prsBar2.Value = 0;
			
			RscStore store = new RscStore();
			
			string[] fldrs = RscSort.OrderBy(store.GetFolderNames(m_TEMP_sSearchPath, m_TEMP_sSearchWildCard, m_bIncludeHidden, true));
			if( fldrs.Length > 0 )
			{
				m_TEMP_iCntFolders = fldrs.Length;
				m_TEMP_asFolders = fldrs;
				
				prsBar1.Maximum = m_TEMP_asFolders.Length;
				m_AppFrame.StatusText = CurrentPath(" ") + "\r\n" + "Parsing folders...";
			}
			else
			{
				string[] fles = RscSort.OrderBy(store.GetFileNames(m_TEMP_sSearchPath, m_TEMP_sSearchWildCard, m_bIncludeHidden, true));
				if( fles.Length > 0 )
				{
					m_TEMP_iCntFiles = fles.Length;
					m_TEMP_asFiles = fles;
					
					//DeBug...
					//m_AppFrame.TRACE = m_TEMP_iCntFiles.ToString();
				
					prsBar2.Maximum = m_TEMP_asFiles.Length;
					m_AppFrame.StatusText = CurrentPath(" ") + "\r\n" + "Parsing files...";
				}
				else
				{
					return; //No folders nor files...
				}
			}
			
			if( m_TEMP_asFolders != null ) prsBar1.Visibility = Rsc.Visible;
			if( m_TEMP_asFiles != null ) prsBar2.Visibility = Rsc.Visible;
			
			m_AppFrame.StartTimer( "list files", LayoutRoot, 1, (m_TEMP_iCntFolders + m_TEMP_iCntFiles) - 1 );
		}
		
		private void m_AppFrame_OnTimer(object sender, RscAppFrameTimerEventArgs e)
		{
			
			if( m_TEMP_asFolders == null && m_TEMP_asFiles == null ) return;
			
			//BugFix...
			//m_TEMP_iIdx++;
			
			if( m_TEMP_asFolders != null )
			{
				for(;;)
				{
					bool bBreak = true;
					
					if( m_TEMP_iIdx >= m_TEMP_asFolders.Length )
					{
						m_TEMP_asFolders = null;
						m_TEMP_iIdx = 0; //BugFix... -1;
						
						RscStore store = new RscStore();
						
						string[] fles = RscSort.OrderBy(store.GetFileNames(m_TEMP_sSearchPath, m_TEMP_sSearchWildCard, m_bIncludeHidden, true));
						if( fles.Length > 0 )
						{
							m_TEMP_iCntFiles = fles.Length;
							
							m_TEMP_asFiles = fles;
							prsBar2.Maximum = m_TEMP_asFiles.Length;
							
							e.Max += m_TEMP_iCntFiles;
						}
						else
						{
							ListFiles_Done();
							return;
						}
					}
					else
					{
						for( int iLoad = 0; iLoad < ciLoadAtOnce_Add; iLoad++ )
						{
							if( m_TEMP_iIdx >= m_TEMP_asFolders.Length )
							{
								bBreak = false;
								break;
							}
							
							e.Pos++;
							
							prsBar1.Value += 1;
							AddFile(m_TEMP_asFolders[m_TEMP_iIdx], true);
							
							m_TEMP_iIdx++;
						}
					}
					
					if( bBreak ) break;
				}
			}
			else if( m_TEMP_asFiles != null )
			{
				for(;;)
				{
					bool bBreak = true;
					
					if( m_TEMP_iIdx >= m_TEMP_asFiles.Length )
					{
						ListFiles_Done();
						return;
					}
					else
					{
						for( int iLoad = 0; iLoad < ciLoadAtOnce_Add; iLoad++ )
						{
							if( m_TEMP_iIdx >= m_TEMP_asFiles.Length )
							{
								bBreak = false;
								break;
							}
							
							e.Pos++;
							
							prsBar2.Value += 1;
							AddFile(m_TEMP_asFiles[m_TEMP_iIdx], false);
							
							m_TEMP_iIdx++;
						}
					}
					
					if( bBreak ) break;
				}
			}
		}
		
		private void ListFiles_Done()
		{
			prsBar1.Visibility = Rsc.Collapsed;
			prsBar2.Visibility = Rsc.Collapsed;
			
			string sTmp;
			if( CurrentPath(" ").Trim().Length == 0 )
			{
				imgAddFldr.Visibility = Rsc.Collapsed;
				sTmp = "storage";
			}
			else
			{
				RscStore store = new RscStore();
				
				imgAddFldr.Visibility = Rsc.ConditionalVisibility( store.HasWriteAccess( CurrentPath(" ") ) );
				sTmp = "folder";
			}
			
			string sStat = "";
			switch( m_TEMP_iCntFolders )
			{
				case 0 :
					break;
				case 1 :
					sStat += m_TEMP_iCntFolders.ToString() + " " + sTmp;
					break;
				default :
					sStat += m_TEMP_iCntFolders.ToString() + " " + sTmp + "s";
					break;
			}
			switch( m_TEMP_iCntFiles )
			{
				case 0 :
					break;
				case 1 :
					if( sStat.Length > 0 ) sStat += " | ";
					sStat += m_TEMP_iCntFiles.ToString() + " file";
					break;
				default :
					if( sStat.Length > 0 ) sStat += " | ";
					sStat += m_TEMP_iCntFiles.ToString() + " files";
					break;
			}
			
			m_AppFrame.StatusText = CurrentPath(" ") + "\r\n" + sStat;
		}
		
		private void AddFile(string sFnOrig, bool bFldr)
		{
			
			//To know hidden / normal file / folder...
			char cInfChr = sFnOrig[ 0 ];
			string sFn = sFnOrig.Substring( 1 );
			
			int idx = spFiles.Children.Count + 1;
		
			string fT = sFn;
			string fE = "";
			string fCap = sFn;
			int iPos = sFn.LastIndexOf(".");
			if( iPos >= 0 )
			{
				fT = sFn.Substring(0, iPos);
				fE = sFn.Substring(iPos + 1);
				
				//fCap = fE.ToUpper() + " | " + fT;
			}
			/*
			if( bFldr )
			{
				fCap = "[ " + fT;
				if( fE.Length > 0 )
				{
					fCap += "." + fE;
				}
				fCap += " ]";
			}
			else
			{
			*/
				fCap = fT;
				if( fE.Length > 0 )
				{
					fCap += "." + fE;
				}
			/*
			}
			*/
			if( bFldr )
			{
				string sRd = RscStore.GetRootDescription( fCap );
				if( sRd.Length > 0 )
				{
					fCap += "\n(" + sRd + ")";
				}
			}
			
			RscFileItemDesc it;
			it = new RscFileItemDesc();
			it.idx = idx;
			it.bFolder = bFldr;
			it.sFn = sFn;
			it.sExt = fE;
			it.cInfChr = cInfChr;
			m_a.Add( it );
			
			Grid grdOut = new Grid();
			grdOut.Name = "grdOut_" + idx.ToString();
			grdOut.Margin = new Thickness(0, 0, 0, 4 );
			RowDefinition rd;
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdOut.RowDefinitions.Add(rd);
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdOut.RowDefinitions.Add(rd);
			spFiles.Children.Add(grdOut);
			
			Rectangle rc;
			rc = new Rectangle();
			if( bFldr )
			{
				rc.Fill = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.DialogLightBack );
			}
			else
			{
				rc.Fill = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.DialogLightBack );
			}
			//rc.Opacity = 0.5;
			rc.SetValue(Grid.RowProperty, 0);
			grdOut.Children.Add(rc);
			
			Grid grdBtns = new Grid();
			grdBtns.Name = "grdBtns_" + idx.ToString();
			grdBtns.Margin = new Thickness(0, 0, 0, 0 );
			ColumnDefinition cd;
			//cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdBtns.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdBtns.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = new GridLength(1, GridUnitType.Star); grdBtns.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdBtns.ColumnDefinitions.Add(cd);
			grdBtns.SetValue(Grid.RowProperty, 0);
			grdOut.Children.Add(grdBtns);
			
				RscIcon imgType = new RscIcon(grdBtns, Grid.ColumnProperty, 0, 36, 36, Rsc.Visible);
				if( bFldr )
				{
					if( CurrentPath( " " ).Trim().Length == 0 )
					{
						imgType.Image.Source = m_isDriveSm;
					}
					else
					{
						if( cInfChr == 'n' )
							imgType.Image.Source = m_isFolderSm;
						else
							imgType.Image.Source = m_isFolderHiddenSm;
					}
				}
				else
				{
					if( cInfChr == 'n' )
						imgType.Image.Source = m_isFileSm;
					else
						imgType.Image.Source = m_isFileHiddenSm;
				}
		
				Button btnCap = new Button();
				btnCap.Name = "btnCap_" + idx.ToString();
				btnCap.Content = fCap; //fT;
				btnCap.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
				btnCap.BorderThickness = new Thickness(0);
				btnCap.FontSize = 20;
				btnCap.Foreground = new SolidColorBrush(Colors.Black); //.White); //.Blue);
				btnCap.Margin = new Thickness(-12,-10,-12,-12);
				//btnCap.Tag = it;
				//btnCap.Opacity = 0.5;
				btnCap.SetValue(Grid.ColumnProperty, 1);
				grdBtns.Children.Add(btnCap);
				
				it.btnMore = new RscIconButton(grdBtns, Grid.ColumnProperty, 2, 36, 36, Rsc.Visible);
				it.btnMore.Image.Source = m_isMore;
			
			Grid grdDetails = new Grid();
			grdDetails.Visibility = Rsc.Collapsed;
			grdDetails.Name = "grdDetails_" + idx.ToString();
			grdDetails.Margin = new Thickness(0, 1, 0, 0);
			//ColumnDefinition cd;
			cd = new ColumnDefinition(); cd.Width = new GridLength(1, GridUnitType.Star); grdDetails.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdDetails.ColumnDefinitions.Add(cd);
			grdDetails.SetValue(Grid.RowProperty, 1);
			grdOut.Children.Add(grdDetails);
			
			it.grdDetails = grdDetails;
			
				StackPanel spActBtn = new StackPanel();
				spActBtn.Margin = new Thickness(4, 24, 0, 4);
				spActBtn.SetValue(Grid.ColumnProperty, 1);
				grdDetails.Children.Add(spActBtn);
			
				Grid grdActions = new Grid();
				rd = new RowDefinition(); rd.Height = GridLength.Auto; grdActions.RowDefinitions.Add(rd);
				rd = new RowDefinition(); rd.Height = new GridLength(24, GridUnitType.Pixel); grdActions.RowDefinitions.Add(rd);
				rd = new RowDefinition(); rd.Height = GridLength.Auto; grdActions.RowDefinitions.Add(rd);
				spActBtn.Children.Add( grdActions );
				
					RscIconButton btnTools = new RscIconButton(grdActions, Grid.RowProperty, 0, 36, 36, Rsc.Visible);
					btnTools.Image.Source = m_isTools;
				
					RscIconButton btnOpen = new RscIconButton(grdActions, Grid.RowProperty, 2, 36, 36, Rsc.Visible);
					btnOpen.Image.Source = m_isOpen;
			
			Grid grdPic = new Grid();
			grdPic.Name = "grdPic_" + idx.ToString();
			//ColumnDefinition cd;
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdPic.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); grdPic.ColumnDefinitions.Add(cd);
			grdPic.SetValue(Grid.ColumnProperty, 0);
			grdDetails.Children.Add(grdPic);
			
			Image img = new Image();
			img.Visibility = Rsc.Collapsed;
			img.Margin = new Thickness(0, 0, 0, 0);
			img.Stretch = Stretch.Uniform;
			//img.Source = new BitmapImage(new Uri("Images/Bk001_portrait.jpg", UriKind.Relative));
			img.Width = 120;
			img.Height = 200;
			img.SetValue(Grid.ColumnProperty, 0);
			grdPic.Children.Add(img);
			
			it.img = img;
			
			TextBox txt = new TextBox();
			txt.Visibility = Rsc.Collapsed;
			txt.Margin = new Thickness(0, -12, 0, -12);
			txt.IsReadOnly = false;
			txt.AcceptsReturn = true;
			txt.BorderThickness = new Thickness(0);
			txt.TextWrapping = TextWrapping.Wrap;
			txt.TextAlignment = TextAlignment.Left;
			txt.FontSize = 16;
			txt.Background = new SolidColorBrush(Colors.White);
			txt.Foreground = new SolidColorBrush(Colors.Black);
			txt.Width = 120;
			txt.Height = 200;
			txt.SetValue(Grid.ColumnProperty, 0);
			grdPic.Children.Add(txt);
			
			it.txt = txt;
			
			Grid grdTit = new Grid();
			grdTit.Name = "grdTit_" + idx.ToString();
			//RowDefinition rd;
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdTit.RowDefinitions.Add(rd);
			rd = new RowDefinition(); grdTit.RowDefinitions.Add(rd);
			grdTit.SetValue(Grid.ColumnProperty, 1);
			grdPic.Children.Add(grdTit);
			
			TextBox tbTit = new TextBox();
			tbTit.Name = "tbTit_" + idx.ToString();
			tbTit.TabNavigation = KeyboardNavigationMode.Once; //GetFocusDenie FIX...
			tbTit.FontSize = 16;
			tbTit.Text = sFn;
			tbTit.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.DialogLightBack );
			tbTit.Foreground = new SolidColorBrush(Colors.Black); //White);
			tbTit.Margin = new Thickness(-11, -12, -12, -11);
			tbTit.BorderThickness = new Thickness(0, 0, 0, 0);
			tbTit.AcceptsReturn = true;
			tbTit.TextWrapping = TextWrapping.Wrap;
			tbTit.SetValue(Grid.RowProperty, 0);
			grdTit.Children.Add(tbTit);
			
			TextBox tbDetails = new TextBox();
			tbDetails.Name = "tbDet_" + idx.ToString();
			tbDetails.TabNavigation = KeyboardNavigationMode.Once; //GetFocusDenie FIX...
			tbDetails.FontSize = 16;
			tbDetails.Text = "";
			tbDetails.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.DialogLightBack );
			tbDetails.Foreground = new SolidColorBrush(Colors.Black); //.White);
			tbDetails.Margin = new Thickness(-11, -12, -12, -12);
			tbDetails.BorderThickness = new Thickness(0, 0, 0, 0);
			tbDetails.AcceptsReturn = true;
			tbDetails.TextWrapping = TextWrapping.Wrap;
			tbDetails.SetValue(Grid.RowProperty, 1);
			grdTit.Children.Add(tbDetails);
				
			it.tbDetails = tbDetails;
		
			Button btn = new Button();
			btn.Visibility = Rsc.Collapsed;
			btn.Name = "btnTit_" + idx.ToString();
			btn.Content = "";
			btn.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
			btn.BorderThickness = new Thickness(0);
			btn.Foreground = new SolidColorBrush(Colors.White);
			btn.Margin = new Thickness(-14,-13,-14,-14); //GetFocusDenie FIX...
			//btn.Tag = it;
			btn.Opacity = 0.5;
			btn.SetValue(Grid.ColumnProperty, 0);
			grdDetails.Children.Add(btn);
			
			it.btn = btn;
			
			it.btnMore.Tag = it;
			btnCap.Tag = it;
			btnOpen.Tag = it;
			btnTools.Tag = it;
			btn.Tag = it;
			
			btnCap.Click += new System.Windows.RoutedEventHandler(btnCap_Click);
			btnCap.DoubleTap +=new System.EventHandler<System.Windows.Input.GestureEventArgs>(btnOpen_DoubleTap);
			it.btnMore.Click += new System.Windows.RoutedEventHandler(btnMore_Click);
			btnOpen.Click += new System.Windows.RoutedEventHandler(btnOpen_Click);
			btnTools.Click += new System.Windows.RoutedEventHandler(btnTools_Click);
			//btn.Click += new System.Windows.RoutedEventHandler(btn_Click);
			
		}
		
		private void btnMore_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			RscFileItemDesc it;
			it = (RscFileItemDesc) btn.Tag;
			
			ShowMore( it, (it.grdDetails.Visibility == Rsc.Collapsed) );
		}
		
		private void ShowMore(RscFileItemDesc it, bool bExpand)
		{
			string sPath = CurrentPath(it.sFn);
			
			if( bExpand )
			{
				if( it.grdDetails.Visibility == Rsc.Visible ) return;
				
				it.grdDetails.Visibility = Rsc.Visible;
				it.btn.Visibility = Rsc.Visible;
				
				it.btnMore.Image.Source = m_isLess;
				
				//Do always...
				//if( it.tbDetails.Text.ToString().Length == 0 )
				{
					try
					{
						string sDet = "";
						
						RscStore store = new RscStore();
						
						RscStoreProperties sp;
						if( it.bFolder )
							sp = store.GetFolderProperties( sPath );
						else
							sp = store.GetFileProperties( sPath );
						
						if( sp.HasFullPath )
						{
							if( sDet.Length > 0 ) sDet += "\r\n";
							sDet += "path: ";
							sDet += sp.FullPath;
							sDet += "\r\n";
						}
						
						if( sp.HasDisplayName )
						{
							if( sDet.Length > 0 ) sDet += "\r\n";
							sDet += "display name: ";
							sDet += sp.DisplayName;
							sDet += "\r\n";
						}
							
						if( sp.HasCreationTime )
						{
							if( sDet.Length > 0 ) sDet += "\r\n";
							sDet += "created: ";
							sDet += sp.CreationTime.ToString();
						}
							
						if( sp.HasLastWriteTime )
						{
							if( sDet.Length > 0 ) sDet += "\r\n";
							sDet += "modified: ";
							sDet += sp.LastWriteTime.ToString();
						}
							
						if( sp.HasLastAccessTime )
						{
							if( sDet.Length > 0 ) sDet += "\r\n";
							sDet += "accessed: ";
							sDet += sp.LastAccessTime.ToString();
						}
						
						if( it.bFolder )
						{
							//it.img.Stretch = Stretch.None;
							it.img.Stretch = Stretch.Uniform;
							
							if( CurrentPath( " " ).Trim().Length == 0 )
							{
								it.img.Source = m_isDriveLg;
							}
							else
							{
								if( it.cInfChr == 'n' )
									it.img.Source = m_isFolderLg;
								else
									it.img.Source = m_isFolderHiddenLg;
							}
								
							it.img.Visibility = Rsc.Visible;
						}
						else
						{
							if( sp.HasLength )
							{
								if( sDet.Length > 0 ) sDet += "\r\n";
								if( sDet.Length > 0 ) sDet += "\r\n";
								sDet += RscUtils.toMBstr(sp.Length);
							}
							
							string strFileGroup;
							if( store.IsFolderImageOnly( sPath ) )
								strFileGroup = "Image.Native";
							else
								strFileGroup = RscRegFs.GetFileGroupEx( it.sExt );
							
							switch( strFileGroup )
							{
								
								case "Text" :
									LdlTxt(it, sPath);
									break;
									
								case "Image.Native" :
									LdlImg(it, sPath);
									break;
									
							}
						}
						
						it.tbDetails.Text = sDet;
					}
					catch( Exception ex )
					{
						it.tbDetails.Text = "ERR: " + ex.Message;
					}
				}
			}
			else
			{
				if( it.grdDetails.Visibility == Rsc.Collapsed ) return;

				it.grdDetails.Visibility = Rsc.Collapsed;
				it.btn.Visibility = Rsc.Collapsed;
				
				it.btnMore.Image.Source = m_isMore;
			}
		}
		
  		private void LdlImg(RscFileItemDesc it, string fName, bool bResImg = false)
		{
			if( fName == "" ) return;
			
			BitmapImage bmp = null;
		
			if( bResImg )
			{
				bmp = new BitmapImage(new Uri(fName, UriKind.Relative));
			}
			else
			{
				//DO NOT!!!
				//if( !store.FileExists(fName) ) return;
			
				try
				{
					RscStore store = new RscStore();
					
					System.IO.Stream stream = store.GetReaderStream( fName, true );
			
					bmp = new BitmapImage();
					bmp.SetSource(stream);
					stream.Close();
				}
				catch( Exception )
				{
					bmp = null;
				}
			}
			
			if( bmp == null )
			{
				it.img.Visibility = Rsc.Collapsed;
			}
			else
			{
				it.img.Stretch = Stretch.Uniform;
				it.img.Source = bmp;
				it.img.Visibility = Rsc.Visible;
			}
			
			it.txt.Visibility = Rsc.Collapsed;
		}
		
  		private void LdlTxt(RscFileItemDesc it, string fName)
		{
			if( fName == "" ) return;
			
			string strTxt = "";
		
			//DO NOT!!!
			//if( !store.FileExists(fName) ) return;
		
			try
			{
				RscStore store = new RscStore();
				
				/*
				System.IO.Stream stream = store.GetReaderStream( fName, false );
				if( stream == null ) return;
				
				System.IO.StreamReader sr = new System.IO.StreamReader( stream );
				
				strTxt = sr.ReadToEnd();
				
				sr.Close();
				stream.Close();
				*/
				
				bool bTmp;
				strTxt = store.ReadTextFile( fName, "", out bTmp, 100 );
			}
			catch( Exception )
			{
				return;
			}
			
			it.img.Visibility = Rsc.Collapsed;
			
			it.txt.Text = strTxt;
			it.txt.Visibility = Rsc.Visible;
		}

		private void btnCap_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			RscFileItemDesc it;
			it = (RscFileItemDesc) btn.Tag;
			
			m_sReturnFileName = "";
			if( !it.bFolder )
			{
				if( m_sReturnFileType.Length > 0 )
				{
					if( m_sReturnFileType == "*" || it.sExt == m_sReturnFileType )
					{
						m_sReturnFileName = it.sFn;
						m_AppFrame.ShowButtonNext( true );
					}
				}
			}
			
			/*
			if( it.bFolder )
			{
				m_path.Push( it.sFn );
				
				ListFiles();
			}
			else
			{
			*/
				ShowMore( it, (it.grdDetails.Visibility == Rsc.Collapsed) );
			/*
			}
			*/
		}

		private void btnOpen_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			RscFileItemDesc it;
			it = (RscFileItemDesc) btn.Tag;
			
			DoDoubleTap( it );
		}
		
		private void DoDoubleTap( RscFileItemDesc it )
		{
			if( it.bFolder )
			{
				m_path.Push( it.sFn );
				
				if( m_AppInput != null )
				{
					m_sReturnFileName = "";
					m_AppFrame.ShowButtonNext(m_path.Count > 0 && m_sReturnFileType.Length == 0);
				}
				
				ListFiles();
			}
			else
			{
				//ShowMore( it, (it.grdDetails.Visibility == Rsc.Collapsed) );
			}
		}
		
		private void btnTools_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			RscFileItemDesc it;
			it = (RscFileItemDesc) btn.Tag;
			
			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "Tools" );
			
			if( it.bFolder )
			{
				appInput.SetData( 0, CurrentPath(it.sFn) );
				appInput.SetData( 1, "*.*" );
				appInput.SetData( 2, "recurse" );
				
				if( m_bIncludeHidden )
					appInput.SetData( 3, "showHidden" );
				else
					appInput.SetData( 3, "" );
			}
			else
			{
				appInput.SetData( 0, CurrentPath() );
				appInput.SetData( 1, it.sFn );
				appInput.SetData( 2, "" ); //non-recurse
				
				if( m_bIncludeHidden )
					appInput.SetData( 3, "showHidden" );
				else
					appInput.SetData( 3, "" );
			}
			
			appInput.SetInput( "RscViewer_FindFilesV12" );
						
			this.NavigationService.Navigate( appInput.GetNavigateUri( csViewersAssy ) );
			
			/*
			//Build error...
			/*
			MessageBoxResult res = MessageBox.Show( "Deleting file '" + it.sFn + "'...", MessageBoxButton.OKCancel );
			if( res != MessageBoxResult.OK ) return;
			*
			MessageBoxResult res = MessageBox.Show( "Deleting file '" + it.sFn + "'...");
			if( res != MessageBoxResult.OK ) return;
						
			RscStore store = new RscStore();
			
			bool bFailed = false;
			
			if( it.bFolder )
			{
				if( store.DirectoryExists(CurrentPath(it.sFn)) )
				{
					try
					{
						store.DeleteDirectory(CurrentPath(it.sFn));
			
						ListFiles();
					}
					catch( Exception )
					{
						bFailed = true;
					}
					
					if( bFailed ) MessageBox.Show("Unable to delete folder '" + it.sFn + "'!");
				}
			}
			else
			{
				if( store.FileExists(CurrentPath(it.sFn)) )
				{
					try
					{
						store.DeleteFile(CurrentPath(it.sFn));
				
						ListFiles();
					}
					catch( Exception )
					{
						bFailed = true;
					}
					
					if( bFailed ) MessageBox.Show("Unable to delete file '" + it.sFn + "'!");
				}
			}
			*/
		}

		private void btnOpen_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			RscFileItemDesc itCurr;
			itCurr = (RscFileItemDesc) btn.Tag;
			
			if( itCurr.bFolder )
			{
				DoDoubleTap( itCurr );
				return;
			}
			
			RscStore store = new RscStore();
						
			string sFullPath = CurrentPath(itCurr.sFn);
			string strFileGroupCurr;
			if( store.IsFolderImageOnly( sFullPath ) )
			{
				strFileGroupCurr = "Image.Native";
			}
			else
			{
				strFileGroupCurr = RscRegFs.GetFileGroupEx( itCurr.sExt );
			}
			
			// //
			//
			
			if( strFileGroupCurr.Length == 0 )
			{
				//Even for unknown (but system-known) types, start HexaViewer first!!!
				/*
				//MessageBox.Show("No open action defined for file type.");
				
				string sErr = "";
				
				if( !RscStore_Storage.LaunchFile( CurrentPath(itCurr.sFn), out sErr ) )
				{
					if( sErr.Length > 0 )
						MessageBox.Show( sErr );
				*/
					strFileGroupCurr = "Data";
				/*
				}
				*/
			}
			
			if( strFileGroupCurr.Length > 0 )
			{
				RscPageArgsRetManager appArgs = new RscPageArgsRetManager();
				
				RscPageArgsRet appInput = new RscPageArgsRet( appArgs,
					m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "Open" );
				
				int iIdx = 0;
				
				if( RscRegFs.GetViewerAppAllowList( strFileGroupCurr ) )
				{
					int i = -1;
					foreach( RscFileItemDesc it in m_a )
					{
						string strFileGroup = RscRegFs.GetFileGroupEx( it.sExt );
						if( strFileGroup == strFileGroupCurr )
						{
							i++;
							appInput.SetData( i, CurrentPath(it.sFn) );
							
							if( it == itCurr ) iIdx = i;
						}
					}
				}
				else
				{
					if( RscRegFs.GetViewerAppSendContent( strFileGroupCurr ) )
					{				
						bool bTmp = false;
						string sContent = store.ReadTextFile( CurrentPath(itCurr.sFn), "", out bTmp );
						appInput.SetData( 0, sContent );
					}
					else
					{
						appInput.SetData( iIdx, sFullPath );
					}
				}
				
				appInput.SetFlag( 0, iIdx.ToString() );
				appInput.SetFlag( 1, LayoutRoot.ActualWidth.ToString() );
				appInput.SetFlag( 2, LayoutRoot.ActualHeight.ToString() );
				
				appInput.SetInput( RscRegFs.GetViewerAppPageName( strFileGroupCurr ) );
							
				this.NavigationService.Navigate( appInput.GetNavigateUri( RscRegFs.GetViewerAppAssyName( strFileGroupCurr ) ) );
			}
			
			//
			// //
		}
		
		private void btnExpAll_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			
			prsBar1.Minimum = 0;
			prsBar1.Maximum = m_a.Count;
			prsBar1.Value = 0;
			prsBar1.Visibility = Rsc.Visible;
			
			m_TEMP_iIdx = 0;
			
			m_tmrEnumExpAll.Start();
		}

		private void m_tmrEnumExpAll_Tick(object sender, System.EventArgs e)
		{
			m_tmrEnumExpAll.Stop();
			
			for(;;)
			{
				bool bBreak = true;
				
				if( m_TEMP_iIdx >= m_a.Count )
				{
					ExpAll_Done();
					return;
				}
				else
				{
					for( int iLoad = 0; iLoad < ciLoadAtOnce_ExpAll; iLoad++ )
					{
						if( m_TEMP_iIdx >= m_a.Count )
						{
							bBreak = false;
							break;
						}
						
						prsBar1.Value += 1;
						ShowMore( m_a[ m_TEMP_iIdx ], true ); //(it.grdDetails.Visibility == Rsc.Collapsed) );
						
						m_TEMP_iIdx++;
					}
				}
				
				if( bBreak ) break;
			}
			
			m_tmrEnumExpAll.Start();
		}
		
		private void ExpAll_Done()
		{
			prsBar1.Visibility = Rsc.Collapsed;
		}
		
		private void btnAddFldr_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			//FIX: Without timer - focus problem...
			m_tmrAddFldr.Start();
		}

		private void m_tmrAddFldr_Tick(object sender, System.EventArgs e)
		{
			m_tmrAddFldr.Stop();

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "txNewFolder" );
			
			appInput.SetFlag( 0, "new folder name" );
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, "" );
			appInput.SetInput( "RscDlg_TxtInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csIPgC_DlgsAssy ) );
		}
		
		private void btnUsrAdm_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_bIncludeHidden = !m_bIncludeHidden;
			
			if( m_bIncludeHidden )
				btnUsrAdm.Content = " usr ";
			else
				btnUsrAdm.Content = " adm ";
			
			ReSet( m_bIncludeHidden );
		}
				
    }
	
}
