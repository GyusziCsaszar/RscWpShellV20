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
using System.Windows.Media;

namespace RscXtests
{
	
    public partial class RscTpVirtualListBoxV11 : PhoneApplicationPage
    {
		
		VirtualSongList11 vs;
		
        public RscTpVirtualListBoxV11()
        {
            InitializeComponent();
			
			/*
			ItemList.ItemTemplate = CreateDataTemplate();
			*/
			
			ItemList.SelectionChanged +=new System.Windows.Controls.SelectionChangedEventHandler(ItemList_SelectionChanged);
			
			vs = new VirtualSongList11();
			vs.mp = this;
			
			/*
			vs.Add( new Song() { Title = "Item " + vs.Count.ToString(), Length = "00:00:00" } );
			vs.Add( new Song() { Title = "Item " + vs.Count.ToString(), Length = "00:00:00" } );
			vs.Add( new Song() { Title = "Item " + vs.Count.ToString(), Length = "00:00:00" } );
			*/
			
			ItemList.ItemsSource = vs;
        }
		
		/*
		private DataTemplate CreateDataTemplate()
		{
			string sXaml =
				@"<DataTemplate
				xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
					<Grid>
						<Grid.RowDefinitions>
							<RowDefinition Height=""Auto"" />
							<RowDefinition Height=""Auto"" />
							<RowDefinition Height=""*"" />
						</Grid.RowDefinitions>
						<TextBlock Grid.Row=""0"" Text=""{Binding Title}"" FontSize=""32"" />
						<Button Grid.Row=""1"" Content=""{Binding Title}"" Click=""MainPage.btnItem_Click"" />
						<TextBlock Grid.Row=""2"" Text=""{Binding Length}"" FontSize=""16"" />
					</Grid>
				</DataTemplate>";
			DataTemplate dt = (DataTemplate) XamlReader.Load(sXaml);
			return dt;
		}
		*/

		private void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			vs.Add( new Song11() { Title = "Item " + vs.Count.ToString(), Length = "00:00:00" } );
		}
		
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
		
		private void SearchVisualTree(DependencyObject targetElement)
		{
			var count = VisualTreeHelper.GetChildrenCount(targetElement);
			if (count == 0)
				return;
		
			for (int i = 0; i < count; i++)
			{
				var child = VisualTreeHelper.GetChild(targetElement, i);
				if (child is TextBlock)
				{
					TextBlock targetItem = (TextBlock)child;
					
					if (targetItem.Text.IndexOf( "Item" ) == 0)
					{
						targetItem.Foreground = new SolidColorBrush(Colors.Green);
						return;
					}
				}
				else
				{
					SearchVisualTree(child);
				}
			}
		}
		
		private void btnItem_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button btn = (Button) sender;
			Log("Button.Tag=" + btn.Tag.ToString() );
			
			Song11 sng = (Song11) btn.Tag;
			int idx = vs.IndexOf( sng );
			Log("idx=" + idx.ToString() );
			if( ItemList.SelectedIndex != idx )
				ItemList.SelectedIndex = idx;
			
			ListBoxItem item = ItemList.ItemContainerGenerator.ContainerFromIndex( ItemList.SelectedIndex ) as ListBoxItem;
			CheckBox tagregCheckBox = FindFirstElementInVisualTree<CheckBox>(item);
			tagregCheckBox.IsChecked = !tagregCheckBox.IsChecked;
			
			SearchVisualTree( item );
		}
		
		public void Log( string sLog )
		{
			lstLog.Items.Insert(0, sLog);
		}

		private void ItemList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			Log("SelectionChanged");
			
			Log(ItemList.SelectedIndex.ToString());
			
			Log(ItemList.SelectedItem.ToString());
		}
		
    }
	
	// //
	// http://shawnoster.com/2010/08/improving-listbox-performance-in-silverlight-for-windows-phone-7-data-virtualization/
	
	public class Song11
	{
		
		public Song11 This { get { return this; } }
		
		public string Title { get; set; }
		public string Length { get; set; }
	}
	
    public class VirtualSongList11 : ObservableCollection<Song11>, IList<Song11>, IList
    {
		
		public RscTpVirtualListBoxV11 mp = null;
		
		//private List<Song> m_a = new List<Song>();
		
		/*
        #region IList<Song> Members

        public int IndexOf(Song item)
        {
            throw new NotImplementedException();
        }
		*/

		/*
        public void Insert(int index, Song item)
        {
            throw new NotImplementedException();
        }
		*/

		/*
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
		*/

		/*
        public Song this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
		*/

		/*
        #region ICollection<Song> Members

        public void Add(Song item)
        {
        	m_a.Add( item );
        }
		*/

		/*
        public void Clear()
        {
            throw new NotImplementedException();
        }
		*/

		/*
        public bool Contains(Song item)
        {
            throw new NotImplementedException();
        }
		*/

		/*
        public void CopyTo(Song[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
		*/

		/*
        public int Count
        {
            get
            {
				mp.Log("Count: " + m_a.Count.ToString());
                return m_a.Count;
            }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(Song item)
        {
            throw new NotImplementedException();
        }

        #endregion
		*/

		/*
        #region IEnumerable<Song> Members

        public IEnumerator<Song> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
		*/

		/*
        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();

            //for (int i = 0; i < Count; ++i)
            //{
            //    yield return new Song();
            //}
        }

        #endregion
		*/

		/*
        #region IList Members

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        object IList.this[int index]
        {
            get
            {
                // here is where the magic happens, create/load your data on the fly.
				
                //Debug.WriteLine("Requsted item " + index.ToString());
				mp.Log("Requsted item " + index.ToString());
				
                //return new Song() { Title = "Song " + index.ToString() };
				return m_a[ index ];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
		*/
    }
	
	// http://shawnoster.com/2010/08/improving-listbox-performance-in-silverlight-for-windows-phone-7-data-virtualization/
	// //
	
}
