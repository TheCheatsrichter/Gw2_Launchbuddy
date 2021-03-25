using CrashReporterDotNET;
using System;
using System.Linq;
using System.Net.Mail;

namespace Gw2_Launchbuddy
{
    public static class CrashReporter
    {

        static private DoctorDumpSettings Settings
        {
            get
            {
                return new DoctorDumpSettings { ApplicationID= new Guid("2b13ec28-f1fe-414c-a320-3f8a25736cc4") };
            }
        }

        public static System.Net.Mail.MailAddress[] emails = new System.Net.Mail.MailAddress[]
        {
            new MailAddress("gw2launchbuddy@gmx.at","TheCheatsrichter")
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
            reportCrash.DoctorDumpSettings = Settings;

            reportCrash.Send(err);
        }

        public static void ReportCrashToAll(Exception err)
        {
            ReportCrash reportCrash = new ReportCrash(null);

            foreach (MailAddress email in emails)
            {
                reportCrash.ToEmail = email.Address;
                reportCrash.DoctorDumpSettings = Settings;
                reportCrash.Send(err);
            }
        }
    }
}