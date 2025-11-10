using System.Runtime.CompilerServices;

namespace DeskAssistant.Helpers
{
    internal interface ILoggerHelper
    {

        public void LogEnteringTheMethod([CallerMemberName] string methodName = "");

        public void LogExitingTheMethod([CallerMemberName] string methodName = "");

    }
}
