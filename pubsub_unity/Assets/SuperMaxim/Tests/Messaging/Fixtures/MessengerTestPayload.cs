namespace SuperMaxim.Tests.Messaging.Fixtures
{
    public class MessengerTestPayload<T>
    {
        public int Id { get; set; }

        public int CallbackCount { get; set; }

        public T Data { get; set; }
    }
}
