using System;
using System.Collections.Generic;
using System.Text;

namespace Jsubber {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("JsubberConsole started at '" + DateTime.Now.ToLongDateString() +
                " " + DateTime.Now.ToLongTimeString() + "'");
            JobManager m_xManager = new JobManager();
            m_xManager.JobStatusMsg += new JobManager.JobManagerEventHandler(JobStatusMsg);
            m_xManager.LoadJobs();
            m_xManager.ExecuteJobs();
            Console.WriteLine("JsubberConsole ended at '" + DateTime.Now.ToLongDateString() +
                " " + DateTime.Now.ToLongTimeString() + "'");
        }

        static private void JobStatusMsg(
            Object sender, IJob.JobEventArgs e) {
            StringBuilder sbMessage = new StringBuilder();
            sbMessage.Append(e.jobName);
            sbMessage.Append("\t\t");
            sbMessage.Append(e.jobStatus.ToString());
            Console.WriteLine(sbMessage.ToString());
        }
    }
}
