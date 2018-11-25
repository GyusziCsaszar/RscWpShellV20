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

using Ressive.Utils;
using Ressive.Store;

using Ressive.ShellTiles;

namespace RscGoogleApiMail_Lib
{
	
	public class SysTiles : RscShellTileManager
	{
		
		public const string csTileID = "GMail";
		
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
					
					case csTileID :
					{						
						int iCount_NEW = 0;
						int iCount_NonAckAll = 0;
						int iCountAll = 0;
						
						RscStore store = new RscStore();
				
						string sUserIDlast = store.ReadTextFile( AppLogic.csSecretsFolder + "\\" + "UserIDlast.txt", "" );
						
						if( sUserIDlast.Length > 0 )
						{
							string sPath = AppLogic.csSecretsFolder + "\\" + sUserIDlast;
							
							sPath += "\\Threads";
							if( store.FolderExists( sPath ) )
							{
								iCount_NEW = store.ReadXmlDataFile( sPath + "\\" + "Count_NEW" + ".xml", iCount_NEW );
								iCount_NonAckAll = store.ReadXmlDataFile( sPath + "\\" + "Count_NonAck" + ".xml", iCount_NonAckAll );					
								iCountAll = store.ReadXmlDataFile( sPath + "\\" + "Count" + ".xml", iCountAll );
							}
						}
						
						int iNewNow = 0;
						if( bForSysTile )
						{
							/*
							try
							{
								AppLogic al = new AppLogic();
								iNew = al.ReadThreadData( );
							}
							catch( Exception )
							{
								iNew = -1;
							}
							*/
							
							if( bCalledByAgent )
							{
								iNewNow = 0;
								if( !Int32.TryParse( oAgentParam.ToString(), out iNewNow ) )
									iNewNow = -400;
							}
						}
						else
						{
						}
						
						string sTile;
						if( iNewNow < 0 )
						{
							if( iNewNow == AppLogic.ciAuthFail )
							{
								sTile = "NO CONN!";
							}
							else
							{
								sTile = "ERR: " + iNewNow.ToString(); //"ERROR!";
							}
						}
						else
						{
							sTile = "new: " + iCount_NEW.ToString();
						}
						sCnt += sTile;
						
						string sCont = "non-ack: " + iCount_NonAckAll.ToString() + " / " + iCountAll.ToString();
						
						sCnt += "\n" + "non-ack: " + iCount_NonAckAll.ToString();
						sCnt += "\n" + "all: " + iCountAll.ToString();
						
						sInfoToChngChk = sCnt;
						
						DateTime dNow = DateTime.Now;
						sCnt +=	"\n\n" + "at: " + RscUtils.pad60(dNow.Hour) +
								":" + RscUtils.pad60(dNow.Minute);
													
						if( iNewNow < 0 || iNewNow > 0 || iCount_NEW > 0 )
						{
							brBk = new SolidColorBrush( Colors.Red );
							
							sNotiTitle = sTile;
							sNotiContent = sCont;
							
							//sNotiSound = /*"/Lib_Rsc;component/" +*/ "Media/BociBociTarka.wav";
							sNotiSound = /*"/Lib_Rsc;component/" +*/ "Media/Ding.wav";
						}
						else
						{
							if( iCount_NonAckAll > 0 )
							{
								brBk = new SolidColorBrush( Colors.Orange );
							}
							else
							{
								brBk = new SolidColorBrush( Colors.Green );
							}
						}
						brFore = new SolidColorBrush( Colors.White );
						
						dFontSize = 11;
						
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