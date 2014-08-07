using System;
using System.Collections.Generic;
using System.Text;

namespace Mvvm
{
    public static class StackTraceHelper
    {
        public static string Get()
        {
#if UNIVERSAL || WINDOWS_PHONE
            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {
                return ex.StackTrace;
            }
#else
            return Environment.StackTrace;
#endif
        }
    }
}
