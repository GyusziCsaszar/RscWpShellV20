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

namespace Ressive.Utils
{
	
	public static class RscRegFs
	{
		
		public static void RegisterFileGroup( string sViewerAppPageName, string sViewerAppAssyName, bool bViewerAppAllowList, bool bViewerAppSendContent, string sGroup, string sExtList )
		{
			if( sGroup.Length == 0 ) return;
			
			string [] asExts = sExtList.Split(';');
			foreach( string sExt in asExts )
			{
				if( sExt.Length == 0 ) continue;
				
				string sExtNorm = sExt.ToLower();
				int iPos = sExtNorm.LastIndexOf('.');
				if( iPos >= 0 )
					sExtNorm = sExtNorm.Substring( iPos );
				else
					sExtNorm = "." + sExtNorm;
				
				RscRegistry.WriteString( HKEY.HKEY_CLASSES_ROOT, "()" + sExtNorm, "Group", sGroup );
			}
			
			RscRegistry.WriteString( HKEY.HKEY_CLASSES_ROOT, "Groups\\" + sGroup, "ViewerAppPageName", sViewerAppPageName );
			RscRegistry.WriteString( HKEY.HKEY_CLASSES_ROOT, "Groups\\" + sGroup, "ViewerAppAssyName", sViewerAppAssyName );
			RscRegistry.WriteBool( HKEY.HKEY_CLASSES_ROOT, "Groups\\" + sGroup, "ViewerAppAllowList", bViewerAppAllowList );
			RscRegistry.WriteBool( HKEY.HKEY_CLASSES_ROOT, "Groups\\" + sGroup, "ViewerAppSendContent", bViewerAppSendContent );
		}
		
		public static string GetFileGroupEx( string sExtension )
		{
			if( sExtension.Length == 0 ) return "";
			if( sExtension[ 0 ] != '.' ) sExtension = "." + sExtension;
			
			return RscRegistry.ReadString( HKEY.HKEY_CLASSES_ROOT, "()" + sExtension, "Group", "" );
		}
		
		public static string GetViewerAppPageName( string sGroup )
		{			
			return RscRegistry.ReadString( HKEY.HKEY_CLASSES_ROOT, "Groups\\" + sGroup, "ViewerAppPageName", "" );
		}
		
		public static string GetViewerAppAssyName( string sGroup )
		{	
			return RscRegistry.ReadString( HKEY.HKEY_CLASSES_ROOT, "Groups\\" + sGroup, "ViewerAppAssyName", "" );
		}
		
		public static bool GetViewerAppAllowList( string sGroup )
		{
			return RscRegistry.ReadBool( HKEY.HKEY_CLASSES_ROOT, "Groups\\" + sGroup, "ViewerAppAllowList", false );
		}
		
		public static bool GetViewerAppSendContent( string sGroup )
		{
			return RscRegistry.ReadBool( HKEY.HKEY_CLASSES_ROOT, "Groups\\" + sGroup, "ViewerAppSendContent", false );
		}
		
	}
	
}