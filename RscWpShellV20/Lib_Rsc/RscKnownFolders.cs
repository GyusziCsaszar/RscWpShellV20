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
	
	public static class RscKnownFolders
	{
		
		public static string GetTempPath( string strFolder, string strSubFolder = "" )
		{
			string strPath = "";
			
			RscStore store = new RscStore();
			
			strPath = strPath + "A:\\TEMP";
			if( !store.FolderExists(strPath) )
			{
				store.CreateFolder(strPath);
			}
			strPath = strPath + "\\";
			
			strPath = strPath + strFolder;
			if( !store.FolderExists(strPath) )
			{
				store.CreateFolder(strPath);
			}
			//strPath = strPath + "\\";
			
			if( strSubFolder.Length > 0 )
			{
				strPath = strPath + "\\";
				
				strPath = strPath + strSubFolder;
				if( !store.FolderExists(strPath) )
				{
					store.CreateFolder(strPath);
				}
				//strPath = strPath + "\\";
			}
			
			return strPath;
		}

		public static string GetMediaPath( string strSysFolder, string strSysSubFolder = "", bool bCreate = true )
		{
			string strPath = "";
			
			RscStore store = new RscStore();
			
			strPath = strPath + "A:\\Media";
			if( !store.FolderExists(strPath) )
			{
				if( bCreate ) store.CreateFolder(strPath);
			}
			strPath = strPath + "\\";
			
			strPath = strPath + strSysFolder;
			if( !store.FolderExists(strPath) )
			{
				if( bCreate ) store.CreateFolder(strPath);
			}
			//strPath = strPath + "\\";
			
			if( strSysSubFolder.Length > 0 )
			{
				strPath = strPath + "\\";
				
				strPath = strPath + strSysSubFolder;
				if( !store.FolderExists(strPath) )
				{
					if( bCreate ) store.CreateFolder(strPath);
				}
				//strPath = strPath + "\\";
			}
			
			return strPath;
		}

		public static string GetSystemPath( string strSysFolder, string strSysSubFolder = "" )
		{
			string strPath = "";
			
			RscStore store = new RscStore();
			
			strPath = strPath + "A:\\System";
			if( !store.FolderExists(strPath) )
			{
				store.CreateFolder(strPath);
			}
			strPath = strPath + "\\";
			
			strPath = strPath + strSysFolder;
			if( !store.FolderExists(strPath) )
			{
				store.CreateFolder(strPath);
			}
			//strPath = strPath + "\\";
			
			if( strSysSubFolder.Length > 0 )
			{
				strPath = strPath + "\\";
				
				strPath = strPath + strSysSubFolder;
				if( !store.FolderExists(strPath) )
				{
					store.CreateFolder(strPath);
				}
				//strPath = strPath + "\\";
			}
			
			return strPath;
		}
		
	}
	
}