using Microsoft.Phone.Scheduler;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Device.Location;
using Microsoft.Phone.Shell;
using System;
using System.Threading;

namespace Launcher_Agent
{
	
    public class TaskScheduler : ScheduledTaskAgent
    {
		
        protected override void OnInvoke(ScheduledTask task)
        {
            DoTileUpdates(null);    

            TimeSpan waitToSyncThreads = TimeSpan.FromSeconds(8);
            if (System.Diagnostics.Debugger.IsAttached)
                waitToSyncThreads = TimeSpan.FromSeconds(30);

            if (locationStatus == TaskHelperCompletionStatus.Blocked &&
                updaterStatus == TaskHelperCompletionStatus.Blocked)
                this.Abort();
            else
                this.NotifyComplete();
        }

        void DoTileUpdates(object ununsed)
        {

            TaskProgressTileUpdater updater = new TaskProgressTileUpdater();             
            updater.Completed += new EventHandler<TaskHelperEventArgs>(updater_Completed);            
            updater.Start();
        }

        WaitHandle[] runningHelperHandles;
        IBackgroundTaskHelper[] runningHelpers;

        TaskHelperCompletionStatus locationStatus = TaskHelperCompletionStatus.Started;
        TaskHelperCompletionStatus updaterStatus = TaskHelperCompletionStatus.Started;
        void updater_Completed(object sender, TaskHelperEventArgs e)
        {
            // throw new NotImplementedException();
            updaterStatus = e.Status;
            System.Diagnostics.Debug.WriteLine("Updater completed " + e.Status.ToString() + " " + DateTime.Now.ToString());

        }

        void locationService_Completed(object sender, TaskHelperEventArgs e)
        {
            locationStatus = e.Status;
            if (runningHelperHandles.Length > 0 && runningHelperHandles[(int)TaskOrder.Location] != null)
            {
                AutoResetEvent evt = runningHelperHandles[(int)TaskOrder.Location] as AutoResetEvent;
                if (evt != null)
                    evt.Set();
            }
            System.Diagnostics.Debug.WriteLine("Location completed " + e.Status.ToString() + " " + DateTime.Now.ToString());
        }

        enum TaskOrder : int
        {
            Location,
            TileUpdates,
            End = 1
        };
		
    }
	
}