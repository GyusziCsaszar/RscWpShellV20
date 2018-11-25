using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RscGoogleApiMail_Agent
{
	
    public enum TaskHelperCompletionStatus 
    { 
        Started, 
        Completed, 
        Failed, 
        TimedOut, 
        Blocked 
    }; 

    public class TaskHelperEventArgs  : EventArgs 
    {
        public TaskHelperEventArgs(TaskHelperCompletionStatus status)
            : base()
        {
            Status = status;  
        }
        public TaskHelperCompletionStatus Status
        {
            get;
            private set;
        } 
    }
	
    interface IBackgroundTaskHelper
    {
        event EventHandler<TaskHelperEventArgs> Completed;
        void Start();
        void Stop();
        int ProgressCompleted { get; }
        TaskHelperCompletionStatus Status { get; } 
    }
	
}
