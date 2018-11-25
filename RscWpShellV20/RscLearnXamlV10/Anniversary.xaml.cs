using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Collections;
using System.Collections.ObjectModel;

using Ressive.Utils;
using Ressive.Store;

namespace RscLearnXamlV10
{
	
	public class Anniversary_coll_item { public object ObjectA { get; set; } public object ObjectB { get; set; } }
	public class Anniversary_coll : ObservableCollection<NoteEditor_coll_item>, IList<NoteEditor_coll_item>, IList {}
	
    public partial class Anniversary : PhoneApplicationPage
    {
		// //
		// Helpers {
		//ATT!!! substring parameters differ (iChrFrom, iChrTo) !!!
		void alert( string sMsg ) { MessageBox.Show( sMsg ); }
		int parseInt( string sN ) { int iRes = 0; Int32.TryParse( sN, out iRes ); return iRes; }
		// //
		Anniversary host { get{ return this; } }
		Anniversary UI { get{ return this; } }
		// //
		NoteEditor_coll createCollection() { return new NoteEditor_coll(); }
		NoteEditor_coll_item createBindableObject() { return new NoteEditor_coll_item(); }
		// } Helpers
		// //
		
		const string csDocFolder = "A:\\Documents\\Dates";
		
		const int ciDtWidth = 120;
		
		string sTitOld = "";
		string sLstLoadTitle = "";
		
		StackPanel spFiles = null;
		string sEml = "";
		
        public Anniversary()
        {
            InitializeComponent();
 			
			RscStore store = new RscStore();
			store.CreateFolderPath( csDocFolder );
			
			spFiles = (StackPanel) host.UI.scrl.FindName("filesPanel");
			sEml = "";
		
			doRefresh();
			
			btnSndAll.Click += new System.Windows.RoutedEventHandler(btnSndAll_Click);
			btnSend.Click += new System.Windows.RoutedEventHandler(btnSend_Click);
			btnNew.Click += new System.Windows.RoutedEventHandler(btnNew_Click);
			btnSave.Click += new System.Windows.RoutedEventHandler(btnSave_Click);
			btnDel.Click += new System.Windows.RoutedEventHandler(btnDel_Click);
			
			txY.TextChanged += new System.Windows.Controls.TextChangedEventHandler(txtNot_TextChanged);
			txM.TextChanged += new System.Windows.Controls.TextChangedEventHandler(txtNot_TextChanged);
			txD.TextChanged += new System.Windows.Controls.TextChangedEventHandler(txtNot_TextChanged);
			
			txtTit.TextChanged += new System.Windows.Controls.TextChangedEventHandler(txtTit_TextChanged);
       }

        private void btnSndAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			Microsoft.Phone.Tasks.EmailComposeTask eml = new Microsoft.Phone.Tasks.EmailComposeTask();
		
			eml.Subject = "DATES v3";
			eml.Body = sEml;
		
			eml.Show();
        }
		
		private string txDateText()
		{
			
			string sY = txY.Text.ToString();
			string sM = txM.Text.ToString();
			string sD = txD.Text.ToString();
			
			if( sD.Length == 0 )
				sD = "1";
			if( sD.Length == 1 )
				sD = "0" + sD;
			
			if( sY.Length > 0 )
			{
				if( sM.Length == 0 )
					sM = "1";
			}
			if( sM.Length == 1 )
				sM = "0" + sM;
			
			if( sY.Length > 0 )
			{
				if( sY.Length > 4 )
				{
					sY = "1901";
				}
				else
				{
					if( sY.Length < 2 )
						sY = "190" + sY;
					else if( sY.Length < 3 )
						sY = "19" + sY;
					else if( sY.Length < 4 )
						sY = "2" + sY;
				}
			}
			
			string sVal = "";
			
			if( sY.Length > 0 )
				sVal += sY + ".";
			
			if( sM.Length > 0 )
				sVal += sM + ".";
			
			if( sD.Length > 0 )
				sVal += sD + ".";
			
			return sVal;
		}

        private void btnSend_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			Microsoft.Phone.Tasks.EmailComposeTask eml = new Microsoft.Phone.Tasks.EmailComposeTask();
		
			eml.Subject = host.UI.txtTit.Text;
			eml.Body = txDateText();
		
			eml.Show();
        }

        private void btnNew_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			host.UI.txtTit.Visibility = System.Windows.Visibility.Visible;
			host.UI.spDate.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblNot.Visibility = System.Windows.Visibility.Visible;
			host.UI.PageTitle.Visibility = System.Windows.Visibility.Collapsed;
		
			doNew();
			host.UI.txtCch.Text = "0";
        }

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			string sTitle = host.UI.txtTit.Text;
		
			if( sTitle == "" )
			{
				alert("No title!");
				return;
			}
			
			RscStore store = new RscStore();
			
			if( store.FileExists( csDocFolder + "\\" + sTitle + ".txt" ) )
			{
				if( sTitle == sLstLoadTitle )
				{
					//NOP...
				}
				else
				{
					alert("Title already exists!");
					return;
				}
			}
		
			string sVal = txDateText();
			
			store.WriteTextFile( csDocFolder + "\\" + sTitle + ".txt", sVal, true );
		
			sLstLoadTitle = "";
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.btnSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.btnDel.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblDel.Visibility = System.Windows.Visibility.Visible;
		
			doRefresh();
        }

        private void btnDel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			string sTitle = host.UI.txtTit.Text;
		
			if( sTitle == "" )
			{
				alert("No title!");
				return;
			}
			
			RscStore store = new RscStore();
			
			if( store.FileExists( csDocFolder + "\\" + sTitle + ".txt" ) )
			{
				store.DeleteFile( csDocFolder + "\\" + sTitle + ".txt" );
			}
			else
			{
				alert("Title does not exist!");
				return;
			}
		
			doNew();
		
			doRefresh();
        }
		
		private int txDateLength()
		{
			//return txDateText().Length;
			return (txY.Text.ToString().Length + txM.Text.ToString().Length + txD.Text.ToString().Length);
		}

        private void txtNot_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
			string sOld = host.UI.txtCch.Text;
			host.UI.txtCch.Text = txDateLength().ToString();
		
			//alert(sOld);
			if( sOld == "" ) return;
			if( sOld == host.UI.txtCch.Text.ToString() ) return;
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Visible;
			host.UI.btnSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSend.Visibility = System.Windows.Visibility.Visible;
        }

        private void txtTit_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
			if( host.UI.txtTit.Text.ToString() == sTitOld )
				return;
			
			if( host.UI.txtTit.Text.ToString() == "" ) return;
			sTitOld = host.UI.txtTit.Text.ToString();
		
			sLstLoadTitle = "";
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Visible;
			host.UI.btnSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.btnDel.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblDel.Visibility = System.Windows.Visibility.Collapsed;
        }

		private void doRefresh()
		{
			spFiles.Children.Clear();
			sEml = "";
			
			RscStore store = new RscStore();
								
			string[] fles = RscSort.OrderBy(store.GetFileNames(csDocFolder, "*.txt"));
			foreach( string sFle in fles )
			{
				bool bTmp;
				string sTx = store.ReadTextFile( csDocFolder + "\\" + sFle, "", out bTmp );
				if( sTx.Length > 0 )
				{
					addDate(csDocFolder + "\\" + sFle, sTx, RscStore.FileNameOfPath( sFle ));
				}
			}
		}
		
		private void addDate(string sPath, string sDate, string sDesc)
		{
			DateTime dtNow = DateTime.Now;
			string sYnow = dtNow.Year.ToString();
			string sMnow = dtNow.Month.ToString();
		
			int iCyc = 0;
			for( iCyc = 0; iCyc < 2; iCyc++ )
			{
				string sY;
				string sM;
				string sD;
			
				bool bAnniver = (sDate.Substring(2,1) == ".");
				if( bAnniver )
				{
					if( sDate.Length == 3 )
					{
						sY = sYnow;
						sM = sMnow;
						sD = sDate.Substring(0,2);
				
						//FIX...
						//sMnow = (dtNow.Month + 1).ToString();			
						if( dtNow.Month >= 12 )
						{
							sMnow = "1";
							sYnow = (dtNow.Year + 1).ToString();
						}
						else
						{
							sMnow = (dtNow.Month + 1).ToString();
						}
					}
					else
					{
						sY = sYnow;
						sM = sDate.Substring(0,2);
						sD = sDate.Substring(3,2);
				
						sYnow = (dtNow.Year + 1).ToString();
					}
				}
				else
				{
					sY = sDate.Substring(0,4);
					sM = sDate.Substring(5,2);
					sD = sDate.Substring(8,2);
				}
			
				int iY = parseInt(sY);
				int iM = parseInt(sM);
				int iD = parseInt(sD);
			
				dtNow = DateTime.Now;
				DateTime dt1 = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day);
				DateTime dt2 = new DateTime(iY,iM,iD);
			
				//var dDiff = dt1.getTime() - dt2.getTime();
				TimeSpan tsDiff = dt1 - dt2;
				double dDiff = tsDiff.TotalMilliseconds;
			
				double dDiffD = Math.Floor(dDiff / (1000 * 3600 * 24));
			
				double dDiffW;
				if( dDiffD < 0 )
				{
					dDiffW = Math.Floor((dDiffD * -1) / 7) * -1;
				}
				else
				{
					dDiffW = Math.Floor(dDiffD / 7);
				}
			
				if( bAnniver && (dDiffD > 7) ) continue;
			
				var sWtit = dDiffW.ToString() +
					"w " + (dDiffD - (dDiffW * 7)).ToString() + "d";
			
				addFile(sPath, sDate, sDesc, bAnniver, dDiffD, sWtit);
			
				break;
			}
		}

		private void addFile(string sPath, string sDate, string sDesc, bool bAnniver, double dD, string sW)
		{
			if( sEml != "" ) sEml = sEml + "\r\n";
			sEml = sEml + sDate + " " + sDesc;
		
			int idx = spFiles.Children.Count + 1;
		
			StackPanel sp = new System.Windows.
			Controls.StackPanel();
			sp.Name = "spF_" + idx.ToString();
			sp.Orientation = System.Windows.Controls.
			Orientation.Horizontal;
			spFiles.Children.Add(sp);
		
			StackPanel sp2 = new System.Windows.
			Controls.StackPanel();
			sp2.Name = "spExt_" + idx.ToString();
			if( bAnniver )
			{
				if( (dD * dD) < 10 )
				{
					sp2.Background = new System.Windows.Media.
						SolidColorBrush(System.Windows.Media.
						Colors.Red);
				}
				else
				{
					if( (dD * dD) < 50 )
					{
						sp2.Background = new System.Windows.Media.
							SolidColorBrush(System.Windows.Media.
							Colors.Orange);
					}
					else
					{
						if( (dD * dD) < 197 )
						{
							sp2.Background = new System.Windows.Media.
								SolidColorBrush(System.Windows.Media.
								Colors.Green);
						}
						else
						{
							sp2.Background = new System.Windows.Media.
								SolidColorBrush(System.Windows.Media.
								Colors.Blue);
						}
					}
				}
			}
			else
			{
				sp2.Background = new System.Windows.Media.
					SolidColorBrush(System.Windows.Media.
					Colors.Gray);
			}
			sp2.Margin = new System.Windows.
			Thickness(6,6,0,0);
			sp2.Width = ciDtWidth;
			sp.Children.Add(sp2);
		
			TextBlock tb = new System.Windows.
			Controls.TextBlock();
			tb.Name = "tbExt_" + idx.ToString();
			tb.Text = sDate;
			tb.Foreground = new System.Windows.Media.
			SolidColorBrush(System.Windows.Media.
				Colors.White);
			tb.TextAlignment = System.Windows.
			TextAlignment.Center;
			sp2.Children.Add(tb);
		
			tb = new System.Windows.
			Controls.TextBlock();
			tb.Name = "tbExt2_" + idx.ToString();
			tb.Text = dD.ToString() + "d";
			tb.Foreground = new System.Windows.Media.
			SolidColorBrush(System.Windows.Media.
				Colors.White);
			tb.TextAlignment = System.Windows.
			TextAlignment.Center;
			sp2.Children.Add(tb);
		
			tb = new System.Windows.
			Controls.TextBlock();
			tb.Name = "tbExt3_" + idx.ToString();
			tb.Text = sW;
			tb.Foreground = new System.Windows.Media.
			SolidColorBrush(System.Windows.Media.
				Colors.White);
			tb.TextAlignment = System.Windows.
			TextAlignment.Center;
			sp2.Children.Add(tb);
		
			Button btn = new System.Windows.
			Controls.Button();
			btn.Name = "btnTit_" + idx.ToString();
			btn.Content = sDesc;
			btn.HorizontalContentAlignment =
			System.Windows.HorizontalAlignment.Left;
			btn.BorderThickness = new System.Windows.
			Thickness(0);
			btn.Foreground = new System.Windows.Media.
			SolidColorBrush(System.Windows.Media.
				Colors.Black);
			btn.Margin = new System.Windows.
			Thickness(-12,-10,-12,-12);
			btn.Tag = sPath;
			sp.Children.Add(btn);
		
			btn.Click += new System.Windows.RoutedEventHandler(btn_Click);
		}

		private void btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			host.UI.lbTit.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblTitles.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.PageTitle.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.txtTit.Visibility = System.Windows.Visibility.Visible;
			host.UI.spDate.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblNot.Visibility = System.Windows.Visibility.Visible;
			host.UI.FileName.Visibility = System.Windows.Visibility.Visible;
		
			sTitOld = ((Button) sender).Content.ToString();
			sLstLoadTitle = sTitOld;
			host.UI.txtTit.Text = sTitOld;
			
			RscStore store = new RscStore();
		
			bool bTmp;
			string sDt = store.ReadTextFile(((Button) sender).Tag.ToString(), "", out bTmp );
			//host.UI.txtNot.Text = sDt
			try
			{
				txY.Text = "";
				txM.Text = "";
				txD.Text = "";
				
				int iOff = 0;
				
				if( sDt.Length > 6 )
				{
					txY.Text = sDt.Substring(0, 4);
					iOff += 5;
				}
				
				if( sDt.Length > 3 )
				{
					txM.Text = sDt.Substring(iOff, 2);
					iOff += 3;
				}
				
				if( sDt.Length > 0 )
				{
					txD.Text = sDt.Substring(iOff, 2);
				}
			}
			catch( Exception )
			{
			}
			
			host.UI.txtCch.Text = "";
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.btnSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.btnDel.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblDel.Visibility = System.Windows.Visibility.Visible;
		}

		private void doNew()
		{
			sLstLoadTitle = "";
		
			sTitOld = "";
			host.UI.txtTit.Text = "";
		
			host.UI.txtCch.Text = "";
			host.UI.txY.Text = "";
			host.UI.txM.Text = "";
			host.UI.txD.Text = "";
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.btnSend.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblSend.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.btnDel.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblDel.Visibility = System.Windows.Visibility.Collapsed;
		
		}

		private void listKeys()
		{
			var coll = host.createCollection();
			
			RscStore store = new RscStore();
								
			string[] fles = RscSort.OrderBy(store.GetFileNames(csDocFolder, "*.txt"));
			foreach( string sFle in fles )
			{
				NoteEditor_coll_item item= host.createBindableObject();
				
				item.ObjectA = RscStore.FileNameOfPath( sFle );
				item.ObjectB = csDocFolder + "\\" + sFle;
				
				coll.Add(item);
			}
		
			host.UI.lbTit.ItemsSource = coll;
			host.UI.lbTit.DisplayMemberPath = "ObjectA";
		}
		
    }
	
}
