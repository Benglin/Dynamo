#if ENABLE_DYNAMO_SCHEDULER

using System;

namespace Dynamo.Core.Threading
{
    internal struct DelegateBasedParams
    {
        internal DynamoScheduler DynamoScheduler { get; set; }
        internal AsyncTask.TaskPriority TaskPriority { get; set; }
        internal Action ActionToPerform { get; set; }
    }

    /// <summary>
    /// DelegateBasedAsyncTask allows for a delegate or System.Action object 
    /// to be scheduled for asynchronous execution on the ISchedulerThread. A 
    /// DelegateBasedAsyncTask has a default priority of TaskPriority.Normal, 
    /// which can be overwritten to other priority values through construction.
    /// </summary>
    /// 
    internal class DelegateBasedAsyncTask : AsyncTask
    {
        private readonly Action actionToPerform;
        private readonly TaskPriority assignedPriority = TaskPriority.Normal;

        internal override TaskPriority Priority
        {
            get { return assignedPriority; }
        }

        #region Public Class Operational Methods

        internal DelegateBasedAsyncTask(DelegateBasedParams initParams)
            : base(initParams.DynamoScheduler)
        {
            if (initParams.ActionToPerform == null)
                throw new ArgumentNullException("initParams.ActionToPerform");

            actionToPerform = initParams.ActionToPerform;
            assignedPriority = initParams.TaskPriority;
        }

        #endregion

        #region Protected Overridable Methods

        protected override void HandleTaskExecutionCore()
        {
            actionToPerform();
        }

        protected override void HandleTaskCompletionCore()
        {
            // Does nothing here after invocation of the Action.
        }

        #endregion
    }
}

#endif
