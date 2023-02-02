using SuperMaxim.Core.Logging;
using SuperMaxim.Messaging.API;

namespace SuperMaxim.Tests.Messaging
{
    public abstract class BaseMessengerTest
    {
        protected readonly IMessenger Messenger;

        protected readonly ILogger Logger;

        protected BaseMessengerTest()
        {
            Logger = Loggers.Console;
            Messenger = SuperMaxim.Messaging.Messenger.Default;
        }
    }
}
