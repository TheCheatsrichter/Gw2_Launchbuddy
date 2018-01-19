using CrashReporterDotNET;
using System;
using System.Linq;
using System.Net.Mail;

namespace Gw2_Launchbuddy
{
    public static class CrashReporter
    {
        public static System.Net.Mail.MailAddress[] emails = new System.Net.Mail.MailAddress[]
        {
            new MailAddress("gw2launchbuddy@gmx.at","TheCheatsrichter"),
            new MailAddress("launchbuddy@kairubyte.mailclark.ai","KairuByte"),
        };

        public static void TestReportSingle(string name)
        {
            try
            {
                throw new System.Exception("This is a test exception report");
            }
            catch (Exception err)
            {
                ReportCrashToSingle(err, name);
            }
        }

        public static void TestReportAll()
        {
            try
            {
                throw new System.Exception("This is a test exception report");
            }
            catch (Exception err)
            {
                ReportCrashToAll(err);
            }
        }

        public static void ReportCrashToSingle(Exception err, string targetname)
        {
            ReportCrash reportCrash = new ReportCrash(emails.FirstOrDefault(a => a.DisplayName == targetname).Address);

            reportCrash.Send(err);
        }

        public static void ReportCrashToAll(Exception err)
        {
            ReportCrash reportCrash = new ReportCrash(null);

            foreach (MailAddress email in emails)
            {
                reportCrash.ToEmail = email.Address;
                reportCrash.Send(err);
            }
        }
    }
}