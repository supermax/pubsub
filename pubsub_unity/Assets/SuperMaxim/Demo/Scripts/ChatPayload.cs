public class ChatPayload
{
    public string UserId { get; set; }

    public string Text { get; set; }

    public override string ToString()
    {
        return $"UserId: {UserId}, Text: {Text}";
    }
}