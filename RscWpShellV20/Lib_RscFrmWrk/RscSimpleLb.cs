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
	
	public class RscSimpleLbItemList : ObservableCollection<RscSimpleLbItemBase>, IList<RscSimpleLbItemBase>, IList
	{
		public int ListBoxAsteriskWidth { set; get; }
		
		protected RscTheme m_Theme = null;
		
		protected ListBox m_lb = null;
		
		public RscSimpleLbItemList( ListBox lb, RscTheme Theme )
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
		
	}
	
	public class RscSimpleLbItemBase : INotifyPropertyChanged
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
		
		protected RscSimpleLbItemList m_Holder = null;
		
		public Brush BackBrush
		{
			get
			{
				return new SolidColorBrush(GetBackColor());
			}
		}
		
		public Brush ForeBrush
		{
			get
			{
				return new SolidColorBrush(GetForeColor());
			}
		}
		
		public Brush DescBackBrush
		{
			get
			{
				return new SolidColorBrush( Holder.Theme.ThemeColors.TreeDescBack );
			}
		}
		
		public Brush DescForeBrush
		{
			get
			{
				return new SolidColorBrush( Holder.Theme.ThemeColors.TreeDescFore );
			}
		}
		
		public virtual string Title
		{
			set
			{
			}
			get
			{
				return "";
			}
		}
		
		ImageSource m_isBtnCust1 = null;
		public ImageSource BtnCust1Img
		{
			set
			{
				m_isBtnCust1 = value;
				
				RaisePropertyChanged( "BtnCust1Img" );
			}
			get
			{
				return m_isBtnCust1;
			}
		}
		
		Visibility m_visBtnCust1 = Rsc.Collapsed;
		public Visibility BtnCust1Vis
		{
			set
			{
				m_visBtnCust1 = value;
				
				RaisePropertyChanged( "BtnCust1Vis" );
			}
			get
			{
				return m_visBtnCust1;
			}
		}
		
		ImageSource m_isBtnCust2 = null;
		public ImageSource BtnCust2Img
		{
			set
			{
				m_isBtnCust2 = value;
				
				RaisePropertyChanged( "BtnCust2Img" );
			}
			get
			{
				return m_isBtnCust2;
			}
		}
		
		Visibility m_visBtnCust2 = Rsc.Collapsed;
		public Visibility BtnCust2Vis
		{
			set
			{
				m_visBtnCust2 = value;
				
				RaisePropertyChanged( "BtnCust2Vis" );
			}
			get
			{
				return m_visBtnCust2;
			}
		}
		
		ImageSource m_isBtnCust3 = null;
		public ImageSource BtnCust3Img
		{
			set
			{
				m_isBtnCust3 = value;
				
				RaisePropertyChanged( "BtnCust3Img" );
			}
			get
			{
				return m_isBtnCust3;
			}
		}
		
		Visibility m_visBtnCust3 = Rsc.Collapsed;
		public Visibility BtnCust3Vis
		{
			set
			{
				m_visBtnCust3 = value;
				
				RaisePropertyChanged( "BtnCust3Vis" );
			}
			get
			{
				return m_visBtnCust3;
			}
		}
		
		ImageSource m_isBtnCust4 = null;
		public ImageSource BtnCust4Img
		{
			set
			{
				m_isBtnCust4 = value;
				
				RaisePropertyChanged( "BtnCust4Img" );
			}
			get
			{
				return m_isBtnCust4;
			}
		}
		
		Visibility m_visBtnCust4 = Rsc.Collapsed;
		public Visibility BtnCust4Vis
		{
			set
			{
				m_visBtnCust4 = value;
				
				RaisePropertyChanged( "BtnCust4Vis" );
			}
			get
			{
				return m_visBtnCust4;
			}
		}
					
		public virtual Visibility Desc1Vis
		{
			set
			{
			}
			get
			{
				return Rsc.Visible;
			}
		}
		public virtual string Desc1
		{
			set
			{
			}
			get
			{
				return "";
			}
		}
		
		public virtual string Desc2
		{
			set
			{
			}
			get
			{
				return "";
			}
		}
		
		public RscSimpleLbItemBase This { get{ return this; } }
		public RscSimpleLbItemList Holder { get{ return m_Holder; } }
		public int ListBoxAsteriskWidth { get{ return Holder.ListBoxAsteriskWidth; } }
		
		public RscSimpleLbItemBase( RscSimpleLbItemList oHolder )
		{
			m_Holder = oHolder;
		}
		
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
			
			return Holder.Theme.ThemeColors.TreeLeafBack;
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
			
			return Holder.Theme.ThemeColors.TreeLeafFore;
		}
		
	}
	
}