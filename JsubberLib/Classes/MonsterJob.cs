using System;
using System.Collections.Generic;
using System.Text;
using IEDriverLib;
using Config = System.Configuration.ConfigurationManager;

namespace Jsubber {
    class MonsterJob : IJob {

        static string C_sFirstObjective = Config.AppSettings["MonsterObjective_1"];
        static string C_sSecondObjective = Config.AppSettings["MonsterObjective_2"];

        public MonsterJob() {
            JobName = this.GetType().ToString();
        }

        public override bool OpenWebsite() {
            driver = new IEDriver();
            if (null == driver)
                return false;
            return driver.Navigate("http://my.monster.ca/login.aspx");
        }
        public override bool Login() {
            if (driver.DoesElementExist("BodyContent_MyLogin_txtUsername") &&
                driver.DoesElementExist("BodyContent_MyLogin_txtPassword")) {
                driver.SetInputStringValue("BodyContent_MyLogin_txtUsername", MY_USER_NAME);
                driver.SetInputStringValue("BodyContent_MyLogin_txtPassword", MY_PASSWORD);
                driver.ClickAnchor("BodyContent_MyLogin_monsLogin");
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
            return driver.ClickAnchor("headerContent_header1__ctl0_hlLogout"); // Logout
        }

        private bool UpdateResume() {
            if (!driver.ClickAnchorWithValue(MY_RESUME_TITLE))
                return false;
            driver.ClickAnchor("BodyContent_Objective_lnkAddEdit"); // Objective Edit
            mshtml.IHTMLElement element = driver.GetElementById("ResumeBodyContent_ObjectiveEdit1_Objective");
            mshtml.HTMLTextAreaElementClass txtObjective = (mshtml.HTMLTextAreaElementClass)element;

            if (C_sFirstObjective == txtObjective.value) {
                txtObjective.value = C_sSecondObjective;
            } else if (C_sSecondObjective == txtObjective.value) {
                txtObjective.value = C_sFirstObjective;
            } else {
                return false;
            }
            driver.ClickInput("ResumeBodyContent_ObjectiveEdit1_Submit"); // finish Objective editing
            driver.ClickInput("BodyContent_btnFinishedBottom"); // finish resume editing

            return true;
        }
    }
}
