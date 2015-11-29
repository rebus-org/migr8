using System;

namespace Migr8.Internals
{
    class LogActionWriter : IWriter
    {
        readonly Action<string> _logAction;

        public LogActionWriter(Action<string> logAction)
        {
            _logAction = logAction;
        }

        public void Write(string text)
        {
            _logAction(text);
        }
    }
}