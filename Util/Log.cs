using System;

namespace Nipster.Util
{
    public class Log
    {
        private readonly Type _type;

        public Log(Type type)
        {
            _type = type;
        }

        public void Info(string message)
        {
            Console.WriteLine($"[{_type.Name}] {message}");
        }
    }
}