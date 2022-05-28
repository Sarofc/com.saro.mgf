

namespace Saro
{
    public interface ILogger
    {
        void INFO(string message);
        void WARN(string message);
        void ERROR(string message);
        void ERROR(System.Exception exception);
        void Assert(bool condition, string message);
    }
}