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
using System.Windows.Threading;
using System.Windows.Media;

using Ressive.Utils;
using Ressive.Store;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

namespace Lib_RscIPgC_Dlgs
{
	
    public partial class RscDlg_FolderInputV10 : PhoneApplicationPage
    {
		
		const string csViewersAssy = "Lib_RscViewers";
		
		const string csClsName = "RscDlg_FolderInputV10";
		
		RscAppFrame m_AppFrame;
	
		RscPageArgsRet m_AppInput;
	
		RscPageArgsRetManager m_AppArgs;
		
		RscRegistryValueList m_RegHistory;
		
		DispatcherTimer m_tmrBrowse;
		
        public RscDlg_FolderInputV10()
        {
            InitializeComponent();
 			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Folder Input 1.0", "Images/Ico001_Ressive.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			
			m_tmrBrowse = new DispatcherTimer();
			m_tmrBrowse.Interval = new TimeSpan(500);
			m_tmrBrowse.Tick += new System.EventHandler(m_tmrBrowse_Tick);
			
			m_AppArgs = new RscPageArgsRetManager();
			
			RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
			m_AppInput = appArgsMgr.GetInput( csClsName );
			if( m_AppInput != null )
			{
				
				m_AppFrame.AppTitle = m_AppInput.CallerAppTitle;
				m_AppFrame.AppIconRes = m_AppInput.CallerAppIconRes;
				
				lbStr.Text = m_AppInput.GetFlag(0);
				txStr.Text = m_AppInput.GetData(0);
				
				if( txStr.Text.Length == 0 )
				{
					string sPath = "Software\\Ressive.Hu\\" + csClsName + "\\History";
					sPath += "\\" + m_AppInput.CallerAppTitle;
					sPath += "\\" + m_AppInput.ID;
					txStr.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER, sPath, "LastOk", "" );
				}
				
				_LoadHistory();
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
						
						case "BrowseForFolder" :
							
							if( appOutput.GetFlag(0) == "Ok" )
							{
								txStr.Text = appOutput.GetData(0);
							}
							else
							{
								//NOP...
							}
							
							m_AppArgs.Vipe();
							
							break;
							
					}
				}
				
				m_AppArgs.Clear();
			}
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Back" );
				appOutput.SetOutput();
				
				//Maybe user deleted some items...
				m_RegHistory.Flush();
			}
			
			//e.Cancel = true;
        }
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			OnOK();
		}
				
		private void btnOk_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			OnOK();
		}
		
		private void OnOK()
		{
			if( m_AppInput != null )
			{
				string strRes = txStr.Text;
				
				//NoEmpty...
				if( m_AppInput.GetFlag(1).Length > 0 )
				{
					strRes = strRes.Trim();
					
					if( strRes.Length == 0 )
					{
						MessageBox.Show("Value must not be empty!");
						return;
					}
				}
				
				//FileName...
				if( m_AppInput.GetFlag(1).Length > 0 )
				{
					strRes = strRes.Trim();
					
					//string strChk = "\\/:*?\"<>|";
					//string strChk = "/:*?\"<>|";
					string strChk = "/*?\"<>|";
					foreach( char cChk in strChk )
					{
						if( strRes.IndexOf( cChk ) >= 0 )
						{
							MessageBox.Show("Value must not contain characters of '" + strChk + "'!");
							return;
						}
					}
				}
				
				//Folder exits...
				{
					RscStore store = new RscStore();
					
					strRes = strRes.Trim();
					
					if( !store.FolderExists( strRes ) )
					{
						MessageBox.Show("Folder does not exist!");
						return;
					}
				}
				
				string sPath = "Software\\Ressive.Hu\\" + csClsName + "\\History";
				sPath += "\\" + m_AppInput.CallerAppTitle;
				sPath += "\\" + m_AppInput.ID;
				RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER, sPath, "LastOk", txStr.Text );
				
				m_RegHistory.Add( txStr.Text );
				m_RegHistory.Flush();
				
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Ok" );
				appOutput.SetData( 0, txStr.Text );
				appOutput.SetOutput();
			}

			this.NavigationService.GoBack();
		}
		
		private void m_AppFrame_OnExit(object sender, EventArgs e)
		{
			OnCancel();
		}
				
		private void btnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			OnCancel();
		}
		
		private void OnCancel()
		{
			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Cancel" );
				appOutput.SetOutput();
				
				//Maybe user deleted some items...
				m_RegHistory.Flush();
			}

			this.NavigationService.GoBack();
		}
		
		private void _LoadHistory()
		{
			spHis.Children.Clear();
			
			string sPath = "Software\\Ressive.Hu\\" + csClsName + "\\History";
			sPath += "\\" + m_AppInput.CallerAppTitle;
			sPath += "\\" + m_AppInput.ID;
			
			m_RegHistory = new RscRegistryValueList(HKEY.HKEY_CURRENT_USER, sPath);
			
			for( int i = 0; i < m_RegHistory.Count; i++ )
			{
				_AddHis( m_RegHistory.Get( i ), i );
			}
			if( m_RegHistory.Count == 0 )
			{
				_AddHis( "<history is empty>", 0, true );
			}
		}
		
		private void _AddHis(string sValue, int iIndex, bool bDummyItem = false)
		{
			
			int idx = spHis.Children.Count + 1;
			
			Grid grdOut = new Grid();
			grdOut.Name = "grdOut_" + idx.ToString();
			grdOut.Margin = new Thickness(0, 0, 0, 4 );
			ColumnDefinition cd;
			cd = new ColumnDefinition(); cd.Width = new GridLength(1, GridUnitType.Star); grdOut.ColumnDefinitions.Add(cd);
			cd = new ColumnDefinition(); cd.Width = GridLength.Auto; grdOut.ColumnDefinitions.Add(cd);
			spHis.Children.Add(grdOut);
			grdOut.Tag = iIndex;
			
			TextBoxDenieEdit txtItem = new TextBoxDenieEdit(true, true, grdOut, Grid.ColumnProperty, 0);
			txtItem.Background = new SolidColorBrush(Colors.Gray);
			txtItem.Foreground = new SolidColorBrush(Colors.White);
			//txtItem.FontSize = 16;
			txtItem.Text = sValue;
			
			if( bDummyItem ) return;
			
			Button btn = txtItem.ButtonShield;
			btn.Click += new System.Windows.RoutedEventHandler(btn_Click);
			
			RscIconButton btnRemove = new RscIconButton(grdOut, Grid.ColumnProperty, 1, 36, 36, Rsc.Visible);
			btnRemove.Image.Source = m_AppFrame.Theme.GetImage("Images/Btn001_Remove.jpg");
			btnRemove.Tag = grdOut;
			btnRemove.Click += new System.Windows.RoutedEventHandler(btnRemove_Click);
		
		}

		private void btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn = (Button) sender;
			TextBoxDenieEdit txtItem = (TextBoxDenieEdit) btn.Tag;
			
			txStr.Text = txtItem.Text;
			
			m_AppFrame.AutoClick( btnOk, btnOk_Click );
		}

		private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btnRemove = (Button) sender;
			Grid grdOut = (Grid) btnRemove.Tag;
			int iIndex = (int) grdOut.Tag;
			
			m_RegHistory.Delete( iIndex );
			spHis.Children.Remove(grdOut);
		}
				
		private void btnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			m_tmrBrowse.Start();
		}

		private void m_tmrBrowse_Tick(object sender, System.EventArgs e)
		{
			m_tmrBrowse.Stop();
			
			string strCallerAppTitle = m_AppFrame.AppTitle;
			string strCallerAppIconRes = m_AppFrame.AppIconRes;
			if( m_AppInput != null )		
			{
				strCallerAppTitle = m_AppInput.CallerAppTitle;
				strCallerAppIconRes = m_AppInput.CallerAppIconRes;
			}

			RscPageArgsRet appInput = new RscPageArgsRet( m_AppArgs,
				strCallerAppTitle, strCallerAppIconRes, "BrowseForFolder" );
			appInput.SetFlag( 0, "" ); //FileType Filter (if empty, folder path browsed)
			/*
			appInput.SetFlag( 1, "NoEmpty" );
			appInput.SetFlag( 2, "FileName" );
			appInput.SetData( 0, m_Root.FullPath );
			*/
			appInput.SetInput( "RscViewer_FsV12" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( csViewersAssy ) );
		}
		
    }
	
}
