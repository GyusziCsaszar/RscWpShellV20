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
using Ressive.FrameWork;

using Ressive.Formats;

using RscGoogleApiMail_Lib;

namespace RscGoogleApiMail
{
	
	public class TreeLbItemThread : RscTreeLbItemBase
	{
		
		public static string DecorateSnippet( MyThread2 th )
		{
			string sRet = RscJSon.UnDecorate( th.Snippet );
			
			//Making more readable...
			sRet += "...";
			sRet = sRet.Replace( "! ", "!\n\n" );
			sRet = sRet.Replace( ". ", ".\n\n" );
			
			//Adding HisID to debug, test, etc...
			sRet += "\n\n(History ID: " + th.HistoryID + ")";
			
			if( th.Acknowledged )
			{
				sRet += "\n(acknowledged: " + th.DateAcked.ToString() + ")";
			}
			
			return sRet;
		}
		
		public string sID = "";
		public string sHistoryID = "";
		
		public List<MyThread2> m_a = new List<MyThread2>();
		
		public TreeLbItemThread( RscTreeLbItemList oHolder, RscTreeLbItemBase oParent )
		: base( oHolder, oParent )
		{
		}
		
		override public void ClearData()
		{
			//Called to force data reload on next Expand...
			
			//NOP...
		}
		
		override public void Expand()
		{
			if( Expanded )
				return;
			
			if( sID.Length == 0 || sHistoryID.Length == 0 )
			{
				base.Expand();
				return;
			}
			
			PreInserts();
			
			foreach( MyThread2 thSub in m_a )
			{
				TreeLbItem tiSub = new TreeLbItem( Holder, this );
				//
				tiSub.sID = thSub.ID;
				tiSub.sHistoryID = thSub.HistoryID;
				//
				tiSub.DetailsFontSize = DetailsFontSize;
				tiSub.DetailsBackColor = Holder.Theme.ThemeColors.TextDarkBack;
				tiSub.DetailsForeColor = Holder.Theme.ThemeColors.TextDarkFore;
				if( !thSub.Acknowledged )
				{
					tiSub.CustomBackColor = Colors.Orange;
					
					tiSub.BtnCustom1Visibility = Rsc.Visible;
				}
				tiSub.BtnCustom1Image = BtnCustom1Image;
				//
				tiSub.Title = RscUtils.toDateDiff( thSub.DateSaved );
				tiSub.DetailsOfTitle = TreeLbItemThread.DecorateSnippet( thSub );
				tiSub.IsLeaf = true;
				
				Insert( tiSub );
			}
			
			base.Expand();
		}
		
	}
	
	/*
	public class MyThread
	{
		
		public string ID {set; get;}
		public string HistoryID {set; get;}
		public string Snippet {set; get;}
		
		public DateTime DateSaved {set; get;}
		public DateTime DateAcked {set; get;}
		
		public MyThread()
		{
			ID = "";
			HistoryID = "";
			Snippet = "";
			
			DateSaved = DateTime.Now;
			DateAcked = DateTime.MinValue;
		}
		
		public bool Acknowledged
		{
			get
			{
				if( DateAcked == DateTime.MinValue ) return false;
				return true;
			}
		}
		
	}
	public static class MyThread_Upgrade
	{
		
		public static bool VersionUpgrade( string sUserIDlast, bool bChkOnly )
		{			
			RscStore store = new RscStore();
			
			string sFolder = AppLogic.csSecretsFolder + "\\" + sUserIDlast + "\\" + "Threads";
			
			int iOldVer = 0;
			
			if( !store.FolderExists( sFolder ) )
			{
				if( bChkOnly ) return true; //NOTHING stored yet...
				
				store.CreateFolderPath( sFolder );
			}
			
			string sVersion = store.ReadTextFile( sFolder + "\\" + "Version.txt", "0" );
			if( sVersion == AppLogic.ciCurrentVersion.ToString() )
			{
				return true;
			}
			
			if( bChkOnly ) return false;
			
			if( !Int32.TryParse( sVersion, out iOldVer ) )
				iOldVer = 0;
			
			// //
			//
			
			int iVer = iOldVer;
			for(;;)
			{
				if( iVer >= AppLogic.ciCurrentVersion )
					break;
			
				switch( iVer )
				{
					
					case 102 :
					{
						string [] asIDs = store.GetFolderNames( sFolder, "*.*" );
						
						int iSubCntr = 0;
						
						for( int i = 0; i < asIDs.Length; i++ )
						{
							string sIdOrder = "";
							sIdOrder = store.ReadTextFile( sFolder + "\\" + asIDs[ i ] + "\\" + "IdOrder" + ".txt", sIdOrder );
							string [] asIdOrders = sIdOrder.Split( '|' );
							
							foreach( string sId in asIdOrders )
							{
								MyThread th = new MyThread();
								th = store.ReadXmlDataFile( sFolder + "\\" +  asIDs[ i ] + "\\" + sId + ".xml", th );
								if( th.ID.Length > 0 )
								{
									MyThread2 th2 = new MyThread2();
									
									th2.ID = th.ID;
									th2.HistoryID = th.HistoryID;
									th2.Snippet = th.Snippet;
									th2.DateSaved = th.DateSaved;
									th2.DateAcked = th.DateAcked;
									
									store.WriteXmlDataFile( sFolder + "\\" +  asIDs[ i ] + "\\" + sId + ".xml", th2, true );
								}
							}
						}
						
						iVer = 103;
						
						break;
					}
					
					//Denie endless loop...
					default :
						iVer = AppLogic.ciCurrentVersion;
						break;
				}
				
				store.WriteTextFile( sFolder + "\\" + "Version.txt", iVer.ToString(), true );
			}
			
			//
			// //
			
			return true;
		}
	}
	*/
			
}