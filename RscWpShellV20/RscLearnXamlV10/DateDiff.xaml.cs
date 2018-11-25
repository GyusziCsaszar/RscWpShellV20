using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace RscLearnXamlV10
{
	
    public partial class DateDiff : PhoneApplicationPage
    {
		
        public DateDiff()
        {
            InitializeComponent();
 			
			DateTime dt = DateTime.Now;
			
			dtin_y.Text = dt.Year.ToString();
			dtin_m.Text = dt.Month.ToString();
			dtin_d.Text = dt.Day.ToString();
			
			btnCalc.Click +=new System.Windows.RoutedEventHandler(btnCalc_Click);
       }

        private void btnCalc_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	cntout.Text = "...";
			
			try
			{
				int iY = Int32.Parse( dtin_y.Text );
				int iM = Int32.Parse( dtin_m.Text );
				int iD = Int32.Parse( dtin_d.Text );
				
				DateTime dtNow = DateTime.Now;
				
				DateTime d1 = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day);
				DateTime d2 = new DateTime(iY, iM, iD);
				
				TimeSpan ts = d1 - d2;
				
				cntout.Text = Math.Round(ts.TotalDays, 0).ToString();
			}
			catch( Exception )
			{
				cntout.Text = "<error>";
			}
        }
		
    }
	
}