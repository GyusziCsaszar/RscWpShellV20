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

namespace RscGoogleApiMail_Lib
{
	
	public class MyThread2
	{
		
		public string ID {set; get;}
		public string HistoryID {set; get;}
		public string Snippet {set; get;}
		
		public DateTime DateSaved {set; get;}
		public DateTime DateAcked {set; get;}
		
		public MyThread2()
		{
			ID = "";
			HistoryID = "";
			Snippet = "";
			
			DateSaved = DateTime.Now;
			DateAcked = DateTime.MinValue;
		}
		
		public bool Acknowledged
		{
			get
			{
				if( DateAcked == DateTime.MinValue ) return false;
				return true;
			}
		}
		
	}
	
}