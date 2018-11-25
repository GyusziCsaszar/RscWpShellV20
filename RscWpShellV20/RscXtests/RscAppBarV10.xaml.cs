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
	
    public partial class RscAppBarV10 : PhoneApplicationPage
    {
		
        public RscAppBarV10()
        {
            InitializeComponent();
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
			txtblk.Text = "Event: ApplicationBarIconButton_Click";
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
			txtblk.Text = "Event: ApplicationBarIconButton_Click_1";
        }
		
    }
	
}