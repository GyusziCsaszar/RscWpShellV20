using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Ressive.Utils;
using Ressive.Store;

namespace RscLearnXamlV10
{
	
    public partial class PerDay : PhoneApplicationPage
    {
		
        public PerDay()
        {
            InitializeComponent();
 			
			bool bLoaded = false;
			try
			{
				RscStore store = new RscStore();
				
				if( store.FolderExists( "A:\\Documents\\PerDay" ) )
				{
					bool bTmp;
					string sTx = store.ReadTextFile( "A:\\Documents\\PerDay\\Default.txt", "", out bTmp );
					if( sTx.Length > 0 )
					{
						string [] aTx = sTx.Split('|');
						if( aTx.Length == 5 )
						{
							dtin_y.Text = aTx[0];
							dtin_m.Text = aTx[1];
							dtin_d.Text = aTx[2];
							
							amo.Text = aTx[3];
							
							unt.Text = aTx[4];
							
							Calc();
							
							bLoaded = true;
						}
					}
				}
			}
			catch( Exception )
			{
			}

			if( !bLoaded )
			{
				DateTime dt = DateTime.Now;
				
				dtin_y.Text = dt.Year.ToString();
				dtin_m.Text = dt.Month.ToString();
				dtin_d.Text = dt.Day.ToString();
				
				amo.Text = "";
				
				unt.Text = "";
			}
			
			btnCalc.Click +=new System.Windows.RoutedEventHandler(btnCalc_Click);
       }

        private void btnCalc_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			Calc();
		}
		
		private void Calc()
		{
        	cntout.Text = "...";
        	cntout2.Text = "...";
            cntout3.Text = "...";
			
			try
			{
				int iY = Int32.Parse( dtin_y.Text );
				int iM = Int32.Parse( dtin_m.Text );
				int iD = Int32.Parse( dtin_d.Text );
				
				DateTime dtNow = DateTime.Now;
				
				DateTime d1 = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day);
				DateTime d2 = new DateTime(iY, iM, iD);
				
				TimeSpan ts = d2 - d1;
				int iDays =  Math.Max( 1, (int) Math.Round(ts.TotalDays, 0) );
				
				double dAmo = 0;
				if( amo.Text.Length > 0 )
					dAmo = double.Parse(amo.Text);
				
				string sPath = "A:\\Documents\\PerDay";
				RscStore store = new RscStore();
				store.CreateFolderPath( sPath );
				
				if( dAmo != 0 )
				{
					int iRes = (int) Math.Round(dAmo / iDays, 0);
					
					cntout.Text = iRes.ToString();
					if( unt.Text.Length > 0 )
						cntout.Text += " " + unt.Text;
					
					cntout2.Text = Math.Max( 0, (int) Math.Round(ts.TotalDays, 0) ).ToString();

                    ////
                    //

                    string sInf = "";
                    switch (d2.DayOfWeek)
                    {
                        case DayOfWeek.Monday: sInf += "Hétfő"; break;
                        case DayOfWeek.Tuesday: sInf += "Kedd"; break;
                        case DayOfWeek.Wednesday: sInf += "Szerda"; break;
                        case DayOfWeek.Thursday: sInf += "Csütörtök"; break;
                        case DayOfWeek.Friday: sInf += "Péntek"; break;
                        case DayOfWeek.Saturday: sInf += "Szombat"; break;
                        case DayOfWeek.Sunday: sInf += "Vasárnap"; break;
                    }

                    sInf += ", " + RscUtils.pad60(RscUtils.WeekOfYearHU(d2)) + ". hét";

                    cntout3.Text = sInf;

                    //
                    ////
					
					string sCnt = iY.ToString() + "|"
						+ iM.ToString() + "|"
						+ iD.ToString() + "|"
						+ dAmo.ToString() + "|"
						+ unt.Text;
					
					store.WriteTextFile( sPath + "\\" + "Default.txt", sCnt, true );
				}
				else
				{
					cntout.Text = "";
					
					store.DeleteFile( sPath + "\\" + "Default.txt" );
				}
				
			}
			catch( Exception )
			{
				cntout.Text = "<error>";
			}
        }
		
    }
	
}
