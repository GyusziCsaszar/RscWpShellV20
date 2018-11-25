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
using Ressive.FrameWork;

using Ressive.FTP;

namespace RscFtpClients
{
	
	public class RscFtp_UpLoad_SimpleLbItem : RscSimpleLbItemBase
	{
		
		public bool m_bLogItems = false;
		
		public bool bFile;
		public bool bFolder;
		
		bool m_bWalked = false; //Folder is walked...
		public bool bWalked
		{
			get{ return m_bWalked; }
			set{ m_bWalked = value; RaisePropertyChanged( "Desc1" ); }
		}
		
		public string strOwner;
		public string strName;
		
		public RscFtp_UpLoad_SimpleLbItem itOwner = null;
		public RscFtpServerData ServerData = null;
		
		long m_lFileSize = -1;
		public long FileSize
		{
			get{ return m_lFileSize; }
			set{ m_lFileSize = value; RaisePropertyChanged( "Desc1" ); }
		}
		
		long m_lRemoteFileSize = -1;
		public long RemoteFileSize
		{
			get{ return m_lRemoteFileSize; }
			set{ m_lRemoteFileSize = value; RaisePropertyChanged( "Desc1" ); }
		}
		
		public int RefCount = 0;
		
		bool m_bCreated = false;
		public bool Created
		{
			get{ return m_bCreated; }
			set{ m_bCreated = value; RaisePropertyChanged( "Desc1" ); }
		}
		
		bool m_bDone = false;
		public bool Done
		{
			get{ return m_bDone; }
			set{ m_bDone = value; RaisePropertyChanged( "Desc1" ); }
		}
		
		bool m_bAcked = false;
		public bool Acked
		{
			get{ return m_bAcked; }
			set{ m_bAcked = value; RaisePropertyChanged( "Desc1" ); }
		}
		
		public int ChkBackFailCount = 0;
		
		public RscFtp_UpLoad_SimpleLbItem( RscSimpleLbItemList oHolder )
		: base( oHolder )
		{
			CustomBackColor = Holder.Theme.ThemeColors.TreeLeafBack; //Colors.Gray;
			CustomForeColor = Holder.Theme.ThemeColors.TreeLeafFore; //Colors.White;
		}
		
		public override Color GetBackColor()
		{
			if( !bFile && !bFolder ) return m_clrCustomBackColor;
			
			if( !bFolder ) return Holder.Theme.ThemeColors.TreeLeafBack; //Colors.Blue;
			return Holder.Theme.ThemeColors.TreeContainerBack; //Colors.Orange;
		}
		
		public override Color GetForeColor()
		{
			if( !bFile && !bFolder ) return m_clrCustomForeColor;
			
			if( !bFolder ) return Holder.Theme.ThemeColors.TreeLeafFore; //Colors.White;
			return Holder.Theme.ThemeColors.TreeContainerFore; //Colors.White;
		}
		
		public override string Title
		{
			get
			{
				if( bFolder && strName.Length == 0 ) return ".";
				return strName;
			}
		}
		
		public override string Desc1
		{
			get
			{
				return GetStateTitle(m_bLogItems);
			}
		}
		
		public override string Desc2
		{
			get
			{
				return GetPath() + Details;
			}
		}
		
		public void AddRef()
		{
			RefCount++;
		}
		public void Release()
		{
			RefCount = Math.Max(0, RefCount - 1);
		}
		
		public string GetPath()
		{
			if( !bFile && !bFolder ) return strOwner;
			
			string strPath;
			
			strPath = strOwner;
			
			if( strName.Length > 0 )
			{
				if( strOwner.Length > 0 ) strPath += "\\";
				
				strPath += strName;
			}
			
			return strPath;
		}
		
		public string GetStateTitle(bool bDetailed)
		{
			string strSt = "";
			
			if( !bFile && !bFolder ) return "message";
			if( !bFolder )
			{
				strSt = "file";
				if( FileSize > -1 )
				{
					strSt += " (local: ";
					strSt += FileSize.ToString();
					strSt += " bytes, " + RscUtils.toMBstr( FileSize, false ) + ")";
				}
				if( RemoteFileSize > -1 )
				{
					strSt += " (remote: ";
					strSt += RemoteFileSize.ToString();
					strSt += " bytes, " + RscUtils.toMBstr( RemoteFileSize, false ) + ")";
				}
				if( bDetailed )
				{
					if( Done ) strSt += " (DONE)";
					if( Acked ) strSt += " (ACKED)";
				}
				return strSt;
			}
			
			strSt = "folder";
			if( bDetailed )
			{
				strSt += " (RefCount=" + RefCount.ToString() + ")";
				if( !bWalked ) strSt += " (NOT WALKED!!!)";
				if( Created ) strSt += " (Created)";
				if( Done ) strSt += " (DONE)";
				if( Acked ) strSt += " (ACKED)";
			}
			return strSt;
		}
		
		string m_sDetails = "";
		public string Details
		{
			set
			{
				m_sDetails = value;
				
				RaisePropertyChanged( "Desc2" );
			}
			get
			{
				return m_sDetails;
			}
		}
		
	}
	
}