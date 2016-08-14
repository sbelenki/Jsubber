using System;
using System.Collections.Generic;
using System.Text;
using IEDriverLib;

namespace Jsubber
{
    public enum JobStatus {
        Idle,
        Waiting,
        Processing,
        Completed,
        Error
    }

    public enum JobStatusErrorReason {
        None,
        OpenWebsite,
        Login,
        SelectResumePage,
        UpdateFirstTime,
        UpdateSecondTime,
        Logout
    }

    static public class JobStatusToString {
        static public string Convert(JobStatus status) {
            switch (status) {
                case JobStatus.Idle: return "Idle";
                case JobStatus.Waiting: return "Waiting";
                case JobStatus.Processing: return "Processing";
                case JobStatus.Completed: return "Completed";
                case JobStatus.Error: return "Error";
                default: return "Undefined";
            }
        }
    }

    class WrongStatusException : Exception {
        public WrongStatusException(int status) {
            System.Diagnostics.Debug.WriteLine(
                "WrongStatusException: " + status.ToString());
        }
    }

    public abstract class IJob {

        protected IEDriver driver = null;
        private JobStatus m_xJobStatus = JobStatus.Idle;
        private JobStatusErrorReason m_xError = JobStatusErrorReason.None;
        private string m_sJobName;
        bool m_bUpdateOk = true;

        #region JobStatus Change
        public class JobEventArgs : EventArgs {
            public JobEventArgs(
                String jobName, JobStatus jobStatus) {
                this.jobName = jobName;
                this.jobStatus = jobStatus;
            }
            public readonly String jobName;
            public readonly JobStatus jobStatus;
        }

        public delegate void JobStatusEventHandler(Object sender, JobEventArgs args);
        public event JobStatusEventHandler JobStatusMsg;
        protected virtual void OnJobStatusMsg(JobEventArgs e) {
            if (JobStatusMsg != null) {
                JobStatusMsg(this, e);
            }
        }
        #endregion

        // command and template method patterns combined
        public virtual void Execute() {
            SetStatus(JobStatus.Processing);
            m_bUpdateOk = OpenWebsite();
            if (m_bUpdateOk)
                m_bUpdateOk = Login();
            else {
                SetError(JobStatusErrorReason.OpenWebsite);
                CloseWebsite();
                return;
            }
            if (m_bUpdateOk)
                m_bUpdateOk = SelectResumePage();
            else {
                SetError(JobStatusErrorReason.SelectResumePage);
                CloseWebsite();
                return;
            }
            if (m_bUpdateOk)
                m_bUpdateOk = UpdateFirstTime();
            else {
                SetError(JobStatusErrorReason.Login);
                CloseWebsite();
                return;
            }
            if (m_bUpdateOk)
                m_bUpdateOk = UpdateSecondTime();
            else {
                SetError(JobStatusErrorReason.UpdateFirstTime);
                CloseWebsite();
                return;
            }
            if (m_bUpdateOk)
                m_bUpdateOk = Logout();
            else {
                SetError(JobStatusErrorReason.UpdateSecondTime);
                CloseWebsite();
                return;
            }
            if (m_bUpdateOk)
                SetStatus(JobStatus.Completed);
            else {
                SetError(JobStatusErrorReason.Logout);
            }
            CloseWebsite();
        }

        public abstract bool OpenWebsite();
        public abstract bool Login();
        public abstract bool SelectResumePage();
        public abstract bool UpdateFirstTime();
        public abstract bool UpdateSecondTime();
        public abstract bool Logout();

        public virtual void CloseWebsite() {
            driver.Stop();
            driver.Quit();
            driver = null;
        }

        public JobStatus JobStatus {
            get { return m_xJobStatus; }
        }

        public string JobName {
            set { m_sJobName = value; }
            get { return m_sJobName; }
        }

        public JobStatusErrorReason JobStatusErrorReason {
            get { return m_xError; }
        }

    #region Helpers
        private void SetStatus(JobStatus status) {
            m_xJobStatus = status;
            OnJobStatusMsg(new JobEventArgs(this.m_sJobName, this.m_xJobStatus));
        }
        private void SetError(JobStatusErrorReason reason) {
            SetStatus(JobStatus.Error);
            m_xError = reason;
        }
	#endregion
    }
}
