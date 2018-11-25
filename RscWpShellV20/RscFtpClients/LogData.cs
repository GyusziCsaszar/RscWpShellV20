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

using System.Collections;
using System.Collections.ObjectModel;

namespace RscFtpClients
{
	
    public class MyLogItemList : ObservableCollection<MyLogItem>, IList<MyLogItem>, IList
    {
		public int ListBoxAsteriskWidth { set; get; }
	}
	
	public class MyLogItem
	{
		
		public bool bFullEmpty = false;
		public bool bSent = false;
		public string sCh = "";
		public string sLog = "";
		public bool bDataItem = false;
		public object oTag = null;
		
		public MyLogItem This { get{ return this; } }
		public MyLogItemList Parent { set; get; }
		public int ListBoxAsteriskWidth { get{ return Parent.ListBoxAsteriskWidth; } }
		
		public string Text
		{
			get{ return sLog; }
		}
		
		public Brush BkBrush
		{
			get
			{
				if( sCh.Length == 0 ) return new SolidColorBrush(Colors.DarkGray);
				if( bSent ) return new SolidColorBrush(Colors.Orange);
				if( bDataItem ) return new SolidColorBrush(Colors.White);
				return new SolidColorBrush(Colors.Green);
			}
		}
		
		public TextAlignment Align
		{
			get
			{
				if( bSent ) return TextAlignment.Left;
				return TextAlignment.Right;
			}
		}
		
		public string LeftText
		{
			get
			{
				if( bSent ) return "";
				if( sCh.Length == 0 ) return "";
				return " " + sCh;
			}
		}
		
		public string RightText
		{
			get
			{
				if( !bSent ) return "";
				if( sCh.Length == 0 ) return "";
				return sCh + " ";
			}
		}
		
		public int LeftIndent
		{
			get
			{
				if( bSent ) return 0;
				return 50;
			}
		}
		
		public int RightIndent
		{
			get
			{
				if( !bSent ) return 0;
				return 50;
			}
		}
		
		public int FontSizeLg
		{
			get
			{
				if( bFullEmpty ) return 4;
				return 20;
			}
		}
		
		public int FontSize
		{
			get
			{
				if( bFullEmpty ) return 4;
				return 16;
			}
		}
		
	}
	
}