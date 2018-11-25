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

namespace Ressive.UtilsEx
{
	
	public class RscStoreItem
	{
		
		protected RscStoreItem m_oParent;
		
		protected string m_sName;
		
		protected string m_sFileName;
		protected string m_sFileExt;
		
		protected string m_sFileGroup;
		
		protected bool m_bFolder;
		
		List<RscStoreItem> m_ao = null;
		
		public RscStoreItem(string sFileName, string sFileExt, string sFileGroup)
		{
			m_oParent = null;
			
			m_sName = sFileName + sFileExt;
			
			m_sFileName = sFileName;
			m_sFileExt = sFileExt;
			
			m_sFileGroup = sFileGroup;
			
			m_bFolder = false;
			
			m_ao = null;
		}
		
		public RscStoreItem Parent
		{
			get{ return m_oParent; }
		}
		
		public string Name
		{
			get{ return m_sName; }
		}
		
		public string FileName
		{
			get{ return m_sFileName; }
		}
		
		public string FileExt
		{
			get{ return m_sFileExt; }
		}
		
		public bool Folder
		{
			get{ return m_bFolder; }
		}
		
		public int Count
		{
			get
			{
				if( m_ao == null ) return 0;
				return m_ao.Count;
			}
		}
		
		public void AddItem( RscStoreItem oItem )
		{
			if( m_ao == null )
				m_ao = new List<RscStoreItem>();
			
			oItem.m_oParent = this;
			m_ao.Add( oItem );
		}
		
		public void Clear()
		{
			if( m_ao == null ) return;
			m_ao.Clear();
			m_ao = null;
		}
		
		public RscStoreItem Item( int iIndex )
		{
			if( iIndex < 0 ) return null;
			if( iIndex >= Count ) return null;
			return m_ao[ iIndex ];
		}
		
		public virtual void Parse(string sFileGroupPre, bool bListFoldersToo, bool bReverseOrder)
		{
			//NOP...
		}
		
		public string FullPath
		{
			get{ return GetFullPath(); }
		}
		
		public string GetFullPath(bool bDecorated = false)
		{
			string s = "";
			
			if( m_oParent != null )
			{
				s += m_oParent.GetFullPath( bDecorated );
			}
			else
			{
				//if( bDecorated ) s += m_sSystem + ":";
			}
			
			if( s.Length > 0 ) s += "\\";
			s += m_sName;
			
			return s;
		}
		
	}
	
	public class RscStoreItemFile : RscStoreItem
	{
		
		public RscStoreItemFile(string sFileName, string sFileExt, string sFileGroup)
			: base( sFileName, sFileExt, sFileGroup )
		{
			m_bFolder = false;
		}
		
	}
	
	public class RscStoreItemFolder : RscStoreItem
	{
		
		public RscStoreItemFolder(string sName)
			: base( sName, "", "" )
		{
			m_bFolder = true;
		}
		
		public override void Parse(string sFileGroupPre, bool bListFoldersToo, bool bReverseOrder)
		{
			Clear();
			
			RscStore store = new RscStore();
			
			if( bListFoldersToo )
			{
				string[] fldrsAll = RscSort.OrderBy(store.GetFolderNames( FullPath, "*.*" ), bReverseOrder);
				RscStoreItemFolder oFolder;
				foreach( string fldr in fldrsAll )
				{
					oFolder = new RscStoreItemFolder( fldr );
					AddItem( oFolder );
				}
			}
			
			string[] flesAll = RscSort.OrderBy(store.GetFileNames( FullPath, "*.*" ), bReverseOrder);
			
			int iPos;
			string sExt;
			string sFileGroup;
			RscStoreItemFile oFile;
			
			foreach( string fle in flesAll )
			{
				iPos = fle.LastIndexOf('.');
				if( iPos >= 0 )
				{
					sExt = fle.Substring( iPos );
					sFileGroup = RscRegFs.GetFileGroupEx( sExt );
					if( sFileGroupPre.Length == 0 || sFileGroup.IndexOf( sFileGroupPre ) == 0 )
					{
						oFile = new RscStoreItemFile( fle.Substring(0, iPos), sExt, sFileGroup );
						AddItem( oFile );
					}
				}
			}
			
		}
		
	}
	
}