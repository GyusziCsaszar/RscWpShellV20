using System;
using System.Collections.Generic;
using Microsoft.Phone.BackgroundAudio;

using Ressive.Utils;
using Ressive.Store;

namespace AudioPlaybackAgent
{
	
	public class AudioPlayer : AudioPlayerAgent
	{
		
		static List<MyTrack> m_aTracks = new List<MyTrack>();
		static int m_iCurrentTrack = 0;

		public AudioPlayer()
			: base()
		{
		}

		protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
		{
			switch ( playState )
			{
				case PlayState.TrackEnded:
					PlayNext(player);
					break;
				case PlayState.TrackReady:
					player.Play();
					break;
				default:
					break;
			}

			NotifyComplete();
		}

		private void PlayNext(BackgroundAudioPlayer player)
		{
			ReadPlaylist();
			m_iCurrentTrack++;
			if( m_iCurrentTrack >= m_aTracks.Count ) m_iCurrentTrack = 0;
			if( m_iCurrentTrack >= m_aTracks.Count ) return;
			
			RscStore store = new RscStore();
			store.CreateFolderPath("A:\\System\\AudioPlaybackAgent");
			store.WriteTextFile("A:\\System\\AudioPlaybackAgent\\CurrentTrack.txt", m_iCurrentTrack.ToString(), true );
			
			Play(player);

		}
		private void PlayPrev(BackgroundAudioPlayer player)
		{
			ReadPlaylist();
			m_iCurrentTrack--;
			if( m_iCurrentTrack < 0 ) m_iCurrentTrack = m_aTracks.Count - 1;
			if( m_iCurrentTrack < 0 ) m_iCurrentTrack = 0;
			if( m_iCurrentTrack >= m_aTracks.Count ) return;
			
			RscStore store = new RscStore();
			store.CreateFolderPath("A:\\System\\AudioPlaybackAgent");
			store.WriteTextFile("A:\\System\\AudioPlaybackAgent\\CurrentTrack.txt", m_iCurrentTrack.ToString(), true );
			
			Play(player);
		}


		protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
		{
			switch ( action )
			{
				case UserAction.FastForward:
					player.FastForward();
					break;
				case UserAction.Pause:
					player.Pause();
					break;
				case UserAction.Play:
					if ( player.PlayerState == PlayState.Paused )
					{
						player.Play();
					}
					else
					{
						Play(player);
					}
					break;
				case UserAction.Rewind:
					player.Rewind();
					break;
				case UserAction.Seek:
					player.Position = (TimeSpan)param;
					break;
				case UserAction.SkipNext:
					PlayNext(player);
					break;
				case UserAction.SkipPrevious:
					PlayPrev(player);
					break;
				case UserAction.Stop:
					player.Stop();
					break;
				default:
					break;
			}

			NotifyComplete();
		}

		protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
		{
			NotifyComplete();
		}

		private void Play(BackgroundAudioPlayer player)
		{			
			ReadPlaylist();
			if( m_iCurrentTrack >= m_aTracks.Count ) return;
			
			try
			{
				RscStore store = new RscStore();
				
				string strAlbArt = "System/Theme/Current/Ico001_SoundPlayer.jpg";
				if( store.FileExists("A:\\System\\Theme\\Current\\Ico001_SoundPlayer.png") )
					strAlbArt = "System/Theme/Current/Ico001_SoundPlayer.png";
				
				string sPath = m_aTracks[m_iCurrentTrack].Path;
				Uri uri;
				if( sPath.ToLower()[0] == 'a' && sPath.ToLower()[1] == ':' )
				{
					//Iso Store...
					uri = new Uri(sPath.Substring(3), UriKind.Relative);
				}
				else
				{
					uri = new Uri("file:///" + sPath, UriKind.Absolute);
				}
				
				var audioTrack = new AudioTrack( uri,
											m_aTracks[m_iCurrentTrack].Title,
											"N/A",
											m_aTracks[m_iCurrentTrack].Album,
											new Uri(strAlbArt, UriKind.Relative),
											m_iCurrentTrack.ToString(),
											EnabledPlayerControls.All);
				player.Track = audioTrack;
			}
			catch( Exception e )
			{
				RscStore.AddSysEvent( e );
			}
		}


		protected override void OnCancel()
		{
			NotifyComplete();
		}
		
		private void ReadPlaylist()
		{
			
			m_iCurrentTrack = 0;
			m_aTracks.Clear();
			
			RscStore store = new RscStore();
			
			store.CreateFolderPath("A:\\System\\AudioPlaybackAgent");
			
			bool bPlNotExist;
			string sPl = store.ReadTextFile("A:\\System\\AudioPlaybackAgent\\Playlist.txt", "", out bPlNotExist);
			
			if( bPlNotExist ) return;
			
			string [] astr = sPl.Split(new String [] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			foreach( string str in astr )
			{
				string [] astrTi = str.Split('|');
				if( astrTi.Length == 3 )
				{
				
					MyTrack t = new MyTrack();
				
					t.Album = astrTi[0];
					t.Title = astrTi[1];
					t.Path = astrTi[2];
					
					m_aTracks.Add( t );
				}
			}
			
			string sCt = store.ReadTextFile("A:\\System\\AudioPlaybackAgent\\CurrentTrack.txt", "0", out bPlNotExist);
			if( !Int32.TryParse(sCt, out m_iCurrentTrack ) ) m_iCurrentTrack = 0;

		}
		
	}
	
	class MyTrack
	{
		public string Album {set; get;}
		public string Title {set; get;}
		public string Path {set; get;}
	}
	
}