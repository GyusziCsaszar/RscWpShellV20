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

using Microsoft.Phone.Controls;

using Ressive.Utils;

namespace RscIeV10
{
	
	public class RscBrowserBroker
	{
		
		WebBrowser m_wb1;
		WebBrowser m_wb2;
		
		WebBrowser Active = null;
		
		string m_sUserAgentID = "";
		string m_sUserAgent = "";
		
		int m_iToPurge = 0;
		
		public RscBrowserBroker( WebBrowser wb1, WebBrowser wb2 )
		{
			NightMode = false;
			
			m_wb1 = wb1;
			m_wb1.Visibility = Rsc.Visible;
			
			m_wb2 = wb2;
			m_wb2.Visibility = Rsc.Collapsed;
			
			Active = m_wb1;
		}
		
		public void Purge()
		{
			m_iToPurge = 2;
			m_wb1.NavigateToString("");
			m_wb2.NavigateToString("");
		}
		
		public bool OnNavigating( WebBrowser wb, Uri uri, out bool bCancel )
		{
			bCancel = false;
			
			//Stop or Purge...
			if( uri.ToString().Length == 0 || uri.ToString() == "about:blank" )
			{
				//Purge...
				if( m_iToPurge > 0 )
				{
					return false;
				}
			}
			
			if( !HasUserAgent )
				return true;
			
			// //
			//
			
			if( wb.Visibility == Rsc.Collapsed )
				return true;
			
			Active = null;
			
			WebBrowser wbOther = TheOther( wb );
			
			wbOther.Visibility = Rsc.Collapsed;
			
			wbOther.Navigate( uri, (byte []) null, "User-Agent: " + m_sUserAgent );
			
			wb.Visibility = Rsc.Collapsed;
			
			//To force other browser to release resources (???)...
			m_iToPurge = 1;
			wb.NavigateToString("");
			
			bCancel = true;
			return false;
			
			//
			// //
		}
		
		public bool OnNavigated( WebBrowser wb, Uri uri, bool bFailed )
		{
			//Stop or Purge...
			if( uri.ToString().Length == 0 || uri.ToString() == "about:blank" )
			{
				//Purge...
				if( m_iToPurge > 0 )
				{
					m_iToPurge = Math.Max( 0, m_iToPurge - 1 );
					return false;
				}
			}
			
			ApplyNightMode( wb );
			
			wb.Visibility = Rsc.Visible;
			
			Active = wb;
			
			return true;
		}
		
		protected WebBrowser TheOther( WebBrowser wb )
		{
			if( wb == m_wb1 ) return m_wb2;
			/*if( wb == m_wb2 )*/ return m_wb1;
		}
		
		protected WebBrowser TheCurrent
		{
			get
			{
				if( Active != null )
					return Active;
				
				if( m_wb1.Visibility == Rsc.Visible ) return m_wb1;
				if( m_wb2.Visibility == Rsc.Visible ) return m_wb2;
				
				m_wb1.Visibility = Rsc.Visible;
				return m_wb1;
			}
		}
		
		public string UserAgentID { get{ return m_sUserAgentID; } }
		public void SetUserAgentID( string sUaID )
		{
			m_sUserAgentID = sUaID;
			m_sUserAgent = RscUserAgents.UserAgentFromID( sUaID, false );
		}
		
		public bool HasUserAgent
		{
			get
			{
				return (m_sUserAgent.Length > 0);
			}
		}
		
		public Visibility Visibility
		{
			set
			{
				if( value == Rsc.Visible )
				{
					if( Active != null )
					{
						Active.Visibility = Rsc.Visible;
					}
				}
				else
				{
					m_wb1.Visibility = Rsc.Collapsed;
					m_wb2.Visibility = Rsc.Collapsed;
				}
			}
		}
		
		public void NavigateToNull()
		{
			TheCurrent.NavigateToString("");
		}
		
		public void Navigate( Uri uri )
		{
			TheCurrent.Navigate( uri );
			
			//
			// PROBLEM: Sets only for given navigation!!!
			// DOES NOT apply to navigation by clicking a link in browser!!!
			//
			/*
			//This is needed, don't know why, no compile otherwise...
			WebBrowser w = wbc;
			//SRC: http://www.lukepaynesoftware.com/articles/programming-tutorials/changing-the-user-agent-in-a-web-browser-control/
			w.Navigate( uri, (byte []) null, "User-Agent: " + csUserAgent_Wp71 );
			*/
		}
		
		public bool NightMode {get; set;}
		
		public void ApplyNightMode( WebBrowser wb )
		{
			if( NightMode )
				wb.Opacity = 0.6;
			else
				wb.Opacity = 1;
		}
			
			//ERR: Not compiled by Blend2012!!!
			//SRC: http://stackoverflow.com/questions/20853441/wp-webbrowser-black-background
			/*
            IEnumerable<DependencyObject> borders = wb.Descendants<Border>();
            foreach (var o in borders)
            {
                var ding = o as Border;
                ding.Background = new SolidColorBrush(Colors.Black);
            }
			*/
			
			/*
 			Border brd = FindFirstElementInVisualTree<Border>(wb);
			for(;;)
			{
				if( brd == null )
					break;
				
				brd.Background = new SolidColorBrush( Colors.Black );
				
				brd = FindFirstElementInVisualTree<Border>(brd);
			}
			*/
		//}
		
		/*
		private T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
		{
			var count = VisualTreeHelper.GetChildrenCount(parentElement);
			if (count == 0)
				return null;
		
			for (int i = 0; i < count; i++)
			{
				var child = VisualTreeHelper.GetChild(parentElement, i);
				
				if (child != null && child is T)
				{
					return (T)child;
				}
				else
				{
					var result = FindFirstElementInVisualTree<T>(child); 
					if (result != null)
						return result;
				
				}
			}
			return null;
		}
		*/
		
	}
	
}