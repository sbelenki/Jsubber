using System;
using System.Collections.Generic;
using System.Text;

namespace Jsubber
{
    public class JobManager{
        private bool m_bIsRunning = false;
        private LinkedList<IJob> m_xJobs = new LinkedList<IJob>();

        public bool IsRunning {
            get { return m_bIsRunning; }
        }

        #region Propagate JobStatus Change
        public delegate void JobManagerEventHandler(Object sender, IJob.JobEventArgs args);
        public event JobManagerEventHandler JobStatusMsg;
        protected virtual void OnJobStatusMsg(IJob.JobEventArgs e) {
            if (JobStatusMsg != null) {
                JobStatusMsg(this, e);
            }
        } 
        #endregion

        public void LoadJobs(){
            m_xJobs.Clear();
            m_xJobs.AddFirst(new WorkopolisJob());
            foreach(IJob job in m_xJobs)
                job.JobStatusMsg += new IJob.JobStatusEventHandler(job_JobStatusMsg);
        }

        void job_JobStatusMsg(object sender, IJob.JobEventArgs args) {
            OnJobStatusMsg(new IJob.JobEventArgs(args.jobName, args.jobStatus));
        }

        public void ExecuteJobs(){
            m_bIsRunning = true;
            foreach (IJob job in m_xJobs) {
                job.Execute();
            }
            m_bIsRunning = false;
        }

        public void StopJobs(){
            if (m_bIsRunning) {
                m_bIsRunning = false;
            }
        }
    }
}
