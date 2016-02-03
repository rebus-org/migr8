using System;
using System.Threading;
using Migr8.Internals;

namespace Migr8.Test.Basic
{
    class ThreadPrintingConsoleWriter : IWriter
    {
        public void Info(string text)
        {
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] INFO: {text}");
        }

        public void Verbose(string text)
        {
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] VERBOSE: {text}");
        }
    }
}