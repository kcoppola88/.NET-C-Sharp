using System;

/// <summary>
/// Captures application errors and sends information to Error Log endpoints
/// </summary>
public sealed class ExceptionUtility
{

    private ExceptionUtility() { }

    public static void LogException(Exception exc, string userName)
    {
        //get IP if unknown user
        string ipAddress = "";
        if (userName == null || userName == string.Empty)
        {
            ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    ipAddress = addresses[0];
                }
            }
            else
            {
                ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            userName = "Unauthenticated user: " + ipAddress;
        }
        else
        {
            userName = HttpContext.Current.User.Identity.Name;
        }
        string logFile = "~\\App_Data\\ErrorLog.txt";
        logFile = HttpContext.Current.Server.MapPath(logFile);
        StreamWriter sw = new StreamWriter(logFile, true);
        sw.WriteLine("**********{0}**********", DateTime.Now);
        if (exc.InnerException != null)
        {
            sw.Write("Inner Exception Type: ");
            sw.WriteLine(exc.InnerException.GetType().ToString());
            sw.Write("Inner Exception: ");
            sw.WriteLine(exc.InnerException.Message);
            sw.Write("Inner Source: ");
            sw.WriteLine(exc.InnerException.Source);
            if (exc.InnerException.StackTrace != null)
            {
                sw.WriteLine("Inner Stack Trace: ");
                sw.WriteLine(exc.InnerException.StackTrace);
            }
        }
        sw.Write("Username was: ");
        sw.WriteLine(userName);
        sw.Write("Exception Type: ");
        sw.WriteLine(exc.GetType().ToString());
        sw.WriteLine("Exception: " + exc.Message);
        sw.WriteLine("Stack Trace: ");
        if (exc.StackTrace != null)
        {
            sw.WriteLine(exc.StackTrace);
            sw.WriteLine();
        }
        sw.Close();
    }

    public static void NotifyAdmin(Exception exc, string userName)
    {
        //get IP if unknown user
        string ipAddress = "";
        if (userName == null || userName == string.Empty)
        {
            ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    ipAddress = addresses[0];
                }
            }
            else
            {
                ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            userName = "Unauthenticated user: " + ipAddress;
        }
        else
        {
            userName = HttpContext.Current.User.Identity.Name;
        }

        MailManager mm = new MailManager();
        mm.SendErrorEmail("[insert email here]", exc.ToString(), userName);
    }
}
