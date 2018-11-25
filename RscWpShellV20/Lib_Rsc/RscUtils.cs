using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Ressive.Store;

namespace Ressive.Utils
{
	
	public static class RscUtils
	{

        public static int WeekOfYearHU(DateTime dt)
        {

            //Excel: =1+INT((B3-DÁTUM(ÉV(B3+4-HÉT.NAPJA(B3+6));1;5)+HÉT.NAPJA(DÁTUM(ÉV(B3+4-HÉT.NAPJA(B3+6));1;3)))/7)

            DateTime dtA = new DateTime(dt.Year, dt.Month, dt.Day);
            DateTime dtB = dtA.AddDays(6);
            TimeSpan ts2 = new TimeSpan((int)(4 - DayOfWeekExcel(dtB)), 0, 0, 0);
            DateTime dtC = dtA + ts2;
            DateTime dtD = new DateTime(dtC.Year, 1, 3);
            DateTime dtE = new DateTime(dtC.Year, 1, 5);
            double dF = dtA.ToOADate() - dtE.ToOADate() + ((double)DayOfWeekExcel(dtD));
            dF = dF / 7;
            return (1 + ((int)dF));
        }

        public static int DayOfWeekExcel(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday: return 2;
                case DayOfWeek.Tuesday: return 3;
                case DayOfWeek.Wednesday: return 4;
                case DayOfWeek.Thursday: return 5;
                case DayOfWeek.Friday: return 6;
                case DayOfWeek.Saturday: return 7;
                case DayOfWeek.Sunday: return 1;
            }

            return 0;
        }
		
		public static string GetDeviceName()
		{
			string sName = "";
			
			try
			{
				sName = Windows.Networking.Proximity.PeerFinder.DisplayName;
				
				//Misc chars like space, paranthesis, colon will be '-'...
				sName = sName.Replace( "-", " " );
				
				//Removing duplicated spaces...
				sName = sName.Replace( "  ", " " );
				sName = sName.Replace( "  ", " " );
				sName = sName.Replace( "  ", " " );
				
				sName = sName.Trim();
			}
			catch( Exception )
			{
			}
			
			if( sName.Length == 0 )
				sName = Microsoft.Phone.Info.DeviceStatus.DeviceName;
			
			return sName;
		}
		
		public static void OnNavigatedTo_ExitOnBack(IDictionary<string, string> QueryString)
		{
			IDictionary<string, string> pars = QueryString;
			
			if( !pars.ContainsKey("ExitOnBack") ) return;
			
			if( pars[ "ExitOnBack" ] != "True" ) return;
			
			string sPathNotExitOnBack = RscKnownFolders.GetTempPath( "Launcher" ) + "\\" + "NotExitOnBack.txt";
			
			RscStore store = new RscStore();
			store.DeleteFile( sPathNotExitOnBack );
		}
		
		public static bool FindStart( string str, string strStart )
		{
			if( strStart.Length == 0 ) return false;
			if( str.Length < strStart.Length ) return false;
			string strTmp = str.Substring(0, strStart.Length);
			if( strTmp == strStart ) return true;
			return false;
		}
		
		public static string RemoveStarting( string str, string strStarting )
		{
			if( strStarting.Length == 0 ) return str;
			if( str.Length < strStarting.Length ) return str;
			string strTmp = str.Substring( 0, strStarting.Length );
			if( strTmp == strStarting ) return str.Substring(strStarting.Length );
			return str;
		}
		
		public static string RemoveEnding( string str, string strEnding )
		{
			if( strEnding.Length == 0 ) return str;
			if( str.Length < strEnding.Length ) return str;
			string strTmp = str.Substring( str.Length - strEnding.Length );
			if( strTmp == strEnding ) return str.Substring( 0, str.Length - strEnding.Length );
			return str;
		}
		
		public static string pad60(int i)
		{
			var res = "";
			if( i>0 && i<10 )
			{
				res = "0";
			}
			if( i==10 || i==20 || i==30 || i==40 || i==50 )
			{
				double d = i / 10;
				res = Math.Floor(d).ToString() + "0";
			}
			else
			{
				if( i==0 )
				{
					res = "00";
				}
				else
				{
					res = res + i.ToString();
				}
			}
			return res;
		}

		public static string toMBstr(long lBytes, bool bNewLine = false, bool bAddUnit = true, int iDigits = 2)
		{
			
			double dKB = ((double) lBytes) / 1024;
			double dMB = ((double) lBytes) / 1024 / 1024;
			string sRes = "";
			
			if( dKB > 1024 )
			{
				if( dMB > 1024 )
				{
					double dGB = dMB / 1024;
					sRes = Math.Round(dGB, iDigits).ToString();
					
					if( bAddUnit )
					{
						if( bNewLine )
							sRes += "\n";
						else
							sRes += " ";
						sRes += "GB";
					}
				}
				else
				{
					sRes = Math.Round(dMB, iDigits).ToString();
					
					if( bAddUnit )
					{
						if( bNewLine )
							sRes += "\n";
						else
							sRes += " ";
						sRes += "MB";
					}
				}
			}
			else
			{
				if( lBytes > 1024 )
				{
					sRes = Math.Round(dKB, iDigits).ToString();
					
					if( bAddUnit )
					{
						if( bNewLine )
							sRes += "\n";
						else
							sRes += " ";
						sRes += "KB";
					}
				}
				else
				{
					sRes = lBytes.ToString();
					
					if( bAddUnit )
					{
						if( bNewLine )
							sRes += "\n";
						else
							sRes += " ";
						sRes += "B";
					}
				}
			}
			
			return sRes;
		}
		
		public static string toDurationStr( Duration dur )
		{
			return toDurationStr( dur.ToString() );
		}
		public static string toDurationStr( string sDur )
		{
			
			int iPos = sDur.LastIndexOf(':');
			if( iPos < 0 ) return sDur;
			
			string sd = toRoundedDoubleStr( sDur.Substring( iPos + 1 ), 0);
			while( sd.Length < 2 )
				sd = "0" + sd;
			
			return sDur.Substring(0, iPos + 1) + sd;
		}
		
		public static string toRoundedDoubleStr( string s, int iDecimals )
		{
			double d = 0;
			if( !double.TryParse( s, out d ) )
			{
				string s2 = s.Replace('.', ',');
				if( !double.TryParse( s2, out d ) )
					return s;
			}
			if( iDecimals == 0 )
			{
				return ((int) (Math.Round( d, iDecimals))).ToString();
			}
			return Math.Round( d, iDecimals).ToString();
		}
		
		public static string toDayOfWeekShort( DateTime dt )
		{
			switch( dt.DayOfWeek )
			{
				case System.DayOfWeek.Sunday : return "V";
				case System.DayOfWeek.Monday: return "H";
				case System.DayOfWeek.Tuesday: return "K";
				case System.DayOfWeek.Wednesday: return "Sze";
				case System.DayOfWeek.Thursday: return "Cs";
				case System.DayOfWeek.Friday: return "P";
				case System.DayOfWeek.Saturday: return "Szo";
			}
			return "?";
		}
		
		public static string toDateDiff( DateTime dt )
		{
			DateTime dtNow = DateTime.Now;
			
			if( dt.Year != dtNow.Year ) return dt.ToString();

			string sDiff = "";
			
			if( dt.Month != dtNow.Month )
			{
				sDiff += pad60(dt.Month) + "." + pad60(dt.Day) + ". "
					+ toDayOfWeekShort( dt ) + " "
					+ pad60(dt.Hour) + ":" + pad60(dt.Minute) + " ";
				
				return sDiff;
			}
			
			if( dt.Day != dtNow.Day )
			{
				int iD = dtNow.Day - dt.Day;
				
				if( iD / 7 > 0 )
				{
					sDiff += (iD / 7).ToString() + "h ";
					iD = iD % 7;
				}
				
				if( iD > 0 )
					sDiff += iD.ToString() + "n ";
				
				sDiff += "- ";
				sDiff += pad60(dt.Month) + "." + pad60(dt.Day) + ". "
					+ toDayOfWeekShort( dt ) + " "
					+ pad60(dt.Hour) + ":" + pad60(dt.Minute) + " ";
				
				return sDiff;
			}
			
			TimeSpan ts = dtNow - dt;
			
			if( ts.Hours > 0 ) sDiff += ts.Hours.ToString() + "ó ";
			if( ts.Minutes > 0 ) sDiff += ts.Minutes.ToString() + "p ";
			
			if( sDiff.Length == 0 )
				sDiff = "0p ";
			
			return sDiff;
		}
		
		public static bool IsIpAddress( string sHost )
		{
			if( sHost.Length == 0 ) return false;
			
			//
			//
			foreach( char c in sHost )
			{
				if( c >= '0' && c <= '9' )
				{
					//NOP...
				}
				else if( c == '.' )
				{
					//NOP...
				}
				else
				{
					return false;
				}
			}
			
			return true;
		}
		
	}
	
	public static class RscDecode
	{
		
		public static string QuotedPrintable( string s )
		{
			string sRet = "";
			
			string [] asChrs = s.Split('=');
			foreach( string sHexaChr in asChrs )
			{
				if( sHexaChr.Length == 0 ) continue;
				sRet += ((char) (HexaStringToInt( sHexaChr )));
			}
			
			return sRet;
		}
		
		public static int HexaStringToInt( string sHexa )
		{
			int iRet = 0;
			
			sHexa = sHexa.ToUpper();
			
			int iMulti = 1;
			int iLen = sHexa.Length;
			for( int iChr = (iLen - 1); iChr >= 0; iChr-- )
			{
				switch( sHexa[ iChr ] )
				{
					case '0' : iRet += (iMulti * 0); break;
					case '1' : iRet += (iMulti * 1); break;
					case '2' : iRet += (iMulti * 2); break;
					case '3' : iRet += (iMulti * 3); break;
					case '4' : iRet += (iMulti * 4); break;
					case '5' : iRet += (iMulti * 5); break;
					case '6' : iRet += (iMulti * 6); break;
					case '7' : iRet += (iMulti * 7); break;
					case '8' : iRet += (iMulti * 8); break;
					case '9' : iRet += (iMulti * 9); break;
					case 'A' : iRet += (iMulti * 10); break;
					case 'B' : iRet += (iMulti * 11); break;
					case 'C' : iRet += (iMulti * 12); break;
					case 'D' : iRet += (iMulti * 13); break;
					case 'E' : iRet += (iMulti * 14); break;
					case 'F' : iRet += (iMulti * 15); break;
					default : return 0;
				}
				
				iMulti = iMulti * 16;
			}
			
			return iRet;
		}
		
		public static Color HexaStringToColor(String hex)
		{
			//src: http://stackoverflow.com/questions/11737583/converting-colour-in-windows-phone
			
			//remove the # at the front
			hex = hex.Replace("#", "");
		
			byte a = 255;
			byte r = 255;
			byte g = 255;
			byte b = 255;
		
			int start = 0;
		
			//handle ARGB strings (8 characters long)
			if (hex.Length == 8)
			{
				a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				start = 2;
			}
		
			//convert RGB characters to bytes
			r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
			g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
			b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);
		
			return Color.FromArgb(a, r, g, b);
		}
		
	}
	
	public static class RscEncode
	{
		
		public static string UrlEncodingToString( string s )
		{
			s = s.Replace( "%5b", "[" );
			s = s.Replace( "%5d", "]" );
			return s;
		}
		
		public static string IntToHexaString( int iByte, int iDesiredLen = 0 )
		{
			string sRet = "";
			
			for(;;)
			{
				string sDig = "";
				switch( iByte % 16 )
				{
					case 0 : sDig = "0"; break;
					case 1 : sDig = "1"; break;
					case 2 : sDig = "2"; break;
					case 3 : sDig = "3"; break;
					case 4 : sDig = "4"; break;
					case 5 : sDig = "5"; break;
					case 6 : sDig = "6"; break;
					case 7 : sDig = "7"; break;
					case 8 : sDig = "8"; break;
					case 9 : sDig = "9"; break;
					case 10 : sDig = "A"; break;
					case 11 : sDig = "B"; break;
					case 12 : sDig = "C"; break;
					case 13 : sDig = "D"; break;
					case 14 : sDig = "E"; break;
					case 15 : sDig = "F"; break;
				}
				sRet = sRet + sDig;
				
				if( iByte / 16 == 0 ) break;
				
				iByte = iByte / 16;
			}
			
			while( sRet.Length < iDesiredLen )
				sRet = "0" + sRet;
			
			return sRet;
		}
		
		public static byte[] StringToAscii( string s )
		{
			
			int iSize;
			byte[] ayRes;
			int i;
			char c;
			
			iSize = s.Length;
			ayRes = new byte[ iSize ];
			
			for( i = 0; i < iSize; i++ )
			{
				c = s[i];
				
				switch( c )
				{
					
					// //
					//
					
					case 'á' :
						ayRes[ i ] = 0xE1;
						break;
					
					case 'é' :
						ayRes[ i ] = 0xE9;
						break;
					
					case 'í' :
						ayRes[ i ] = 0xED;
						break;
					
					case 'ó' :
						ayRes[ i ] = 0xF3;
						break;
						
					case 'ö' :
						ayRes[ i ] = 0xF6;
						break;
					
					case 'ő' :
						ayRes[ i ] = 0xF5;
						break;
					
					case 'ú' :
						ayRes[ i ] = 0xFA;
						break;
					
					case 'ü' :
						ayRes[ i ] = 0xFC;
						break;
					
					case 'ű' :
						ayRes[ i ] = 0xFB;
						break;
					
					//
					// //
					//
					
					case 'Á' :
						ayRes[ i ] = 0xC1;
						break;
					
					case 'É' :
						ayRes[ i ] = 0xC9;
						break;
					
					case 'Í' :
						ayRes[ i ] = 0xCD;
						break;
					
					case 'Ó' :
						ayRes[ i ] = 0xD3;
						break;
					
					case 'Ö' :
						ayRes[ i ] = 0xD6;
						break;
					
					case 'Ő' :
						ayRes[ i ] = 0xD5;
						break;
					
					case 'Ú' :
						ayRes[ i ] = 0xDA;
						break;
					
					case 'Ü' :
						ayRes[ i ] = 0xDC;
						break;
					
					case 'Ű' :
						ayRes[ i ] = 0xDB;
						break;

					//
					// //
					
					default :
						ayRes[ i ] = ((byte) c);
						break;
					
				}
			}
			
			return ayRes;
		}
		
		public static string AsciiToString( byte[] ay, int iOffset, int iSize )
		{
			
			StringBuilder sb;
			int i;
			byte y;
			
			sb = new StringBuilder( iSize + 1 );
			for( i = 0; i < iSize; i++ )
			{
				y = ay[ iOffset + i ];
				
				switch( y )
				{
					
					// //
					//

                    case 0xE1 :
                        sb.Append('á');
						break;

                    case 0xE9 :
                        sb.Append('é');
						break;

                    case 0xED :
                        sb.Append('í');
						break;

                    case 0xF3 :
                        sb.Append('ó');
						break;

                    case 0xF6 :
                        sb.Append('ö');
						break;

                    case 0xF5 :
                        sb.Append('ő');
						break;

                    case 0xFA :
                        sb.Append('ú');
						break;

                    case 0xFC :
                        sb.Append('ü');
						break;

                    case 0xFB :
                        sb.Append('ű');
						break;
			
					//
					// //
					//

                    case 0xC1 :
                        sb.Append('Á');
						break;

                    case 0xC9 :
                        sb.Append('É');
						break;

                    case 0xCD :
                        sb.Append('Í');
						break;

                    case 0xD3 :
                        sb.Append('Ó');
						break;

                    case 0xD6 :
                        sb.Append('Ö');
						break;

                    case 0xD5 :
                        sb.Append('Ő');
						break;

                    case 0xDA :
                        sb.Append('Ú');
						break;

                    case 0xDC :
                        sb.Append('Ü');
						break;

                    case 0xDB :
                        sb.Append('Ű');
						break;
						
					//
					// //
					
					default :
						sb.Append(((char) y));
						break;
				}
				
			}
			
			return sb.ToString();
		}
	}
	
	public static class Rsc
	{
		
		public const System.Windows.Visibility Visible = System.Windows.Visibility.Visible;
		public const System.Windows.Visibility Collapsed = System.Windows.Visibility.Collapsed;
		
		public static System.Windows.Visibility ConditionalVisibility( bool bCondition )
		{
			if( bCondition )
				return Rsc.Visible;
			else
				return Rsc.Collapsed;
		}
		
	}
	
}