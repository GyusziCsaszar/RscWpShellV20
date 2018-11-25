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

using System.Threading.Tasks;

using Ressive.Utils;
using Ressive.Store;

using Ressive.Formats;

using Ressive.GoogleApi;

namespace RscGoogleApiMail_Lib
{
	
	public class AppLogic
	{
		
		public const int ciAuthFail = -2000;
		
		public const string csSecretsFolder = "A:\\Secrets\\GoogleApiMail";
		
		public const int ciCurrentVersion = 103;
		
		public static RscGoogleAuthResult LoadAuthResult( out bool bNotExists )
		{
			bNotExists = false;
			
            RscGoogleAuthResult auth;
			
			RscStore store = new RscStore();
			store.CreateFolderPath( AppLogic.csSecretsFolder );
			auth = store.ReadXmlDataFile<RscGoogleAuthResult>( AppLogic.csSecretsFolder + "\\" + "AUTH.xml", null, out bNotExists );
			
			return auth;
		}
		
		public static void SaveAuthResult( RscGoogleAuthResult auth )
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
            {				
				RscStore store = new RscStore();
				store.CreateFolderPath( AppLogic.csSecretsFolder );
				store.WriteXmlDataFile( AppLogic.csSecretsFolder + "\\" + "AUTH.xml", auth, true );
			});
		}
		
		public static int SaveThreadData( bool bCalledByAgent, string sPath, RscJSonItem jsonThreads )
		{
			
			RscJSonItem json = jsonThreads.GetChildByName( "threads" );
			if( json == null )
				return -200;
			
			RscStore store = new RscStore();

            bool bFirstRun = false;
			
			sPath += "\\Threads";
			if( !store.FolderExists( sPath ) )
			{
                bFirstRun = true;

				store.CreateFolderPath( sPath );
				store.WriteTextFile( sPath + "\\" + "Version.txt", ciCurrentVersion.ToString(), true );
			}
			
			string sThreadIdOrder_OLD = "";
			sThreadIdOrder_OLD = store.ReadTextFile( sPath + "\\" + "IdOrder" + ".txt", sThreadIdOrder_OLD );
			sThreadIdOrder_OLD += "|"; //ATTENTION!!! 1 of 2
			string sThreadIdOrder_NEW = "";
			
			int iJustAdded = 0;
			
			int iThCnt = json.ChildCount;
			for( int iTh = 0; iTh < iThCnt; iTh++ )
			{
				RscJSonItem jsonTh = json.GetChild( iTh );
				
				string sID = jsonTh.GetPropertyValue( "id" );
				string sHistoryID = jsonTh.GetPropertyValue( "historyId" );
				string sSnippet = jsonTh.GetPropertyValue( "snippet" ); //, true );
				
				if( sID.Length == 0 || sHistoryID.Length == 0 || sSnippet.Length == 0 )
					continue;
				
				store.CreateFolderPath( sPath + "\\" + sID );
				
				if( store.FileExists( sPath + "\\" + sID + "\\" + sHistoryID + ".xml" ) )
				{
					
					//Threads arrives in reverse order, so...
					break;
					
				}
				else
				{
					sThreadIdOrder_OLD = sThreadIdOrder_OLD.Replace( sID + "|", "" ); //Removing ID
					if( sThreadIdOrder_NEW.Length > 0 ) sThreadIdOrder_NEW += "|";
					sThreadIdOrder_NEW += sID; //Adding ID
					
					string sIdOrder = "";
					sIdOrder = store.ReadTextFile( sPath + "\\" + sID + "\\" + "IdOrder" + ".txt", "" );
					
					iJustAdded++;
					
					MyThread2 th = new MyThread2();
					th.ID = sID;
					th.HistoryID = sHistoryID;
					th.Snippet = sSnippet;

                    if (bFirstRun)
                    {
                        th.DateAcked = DateTime.Now;
                    }
					
					store.WriteXmlDataFile( sPath + "\\" + sID + "\\" + sHistoryID + ".xml", th, true );
					
					if( sIdOrder.Length > 0 )
						sIdOrder = "|" + sIdOrder;
					sIdOrder = sHistoryID + sIdOrder;
					store.WriteTextFile( sPath + "\\" + sID + "\\" + "IdOrder" + ".txt", sIdOrder, true );
				}
			}
			
			if( sThreadIdOrder_OLD.Length > 0 )
			{
				 //ATTENTION!!! 2 of 2
				sThreadIdOrder_OLD = sThreadIdOrder_OLD.Substring( 0, sThreadIdOrder_OLD.Length - 1 );
			}
			if( sThreadIdOrder_NEW.Length > 0 && sThreadIdOrder_OLD.Length > 0 ) sThreadIdOrder_NEW += "|";
			sThreadIdOrder_NEW += sThreadIdOrder_OLD;
			store.WriteTextFile( sPath + "\\" + "IdOrder" + ".txt", sThreadIdOrder_NEW, true );
			
			int iCountAll = 0;
			iCountAll = store.ReadXmlDataFile( sPath + "\\" + "Count" + ".xml", iCountAll );
			iCountAll += iJustAdded;
			store.WriteXmlDataFile( sPath + "\\" + "Count" + ".xml", iCountAll, true );			
			
			int iCount_NonAckAll = 0;
			iCount_NonAckAll = store.ReadXmlDataFile( sPath + "\\" + "Count_NonAck" + ".xml", iCount_NonAckAll );
            if (!bFirstRun)
            {
                iCount_NonAckAll += iJustAdded;
            }
			store.WriteXmlDataFile( sPath + "\\" + "Count_NonAck" + ".xml", iCount_NonAckAll, true );
			
			int iCount_NEW = 0;
			iCount_NEW = store.ReadXmlDataFile( sPath + "\\" + "Count_NEW" + ".xml", iCount_NEW );
            if (bCalledByAgent)
                iCount_NEW += iJustAdded;
            else if (bFirstRun)
                iCount_NEW = 0;
            else
                iCount_NEW = 0;
			store.WriteXmlDataFile( sPath + "\\" + "Count_NEW" + ".xml", iCount_NEW, true );			
			
			return iJustAdded;
		}
		
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
			if( sVersion == ciCurrentVersion.ToString() )
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
				if( iVer >= ciCurrentVersion )
					break;
			
				switch( iVer )
				{
					
					case 0 :
					case 100 :
					case 101 :
					{
						string [] asIDs = store.GetFolderNames( sFolder, "*.*" );
						
						int iSubCntr = 0;
						
						for( int i = 0; i < asIDs.Length; i++ )
						{
							string sIdOrder = "";
							sIdOrder = store.ReadTextFile( sFolder + "\\" + asIDs[ i ] + "\\" + "IdOrder" + ".txt", sIdOrder );
							string [] asIdOrders = sIdOrder.Split( '|' );
							
							iSubCntr += asIdOrders.Length;
							
							string sId = asIdOrders[ asIdOrders.Length - 1 ];
					
							MyThread2 th = new MyThread2();
							th = store.ReadXmlDataFile( sFolder + "\\" +  asIDs[ i ] + "\\" + sId + ".xml", th );
							if( th.ID.Length == 0 )
								return false; //FAIL!!!
							
							string sTmp = "";
							
							sTmp += th.DateSaved.Year.ToString();
							sTmp += RscUtils.pad60( th.DateSaved.Month );
							sTmp += RscUtils.pad60( th.DateSaved.Day );
							sTmp += "_";
							sTmp += RscUtils.pad60( th.DateSaved.Hour );
							sTmp += RscUtils.pad60( th.DateSaved.Minute );
							sTmp += RscUtils.pad60( th.DateSaved.Second );
							sTmp += "_";
							int iMs = th.DateSaved.Millisecond;
							if( iMs < 10 )
							{
								sTmp += "00";
							}
							else
							{
								if( iMs < 100 ) sTmp += "0";
							}
							sTmp += iMs.ToString();
							
							asIDs[ i ] = sTmp + "|" + asIDs[ i ];
						}
						
						asIDs = RscSort.OrderBy(asIDs, true);
						
						/*
						StringBuilder sb = new StringBuilder();
						*/
						
						string sRes = "";
						
						foreach( string sID in asIDs )
						{
							/*
							sb.AppendLine( sID );
							*/
							
							if( sRes.Length > 0 ) sRes += "|";
							sRes += sID.Substring( 20 );
						}
						/*
						sb.Append( "\r\n" );
						sb.Append( "Cntr: " + asIDs.Length.ToString() + "\r\n" );
						sb.Append( "Sub-Cntr: " + iSubCntr.ToString() + "\r\n" );
						store.WriteTextFile( "A:\\GMail.txt", sb.ToString(), true );
						*/
						
						store.WriteTextFile( sFolder + "\\" + "IdOrder" + ".txt", sRes, true );
						
						iVer = 102;
						
						break;
					}
					
					//Denie endless loop...
					default :
						iVer = ciCurrentVersion;
						break;
				}
				
				store.WriteTextFile( sFolder + "\\" + "Version.txt", iVer.ToString(), true );
			}
			
			//
			// //
			
			return true;
		}
		
		string m_sUserIDlast = "";
		
		//RscGoogleAuth m_gAuth = null;
		
		public AppLogic()
		{
		}
		
		public int ReadThreadData()
		{
			
			RscStore store = new RscStore();
			store.CreateFolderPath( AppLogic.csSecretsFolder );
			
			string sPath = AppLogic.csSecretsFolder + "\\" + "client_secret.json";
			bool bNotExists = false;
			string sJSon = store.ReadTextFile( sPath, "", out bNotExists );
			if( bNotExists )
			{
				return -2;
			}
			
			string sErr = "";
			RscJSonItem json = RscJSon.FromResponseContetn( sJSon, out sErr );
			if( sErr.Length > 0 )
			{
				//Try to get values needed...
				//return -3;
			}
				
			m_sUserIDlast = store.ReadTextFile( AppLogic.csSecretsFolder + "\\" + "UserIDlast.txt", "" );
			if( m_sUserIDlast.Length == 0 )
			{
				return -3;
			}
			
			// //
			//
			
			//if( gAuth == null )
			//{
				RscGoogleAuth gAuth = new RscGoogleAuth( json,
						RscGoogleScopes.UserinfoEmail
						+ " " + RscGoogleScopes.UserinfoProfile
						+ " " + RscGoogleScopes.Gmail,
						null ); //webBrowser1 );
				
				gAuth.Authenticated += new EventHandler(m_gAuth_Authenticated);
				gAuth.AuthenticationFailed += new EventHandler(m_gAuth_AuthenticationFailed);
				gAuth.ShowAuthPage += new EventHandler(m_gAuth_ShowAuthPage);
				gAuth.ResponseReceived += new Ressive.GoogleApi.RscGoogleAuth.ResponseReceived_EventHandler(m_gAuth_ResponseReceived);
				
				//bool bNotExists;
				gAuth.AuthResult = LoadAuthResult( out bNotExists);
				if( bNotExists )
					return -4;
			//}
			
			//
			// //
			
			string sUriResource = "";
			string sBaseUrl = GoogleUtils.GoogleRequestBaseUrl( GoogleRequest.GMail_Threads, out sUriResource, "me", "" );
			if( sBaseUrl.Length == 0 )
			{
				return -5;
			}
			
			/*
			Object oRes = null;
			var taskHlp = Task.Run(async () => { oRes = await m_gAuth.SendRequestTask( sBaseUrl, sUriResource ); });
			taskHlp.Wait();
			
			if( oRes == null )
				return -6;
			else
				return oRes.ToString().Length;
			*/
			
			//Task<Object> tsk = gAuth.SendRequestTask( sBaseUrl, sUriResource );
			//tsk.Wait();
			
			/*
			int iRet = 0;
			var taskHlp = Task.Run(async () => { iRet = await SendRequestTask( gAuth, sBaseUrl, sUriResource ); });
			taskHlp.Wait();
			*/
			
			/*
			Task<Object> tsk = gAuth.SendRequestTask( sBaseUrl, sUriResource );
			for(;;)
			{
				if( tsk.IsCompleted )
					break;
		
				//System.Threading.Tasks.Task.Delay(100).Wait();
				
				MessageBox.Show( "Not complete..." );
			}
			*/
			
			try
			{
				gAuth.SendRequest( sBaseUrl, sUriResource );
			}
			catch( Exception )
			{
				return -6;
			}
			
			return 0;
		}
		/*
		protected async Task<int> SendRequestTask(RscGoogleAuth gAuth, string sBaseUrl, string sUriResource)
		{
			await gAuth.SendRequestTask( sBaseUrl, sUriResource );
			return 0;
		}
		*/
		
        void m_gAuth_Authenticated(object sender, EventArgs e)
        {
			//SaveAuthResult( m_gAuth.AuthResult );
        }

        void m_gAuth_AuthenticationFailed(object sender, EventArgs e)
        {
        	//MessageBox.Show("Please try again", "Login failed", MessageBoxButton.OK);
		
			OnDone( ciAuthFail );
        }
		
		void m_gAuth_ShowAuthPage(object sender, EventArgs e)
		{
			/*
			
				webBrowser1.Visibility = Rsc.Visible;
				webBrowser1.Source = m_gAuth.AuthUri;
			
			*/
		}
		
		void m_gAuth_ResponseReceived(object sender, RscGoogleAuthEventArgs e)
		{	
			string sErr = "";
			RscJSonItem jsonRoot = null;	
			jsonRoot = RscJSon.FromResponseContetn( jsonRoot, e.Content, out sErr, e.Uri, e.ContentType );
			if( sErr.Length > 0 )
			{
				//OnDone( -101 );
				
				// If m_gAuth_AuthenticationFailed has called,
				// this will called too!!!
				OnDone( ciAuthFail );
				
				return;
			}
				
			string sErrorCode = "";
			if( jsonRoot.ChildCount > 0 )
			{
				if( jsonRoot.GetChild( 0 ).Name == "error" )
				{
					OnDone( -102 );
					return;
				}
			}
		
			string sPath = AppLogic.csSecretsFolder + "\\" + m_sUserIDlast;
			
			//Saving threads to have older thread content...
			int iNew = AppLogic.SaveThreadData( true, sPath, jsonRoot );
			OnDone( iNew );
			
			//MessageBox.Show( "Completed..." );
		}
		
		protected virtual void OnDone( int iNew )
		{
		}
		
	}
	
}