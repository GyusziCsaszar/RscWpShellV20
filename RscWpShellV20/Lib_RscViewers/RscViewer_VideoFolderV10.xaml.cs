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
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Input;

using Ressive.Utils;
using Ressive.UtilsEx;
using Ressive.Store;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

namespace Lib_RscViewers
{
	
    public partial class RscViewer_VideoFolderV10 : PhoneApplicationPage
    {
		
		const string csDlgsAssy = "Lib_RscIPgC_Dlgs";
		
		RscAppFrame m_AppFrame;
		ImageSource m_isChbOn = null;
		ImageSource m_isChbOff = null;
		ImageSource m_isDummy = null;
		ImageSource m_isFolder = null;
		ImageSource m_isVideo = null;
		ImageSource m_isUp = null;
		ImageSource m_isDown = null;
		
		TextBoxDenieEdit m_txtTitle;
		RscIconButton m_btnPrev;
		RscIconButton m_btnFldr;
		RscIconButton m_btnOrder;
		bool m_bOrderAscending = true;
		RscIconButton m_btnUp;
		RscIconButton m_btnNext;
	
		RscPageArgsRetManager appArgs;
		
		class GalleryImage
		{
			
			public string Name;
			
			public RscStoreItem FsItem;
			
			public string ThumbPath;
			
			public RscIconButton IbChb;
			public bool Checked;
			
			public GalleryImage(string sName, string sImage)
			{
				Name = sName;
				
				FsItem = null;
				
				ThumbPath = "";
				
				IbChb = null;
				Checked = false;
			}
			
			public bool ThumbnailPresent
			{
				get{ return (ThumbPath.Length > 0); }
			}
			
		}
		
		class Gallery
		{
			
			public List<GalleryImage> m_a = null;
			
			public Gallery()
			{
				m_a = new List<GalleryImage>();
			}
		}
		
		bool m_bLoaded = false;
		Size m_sContentPanel = new Size(100, 100);
		DispatcherTimer m_tmrLoad;
		
		RscStoreItem m_Root = null;
		
		List<Gallery> m_a = new List<Gallery>();
		int m_iSelCount = 0;
		int m_iTop = 0;
		int m_iCX = 3;
		int m_iCY = 1;
		int m_iDummiesAtEnd = 0;
		
		Point m_ptTouchDown;
		
		DispatcherTimer m_tmrBrowse;
		
		bool m_bInThisApp = true;
		
		int m_iXCur = -1;
		int m_iYCur = -1;
		
		bool m_bIsInSwipe = false;
		
        public RscViewer_VideoFolderV10()
        {
            InitializeComponent();
			
			//Register all file-type associations...
			RscFileTypes.RegisterAll();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Video Gallery 1.0", "Images/IcoSm001_VideoGallery.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			// ///////////////
			m_isChbOn = m_AppFrame.Theme.GetImage("Images/CheckOn.jpg");
			m_isChbOff = m_AppFrame.Theme.GetImage("Images/CheckOff.jpg");
			m_isDummy = m_AppFrame.Theme.GetImage("Images/Img001_Dummy.jpg");
			m_isFolder = m_AppFrame.Theme.GetImage("Images/Type001_(dir).jpg");
			m_isVideo = m_AppFrame.Theme.GetImage("Images/Ico001_VideoGallery.jpg");
			m_isUp = m_AppFrame.Theme.GetImage("Images/Btn001_Inc.jpg");
			m_isDown = m_AppFrame.Theme.GetImage("Images/Btn001_Dec.jpg");
			
			TitlePanel.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack );
			
			m_btnPrev = new RscIconButton(TitlePanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible);
			m_btnPrev.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipPrev.jpg");
			m_btnPrev.Click += new System.Windows.RoutedEventHandler(m_btnPrev_Click);
			
			m_txtTitle = new TextBoxDenieEdit(true, true, TitlePanel, Grid.ColumnProperty, 1);
			m_txtTitle.Background = new SolidColorBrush(m_AppFrame.Theme.ThemeColors.ToolBarLightBack); //Colors.LightGray);
			m_txtTitle.Foreground = new SolidColorBrush(m_AppFrame.Theme.ThemeColors.ToolBarLightFore); //Colors.Black);
			m_txtTitle.FontSize = 16;
			m_txtTitle.Text = "";
			
			m_btnOrder = new RscIconButton(TitlePanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Visible);
			//m_bOrderAscending = true;
			//m_btnOrder.Image.Source = m_isDown;
			m_bOrderAscending = false;
			m_btnOrder.Image.Source = m_isUp;
			m_btnOrder.Click += new System.Windows.RoutedEventHandler(m_btnOrder_Click);
			
			m_btnUp = new RscIconButton(TitlePanel, Grid.ColumnProperty, 3, 50, 50, Rsc.Collapsed);
			m_btnUp.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Empty.jpg");
			m_btnUp.Content = "..";
			m_btnUp.Opacity = 1;
			m_btnUp.Foreground = new SolidColorBrush(Colors.Black);
			m_btnUp.Click += new System.Windows.RoutedEventHandler(m_btnUp_Click);
			
			m_btnNext = new RscIconButton(TitlePanel, Grid.ColumnProperty, 4, 50, 50, Rsc.Visible);
			m_btnNext.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipNext.jpg");
			m_btnNext.Click += new System.Windows.RoutedEventHandler(m_btnNext_Click);
			
			m_btnFldr = new RscIconButton(TitlePanel, Grid.ColumnProperty, 5, 50, 50, Rsc.Visible);
			m_btnFldr.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_(dir).jpg");
			m_btnFldr.Click += new System.Windows.RoutedEventHandler(m_btnFldr_Click);
			
			appArgs = new RscPageArgsRetManager();
			
			m_tmrLoad = new DispatcherTimer();
			m_tmrLoad.Interval = new TimeSpan(500);
			m_tmrLoad.Tick += new System.EventHandler(m_tmrLoad_Tick);
			
			this.Loaded += new System.Windows.RoutedEventHandler(RscThemesV10_Loaded);
			ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);

			Touch.FrameReported += new System.Windows.Input.TouchFrameEventHandler(Touch_FrameReported);
			m_ptTouchDown = new Point(0,0);
			
			m_tmrBrowse = new DispatcherTimer();
			m_tmrBrowse.Interval = new TimeSpan(500);
			m_tmrBrowse.Tick += new System.EventHandler(m_tmrBrowse_Tick);
 			
			m_AppFrame.ShowButtonNext( false );
			
			string sPath = RscKnownFolders.GetMediaPath("DCVID");
			
			m_Root = new RscStoreItemFolder(sPath);
      }
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			m_bInThisApp = true;
			
			if( appArgs.Waiting )
			{
				RscPageArgsRet appOutput = appArgs.GetOutput();
				if( appOutput != null )
				{
					switch( appOutput.ID )
					{
						
						case "FullPath" :
							if( appOutput.GetFlag(0) == "Ok" )
							{
								string sPath = appOutput.GetData(0);
								
								m_Root = new RscStoreItemFolder(sPath);
			
								m_btnUp.Visibility = Rsc.Collapsed;
			
								m_iTop = 0;
								
								m_iYCur = -1;
								m_iXCur = -1;
								
								m_bIsInSwipe = false;
								
								BuildListEx();
							}
							else
							{
								//NOP...
							}
							break;
							
					}
				}
				
				appArgs.Clear();
			}
		}
		
		protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
    	{
			m_bInThisApp = false;
		}
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			/*
			if( m_iSelCount > 0 )
			{
				m_AppFrame.StartTimer( "apply", LayoutRoot, 1, ((m_a.Count - 1) * m_iCX) - 1 );
			}
			else
			{
			*/
				this.NavigationService.GoBack();
			/*
			}
			*/
		}
		
		private void m_AppFrame_OnExit(object sender, EventArgs e)
		{
			this.NavigationService.GoBack();
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( m_Root != null )
			{
				if( m_Root.Parent != null )
				{
					DoUp();
					e.Cancel = true;
				}
			}
			
			//e.Cancel = true;
        }
		
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
			
		private void m_btnFldr_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_tmrBrowse.Start();
		}

		private void m_tmrBrowse_Tick(object sender, System.EventArgs e)
		{
			m_tmrBrowse.Stop();

			RscPageArgsRet appInput = new RscPageArgsRet( appArgs,
				m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "FullPath" );
			appInput.SetFlag( 0, "video folder path" );
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, m_Root.FullPath );
			appInput.SetInput( "RscDlg_FolderInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csDlgsAssy ) );
			
		}
			
		private void m_btnUp_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoUp();
		}
		
		private void DoUp()
		{
			if( m_Root == null ) return;
			if( m_Root.Parent == null ) return;
			
			m_iTop = 0;
								
			m_iYCur = -1;
			m_iXCur = -1;
			
			m_Root = m_Root.Parent;
			
			if( m_Root.Parent == null ) m_btnUp.Visibility = Rsc.Collapsed;
			
			BuildListEx();
		}

		private void RscThemesV10_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			if( !m_bLoaded )
			{
				m_bLoaded = true;
			
				BuildListEx();
			}
		}
		
		private void ClearView()
		{
			foreach( Gallery gal in m_a )
			{
				foreach( GalleryImage galimg in gal.m_a )
				{
					galimg.IbChb = null;
				}
			}
			
			spImgLst.Children.Clear();
		}

		private void m_tmrLoad_Tick(object sender, System.EventArgs e)
		{
			m_tmrLoad.Stop();
			
			int iWidth = (int) m_sContentPanel.Width;
			int iDim = (iWidth - (6 * m_iCX)) / m_iCX;
			m_iCY = Math.Max(1, ((int) ((m_sContentPanel.Height - 50) / (iDim + 30))));
			
			//m_AppFrame.SetStatusText( "m_iCY = " + m_iCY.ToString() );
			
			//ListThemeImages();
			ClearView();
			m_AppFrame.StartTimer( "load", LayoutRoot, 1, Math.Min((m_a.Count - 1) - m_iTop, m_iCY - 1) );
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
			
			if( !m_bLoaded ) return;
			if( m_a.Count == 0 ) return;
			
			//FIX: No refresh if returning from other page...
			if( bNoChng ) return;
			
			int iWidth = (int) m_sContentPanel.Width;
			int iDim = (iWidth - (6 * m_iCX)) / m_iCX;
			m_iCY = Math.Max(1, ((int) ((m_sContentPanel.Height - 50) / (iDim + 30))));
			
			// //
			//
			
			m_iTop = 0;
			
			int iCurItem = -1;
			if( m_iYCur >= 0 && m_iXCur >= 0 )
			{
				iCurItem = (m_iYCur * m_iCX) + m_iXCur;
				
				m_iTop = ((iCurItem / m_iCX) / m_iCY) * m_iCY;
			}
			
			//
			// //
			
			//m_AppFrame.SetStatusText( "m_iCY = " + m_iCY.ToString() );
			
			//ListThemeImages();
			ClearView();
			m_AppFrame.StartTimer( "load", LayoutRoot, 1, Math.Min((m_a.Count - 1) - m_iTop, m_iCY - 1) );
		}
			
		private void m_btnPrev_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoPrev();
		}
		
		private void DoPrev()
		{
			if( m_iTop == 0 ) return;
			
			m_iTop = Math.Max( 0, m_iTop - m_iCY );
								
			m_iYCur = m_iTop;
			m_iXCur = 0;
			
			//ListThemeImages();
			ClearView();
			m_AppFrame.StartTimer( "load", LayoutRoot, 1, Math.Min((m_a.Count - 1) - m_iTop, m_iCY - 1) );
		}
			
		private void m_btnNext_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoNext();
		}
		
		private void DoNext()
		{
			if( (m_iTop + m_iCY) > (m_a.Count - 1) ) return;
			
			m_iTop += m_iCY;
								
			m_iYCur = m_iTop;
			m_iXCur = 0;
			
			//ListThemeImages();
			ClearView();
			m_AppFrame.StartTimer( "load", LayoutRoot, 1, Math.Min((m_a.Count - 1) - m_iTop, m_iCY - 1) );
		}
		
		private void LoadDone()
		{
			string sFldr = "N/A";
			if( m_Root != null ) sFldr = m_Root.GetFullPath(true);
			
			int iCnt = (m_a.Count * m_iCX) - m_iDummiesAtEnd;
			
			string sRev = "";
			if( !m_bOrderAscending ) sRev = " (reverse order)";
			
			int iFrom = (m_iTop * m_iCX) + 1;
			if( m_a.Count == 0 ) iFrom = 0;
			m_txtTitle.Text = sFldr + "\r\n"
				+ iFrom.ToString() + " to "
				+ Math.Min( (m_iTop * m_iCX) + (m_iCY * m_iCX), iCnt).ToString()
				+ " of " + (iCnt).ToString() + sRev;
			
			if( m_iSelCount > 0 )
				m_txtTitle.Text += "\r\n" + "(" + m_iSelCount.ToString() + " selected)";
			
			if( m_iTop == 0 )
				m_btnPrev.Visibility = Rsc.Collapsed;
			else
				m_btnPrev.Visibility = Rsc.Visible;
			
			if( (m_iTop + m_iCY) > (m_a.Count - 1) )
				m_btnNext.Visibility = Rsc.Collapsed;
			else
				m_btnNext.Visibility = Rsc.Visible;
		}
		
		private void m_AppFrame_OnTimer(object sender, RscAppFrameTimerEventArgs e)
		{
			switch( e.Reason )
			{
				
				case "list files" :
				{
					BuildListStep();
					break;
				}
				
				case "load" :
				{
					m_AppFrame.SetStatusText("Loading " + (m_iTop + e.Pos + 1).ToString() + " / " + m_a.Count.ToString() + "...");
					
					ListImageRow(m_iTop + e.Pos);
					
					if( e.Pos >= e.Max )
					{
						m_AppFrame.SetStatusText(); //m_a.Count.ToString() + " items loaded.");
						
						LoadDone();
					}
					
					break;
				}
				
				case "apply" :
				{
					m_AppFrame.SetStatusText("Appliyng " + (e.Pos + 1).ToString() + " / " + (e.Max + 1).ToString() + "...");
					
					CopyImage(e.Pos);
					
					if( e.Pos >= e.Max )
					{
						m_AppFrame.SetStatusText(m_iSelCount.ToString() + " items applied.");
						
						MassCheck( 0, -1, 0, -1, false, false );
						
						MessageBox.Show( "Restart application new Theme to take into effect!");
						
						this.NavigationService.GoBack();
					}
					
					break;
				}
				
				default :
				{
					e.Completed = true;
					break;
				}
				
			}
		}
		
		private void ListImageRow(int iIndex)
		{
			int iWidth = (int) m_sContentPanel.Width;
						
			if( m_a.Count == 0 ) return;
			if( iIndex >= m_a.Count ) return;
			
			int idx = spImgLst.Children.Count + 1;
			
			int iDim = (iWidth - (6 * m_iCX)) / m_iCX;
			
			/*
			int iChbDim = 24;
			*/
			
			/*
			idx--;
			foreach( Theme thm in m_a )
			{
				idx++;
				*/
			
				Gallery gal = m_a[ iIndex ];
			
				Grid grdOut = new Grid();
				grdOut.Name = "grdOut_" + idx.ToString();
				grdOut.Margin = new Thickness(0, 0, 0, 4 );
				ColumnDefinition cd;
				for( int i = 0; i < m_iCX; i++ )
				{
					cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdOut.ColumnDefinitions.Add(cd);
				}
				cd = new ColumnDefinition(); grdOut.ColumnDefinitions.Add(cd);
				spImgLst.Children.Add(grdOut);
				
				for( int i = 0; i < m_iCX; i++ )
				{
					if( gal.m_a[ i ].Name != "blank" )
					{
						string sId = iIndex.ToString() + "|" + i.ToString();
							
						Grid grdIn = new Grid();
						grdIn.Name = "grdIn_" + idx.ToString() + i.ToString();
						RowDefinition rd;
						rd = new RowDefinition(); rd.Height = GridLength.Auto; grdIn.RowDefinitions.Add( rd );
						rd = new RowDefinition(); rd.Height = GridLength.Auto; grdIn.RowDefinitions.Add( rd );
						grdIn.SetValue(Grid.ColumnProperty, i );
						grdOut.Children.Add( grdIn );
						
						RscIconButton ib = new RscIconButton(grdIn, Grid.RowProperty, 0, iDim, iDim, Rsc.Visible);
						ib.BorderThickness = new Thickness(1);
						ib.BorderBrush = new SolidColorBrush(m_AppFrame.Theme.ThemeColors.GalleryItemBorder ); //Colors.LightGray);
						ib.Tag = sId;
						ib.Click += new System.Windows.RoutedEventHandler(m_Image_Click);
						
						Grid grdTit = new Grid();
						grdTit.Name = "grdTit_" + idx.ToString() + i.ToString();;
						//ColumnDefinition cd;
						cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdTit.ColumnDefinitions.Add(cd);
						cd = new ColumnDefinition(); grdTit.ColumnDefinitions.Add(cd);
						grdTit.SetValue(Grid.RowProperty, 1 );
						grdIn.Children.Add( grdTit );
				
						TextBoxDenieEdit tit = new TextBoxDenieEdit(true, true, grdTit, Grid.ColumnProperty, 1);
						tit.Width = iDim;
						tit.Background = new SolidColorBrush(m_AppFrame.Theme.ThemeColors.GalleryItemBack ); //Colors.LightGray);
						tit.Foreground = new SolidColorBrush(m_AppFrame.Theme.ThemeColors.GalleryItemFore ); //Colors.Black);
						tit.FontSize = 16;
						
						string sTitle = gal.m_a[ i ].FsItem.Name;
						if( gal.m_a[ i ].ThumbnailPresent )
							sTitle += " (tn)";
						tit.Text = sTitle;
						if( gal.m_a[ i ].FsItem.Folder )
						{
							ib.Image.Source = m_isFolder;
						}
						else
						{
							if( gal.m_a[ i ].ThumbPath.Length > 0 )
							{
								//m_AppFrame.TRACE = gal.m_a[ i ].Path;
								ib.Image.Source = LoadImage(gal.m_a[ i ].ThumbPath, iDim, iDim);
							}
							else
							{
								ib.Image.Source = m_isVideo;
							}
						}
						
						/*
						gal.m_a[ i ].IbChb = new RscIconButton(grdTit, Grid.ColumnProperty, 0, iChbDim, iChbDim, Rsc.Visible);
						/*
						ibChb.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
						ibChb.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
						ibChb.Margin = new Thickness(iDim - iChbDim, iDim - iChbDim, 0, 0 );
						*
						if( gal.m_a[ i ].Checked )
							gal.m_a[ i ].IbChb.Image.Source = m_isChbOn;
						else
							gal.m_a[ i ].IbChb.Image.Source = m_isChbOff;
						gal.m_a[ i ].IbChb.Tag = sId;
						gal.m_a[ i ].IbChb.Click += new System.Windows.RoutedEventHandler(m_Image_Click);
						*/
					}
				}

			/*
			}
			*/
		}
		
		private ImageSource LoadImage(string sFullPath, int iCX, int iCY)
		{
			if( sFullPath.Length == 0 )
				return m_isDummy;
			
			return LoadThumbnail( sFullPath, iCX, iCY );
		}
		
		private ImageSource LoadThumbnail( string sPath, int iCX, int iCY )
		{
			
			RscStore store = new RscStore();
			
			System.IO.Stream stream = store.GetReaderStream( sPath, false );
			
			System.Windows.Media.Imaging.WriteableBitmap wbmp = 
				Microsoft.Phone.PictureDecoder.DecodeJpeg(stream, iCX, iCY );
			
			return wbmp;
		}
		
		private void m_Image_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_bIsInSwipe ) return;
			m_bIsInSwipe = false;
			
			Button btn = (Button) sender;
			string sId = (string) btn.Tag;
			
			string [] asID = sId.Split('|');
			int iYcurr = Int32.Parse(asID[ 0 ]);
			int iXcurr = Int32.Parse(asID[ 1 ]);
			
			if( m_a[ iYcurr ].m_a[ iXcurr ].FsItem.Folder )
			{
				m_iTop = 0;
				
				m_iYCur = -1;
				m_iXCur = -1;
								
				m_Root = m_a[ iYcurr ].m_a[ iXcurr ].FsItem;
				
				m_btnUp.Visibility = Rsc.Visible;
			
				BuildListEx();
			}
			else
			{
				m_iYCur = iYcurr;
				m_iXCur = iXcurr;
				
				string strFileGroupCurr = RscRegFs.GetFileGroupEx( m_a[ iYcurr ].m_a[ iXcurr ].FsItem.FileExt );
				switch( strFileGroupCurr )
				{
						
					case "Video.Native" :
					case "Video" :
					{
						RscPageArgsRetManager appArgs = new RscPageArgsRetManager();
						
						RscPageArgsRet appInput = new RscPageArgsRet( appArgs,
							m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "Open" );
						
						//DeBug...
						//m_AppFrame.TRACE = m_a.Count.ToString();
						
						int iIdx = 0;
						int i = -1;
						for( int iY = 0; iY < m_a.Count; iY++ )
						{
							for( int iX = 0; iX < m_iCX; iX++ )
							{
								if( m_a[ iY ].m_a[ iX ].Name != "blank" )
								{
									string strFileGroup = RscRegFs.GetFileGroupEx( m_a[ iY ].m_a[ iX ].FsItem.FileExt );
									switch( strFileGroup )
									{
										
										case "Video.Native" :
										{	
											i++;
											appInput.SetData( i, m_a[ iY ].m_a[ iX ].FsItem.FullPath );
											
											if( iY == iYcurr && iX == iXcurr ) iIdx = i;
											
											break;
										}
										
									}
								}
							}
						}
						
						appInput.SetFlag( 0, iIdx.ToString() );
						appInput.SetFlag( 1, LayoutRoot.ActualWidth.ToString() );
						appInput.SetFlag( 2, LayoutRoot.ActualHeight.ToString() );
						
						appInput.SetInput( RscRegFs.GetViewerAppPageName( strFileGroupCurr ) );
						
						this.NavigationService.Navigate( appInput.GetNavigateUri( RscRegFs.GetViewerAppAssyName( strFileGroupCurr ) ) );
						
						break;
					}
						
					default :
						MessageBox.Show("No open action defined for file type.");
						break;
					
				}
			}
			
			/*
			if( m_a[ iYcurr ].m_a[ iXcurr ].Checked )
			{
				MassCheck( iYcurr, iYcurr, iXcurr, iXcurr, false );
			}
			else
			{
				MassCheck( iYcurr, iYcurr, iXcurr, iXcurr, true );
			}
			*/
		}
		
		private void MassCheck( int iYstart, int iYend, int iXstart, int iXend, bool bChecked, bool bSetStatus = true )
		{
			
			if( m_a.Count == 0 ) return;
			
			if( iYend == -1 ) iYend = m_a.Count - 1;
			if( iXend == -1 ) iXend = m_a[0].m_a.Count - 1;
			
			for( int iY = iYstart; iY <= iYend; iY++ )
			{
				for( int iX = iXstart; iX <= iXend; iX++ )
				{
					if( bChecked )
					{
						if( !m_a[ iY ].m_a[ iX ].Checked )
						{
							//Only if image exists!!!
							if( m_a[ iY ].m_a[ iX ].FsItem.FullPath.Length > 0 )
							{
								m_a[ iY ].m_a[ iX ].Checked = true;
								
								if( m_a[ iY ].m_a[ iX ].IbChb != null )
								{
									m_a[ iY ].m_a[ iX ].IbChb.Image.Source = m_isChbOn;
								}
							
								m_iSelCount++;
							}
						}
					}
					else
					{
						if( m_a[ iY ].m_a[ iX ].Checked )
						{
							m_a[ iY ].m_a[ iX ].Checked = false;
							
							if( m_a[ iY ].m_a[ iX ].IbChb != null )
							{
								m_a[ iY ].m_a[ iX ].IbChb.Image.Source = m_isChbOff;
							}
							
							m_iSelCount--;
						}
					}
				}
			}
			
			if( bSetStatus )
			{
				/*
				m_AppFrame.SetStatusText(m_iSelCount.ToString() + " of "
					+ ((m_a.Count - 1) * m_iCX).ToString() + " selected.");
				*/
				
				LoadDone();
			}
		}
		
		private void CopyImage( int iPos )
		{
			/*
			if( m_a.Count == 0 ) return;
			
			int iY = (iPos / m_iCX) + 1;
			int iX = iPos % m_iCX;
			
			//m_AppFrame.SetStatusText( iY.ToString() + " | " + iX.ToString() );
			
			ThemeImg thi = m_a[ iY ].m_a[ iX ];
			
			if( !thi.Checked ) return;
			if( thi.FullPath.Length == 0 ) return;
			
			//m_AppFrame.SetStatusText( thi.FullPath );
			
			RscStore store = new RscStore();
			string sPath;
			
			sPath = m_strThemeFolder + "\\" + "Current" + "\\" + thi.Image + ".png";
			if( store.FileExists( sPath ) )
				store.DeleteFile( sPath );
			
			sPath = m_strThemeFolder + "\\" + "Current" + "\\" + thi.Image + ".jpg";
			if( store.FileExists( sPath ) )
				store.DeleteFile( sPath );
			
			string sPathTrg = thi.FullPath;
			sPathTrg = sPathTrg.Replace( "\\" + thi.Name + "\\", "\\" + "Current" + "\\");
			
			store.CopyFile( thi.FullPath, sPathTrg );
			*/
		}
			
		private void m_btnOrder_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_bOrderAscending )
			{
				m_bOrderAscending = !m_bOrderAscending;
				m_btnOrder.Image.Source = m_isUp;
			}
			else
			{
				m_bOrderAscending = !m_bOrderAscending;
				m_btnOrder.Image.Source = m_isDown;
			}
			
			m_iYCur = -1;
			m_iXCur = -1;
			
			m_bIsInSwipe = false;
			
			BuildListEx();
		}

        private void BuildListEx()
        {
			m_a.Clear();
			
			m_iSelCount = 0;
			m_iDummiesAtEnd = 0;
			
			m_AppFrame.SetStatusText( "Listing items..." );
			m_AppFrame.StartTimer( "list files", LayoutRoot, 1, 0 );
		}
		
		private void BuildListStep()
		{
			
			// //
			//
			
			m_Root.Parse( "Video", true, (!m_bOrderAscending) );
			
			//
			// //
			//
			
			RscStore store = new RscStore();
			
			bool bBreak = false;
			Gallery gal = null;
			int iFle = -1;
			for(;;)
			{				
				for( int iX = 0; iX < m_iCX; iX++ )
				{
					iFle++;
					
					if( iX == 0 )
					{
						if( iFle >= m_Root.Count )
						{
							bBreak = true;
							break;
						}
						
						gal = new Gallery();
						m_a.Add( gal );
					}
				
					GalleryImage galimg = new GalleryImage( "N/A", "" );
					gal.m_a.Add( galimg );
					
					if( iFle >= m_Root.Count )
					{
						galimg.Name = "blank";
						galimg.FsItem = null;
						
						m_iDummiesAtEnd++;
					}
					else
					{
						galimg.Name = "Image";
						galimg.FsItem = m_Root.Item( iFle );
						
						galimg.ThumbPath = "";
						
						if( !galimg.FsItem.Folder )
						{
							string sTn = m_Root.FullPath + "\\" + "tn" + "\\" + galimg.FsItem.FileName;
							sTn += ".tn";
							sTn += ".jpg";
							
							//m_AppFrame.TRACE = sTn;
							
							if( store.FileExists( sTn ) )
							{
								//m_AppFrame.TRACE = sTn;
								
								galimg.ThumbPath = sTn;
							}
						}
					}
				}
				
				if( bBreak ) break;
			}
			
			//
			// //
			//
			
			m_AppFrame.SetStatusText();
			
			m_tmrLoad.Start();
			
			//
			// //
        }
		
    }
	
}
