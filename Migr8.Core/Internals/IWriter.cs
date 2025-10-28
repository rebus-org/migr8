namespace Migr8.Internals
{
    public interface IWriter
    {
        void Info(string text);
        void Verbose(string text);
    }
}