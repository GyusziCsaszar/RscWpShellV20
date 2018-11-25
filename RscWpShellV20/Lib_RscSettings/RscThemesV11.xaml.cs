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
using System.Windows.Input;
using System.Text;
using System.Reflection;
using System.Resources;
using System.Threading;

using Ressive.Utils;
using Ressive.Store;
using Ressive.FrameWork;

namespace Lib_RscSettings
{
	
    public partial class RscThemesV11 : PhoneApplicationPage
    {
		
		RscAppFrame m_AppFrame;
		ImageSource m_isChbOn = null;
		ImageSource m_isChbOff = null;
		ImageSource m_isDummy = null;
		
		TextBoxDenieEdit m_txtTitle;
		RscIconButton m_btnPrev;
		RscIconButton m_btnNext;
		
		class ThemeImg
		{
			
			public string Name;
			public string Image;
			
			public string FullPath;
			
			public RscIconButton IbChb;
			public bool Checked;
			
			public ThemeImg(string sName, string sImage)
			{
				Name = sName;
				Image = sImage;
				
				FullPath = "";
				
				IbChb = null;
				Checked = false;
			}
		}
		
		class Theme
		{
			
			public List<ThemeImg> m_a = null;
			
			public Theme()
			{
				m_a = new List<ThemeImg>();
			}
		}
		
		bool m_bLoaded = false;
		Size m_sContentPanel = new Size(100, 100);
		string m_strThemeFolder = "";
		List<Theme> m_a = new List<Theme>();
		DispatcherTimer m_tmrLoad;
		int m_iSelCount = 0;
		
		int m_iTop = 0;
		int m_iCX = 0;
		int m_iCY = 1;
		
		Point m_ptTouchDown;
		
		bool m_bInThisApp = true;
				
        public RscThemesV11()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Theme Selector 1.1", "Images/IcoSm001_Themes.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			// ///////////////
			m_isChbOn = m_AppFrame.Theme.GetImage("Images/CheckOn.jpg");
			m_isChbOff = m_AppFrame.Theme.GetImage("Images/CheckOff.jpg");
			m_isDummy = m_AppFrame.Theme.GetImage("Images/Img001_Dummy.jpg");
			
			TitlePanel.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack );
			
			m_btnPrev = new RscIconButton(TitlePanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible);
			m_btnPrev.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipPrev.jpg");
			m_btnPrev.Click += new System.Windows.RoutedEventHandler(m_btnPrev_Click);
			
			m_txtTitle = new TextBoxDenieEdit(true, true, TitlePanel, Grid.ColumnProperty, 1);
			m_txtTitle.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack ); //Colors.LightGray);
			m_txtTitle.Foreground = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightFore ); //Colors.Black);
			m_txtTitle.FontSize = 16;
			m_txtTitle.Text = "";
			
			m_btnNext = new RscIconButton(TitlePanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Visible);
			m_btnNext.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipNext.jpg");
			m_btnNext.Click += new System.Windows.RoutedEventHandler(m_btnNext_Click);
			
			m_tmrLoad = new DispatcherTimer();
			m_tmrLoad.Interval = new TimeSpan(500);
			m_tmrLoad.Tick += new System.EventHandler(m_tmrLoad_Tick);
			
			this.Loaded += new System.Windows.RoutedEventHandler(RscThemesV10_Loaded);
			ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);

			Touch.FrameReported += new System.Windows.Input.TouchFrameEventHandler(Touch_FrameReported);
			m_ptTouchDown = new Point(0,0);
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
			if( m_iSelCount > 0 )
			{
				m_AppFrame.StartTimer( "apply", LayoutRoot, 1, ((m_a.Count /*- 1*/) * m_iCX) - 1 );
			}
			else
			{
				this.NavigationService.GoBack();
			}
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
			m_bLoaded = true;
			
			BuildList();
			
			m_tmrLoad.Start();
		}
		
		private void ClearView()
		{
			foreach( Theme thm in m_a )
			{
				foreach( ThemeImg thmimg in thm.m_a )
				{
					thmimg.IbChb = null;
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
			m_txtTitle.Text = ((m_iTop * m_iCX) + 1).ToString() + " to "
				+ Math.Min( (m_iTop * m_iCX) + (m_iCY * m_iCX), m_a.Count * m_iCX).ToString()
				+ " of " + (m_a.Count * m_iCX).ToString();
			
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
					
					ListThemeImages(m_iTop + e.Pos);
					
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

        private void BuildList()
        {
			// //
			//
			
			m_strThemeFolder = RscKnownFolders.GetSystemPath( "Theme" );
			
			RscStore store = new RscStore();
			
			string[] fldrs = RscSort.OrderBy(store.GetFolderNames( m_strThemeFolder, "*.*" ));
			string sFldrs = "Default";
			foreach( string fldr in fldrs )
			{
				switch( fldr )
				{
					
					case "Current":
					case "Default":
						//Special folders...
						break;
						
					default :
					{
						string sPathPre = m_strThemeFolder + "\\" + fldr + "\\" + "Theme";
						bool bFullTheme = false;
						
						//There are WallPaper only folders!!!
						if( store.FileExists( sPathPre + ".png" ) )
							bFullTheme = true;
						if( store.FileExists( sPathPre + ".jpg" ) )
							bFullTheme = true;
						
						if( bFullTheme )
							sFldrs += "|" + fldr;
						
						break;
					}
						
				}
			}
			fldrs = sFldrs.Split('|');
			
			//
			// //
			//
			
			var listOfImageResources = new StringBuilder();
			
			//var asm = Assembly.GetExecutingAssembly();
			//MessageBox.Show( asm.FullName );
			//
			// NOT WORKS: //var asm = Assembly.LoadFrom("RscLib.dll");
			//var asm = Assembly.Load("Microsoft.Phone.Media.Extended, Version=7.0.0.0, Culture=neutral, PublicKeyToken=24eec0d8c86cda1e");
			var asm = Assembly.Load("Lib_Rsc, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			
			var mrn = asm.GetManifestResourceNames();
			
			foreach (var resource in mrn)
			{
				var rm = new ResourceManager(resource.Replace(".resources", ""), asm);
			
				try
				{
					var NOT_USED = rm.GetStream("app.xaml"); // without getting a stream, next statement doesn't work - bug?
			
					var rs = rm.GetResourceSet(Thread.CurrentThread.CurrentUICulture, false, true);
			
					var enumerator = rs.GetEnumerator();
					while (enumerator.MoveNext())
					{
						if (enumerator.Key.ToString().StartsWith("images/"))
						{
							listOfImageResources.AppendLine(
								RscEncode.UrlEncodingToString(enumerator.Key.ToString())
							);
						}
					}
				}
				catch (MissingManifestResourceException)
				{
					// Ignore any other embedded resources (they won't contain app.xaml)
				}
			}
			
			string str = listOfImageResources.ToString();
			string [] astrDelims = new String [1];
			astrDelims[ 0 ] = "\r\n";
			string [] astrKeys = RscSort.OrderBy(str.Split(astrDelims, StringSplitOptions.RemoveEmptyEntries));
			
			//
			// //
			//
		
			int iCount = astrKeys.Length;
			for( int i = -1; i < iCount; i++ )
			{
				
				string sImage;
			
				if( i == -1 )
				{
					sImage = "Theme";
				}
				else
				{
					string sTmp = astrKeys[ i ];
					int iPos1 = sTmp.LastIndexOf('/');
					if( iPos1 < 0 ) iPos1 = -1;
					int iPos2 = sTmp.LastIndexOf('.');
					if( iPos2 < 0 ) iPos2 = sTmp.Length;
					sImage = sTmp.Substring(iPos1 + 1, iPos2 - (iPos1 + 1));
				}
				
				string [] astrFldrLst;
				string sName;
				int iPos;
				
				Theme thm = new Theme();
				m_a.Add( thm );
				foreach( string fldr in fldrs )
				{					
					ThemeImg thmimg = new ThemeImg( fldr, sImage );
					thm.m_a.Add( thmimg );
			
					string sPath;
					sPath = m_strThemeFolder + "\\" + thmimg.Name + "\\" + thmimg.Image + ".png";
					if( store.FileExists( sPath ) )
					{
						astrFldrLst = store.GetFileNames(m_strThemeFolder + "\\" + thmimg.Name, thmimg.Image + ".png");
						sName = "";
						if( astrFldrLst.Length == 1 )
						{
							sName = astrFldrLst[0];
						}
						if( sName.Length > 0 )
						{
							iPos = sName.LastIndexOf( '.' );
							if( iPos >= 0 )
							{
								//FIX: Orig value of thmimg.Image (Key) is LOWERCASE!!!
								thmimg.Image = sName.Substring( 0, iPos );
								sPath = m_strThemeFolder + "\\" + thmimg.Name + "\\" + sName;
							}
						}
						
						thmimg.FullPath = sPath;
					}
					else
					{
						sPath = m_strThemeFolder + "\\" + thmimg.Name + "\\" + thmimg.Image + ".jpg";
						if( store.FileExists( sPath ) )
						{
							astrFldrLst = store.GetFileNames(m_strThemeFolder + "\\" + thmimg.Name, thmimg.Image + ".jpg");
							sName = "";
							if( astrFldrLst.Length == 1 )
							{
								sName = astrFldrLst[0];
							}
							if( sName.Length > 0 )
							{
								iPos = sName.LastIndexOf( '.' );
								if( iPos >= 0 )
								{
									//FIX: Orig value of thmimg.Image (Key) is LOWERCASE!!!
									thmimg.Image = sName.Substring( 0, iPos );
									sPath = m_strThemeFolder + "\\" + thmimg.Name + "\\" + sName;
								}
							}
							
							thmimg.FullPath = sPath;
						}
						else
						{
							thmimg.FullPath = ""; //Not found...
						}
					}
				}
			}
			
			//
			// //
			//
			
			if( m_a.Count == 0 )
				m_iCX = 0;
			else
				m_iCX = m_a[ 0 ].m_a.Count;
			
			//
			// //
        }
		
		private void ListThemeImages(int iIndex)
		{
			int iWidth = (int) m_sContentPanel.Width;
						
			if( m_a.Count == 0 ) return;
			if( iIndex >= m_a.Count ) return;
			
			int idx = spImgLst.Children.Count + 1;
			
			int iDim = (iWidth - (6 * m_iCX)) / m_iCX;
			int iChbDim = 24;
			
			/*
			idx--;
			foreach( Theme thm in m_a )
			{
				idx++;
				*/
			
				Theme thm = m_a[ iIndex ];
			
				Grid grdOut = new Grid();
				grdOut.Name = "grdOut_" + idx.ToString();
				if( idx == 1 && m_iTop == 0 )
				{
					grdOut.Margin = new Thickness(0, 0, 0, 12 );
				}
				else
				{
					grdOut.Margin = new Thickness(0, 0, 0, 4 );
				}
				ColumnDefinition cd;
				for( int i = 0; i < m_iCX; i++ )
				{
					cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdOut.ColumnDefinitions.Add(cd);
				}
				cd = new ColumnDefinition(); grdOut.ColumnDefinitions.Add(cd);
				spImgLst.Children.Add(grdOut);
				
				for( int i = 0; i < m_iCX; i++ )
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
					
					if( idx == 1 && m_iTop == 0 )
					{
						tit.Text = thm.m_a[ i ].Name;
						ib.Image.Source = LoadImage(thm.m_a[ i ].FullPath, iDim, iDim);
					}
					else
					{
						tit.Text = thm.m_a[ i ].Image;
						ib.Image.Source = LoadImage(thm.m_a[ i ].FullPath, iDim, iDim);
					}
					
					thm.m_a[ i ].IbChb = new RscIconButton(grdTit, Grid.ColumnProperty, 0, iChbDim, iChbDim, Rsc.Visible);
					/*
					ibChb.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
					ibChb.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
					ibChb.Margin = new Thickness(iDim - iChbDim, iDim - iChbDim, 0, 0 );
					*/
					if( thm.m_a[ i ].Checked )
						thm.m_a[ i ].IbChb.Image.Source = m_isChbOn;
					else
						thm.m_a[ i ].IbChb.Image.Source = m_isChbOff;
					thm.m_a[ i ].IbChb.Tag = sId;
					thm.m_a[ i ].IbChb.Click += new System.Windows.RoutedEventHandler(m_Image_Click);
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
			
			System.IO.Stream stream = store.GetReaderStream(sPath);
			
			System.Windows.Media.Imaging.WriteableBitmap wbmp = 
				Microsoft.Phone.PictureDecoder.DecodeJpeg(stream, iCX, iCY );
			
			stream.Close();
			
			return wbmp;
		}
		
		private void m_Image_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn = (Button) sender;
			string sId = (string) btn.Tag;
			
			string [] asID = sId.Split('|');
			int iY = Int32.Parse(asID[ 0 ]);
			int iX = Int32.Parse(asID[ 1 ]);
			
			if( iY == 0 )
			{
				if( m_a[ iY ].m_a[ iX ].Checked )
				{
					MassCheck( 0, -1, 0, -1, false );
				}
				else
				{
					MassCheck( 0, -1, 0, -1, false );
					MassCheck( 0, -1, iX, iX, true );
				}
			}
			else
			{
				if( m_a[ iY ].m_a[ iX ].Checked )
				{
					MassCheck( iY, iY, iX, iX, false );
				}
				else
				{
					MassCheck( iY, iY, 0, -1, false );
					MassCheck( iY, iY, iX, iX, true );
				}
			}
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
							if( m_a[ iY ].m_a[ iX ].FullPath.Length > 0 )
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
			if( m_a.Count == 0 ) return;
			
			int iY = (iPos / m_iCX); // + 1;
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
			
			if( iY == 0 )
			{
				int iPos2 = thi.FullPath.LastIndexOf( '\\' );
				if( iPos2 >= 0 )
				{
					string sSrc = thi.FullPath.Substring( 0, iPos2 + 1 );
					sSrc += "ThemeColors.xml";
					
					if( store.FileExists( sSrc ) )
					{
						string sTrg = sSrc.Replace( "\\" + thi.Name + "\\", "\\" + "Current" + "\\");
						
						if( store.FileExists( sTrg ) )
							store.DeleteFile( sTrg );
				
						store.CopyFile( sSrc, sTrg );
					}
				}
			}
		}
		
    }
	
}
