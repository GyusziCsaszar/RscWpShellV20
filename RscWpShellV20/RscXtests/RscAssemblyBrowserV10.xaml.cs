using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Shapes;

using System.Reflection;

using Ressive.FrameWork;

namespace RscXtests
{
	
    public partial class RscAssemblyBrowserV10 : PhoneApplicationPage
    {
		
		RscAppFrame m_AppFrame;
		
		class TreeItem
		{
			
			public TreeItem Parent;
			
			public int Level;
			
			public string ContainerID;
			
			public string Title;
			
			public string Details;
			
			public Grid UIElement = null;
			
			public Assembly asy = null;
			public Type typ = null;
			public MethodInfo mi = null;
			
			public List<TreeItem> Children = new List<TreeItem>();
			
			public TreeItem(TreeItem tiParent, string sContainerID, string sTitle, string sDetails = "")
			{
				Parent = tiParent;
				
				if( Parent == null )
				{
					Level = 0;
				}
				else
				{
					Level = Parent.Level + 1;
					Parent.Children.Add( this );
				}
				
				ContainerID = sContainerID;
				Title = sTitle;
				Details = sDetails;
			}
			
		}
		
        public RscAssemblyBrowserV10()
        {
            InitializeComponent();
			
			m_AppFrame = new RscAppFrame("Ressive.Hu", "Assembly Browser 1.0", "Images/Ico001_Ressive.jpg"
				, this, AppTitleBar, AppStatusBar);
			// ///////////////
			m_AppFrame.OnNext +=new Ressive.FrameWork.RscAppFrame.OnNext_EventHandler(m_AppFrame_OnNext);
			m_AppFrame.OnExit +=new Ressive.FrameWork.RscAppFrame.OnExit_EventHandler(m_AppFrame_OnExit);
			
			m_AppFrame.ShowButtonNext( false );
			
			TreeItem ti;
			
			// CURRENT APP PARTS
			/*
			AssemblyPartCollection parts = Deployment.Current.Parts;
			foreach( AssemblyPart part in parts )
			{
				var assemblyName = part.Source.ToString().Replace(".dll", string.Empty);
				
				ti = new TreeItem( null, "Assembly", assemblyName.ToString(),
					assemblyName.ToString());
				ti.asy = Assembly.Load(ti.Details);
				AddItem( ti );
			}
			*/
			
			// SYSTEM DLLS REFERENCED BY THIS APP
			/*
			ExternalPartCollection partsExt = Deployment.Current.ExternalParts;
			foreach( ExternalPart part in partsExt )
			{
				var assemblyName = part.Source.ToString().Replace(".dll", string.Empty);
				
				ti = new TreeItem( null, "Assembly", assemblyName.ToString(),
					assemblyName.ToString());
				ti.asy = Assembly.Load(ti.Details);
				AddItem( ti );
			}
			*/
			
			
			//CAMERA ASSY
			/*
			ti = new TreeItem( null, "Assembly", "Microsoft.Phone.Media.Extended",
				"Microsoft.Phone.Media.Extended, Version=7.0.0.0, Culture=neutral, PublicKeyToken=24eec0d8c86cda1e");
			ti.asy = Assembly.Load(ti.Details);
			AddItem( ti );
			*/
			
			
			//Wp8
			ti = new TreeItem( null, "Assembly", "Microsoft.Phone",
				"Microsoft.Phone, Version=8.0.0.0, Culture=neutral, PublicKeyToken=24eec0d8c86cda1e");
			ti.asy = Assembly.Load(ti.Details);
			AddItem( ti );
			
			
			/*
			
			//Wp7: \\Applications\\Data\\32C945F5-B5D7-4287-95CE-B814F446339F\\Data\\IsolatedStore\\...
			//Wp8: C:\\Data\\Users\\DefApps\\AppData\\{EF1C8978-ACB9-4231-97A7-6F2CFC10AC5C}\\Local\\
			
			string sPath = "\\Applications\\Data\\a47fac39-2a1b-48d2-b906-053a4f1e5e95\\Data\\IsolatedStore\\";
			sPath += "FTP\\RscWpShellV10";
			//sPath += ".dll";
			
			BitmapImage bmp = new BitmapImage(
				new Uri(
				"file://Application/Data/A47FAC39-2A1B-48D2-B906-053A4F1E5E95/Data/IsolatedStore/FTP/Test.jpg"
				, UriKind.Absolute));
			
			imgTest.Source = bmp;
			
			TreeItem ti = new TreeItem( null, "Assembly", "FTP\\RscWpShellV10.dll", sPath );
			
			try
			{
				
				//ti.asy = Assembly.LoadFrom(ti.Details);
				
				//ti.asy = Assembly.Load(ti.Details);
				
				AddItem( ti );
			}
			catch( Exception exc )
			{
				MessageBox.Show( exc.Message );
			}
			*/
        }
		
		private void m_AppFrame_OnExit(object sender, EventArgs e)
		{
			this.NavigationService.GoBack();
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
			
			if( m_AppFrame.CancelTimer() )
				e.Cancel = true;
			
			//e.Cancel = true;
        }
		
		private void m_AppFrame_OnNext(object sender, EventArgs e)
		{
			this.NavigationService.GoBack();
		}
		
		private void AddItem(TreeItem ti)
		{
			int iInsertAt = spItems.Children.Count;
			if( ti.Parent != null )
			{
				iInsertAt = spItems.Children.IndexOf( ti.Parent.UIElement );
				iInsertAt += ti.Parent.Children.Count;
				//iInsertAt++; //Count includes this ti instance!!!
			}
			
			//int idx = spItems.Children.Count + 1;
			
			Grid grdOut = new Grid();
			//grdOut.Name = "grdOut_" + idx.ToString();
			grdOut.Margin = new Thickness(0, 0, 0, 4 );
			RowDefinition rd;
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdOut.RowDefinitions.Add(rd);
			rd = new RowDefinition(); rd.Height = GridLength.Auto; grdOut.RowDefinitions.Add(rd);
			//spItems.Children.Add(grdOut);
			spItems.Children.Insert( iInsertAt, grdOut );
			
			ti.UIElement = grdOut;
			
			Rectangle rc;
			rc = new Rectangle();
			rc.Margin = new Thickness( (ti.Level * 12), 0, 0, 0 );
			if( ti.ContainerID.Length > 0 )
			{
				rc.Fill = new SolidColorBrush(Color.FromArgb( 255, 98, 98, 98));
			}
			else
			{
				rc.Fill = new SolidColorBrush(Colors.LightGray);
			}
			//rc.Opacity = 0.5;
			rc.SetValue(Grid.RowProperty, 0);
			grdOut.Children.Add(rc);
	
			Button btnMore = new Button();
			//btnMore.Name = "btnOpen_" + idx.ToString();
			btnMore.Content = ti.Title;
			btnMore.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
			btnMore.BorderThickness = new Thickness(0);
			btnMore.FontSize = 20;
			if( ti.ContainerID.Length > 0 )
			{
				btnMore.Foreground = new SolidColorBrush(Colors.White);
			}
			else
			{
				btnMore.Foreground = new SolidColorBrush(Colors.Black);
			}
			btnMore.Margin = new Thickness(-12 + (ti.Level * 12),-10,-12,-12);
			btnMore.SetValue(Grid.RowProperty, 0);
			grdOut.Children.Add(btnMore);
			
			if( ti.Details.Length > 0 )
			{
				Grid grdTit = new Grid();
				//grdTit.Name = "grdTit_" + idx.ToString();
				grdTit.Margin = new Thickness(0, 0, 0, 0);
				//RowDefinition rd;
				rd = new RowDefinition(); grdTit.RowDefinitions.Add(rd);
				grdTit.SetValue(Grid.RowProperty, 1);
				grdOut.Children.Add(grdTit);
				
				TextBox tbDetails = new TextBox();
				//tbDetails.Name = "tbDet_" + idx.ToString();
				tbDetails.FontSize = 16;
				tbDetails.Text = ti.Details;
				tbDetails.Background = new SolidColorBrush(Color.FromArgb( 255, 236, 244, 178));
				tbDetails.Foreground = new SolidColorBrush(Color.FromArgb( 255, 98, 98, 98));
				tbDetails.Margin = new Thickness(-12, -12, -12, -12);
				tbDetails.BorderThickness = new Thickness(0, 0, 0, 0);
				tbDetails.AcceptsReturn = true;
				tbDetails.TextWrapping = TextWrapping.Wrap;
				tbDetails.SetValue(Grid.RowProperty, 0);
				grdTit.Children.Add(tbDetails);
			
				Button btn = new Button();
				//btn.Name = "btnTit_" + idx.ToString();
				btn.Content = "";
				btn.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
				btn.BorderThickness = new Thickness(0);
				btn.Foreground = new SolidColorBrush(Colors.White); //.Blue);
				btn.Margin = new Thickness(-12,-10,-12,-12);
				btn.Opacity = 0.5;
				btn.SetValue(Grid.RowProperty, 1);
				grdOut.Children.Add(btn);
			
				btn.Tag = ti;
				btn.Click += new System.Windows.RoutedEventHandler(btn_Click);
			}
			
			btnMore.Tag = ti;
			btnMore.Click += new System.Windows.RoutedEventHandler(btn_Click);
		}

		private void btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn = (Button) sender;
			TreeItem tiCurrent = (TreeItem) btn.Tag;
			
			if( tiCurrent.ContainerID.Length == 0 ) return;
			
			if( tiCurrent.Children.Count > 0 )
			{
				//TODO...
				return;
			}
			
			switch( tiCurrent.ContainerID )
			{
				
				case "Assembly" :
				{
					TreeItem ti = new TreeItem( tiCurrent, "Assembly_Types", "Types" );
					ti.asy = tiCurrent.asy;
					AddItem( ti );
					break;
				}
				
				case "Assembly_Types" :
				{
					foreach( Type typ in tiCurrent.asy.GetTypes() )
					{
						TreeItem ti = new TreeItem( tiCurrent, "Assembly_Type", typ.FullName );
						ti.typ = typ;
						AddItem( ti );
					}
					break;
				}
				
				case "Assembly_Type" :
				{
					TreeItem ti = new TreeItem( tiCurrent, "Type_Methods", "Methods" );
					ti.typ = tiCurrent.typ;
					AddItem( ti );
					break;
				}
				
				case "Type_Methods" :
				{
					foreach( MethodInfo mi in tiCurrent.typ.GetMethods() )
					{
						string s = "Parameter Count: " + mi.GetGenericArguments().Length;
						s += "\r\n" + "Return Type: " + mi.ReturnType.FullName;
						TreeItem ti = new TreeItem( tiCurrent, "Method", mi.Name, s );
						ti.mi = mi;
						AddItem( ti );
					}
					break;
				}
				
			}
		}
		
    }
	
}
