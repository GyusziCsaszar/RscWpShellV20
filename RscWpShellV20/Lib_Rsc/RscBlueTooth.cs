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

using System.Threading.Tasks;

namespace Ressive.BlueTooth
{
	
	public class RscBlueTooth
	{

		//
		// VERY-VERY SLOW!!!
		// (but working...)
		//
		/*
		public static bool Enabled
		{
			get
			{
				bool bEnabled = false;
				
				var taskHlp = Task.Run(async () => { bEnabled = await FindPaired(); });
				taskHlp.Wait();
				
				return bEnabled;
			}
		}
		
		protected async static Task<bool> FindPaired( )
		{
			bool bth = false;
		
			//src: http://stackoverflow.com/questions/15895884/how-can-i-know-bluetooth-is-enabled-or-not-on-windows-phone-8
			
			Windows.Networking.Proximity.PeerFinder.Start();
			try
			{
				var peers = await Windows.Networking.Proximity.PeerFinder.FindAllPeersAsync();
				bth = true; //boolean variable
			}
			catch (Exception ex)
			{
				if ((uint)ex.HResult == 0x8007048F)
				{
					bth = false;
				}
			}
			
			return bth;
		}
		*/
		
	}
			
}
