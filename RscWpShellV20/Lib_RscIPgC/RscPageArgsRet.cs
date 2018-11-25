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

namespace Ressive.InterPageCommunication
{
	
	public class RscPageArgsRet
	{
		
		RscPageArgsRetManager Manager;
		
		public string CallerAppTitle = "";
		public string CallerAppIconRes = "";
		public string ID = "";
		
		List<string> m_aFlags = new List<string>();
		List<string> m_aDatas = new List<string>();
		
		public RscPageArgsRet(RscPageArgsRetManager mgr)
		{
			Manager = mgr;
		}
		
		public RscPageArgsRet(RscPageArgsRetManager mgr, string strCallerAppTitle, string strCallerAppIconRes, string strID)
		{
			Manager = mgr;
			CallerAppTitle = strCallerAppTitle;
			CallerAppIconRes = strCallerAppIconRes;
			ID = strID;
		}
		
		public void SetFlag( int iIdx, string strFlag )
		{
			while( m_aFlags.Count <= iIdx ) m_aFlags.Add("");
			m_aFlags[iIdx] = strFlag;
		}
		
		public string GetFlag( int iIdx )
		{
			if( iIdx >= m_aFlags.Count ) return "";
			return m_aFlags[iIdx];
		}
		
		public void SetData( int iIdx, string strData )
		{
			while( m_aDatas.Count <= iIdx ) m_aDatas.Add("");
			m_aDatas[iIdx] = strData;
		}
		
		public string GetData( int iIdx )
		{
			if( iIdx >= m_aDatas.Count ) return "";
			return m_aDatas[iIdx];
		}
		
		public int DataCount
		{
			get{ return m_aDatas.Count; }
		}
		
		public RscPageArgsRet CreateOutPut()
		{
			RscPageArgsRet OutPut = new RscPageArgsRet( Manager, CallerAppTitle, CallerAppIconRes, ID );
			return OutPut;
		}
		
		public void SetOutput()
		{
			Manager.SetOutput( this );
		}
		
		public void SetInput( string strPageName )
		{
			Manager.SetInput( this, strPageName );
		}
		
		public void FromString( string str )
		{
			CallerAppTitle = "";
			ID = "";
			m_aFlags.Clear();
			m_aDatas.Clear();
			
			string [] astrDelims = new string [1];
			astrDelims[ 0 ] = "\r\n";
			
			string [] astr = str.Split(astrDelims, StringSplitOptions.None);
			string strFlags = "";
			
			if( astr.Length <  1 ) return; CallerAppTitle = astr[0];
			if( astr.Length <  2 ) return; CallerAppIconRes = astr[1];
			if( astr.Length <  3 ) return; ID = astr[2];
			if( astr.Length <  4 ) return; strFlags = astr[3];
			
			string [] astrFlags = strFlags.Split(';');
			for( int i = 0; i < astrFlags.Length; i++ )
				m_aFlags.Add( astrFlags[ i ] );
			
			for( int i = 4; i < astr.Length; i++ )
				m_aDatas.Add( astr[ i ] );
		}
		
		public override string ToString()
		{
			string str = "";
			
			str += CallerAppTitle + "\r\n";
			str += CallerAppIconRes + "\r\n";
			str += ID + "\r\n";
			
			for( int i = 0; i < m_aFlags.Count; i++ )
			{
				str += m_aFlags[ i ];
				if( i < m_aFlags.Count - 1 ) str += ";";
			}
			str += "\r\n";
			
			for( int i = 0; i < m_aDatas.Count; i++ )
			{
				str += m_aDatas[ i ];
				if( i < m_aDatas.Count - 1 ) str += "\r\n";
			}
			
			return str;
		}
		
		public Uri GetNavigateUri( string sAssemblyName )
		{
			string sUri = "/" + sAssemblyName + ";component/"
				+ Manager.PageName + ".xaml";
			
			return new Uri(sUri, UriKind.Relative);
		}
		
	}
	
	public class RscPageArgsRetManager
	{
		
		string m_strPageName;
		
		public RscPageArgsRetManager()
		{
			m_strPageName = "";
		}
		
		public bool Waiting
		{
			get
			{
				return m_strPageName != "";
			}
		}
		
		public void SetInput( RscPageArgsRet InPut, string strPageName )
		{
			m_strPageName = strPageName;
			string strPath = RscKnownFolders.GetTempPath("PageArgsRet") + "\\" + m_strPageName + "_IN" + ".txt";
			
			RscStore store = new RscStore();
			store.WriteTextFile( strPath, InPut.ToString(), true );
		}
		
		public string PageName
		{
			get
			{
				return m_strPageName;
			}
		}
		
		public RscPageArgsRet GetInput( string strPageName )
		{
			m_strPageName = strPageName;
			string strPath = RscKnownFolders.GetTempPath("PageArgsRet") + "\\" + m_strPageName + "_IN" + ".txt";
			bool bNotExist = false;
			
			RscStore store = new RscStore();
			string strInput = store.ReadTextFile(strPath, "", out bNotExist);
			
			if( bNotExist ) return null;
			RscPageArgsRet InPut = new RscPageArgsRet(this);
			InPut.FromString( strInput );
			return InPut;
		}
		
		public void SetOutput( RscPageArgsRet OutPut )
		{
			string strPath = RscKnownFolders.GetTempPath("PageArgsRet") + "\\" + m_strPageName + "_OUT" + ".txt";
			
			RscStore store = new RscStore();
			store.WriteTextFile( strPath, OutPut.ToString(), true );
		}
		
		public RscPageArgsRet GetOutput( )
		{
			string strPath = RscKnownFolders.GetTempPath("PageArgsRet") + "\\" + m_strPageName + "_OUT" + ".txt";
			bool bNotExist = false;
			
			RscStore store = new RscStore();
			string strOutput = store.ReadTextFile(strPath, "", out bNotExist);
			
			RscPageArgsRet OutPut = new RscPageArgsRet(this);
			OutPut.FromString( strOutput );
			
			return OutPut;
		}
		
		public void Vipe()
		{
			RscStore store = new RscStore();
			store.DeleteFile( RscKnownFolders.GetTempPath("PageArgsRet") + "\\" + m_strPageName + "_IN" + ".txt" );
			store.DeleteFile( RscKnownFolders.GetTempPath("PageArgsRet") + "\\" + m_strPageName + "_OUT" + ".txt" );
		}
		
		public void Clear()
		{
			m_strPageName = "";
		}
		
	}
	
}