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
	
	public class RscWildCard
	{
		
		protected string m_sWildCard_lo;
		
		protected string m_sWcFle_lo = "";
		protected string m_sWcExt_lo = "";
		
		public RscWildCard( string sWildCard )
		{
			m_sWildCard_lo = sWildCard.Trim().ToLower();
			if( m_sWildCard_lo.Length == 0 ) return;
			
			int iPos = m_sWildCard_lo.LastIndexOf( '.' );
			if( iPos < 0 )
			{
				m_sWcFle_lo = m_sWildCard_lo;
			}
			else
			{
				m_sWcFle_lo = m_sWildCard_lo.Substring( 0, iPos );
				m_sWcExt_lo = m_sWildCard_lo.Substring( iPos + 1);
			}
			
			if( m_sWcFle_lo == "*" ) m_sWcFle_lo = "";
			if( m_sWcExt_lo == "*" ) m_sWcExt_lo = "";
		}
		
		public bool Wanted( string sFileName )
		{
			if( m_sWildCard_lo.Length == 0 ) return true; //All files...
			
			string sFileName_lo = sFileName.Trim().ToLower();
			if( sFileName_lo.Length == 0 ) return false; //Error...
			
			string sFle_lo = "";
			string sExt_lo = "";
			
			int iPos = sFileName_lo.LastIndexOf( '.' );
			if( iPos < 0 )
			{
				sFle_lo = sFileName_lo;
			}
			else
			{
				sFle_lo = sFileName_lo.Substring( 0, iPos );
				sExt_lo = sFileName_lo.Substring( iPos + 1);
			}
			
			if( !Check( sFle_lo, m_sWcFle_lo ) ) return false;
			if( !Check( sExt_lo, m_sWcExt_lo ) ) return false;
			
			return true;
		}
		
		protected bool Check( string sName, string sWc )
		{
			if( sWc.Length == 0 ) return true;
			if( sName.Length == 0 ) return false; //For example no file ext, but has wc ext...
			
			if( sName == sWc ) return true; //Can happen... ...filtering one file...
			
			//TODO...
			return false;
		}
		
	}
	
}