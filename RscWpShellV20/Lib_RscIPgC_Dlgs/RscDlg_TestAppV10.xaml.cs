using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Ressive.InterPageCommunication;

namespace Lib_RscIPgC_Dlgs
{
	
    public partial class RscDlg_TestAppV10 : PhoneApplicationPage
    {
	
		RscPageArgsRetManager appArgs;
	
        public RscDlg_TestAppV10()
        {
            InitializeComponent();
			
			appArgs = new RscPageArgsRetManager();
        }
		
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if( appArgs.Waiting )
			{
				RscPageArgsRet appOutput = appArgs.GetOutput();
				if( appOutput != null )
				{
					txOut.Text = appOutput.ID + " | " + appOutput.GetFlag(0);
				}
				
				appArgs.Clear();
			}
			else
			{
				txOut.Text = "OnNavigatedTo...";
			}
		}
				
		private void btnMsg_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			txOut.Text = "NavigationService.Navigate...";
			
			RscPageArgsRet appInput = new RscPageArgsRet( appArgs,
				"Rsc Dlg Test V1.0", "Images/Ico001_Ressive.jpg", "ABC" );
			appInput.SetData( 0, txIn.Text );
			appInput.SetInput( "RscDlg_MsgBoxV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( "Lib_RscIPgC_Dlgs" ) );
		}
				
		private void btnTxt_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			txOut.Text = "NavigationService.Navigate...";
			
			RscPageArgsRet appInput = new RscPageArgsRet( appArgs,
				"Rsc Dlg Test V1.0", "Images/Ico001_Ressive.jpg", "DEF" );
			appInput.SetFlag( 0, "sample data input" );
			appInput.SetData( 0, txIn.Text );
			appInput.SetInput( "RscDlg_TxtInputV10" );
			
			this.NavigationService.Navigate( appInput.GetNavigateUri( "Lib_RscIPgC_Dlgs" ) );
		}
		
    }
	
}
