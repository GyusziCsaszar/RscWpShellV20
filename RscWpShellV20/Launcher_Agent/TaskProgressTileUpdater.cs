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

using Launcher_Lib;

namespace Launcher_Agent
{

    public class TaskProgressTileUpdater : IBackgroundTaskHelper 
    {

        public event EventHandler<TaskHelperEventArgs> Completed;

        public void Start()
        {
            DoWork();   
        }

        public void DoWork()
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
						st.DoUpdate( true, null );
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
