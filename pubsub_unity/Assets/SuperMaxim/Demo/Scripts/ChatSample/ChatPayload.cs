public class ChatPayload
{
    public string UserId { get; set; }

    public string Text { get; set; }

    public override string ToString()
    {
        return string.Format("UserId: {0}, Text: {1}", UserId, Text);
    }
}
