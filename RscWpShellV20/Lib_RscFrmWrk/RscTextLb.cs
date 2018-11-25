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
using System.ComponentModel;

using Ressive.Utils;
using Ressive.Theme;

namespace Ressive.FrameWork
{
	
	public class RscTextLbItemList : ObservableCollection<RscTextLbItemBase>, IList<RscTextLbItemBase>, IList
	{
		public int ListBoxAsteriskWidth { set; get; }
		
		protected RscTheme m_Theme = null;
		
		protected ListBox m_lb = null;
		public ListBox ListBox
		{
			get
			{
				return m_lb;
			}
		}
		
		public RscTextLbItemList( ListBox lb, RscTheme Theme )
		{
			m_Theme = Theme;
			
			m_lb = lb;
			
			ListBoxAsteriskWidth = 100;
			m_lb.SizeChanged += new System.Windows.SizeChangedEventHandler(m_lb_SizeChanged);
			
			m_lb.ItemsSource = this;
		}
		
		private void m_lb_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			int iListBoxAsteriskWidth = (int) (e.NewSize.Width);
			//ATT!!! - Otherwise slowdown...
			if( ListBoxAsteriskWidth != iListBoxAsteriskWidth )
			{
				ListBoxAsteriskWidth = iListBoxAsteriskWidth;
				
				if( Count > 0 )
				{
					//This is a slow, full refresh!!!
					//But it's ok, called rarely here...
					m_lb.ItemsSource = null;
					m_lb.ItemsSource = this;
				}
			}
		}
		
		public RscTheme Theme { get{ return m_Theme; } }
		
		public string Text
		{
			set
			{
				Clear();
				
				string [] asLines = value.Split( '\n' );
				foreach( string sLine in asLines )
				{
					RscTextLbItemBase it = new RscTextLbItemBase( this );
					
					it.Text = sLine;
					
					Add( it );
				}
			}
			get
			{
				StringBuilder sb = new StringBuilder();
				
				bool bFirst = true;
				bool bCRLF = false;
				foreach( RscTextLbItemBase it in this )
				{
					if( !bFirst )
					{
						if( bCRLF )
							sb.Append( "\r\n" );
						else
							sb.Append( '\n' );
					}
					
					sb.Append( it.Text );
					
					bCRLF = it.CRLF;
					
					bFirst = false;
				}
				
				return sb.ToString();
			}
		}
		
		double m_dFs = 18;
		public double FontSize
		{
			set
			{
				m_dFs = value;
			}
			get
			{
				return m_dFs;
			}
		}
		
	}
	
	public class RscTextLbItemBase : INotifyPropertyChanged
	{
		//FIX: To auto-update bindings...
		//SRC: http://stackoverflow.com/questions/26806762/c-sharp-listbox-update-binding-text
		public event PropertyChangedEventHandler PropertyChanged;
		protected void RaisePropertyChanged(string propName)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propName));
		}
		//MORE: Automatic version of the same (called by set property automatically, no explicit call needed.
		//SRC: https://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged%28v=vs.110%29.aspx
		/*
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
		*/
		
		// //
		//
		
		protected RscTextLbItemList m_Holder = null;
		
		public Brush BackBrush
		{
			get
			{
				return Holder.ListBox.Background;
				//return new SolidColorBrush(GetBackColor());
			}
		}
		
		public Brush ForeBrush
		{
			get
			{
				return Holder.ListBox.Foreground;
				//return new SolidColorBrush(GetForeColor());
			}
		}
		
		bool m_bCr = false;
		public bool CRLF { get{ return m_bCr; } }
		
		string m_sText = "";
		public string Text
		{
			set
			{
				m_bCr = false;
				if( value.Length > 0 )
				{
					if( value[ value.Length - 1 ] == '\r' )
						m_bCr = true;
				}
				
				if( m_bCr )
					m_sText = value.Substring( 0, value.Length - 1 );
				else
					m_sText = value;
			}
			get
			{
				return m_sText;
			}
		}
		
		public double FontSize
		{
			get
			{
				return Holder.FontSize;
			}
		}
		
		public RscTextLbItemBase This { get{ return this; } }
		public RscTextLbItemList Holder { get{ return m_Holder; } }
		public int ListBoxAsteriskWidth { get{ return Holder.ListBoxAsteriskWidth; } }
		
		public RscTextLbItemBase( RscTextLbItemList oHolder )
		{
			m_Holder = oHolder;
		}
		
		/*
		protected bool m_bCustomBackColor = false;
		protected Color m_clrCustomBackColor;
		public Color CustomBackColor
		{
			set
			{
				m_bCustomBackColor = true;
				m_clrCustomBackColor = value;
				
				RaisePropertyChanged( "BackBrush" );
			}
		}
		public void ClearCustomBackColor() { m_bCustomBackColor = false; RaisePropertyChanged( "BackBrush" ); }
		public virtual Color GetBackColor()
		{
			if( m_bCustomBackColor ) return m_clrCustomBackColor;
			
			return Holder.Theme.ThemeColors.TextLightBack;
		}
		
		protected bool m_bCustomForeColor = false;
		protected Color m_clrCustomForeColor;
		public Color CustomForeColor
		{
			set
			{
				m_bCustomForeColor = true;
				m_clrCustomForeColor = value;
				
				RaisePropertyChanged( "ForeBrush" );
			}
		}
		public void ClearCustomForeColor() { m_bCustomForeColor = false; RaisePropertyChanged( "ForeBrush" ); }
		public virtual Color GetForeColor()
		{
			if( m_bCustomForeColor ) return m_clrCustomForeColor;
			
			return Holder.Theme.ThemeColors.TextLightFore;
		}
		*/
		
	}
	
}