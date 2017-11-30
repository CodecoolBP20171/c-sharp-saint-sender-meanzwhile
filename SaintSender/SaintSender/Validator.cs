using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaintSender
{
    public class Validator
    {
        public static bool ValidateEmailAddress(string inputEmail)
        {
            string userName = @"[0-9a-z\.-]+";
            string domain = @"([0-9a-z-] +\.)+";
            string tld = "[a-z]{2,4}";
            string pattern = "^" + userName + "@" + domain + tld + "$";
            string patternGood = @"^[0-9a-z\.-]+@([0-9a-z-]+\.)+[a-z]{2,4}$";
            return Regex.IsMatch(inputEmail, patternGood);
        }
    }
}
