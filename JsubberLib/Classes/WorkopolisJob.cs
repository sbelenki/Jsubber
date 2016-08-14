using System;
using System.Collections.Generic;
using System.Text;
using IEDriverLib;

namespace Jsubber
{
    class WorkopolisJob : IJob
    {
        public WorkopolisJob() {
            JobName = this.GetType().ToString();
        }

        public override bool OpenWebsite() {
            driver = new IEDriver();
            if (null == driver)
                return false;
            return driver.Navigate("https://www.workopolis.com/account/en/signin/");
        }
        public override bool Login() {
            if (driver.DoesElementExist("email-username") &&
                driver.DoesElementExist("password")) {
                driver.SetInputStringValue("Email", MY_EMAIL);
                driver.SetInputStringValue("Password", MY_PASSWORD);
                driver.ClickButtonWithValue("Sign In");
            } else {
                return false; // no user or password element exists
            }
            return true;
        }
        public override bool SelectResumePage() {
            if (!driver.ClickAnchorWithAttributeValue("href",
                "http://www.workopolis.com/Default.aspx?action=MyResume&lang=EN")) // Resumes and Cover Letters
            {
                return false;
            }
            return true;
        }
        public override bool UpdateFirstTime() {
            return UpdateResume();
        }
        public override bool UpdateSecondTime() {
            //return UpdateResume();
            return true;
        }
        public override bool Logout() {
            return driver.ClickAnchorWithAttributeValue("href",
                "https://www.workopolis.com/account/en/authentication/delete"); // (Sign out)
        }

        private bool UpdateResume() {

            if (!driver.ClickAnchorWithValue(MY_RESUME_NAME))
                return false;

            if (!driver.ClickButton("previewButton")) {
                return false;
            }

            return driver.ClickButtonWithInnerText("Finish");
        }
    }
}
