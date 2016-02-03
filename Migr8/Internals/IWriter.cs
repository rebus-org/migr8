namespace Migr8.Internals
{
    interface IWriter
    {
        void Info(string text);
        void Verbose(string text);
    }
}