using System;

namespace Migr8.Internals
{
    class LogActionWriter : IWriter
    {
        readonly Action<string> _logAction;
        readonly Action<string> _detailedLogAction;

        public LogActionWriter(Action<string> logAction, Action<string> detailedLogAction)
        {
            _logAction = logAction;
            _detailedLogAction = detailedLogAction;
        }

        public void Info(string text)
        {
            _logAction(text);
        }

        public void Verbose(string text)
        {
            _detailedLogAction(text);
        }
    }
}