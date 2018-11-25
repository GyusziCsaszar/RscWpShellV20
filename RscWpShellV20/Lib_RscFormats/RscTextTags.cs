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

namespace Ressive.Formats
{

	
	public class RscTextTags
	{
		
		protected List<RscTextTag> m_aTags = new List<RscTextTag>();
		
		protected int LastErrorTagIndex = -1;
		protected string LastError = "";
		
		public RscTextTags()
		{
			LastErrorTagIndex = -1;
			LastError = "";
		}
		
		public override string ToString()
		{
			string sRes = "";
			
			int iStart = 0;
			if( LastError.Length > 0 )
			{
				iStart = Math.Max( 0, Math.Min( m_aTags.Count - 1, LastErrorTagIndex ) );
				sRes = "ERROR: " + LastError;
			}
			
			RscTextTag tt;
			int iCount = m_aTags.Count;
			for( int i = iStart; i < iCount; i++ )
			{
				tt = m_aTags[ i ];
				
				if( sRes.Length > 0 ) sRes += "\r\n";
				
				sRes += "(" + i.ToString() + ") ";
				sRes += tt.LeftSide;
				sRes += " --> ";
				sRes += tt.RightSide;
			}
			
			return sRes;
		}
		
		public virtual void Parse( string sTxt, string sTagDelimiter, string sTagDataDelimiter, string sTagSubDataDelimiter )
		{
			
			LastErrorTagIndex = -1;
			LastError = "";
			
			m_aTags.Clear();
			
			string [] asTagDelim = { sTagDelimiter };
			string [] asTags = sTxt.Split( asTagDelim, StringSplitOptions.RemoveEmptyEntries );
			
			foreach( string sTag in asTags )
			{
				
				string [] asTagDataDelim = { sTagDataDelimiter };
				string [] asTagDatas = sTag.Split( asTagDataDelim, StringSplitOptions.RemoveEmptyEntries );
				
				RscTextTag tt = new RscTextTag();
				m_aTags.Add( tt );
				
				string [] asTagSubDataDelim = { sTagSubDataDelimiter };
				tt.m_asLeft = asTagDatas[ 0 ].Split( asTagSubDataDelim, StringSplitOptions.RemoveEmptyEntries );
				
				if( asTagDatas.Length > 1 )
				{
					tt.m_asRight = asTagDatas[ 1 ].Split( asTagSubDataDelim, StringSplitOptions.RemoveEmptyEntries );
				}
				
			}
			
		}
		
	}
	
	public class RscTextTag
	{
		
		public string [] m_asLeft = null;
		public string [] m_asRight = null;
		
		public RscTextTag()
		{
		}
		
		public string ID
		{
			get
			{
				if( m_asLeft == null ) return "<NONE>";
				if( m_asLeft.Length == 0 ) return "<EMPTY>";
				return m_asLeft[0].ToUpper();
			}
		}
		
		public string LeftSide
		{
			get
			{
				string s = "";
				if( m_asLeft != null )
				{
					foreach( string si in m_asLeft )
					{
						if( s.Length > 0 ) s += "|";
						s += si;
					}
				}
				return s;
			}
		}
		
		public string RightSide
		{
			get
			{
				string s = "";
				if( m_asRight != null )
				{
					foreach( string si in m_asRight )
					{
						if( s.Length > 0 ) s += "|";
						s += si;
					}
				}
				return s;
			}
		}
		
		public bool ChkAgains( string sData, string sValue )
		{
			if( m_asLeft == null ) return false;
			if( m_asRight == null ) return false;
			
			if( m_asLeft.Length != 1 ) return false;
			if( m_asRight.Length != 1 ) return false;
			
			if( sData.ToUpper().CompareTo( LeftSide.ToUpper() ) != 0 ) return false;
			if( sValue.ToUpper().CompareTo( RightSide.ToUpper() ) != 0 ) return false;
			
			return true;
		}
		
		public int LeftIndexOf( string str )
		{
			if( m_asLeft == null ) return -1;
			int iIdx = -1;
			foreach( string s in m_asLeft )
			{
				iIdx++;
				if( s.ToUpper().CompareTo( str.ToUpper() ) == 0 )
					return iIdx;
			}
			return -1;
		}
		
		public string LeftSideSub( int iIndex )
		{
			if( m_asLeft == null ) return "";
			if( iIndex >= m_asLeft.Length ) return "";
			return m_asLeft[ iIndex ];
		}
		
	}
	
}