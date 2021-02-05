using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coneckt.Web
{
    public class Log
    {

        public void LogThis(string action, string logMessage)
        {
            var dateTime = DateTime.Now.ToString();
            logMessage = dateTime + " - [Action: " + action + "] " + logMessage;
            //Console.WriteLine(logMessage);
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter("../" + FileDate() + ".txt", true))
            {
                writer.WriteLine(logMessage);
                writer.WriteLine();
            }
        }

        static string FileDate()
        {
            return DateTime.Now.ToString("yyyy") + "-" + DateTime.Now.ToString("MM");
        }
    }
}
