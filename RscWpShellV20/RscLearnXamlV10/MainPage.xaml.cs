using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using RscLearnXamlV10.Resources;

using System.Windows.Media;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Ressive.Utils;
using Ressive.FrameWork;

namespace RscLearnXamlV10
{
    public partial class MainPage : PhoneApplicationPage
    {
		
		const string csAssyName = "/RscLearnXamlV10;component/";
		const string csTitle = "Learn XAML Apps";
		
		private void AddApps( )
		{
			m_asApps.Add( new GalleryItem( "DateDiff", "elapsed days" ) );
			m_asApps.Add( new GalleryItem( "FaviconFinder", "favicon finder" )  );
			m_asApps.Add( new GalleryItem( "FaviconViewer", "favicon viewer" )  );
			m_asApps.Add( new GalleryItem( "ClockSaver", "clock screensaver" )  );
			m_asApps.Add( new GalleryItem( "NoteEditor", "note editor" )  );
			m_asApps.Add( new GalleryItem( "ColorMatchGame", "color match game" )  );
			m_asApps.Add( new GalleryItem( "Anniversary", "anniversary manager" )  );
			m_asApps.Add( new GalleryItem( "PerDay", "amunt per day" )  );
		}
		
		RscAppFrame m_AppFrame;
		ImageSource m_isChbOn = null;
		ImageSource m_isChbOff = null;
		ImageSource m_isDummy = null;
		ImageSource m_isFolder = null;
		
		TextBoxDenieEdit m_txtTitle;
		RscIconButton m_btnPrev;
		RscIconButton m_btnNext;
		
		class GalleryItem
		{
			
			public string Name;
			
			public string Title;
			
			public GalleryItem(string sName, string sTitle)
			{
				Name = sName;
				
				Title = sTitle;
			}
			
		}
		
		class Gallery
		{
			
			public List<GalleryItem> m_a = null;
			
			public Gallery()
			{
				m_a = new List<GalleryItem>();
			}
		}
		
		bool m_bLoaded = false;
		Size m_sContentPanel = new Size(100, 100);
		DispatcherTimer m_tmrLoad;
		
		List<Gallery> m_a = new List<Gallery>();
		int m_iSelCount = 0;
		int m_iTop = 0;
		int m_iCX = 3;
		int m_iCY = 1;
		int m_iDummiesAtEnd = 0;
		
		Point m_ptTouchDown;
		
		bool m_bInThisApp = true;
		
		List<GalleryItem> m_asApps = new List<GalleryItem>();
		
		public MainPage()
        {
            InitializeComponent();
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Learn XAML Apps 1.0", "Images/Ico001_Ressive.jpg"
				, this ); //, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			// ///////////////
			m_isChbOn = m_AppFrame.Theme.GetImage("Images/CheckOn.jpg");
			m_isChbOff = m_AppFrame.Theme.GetImage("Images/CheckOff.jpg");
			m_isDummy = m_AppFrame.Theme.GetImage("Images/Img001_Dummy.jpg");
			m_isFolder = m_AppFrame.Theme.GetImage("Images/Type001_[dir].jpg");
			
			m_btnPrev = new RscIconButton(TitlePanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Collapsed);
			m_btnPrev.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipPrev.jpg");
			m_btnPrev.Click += new System.Windows.RoutedEventHandler(m_btnPrev_Click);
			
			m_txtTitle = new TextBoxDenieEdit(true, true, TitlePanel, Grid.ColumnProperty, 1);
			m_txtTitle.Background = new SolidColorBrush(Colors.LightGray);
			m_txtTitle.Foreground = new SolidColorBrush(Colors.Black);
			m_txtTitle.FontSize = 16;
			m_txtTitle.Text = csTitle;
			
			m_btnNext = new RscIconButton(TitlePanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Collapsed);
			m_btnNext.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipNext.jpg");
			m_btnNext.Click += new System.Windows.RoutedEventHandler(m_btnNext_Click);
			
			m_tmrLoad = new DispatcherTimer();
			m_tmrLoad.Interval = new TimeSpan(500);
			m_tmrLoad.Tick += new System.EventHandler(m_tmrLoad_Tick);
			
			this.Loaded += new System.Windows.RoutedEventHandler(RscThemesV10_Loaded);
			ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);

			Touch.FrameReported += new System.Windows.Input.TouchFrameEventHandler(Touch_FrameReported);
			m_ptTouchDown = new Point(0,0);
			
			AddApps();
		}
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			m_bInThisApp = true;
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
					break;
					
				case TouchAction.Move :
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
					
					int iDistance = 0;
					int iDistanceOther = 0;
					
					if( ContentPanel.ActualWidth < ContentPanel.ActualHeight )
					{
						iDistance = (int) (primaryTouchPoint.Position.X - m_ptTouchDown.X);
						iDistanceOther = (int) (primaryTouchPoint.Position.Y - m_ptTouchDown.Y);
					}
					else
					{
						iDistance = (int) (primaryTouchPoint.Position.Y - m_ptTouchDown.Y);
						iDistanceOther = (int) (primaryTouchPoint.Position.X - m_ptTouchDown.X);
					}
					if( iDistanceOther < 0 ) iDistanceOther *= -1;
					
					//SetStatus(iDistanceOther.ToString());
					
					if( iDistance > 50 && iDistanceOther < 200 )
					{
						//lblStatus.Text = "...to the Right (" + iDistance.ToString() + ")...";
						DoPrev();
					}
					else if( iDistance < -50 && iDistanceOther < 200 )
					{
						//lblStatus.Text = "...to the Left (" + iDistance.ToString() + ")...";
						DoNext();
					}
					break;
			}			
		}

		private void RscThemesV10_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			if( !m_bLoaded )
			{
				m_bLoaded = true;
			
				BuildList();
			
				m_tmrLoad.Start();
			}
		}
		
		private void ClearView()
		{
			foreach( Gallery gal in m_a )
			{
				foreach( GalleryItem galimg in gal.m_a )
				{
					//galimg.IbChb = null;
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
			
			m_iTop = 0;
			
			int iWidth = (int) m_sContentPanel.Width;
			int iDim = (iWidth - (6 * m_iCX)) / m_iCX;
			m_iCY = Math.Max(1, ((int) ((m_sContentPanel.Height - 50) / (iDim + 30))));
			
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
			
			//ListThemeImages();
			ClearView();
			m_AppFrame.StartTimer( "load", LayoutRoot, 1, Math.Min((m_a.Count - 1) - m_iTop, m_iCY - 1) );
		}
		
		private void LoadDone()
		{
			
			int iCnt = (m_a.Count * m_iCX) - m_iDummiesAtEnd;
			
			int iFrom = (m_iTop * m_iCX) + 1;
			if( m_a.Count == 0 ) iFrom = 0;
			
			m_txtTitle.Text =  csTitle + " (" + iFrom.ToString() + " to "
				+ Math.Min( (m_iTop * m_iCX) + (m_iCY * m_iCX), iCnt).ToString()
				+ " of " + (iCnt).ToString() + ")";
			
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
				
				default :
				{
					e.Completed = true;
					break;
				}
				
			}
		}

        private void BuildList()
        {
			m_a.Clear();
			
			m_iSelCount = 0;
			m_iDummiesAtEnd = 0;
			
			// //
			//
			
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
						if( iFle >= m_asApps.Count )
						{
							bBreak = true;
							break;
						}
						
						gal = new Gallery();
						m_a.Add( gal );
					}
				
					GalleryItem galimg = new GalleryItem( "N/A", "N/A" );
					gal.m_a.Add( galimg );
					
					if( iFle >= m_asApps.Count )
					{
						galimg.Name = "blank";
						galimg.Title = "";
						
						m_iDummiesAtEnd++;
					}
					else
					{
						galimg.Name = m_asApps[ iFle ].Name;
						galimg.Title = m_asApps[ iFle ].Title;
					}
				}
				
				if( bBreak ) break;
			}
			
			//
			// //
        }
		
		private void ListImageRow(int iIndex)
		{
			int iWidth = (int) m_sContentPanel.Width;
						
			if( m_a.Count == 0 ) return;
			if( iIndex >= m_a.Count ) return;
			
			int idx = spImgLst.Children.Count + 1;
			
			int iDim = (iWidth - (6 * m_iCX)) / m_iCX;
		
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
					
					tit.Text = gal.m_a[ i ].Title;
					
					try
					{
						BitmapImage bmp = new BitmapImage(new Uri(csAssyName
							+ "/Images/" + gal.m_a[ i ].Name + ".jpg", UriKind.Relative));
						ib.Image.Source = bmp;
					}
					catch( Exception )
					{
						ib.Image.Source = m_isDummy;
					}
				}
			}
		}
		
		private void m_Image_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn = (Button) sender;
			string sId = (string) btn.Tag;
			
			string [] asID = sId.Split('|');
			int iYcurr = Int32.Parse(asID[ 0 ]);
			int iXcurr = Int32.Parse(asID[ 1 ]);
			
			string sUri = csAssyName + m_a[ iYcurr ].m_a[ iXcurr ].Name + ".xaml";
			this.NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}
		
    }
	
}