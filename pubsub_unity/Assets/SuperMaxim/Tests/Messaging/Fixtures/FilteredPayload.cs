namespace SuperMaxim.Tests.Messaging.Fixtures
{
    public class FilteredPayload : MessengerTestPayload<bool>
    {
        public bool IsFilterOn { get; set; }
    }
}
