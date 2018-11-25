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
	
	public enum HKEY
	{
		HKEY_CLASSES_ROOT = 0,
		HKEY_CURRENT_USER = 1
	}
	
	public class RscRegistry
	{
		
		public static string[] GetKeys( HKEY hk, string sPath, string sFilter = "" )
		{
			string strFullPath = GetFullPath( hk, sPath );
			
			RscStore store = new RscStore();
			if( !store.FolderExists( strFullPath ) ) return new String[] {};
			
			if( sFilter.Length == 0 ) sFilter = "*.*";
			
			return store.GetFolderNames(strFullPath, sFilter);
		}
		
		public static void DeleteValue( HKEY hk, string sPath, string sName )
		{
			string strFullPath = GetFullPath( hk, sPath );
			
			RscStore store = new RscStore();
			
			//MUST NOT!!!
			//store.CreateFolderPath( strFullPath );
			
			strFullPath += "\\" + sName + ".DWORD";
			
			store.DeleteFile( strFullPath );
		}
		
		public static bool ReadBool( HKEY hk, string sPath, string sName, bool bDefaultValue = false )
		{
			int iDef = 0;
			if( bDefaultValue ) iDef = 1;
			int iVal = ReadDWORD( hk, sPath, sName, iDef );
			if( iVal != 0 ) return true;
			return false;
		}
		
		public static void WriteBool( HKEY hk, string sPath, string sName, bool bValue )
		{
			int iVal = 0;
			if( bValue ) iVal = 1;
			WriteDWORD( hk, sPath, sName, iVal );
		}
		
		public static int ReadDWORD( HKEY hk, string sPath, string sName, int iDefaultValue = 0 )
		{
			string strFullPath = GetFullPath( hk, sPath );
			
			RscStore store = new RscStore();
			
			//MUST NOT!!!
			//store.CreateFolderPath( strFullPath );
			
			strFullPath += "\\" + sName + ".DWORD";
			
			bool bNotExist = false;
			string sValue = store.ReadTextFile( strFullPath, iDefaultValue.ToString(), out bNotExist );
			
			int iValue = iDefaultValue;
			Int32.TryParse( sValue, out iValue );
			
			return iValue;
		}
		
		public static void WriteDWORD( HKEY hk, string sPath, string sName, int iValue )
		{
			string strFullPath = GetFullPath( hk, sPath );
			
			RscStore store = new RscStore();
			
			store.CreateFolderPath( strFullPath );
			
			strFullPath += "\\" + sName + ".DWORD";
			
			store.WriteTextFile( strFullPath, iValue.ToString(), true );
		}
		
		public static string ReadString( HKEY hk, string sPath, string sName, string sDefaultValue = "" )
		{
			string strFullPath = GetFullPath( hk, sPath );
			
			RscStore store = new RscStore();
			
			//MUST NOT!!!
			//store.CreateFolderPath( strFullPath );
			
			strFullPath += "\\" + sName + ".String";
			
			bool bNotExist = false;
			return store.ReadTextFile( strFullPath, sDefaultValue, out bNotExist );
		}
		
		public static void WriteString( HKEY hk, string sPath, string sName, string sValue )
		{
			string strFullPath = GetFullPath( hk, sPath );
			
			RscStore store = new RscStore();
			
			store.CreateFolderPath( strFullPath );
			
			strFullPath += "\\" + sName + ".String";
			
			store.WriteTextFile( strFullPath, sValue, true );
		}
		
		public static string GetFullPath( HKEY hk, string sPath )
		{
			string strFullPath;
			
			strFullPath = RscKnownFolders.GetSystemPath( "Registry" );
			strFullPath += "\\" + HKEY2String( hk );
			
			sPath = RscUtils.RemoveStarting(sPath, "\\");
			sPath = RscUtils.RemoveEnding(sPath, "\\");
			if( sPath.Length > 0 ) strFullPath += "\\" + sPath;
			
			return strFullPath;
		}
		
		public static string HKEY2String( HKEY hk )
		{
			switch( hk )
			{
				
				case HKEY.HKEY_CLASSES_ROOT :
					return "HKEY_CLASSES_ROOT";
				
				case HKEY.HKEY_CURRENT_USER :
					return "HKEY_CURRENT_USER";
					
				default :
					return "";
					
			}
		}
		
	}
	
	public class RscRegistryValueList
	{
		
		HKEY m_hk;
		string m_sPath;
		string m_sTitle;
		
		int m_iCountWritten = 0;
		
        List<string> m_a = new List<string>();
		
		public RscRegistryValueList(HKEY hk, string sPath, string sTitle = "Item")
		{
			m_hk = hk;
			m_sPath = sPath;
			m_sTitle = sTitle;
			
			m_iCountWritten = RscRegistry.ReadDWORD( m_hk, m_sPath, sTitle + "Count", 0 );
			for( int i = 0; i < m_iCountWritten; i++ )
			{
				string sVal = RscRegistry.ReadString( m_hk, m_sPath, sTitle + i.ToString(), "" );
				m_a.Add( sVal );
			}
		}
		
		public int Count
		{
			get{ return m_a.Count; }
		}
		
		public string Get( int iIdx )
		{
			if( iIdx < 0 ) return "";
			if( iIdx >= m_a.Count ) return "";
			return m_a[ iIdx ];
		}
		
		public void Add( string sVal )
		{
			int iIdx = _Find( sVal );
			if( iIdx < 0 )
			{
				m_a.Add( sVal );
			}
			else
			{
				m_a.RemoveAt( iIdx );
				m_a.Insert( iIdx, sVal );
			}
		}
		
		public void Delete( int iIdx )
		{
			if( iIdx < 0 ) return;
			if( iIdx >= m_a.Count ) return;
			m_a.RemoveAt( iIdx );
		}
		
		public void Flush()
		{
			for( int i = 0; i < m_a.Count; i++ )
			{
				RscRegistry.WriteString( m_hk, m_sPath, m_sTitle + i.ToString(), m_a[ i ]);
			}
			RscRegistry.WriteDWORD( m_hk, m_sPath, m_sTitle + "Count", m_a.Count );
			
			for( int i = ((m_a.Count - 1) + 1); i < (m_iCountWritten - 1); i++ )
			{
				RscRegistry.DeleteValue( m_hk, m_sPath, m_sTitle + i.ToString() );
			}
		}
		
		private int _Find( string sVal )
		{
			int iIdx = -1;
			foreach( string s in m_a )
			{
				iIdx++;
				if( s == sVal ) return iIdx;
			}
			return -1;
		}
		
	}
	
}