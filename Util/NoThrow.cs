using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nipster.Util
{
    namespace Nipster
    {
        public static class NoThrow
        {
            public static string String(Func<dynamic> func)
            {
                try
                {
                    return "" + func();
                }
                catch
                {
                }
                return null;
            }

            public static int Int(Func<dynamic> func)
            {
                try
                {
                    int i;
                    return int.TryParse("" + func(), out i) ? i : 0;
                }
                catch
                {
                }
                return 0;
            }

            public static string[] StringArray(Func<object> func)
            {
                try
                {
                    var array = func() as JArray;
                    return array?.Select(val => "" + val).ToArray();
                }
                catch
                {
                }
                return null;
            }

            public static DateTime? DateTime(Func<object> func)
            {
                try
                {
                    return (DateTime) func();
                }
                catch
                {
                }
                return null;
            }
        }
    }
}