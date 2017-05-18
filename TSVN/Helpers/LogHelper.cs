using System;

namespace SamirBoulema.TSVN.Helpers
{

    public static class LogHelper
    {
        public static void Initialize(IServiceProvider serviceProvider, string name)
        {
            Logger.Initialize(serviceProvider, name);
        }

        public static void Log(string message)
        {
        #if DEBUG
            Logger.Log(message);
        #endif
        }
    }

}
