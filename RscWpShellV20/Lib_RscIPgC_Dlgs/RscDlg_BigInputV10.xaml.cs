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

using Ressive.Utils;
using Ressive.Store;
using Ressive.FrameWork;
using Ressive.InterPageCommunication;

namespace Lib_RscIPgC_Dlgs
{
	
    public partial class RscDlg_BigInputV10 : PhoneApplicationPage
    {
		
		const string csClsName = "RscDlg_BigInputV10";
		
		RscAppFrame m_AppFrame;
	
		RscPageArgsRet m_AppInput;
		
		/*
		RscRegistryValueList m_RegHistory;
		
		bool m_bUseHistory = true;
		*/
		
        public RscDlg_BigInputV10()
        {
            InitializeComponent();
  			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Big Input 1.0", "Images/Ico001_Ressive.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			
			RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
			m_AppInput = appArgsMgr.GetInput( csClsName );
			if( m_AppInput != null )
			{
				
				m_AppFrame.AppTitle = m_AppInput.CallerAppTitle;
				m_AppFrame.AppIconRes = m_AppInput.CallerAppIconRes;
				
				lbStr.Text = m_AppInput.GetFlag(0);
				txStr.Text = m_AppInput.GetData(0);
				
				//FileName...
				if( m_AppInput.GetFlag(1).Length > 0 )
				{
					btnClearFn.Visibility = Rsc.Visible;
				}

				
				/*
				m_bUseHistory = (m_AppInput.GetFlag(3).Length == 0);
				
				lbHis.Visibility = Rsc.ConditionalVisibility( m_bUseHistory );
				scrl.Visibility = Rsc.ConditionalVisibility( m_bUseHistory );
				
				if( txStr.Text.Length == 0 && m_bUseHistory )
				{
					string sPath = "Software\\Ressive.Hu\\" + csClsName + "\\History";
					sPath += "\\" + m_AppInput.CallerAppTitle;
					sPath += "\\" + m_AppInput.ID;
					txStr.Text = RscRegistry.ReadString( HKEY.HKEY_CURRENT_USER, sPath, "LastOk", "" );
				}
				
				_LoadHistory();
				*/
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
				
				/*
				//Maybe user deleted some items...
				m_RegHistory.Flush();
				*/
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
					
					string strChk = "\\/:*?\"<>|";
					foreach( char cChk in strChk )
					{
						if( strRes.IndexOf( cChk ) >= 0 )
						{
							MessageBox.Show("Value must not contain characters of '" + strChk + "'!");
							return;
						}
					}
				}
				
				/*
				if( m_bUseHistory )
				{
					string sPath = "Software\\Ressive.Hu\\" + csClsName + "\\History";
					sPath += "\\" + m_AppInput.CallerAppTitle;
					sPath += "\\" + m_AppInput.ID;
					RscRegistry.WriteString( HKEY.HKEY_CURRENT_USER, sPath, "LastOk", txStr.Text );
					
					m_RegHistory.Add( txStr.Text );
					m_RegHistory.Flush();
				}
				*/
				
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
				
				/*
				//Maybe user deleted some items...
				if( m_bUseHistory )
					m_RegHistory.Flush();
				*/
			}

			this.NavigationService.GoBack();
		}
		
		/*
		private void _LoadHistory()
		{
			if( !m_bUseHistory ) return;
			
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
			if( !m_bUseHistory ) return;
			
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
		*/
		
		private void btnClearFn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if( txStr.Text.Length == 0 )
				return;
			
			txStr.Text = RscStore.ExtensionOfPath( txStr.Text );
		}
		
    }
	
}