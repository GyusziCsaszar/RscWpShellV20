using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Windows.Shapes;
using System.Windows.Media;

using Ressive.Theme;

namespace RscLearnXamlV10
{
	
    public partial class ColorMatchGame : PhoneApplicationPage
    {
		// //
		// Helpers {
		//ATT!!! substring parameters differ (iChrFrom, iChrTo) !!!
		void alert( string sMsg ) { MessageBox.Show( sMsg ); }
		int parseInt( string sN ) { int iRes = 0; Int32.TryParse( sN, out iRes ); return iRes; }
		// //
		ColorMatchGame host { get{ return this; } }
		ColorMatchGame UI { get{ return this; } }
		// } Helpers
		// //
		
		RscTheme m_Theme = null;
		Size m_sContentPanel = new Size(100, 100);

		int cx = 6; //8;
		int cy = 10; //7; //9;
		int clrCnt = 6; //6;
		int rcW = 58;
		int rcH = 58;
		int rcG = 6;
		bool b3D = true;
		bool bHis = true;
		string sTodo = "";
		string sHis = "";
		int iVal = 0;
		
		int xT = 0;
		int yT = 0;
		int xB = 0;
		int yB = 0;
				
        public ColorMatchGame()
        {
            InitializeComponent();
 			
			//MemUsage Optimization...
			Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			m_Theme = (RscTheme) GlobalDILholder.Tag;
			//m_dil = new RscDefaultedImageList( "Theme", "Current", "Default" );
			
			host.UI.btnStart.Click += new System.Windows.RoutedEventHandler(btnStart_Click);
			host.UI.his.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(his_SelectionChanged);
			
			ContentPanel.SizeChanged += new System.Windows.SizeChangedEventHandler(ContentPanel_SizeChanged);
       }
		
		private void ContentPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			bool bNoChng = (m_sContentPanel.Width == e.NewSize.Width && m_sContentPanel.Height == e.NewSize.Height);
			m_sContentPanel = e.NewSize;
			
			if( !bNoChng )
			{
				if( e.NewSize.Width < e.NewSize.Height )
					imgBk.Source = m_Theme.GetImage("Images/Bk001_portrait.jpg");
				else
					imgBk.Source = m_Theme.GetImage("Images/Bk001_landscape.jpg");
			}
		}

        private void btnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
      		doStart();
        }

        private void his_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
			object item = host.UI.his.SelectedItem;
			if( item == null ) return;
			string sh=item.ToString();
			if( sh == "" ) return;
			hisRestore(sh);
        }
		
		void preStart()
		{
			host.UI.doc.Children.Clear();
			host.UI.view.Children.Clear();
			host.UI.his.Items.Clear();
		
			sTodo = "";
			sHis = "";
		
			b3D = host.UI.chb3D.IsChecked.Value;
		
			bHis = host.UI.chbShowHis.IsChecked.Value;
			if( bHis )
			{
				host.UI.lblHis.Visibility =
					System.Windows.
						Visibility.Visible;
				host.UI.his.Visibility =
					System.Windows.
						Visibility.Visible;
			}
			else
			{
				host.UI.lblHis.Visibility =
					System.Windows.
						Visibility.Collapsed;
				host.UI.his.Visibility =
					System.Windows.
						Visibility.Collapsed;
			}
		
			if( host.UI.chbDbg.IsChecked.Value )
			{
				host.UI.lblDoc.Visibility =
					System.Windows.
						Visibility.Visible;
				host.UI.doc.Visibility =
					System.Windows.
						Visibility.Visible;
			}
			else
			{
				host.UI.lblDoc.Visibility =
					System.Windows.
						Visibility.Collapsed;
				host.UI.doc.Visibility =
					System.Windows.
						Visibility.Collapsed;
			}
		
			if( !getNum(host.UI.tbCx, "width", 1, 9) ) return;
			cx = iVal;
		
			if( !getNum(host.UI.tbCy, "height", 1, 10) ) return;
			cy = iVal;
		
			if( !getNum(host.UI.tbCc, "colors", 1, 6) ) return;
			clrCnt = iVal;
		}

		bool getNum(TextBox tb, string sTit, int iMin, int iMax)
		{
			iVal = 0;
			if( tb.Text == "" )
			{
			alert("Enter " + sTit + "!");
			return false;
			}
			iVal = parseInt(tb.Text);
			if( tb.Text != iVal.ToString() )
			{
			alert("Not a number:  " + sTit + "!");
			return false;
			}
			if( (iVal < iMin) || (iVal > iMax) )
			{
			alert("Not in range (" +
				iMin.ToString() + " - " +
				iMax.ToString() + "):  " + sTit + "!");
			return false;
			}
			return true;
		}

		void fBack(int iReason, int x, int y)
		{
			switch( iReason )
			{
		
			case 0 : //Cell mouse down
				host.UI.prsBar.Visibility =
				System.Windows.
					Visibility.Visible;
				break;
		
			case 1 : //Cell mouse up - started
				break;
		
			case 2 : //Cell mouse up - ended
				host.UI.prsBar.Visibility =
				System.Windows.
					Visibility.Collapsed;
				break;
		
			}
		}

		void hisRestore(string sh)
		{
			if( !bHis ) return;
		
			sHis = sh.Substring(6);
			for( int xh = 0; xh < cx; xh++ )
			{
				for( int yh = 0; yh < cy; yh++ )
				{
					int iCnum = parseInt(
					sHis.Substring( (xh * cy) + yh,
						/*((xh * cy) + yh) + 1 */ 1 ) );
			
					setCnum(xh, yh, iCnum);
				}
			}
		
			sTodo = "";
		
			genView(host.UI.view);
		}

		void hisSave()
		{
			if( !bHis ) return;
		
			sHis = (host.UI.his.Items.Count + 1).ToString();
			if( sHis.Length == 1 ) sHis = " " + sHis;
			if( sHis.Length == 2 ) sHis = " " + sHis;
			while( sHis.Length < 6 ) sHis += " ";
		
			for( int xh = 0; xh < cx; xh++ )
			{
				for( int yh = 0; yh < cy; yh++ )
				{
					sHis += getCnum(xh, yh);
				}
			}
		
			host.UI.his.Items.Insert(0, sHis);
		}
 
		System.Windows.Media.Color getCval(int x, int y)
		{
			int iCnum = getCnum(x, y);
			return getCvalDir(iCnum);
		}

		System.Windows.Media.Color getCvalDir(int iCnum)
		{
			switch( iCnum )
			{
			case 1 :
				return System.Windows.Media.
				Colors.Red;
			case 2 :
				return System.Windows.Media.
				Colors.Green;
			case 3 :
				return System.Windows.Media.
				Colors.Blue;
			case 4 :
				return System.Windows.Media.
				Colors.Orange;
			case 5 :
				return System.Windows.Media.
				Colors.Purple;
			case 6 :
				return System.Windows.Media.
				Colors.DarkGray;
			default :
				return System.Windows.Media.
				Colors.White;
			}
		}

		void genView(Canvas can)
		{
			can.Children.Clear();
		
			for( int x = 0; x < cx; x++)
			{
				for( int y = 0; y < cy; y++ )
				{
					int iCnum = getCnum(x, y);
			
					Ellipse ell = new System.Windows.Shapes.
					Ellipse();
					ell.Name = "v_" + sIdx(x) + "_" + sIdx(y);
					ell.Width = rcW;
					ell.Height = rcH;
					ell.Margin = new System.Windows.
					Thickness( rcG + (x * (rcG + rcW)),
						rcG + (y * (rcG + rcH)), 0, 0 );
			
					if( iCnum > 0 )
					{
						if( b3D )
						{
				
							RadialGradientBrush br = new System.Windows.Media.
							RadialGradientBrush();
							br.Center = new System.Windows.
							Point( 0.4, 0.4 );
							br.GradientOrigin = new System.Windows.
							Point( 0.4, 0.4 );
				
							GradientStop gs1 = new System.Windows.Media.
							GradientStop();
							gs1.Offset = 0;
							gs1.Color = System.Windows.Media.
							Colors.White;
							br.GradientStops.Add(gs1);
				
							GradientStop gs2 = new System.Windows.Media.
							GradientStop();
							gs2.Offset = 1;
							gs2.Color = getCvalDir(iCnum);
							br.GradientStops.Add(gs2);
				
							ell.Fill = br;
						}
						else
						{
							ell.Fill = new System.Windows.Media.
							SolidColorBrush( getCvalDir(iCnum) );
						}
					}
					else
					{
						ell.Fill = host.UI.view.Background;
					}
			
					addClk(ell);
			
					can.Children.Add(ell);
				}
			}
		}

		void doPlay(int x, int y)
		{
			fBack(1, x, y);
		
			sTodo = "";
		
			if( !doCell( x, y, 0 ) )
			{
				fBack(2, x, y);
				return;
			}
		
			xT = x;
			yT = y;
			xB = x;
			yB = y;
		
			while( sTodo.Length > 0 )
			{
				string sItm = sTodo.Substring(0, 6);
				//alert(sItm);
				int tX = parseInt(sItm.Substring(0, 2));
				//alert(tX.toString());
				int tY = parseInt(sItm.Substring(3, 2)); //5));
				//alert(tY.toString());
			
				doCell( tX, tY, 1 );
			
				sTodo = sTodo.Substring(6);
				if( sTodo == ";" ) sTodo = "";
				//alert(sTodo);
			}
		
			doClear();
		
			genView(host.UI.view);
		
			if( bHis ) hisSave();
		
			fBack(2, x, y);
		}

		void doClear()
		{
			for( int x = xT; x <= xB; x++ )
			{
				for(;;)
				{
					bool bChng = false;
					int iCnum = 0;
					for( int y = 0; y < (cy - 1); y++ )
					{
						iCnum = getCnum(x, y);
						if( iCnum > 0 )
						{
							if( getCnum(x, y + 1) == 0 )
							{
							setCnum( x, y + 1, iCnum );
							setCnum( x, y, 0 );
							bChng = true;
							}
						}
					}
					if( !bChng ) break;
				}
			}
		
			for( int y = 0; y <= yB; y++ )
			{
				for(;;)
				{
					bool bChng = false;
					int iCnum = 0;
					for( int x = 0; x < (cx - 1); x++ )
					{
						iCnum = getCnum(x, y);
						if( iCnum > 0 )
						{
							if( getCnum(x + 1, y) == 0 )
							{
							setCnum( x + 1, y, iCnum );
							setCnum( x, y, 0 );
							bChng = true;
							}
						}
					}
					if( !bChng ) break;
				}
			}
		}

		bool doCell(int x, int y, int iMatchDef)
		{
		
			int iNumCurr = getCnum(x, y);
			int tX = 0;
			int tY = 0;
			int iMatch = iMatchDef;
		
			if( iNumCurr == 0 ) return false;
		
			if( x > 0 )
			{
				tX = x - 1;
				tY = y;
				if( iNumCurr == getCnum(tX, tY) )
				{
					sTodo = sTodo + sIdx(tX) + "-" + sIdx(tY) + ";";
					iMatch++;
				}
			}
		
			if( x < (cx - 1) )
			{
				tX = x + 1;
				tY = y;
				if( iNumCurr == getCnum(tX, tY) )
				{
					sTodo = sTodo + sIdx(tX) + "-" + sIdx(tY) + ";";
					iMatch++;
				}
			}
		
			if( y > 0 )
			{
				tX = x;
				tY = y - 1;
				if( iNumCurr == getCnum(tX, tY) )
				{
					sTodo = sTodo + sIdx(tX) + "-" + sIdx(tY) + ";";
					iMatch++;
				}
			}
		
			if( y < (cy - 1) )
			{
				tX = x;
				tY = y + 1;
				if( iNumCurr == getCnum(tX, tY) )
				{
					sTodo = sTodo + sIdx(tX) + "-" + sIdx(tY) + ";";
					iMatch++;
				}
			}
		
			if( iMatch != 0 )
			{
				xT = Math.Min(xT, x);
				yT = Math.Min(yT, y);
				xB = Math.Max(xB, x);
				yB = Math.Max(yB, y);
			
				setCnum( x, y, 0 );
				return true;
			}
		
			return false;
		}

		int getX(string sNm)
		{
			return parseInt(sNm.Substring(2, 2)); //4));
		}

		int getY(string sNm)
		{
			return parseInt(sNm.Substring(5));
		}

		void doStart()
		{
			preStart();
		
			sTodo = "";
		
			if( bHis ) host.UI.his.Items.Clear();
		
			genDoc(host.UI.doc);
			genView(host.UI.view);
		
			if( bHis ) hisSave();
		}

		int getCnum(int x, int y)
		{
			TextBlock tb = (TextBlock) host.UI.doc.FindName(
			"d_" + sIdx(x) + "_" + sIdx(y));
			return parseInt(tb.Text);
		}

		void setCnum(int x, int y, int iN)
		{
			TextBlock tb = (TextBlock) host.UI.doc.FindName(
			"d_" + sIdx(x) + "_" + sIdx(y));
			tb.Text = iN.ToString();
		}
 
		void genDoc(StackPanel sp)
		{
			sp.Children.Clear();
			
			Random rnd = new Random();
		
			for( int x = 0; x < cx; x++)
			{
				for( int y = 0; y < cy; y++ )
				{
					TextBlock tb = new System.Windows.Controls.
					TextBlock();
					tb.Name = "d_" + sIdx(x) + "_" + sIdx(y);
					tb.Text = Math.Round(1 + ((clrCnt - 1) *
						rnd.NextDouble()), 0).ToString();
					sp.Children.Add(tb);
				}
			}
		}

		string sIdx(int i)
		{
			string res = "";
			if( i < 10 ) res = "0";
			res += i.ToString();
			return res;
		}
 
		void addClk(System.Windows.Shapes.Ellipse ctl)
		{
			ctl.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(ctl_MouseLeftButtonDown);
			ctl.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(ctl_MouseLeftButtonUp);
		}

		private void ctl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			((Ellipse) sender).CaptureMouse();
		
			fBack(0, getX(((Ellipse) sender).Name),
				getY(((Ellipse) sender).Name));
		}

		private void ctl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			doPlay(getX(((Ellipse) sender).Name),
				getY(((Ellipse) sender).Name));
		}
		
    }
	
}
