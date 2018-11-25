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
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;

//using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.BackgroundAudio;

using Ressive.Utils;
using Ressive.Store;
using Ressive.Theme;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

namespace Lib_RscViewers
{
	
    public partial class RscViewer_SoundV11 : PhoneApplicationPage
    {
		const string csPageName = "RscViewer_SoundV11"; //Used by FileTypes...
		
		const string csDlgsAssy = "Lib_RscIPgC_Dlgs";
		
		RscAppFrame m_AppFrame;
	
		RscPageArgsRetManager m_AppArgs;
		
		ImageSource m_isPlay;
		ImageSource m_isPause;
		
		RscIconButton m_btnPrev;
		RscIconButton m_btnPlayPause;
		TextBoxDenieEdit m_txtSnd;
		RscIconButton m_btnExtOpen;
		RscIconButton m_btnStop;
		RscIconButton m_btnNext;
		
		//RscIconButton m_btnShowHide;
		RscIconButton m_btnFldrOpen;
		DispatcherTimer m_tmrBrowse;
			
		RscPageArgsRet appInput;
		
		MySoundInfoList m_sounds = new MySoundInfoList();
		
		SoundInfo m_siInPlayer = null;
		
		DispatcherTimer m_currentPosition = new DispatcherTimer();
		
		bool m_bLoaded = false;
		
		//Size m_sContentPanel = new Size(100, 100);
		
        public RscViewer_SoundV11()
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
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Sound Player 1.1", "Images/IcoSm001_SoundPlayer.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			//m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			// ///////////////
			m_isPlay = m_AppFrame.Theme.GetImage("Images/Btn001_Play.jpg");
			m_isPause = m_AppFrame.Theme.GetImage("Images/Btn001_Pause.jpg");
			
			// //
			//
			
			m_btnPrev = new RscIconButton(TitlePanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Collapsed);
			m_btnPrev.Image.Source = m_AppFrame.Theme.GetImage("Images/BtnDrk001_SkipPrev.jpg");
			m_btnPrev.Click += new System.Windows.RoutedEventHandler(m_btnPrev_Click);
			
			m_btnPlayPause = new RscIconButton(TitlePanel, Grid.ColumnProperty, 1, 50, 50, Rsc.Collapsed);
			m_btnPlayPause.Image.Source = m_isPlay;
			m_btnPlayPause.Click += new System.Windows.RoutedEventHandler(m_btnPlayPause_Click);
			
			m_txtSnd = new TextBoxDenieEdit(true, true, TitlePanel, Grid.ColumnProperty, 2);
			m_txtSnd.Background = new SolidColorBrush(Colors.Black); //Colors.LightGray);
			m_txtSnd.Foreground = new SolidColorBrush(Colors.LightGray); //Colors.Black);
			m_txtSnd.FontSize = 16;
			m_txtSnd.Text = "";
			
			m_btnExtOpen = new RscIconButton(TitlePanel, Grid.ColumnProperty, 3, 50, 50, Rsc.Collapsed);
			m_btnExtOpen.Image.Source = m_AppFrame.Theme.GetImage("Images/BtnDrk001_Open.jpg");
			m_btnExtOpen.Click += new System.Windows.RoutedEventHandler(m_btnExtOpen_Click);
			
			m_btnFldrOpen = new RscIconButton(TitlePanel, Grid.ColumnProperty, 4, 50, 50, Rsc.Visible);
			m_btnFldrOpen.Image.Source = m_AppFrame.Theme.GetImage("Images/BtnDrk001_(dir).jpg");
			m_btnFldrOpen.Click += new System.Windows.RoutedEventHandler(m_btnFldrOpen_Click);
			
			m_btnStop = new RscIconButton(TitlePanel, Grid.ColumnProperty, 5, 50, 50, Rsc.Collapsed);
			m_btnStop.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Stop.jpg");
			m_btnStop.Click += new System.Windows.RoutedEventHandler(m_btnStop_Click);
			
			m_btnNext = new RscIconButton(TitlePanel, Grid.ColumnProperty, 6, 50, 50, Rsc.Collapsed);
			m_btnNext.Image.Source = m_AppFrame.Theme.GetImage("Images/BtnDrk001_SkipNext.jpg");
			m_btnNext.Click += new System.Windows.RoutedEventHandler(m_btnNext_Click);
			
			//
			// //
			
			m_AppArgs = new RscPageArgsRetManager();
			
			m_tmrBrowse = new DispatcherTimer();
			m_tmrBrowse.Interval = new TimeSpan(500);
			m_tmrBrowse.Tick += new System.EventHandler(m_tmrBrowse_Tick);
			
			m_sounds.ListBoxAsteriskWidth = 100;
			lbSounds.ItemsSource = m_sounds;
			lbSounds.SizeChanged += new System.Windows.SizeChangedEventHandler(lbSounds_SizeChanged);
			
			if( bStandAloneApp )
			{
				m_AppFrame.ShowButtonTools( true, "" );
			}
			
			this.Loaded +=new System.Windows.RoutedEventHandler(RscViewer_SoundV10_Loaded);
			this.Unloaded +=new System.Windows.RoutedEventHandler(RscViewer_SoundV10_Unloaded);
			
			m_AppFrame.ShowButtonNext( false );
			
			//ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);
			
			BackgroundAudioPlayer.Instance.PlayStateChanged += new EventHandler(Instance_PlayStateChanged);
        }

		/*
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
		*/
		
		private void Instance_PlayStateChanged(object sender, EventArgs e)
		{
			switch( BackgroundAudioPlayer.Instance.PlayerState )
			{
				
				case PlayState.Playing :
				{
					m_AppFrame.StatusText = "Playing...";
					
					m_currentPosition.Stop();
				
					m_txtSnd.Text = BackgroundAudioPlayer.Instance.Track.Title;
					m_txtSnd.Text += " (" + BackgroundAudioPlayer.Instance.PlayerState.ToString() + ")";
					
					m_btnPlayPause.Image.Source = m_isPause;
					m_btnPlayPause.Visibility = Rsc.Visible;
					
					m_btnExtOpen.Visibility = Rsc.Visible;
					m_btnStop.Visibility = Rsc.Visible;
					
					string sTag = BackgroundAudioPlayer.Instance.Track.Tag;
					if( sTag.Length > 0 )
					{
						int iTag = 0;
						if( Int32.TryParse( sTag, out iTag ) )
						{
							//ATT: Can happen...
							if( (iTag >= 0) && (iTag < m_sounds.Count) )
							{
								try
								{
									//m_txtSnd.Text += " " + iTag.ToString();
									
									m_siInPlayer = m_sounds[ iTag ];
									
									m_btnPrev.Visibility = Rsc.ConditionalVisibility( iTag > 0 );
									m_btnNext.Visibility = Rsc.Visible; //Allowe to restart list... //Rsc.ConditionalVisibility( idx < (m_sounds.Count - 1) );
									
									prsBarLen.Minimum = 0;
									prsBarLen.Maximum = (int) BackgroundAudioPlayer.Instance.Track.Duration.TotalMilliseconds;
									
									m_siInPlayer.sLen = RscUtils.toDurationStr(BackgroundAudioPlayer.Instance.Track.Duration);
									
									//Refresh...
									lbSounds.ItemsSource = null;
									lbSounds.ItemsSource = m_sounds;
									
									m_currentPosition.Start();
								}
								catch( Exception )
								{
									//NOP...
								}
							}
						}
					}
					
					break;
				}
				
				case PlayState.Paused :
				{
					m_AppFrame.StatusText = "Paused...";
										
					m_currentPosition.Stop();
				
					m_txtSnd.Text = BackgroundAudioPlayer.Instance.Track.Title;
					m_txtSnd.Text += " (" + BackgroundAudioPlayer.Instance.PlayerState.ToString() + ")";

					m_btnPlayPause.Image.Source = m_isPlay;
					m_btnPlayPause.Visibility = Rsc.Visible;
					
					m_btnExtOpen.Visibility = Rsc.Visible;
					m_btnStop.Visibility = Rsc.Visible;
					
					break;
				}
				
				case PlayState.Stopped :
				{
					m_AppFrame.StatusText = "Stopped...";
										
					m_currentPosition.Stop();
				
					m_txtSnd.Text = BackgroundAudioPlayer.Instance.Track.Title;
					m_txtSnd.Text += " (" + BackgroundAudioPlayer.Instance.PlayerState.ToString() + ")";
					
					m_btnPlayPause.Image.Source = m_isPlay;
					m_btnPlayPause.Visibility = Rsc.Visible;
					
					m_btnExtOpen.Visibility = Rsc.Collapsed;
					m_btnStop.Visibility = Rsc.Collapsed;
					
					prsBarLen.Value = 0;
					
					break;
				}
				
				default :
				{
					m_txtSnd.Text = "Loading..."; //BackgroundAudioPlayer.Instance.PlayerState.ToString();
					break;
				}
				
			}
		}

		private void RscViewer_SoundV10_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			m_AppFrame.SetNoSleep( true );
			
			if( m_bLoaded ) return;
			m_bLoaded = true;
			
			m_currentPosition.Tick += new System.EventHandler(m_currentPosition_Tick);
			
			RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
			appInput = appArgsMgr.GetInput( csPageName );
			
			if( appInput != null )
			{
				
				m_AppFrame.AppTitle = appInput.CallerAppTitle;
				m_AppFrame.AppIconRes = appInput.CallerAppIconRes;
				
				int iIndex = 0;
				if( !Int32.TryParse( appInput.GetFlag(0), out iIndex ) ) return;
				
				//NOT NEEDED...
				/*
				if( !double.TryParse( appInput.GetFlag(1), out dWidth ) ) return;
				if( !double.TryParse( appInput.GetFlag(2), out dHeight ) ) return;
				*/
				
				m_siInPlayer = null;
				ClearAllSound();
				
				for( int i = 0; i < appInput.DataCount; i++ )
				{
					SoundInfo si = AddSound( appInput.GetData( i ) );
					if( i == iIndex )
					{
						m_siInPlayer = si;
					}
				}
				
				if( m_siInPlayer != null )
				{
					DoPlay();
				}
				
				//Denie to auto-reload on next start...
				appArgsMgr.Vipe();
			}
			else
			{
				string sLastOk = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER, "Software\\Ressive.Hu\\RscViewer_SoundV11"
					, "LastOk", "" );
				if( sLastOk.Length > 0 )
				{
						
					try
					{
						m_siInPlayer = null;
						
						//DO NOT!!!
						//ClearAllSound();
						
						RscStore store = new RscStore();
						
						string[] fles = RscSort.OrderBy(store.GetFileNames(sLastOk, "*.*"));
						foreach( string sFle in fles )
						{
							string sExt = RscStore.ExtensionOfPath(sFle);
									
							// FIX: To support Tube(HD)'s own local storage Video folder
							//      where files are listed without extension!!!
							bool bInclude = (sExt == "");
									
							string strFileGroup = RscRegFs.GetFileGroupEx( sExt );
							switch( strFileGroup )
							{
								case "Sound.Native" :
								case "Video.Native" :
								{
									bInclude = true;
									break;
								}
							}
									
							if (bInclude)
							{
								SoundInfo si = AddSound( sLastOk + "\\" + sFle );
								if( m_siInPlayer == null ) m_siInPlayer = si;
							}
						}
						
						//DO NOT!!!
						/*
						if( m_siInPlayer != null )
						{
							DoPlay();
						}
						*/
					}
					catch( Exception )
					{
					}
					
				}
			}
		}

		private void RscViewer_SoundV10_Unloaded(object sender, System.Windows.RoutedEventArgs e)
		{
			m_AppFrame.SetNoSleep( false );
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
						
						case "SndFldrPath" :
							if( appOutput.GetFlag(0) == "Ok" )
							{
								
								string sFolderIn = appOutput.GetData(0);
								
								m_siInPlayer = null;
								ClearAllSound();
								
								RscStore store = new RscStore();
								
								string[] fles = RscSort.OrderBy(store.GetFileNames(sFolderIn, "*.*"));
								foreach( string sFle in fles )
								{
									string sExt = RscStore.ExtensionOfPath(sFle);
									
									// FIX: To support Tube(HD)'s own local storage Video folder
									//      where files are listed without extension!!!
									bool bInclude = (sExt == "");
									
									string strFileGroup = RscRegFs.GetFileGroupEx( sExt );
									switch( strFileGroup )
									{
										case "Sound.Native" :
										case "Video.Native" :
										{
											bInclude = true;
											break;
										}
									}
									
									if (bInclude)
									{
										SoundInfo si = AddSound( sFolderIn + "\\" + sFle );
										if( m_siInPlayer == null ) m_siInPlayer = si;
									}
								}
								
								if( m_siInPlayer != null )
								{
									DoPlay();
								}
								
								RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER, "Software\\Ressive.Hu\\RscViewer_SoundV11"
									, "LastOk", sFolderIn );
							}
							else
							{
								//NOP...
							}
							break;
							
					}
				}
				
				m_AppArgs.Clear();
			}
		}

		private void lbSounds_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			int iListBoxAsteriskWidth = (int) (e.NewSize.Width - 10);
			//ATT!!! - Otherwise slowdown...
			if( m_sounds.ListBoxAsteriskWidth != iListBoxAsteriskWidth )
			{
				m_sounds.ListBoxAsteriskWidth = iListBoxAsteriskWidth;
				
				if( m_sounds.Count > 0 )
				{
					//ReQuery...
					lbSounds.ItemsSource = null;
					lbSounds.ItemsSource = m_sounds;
				}
			}
		}
		
		/*
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			//NOT WORKING!!!
			/*
			DoStop();
			
			Microsoft.Phone.BackgroundAudio.AudioTrack track = null;
			
			string sPlayList = "";
			
			foreach( SoundInfo si in m_sounds )
			{
				if( sPlayList.Length > 0 )
					sPlayList += "\r\n";
				sPlayList += si.Path + ";" + si.FileTitle + ";" + si.sFolder;
			
				if( track == null )
				{
					track = new Microsoft.Phone.BackgroundAudio.AudioTrack(
						new Uri(si.Path.Replace("\\", "/"), UriKind.Relative),
						si.FileTitle, si.sFolder, "N/A", null);
				}
			}
			
			RscFs.WriteTextFile("BackgroundAudioPlayer.playlist", sPlayList, true);
			
			if( track != null )
			{
				Microsoft.Phone.BackgroundAudio.BackgroundAudioPlayer.Instance.Volume = 1;
				Microsoft.Phone.BackgroundAudio.BackgroundAudioPlayer.Instance.Track = track;
				Microsoft.Phone.BackgroundAudio.BackgroundAudioPlayer.Instance.Play();
			}
			*
			
			NavigationService.GoBack();
		}
		*/
 
		private void m_AppFrame_OnExit(object sender, System.EventArgs e)
		{
			NavigationService.GoBack();
		}
		
       	protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			//e.Cancel = true;
		}
			
		private void m_btnPrev_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			BackgroundAudioPlayer.Instance.SkipPrevious();
		}
			
		private void m_btnPlayPause_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_siInPlayer == null ) return;
			
			switch( BackgroundAudioPlayer.Instance.PlayerState )
			{
				
				case PlayState.Playing :
					BackgroundAudioPlayer.Instance.Pause();
					break;
					
				case PlayState.Paused :
				case PlayState.Stopped :
					BackgroundAudioPlayer.Instance.Play();
					break;
			}
		}
			
		private void m_btnExtOpen_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_siInPlayer == null ) return;
			
			string sPath = m_siInPlayer.Path;
			
			DoStop();
			
			string sErr = "";
			
			if( !RscStore_Storage.LaunchFile( sPath, out sErr ) )
			{
				if( sErr.Length > 0 )
					MessageBox.Show( sErr );
				else
					MessageBox.Show( "No app installed to open this file." );
			}
		}
			
		private void m_btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoStop();
		}
		
		private void DoStop()
		{
			if( m_siInPlayer == null ) return;
			
			switch( BackgroundAudioPlayer.Instance.PlayerState )
			{
				
				case PlayState.Playing :
					BackgroundAudioPlayer.Instance.Stop();
					break;
					
				case PlayState.Paused :
					BackgroundAudioPlayer.Instance.Stop();
					break;
			}
			
		}
			
		private void m_btnNext_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			BackgroundAudioPlayer.Instance.SkipNext();
		}
		
		private void ClearAllSound( )
		{
			//MUST NOT!!!
			/*
			Ressive.MediaEx.RscMediaEx.StopMusic();
			*/
			BackgroundAudioPlayer.Instance.Close();
			
			m_sounds.Clear();
			
			// //
			//
			
			RscStore store = new RscStore();

			store.CreateFolderPath("A:\\System\\AudioPlaybackAgent");
			store.DeleteFile("A:\\System\\AudioPlaybackAgent\\Playlist.txt");
			store.DeleteFile("A:\\System\\AudioPlaybackAgent\\CurrentTrack.txt");
			
			//
			// //
		}
		
		private SoundInfo AddSound( string sPath )
		{
			
			string sExt = "N/A";
			string sTitle = "N/A";
			string sFolder = "";
			
			int iPosDot = sPath.LastIndexOf('.');
			if( iPosDot >= 0 )
			{
				sExt = sPath.Substring(iPosDot + 1);
			}
			else
			{
				iPosDot = sPath.Length;
				sExt = "<NONE>";
			}
				
			int iPosBs = sPath.LastIndexOf('\\');
			if (iPosBs >= 0)
			{
				if( iPosBs < 0 ) iPosBs = -1;
				sTitle = sPath.Substring( iPosBs + 1, (iPosDot - iPosBs) - 1 );
				
				if( iPosBs > 0 )
					sFolder = sPath.Substring(0, iPosBs );
			}
			
			SoundInfo snd = new SoundInfo();
			
			snd.Path = sPath;
			
			snd.FileType = "\r\n" + sExt.ToUpper();
			snd.FileTitle = sTitle;
			snd.sFolder = sFolder;
			
			snd.SoundState = MediaElementState.Closed;
			
			snd.Parent = m_sounds;
			m_sounds.Add( snd );
			
			// //
			//
			
			RscStore store = new RscStore();
			
			store.CreateFolderPath("A:\\System\\AudioPlaybackAgent");
			
			bool bPlNotExist;
			string sPl = store.ReadTextFile("A:\\System\\AudioPlaybackAgent\\Playlist.txt", "", out bPlNotExist);
			
			if( sPl.Length > 0 ) sPl += "\r\n";
			sPl += sFolder + "|" + sTitle + "|" + sPath;
			store.WriteTextFile("A:\\System\\AudioPlaybackAgent\\Playlist.txt", sPl, true );
			
			if( bPlNotExist )
				store.WriteTextFile("A:\\System\\AudioPlaybackAgent\\CurrentTrack.txt", "0", true );
			
			//
			// //
			
			return snd;
		}
		
		private void btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn;
			btn = ((Button) sender);
			SoundInfo si;
			si = (SoundInfo) btn.Tag;
			
			//Needs not...
			/*
			int idx = m_sounds.IndexOf( ai );
			if( lbApps.SelectedIndex != idx )
				lbApps.SelectedIndex = idx;
			*/
			
			if( m_siInPlayer == si ) return;
			
			m_siInPlayer = si;
			DoPlay();			
		}
		
		private void DoPlay()
		{
			if( m_siInPlayer == null ) return;
			
			//m_txtSnd.Text = m_siInPlayer.FileTitle;
			
			int idx = m_sounds.IndexOf( m_siInPlayer );
			
			// //
			//
			
			RscStore store = new RscStore();
			
			store.CreateFolderPath("A:\\System\\AudioPlaybackAgent");
			store.WriteTextFile("A:\\System\\AudioPlaybackAgent\\CurrentTrack.txt", idx.ToString(), true );
			
			//
			// //
			
			bool bErr = true;
			try
			{
				BackgroundAudioPlayer.Instance.Play();
				bErr = false;
			}
			catch( Exception /*e*/ )
			{
				//FIX: App closed, Music stoped, App started + last loaded, Play FAILS!!!
				//MessageBox.Show( e.Message );
			}
			
			if( bErr )
			{
				try
				{
					//FIX: App closed, Music stoped, App started + last loaded, Play FAILS!!!
					BackgroundAudioPlayer.Instance.Close();
				}
				catch( Exception /*e*/ )
				{
				}
			}
		}

		private void m_currentPosition_Tick(object sender, System.EventArgs e)
		{
			if( m_siInPlayer == null ) return;
			
			/*
			if( sndFull.Position.TotalMilliseconds > sndFull.NaturalDuration.TimeSpan.TotalMilliseconds )
			{
				//Can happen...
				prsBarLen.Maximum = (int) sndFull.Position.TotalMilliseconds;
			}
			*/
			
			try
			{
				prsBarLen.Value = (int) BackgroundAudioPlayer.Instance.Position.TotalMilliseconds;
			}
			catch( Exception )
			{
				m_currentPosition.Stop();
			}
		}
		
		private void m_btnFldrOpen_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_tmrBrowse.Start();
		}

		private void m_tmrBrowse_Tick(object sender, System.EventArgs e)
		{
			m_tmrBrowse.Stop();

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				m_AppFrame.AppTitle, m_AppFrame.AppIconRes, "SndFldrPath" );
			appInput.SetFlag( 0, "music folder path" );
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, "" );
			appInput.SetInput( "RscDlg_FolderInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csDlgsAssy ) );
		}
		
    }
	
	public class SoundInfo
	{
		
		public SoundInfo This { get { return this; } }
		public MySoundInfoList Parent { set; get; }
		public int ListBoxAsteriskWidth { get{ return Parent.ListBoxAsteriskWidth; } }
		
		public string Path;
		
		public string FileType { get; set; }
		
		public string FileTitle { get; set; }
		
		public string sFolder = "";
		public string sSize = "";
		public string sLen = "";
		public string PlayStateAll
		{
			get
			{
				string s = sFolder;
				
				if( sSize.Length > 0 )	
				{
					if( s.Length > 0 )
						s += "\r\n";
					s += sSize;
				}
				
				if( sLen.Length > 0 )
				{
					if( s.Length > 0 )
						s += /*" | ";*/ "\r\n";
					s += sLen;
				}
				
				return s;
			}
		}
		
		public MediaElementState SoundState { get; set; }
		
	}
	
    public class MySoundInfoList : ObservableCollection<SoundInfo>, IList<SoundInfo>, IList
    {
		public int ListBoxAsteriskWidth { set; get; }
	}

}
