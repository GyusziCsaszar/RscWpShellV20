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
using Microsoft.Phone.Scheduler;

using System.Linq;

using Ressive.Utils;
using Ressive.Store;

using OsGrade;

namespace Ressive.ShellTiles
{
	
	public class RscShellTileManager
	{
		
		const double cdFontSize = 70;
		const double cdFontSize_SmDiff = 30;
		
		public RscShellTileManager()
		{
		}
		
		public virtual string GetInfo( bool bForSysTile, string sIcoId, out Brush brBk, out Brush brFore,
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
			
			//Do not generate too many err files...
			//RscStore.AddSysEvent( ex, "Tile_Info_Title_Createion_Error" );
			RscStore.AddSysEvent( "Not implemented: GetInfo", true, "Tile_Info_Title_Createion_Error" );
			
			return "???";
		}
		
		public void Create(string sPageNavTo, string sPageArgs, string sImage, string sStatus, string sIcoId = "")
		{
			RscStore store = new RscStore();		
			
			Guid gd = Guid.NewGuid();
			string gdStr = gd.ToString();				
			string sPageUri = "/MainPage.xaml" + "?IcoGd=" + gdStr;
			if( sIcoId.Length > 0 )
			{
				sPageUri += "&IcoId=" + sIcoId;
			}
			
			if( sIcoId.Length == 0 )
			{
				string sStFldr = RscKnownFolders.GetTempPath("ShellTiles", "" );
				store.WriteTextFile( sStFldr + "\\" + gdStr + ".txt", sPageNavTo + "\r\n" + sPageArgs, true );
			}
			else
			{
				//To make it enumerable...
				string sStFldr = RscKnownFolders.GetTempPath("ShellTiles", "" );
				store.WriteTextFile( sStFldr + "\\" + gdStr + ".txt", sIcoId, true );
			}
			
			string sImageUri = sImage;
			string sImgUriFinal = sImageUri;
			if( sImageUri.Length > 0 )
			{
				string sTileImg = gdStr;
				if( sImageUri.IndexOf( "isostore:\\" ) >= 0 )
				{
					sImageUri = sImageUri.Substring( 10 );
					sTileImg += RscStore.ExtensionOfPath( sImageUri );
				}
				else
				{
					sImageUri = sImageUri.Replace( "Images/", "A:\\System\\Theme\\Current\\" );
					sImageUri = sImageUri.Replace( ".jpg", ".png" );
					if( !store.FileExists( sImageUri ) )
					{
						sImageUri = sImageUri.Replace( ".png", ".jpg" );
						sTileImg += ".jpg";
					}
					else
					{
						sTileImg += ".png";
					}
				}
				
				string sTileImgPath = "A:\\Shared\\ShellContent";
				store.CreateFolderPath( sTileImgPath );
				sTileImgPath += "\\" + sTileImg;
				store.CopyFileForce(sImageUri, sTileImgPath );
				
				sImgUriFinal = "isostore:/" + sTileImgPath.Substring( 3 ).Replace('\\', '/');
			}
			
			string sTitle = sStatus;
			
			if( sIcoId.Length > 0 )
			{
				sTitle = "";
					
				Brush brBk = null;
				Brush brFore = null;
				double dFontSize = 0;
				string sErr = "";
				string sNotiTitle = "";
				string sNotiContent = "";
				string sNotiSound = "";
				string sInfoToChngChk = "";
				
				string sInfo = GetInfo( true, sIcoId, out brBk, out brFore, out dFontSize,
					out sErr, out sNotiTitle, out sNotiContent, out sNotiSound, 
					false, null, out sInfoToChngChk );
				
				if( sInfo == "" ) sInfo = "\n\n(N/A)";
				if( brBk == null ) brBk = new SolidColorBrush( Color.FromArgb( 255, 32, 32, 32 ) ); //Colors.Black );
				if( brFore == null ) brFore = new SolidColorBrush( Colors.White );
				if( dFontSize > 0 )
					dFontSize = cdFontSize - cdFontSize_SmDiff;
				else
					dFontSize = cdFontSize;
				if( sInfoToChngChk.Length == 0 )
					sInfoToChngChk = sInfo;
				
				//To update only if info has changed...
				string sStCntFldr = RscKnownFolders.GetTempPath("ShellTiles", "Content" );
				store.WriteTextFile( sStCntFldr + "\\" + gdStr + ".txt", sInfo, true );
			
				string sTileImgPath = "A:\\Shared\\ShellContent";
				store.CreateFolderPath( sTileImgPath );
				sTileImgPath += "\\" + gdStr + ".jpg";
				store.DeleteFile( sTileImgPath );
				RenderText( sInfo, sTileImgPath, brBk, brFore, dFontSize );
				
				sImgUriFinal = "isostore:/" + sTileImgPath.Substring( 3 ).Replace('\\', '/');
			}
			
			//MessageBox.Show( "Title: " + sTitle + "\r\n" + "NavTo: " + sPageUri + "\r\n" + "Image: " + sImageUri );
			
			StandardTileData initialData = new StandardTileData();
			{
				if( sImgUriFinal.Length > 0 ) initialData.BackgroundImage = new Uri(sImgUriFinal, UriKind.Absolute);
				if( sTitle.Length > 0 ) initialData.Title = sTitle;
			}
    		ShellTile.Create(new Uri(sPageUri, UriKind.Relative), initialData);
		}
		
		public void DoUpdate( bool bCalledByAgent, object oAgentParam )
		{
		
			RscStore.AddSysEvent( DateTime.Now.ToString(), false, "ShellTiles_DoUpdate" );
			
            var tiles = ShellTile.ActiveTiles;
			
			string sDbg = "";
			
			string sStCntFldr = RscKnownFolders.GetTempPath("ShellTiles", "Content" );
			
            foreach (ShellTile tile in tiles)
            {
				if( sDbg.Length > 0 ) sDbg += "\r\n";
				sDbg += tile.NavigationUri.OriginalString;
				
				try
				{
					StandardTileData updatedData = new StandardTileData();
					
					int iPos = tile.NavigationUri.OriginalString.IndexOf("IcoId=");
					if( iPos < 0 ) continue;
					iPos += 6;
					string sIcoId = tile.NavigationUri.OriginalString.Substring( iPos );
					
					iPos = tile.NavigationUri.OriginalString.IndexOf("IcoGd=");
					if( iPos < 0 ) continue;
					iPos += 6;
					int iPos2 = tile.NavigationUri.OriginalString.IndexOf('&', iPos);
					if( iPos2 < 0 ) continue;
					iPos2--;
					string sIcoGd = tile.NavigationUri.OriginalString.Substring(iPos, (iPos2 - iPos) + 1);
					
					/*
					DateTime dNow = DateTime.Now;
					string sTm = 	RscUtils.pad60(dNow.Hour) +
									":" + RscUtils.pad60(dNow.Minute); // + ":" + RscUtils.pad60(dNow.Second);
					
					updatedData.Title = RscUtils.pad60(dNow.Day) + ". " + sTm;
					*/
					
					Brush brBk = null;
					Brush brFore = null;
					double dFontSize = 0;
					string sErr = "";
					string sNotiTitle = "";
					string sNotiContent = "";
					string sNotiSound = "";
					string sInfoToChngChk = "";
					
					string sInfo = GetInfo( true, sIcoId, out brBk, out brFore, out dFontSize,
						out sErr, out sNotiTitle, out sNotiContent, out sNotiSound,
						bCalledByAgent, oAgentParam, out sInfoToChngChk );
					
					if( sInfo == "" ) sInfo = "\n\n(N/A)";
					if( brBk == null ) brBk = new SolidColorBrush( Color.FromArgb( 255, 32, 32, 32 ) ); //Colors.Black );
					if( brFore == null ) brFore = new SolidColorBrush( Colors.White );
					if( dFontSize > 0 )
						dFontSize = cdFontSize - cdFontSize_SmDiff;
					else
						dFontSize = cdFontSize;
					if( sInfoToChngChk.Length == 0 )
						sInfoToChngChk = sInfo;
					if( sErr.Length > 0 )
						sDbg += "\r\nERROR: " + sErr;
					
					RscStore store = new RscStore();
				
					// //
					//
					
					string sInfoToChngChk_Old = store.ReadTextFile( sStCntFldr + "\\" + sIcoGd + ".txt", "" );
					if( sInfoToChngChk_Old.CompareTo( sInfoToChngChk ) != 0 )
					{
						if( sNotiTitle.Length > 0 && sNotiContent.Length > 0 )
						{
							
							string sUriSnd = sNotiSound;
							if( sUriSnd.Length == 0 )
								sUriSnd = /*"/Lib_Rsc;component/" +*/ "Media/empty.mp3";
							
							ShellToast_Wp80U3.ShowToast( sNotiTitle, sNotiContent,
								new Uri( sUriSnd, UriKind.Relative) );
						}
						
						store.WriteTextFile( sStCntFldr + "\\" + sIcoGd + ".txt", sInfoToChngChk, true );
					}
					
					//
					// //
					//
					
					string sInfo_Old = store.ReadTextFile( sStCntFldr + "\\" + sIcoGd + "(full).txt", "" );
					if( sInfo_Old.CompareTo( sInfo ) == 0 )
					{
						sDbg += " " + sIcoId + "|" + sIcoGd + " (NO UPDATE, NO CHANGE)";
						continue; //No change!!!				
					}
				
					string sTileImgPath = "A:\\Shared\\ShellContent";
					store.CreateFolderPath( sTileImgPath );
					sTileImgPath += "\\" + sIcoGd + ".jpg";
					store.DeleteFile( sTileImgPath );
					RenderText( sInfo, sTileImgPath, brBk, brFore, dFontSize );
					
					string sImgUriFinal = "isostore:/" + sTileImgPath.Substring( 3 ).Replace('\\', '/');
					updatedData.BackgroundImage = new Uri(sImgUriFinal, UriKind.Absolute);
					
					tile.Update(updatedData);
					
					sDbg += " " + sIcoId + "|" + sIcoGd + " (Updated)";
					
					store.WriteTextFile( sStCntFldr + "\\" + sIcoGd + "(full).txt", sInfo, true );
					
					//
					// //
				}
				catch( Exception e )
				{
					sDbg += "\r\nERROR: " + e.Message + "\r\n" + e.StackTrace;
				}
            }
			
			RscStore.AddSysEvent( sDbg, false, "ShellTiles_DoUpdate_List" );
			
		}
		
		private void RenderText(string info, string imagepath, Brush brBk, Brush brFore, double fontsize)
		{
			//Medium size Tile 336x336 px
			//Crete image for BackBackgroundImage in IsoStore
			if (info.Length >= 135)
			{
				RenderText(info.Substring(0, 135) + "...", 336, 336, fontsize /*40 /30*/, imagepath, brBk, brFore);
			}
			else
			{
				RenderText(info, 336, 336, fontsize /*38 /28*/, imagepath, brBk, brFore);
			}
		}
		private void RenderText(string text, int width, int height, double fontsize, string imagepath, Brush backColor, Brush brFore)
		{
			WriteableBitmap b = new WriteableBitmap(width, height);
		
			var canvas = new Grid();
			canvas.Width = b.PixelWidth;
			canvas.Height = b.PixelHeight;
		
			var background = new Canvas();
			background.Height = b.PixelHeight;
			background.Width = b.PixelWidth;
		
			//SolidColorBrush backColor = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
			//SolidColorBrush backColor = new SolidColorBrush( Colors.Black );
			background.Background = backColor;
		
			var textBlock = new TextBlock();
			textBlock.Text = text;
			textBlock.FontWeight = FontWeights.Bold;
			textBlock.TextAlignment = TextAlignment.Left;
			textBlock.HorizontalAlignment = HorizontalAlignment.Center;
			textBlock.VerticalAlignment = VerticalAlignment.Stretch;
			textBlock.Margin = new Thickness(35);
			textBlock.Width = b.PixelWidth - textBlock.Margin.Left * 2;
			textBlock.TextWrapping = TextWrapping.Wrap;
			textBlock.Foreground = brFore; //new SolidColorBrush(Colors.White); //color of the text on the Tile
			textBlock.FontSize = fontsize;
		
			canvas.Children.Add(textBlock);
		
			b.Render(background, null);
			b.Render(canvas, null);
			b.Invalidate(); //Draw bitmap
		
			RscStore store = new RscStore();
			
			System.IO.Stream stream = store.CreateFile( imagepath );
					b.SaveJpeg(stream, b.PixelWidth, b.PixelHeight, 0, 100);
			
			stream.Close();
		}
		
		public static bool HasShellTask( string sTaskName )
		{
			PeriodicTask t = ScheduledActionService.Find(sTaskName) as PeriodicTask;
			return (t != null);
		}
		
		public static void InitShellTask( string sTaskName, bool bExitIfNone = true )
		{
            try
            {
				PeriodicTask tOld = ScheduledActionService.Find(sTaskName) as PeriodicTask;
				
				if( tOld != null )
				{
					ScheduledActionService.Remove(tOld.Name);
				}
				else
				{
					if( bExitIfNone ) return;
				}
				
				PeriodicTask t = new PeriodicTask(sTaskName);
				
				t.Description = "Tile updater.";
				
            	//t.ExpirationTime = DateTime.Now.AddDays(10);
				
				ScheduledActionService.Add(t);
			
				/*
					ScheduledActionService.LaunchForTest(t.Name, TimeSpan.FromSeconds(5));
				*/
            }
            catch (InvalidOperationException exception)
            {
                MessageBox.Show(exception.Message);
            }
            catch (SchedulerServiceException schedulerException)
            {
                MessageBox.Show(schedulerException.Message);
            }
		}
		
		public static void CleanUpShellTileData( )
		{
			
			string sStFldr = RscKnownFolders.GetTempPath("ShellTiles", "" );
			string sStCntFldr = RscKnownFolders.GetTempPath("ShellTiles", "Content" );
			string sScFldr = "A:\\Shared\\ShellContent";
			
			RscStore store = new RscStore();
			
			string[] fles = store.GetFileNames(sStFldr, "*.txt");
			
			foreach( string fle in fles )
			{
				
				string sGd = RscStore.FileNameOfPath( fle );
				
            	ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("IcoGd=" + sGd));
				if( TileToFind != null )
					continue;
				
				store.DeleteFile( sStFldr + "\\" + sGd + ".txt" );
				store.DeleteFile( sStCntFldr + "\\" + sGd + ".txt" );
				
				if( store.FolderExists( sScFldr ) )
				{
					store.DeleteFile( sScFldr + "\\" + sGd + ".jpg" );
					store.DeleteFile( sScFldr + "\\" + sGd + ".png" );
				}				
			}
		}
		
	}
	
}