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

namespace RscXtests
{
	
    public partial class RscTpVirtualListBoxV10 : PhoneApplicationPage
    {
		
        public RscTpVirtualListBoxV10()
        {
            InitializeComponent();
 			
			ItemList.ItemsSource = new VirtualSongList10();
       }
		
    }
	
	// //
	// SRC: http://shawnoster.com/2010/08/improving-listbox-performance-in-silverlight-for-windows-phone-7-data-virtualization/
	
	public class Song10
	{
		public string Title { get; set; }
		public string Length { get; set; }
	}
	
    public class VirtualSongList10 : IList<Song10>, IList
    {
        #region IList<Song10> Members

        public int IndexOf(Song10 item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Song10 item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public Song10 this[int index]
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

        #region ICollection<Song10> Members

        public void Add(Song10 item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Song10 item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Song10[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return the total number of items in your list.
        /// </summary>
        public int Count
        {
            get
            {
                return 10000;
            }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(Song10 item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<Song10> Members

        public IEnumerator<Song10> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

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
                return new Song10() { Title = "Song " + index.ToString() };
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
    }
	
	// SRC: http://shawnoster.com/2010/08/improving-listbox-performance-in-silverlight-for-windows-phone-7-data-virtualization/
	// //
	
}