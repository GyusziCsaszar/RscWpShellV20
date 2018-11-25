using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Collections;
using System.Collections.ObjectModel;

using Ressive.Utils;
using Ressive.Store;

namespace RscLearnXamlV10
{
	
	public class NoteEditor_coll_item { public object ObjectA { get; set; } public object ObjectB { get; set; } }
	public class NoteEditor_coll : ObservableCollection<NoteEditor_coll_item>, IList<NoteEditor_coll_item>, IList {}
	
    public partial class NoteEditor : PhoneApplicationPage
    {
		// //
		// Helpers {
		//ATT!!! substring parameters differ (iChrFrom, iChrTo) !!!
		void alert( string sMsg ) { MessageBox.Show( sMsg ); }
		// //
		NoteEditor host { get{ return this; } }
		NoteEditor UI { get{ return this; } }
		// //
		NoteEditor_coll createCollection() { return new NoteEditor_coll(); }
		NoteEditor_coll_item createBindableObject() { return new NoteEditor_coll_item(); }
		// } Helpers
		// //
		
		const string csDocFolder = "A:\\Documents\\Notes";
		
		string sTitOld = "";
		string sLstLoadTitle = "";
		
        public NoteEditor()
        {
            InitializeComponent();
			
			RscStore store = new RscStore();
			store.CreateFolderPath( csDocFolder );
			
			btnSend.Click += new System.Windows.RoutedEventHandler(btnSend_Click);
			btnNew.Click += new System.Windows.RoutedEventHandler(btnNew_Click);
			lbTit.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(lbTit_SelectionChanged);
			btnLoad.Click += new System.Windows.RoutedEventHandler(btnLoad_Click);
			btnSave.Click += new System.Windows.RoutedEventHandler(btnSave_Click);
			btnDel.Click += new System.Windows.RoutedEventHandler(btnDel_Click);
			txtNot.TextChanged += new System.Windows.Controls.TextChangedEventHandler(txtNot_TextChanged);
			txtTit.TextChanged += new System.Windows.Controls.TextChangedEventHandler(txtTit_TextChanged);
        }
		
		void doNew()
		{
			sLstLoadTitle = "";
		
			sTitOld = "";
			host.UI.txtTit.Text = "";
		
			host.UI.txtNot.Text = "";
			host.UI.txtCch.Text = "";
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.btnSend.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblSend.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.btnDel.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblDel.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void btnSend_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var eml = new Microsoft.
				Phone.Tasks.
				EmailComposeTask();
		
			eml.Subject = host.UI.txtTit.Text;
			eml.Body = host.UI.txtNot.Text;
		
			eml.Show();
		}

		private void btnNew_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			doNew();
			host.UI.txtCch.Text = "0";
		}

		private void lbTit_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			var item = (NoteEditor_coll_item) host.UI.lbTit.SelectedItem;
			if( item == null ) return;
		
			host.UI.lbTit.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblTitles.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.txtNot.Visibility = System.Windows.Visibility.Visible;
			host.UI.FileName.Visibility = System.Windows.Visibility.Visible;
		
			sTitOld = item.ObjectA.ToString();
			sLstLoadTitle = sTitOld;
			host.UI.txtTit.Text = sTitOld;
		
			RscStore store = new RscStore();
			
			bool bTmp;
			host.UI.txtNot.Text = store.ReadTextFile(item.ObjectB.ToString(), "", out bTmp );
			host.UI.txtCch.Text = "";
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.btnSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.btnDel.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblDel.Visibility = System.Windows.Visibility.Visible;
		}

		private void btnLoad_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			host.UI.txtNot.Text = "";
		
			listKeys();
			if( host.UI.lbTit.Items.Count == 0 )
			{
				alert("No note saved yet!");
				return;
			}
		
			host.UI.FileName.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblTitles.Visibility = System.Windows.Visibility.Visible;
			host.UI.txtNot.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lbTit.Visibility = System.Windows.Visibility.Visible;
		}

		private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sTitle = host.UI.txtTit.Text;
		
			if( sTitle == "" )
			{
				alert("No title!");
				return;
			}
			
			RscStore store = new RscStore();
			
			if( store.FileExists( csDocFolder + "\\" + sTitle + ".txt" ) )
			{
				if( sTitle == sLstLoadTitle )
				{
					//NOP...
				}
				else
				{
					alert("Title already exists!");
					return;
				}
			}
		
			string sVal = host.UI.txtNot.Text.ToString();
			
			store.WriteTextFile( csDocFolder + "\\" + sTitle + ".txt", sVal, true );
		
			sLstLoadTitle = sTitle; //"";
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.btnSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.btnDel.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblDel.Visibility = System.Windows.Visibility.Visible;
		}

		private void btnDel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sTitle = host.UI.txtTit.Text;
		
			if( sTitle == "" )
			{
				alert("No title!");
				return;
			}
			
			RscStore store = new RscStore();
			
			if( store.FileExists( csDocFolder + "\\" + sTitle + ".txt" ) )
			{
				store.DeleteFile( csDocFolder + "\\" + sTitle + ".txt" );
			}
			else
			{
				alert("Title does not exist!");
				return;
			}
		
			doNew();
		}

		private void txtNot_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			var sOld = host.UI.txtCch.Text;
			host.UI.txtCch.Text = host.UI.txtNot.Text.Length.ToString();
		
			//alert(sOld);
			if( sOld == "" ) return;
			if( sOld == host.UI.txtCch.Text ) return;
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Visible;
			host.UI.btnSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSend.Visibility = System.Windows.Visibility.Visible;
		}

		private void txtTit_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if( host.UI.txtTit.Text == sTitOld )
			{
				return;
			}
			if( host.UI.txtTit.Text == "" ) return;
			sTitOld = host.UI.txtTit.Text;
		
			sLstLoadTitle = "";
		
			host.UI.btnSave.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSave.Visibility = System.Windows.Visibility.Visible;
			host.UI.btnSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.lblSend.Visibility = System.Windows.Visibility.Visible;
			host.UI.btnDel.Visibility = System.Windows.Visibility.Collapsed;
			host.UI.lblDel.Visibility = System.Windows.Visibility.Collapsed;
		}

		void listKeys()
		{
			var coll = host.createCollection();
			
			RscStore store = new RscStore();
								
			string[] fles = RscSort.OrderBy(store.GetFileNames(csDocFolder, "*.txt"));
			foreach( string sFle in fles )
			{
				/*
				string strFileGroup = RscRegFs.GetFileGroup( RscFs.ExtensionOfPath(sFle) );
				switch( strFileGroup )
				{
					case "Text" :
					{
				*/
						var item= host.createBindableObject();
						
						item.ObjectA = RscStore.FileNameOfPath( sFle );
						item.ObjectB = csDocFolder + "\\" + sFle;
						
						coll.Add(item);
				
				/*
						break;
					}
				}
				*/
			}
		
			host.UI.lbTit.ItemsSource = coll;
			host.UI.lbTit.DisplayMemberPath = "ObjectA";
		}
		
    }
	
}
