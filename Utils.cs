using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    public static class Utils
    {
        public static int ParseInt32(string s)
        {
            bool isHex;
            var s_number = s;
            if (isHex = s_number.StartsWith("0x"))
                s_number = s_number.Substring(2);
            s_number = s_number.Split('(')[0].Trim();
            if (s_number.Contains(' '))
                s_number = s_number.Split(' ')[0];
            var flags = isHex ? System.Globalization.NumberStyles.AllowHexSpecifier : NumberStyles.None;
            int number = int.Parse(s_number, flags, CultureInfo.InvariantCulture);
            return number;
        }

        public static Int64 ParseInt64(string s)
        {
            bool isHex;
            var s_number = s;
            if (isHex = s_number.StartsWith("0x"))
                s_number = s_number.Substring(2);
            s_number = s_number.Split('(')[0].Trim();
            if (s_number.Contains(' '))
                s_number = s_number.Split(' ')[0];
            var flags = isHex ? System.Globalization.NumberStyles.AllowHexSpecifier : NumberStyles.None;
            Int64 number = Int64.Parse(s_number, flags, CultureInfo.InvariantCulture);
            return number;
        }

        public static uint ParseUInt32(string s)
        {
            bool isHex;
            var s_number = s;
            if (isHex = s_number.StartsWith("0x"))
                s_number = s_number.Substring(2);
            s_number = s_number.Split('(')[0].Trim();
            if (s_number.Contains(' '))
                s_number = s_number.Split(' ')[0];
            var flags = isHex ? System.Globalization.NumberStyles.AllowHexSpecifier : NumberStyles.None;
            uint number = uint.Parse(s_number, flags, CultureInfo.InvariantCulture);
            return number;
        }

        public static UInt64 ParseUInt64(string s)
        {
            bool isHex;
            var s_number = s;
            if (isHex = s_number.StartsWith("0x"))
                s_number = s_number.Substring(2);
            s_number = s_number.Split('(')[0].Trim();
            if (s_number.Contains(' '))
                s_number = s_number.Split(' ')[0];
            var flags = isHex ? System.Globalization.NumberStyles.AllowHexSpecifier : NumberStyles.None;
            UInt64 number = UInt64.Parse(s_number, flags, CultureInfo.InvariantCulture);
            return number;
        }
    }
}
