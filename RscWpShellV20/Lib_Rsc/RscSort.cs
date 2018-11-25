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

using System.Linq;

namespace Ressive.Utils
{
	
	public static class RscSort
	{
		
		public static string[] OrderBy( string[] astrBase, bool bReverseOrder = false )
		{
			
			IEnumerable<string> astrOrdered;

			if( bReverseOrder )
			{
				astrOrdered =
					from str in astrBase
					orderby str descending
					select str;
			}
			else
			{
				astrOrdered =
					from str in astrBase
					orderby str
					select str;
			}
			
			string[] res = new string[ astrBase.Length ];
			
			int iIdx = -1;
			foreach( string str in astrOrdered )
			{
				iIdx++;
				res[ iIdx ] = str;
			}
			
			return res;
		}
		
	}
	
}