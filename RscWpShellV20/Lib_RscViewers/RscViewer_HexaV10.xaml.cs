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

namespace Lib_RscViewers
{
	
    public partial class RscViewer_HexaV10 : PhoneApplicationPage
    {
		
		const long clLn = 16;
		const long clCb = 8;
		
		RscAppFrame m_AppFrame;
	
		RscPageArgsRet m_AppInput;
		
		TextBoxDenieEdit m_txtPath;
		TextBoxDenieEdit m_txtContent;
		
		string m_sPath = "";
		System.IO.Stream m_stream = null;
		long m_lPos = 0;
			
		RscIconButton m_btnPrev;
		RscIconButton m_btnExtOpen;
		/*
		RscIconButton m_btnShare;
		RscIconButton m_btnDelete;
		*/
		RscIconButton m_btnNext;
		
		Point m_ptTouchDown;
		bool m_bInThisApp = true;
		bool m_bIsInSwipe = false;
		
		bool m_bFirstLoad = true;
		
        public RscViewer_HexaV10()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Hexa Viewer 1.0", "Images/Btn001_().jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			//
			// Register File Type later...
			//
			m_AppFrame.ShowButtonNext( false );
			//m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			
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
			
			/*
			m_btnDelete = new RscIconButton(TitlePanel, Grid.ColumnProperty, 2, 50, 50, Rsc.Visible);
			m_btnDelete.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Delete.jpg");
			m_btnDelete.Click += new System.Windows.RoutedEventHandler(m_btnDelete_Click);
			
			m_btnShare = new RscIconButton(TitlePanel, Grid.ColumnProperty, 3, 50, 50, Rsc.Visible);
			m_btnShare.Image.Source = m_AppFrame.Theme.GetImage("Images/Type001_().jpg");
			m_btnShare.Click += new System.Windows.RoutedEventHandler(m_btnShare_Click);
			*/
			
			m_btnNext = new RscIconButton(TitlePanel, Grid.ColumnProperty, 4, 50, 50, Rsc.Visible);
			m_btnNext.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_SkipNext.jpg");
			m_btnNext.Click += new System.Windows.RoutedEventHandler(m_btnNext_Click);
			
			spText.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.TextDarkBack );
			
			m_txtContent = new TextBoxDenieEdit(true, true, TextPanel, Grid.RowProperty, 0);
			m_txtContent.Background = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.TextDarkBack ); //Colors.Black);
			m_txtContent.Foreground = new SolidColorBrush( m_AppFrame.Theme.ThemeColors.TextDarkFore ); //Colors.LightGray);
			m_txtContent.FontSize = 24;
			m_txtContent.Text = "<empty>";

			Touch.FrameReported += new System.Windows.Input.TouchFrameEventHandler(Touch_FrameReported);
			m_ptTouchDown = new Point(0,0);
        }
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			m_bInThisApp = true;
			
			if( !m_bFirstLoad ) return;
			m_bFirstLoad = false;
			
			RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
			m_AppInput = appArgsMgr.GetInput( "RscViewer_HexaV10" );
			if( m_AppInput != null )
			{
				
				m_AppFrame.AppTitle = m_AppInput.CallerAppTitle;
				m_AppFrame.AppIconRes = m_AppInput.CallerAppIconRes;
				
				if( m_stream != null )
				{
					m_stream.Close();
					m_stream = null;
				}
				m_lPos = 0;
	
				m_sPath = m_AppInput.GetData( 0 );			
				
				//appArgsMgr.Vipe();
				
				RscStore store = new RscStore();
				try
				{
					m_stream = store.GetReaderStream( m_sPath );
				}
				catch( Exception ex )
				{
					MessageBox.Show( ex.Message );
					m_stream = null;
				}
				
				_ShowDump();
			}
		}
		
		protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
    	{
			m_bInThisApp = false;
		}
		
		//
		// Register File Type later...
		//
		/*
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			if( MessageBoxResult.OK == MessageBox.Show( "Do you realy want to open file as a known type?"
				+ "\n\nATT: Bad input file can cause error in app!!!"
				+ "\n\n(Press Back to Cancel!)" ) )
			{
				
				//TODO... ...Open With...
				
			}
			
			/*
			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Next" );
				appOutput.SetOutput();
			}
			
			this.NavigationService.GoBack();
			*
		}
		*/
		
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
			
			if( m_stream != null )
			{
				m_stream.Close();
				m_stream = null;
				m_lPos = 0;
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
			
		private void m_btnPrev_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoPrev();
		}
		
		private void DoPrev()
		{
			if( m_stream == null ) return;
			
			m_lPos = Math.Max( 0, m_lPos - (clLn * clCb) );
			
			_ShowDump();
		}
			
		private void m_btnNext_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DoNext();
		}
		
		private void DoNext()
		{
			if( m_stream == null ) return;
			
			long lPos = m_lPos + (clLn * clCb);
			if( lPos < m_stream.Length )
				m_lPos = lPos;
			
			_ShowDump();
		}
		
		private void _ShowDump()
		{
			if( m_lPos == 0 )
				m_btnPrev.Visibility = Rsc.Collapsed;
			else
				m_btnPrev.Visibility = Rsc.Visible;
			
			long lLen = 0;
			if( m_stream != null )
				lLen = m_stream.Length;
			
			if( (m_lPos + (clLn * clCb)) >= (lLen - 1) )
				m_btnNext.Visibility = Rsc.Collapsed;
			else
				m_btnNext.Visibility = Rsc.Visible;
			
			/*
			m_btnShare.Visibility = Rsc.ConditionalVisibility( m_lPos > 0 );
			*/
			
			if( m_stream == null )
			{
				m_AppFrame.StatusText = "0 to 0";
				m_txtPath.Text = "";
				m_txtContent.Text = "";
				
				return;
			}
			
			m_AppFrame.StatusText = m_lPos.ToString() + " to "
				+ (Math.Min( m_stream.Length - 1, (m_lPos + (clLn * clCb)) - 1 )).ToString() + "\n"
				+ "of " + (m_stream.Length - 1).ToString() + " (" + RscUtils.toMBstr( m_stream.Length - 1, false ) + ")";
			
			m_txtPath.Text = m_sPath;
			
			string sContent = "";
			
			try
			{
				m_stream.Seek( m_lPos, System.IO.SeekOrigin.Begin );
				
				long lChk = m_lPos - 1;
				for( long li = 0; li < clLn; li++ )
				{
					sContent += "\n  ";
					
					string sAsc = "";
					
					bool bBreak = false;
					for( long lj = 0; lj < clCb; lj++ )
					{
						lChk++;
						if( lChk >= m_stream.Length )
						{
							for( long lj2 = lj; lj2 < clCb; lj2++ )
								sContent += " __";
							bBreak = true;
							break;
						}
						
						if( lj == 4 )
							sContent += " ";
						
						int y = m_stream.ReadByte();
						sContent += " " + RscEncode.IntToHexaString( y, 2 );
						
						if( (y < 32) || (y >= 128) )
							sAsc += ".";
						else
							sAsc += ((char) y);
						
					}
					
					sContent += "   " + sAsc;
					
					if( bBreak ) break;
				}
				
				m_txtContent.Text = sContent;
			}
			catch( Exception e )
			{
				m_txtContent.Text = "ERROR: " + e.Message;
			}
		}
		
		private void m_btnExtOpen_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( m_sPath.Length == 0 ) return;
			
			string sErr = "";
			
			if( !RscStore_Storage.LaunchFile( m_sPath, out sErr ) )
			{
				if( sErr.Length > 0 )
					MessageBox.Show( sErr );
				else
					MessageBox.Show( "No app installed to open this file." );
			}
		}
			
		private void m_btnShare_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			/*
			if( m_aPathes.Count == 0 ) return;
			
			Microsoft.Phone.Tasks.EmailComposeTask eml = new Microsoft.Phone.Tasks.EmailComposeTask();
		
			eml.Subject = m_aPathes[ m_iIndex ];
			eml.Body = m_txtContent.Text;
		
			eml.Show();
			*/
		}
			
		private void m_btnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			/*
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
			*/
		}
		
    }
	
}
