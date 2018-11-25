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

using Ressive.FrameWork;

using Ressive.Formats;

namespace Lib_RscViewers
{
	
	public class RscViewer_JSon_TreeLbItem : RscTreeLbItemBase
	{
		
		protected RscJSonItem m_json = null;
		
		public RscViewer_JSon_TreeLbItem( RscTreeLbItemList oHolder, RscTreeLbItemBase oParent )
		: base( oHolder, oParent )
		{
		}
		
		override public string GetTitle()
		{
			if( m_json == null )
				return "<none>";
			
			if( m_json.Name.Length == 0 )
				return "<empty>";
			
			return m_json.Name;
		}
		
		public bool HasJSon {get{ return (m_json != null); } }
		public void SetJSon( RscJSonItem json )
		{
			m_json = json;
			
			Expand();
		}
		
		override public void Expand()
		{
			if( Expanded )
				return;
			
			if( !HasJSon )
				return;
			
			PreInserts();
			
			if( m_json.ID.Length > 0 )
			{
				RscViewer_JSon_TreeLbItem ti = new RscViewer_JSon_TreeLbItem( Holder, this );
				ti.DetailsOnly = "RscJSonItem.ID" + ": " + m_json.ID;
				
				Insert( ti );
			}
			if( m_json.Description.Length > 0 )
			{
				RscViewer_JSon_TreeLbItem ti = new RscViewer_JSon_TreeLbItem( Holder, this );
				ti.DetailsOnly = "RscJSonItem.Description" + ": " + m_json.Description;
				
				Insert( ti );
			}
						
			for( int i = 0; i < m_json.PropertyCount; i++ )
			{
				RscJSonItemProperty oProp = m_json.GetProperty( i );
	
				RscViewer_JSon_TreeLbItem ti = new RscViewer_JSon_TreeLbItem( Holder, this );
				ti.DetailsOnly = oProp.Name + ": " + oProp.Value( false );
				
				Insert( ti );
			}
						
			for( int i = 0; i < m_json.ChildCount; i++ )
			{
				RscJSonItem oChild = m_json.GetChild( i );
	
				RscViewer_JSon_TreeLbItem ti = new RscViewer_JSon_TreeLbItem( Holder, this );
				ti.m_json = oChild;
				
				Insert( ti );
			}
			
			base.Expand();
		}
		
		override public bool HasChildren()
		{
			if( m_json != null )
			{
				if( m_json.ChildCount > 0 )
					return true;
			}
			
			return false; //Details / Property leaves are not children!!!
		}
		
	}

}