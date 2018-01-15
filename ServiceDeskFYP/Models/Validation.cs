using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceDeskFYP.Models
{
    public class ValidationHelpers
    {
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
    }
}