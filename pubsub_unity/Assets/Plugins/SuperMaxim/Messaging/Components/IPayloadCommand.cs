using System.Collections.Generic;

namespace SuperMaxim.Messaging.Components
{
    public interface IPayloadCommand
    {
        string Id { get; }

        IDictionary<string, string> Data { get; }
    }
}