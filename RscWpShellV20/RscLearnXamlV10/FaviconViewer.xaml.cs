using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Windows.Media;
using System.Windows.Shapes;

using Ressive.Utils;
using Ressive.Theme;
using Ressive.Store;

namespace RscLearnXamlV10
{
	
    public partial class FaviconViewer : PhoneApplicationPage
    {
		// //
		// Helpers {
		//ATT!!! substring parameters differ (iChrFrom, iChrTo) !!!
		void alert( string sMsg ) { MessageBox.Show( sMsg ); }
		int parseInt( string sN ) { int iRes = 0; Int32.TryParse( sN, out iRes ); return iRes; }
		// //
		FaviconViewer host { get{ return this; } }
		FaviconViewer UI { get{ return this; } }
		// } Helpers
		// //
		
		const string csDocFolder = "A:\\Documents";
		
		RscTheme m_Theme = null;
		
		System.IO.MemoryStream m_memIco = null;
		
		private string ImgFolderPath()
		{
			string sPath = "A:\\Internet\\Favicons";
			
			RscStore store = new RscStore();
			
			store.CreateFolderPath( sPath );
			
			return sPath + "\\";
		}
		
  		System.Uri uIco = null;
		int posIco = 0;
		int fldIco = 0;
		int wIco = 0;
		int hIco = 0;
		int cbDib = 0;
		int offDib = 0;
		int bytesRem = 0;
				
        public FaviconViewer()
        {
            InitializeComponent();
			
			//MemUsage Optimization...
			Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			m_Theme = (RscTheme) GlobalDILholder.Tag;
			//m_dil = new RscDefaultedImageList( "Theme", "Current", "Default" );
 			
			RscStore store = new RscStore();
			store.CreateFolderPath( csDocFolder );

			//Building Palette...
			for(int y = 0; y < 16; y++)
			{
				for(int x = 0; x < 16; x++)
				{
					System.Windows.Shapes.Rectangle rc = new System.Windows.Shapes.Rectangle();
					rc.Name = "clr" + toHexa(y,1) + toHexa(x,1);
					rc.Width = 4;
					rc.Height = 4;
					rc.Margin = new System.Windows.Thickness(4 * x, 4 * y, 0, 0);
					rc.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
					
					host.UI.imgPalette.Children.Add(rc);
				}
			}

			//Building Icon...
			for(int y = 0; y < 16; y++)
			{
				for(int x = 0; x < 16; x++)
				{
					System.Windows.Shapes.Rectangle rc = new System.Windows.Shapes.Rectangle();
					rc.Name = "pix" + toHexa(x,1) + toHexa(15 - y,1);
					rc.Width = 4;
					rc.Height = 4;
					rc.Margin = new System.Windows.Thickness(4 * x, 4 * y, 0, 0);
					rc.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
					
					host.UI.imgIcon.Children.Add(rc);
				}
			}

			//Building Icon Sm...
			host.UI.imgIconSm.Background = new SolidColorBrush( m_Theme.ThemeColors.ThemeBack );
			int iCX = 16;
			int iCY = 16;
			for(int y = 0; y < 16; y++)
			{
				for(int x = 0; x < 16; x++)
				{
					System.Windows.Shapes.Rectangle rc = new System.Windows.Shapes.Rectangle();
					rc.Name = "pixSm" + toHexa(x,1) + toHexa(15 - y,1);
					rc.Width = 2;
					rc.Height = 2;
					rc.Margin = new System.Windows.Thickness(iCX + (2 * x), iCY + (2 * y), 0, 0);
					rc.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
					
					host.UI.imgIconSm.Children.Add(rc);
				}
			}
			
			btnWebDog.Click += new System.Windows.RoutedEventHandler(btnWebDog_Click);
			btnSave.Click += new System.Windows.RoutedEventHandler(btnSave_Click);
			btnGet.Click += new System.Windows.RoutedEventHandler(btnGet_Click);
       }
		
		protected override void OnNavigatedTo(NavigationEventArgs args)
		{
			IDictionary<string, string> parameters = this.NavigationContext.QueryString;
			
			if( parameters.ContainsKey( "uri" ) )
			{
				string sUri;
				
				sUri = parameters["uri"];
				
				sUri = sUri.Replace("(HASH)", "#");
				
				if( sUri.IndexOf( "://" ) < 0 )
					sUri = "http://" + sUri;
				
				//wbc.Navigate(new Uri(sUri, UriKind.Absolute));
				host.UI.tbUri.Text = sUri;
			}
			
			base.OnNavigatedTo(args);
		}

		private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
		{
      		mySave();
		}

		private void btnGet_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Uri uri = null;
			try{ uri = new System.Uri(host.UI.tbUri.Text); }
			catch( Exception ) { MessageBox.Show( "Bad URI format!" ); return; }
			
			if( m_memIco != null )
			{
				m_memIco.Close();
				m_memIco = null;
			}
			
			host.UI.icoLst.Items.Clear();
			host.UI.imgPalette.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.imgIcon.Visibility = System.Windows.Visibility.Collapsed;
		
			uIco = uri;
			myLoad();
		
			System.Net.WebClient client = new System.Net.WebClient();
			client.OpenReadCompleted += new System.Net.OpenReadCompletedEventHandler(client_OpenReadCompleted);
		
			client.OpenReadAsync(uIco);
		
			host.UI.prsBar.Visibility = System.Windows.Visibility.Visible;
		}

		private void client_OpenReadCompleted(object sender, System.Net.OpenReadCompletedEventArgs e)
		{
			host.UI.prsBar.Visibility = System.Windows.Visibility.Collapsed;
			
			if (e.Error == null)
			{
				qryIco(e.Result);
				
				e.Result.Seek( 0, System.IO.SeekOrigin.Begin );
				byte [] ay = new byte [e.Result.Length];
				e.Result.Read( ay, 0, (int) e.Result.Length );
				
				m_memIco = new System.IO.MemoryStream((int) e.Result.Length);
				m_memIco.Write( ay, 0, (int) e.Result.Length);
				m_memIco.Seek( 0, System.IO.SeekOrigin.Begin);
			}
			else
			{
				alert("error!");
			}
		}

		private void btnWebDog_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			RscStore store = new RscStore();
			store.DeleteFile(csDocFolder + "\\" + "WebDogUri.txt"); //To reset all File Dates!!!
			store.WriteTextFile(csDocFolder + "\\" + "WebDogUri.txt", host.UI.tbUri.Text.ToString(), true );
			alert("WebDog tile on Desktop configured successfuly!\n\nTile shows last download time.\nTo disable tile delete file isostore:\\Documents\\WebDogUri.txt." );
		}
		
		private void mySave()
		{
			RscStore store = new RscStore();
			
			System.IO.Stream stream = null;
			
			string fName = host.UI.isoName.Text;
			if( fName == "" ) return;
			fName = ImgFolderPath() + fName;
			
			// //
			//
			
			fName += ".ico";
			
			if( store.FileExists(fName) ) store.DeleteFile(fName);
		
			//Save original .ICO
			if( m_memIco != null )
			{
				stream = store.CreateFile(fName);
				m_memIco.WriteTo(stream);
				stream.Close();
			}
			
			//
			// //
			//
			
			//Save ico uri as IE bookmark...
			store.WriteTextFile( fName + ".ilnk", uIco.ToString(), true );
			
			//
			// //
			//
			
			//Lg
			{
			
				string sIco = fName + ".jpg";
			
				System.Windows.Media.Imaging.WriteableBitmap wbmp = new System.Windows.Media.
				Imaging.WriteableBitmap(
				host.UI.imgIcon,
				new System.Windows.Media.MatrixTransform());
			
				if( store.FileExists(sIco) ) store.DeleteFile(sIco);
			
				stream = store.CreateFile(sIco);
			
				System.Windows.Media.Imaging.
				Extensions.SaveJpeg(wbmp, stream,
				wbmp.PixelWidth, wbmp.PixelHeight,
				0, 100);
			
				stream.Close();
		
				host.UI.isoImg.Source = wbmp;
				host.UI.isoImg.Visibility = System.Windows.Visibility.Visible;
				
			}
			
			//Lg
			{
			
				string sIco = fName + ".sm.jpg";
			
				System.Windows.Media.Imaging.WriteableBitmap wbmp = new System.Windows.Media.
				Imaging.WriteableBitmap(
				host.UI.imgIconSm,
				new System.Windows.Media.MatrixTransform());
			
				if( store.FileExists(sIco) ) store.DeleteFile(sIco);
			
				stream = store.CreateFile(sIco);
			
				System.Windows.Media.Imaging.
				Extensions.SaveJpeg(wbmp, stream,
				wbmp.PixelWidth, wbmp.PixelHeight,
				0, 100);
			
				stream.Close();
		
				host.UI.isoImgSm.Source = wbmp;
				host.UI.isoImgSm.Visibility = System.Windows.Visibility.Visible;
				
			}
			
			//
			// //
		}

		private void myLoad()
		{
			host.UI.btnSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.btnWebDog.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblWebDog.Visibility = System.Windows.Visibility.Collapsed;
			
			host.UI.isoImg.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.isoImgSm.Visibility = System.Windows.Visibility.Collapsed;
		
			host.UI.isoName.Text = "";
		
			if( uIco == null ) return;
			if( uIco.Host == "") return;
		
			string sFn = uIco.Host.ToString();
			int iPos1 = sFn.LastIndexOf(".");
			if( iPos1 >= 0 )
			{
				sFn = sFn.Substring(0, iPos1);
				int iPos2 = sFn.LastIndexOf(".");
				if( iPos2 >= 0 )
				{
					sFn = sFn.Substring(iPos2 + 1);
				}
			}
			host.UI.isoName.Text = sFn; // + ".ico.jpg";
		
			loadExisting();
		}
		
		private void loadExisting()
		{
			string fName = host.UI.isoName.Text;
			if( fName == "" ) return;
			
			//Lg
			{
				string sIco = ImgFolderPath() + fName + ".ico.jpg";
			
				RscStore store = new RscStore();
			
				if( !store.FileExists(sIco) ) return;
			
				System.IO.Stream stream = store.GetReaderStream( sIco );
				
				System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.
				Imaging.BitmapImage();
			
				bmp.SetSource(stream);
			
				stream.Close();
			
				host.UI.isoImg.Source = bmp;
				host.UI.isoImg.Visibility = System.Windows.Visibility.Visible;
			}
			
			//Sm
			{
				string sIco = ImgFolderPath() + fName + ".ico.sm.jpg";
			
				RscStore store = new RscStore();
			
				if( !store.FileExists(sIco) ) return;
			
				System.IO.Stream stream = store.GetReaderStream( sIco );
				
				System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.
				Imaging.BitmapImage();
			
				bmp.SetSource(stream);
			
				stream.Close();
			
				host.UI.isoImgSm.Source = bmp;
				host.UI.isoImgSm.Visibility = System.Windows.Visibility.Visible;
			}
		}

		private void qryIco(System.IO.Stream s)
		{
			posIco = 0;
		
			host.UI.icoLst.Items.Add(
			"\n" + "--> file");
		
			addWord(s, "reserved");
			addWord(s, "resource type");
			if( fldIco != 1 )
			{
				host.UI.icoLst.Items.Add(
					"ERROR: Not icon!");
				return;
			}
			addWord(s, "resource count");
			int iCnt = fldIco;
		
			cbDib = 0;
			offDib = 0;
			int hotYMax = 0;
			int hotY = 0;
		
			for(int i=0; i < iCnt; i++)
			{
				host.UI.icoLst.Items.Add(
					"\n" + "--> resource #" + i.ToString());
			
				addByte(s, "  width");
				wIco = fldIco;
				addByte(s, "  height");
				hIco = fldIco;
				addByte(s, "  color count");
				addByte(s, "  reserved");
				addWord(s, "  hotspot X");
				addWord(s, "  hotspot Y");
				hotY = fldIco;
				addDWord(s, "  DIB size");
				if( hotYMax <= hotY && wIco == 16 && hIco == 16 )
				{
					cbDib = fldIco;
				}
				addDWord(s, "  DIB offset");
				if( hotYMax <= hotY && wIco == 16 && hIco == 16 )
				{
					hotYMax = hotY;
					offDib = fldIco;
				}
			}
		
			if( cbDib == 0 ) return;
			
			wIco = 16;
			hIco = 16;
		
			if( offDib != posIco )
			{
				mySeek(s, offDib);
			}
		
			qryDib(s);
		}

		private void mySeek(System.IO.Stream s, int ito)
		{
			int offset = ito - posIco;
		
			//alert("...seeking " + offset.toString() + " forward...");
		
			//s.Seek(offset,
			//  System.IO.SeekOrigin.Current);
			//posIco = ito;
		
			int byt = 0;
			for(int ske = 0; ske < offset; ske++)
			{
				byt = s.ReadByte();
				posIco += 1;
			}
		}

		private void imgReady()
		{
			host.UI.imgIcon.Visibility = System.Windows.Visibility.Visible;
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Visible;
		
			host.UI.btnWebDog.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblWebDog.Visibility = System.Windows.Visibility.Visible;
			
			RscStore store = new RscStore();
			
			bool bTmp;
			string sUri = store.ReadTextFile( csDocFolder + "\\" + "WebDogUri.txt", "", out bTmp );
			if( sUri == host.UI.tbUri.Text.ToString() )
			{
				//Refresh file date...
				store.WriteTextFile( csDocFolder + "\\" + "WebDogUri.txt", sUri, true );
			}
		}
 
		private void qryPixels16(System.IO.Stream s, int cx, int cy)
		{
			host.UI.icoLst.Items.Add(
			"...filling icon pixels...\n");
		
			imgReady();
		
			for(int y = 0; y < cy; y++)
			{
				string sLn = "";
				int xr = -1;
				
				for(int x = 0; x < (cx / 2); x++)
				{
					int pix = s.ReadByte();
			
					int lo = pix & 15;
					int hi = (pix >> 4) & 15;
			
					xr += 1;
					setCol(xr, y, getCol(hi));
			
					xr += 1;
					setCol(xr, y, getCol(lo));
			
					sLn += lo.ToString() + " " + hi.ToString() + " ";
				}
				
				host.UI.icoLst.Items.Add(sLn);
			}
		
			bytesRem -= (cx * cy / 2);
			
			host.UI.icoLst.Items.Add(
			"\n... " + bytesRem.ToString() + " bytes to read...\n");
		
			dumpRem(s);
		}

		private void qryPixels256(System.IO.Stream s, int cx, int cy)
		{
			host.UI.icoLst.Items.Add(
			"...filling icon pixels...\n");
		
			imgReady();
		
			for(int y = 0; y < cy; y++)
			{
				for(int x = 0; x < cx; x++)
				{
					int pix = s.ReadByte();
			
					setCol(x, y, getCol(pix));
				}
			}
		
			bytesRem -= (cx * cy);
			
			host.UI.icoLst.Items.Add(
			"\n... " + bytesRem.ToString() + " bytes to read...\n");
		
			dumpRem(s);
		}

		private void qryPixelsXX(System.IO.Stream s, int bitCnt, int cx, int cy)
		{
			switch( bitCnt )
			{
				case 4: qryPixels16(s, cx, cy); break;
				case 8: qryPixels256(s, cx, cy); break;
				case 24: qryPixels24bit(s, cx, cy); break;
			}
		}

		private void qryPixelsTrue(System.IO.Stream s, int cx, int cy)
		{
			host.UI.icoLst.Items.Add(
			"...filling icon pixels...");
		
			imgReady();
		
			for(int y = 0; y < cy; y++)
			{
				for(int x = 0; x < cx; x++)
				{
					int yB = s.ReadByte();
					int yG = s.ReadByte();
					int yR = s.ReadByte();
					int yA = s.ReadByte();
					posIco += 4;
			
					System.Windows.Media.Color clr = System.Windows.Media.Color.
					FromArgb(
					System.Convert.ToByte(yA),
					System.Convert.ToByte(yR),
					System.Convert.ToByte(yG),
					System.Convert.ToByte(yB));
			
					setCol(x, y,
					new System.Windows.Media.SolidColorBrush(
					clr));
				}
			}
		
			bytesRem -= (cx * cy * 4);
			host.UI.icoLst.Items.Add(
			"\n... " + bytesRem.ToString() + " bytes to read...\n");
		
			dumpRem(s);
		}

		private void qryPixels24bit(System.IO.Stream s, int cx, int cy)
		{
			host.UI.icoLst.Items.Add(
			"...filling icon pixels...");
		
			imgReady();
		
			for(int y = 0; y < cy; y++)
			{
				for(int x = 0; x < cx; x++)
				{
					int yB = s.ReadByte();
					int yG = s.ReadByte();
					int yR = s.ReadByte();
					posIco += 3;
			
					int yA = 255;
			
					System.Windows.Media.Color clr = System.Windows.Media.Color.
					FromArgb(
					System.Convert.ToByte(yA),
					System.Convert.ToByte(yR),
					System.Convert.ToByte(yG),
					System.Convert.ToByte(yB));
			
					setCol(x, y,
					new System.Windows.Media.SolidColorBrush(
					clr));
				}
			}
		
			bytesRem -= (cx * cy * 3);
			host.UI.icoLst.Items.Add(
			"\n... " + bytesRem.ToString() + " bytes to read...\n");
		
			dumpRem(s);
		}

		private void dumpRem(System.IO.Stream s)
		{
			int i =  -1;
			string sLn = "";
			
			for(;;)
			{
				i += 1;
			
				if( i > 0 && i % 8 == 0 || i == bytesRem )
				{
					host.UI.icoLst.Items.Add(sLn);
					sLn = "";
				}
				if( i >= bytesRem ) break;
			
				sLn = sLn + toHexa( s.ReadByte(), 2 ) + " ";
			}
		}

		private void setCol(int x, int y, Brush br)
		{
			//Lg
			{
				string sRc = "pix" + toHexa(x,1) + toHexa(y,1);
				Rectangle pix = (Rectangle) host.UI.imgIcon.FindName(sRc);
				if( pix == null ) return;
				pix.Fill = br;
			}
			
			//Sm
			{
				string sRc = "pixSm" + toHexa(x,1) + toHexa(y,1);
				Rectangle pix = (Rectangle) host.UI.imgIconSm.FindName(sRc);
				if( pix == null ) return;
				pix.Fill = br;
			}
		}
		
		private Brush getCol(int idx)
		{
			string sRc = "clr" + toHexa(idx,2);
			Rectangle clr = (Rectangle) host.UI.imgPalette.FindName(sRc);
			if( clr == null ) return null;
			return clr.Fill;
		}

		private void qryPalette(System.IO.Stream s, int nClr)
		{
			host.UI.imgPalette.Visibility = System.Windows.Visibility.Visible;
		
			host.UI.icoLst.Items.Add("...filling palette with " +
			nClr.ToString() + " colors...");
		
			for(int i = 0; i < nClr; i++)
			{
				int yB = s.ReadByte();
				int yG = s.ReadByte();
				int yR = s.ReadByte();
				int yA = s.ReadByte();
				posIco += 4;
			
				//Assume 0!!!
				yA = 255;
				
				System.Windows.Media.Color clr = System.Windows.Media.Color.
				FromArgb(
				System.Convert.ToByte(yA),
				System.Convert.ToByte(yR),
				System.Convert.ToByte(yG),
				System.Convert.ToByte(yB));
			
				string sRc = "clr" + toHexa(i,2);
			
				System.Windows.Shapes.Rectangle clrRc = (System.Windows.Shapes.Rectangle)
					host.UI.imgPalette.FindName(sRc);
				
				if( clrRc == null ) continue;
				clrRc.Fill = new
					System.Windows.Media.SolidColorBrush(
					clr);
			}
		}

		private void qryDib(System.IO.Stream s)
		{
			bytesRem = cbDib;
		
			host.UI.icoLst.Items.Add(
			"\n... " + bytesRem.ToString() + " bytes to read...\n");
		
			host.UI.icoLst.Items.Add(
			"--> BITMAPINFOHEADER");
		
			addDWord(s, "    biSize");
			if( fldIco != 40 )
			{
				host.UI.icoLst.Items.Add(
					"ERROR: biSize must be 40!");
				return;
			}
			addDWord(s, "    biWidth");
			//Assert = wICO!!!
			addDWord(s, "    biHeight");
			//Assert = hICO!!!
			addWord(s, "    biPlanes");
			addWord(s, "    biBitCount");
			int bitCnt = fldIco;
			addDWord(s, "    biCompression");
			addDWord(s, "    biSizeImage");
			addDWord(s, "    biXPelsPerMeter");
			addDWord(s, "    biYPelsPerMeter");
			addDWord(s, "    biClrUsed");
			int clrUsed = fldIco;
			addDWord(s, "    biClrImportant");
		
			bytesRem -= 40;
			host.UI.icoLst.Items.Add(
			"\n... " + bytesRem.ToString() + " bytes to read...\n");
		
			int clrCnt = 0;
			switch( bitCnt )
			{
				case 4: clrCnt = 16; break;
				case 8: clrCnt = 256; break;
			}
		
			if( clrCnt > 0 )
			{
				qryPalette(s, clrCnt);
			
				bytesRem -= (clrCnt * 4);
				host.UI.icoLst.Items.Add(
					"\n..." + bytesRem.ToString() + " bytes to read...\n");
			}
		
			switch( bitCnt )
			{
				case 32: qryPixelsTrue(s, wIco, hIco); break;
				default: qryPixelsXX(s, bitCnt, wIco, hIco); break;
			}
		}

		private void addByte(System.IO.Stream s, string sName)
		{
			int lo = s.ReadByte();
			
			fldIco = lo;
			
			host.UI.icoLst.Items.Add(
			posIco.ToString() + ": " +
			sName + ": " + lo.ToString() +
			" ( " + fldIco.ToString() + " )");
			
			posIco += 1;
		}

		private void addWord(System.IO.Stream s, string sName)
		{
			int lo = s.ReadByte();
			int hi = s.ReadByte();
			
			fldIco = (hi * 256) + lo;
			
			host.UI.icoLst.Items.Add(
			posIco.ToString() + ": " +
			sName + ": " +
			hi.ToString() + " | " + lo.ToString() +
			" ( " + fldIco.ToString() + " )");
			
			posIco += 2;
		}

		private void addDWord(System.IO.Stream s, string sName)
		{
			int lo = s.ReadByte();
			int hi = s.ReadByte();
			int lo2 = s.ReadByte();
			int hi2 = s.ReadByte();
			
			fldIco = hi2 * 256 * 256 * 256;
			fldIco += lo2 * 256 * 256;
			fldIco += hi * 256;
			fldIco += lo;
			
			host.UI.icoLst.Items.Add(
			posIco.ToString() + ": " +
			sName + ": " +
			hi2.ToString() + " | " + lo2.ToString() + " | " +
			hi.ToString() + " | " + lo.ToString() +
			" ( " + fldIco.ToString() + " )");
			
			posIco += 4;
		}

		private string toHexa(int iVal, int iDigCnt)
		{
			int iRem = iVal;
			string sHDig = "";
			string sRes = "";
		
			for(;;)
			{
				int iDig = iRem % 16;
				
				if( iDig < 10 )
				{
					sHDig = iDig.ToString();
				}
				else
				{
					switch( iDig )
					{
						case 10: sHDig = "A"; break;
						case 11: sHDig = "B"; break;
						case 12: sHDig = "C"; break;
						case 13: sHDig = "D"; break;
						case 14: sHDig = "E"; break;
						case 15: sHDig = "F"; break;
					}
				}
			
				sRes = sHDig + sRes;
			
				iRem = (iRem - iDig) / 16; //Math.floor((iRem - iDig) / 16);
				
				if( iRem == 0 ) break;
			}
		
			while( sRes.Length < iDigCnt )
			{
				sRes = "0" + sRes;
			}
		
			return sRes;
		}
		
    }
	
}
