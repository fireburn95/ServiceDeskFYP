using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Web;

namespace ServiceDeskFYP.Models
{
    public class Helpers
    {
        //Capitalise first letter of string and lower case rest
        public static string FirstLetterTOUpper(string str)
        {
            //If empty, return null
            if (String.IsNullOrEmpty(str))
                return null;

            //If greater than 1, capitalise first character and concat with rest of string
            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            //Otherwise, if one character, just return capital of that
            return str.ToUpper();
        }

        public static void LogEvent(string Type, string Detail)
        {
            using (ApplicationDbContext dbContext = new ApplicationDbContext())
            {
                //Create the log
                Log NewLog = new Log()
                {
                    Type = Type,
                    Detail = Detail,
                    Datetime = DateTime.Now,
                    UserId = HttpContext.Current.User.Identity.GetUserId(),
                    LocalIP = LocalIPAddress(),
                    PublicIP = PublicIPAddress(),
                };

                //Add to DB
                dbContext.Log.Add(NewLog);
                dbContext.SaveChanges();
            }
        }

        public static void LogEvent(string Type, string Detail, string UserId)
        {
            using (ApplicationDbContext dbContext = new ApplicationDbContext())
            {
                //Create the log
                Log NewLog = new Log()
                {
                    Type = Type,
                    Detail = Detail,
                    Datetime = DateTime.Now,
                    UserId = UserId,
                    LocalIP = LocalIPAddress(),
                    PublicIP = PublicIPAddress(),
                };

                //Add to DB
                dbContext.Log.Add(NewLog);
                dbContext.SaveChanges();
            }
        }
 
        public static string LocalIPAddress()
        {
            string ipaddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipaddress))
            {
                string[] addresses = ipaddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static string PublicIPAddress()
        {
            string ipaddress = null;

            if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                ipaddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            else if (HttpContext.Current.Request.UserHostAddress.Length != 0)
            {
                ipaddress = HttpContext.Current.Request.UserHostAddress;
            }
            return ipaddress;
        }
    }
}