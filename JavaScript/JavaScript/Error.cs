using System;

namespace Esprima.NET
{
    public class Error : Exception
    {
        public Error(string message)
            : base(message)
        {
        }

        public int index { get; set; }
        public int lineNumber { get; set; }
        public int column { get; set; }
        public string description { get; set; }
        public string message { get; set; }
    }
}