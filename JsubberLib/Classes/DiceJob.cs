using System;
using System.Collections.Generic;
using System.Text;
using IEDriverLib;

namespace Jsubber {
    class DiceJob : IJob {

        const string C_sAdditionalInfo1 = null;

        const string C_sAdditionalInfo2 =
            "* C/C++\r\n" +
            "* MCSD .NET\r\n" +
            "* OOA.";

        public DiceJob() {
            JobName = this.GetType().ToString();
        }

        public override bool OpenWebsite() {
            driver = new IEDriver();
            if (null == driver)
                return false;
            return driver.Navigate("https://www.dice.com/dashboard/login#/profile");
        }
        public override bool Login() {
            bool bUser = driver.DoesElementWithAttributeValueExist("input", "name", "SJT_USER_NAME");
            bool bPassword = driver.DoesElementWithAttributeValueExist("input", "name", "SJT_PASSWD");
            if (bUser && bPassword) {
                driver.SetInputWithNameStringValue("SJT_USER_NAME", MY_USER_NAME);
                driver.SetInputWithNameStringValue("SJT_PASSWD", MY_PASSWORD);
                driver.ClickInput("SUBMIT");
            } else {
                return false; // no user or password element exists
            }
            return true;
        }
        public override bool SelectResumePage() {
            return true;
        }
        public override bool UpdateFirstTime() {
            return UpdateResume();
        }
        public override bool UpdateSecondTime() {
            return UpdateResume();
        }
        public override bool Logout() {
            return driver.ClickAnchorWithValue("Logout");
        }

        private bool UpdateResume() {
            if(!driver.ClickAnchorWithValue("edit"))
                return false;
            driver.ClickAnchorWithAttributeValue("href",
                "https://www.dice.com/dashboard#work-experience-anchor"); // Edit Contact Info

            mshtml.IHTMLElement element = driver.GetElementByAttributeValue("textarea", "name", "CONTACT_EXTRA_CRAP");
            mshtml.HTMLTextAreaElementClass txtAddInfo = null;
            if (null != element) {
                txtAddInfo = (mshtml.HTMLTextAreaElementClass)element;
            } else
                return false;
            if (C_sAdditionalInfo1 == txtAddInfo.value) {
                txtAddInfo.value = C_sAdditionalInfo2;
            } else if (C_sAdditionalInfo2 == txtAddInfo.value) {
                txtAddInfo.value = C_sAdditionalInfo1;
            } else {
                return false;
            }
            driver.ClickInput("CONTINUE");
            driver.ClickInput("FINISH");

            return true;
        }
    }
}
