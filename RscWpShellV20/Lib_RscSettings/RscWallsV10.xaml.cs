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

using Ressive.Utils;
using Ressive.Store;
using Ressive.FrameWork;

namespace Lib_RscSettings
{
	
    public partial class RscWallsV10 : PhoneApplicationPage
    {
		
		RscAppFrame m_AppFrame;
		ImageSource m_isChbOn = null;
		ImageSource m_isChbOff = null;
		ImageSource m_isDummy = null;
		
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
		
        public RscWallsV10()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Wallpaper Selector 1.0", "Images/IcoSm001_Walls.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			// ///////////////
			m_isChbOn = m_AppFrame.Theme.GetImage("Images/CheckOn.jpg");
			m_isChbOff = m_AppFrame.Theme.GetImage("Images/CheckOff.jpg");
			m_isDummy = m_AppFrame.Theme.GetImage("Images/Img001_Dummy.jpg");
			
			m_tmrLoad = new DispatcherTimer();
			m_tmrLoad.Interval = new TimeSpan(500);
			m_tmrLoad.Tick += new System.EventHandler(m_tmrLoad_Tick);
			
			this.Loaded += new System.Windows.RoutedEventHandler(RscThemesV10_Loaded);
			ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);
       }
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			if( m_iSelCount > 0 )
			{
				m_AppFrame.StartTimer( "apply", LayoutRoot, 1, ((m_a.Count) * m_a[ 0 ].m_a.Count) - 1 );
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

		private void RscThemesV10_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			m_bLoaded = true;
			
			BuildList();
			
			m_tmrLoad.Start();
		}

		private void m_tmrLoad_Tick(object sender, System.EventArgs e)
		{
			m_tmrLoad.Stop();
			
			//ListThemeImages();
			spImgLst.Children.Clear();
			m_AppFrame.StartTimer( "load", LayoutRoot, 1, m_a.Count - 1 );
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
			
			//ListThemeImages();
			spImgLst.Children.Clear();
			m_AppFrame.StartTimer( "load", LayoutRoot, 1, m_a.Count - 1 );
		}
		
		private void m_AppFrame_OnTimer(object sender, RscAppFrameTimerEventArgs e)
		{
			switch( e.Reason )
			{
				
				case "load" :
				{
					m_AppFrame.SetStatusText("Loading " + (e.Pos + 1).ToString() + " / " + m_a.Count.ToString() + "...");
					
					ListThemeImages(e.Pos);
					
					if( e.Pos >= e.Max )
					{
						m_AppFrame.SetStatusText(m_a.Count.ToString() + " items loaded.");
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
						sFldrs += "|" + fldr;
						break;
						
				}
			}
			fldrs = sFldrs.Split('|');
			
			//
			// //
			//
			
			string [] astrKeys = new String [2];
			astrKeys[ 0 ] = "Bk001_portrait";
			astrKeys[ 1 ] = "Bk001_landscape";
			
			//
			// //
			//
		
			int iCount = fldrs.Length;
			for( int i = 0; i < iCount; i++ )
			{
				string fldr = fldrs[ i ];
				
				Theme thm = new Theme();
				m_a.Add( thm );
				foreach( string sImage in astrKeys )
				{
					ThemeImg thmimg = new ThemeImg( fldr, sImage );
					thm.m_a.Add( thmimg );
				}
			}
			
			//
			// //
        }
		
		private void ListThemeImages(int iIndex)
		{
			int iWidth = (int) m_sContentPanel.Width;
						
			if( m_a.Count == 0 ) return;
			if( iIndex >= m_a.Count ) return;
			
			int idx = spImgLst.Children.Count + 1;
			
			int iThmCnt = m_a[0].m_a.Count;
			int iDim = (iWidth - (6 * iThmCnt)) / iThmCnt;
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
				grdOut.Margin = new Thickness(0, 0, 0, 4 );
				ColumnDefinition cd;
				for( int i = 0; i < iThmCnt; i++ )
				{
					cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdOut.ColumnDefinitions.Add(cd);
				}
				cd = new ColumnDefinition(); grdOut.ColumnDefinitions.Add(cd);
				spImgLst.Children.Add(grdOut);
				
				for( int i = 0; i < iThmCnt; i++ )
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
					
					tit.Text = thm.m_a[ i ].Name;
					if( i == 0 )
						tit.Text += " (portrait)";
					else
						tit.Text += " (landscape)";
					
					ib.Image.Source = LoadImage(thm.m_a[ i ].Name, thm.m_a[ i ].Image,
						iDim, iDim, out thm.m_a[ i ].FullPath);
					
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
		
		private ImageSource LoadImage(string sSubFolder, string sFileName, int iCX, int iCY, out string sFullPath)
		{
			sFullPath = "";
			
			RscStore store = new RscStore();
			
			string sPath;
			
			sPath = m_strThemeFolder + "\\" + sSubFolder + "\\" + sFileName + ".png";
			if( store.FileExists( sPath ) )
			{
				sFullPath = sPath;
				return LoadThumbnail( sPath, iCX, iCY );
			}
			
			sPath = m_strThemeFolder + "\\" + sSubFolder + "\\" + sFileName + ".jpg";
			if( store.FileExists( sPath ) )
			{
				sFullPath = sPath;
				return LoadThumbnail( sPath, iCX, iCY );
			}
				
			return m_isDummy;
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
			
			if( m_a[ iY ].m_a[ iX ].Checked )
			{
				MassCheck( iY, iY, iX, iX, false );
			}
			else
			{
				MassCheck( 0, -1, iX, iX, false );
				MassCheck( iY, iY, iX, iX, true );
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
								m_a[ iY ].m_a[ iX ].IbChb.Image.Source = m_isChbOn;
							
								m_iSelCount++;
							}
						}
					}
					else
					{
						if( m_a[ iY ].m_a[ iX ].Checked )
						{
							m_a[ iY ].m_a[ iX ].Checked = false;
							m_a[ iY ].m_a[ iX ].IbChb.Image.Source = m_isChbOff;
							
							m_iSelCount--;
						}
					}
				}
			}
			
			if( bSetStatus )
			{
				m_AppFrame.SetStatusText(m_iSelCount.ToString() + " of "
					+ ((m_a.Count - 1) * m_a[ 0 ].m_a.Count).ToString() + " selected.");
			}
		}
		
		private void CopyImage( int iPos )
		{
			if( m_a.Count == 0 ) return;
			
			int iY = (iPos / m_a[ 0 ].m_a.Count);
			int iX = iPos % m_a[ 0 ].m_a.Count;
			
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
		}
				
    }
	
}
