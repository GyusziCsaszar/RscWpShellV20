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

using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;

using Ressive.Utils;
using Ressive.Store;

using Ressive.ShellTiles;

namespace Launcher_Lib
{
	
	public class SysTiles : RscShellTileManager
	{
		
		public override string GetInfo( bool bForSysTile, string sIcoId, out Brush brBk, out Brush brFore,
			out double dFontSize, out string sErr,
			out string sNotiTitle, out string sNotiContent, out string sNotiSound,
			bool bCalledByAgent, object oAgentParam, out string sInfoToChngChk )
		{
			
			brBk = null;
			brFore = null;
			dFontSize = 0;
			sErr = "";
			
			sNotiTitle = "";
			sNotiContent = "";
			sNotiSound = "";
			
			sInfoToChngChk = "";
			
			string sCnt = "";
			
			try
			{
				
				switch( sIcoId )
				{
					
					case "sysTm" :
					{
						DateTime dNow = DateTime.Now;
						sCnt +=	RscUtils.pad60(dNow.Hour) +
								":" + RscUtils.pad60(dNow.Minute);
						
						if( bForSysTile )
						{
							sCnt += "\n\n(as of)";
						}
						else
						{
							sCnt += "\n:" + RscUtils.pad60(dNow.Second);
						}
						
						break;
					}
					
					case "sysDtFull" :
					{
						DateTime dNow = DateTime.Now;
						sCnt +=	dNow.Year.ToString() +
								".\n" + RscUtils.pad60(dNow.Month) + "." +
								RscUtils.pad60(dNow.Day) + ".";
						break;
					}
					
					case "sysDtDay" :
					{
						DateTime dNow = DateTime.Now;
						sCnt +=	RscUtils.pad60(dNow.Day) + ". ";
						switch( dNow.DayOfWeek )
						{
							case DayOfWeek.Monday: sCnt += "H"; break;
							case DayOfWeek.Tuesday: sCnt += "K"; break;
							case DayOfWeek.Wednesday: sCnt += "Sze"; break;
							case DayOfWeek.Thursday: sCnt += "Cs"; break;
							case DayOfWeek.Friday: sCnt += "P"; break;
							case DayOfWeek.Saturday: sCnt += "Szo"; break;
							case DayOfWeek.Sunday: sCnt += "V"; break;
						}
						
                        sCnt += "\n" + RscUtils.pad60(RscUtils.WeekOfYearHU(dNow)) + ". hét";
						break;
					}
					
					case "sysFsFree" :
					{	
						string sIsoStoreDrive = "";
						long lFree = RscStore.AvailableFreeSpace( out sIsoStoreDrive );
						
						sCnt += RscUtils.toMBstr( lFree, true);
	
						if( sIsoStoreDrive.Length > 0 )
						{
							sCnt = sIsoStoreDrive + "\n" + sCnt;
						}
						
						sNotiTitle = "Free Space";
						sNotiContent = sCnt.Replace( '\n', ' ' );
						
						sInfoToChngChk = RscUtils.toMBstr( lFree, true, false, 1);
						
						break;
					}
					
					case "sysBatPow" :
					{	
						sCnt += Windows.Phone.Devices.Power.Battery.GetDefault().RemainingChargePercent.ToString() + " %";
						
						if( Microsoft.Phone.Info.DeviceStatus.PowerSource == Microsoft.Phone.Info.PowerSource.Battery )
							sCnt += ""; //"\n\nBATT";
						else
							sCnt += "\n\nCHRG";
						
						if( Windows.Phone.Devices.Power.Battery.GetDefault().RemainingChargePercent >= 100 &&
							Microsoft.Phone.Info.DeviceStatus.PowerSource != Microsoft.Phone.Info.PowerSource.Battery )
						{
							brBk = new SolidColorBrush(Colors.Red);
							brFore = new SolidColorBrush( Colors.White );
						
							sNotiTitle = "Battery Charge";
							sNotiContent = sCnt.Replace( '\n', ' ' );
							
							//sNotiSound = /*"/Lib_Rsc;component/" +*/ "Media/BociBociTarka.wav";
							sNotiSound = /*"/Lib_Rsc;component/" +*/ "Media/DingDing.wav";
						}
						
						break;
					}
					
					case "sysCellNet" :
					{
						dFontSize = 11;
						
						string sCmo = DeviceNetworkInformation.CellularMobileOperator;
						for( int i = 0; i < 1; i++ )
						{
							int iPos = sCmo.IndexOf( ' ' );
							if( iPos < 0 )
							{
								sCmo += "\n";
								//if( i == 0 ) sCmo += "\n";
								break;
							}
							
							sCmo = sCmo.Substring( 0, iPos ) + "\n" + sCmo.Substring( iPos + 1 );
						}
						
						sCnt += sCmo + "\n";
						
						if( DeviceNetworkInformation.IsCellularDataEnabled )
							sCnt += "d(ata) ON";
						else
							sCnt += "d(ata) OFF";
						sCnt += "\n";
						
						if( DeviceNetworkInformation.IsCellularDataRoamingEnabled )
							sCnt += "d roam ON";
						else
							sCnt += "d roam OFF";
						
						break;
					}
					
					case "sysCnt_Note" :
					{
						int iCount = 0;
						
						RscStore store = new RscStore();
						
						if( store.FolderExists( "A:\\Documents\\Notes" ) )
						{
							string[] fles = store.GetFileNames("A:\\Documents\\Notes", "*.txt");
							iCount = fles.Length;
						}
						
						brBk = new SolidColorBrush( Color.FromArgb(255, 252, 244, 178) );
						brFore = new SolidColorBrush( Colors.Black );
						sCnt += iCount.ToString() + "\n\nnotes";
						
						break;
					}
					
					case "sysCnt_Anni" :
					{
							
						int iCntRed = 0;
						int iCntOrange = 0;
						int iCntGreen = 0;
						int iCntBlue = 0;
						int iCntGray = 0;
						
						RscStore store = new RscStore();
						
						if( store.FolderExists( "A:\\Documents\\Dates" ) )
						{
							string[] fles = RscSort.OrderBy(store.GetFileNames("A:\\Documents\\Dates", "*.txt"));
							foreach( string sFle in fles )
							{
								bool bTmp;
								string sDate = store.ReadTextFile( "A:\\Documents\\Dates" + "\\" + sFle, "", out bTmp );
												
								if( sDate.Length == 0 ) continue;
							
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
								
									int iY = 1901; Int32.TryParse( sY, out iY ); //parseInt(sY);
									int iM = 1; Int32.TryParse( sM, out iM ); //parseInt(sM);
									int iD = 1; Int32.TryParse( sD, out iD ); //parseInt(sD);
								
									dtNow = DateTime.Now;
									DateTime dt1 = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day);
									DateTime dt2 = new DateTime(iY,iM,iD);
								
									//var dDiff = dt1.getTime() - dt2.getTime();
									TimeSpan tsDiff = dt1 - dt2;
									double dDiff = tsDiff.TotalMilliseconds;
								
									double dDiffD = Math.Floor(dDiff / (1000 * 3600 * 24));
								
									/*
									double dDiffW;
									if( dDiffD < 0 )
									{
										dDiffW = Math.Round((dDiffD * -1) / 7, 0) * -1;
									}
									else
									{
										dDiffW = Math.Round(dDiffD / 7, 0);
									}
									*/
									
									if( bAnniver && (dDiffD > 7) ) continue;
								
									/*
									var sWtit = dDiffW.ToString() +
										"w " + (dDiffD - (dDiffW * 7)).ToString() + "d";
									*/
									
									if( bAnniver )
									{
										if( (dDiffD * dDiffD) < 10 )
										{
											iCntRed++;
										}
										else
										{
											if( (dDiffD * dDiffD) < 50 )
											{
												iCntOrange++;
											}
											else
											{
												if( (dDiffD * dDiffD) < 197 )
												{
													iCntGreen++;
												}
												else
												{
													iCntBlue++;
												}
											}
										}
									}
									else
									{
										iCntGray++;
									}
								}
							}
						}
							
						int iCnt = 0;
						string sMsg = "";
						if( iCntRed > 0 )
						{
							sMsg = "+/- 3d";
							iCnt = iCntRed;
							brBk = new SolidColorBrush(Colors.Red);
							brFore = new SolidColorBrush( Colors.White );
						}
						else if( iCntOrange > 0 )
						{
							sMsg = "+/- 1w";
							iCnt = iCntOrange;
							brBk = new SolidColorBrush(Colors.Orange);
							brFore = new SolidColorBrush( Colors.White );
						}
						else if( iCntGreen > 0 )
						{
							sMsg = "2w";
							iCnt = iCntGreen;
							brBk = new SolidColorBrush(Colors.Green);
							brFore = new SolidColorBrush( Colors.White );
						}
						else if( iCntBlue > 0 )
						{
							sMsg = "> 2w";
							iCnt = iCntBlue;
							brBk = new SolidColorBrush(Colors.Blue);
						}
						else //if( iCntGray > 0 )
						{
							sMsg = "dates";
							iCnt = /*iCntBlue +*/ iCntGray;
							
							//Use default...
							//brBk = new SolidColorBrush(Colors.Black);
							//brFore = new SolidColorBrush( Colors.White );
						}
						
						sCnt += iCnt.ToString() + "\n\n" + sMsg;
						
						sNotiTitle = "Anniversary";
						sNotiContent = sCnt.Replace( '\n', ' ' );
						
						break;
					}
					
					case "sysCnt_WebDog" :
					{
						RscStore store = new RscStore();
						
						if( store.FileExists( "A:\\Documents\\WebDogUri.txt" ) )
						{
							DateTimeOffset dtf = store.GetFileLastWriteTime( "A:\\Documents\\WebDogUri.txt" );
							DateTime dt = dtf.DateTime;
							DateTime dtNow = DateTime.Now;
							TimeSpan tsDiff = dtNow - dt;
							
							bool bTmp;
							string sUri = store.ReadTextFile( "A:\\Documents\\WebDogUri.txt", "", out bTmp );
							if( sUri.Length > 0 )
							{
								try
								{
									Uri uri = new Uri( sUri, UriKind.Absolute );
									string sDns = uri.DnsSafeHost;
									
									if( sDns.Length > 5 )
									{
										sDns = sDns.Substring(0, 5);
										sDns += "...";
									}
									
									sCnt += sDns;
								}
								catch( Exception )
								{
								}
							}
							sCnt += "\n";
							
							//sCnt = RscUtils.toDurationStr( tsDiff.Duration() );
							if( Math.Floor( tsDiff.TotalDays ) > 0 )
							{
								brBk = new SolidColorBrush( Colors.Red );
								brFore = new SolidColorBrush( Colors.White );
								
								sCnt += Math.Floor( tsDiff.TotalDays ).ToString() + "\nday(s)";
							}
							else if( Math.Floor( tsDiff.TotalHours ) > 0 )
							{
								//Use Default...
								//brBk = new SolidColorBrush( Colors.Black );
								//brFore = new SolidColorBrush( Colors.White );

								sCnt += Math.Floor( tsDiff.TotalHours ).ToString() + "\nhour(s)";
							}
							else //if( Math.Floor( tsDiff.TotalMinutes ) > 0 )
							{
								//Use Default...
								//brBk = new SolidColorBrush( Colors.Black );
								//brFore = new SolidColorBrush( Colors.White );

								sCnt += Math.Floor( tsDiff.TotalMinutes ).ToString() + "\nmin(s)";
							}
							/*
							else //if( Math.Floor( tsDiff.TotalSeconds ) > 0 )
							{
								//Use Default...
								//brBk = new SolidColorBrush( Colors.Black );
								//brFore = new SolidColorBrush( Colors.White );

								sCnt += Math.Floor( tsDiff.TotalSeconds ).ToString() + "\n\nsec(s)";
							}
							*/
						}
						else
						{
							if( bForSysTile )
							{
								//Use Default...
								//brBk = new SolidColorBrush( Colors.Black );
								//brFore = new SolidColorBrush( Colors.White );
							}
							else
							{
								brBk = new SolidColorBrush( Color.FromArgb(0, 0, 0, 0) );
								brFore = new SolidColorBrush( Colors.Black );
							}
						}
						
						break;
					}
					
					case "sysCnt_Event" :
					{
						int iCountErr = 0;
						int iCountInf = 0;
						
						RscStore store = new RscStore();
						
						if( store.FolderExists( "A:\\System\\Events" ) )
						{
							
							string[] fles = store.GetFileNames("A:\\System\\Events", "*.error");
							iCountErr = fles.Length;
							
							fles = store.GetFileNames("A:\\System\\Events", "*.info");
							iCountInf = fles.Length;
							
							if( iCountErr > 0 )
							{
								brBk = new SolidColorBrush( Colors.Red );
								brFore = new SolidColorBrush( Colors.White );
							}
							else if( iCountInf > 0 )
							{
								brBk = new SolidColorBrush( Colors.Blue );
								brFore = new SolidColorBrush( Colors.White );
							}
							else
							{
								//Use Default...
								//brBk = new SolidColorBrush( Colors.Black );
								//brFore = new SolidColorBrush( Colors.White );
							}
						}
						
						sCnt += iCountErr.ToString() + " err\n"
							/*+ "\n"*/
							+ iCountInf.ToString() + " inf";
						
						break;
					}
					
					case "sysCnt_PerDay" :
					{
						sCnt += "";
						
						if( bForSysTile )
						{
							//Use Default...
							//brBk = new SolidColorBrush( Colors.Black );
							//brFore = new SolidColorBrush( Colors.White );
						}
						else
						{
							brBk = new SolidColorBrush( Color.FromArgb(0, 0, 0, 0) );
							brFore = new SolidColorBrush( Colors.Black );
						}
						
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
									int iY = Int32.Parse( aTx[0] );
									int iM = Int32.Parse( aTx[1] );
									int iD = Int32.Parse( aTx[2] );
									
									DateTime dtNow = DateTime.Now;
									
									DateTime d1 = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day);
									DateTime d2 = new DateTime(iY, iM, iD);
									
									TimeSpan ts = d2 - d1;
									int iDays =  Math.Max( 1, (int) Math.Round(ts.TotalDays, 0) );
									
									double dAmo = double.Parse(aTx[3]);				
									if( dAmo != 0 )
									{
										int iRes = (int) Math.Round(dAmo / iDays, 0);
										
										string sUnit = "";
										if( aTx[4].Length > 0 )
										{
											sUnit = " " + aTx[4];
										}
										
										sCnt += aTx[3] + sUnit + "\n"
											+ /*(iY % 1000).ToString()*/ aTx[0] + "." + RscUtils.pad60(iM) + "." + RscUtils.pad60(iD) + "." + "\n"
											+ "----------" + "\n"
											+ iRes.ToString() + sUnit + "\n"
											+ "/ day" + " (" + Math.Max( 0, (int) Math.Round(ts.TotalDays, 0) ).ToString() + ")";
														
										brBk = new SolidColorBrush( Colors.Gray );
										brFore = new SolidColorBrush( Colors.White );
										
										dFontSize = 11;
									}
								}
							}
						}
						
						break;
					}
					
					default :
					{
						sCnt = "???";
						break;
					}
					
				}
			}
			catch( Exception e )
			{
				//Do not generate too many err files...
				RscStore.AddSysEvent( e, "Tile_Info_Title_Createion_Error" );
				
				sErr = e.Message + "\r\n" + e.StackTrace;
				sCnt = "ERR!";
			}
			
			return sCnt;
		}

	}
	
}