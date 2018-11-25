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

using Ressive.InterPageCommunication;

namespace Lib_RscIPgC_Dlgs
{
	
    public partial class RscDlg_MsgBoxV10 : PhoneApplicationPage
    {
		
		const string csClsName = "RscDlg_MsgBoxV10";
	
		RscPageArgsRet m_AppInput;
		
        public RscDlg_MsgBoxV10()
        {
            InitializeComponent();
 			
			RscPageArgsRetManager appArgsMgr = new RscPageArgsRetManager();
			m_AppInput = appArgsMgr.GetInput( csClsName );
			if( m_AppInput != null )
			{
				txMsg.Text = m_AppInput.GetData(0);
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
			}
			
			//e.Cancel = true;
        }
				
		private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
		{

			if( m_AppInput != null )
			{
				RscPageArgsRet appOutput = m_AppInput.CreateOutPut();
				appOutput.SetFlag( 0, "Close" );
				appOutput.SetOutput();
			}
			
			this.NavigationService.GoBack();
		}
		
    }
	
}
