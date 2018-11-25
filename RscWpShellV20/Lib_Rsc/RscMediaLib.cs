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

using Microsoft.Xna.Framework.Media;

namespace Ressive.MediaLib
{
	
	public static class RscMediaLib
	{
		
		public static void StopMusic()
		{
			Song s = Song.FromUri("empty", new Uri( /*"/Lib_Rsc;component/" +*/ "Media/empty.mp3", UriKind.Relative));
			Microsoft.Xna.Framework.FrameworkDispatcher.Update();
			MediaPlayer.Play(s);
		}
		
	}
	
}