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
	
    public partial class RscViewer_TextV10 : PhoneApplicationPage
    {
		
		RscAppFrame m_AppFrame;
	
		RscPageArgsRet m_AppInput;
		
		TextBoxDenieEdit m_txtPath;
		
		int m_iIndex = 0;
		List<string> m_aPathes = new List<string>();
		RscTextTags m_tags = null;
			
		RscIconButton m_btnPrev;
		RscIconButton m_btnExtOpen;
		RscIconButton m_btnShare;
		RscIconButton m_btnDelete;
		RscIconButton m_btnNext;
		
		Point m_ptTouchDown;
		bool m_bInThisApp = true;
		bool m_bIsInSwipe = false;
		
		bool m_bFirstLoad = true;
		
		string m_sPath_TEMP = "";
		string m_sContent_TEMP = "";
		
		RscTextLbItemList m_aLines = null;
		
        public RscViewer_TextV10()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Text Viewer 1.0", "Images/IcoSm001_Text.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			m_AppFrame.OnTimer +=new Ressive.FrameWork.RscAppFrame.OnTimer_EventHandler(m_AppFrame_OnTimer);
			
			TitlePanel.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack );
			
			m_btnPrev = new RscIconButton(TitlePanel, Grid.ColumnProperty, 0, 50, 50, Rsc.Visible);
			m_btnPrev.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipPrev.jpg");
			m_btnPrev.Click += new System.Windows.RoutedEventHandler(m_btnPrev_Click);
			
			m_txtPath = new TextBoxDenieEdit(true, true, TitlePanel, Grid.ColumnProperty, 1);
			m_txtPath.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightBack ); //Colors.LightGray);
			m_txtPath.Foreground = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.ToolBarLightFore ); //Colors.Black);
			m_txtPath.FontSize = 16;
			m_txtPath.Text = "N/A";
			
			m_btnExtOpen = new RscIconButton(TitlePanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Visible);
			m_btnExtOpen.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Open.jpg");
			m_btnExtOpen.Click += new System.Windows.RoutedEventHandler(m_btnExtOpen_Click);
			
			m_btnDelete = new RscIconButton(TitlePanel, Grid.ColumnProperty, 3, 50, 50, Rsc.Visible);
			m_btnDelete.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Delete.jpg");
			m_btnDelete.Click += new System.Windows.RoutedEventHandler(m_btnDelete_Click);
			
			m_btnShare = new RscIconButton(TitlePanel, Grid.ColumnProperty, 4, 50, 50, Rsc.Visible);
			m_btnShare.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Share.jpg");
			m_btnShare.Click += new System.Windows.RoutedEventHandler(m_btnShare_Click);
			
			m_btnNext = new RscIconButton(TitlePanel, Grid.ColumnProperty, 5, 50, 50, Rsc.Visible);
			m_btnNext.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipNext.jpg");
			m_btnNext.Click += new System.Windows.RoutedEventHandler(m_btnNext_Click);
			
			lbLines.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.TextLightBack );
			lbLines.Foreground = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.TextLightFore );
			
			m_aLines = new RscTextLbItemList( lbLines, m_AppFrame.Theme );
			
			m_AppFrame.ShowButtonNext( false );

			Touch.FrameReported += new System.Windows.Input.TouchFrameEventHandler(Touch_FrameReported);
			m_ptTouchDown = new Point(0,0);
       }
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			m_bInThisApp = true;
			
			if( !m_bFirstLoad ) return;
			m_bFirstLoad = false;
			
			IDictionary<string, string> parameters = this.NavigationContext.QueryString;
			
			if( parameters.ContainsKey( "folder" ) )
			{
				RscStore store = new RscStore();
				
				string sFolder;
				sFolder = parameters["folder"];
				
				switch( sFolder )
				{
					
					case "A:\\Contacts" :
					{
						m_AppFrame.AppTitle = "Contacts";
						m_AppFrame.AppIconRes = "Images/IcoSm001_Contacts.jpg";
						
						store.CreateFolderPath( sFolder );
						
						string [] asFns = RscSort.OrderBy(store.GetFileNames( sFolder, "*.vcf" ));
						
						foreach( string sFn in asFns )
						{
							string sPath = sFolder + "\\" + sFn;
							
							m_aPathes.Add( sPath );
						}
						
						break;
					}
					
					case "A:\\System\\Events" :
					{
						m_AppFrame.AppTitle = "Events";
						m_AppFrame.AppIconRes = "Images/IcoSm001_EventViewer.jpg";
						
						store.CreateFolderPath( sFolder );
						
						//INFs
						string [] asFns = RscSort.OrderBy(store.GetFileNames( sFolder, "*.info" ));
						foreach( string sFn in asFns )
						{
							string sPath = sFolder + "\\" + sFn;
							
							m_aPathes.Add( sPath );
						}
						
						//ERRORs
						asFns = RscSort.OrderBy(store.GetFileNames( sFolder, "*.error" ));
						foreach( string sFn in asFns )
						{
							string sPath = sFolder + "\\" + sFn;
							
							m_aPathes.Add( sPath );
						}
						
						break;
					}
					
					default :
					{
						store.CreateFolderPath( sFolder );
						
						string [] asFns = RscSort.OrderBy(store.GetFileNames( sFolder, "*.txt" ));
						
						foreach( string sFn in asFns )
						{
							string sPath = sFolder + "\\" + sFn;
							
							m_aPathes.Add( sPath );
						}
						
						break;
					}
					
				}
				
				if( m_aPathes.Count > 0 )
				{
					m_iIndex = 0;
					_ReadTextFile();	
				}
			}
			else if( parameters.ContainsKey( "file" ) )
			{
				m_AppFrame.AppTitle = "Contact";
				m_AppFrame.AppIconRes = "Images/IcoSm001_Contacts.jpg";
				
				string sPath = parameters["file"];
				if( sPath.Length > 0 )
				{	
					m_aPathes.Add( sPath );
					
					m_iIndex = 0;
					_ReadTextFile();	
				}
			}
			else
			{	
				RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
				m_AppInput = appArgsMgr.GetInput( "RscViewer_TextV10" );
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
		}
		
		protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
    	{
			m_bInThisApp = false;
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
					
					if( lbLines != null )
					{
						double dCX = 0;
						
						if( ContentPanel.ActualWidth < ContentPanel.ActualHeight )
							dCX = primaryTouchPoint.Position.X - m_ptTouchDown.X;
						else
							dCX = primaryTouchPoint.Position.Y - m_ptTouchDown.Y;
						
						//Smaller movement...
						dCX = dCX / 5;
							
						lbLines.Margin = new Thickness(dCX, 0, -dCX, 0);
					}
					
					break;
					
				case TouchAction.Up :
					
					if( lbLines != null )
					{
						lbLines.Margin = new Thickness(0);
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
		
		private void _ReadTextFile()
		{
			if( m_iIndex == 0 )
				m_btnPrev.Visibility = Rsc.Collapsed;
			else
				m_btnPrev.Visibility = Rsc.Visible;
			
			if( m_iIndex >= (m_aPathes.Count - 1) )
				m_btnNext.Visibility = Rsc.Collapsed;
			else
				m_btnNext.Visibility = Rsc.Visible;
			
			m_btnShare.Visibility = Rsc.ConditionalVisibility( m_aPathes.Count > 0 );
			
			if( m_aPathes.Count == 0 )
			{
				m_AppFrame.StatusText = "0 of 0";
				m_txtPath.Text = "";
				
				m_aLines.Clear();
				
				return;
			}
			
			m_AppFrame.StatusText = (m_iIndex + 1).ToString() + " of " + m_aPathes.Count.ToString();
			
			m_sPath_TEMP = m_aPathes[ m_iIndex ];
			
			m_txtPath.Text = m_sPath_TEMP;
			
			RscStore store = new RscStore();
			
			bool bNotExist = false;
			m_sContent_TEMP = store.ReadTextFile( m_sPath_TEMP, "", out bNotExist );
			
			if( m_sContent_TEMP.Length == 0 ) return;
			
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
			}
		}
		
		private void LoadContent( )
		{
			string sPath = m_sPath_TEMP;
			m_sPath_TEMP = "";
			string sContent = m_sContent_TEMP;
			m_sContent_TEMP = "";
			
			// //
			//
			
			string sExt = RscStore.ExtensionOfPath( sPath );
			sExt = sExt.ToUpper();
			switch( sExt )
			{
				
				case ".VCF" :
					
					RscTextTags_VCF tags = new RscTextTags_VCF();
					m_tags = tags;
					
					tags.Parse( sContent, "\r\n", ":", ";" );
					
					if( tags.PhotoPresent && tags.PhotoIsBase64 && tags.PhotoIs( "JPEG" ) )
					{
						
						string sBase64 = tags.PhotoData;
						
						//RscFs.WriteTextFile( "vcf.photo.txt", sBase64, true );
						
						byte [] ayImage = Convert.FromBase64String( sBase64 );
						
						if( ayImage != null )
						{
							if( ayImage.Length > 0 )
							{
								
								/*
								RscStore store = new RscStore();
								if( store.FileExists("vcf.photo.jpg") ) store.DeleteFile("vcf.photo.jpg");
								System.IO.Stream stream = store.CreateFile("vcf.photo.jpg");
								stream.Write( ayImage, 0, ayImage.Length );
								stream.Close();
								*/
								
								System.IO.MemoryStream ms = new System.IO.MemoryStream(ayImage.Length);
								ms.Write( ayImage, 0, ayImage.Length );
								ms.Seek( 0, System.IO.SeekOrigin.Begin );
						
								BitmapImage bmp = new BitmapImage();
								bmp.SetSource(ms);
								
								ms.Close();
								
								Img.Source = bmp;
								Img.Visibility = Rsc.Visible;
							}
							else
								Img.Visibility = Rsc.Collapsed;
						}
						else
							Img.Visibility = Rsc.Collapsed;
					}
					else
					{
						//MessageBox.Show( "No photo present!" );
						Img.Visibility = Rsc.Collapsed;
					}
					
					btnCall.Content = "Call " + tags.PhoneNumber( 0 );
					BtnGrid.Visibility = Rsc.ConditionalVisibility( tags.PhoneNumber( 0 ).Length > 0 );
			
					RscStore store = new RscStore();
					
					bool bExists = false;
					if( store.FolderExists( "A:\\Desktop" ) )
					{
						if( store.FolderExists( "A:\\Desktop\\Contacts" ) )
						{
							bExists = store.FileExists("A:\\Desktop\\Contacts\\" + tags.Name + ".txt");
						}
					}
					
					if( bExists )
					{
						btnEx.Content = "Remove from Desktop";
						BtnGridEx.Background = new SolidColorBrush( Colors.Red );
					}
					else
					{
						btnEx.Content = "Add to Desktop";
						BtnGridEx.Background = new SolidColorBrush( Colors.Green );
					}
					BtnGridEx.Visibility = Rsc.ConditionalVisibility( tags.PhoneNumber( 0 ).Length > 0 );
					
					break;
				
				default :
					Img.Visibility = Rsc.Collapsed;
					BtnGrid.Visibility = Rsc.Collapsed;
					m_tags = null;
					break;
				
			}
			
			//
			// //
			
			if( m_tags == null )
			{
				m_aLines.FontSize = 18;
				
				m_aLines.Text = sContent;
			}
			else
			{
				m_aLines.FontSize = 22;
				
				m_aLines.Text = m_tags.ToString();
			}
		}
			
		private void m_btnShare_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_aPathes.Count == 0 ) return;
			
			Microsoft.Phone.Tasks.EmailComposeTask eml = new Microsoft.Phone.Tasks.EmailComposeTask();
		
			eml.Subject = m_aPathes[ m_iIndex ];
			eml.Body = m_aLines.Text;
		
			eml.Show();
		}
			
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
		
		private void btnCall_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_tags == null ) return;
			switch( m_tags.GetType().ToString() )
			{
				
				case "Ressive.Utils.RscTextTags_VCF" :
					Microsoft.Phone.Tasks.PhoneCallTask pct = new Microsoft.Phone.Tasks.PhoneCallTask();
					pct.DisplayName = ((RscTextTags_VCF) m_tags).Name;
					pct.PhoneNumber = ((RscTextTags_VCF) m_tags).PhoneNumber( 0 );
					pct.Show();
					break;
				
				default :
					MessageBox.Show( "No action defined!" );
					break;
				
			}
		}
		
		private void btnSms_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_tags == null ) return;
			switch( m_tags.GetType().ToString() )
			{
				
				case "Ressive.Utils.RscTextTags_VCF" :
					Microsoft.Phone.Tasks.SmsComposeTask sct = new Microsoft.Phone.Tasks.SmsComposeTask();
					sct.To = ((RscTextTags_VCF) m_tags).PhoneNumber( 0 );
					sct.Show();
					break;
				
				default :
					MessageBox.Show( "No action defined!" );
					break;
				
			}
		}
		
		private void btnEx_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_tags == null ) return;
			switch( m_tags.GetType().ToString() )
			{
				
				case "Ressive.Formats.RscTextTags_VCF" :
				{
					RscStore store = new RscStore();
					
					store.CreateFolderPath("A:\\Desktop\\Contacts\\tn");
					
					string sPath   = "A:\\Desktop\\Contacts\\"     + ((RscTextTags_VCF) m_tags).Name + ".txt";
					string sPathTn = "A:\\Desktop\\Contacts\\tn\\" + ((RscTextTags_VCF) m_tags).Name + ".txt.jpg";
					
					if( store.FileExists(sPath) )
					{
						store.DeleteFile( sPath );
						store.DeleteFile( sPathTn );
						
						btnEx.Content = "Add to Desktop";
						BtnGridEx.Background = new SolidColorBrush( Colors.Green );
					}
					else
					{
						
						try
						{
							store.WriteTextFile( sPath, m_aPathes[ m_iIndex ], true );
							
							if( ((RscTextTags_VCF) m_tags).PhotoPresent &&
								((RscTextTags_VCF) m_tags).PhotoIsBase64 &&
								((RscTextTags_VCF) m_tags).PhotoIs( "JPEG" ) )
							{
								
								string sBase64 = ((RscTextTags_VCF) m_tags).PhotoData;
								
								//RscFs.WriteTextFile( "vcf.photo.txt", sBase64, true );
								
								byte [] ayImage = Convert.FromBase64String( sBase64 );
								
								if( store.FileExists(sPathTn) )
								{
									store.DeleteFile(sPathTn);
								}
								
								if( ayImage != null )
								{
									if( ayImage.Length > 0 )
									{
										System.IO.Stream stream = store.CreateFile(sPathTn);
										stream.Write( ayImage, 0, ayImage.Length );
										stream.Close();
									}
								}
							}
						}
						catch( Exception )
						{
							MessageBox.Show( "ERROR: Unable to create Desktop icon for Contact!" );
							return;
						}
						
						btnEx.Content = "Remove from Desktop";
						BtnGridEx.Background = new SolidColorBrush( Colors.Red );
					}
					
					MessageBox.Show( "NOTE: To take into effect, restart application!" );

					break;
				}
				
				default :
					MessageBox.Show( "No action defined!" );
					break;
				
			}
		}
		
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
			
		//Access denied!!!
		/*
		private void m_btnPasteClipboard_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( !System.Windows.Clipboard.ContainsText() )
			{
				MessageBox.Show( "No text on clipboard!" );
				return;
			}
			
			string sClipText = System.Windows.Clipboard.GetText();
			
			string sPath = "Clipboard";
			RscFs.CreateFolderPath(sPath);
			
			DateTime dNow = DateTime.Now;
			sPath += "\\" + dNow.Year.ToString() + RscUtils.pad60(dNow.Month) 
				+ RscUtils.pad60(dNow.Day) + "_" + RscUtils.pad60(dNow.Hour) + RscUtils.pad60(dNow.Minute) 
				+ RscUtils.pad60(dNow.Second) + ".txt";
			
			RscFs.WriteTextFile( sPath, sClipText, true );
			
			m_aPathes.Add( sPath );
			_ReadTextFile();
			
			MessageBox.Show( "Clipboard content saved to " + sPath + " successfuly!" );
		}
		*/
		
		private void TextLbItem_BtnLine_Click(object sender, System.Windows.RoutedEventArgs e)
		{
		}
		
    }
	
}
