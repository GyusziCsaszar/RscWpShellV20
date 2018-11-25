using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Shell;
using System.Linq; 

using RscGoogleApiMail_Lib;

namespace RscGoogleApiMail_Agent
{
	
	class AppLogicTileUpdate : AppLogic
	{
		
		public int m_iNew = -700;
		
		System.Threading.ManualResetEvent m_mre;
		
		public AppLogicTileUpdate( System.Threading.ManualResetEvent mre )
		{
			m_mre = mre;
		}
		
		protected override void OnDone( int iNew )
		{
			m_iNew = iNew;
			m_mre.Set();
		}
	}

    public class TaskProgressTileUpdater : IBackgroundTaskHelper 
    {

        public event EventHandler<TaskHelperEventArgs> Completed;

        public void Start()
        {			
			int iNew = -700;
			
			try
			{
				System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false);
				
				AppLogicTileUpdate altu = null;
				
				Deployment.Current.Dispatcher.BeginInvoke(() =>
            	{
					try
					{
						altu = new AppLogicTileUpdate( mre );
						int iRet = altu.ReadThreadData( );
						
						if( iRet < 0 ) //Something wrong...
						{
							altu.m_iNew = iRet;
							mre.Set();
						}
					}
					catch( Exception )
					{
						//NOP...
					}
				});

				// Wait for Lock Screen image to complete
				mre.WaitOne();
				// Then reset for the Tile Image operation
				mre.Reset();
				
				iNew = altu.m_iNew;
			}
			catch( Exception )
			{
			}
			
			DoWork( iNew );
        }

        public void DoWork( int iNew )
        {

            try
            {
				// //
				//
				
				System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false);
				
				Deployment.Current.Dispatcher.BeginInvoke(() =>
            	{
					try
					{
						SysTiles st = new SysTiles();
						st.DoUpdate(true, iNew);
					}
					catch( Exception )
					{
						//NOP...
					}
					
					mre.Set();
				});

				// Wait for Lock Screen image to complete
				mre.WaitOne();
				// Then reset for the Tile Image operation
				mre.Reset();
				
				//
				// //
				
                Status = TaskHelperCompletionStatus.Completed;
                
            }
            catch (Exception)
            {
                Status = TaskHelperCompletionStatus.Failed; 
            }
			
            NotifyComplete();
        }

        private void NotifyComplete()
        {
            EventHandler<TaskHelperEventArgs> eh = Completed;
            if (eh != null)
                eh(this, new TaskHelperEventArgs(Status));
        }

        public void Stop()
        {
            Status = TaskHelperCompletionStatus.TimedOut;
            NotifyComplete();
        }

        public int ProgressCompleted
        {
            get;
            private set;
        }

        public TaskHelperCompletionStatus Status
        {
            get;
            private set;
        }

    }
}
