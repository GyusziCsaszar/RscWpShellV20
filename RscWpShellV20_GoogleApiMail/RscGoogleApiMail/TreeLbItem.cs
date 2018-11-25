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

using Ressive.GoogleApi;

namespace RscGoogleApiMail
{
	
	public class TreeLbItem : TreeLbItemThread
	{
		
		public GoogleRequest gr = GoogleRequest.None;
		
		protected RscJSonItem m_jsonParameters = null;
		protected RscJSonItem m_jsonResponse = null;
		
		public TreeLbItem( RscTreeLbItemList oHolder, RscTreeLbItemBase oParent )
		: base( oHolder, oParent )
		{
		}
		
		override public string GetTitle()
		{
			if( m_sTitle.Length > 0 )
				return m_sTitle;
			
			if( gr != GoogleRequest.None )
				return GoogleUtils.GoogleRequestTitle( gr );
			
			if( m_jsonResponse != null )
				return m_jsonResponse.Name;
			
			return "N/A";
		}
		
		public bool HasResponse {get{ return (m_jsonResponse != null); } }
		public void SetResponse( RscJSonItem jsonResponse, bool bExpand = true )
		{
			m_jsonResponse = jsonResponse;
			
			if( bExpand )
			{
				Expand();
			}
		}
		public RscJSonItem Response { get{ return m_jsonResponse; } }
		
		override public void ClearData()
		{
			//Called to force data reload on next Expand...
			
			if( gr != GoogleRequest.None )
				m_jsonResponse = null;
		}
		
		override public void Expand()
		{
			if( Expanded )
				return;
			
			if( !HasResponse )
			{
				base.Expand();
				return;
			}
			
			PreInserts();
			
			if( m_jsonResponse.ID.Length > 0 )
			{
				TreeLbItem ti = new TreeLbItem( Holder, this );
				ti.DetailsOnly = "RscJSonItem.ID" + ": " + m_jsonResponse.ID;
				
				ti.DetailsBackColor = Colors.Gray;
				ti.DetailsForeColor = Colors.Black;
				
				Insert( ti );
			}
			if( m_jsonResponse.Description.Length > 0 )
			{
				TreeLbItem ti = new TreeLbItem( Holder, this );
				ti.DetailsOnly = "RscJSonItem.Description" + ": " + m_jsonResponse.Description;
				
				ti.DetailsBackColor = Colors.Gray;
				ti.DetailsForeColor = Colors.Black;
				
				Insert( ti );
			}
						
			if( m_jsonParameters != null )
			{
				//Response properties inherited from parent to get this response...
				for( int i = 0; i < m_jsonParameters.PropertyCount; i++ )
				{
					RscJSonItemProperty oProp = m_jsonParameters.GetProperty( i );
		
					TreeLbItem ti = new TreeLbItem( Holder, this );
					ti.DetailsOnly = oProp.Name + ": " + oProp.Value( false );
				
					ti.DetailsBackColor = Colors.White;
					ti.DetailsForeColor = Colors.Black;
					
					Insert( ti );
				}
			}
						
			for( int i = 0; i < m_jsonResponse.PropertyCount; i++ )
			{
				RscJSonItemProperty oProp = m_jsonResponse.GetProperty( i );
	
				TreeLbItem ti = new TreeLbItem( Holder, this );
				ti.DetailsOnly = oProp.Name + ": " + oProp.Value( false );
				
				Insert( ti );
			}
			
			GoogleRequest grParent = GoogleRequest.None;
			if( Parent != null )
			{
				if( ((TreeLbItem) Parent).Response != null )
				{
					grParent = GoogleUtils.GoogleRequestFromUrl( ((TreeLbItem) Parent).Response.ID );
				}
			}
						
			for( int i = 0; i < m_jsonResponse.ChildCount; i++ )
			{
				RscJSonItem oChild = m_jsonResponse.GetChild( i );
	
				TreeLbItem ti = new TreeLbItem( Holder, this );
				
				/*
				if( m_jsonResponse.Name != "error" )
				{
				*/
				
					switch( grParent )
					{
						
						case GoogleRequest.GMail_Messages :
						{
							if( m_jsonResponse.Name == "messages" )
							{
								//Downloadable, but with parameters...
								ti.gr = GoogleRequest.GMail_Message_Details;
								ti.m_jsonParameters = oChild;
							}
							else
							{
								ti.m_jsonResponse = oChild;
							}
							break;
						}
							
						default :
						{
							//Allowe to expand item...
							ti.m_jsonResponse = oChild;
							break;
						}
							
					}
					
				/*
				}
				else
				{
					ti.m_jsonResponse = oChild;
				}
				*/
				
				Insert( ti );
			}
			
			base.Expand();
		}
		
		override public bool HasChildren()
		{
			if( m_jsonResponse != null )
			{
				if( m_jsonResponse.ChildCount > 0 )
					return true;
			}
			
			return false; //Details / Property leaves are not children!!!
		}
		
		public RscJSonItem Parameters
		{
			get
			{
				return m_jsonParameters;
			}
		}
		
	}
	
}