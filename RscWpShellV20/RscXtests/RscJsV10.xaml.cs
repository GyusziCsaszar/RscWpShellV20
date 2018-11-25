using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace RscXtests
{
	
    public partial class RscJsV10 : PhoneApplicationPage
    {
		
        public RscJsV10()
        {
            InitializeComponent();
        }
				
		private void btnGo_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			wbc.Navigate(new Uri(txUri.Text, UriKind.Absolute));
		}
		
    }
}