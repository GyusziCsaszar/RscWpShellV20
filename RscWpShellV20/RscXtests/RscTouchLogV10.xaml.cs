using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Windows.Input;

using Ressive.Utils;
using Ressive.Theme;

namespace RscXtests
{
	
    public partial class RscTouchLogV10 : PhoneApplicationPage
    {
		
		RscTheme m_Theme = null;
		
		Point ptStart;
		
        public RscTouchLogV10()
        {
            InitializeComponent();
 			
			//MemUsage Optimization...
			Button GlobalDILholder = Application.Current.Resources["GlobalDIL"] as Button;
			m_Theme = (RscTheme) GlobalDILholder.Tag;
			//m_dil = new RscDefaultedImageList( "Theme", "Current", "Default" );
			
			imgTestArea.Source = m_Theme.GetImage("Images/Bk001_portrait.jpg");
			
			Touch.FrameReported += new System.Windows.Input.TouchFrameEventHandler(Touch_FrameReported);
       }

        private void Touch_FrameReported(object sender, System.Windows.Input.TouchFrameEventArgs e)
        {
			
			string sLog;
			
			DateTime dNow = DateTime.Now;
			sLog = RscUtils.pad60(dNow.Minute) + ":" + RscUtils.pad60(dNow.Second) + " - ";
			
			TouchPoint primaryTouchPoint = e.GetPrimaryTouchPoint(null);
			
			if( primaryTouchPoint == null )
			{
				sLog += "null";
				lstLog.Items.Insert(0, sLog);
			}
			else
			{
				if( primaryTouchPoint.TouchDevice.DirectlyOver != imgTestArea )
				{
					return;
				}
				else
				{
					if( primaryTouchPoint.Action == TouchAction.Down )
					{
						ptStart = primaryTouchPoint.Position;
					}
					
					sLog += primaryTouchPoint.Action.ToString();
					lstLog.Items.Insert(0, sLog);
					
					sLog = "      ";
					sLog += primaryTouchPoint.Position.ToString();
					lstLog.Items.Insert(1, sLog);
					
					sLog = "      ";
					sLog += primaryTouchPoint.Size.ToString();
					lstLog.Items.Insert(2, sLog);
					
					sLog = "      ";
					if( ptStart.X <= primaryTouchPoint.Position.X )
						sLog += "Right";
					else
						sLog += "Left";
					lstLog.Items.Insert(3, sLog);
					
					sLog = "      ";
					if( ptStart.Y <= primaryTouchPoint.Position.Y )
						sLog += "Down";
					else
						sLog += "Top";
					lstLog.Items.Insert(4, sLog);
					
					/*
					sLog = "      ";
					sLog += (primaryTouchPoint.Position.X - ptStart.X).ToString();
					sLog += ":";
					sLog += (primaryTouchPoint.Position.Y - ptStart.Y).ToString();
					lstLog.Items.Insert(5, sLog);
					
					sLog = "      ";
					sLog += ptStart.ToString();
					lstLog.Items.Insert(6, sLog);
					*/
					
				}
			}		
        }
		
    }
}
