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

namespace Ressive.Utils
{
	
	public static class RscFileTypes
	{
		
		public static void RegisterAll()
		{

			//File extension -> group -> app associations:
			RscRegFs.RegisterFileGroup( "RscViewer_JSonV10",  "Lib_RscViewers", false, false, "JSon", "json");
			RscRegFs.RegisterFileGroup( "RscViewer_HexaV10",  "Lib_RscViewers", false, false, "Data", "dat");
			//FAILS!!! //RscRegFs.RegisterFileGroup( "MainPage",           "RscIeV10",       false, false, "PDF", "pdf");
			RscRegFs.RegisterFileGroup( "MainPage",           "RscIeV10",       false, true,  "Internet.Link", "ilnk");
			RscRegFs.RegisterFileGroup( "RscViewer_ImageV10", "Lib_RscViewers", true,  false, "Image.Native", "png;jpg;gif");
			RscRegFs.RegisterFileGroup( "RscViewer_MediaV10", "Lib_RscViewers", true,  false, "Video.Native", "mp4;wma;3gp");
			RscRegFs.RegisterFileGroup( "RscViewer_SoundV11", "Lib_RscViewers", true,  false, "Sound.Native", "wav;mp3;m4a;flac");
			RscRegFs.RegisterFileGroup( "RscViewer_TextV10",  "Lib_RscViewers", true,  false, "Text",
				"txt"
				+ ";string;dword"
				+ ";ini;inf"
				+ ";vcf"
				+ ";error;info"
				+ ";xml" );
		
		}
		
	}
	
}