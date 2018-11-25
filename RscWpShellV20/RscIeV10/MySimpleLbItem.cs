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

namespace RscIeV10
{
	public class MySimpleLbItem : RscSimpleLbItemBase
	{
		
		public MySimpleLbItem( RscSimpleLbItemList oHolder )
		: base( oHolder )
		{
			CustomBackColor = Holder.Theme.ThemeColors.TreeLeafBack; //Colors.Gray;
			CustomForeColor = Holder.Theme.ThemeColors.TreeLeafFore; //Colors.White;
		}
		
		public object oTag = null;
		public string IcoInfo = "";
		
		string m_sTitle = "";
		public override string Title
		{
			set
			{
				m_sTitle = value;
			}
			get
			{
				return m_sTitle;
			}
		}
				
		string m_sDesc1 = "";
		public override Visibility Desc1Vis
		{
			set
			{
			}
			get
			{
				if( m_sDesc1.Length == 0 )
					return Rsc.Collapsed;
				
				return Rsc.Visible;
			}
		}
		public override string Desc1
		{
			set
			{
				m_sDesc1 = value;
			}
			get
			{
				return m_sDesc1;
			}
		}
		
		string m_sDesc2 = "";
		public override string Desc2
		{
			set
			{
				m_sDesc2 = value;
			}
			get
			{
				return m_sDesc2;
			}
		}
		
	}
}