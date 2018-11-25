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
	
    public partial class FaviconFinder : PhoneApplicationPage
    {
		// //
		// Helpers {
		//ATT!!! substring parameters differ (iChrFrom, iChrTo) !!!
		void openBrowser(string sUri) { MessageBox.Show("TODO: Open IE task for: " + sUri); }
		void alert( string sMsg ) { MessageBox.Show( sMsg ); }
		int parseInt( string sN ) { int iRes = 0; Int32.TryParse( sN, out iRes ); return iRes; }
		// //
		FaviconFinder host { get{ return this; } }
		FaviconFinder UI { get{ return this; } }
		// } Helpers
		// //
		
		System.Uri uSite = null;
				
        public FaviconFinder()
        {
            InitializeComponent();
			
			btnGo.Click += new System.Windows.RoutedEventHandler(btnGo_Click);
			btnGet.Click += new System.Windows.RoutedEventHandler(btnGet_Click);
        }
		
		protected override void OnNavigatedTo(NavigationEventArgs args)
		{
			IDictionary<string, string> parameters = this.NavigationContext.QueryString;
			
			if( parameters.ContainsKey( "dns" ) )
			{
				string sDns;
				
				sDns = parameters["dns"];
				
				if( sDns.IndexOf( "://" ) < 0 )
					sDns = "http://" + sDns;
				
				//wbc.Navigate(new Uri(sUri, UriKind.Absolute));
				host.UI.tbUri.Text = sDns;
			}
			
			base.OnNavigatedTo(args);
		}

        private void btnGo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			string sUri = host.UI.favUri.Text;
		
			if( sUri=="") return;
		
			//host.openBrowser(sUri);
			NavToPage( "/RscLearnXamlV10;component/FaviconViewer", "?uri=" + sUri );
        }
		
		private void NavToPage( string sPageName, string sPageArgs )
		{
			if( sPageName.Length == 0 ) return;
			
			string sUri = "";
			
			if( sPageName.IndexOf( ";component/" ) >= 0 )
			{
				sUri = sPageName + ".xaml" + sPageArgs;
			}
			else
			{
				sUri = "/" + sPageName + ";component/"
					+ "MainPage" + ".xaml" + sPageArgs;
			}
			
			this.NavigationService.Navigate(new Uri(sUri, UriKind.Relative));
		}

        private void btnGet_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			Uri uri = null;
			try{ uri = new System.Uri(host.UI.tbUri.Text); }
			catch( Exception ) { MessageBox.Show( "Bad URI format!" ); return; }
				
			host.UI.tagLst.Items.Clear();
			host.UI.favUri.Text = "";
		
			uSite = uri;
			host.UI.tbUri.Text = uSite.ToString();
		
			host.UI.tip3.Visibility = System.Windows.Visibility.Collapsed;
		
			System.Net.WebClient client = new System.Net.WebClient();
			client.DownloadStringCompleted += new System.Net.DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
		
			client.DownloadStringAsync(uSite);
		
			host.UI.prsBar.Visibility = System.Windows.Visibility.Visible;
        }

        private void client_DownloadStringCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
			host.UI.prsBar.Visibility = System.Windows.Visibility.Collapsed;
	
			if (e.Error == null)
			{
				listTags(e.Result);
			}
			else
			{
				alert("error!");
			}
        }
		
		private string absUri(System.Uri uBase, string sRef)
		{
			if( sRef.IndexOf("://") >= 0 ) return sRef;
			if( sRef == "" ) return uBase.ToString();
			
			string sBase = uBase.Scheme + "://"
				+ uBase.Host + uBase.LocalPath;
			
			int iPos = sBase.LastIndexOf("/");
			sBase = sBase.Substring(0, iPos);
			
			if( sRef.Substring(0,1) != "/" ) sBase = sBase + "/";
			
			return sBase + sRef;
		}

		string spn = "";
		string spv = "";
		private int exProp( string sTag, int iB, int iE)
		{
			spn = "";
			spv = "";
			
			if( iB > iE ) return -1;
			int iPos = -1;
			string sTmp = "";
			bool bInStr = false;
			string sC = "";
			bool bWs = false;
			
			for( iPos = iB; iPos <= iE; iPos++ )
			{
				sC = sTag.Substring(iPos, 1); //iPos+1);
				bWs = false;
				if( bInStr )
				{
					if( sC == "\"" ) break;
					sTmp = sTmp + sC;
				}
				else
				{
					switch( sC )
					{
						
						case "\"":
							bInStr = true;
							break;
							case "=":
							if( spn != "" ) return -1;
							spn = sTmp;
							sTmp = "";
							break;
							
						case " ":
						case "\t":
						case "\r":
						case "\n":
							bWs = true;
							break;
							default :
							sTmp = sTmp + sC;
							break;
					}
				}
				
				if( bWs && spn != "" && sTmp != "" ) break;
			}
			
			//alert(spn);
			//alert(sTmp);
			if( spn == "" ) return -1;
			if( sTmp == "" ) return -1;
			spv = sTmp;
			
			return (iPos + 1);
		}

		private void listTags(string sHtm)
		{
			host.UI.tagLst.Items.Clear();
			host.UI.favUri.Text = "";
			
			string sHtmUp = sHtm.ToUpper();
			
			int iPos = -1;
			int iPos2 = -1;
			int iPos3 = -1;
			int iStart = 0;
			string sTag = "";
			string sItem = "";
			string sHref = "";
			bool bFavIcon = false;
			
			const string csWhat = "<LINK";
			
			for(;;)
			{
				iPos = sHtmUp.IndexOf(csWhat,iStart);
				if( iPos < 0 ) break;
				iPos2 = sHtmUp.IndexOf(">",iPos);
				if( iPos2 < 0 ) break;
			
				sTag = sHtm.Substring(iPos, (iPos2 - iPos)); // iPos2);
				sItem = "";
				sHref = "";
				bFavIcon = false;
				iPos3 = csWhat.Length;
				for(;;)
				{
					//alert(iPos3.toString() + " " + ((iPos2-iPos)-1).toString());
					iPos3 = exProp(sTag,iPos3,(iPos2-iPos)-1);
					//alert(iPos3.toString());
					if( iPos3 < 0 ) break;
				
					sItem = sItem + "\n" + spn + " = \"" + spv +"\"";
				
					if( spn.ToUpper() == "HREF" )
					{
						sHref =  absUri(uSite, spv);
					}
					if( spn.ToUpper() == "REL" )
					{
						if( spv.ToLower() == "shortcut icon" )
						{
							bFavIcon = true;
						}
					}
				}
			
				if( sItem != "" ) host.UI.tagLst.Items.Add(sItem);
			
				if( bFavIcon )
				{
					host.UI.favUri.Text = sHref;
				}
			
				iStart = iPos + 1;
			}
			
			if( host.UI.favUri.Text == "" )
			{
				host.UI.favUri.Text = absUri(uSite,"favicon.ico");
				host.UI.tip3.Visibility = System.Windows.Visibility.Visible;
			}
		}
		
    }
	
}
