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
	
    public class RscTreeLbItemList : ObservableCollection<RscTreeLbItemBase>, IList<RscTreeLbItemBase>, IList
    {
		public int ListBoxAsteriskWidth { set; get; }
		
		protected RscTheme m_Theme = null;
		
		protected ImageSource m_isBtnExpand = null;
		protected ImageSource m_isBtnCollapse = null;
		
		protected ListBox m_lb = null;
		
		public RscTreeLbItemList( ListBox lb, RscTheme Theme, string sImgBtnExpand, string sImgBtnCollapse )
		{
			m_Theme = Theme;
			
			m_isBtnExpand = Theme.GetImage(sImgBtnExpand);
			m_isBtnCollapse = Theme.GetImage(sImgBtnCollapse);
			
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
		
		public ImageSource IsBtnExpand { get{ return m_isBtnExpand; } }
		public ImageSource IsBtnCollapse { get{ return m_isBtnCollapse; } }
		
		//VERY SLOW!!!
		/*
		public void PreRefresh()
		{
			//Detach list to speed up operatoins...
			
			m_lb.ItemsSource = null;
		}
		*/
		
		//VERY SLOW!!!
		/*
		public void Refresh()
		{			
			//ReQuery...
			
			m_lb.ItemsSource = null;
			m_lb.ItemsSource = this;
		}
		*/
		
		public void RemoveAll( RscTreeLbItemBase tiRemoveAfter )
		{
			int i = IndexOf( tiRemoveAfter );
			if( i < 0 )
				return;
			
			i++;
			for(;;)
			{
				if( i >= Count )
					break;
				
				RemoveAt( i );
			}
		}
		
		public void CollapseAll( bool bClearItemDataToo )
		{
			//VERY SLOW!!!
			//bool bRefreshNeeded = false;
			
			int i = -1;
			for(;;)
			{
				i++;
				if( i >= Count )
					break;
				
				RscTreeLbItemBase it = this[ i ];
				
				if( it.Level > 0 )
					continue;
				
				//VERY SLOW!!!
				/*
				if( !bRefreshNeeded )
				{
					//VERY SLOW!!!
					//PreRefresh();
					
					bRefreshNeeded = true;
				}
				*/
				
				//VERY SLOW!!!
				//it.Collapse( false );
				
				it.Collapse( );
				
				if( bClearItemDataToo )
					it.ClearData();
			}
			
			//VERY SLOW!!!
			/*
			if( bRefreshNeeded )
			{
				//VERY SLOW!!!
				//Refresh();
			}
			*/
		}
		
		public void ExpandAll( )
		{
			if( Count <= 0 )
				return;
			
			int i = -1;
			for(;;)
			{
				i++;
				if( i >= Count )
					break;
				
				RscTreeLbItemBase it = this[ i ];
				
				if( it.Expanded )
					continue;
				
				if( it.IsLeaf || it.IsDetailsOnly )
					continue; //Surely leaf...
				
				it.Expand();
			}
		}
		
	}
		
	public class RscTreeLbItemBase : INotifyPropertyChanged
	{
		//FIX: To auto-update bindings...
		//SRC: http://stackoverflow.com/questions/26806762/c-sharp-listbox-update-binding-text
		public event PropertyChangedEventHandler PropertyChanged;
		private void RaisePropertyChanged(string propName)
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
	
		const int ciCxIndent = 16;
		
		protected RscTreeLbItemList m_Holder = null;
		
		public RscTreeLbItemBase This { get{ return this; } }
		public RscTreeLbItemList Holder { get{ return m_Holder; } }
		public int ListBoxAsteriskWidth { get{ return (Holder.ListBoxAsteriskWidth - (Level * ciCxIndent)); } }
		
		public Thickness IndentedMargin
		{
			get
			{
				return new Thickness( Level * ciCxIndent, 0, 0, 4 );
			}
		}
		
		public Thickness IndentedDetailsMargin
		{
			get
			{
				if( Expanded )
					return new Thickness( ciCxIndent, 0, 0, 0 );
				else
					return new Thickness( 0 );
			}
		}
		
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
		
		private Color m_clrDetailsBackColor;
		public Color DetailsBackColor
		{
			set
			{
				m_clrDetailsBackColor = value;
			}
		}
		public Brush DetailsBackBrush
		{
			get
			{
				return new SolidColorBrush(m_clrDetailsBackColor);
			}
		}
				
		private Color m_clrDetailsForeColor;
		public Color DetailsForeColor
		{
			set
			{
				m_clrDetailsForeColor = value;
			}
		}
		public Brush DetailsForeBrush
		{
			get
			{
				return new SolidColorBrush(m_clrDetailsForeColor);
			}
		}
		
		public Visibility TitleVisibility
		{
			get
			{
				if( m_bDetailsOnly )
					return Rsc.Collapsed;
				
				return Rsc.Visible;
			}
		}
		
		public string Title
		{
			set
			{
				m_sTitle = value;
				
				RaisePropertyChanged( "Title" );
			}
			get
			{
				if( m_sTitle.Length > 0 )
					return m_sTitle;
				
				return GetTitle();
			}
		}
		
		public ImageSource BtnExpCollImage
		{
			get
			{
				if( m_bExpanded )
					return Holder.IsBtnCollapse;
				
				return Holder.IsBtnExpand;
			}
		}
		
		public Visibility BtnExpCollImageVisibility
		{
			get
			{
				if( m_bDetailsOnly || m_bLeaf )
					return Rsc.Collapsed;
				
				return Rsc.Visible;
			}
		}
		
		public ImageSource BtnCustom1Image { get; set; }
		protected Visibility m_visBtnCust1 = Rsc.Collapsed;
		public Visibility BtnCustom1Visibility { get { return m_visBtnCust1; } set { m_visBtnCust1 = value; RaisePropertyChanged( "BtnCustom1Visibility" ); } }
		public ImageSource BtnCustom2Image { get; set; }
		protected Visibility m_visBtnCust2 = Rsc.Collapsed;
		public Visibility BtnCustom2Visibility { get { return m_visBtnCust2; } set { m_visBtnCust2 = value; RaisePropertyChanged( "BtnCustom2Visibility" ); } }
		public ImageSource BtnCustom3Image { get; set; }
		protected Visibility m_visBtnCust3 = Rsc.Collapsed;
		public Visibility BtnCustom3Visibility { get { return m_visBtnCust3; } set { m_visBtnCust3 = value; RaisePropertyChanged( "BtnCustom3Visibility" ); } }
		public ImageSource BtnCustom4Image { get; set; }
		protected Visibility m_visBtnCust4 = Rsc.Collapsed;
		public Visibility BtnCustom4Visibility { get { return m_visBtnCust4; } set { m_visBtnCust4 = value; RaisePropertyChanged( "BtnCustom4Visibility" ); } }
		
		public Visibility DetailsVisibility
		{
			get
			{
				if( Loading )
					return Rsc.Visible;
				
				if( m_bDetailsOnly )
					return Rsc.Visible;
				
				if( m_bDetailsOfTitle )
					return Rsc.Visible;
				
				return Rsc.Collapsed;
			}
		}
		
		private double m_dDetailsFontSize = 16;
		public double DetailsFontSize
		{
			set
			{
				m_dDetailsFontSize = Math.Max( 8, value );
				RaisePropertyChanged( "DetailsFontSize" );
			}
			get
			{
				return m_dDetailsFontSize;
			}
		}
		
		public string Details
		{
			get
			{
				if( Loading )
					return "Loading...";
				
				return m_sDetails;
			}
		}
		
		//
		// //
		
		protected RscTreeLbItemBase m_Parent = null;
		
		protected bool m_bDetailsOnly = false;
		protected bool m_bDetailsOfTitle = false;
		protected bool m_bLeaf = false;
		protected string m_sDetails = "";
		
		protected bool m_bExpanded = false;
		
		protected string m_sTitle = "";
		
		public RscTreeLbItemBase( RscTreeLbItemList oHolder, RscTreeLbItemBase oParent )
		{
			m_Holder = oHolder;
			m_Parent = oParent;
			
			DetailsBackColor = Holder.Theme.ThemeColors.TreeDescBack;
			DetailsForeColor = Holder.Theme.ThemeColors.TreeDescFore;
		}
		
		public RscTreeLbItemBase Parent { get{ return m_Parent; } }
		
		public int Level
		{
			get
			{
				int iLevel = 0;
				
				RscTreeLbItemBase op = m_Parent;
				while( op != null )
				{
					iLevel++;
					op = op.Parent;
				}
				
				return iLevel;
			}
		}
		
		virtual public string GetTitle()
		{
			return "N/A";
		}
		
		protected bool m_bLoading = false;
		public bool Loading
		{
			get
			{
				return m_bLoading;
			}
			set
			{
				m_bLoading = value;
				
				RaisePropertyChanged( "DetailsVisibility" );
				RaisePropertyChanged( "Details" );
			}
		}
		
		public string DetailsOnly
		{
			set
			{
				m_bDetailsOnly = true;
			
				m_sDetails = value;
				
				RaisePropertyChanged( "DetailsVisibility" );
				RaisePropertyChanged( "Details" );
				
				RaisePropertyChanged( "BtnExpCollImageVisibility" );
				RaisePropertyChanged( "TitleVisibility" );
			}
		}
		
		public string DetailsOfTitle
		{
			set
			{
				m_bDetailsOfTitle = true;
				
				m_sDetails = value;
				
				RaisePropertyChanged( "DetailsVisibility" );
				RaisePropertyChanged( "Details" );
			}
		}
		
		public bool IsDetailsOnly
		{
			get
			{
				return m_bDetailsOnly;
			}
		}
		
		public bool IsLeaf
		{
			set
			{
				m_bLeaf = value;
				
				RaisePropertyChanged( "BtnExpCollImageVisibility" );
			}
			get
			{
				return m_bLeaf;
			}
		}
		
		private bool m_bCustomBackColor = false;
		private Color m_clrCustomBackColor;
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
		public Color GetBackColor()
		{
			if( m_bCustomBackColor ) return m_clrCustomBackColor;
			
			if( !HasChildren() || m_bLeaf ) return Holder.Theme.ThemeColors.TreeLeafBack;
			return Holder.Theme.ThemeColors.TreeContainerBack;
		}
		
		private bool m_bCustomForeColor = false;
		private Color m_clrCustomForeColor;
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
		public Color GetForeColor()
		{
			if( m_bCustomForeColor ) return m_clrCustomForeColor;
			
			if( !HasChildren() || m_bLeaf ) return Holder.Theme.ThemeColors.TreeLeafFore;
			return Holder.Theme.ThemeColors.TreeContainerFore;
		}
		
		public bool Expanded
		{
			get
			{
				return m_bExpanded;
			}
		}
		
		private int IndexOf
		{
			get
			{
				return m_Holder.IndexOf( this );
			}
		}
		
		private int m_iIndexOfNext = 0;
		private bool m_bInsertCalled = false;
		public void PreInserts()
		{
			m_bInsertCalled = false;
			m_iIndexOfNext = IndexOf;
		}
		public void Insert( RscTreeLbItemBase it )
		{
			m_bInsertCalled = true;
			m_iIndexOfNext++;
			Holder.Insert( m_iIndexOfNext, it );
			
			if( !m_bHasChildren ) //This is Leaf (at this time)...
			{
				if( !it.m_bLeaf && !it.m_bDetailsOnly ) //Child is not Leaf...
				{
					m_bHasChildren = true;
					
					RaisePropertyChanged( "BackBrush" );
					RaisePropertyChanged( "ForeBrush" );
				}
			}
		}
		
		//VERY SLOW!!!
		//public void Collapse( bool bRefresh = true )
		public void Collapse( )
		{
			if( !Expanded )
				return;
			
			//VERY SLOW!!!
			/*
			if( bRefresh )
				Holder.PreRefresh();
			*/
			
			int iLevel = Level;
			int iIndex = IndexOf + 1;
			
			while( iIndex < Holder.Count )
			{
				if( Holder[ iIndex ].Level <= iLevel )
					break; //All children passed...
				
				Holder.RemoveAt( iIndex );
			}
			
			m_bExpanded = false;
			
			//VERY SLOW!!!
			/*
			if( bRefresh )
				Holder.Refresh();
			*/
			
			RaisePropertyChanged( "BtnExpCollImage" );
			RaisePropertyChanged( "IndentedDetailsMargin" );
		}
		
		virtual public void ClearData()
		{
			//Called to force data reload on next Expand...
		}
		
		virtual public void Expand()
		{
			Expand_Base();
		}
		
		public void Expand_Base()
		{
			//Call this...
			
			if( m_bInsertCalled )
			{
				m_bExpanded = true;
			
				//VERY SLOW!!!
				//Holder.Refresh();
			
				RaisePropertyChanged( "BtnExpCollImage" );
				RaisePropertyChanged( "IndentedDetailsMargin" );
			}
		}
		
		protected bool m_bHasChildren = false;
		virtual public bool HasChildren()
		{
			return m_bHasChildren; //Details / Property leaves are not children!!!
		}
		
	}
	
}