using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Jsubber
{
    public partial class frmMain : Form
    {
        JobManager m_xManager;

        public frmMain()
        {
            InitializeComponent();

            ListBox.CheckForIllegalCrossThreadCalls = false;
            m_xManager = new JobManager();
            m_xManager.JobStatusMsg += new JobManager.JobManagerEventHandler(JobStatusMsg);
            m_xManager.LoadJobs();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Thread workerThread = new Thread(delegate() {
                m_xManager.ExecuteJobs();
            });
            workerThread.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            m_xManager.StopJobs();
        }

        private void btnExit_Click(object sender, EventArgs e) {
            m_xManager.StopJobs();
            this.Close();
        }

        private void JobStatusMsg(
            Object sender, IJob.JobEventArgs e) {
            StringBuilder sbMessage = new StringBuilder();
            sbMessage.Append(e.jobName);
            sbMessage.Append("\t\t");
            sbMessage.Append(e.jobStatus.ToString());
            lbLogger.Items.Add(sbMessage.ToString());
        }
    }
}