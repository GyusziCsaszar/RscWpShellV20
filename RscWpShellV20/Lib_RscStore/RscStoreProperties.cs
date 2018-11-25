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

namespace Ressive.Store
{
	
	public class RscStoreProperties
	{
		
		private bool m_bDisplayName = false;
		private string m_sDisplayName = "";
		
		private bool m_bFullPath = false;
		private string m_sFullPath = "";
		
		private bool m_bCre = false;
		private DateTime m_dtCre = new DateTime();
		
		private bool m_bWrt = false;
		private DateTime m_dtWrt = new DateTime();
		
		private bool m_bAcc = false;
		private DateTime m_dtAcc = new DateTime();
		
		private bool m_bLen = false;
		private long m_lLen = -1;
		
		public RscStoreProperties()
		{
		}
		
		public bool HasDisplayName
		{
			get{ return m_bDisplayName; }
		}
		public string DisplayName
		{
			set{ m_bDisplayName = true; m_sDisplayName = value; }
			get{ return m_sDisplayName; }
		}
		
		public bool HasFullPath
		{
			get{ return m_bFullPath; }
		}
		public string FullPath
		{
			set{ m_bFullPath = true; m_sFullPath = value; }
			get{ return m_sFullPath; }
		}
		
		public bool HasCreationTime
		{
			get{ return m_bCre; }
		}
		public DateTime CreationTime
		{
			set{ m_bCre = true; m_dtCre = value; }
			get{ return m_dtCre; }
		}
		
		public bool HasLastWriteTime
		{
			get{ return m_bWrt; }
		}
		public DateTime LastWriteTime
		{
			set{ m_bWrt = true; m_dtWrt = value; }
			get{ return m_dtWrt; }
		}
		
		public bool HasLastAccessTime
		{
			get{ return m_bAcc; }
		}
		public DateTime LastAccessTime
		{
			set{ m_bAcc = true; m_dtAcc = value; }
			get{ return m_dtAcc; }
		}
		
		public bool HasLength
		{
			get{ return m_bLen; }
		}
		public long Length
		{
			set{ m_bLen = true; m_lLen = value; }
			get{ return m_lLen; }
		}
		
	}
	
}