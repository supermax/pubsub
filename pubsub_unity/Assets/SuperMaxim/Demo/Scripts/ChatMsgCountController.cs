#region

using SuperMaxim.Logging;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class ChatMsgCountController : MonoBehaviour
{
    [SerializeField] private Text _counterText;

    private int _msgCount;

    private void Awake()
    {
        Debug.AssertFormat(_counterText != null, "{0} is not assigned", nameof(_counterText));
    }

    private void Start()
    {
        Messenger.Default
            .Subscribe<ChatPayload>(OnChatMessageReceived, ChatMessageFilter);
    }

    private bool ChatMessageFilter(ChatPayload payload)
    {
        var accepted = payload != null && _counterText != null;
        return accepted;
    }

    private void OnDestroy()
    {
        Loggers.Console.LogInfo("{0} destroyed", nameof(ChatMsgCountController));
    }

    private void OnChatMessageReceived(ChatPayload payload)
    {
        Loggers.Console.LogInfo("Received: {0}", payload);
        _counterText.text = $"Message Count: {++_msgCount}";
    }

    public void KillMe()
    {
        Loggers.Console.LogInfo("Killing {0}", gameObject);
        Destroy(gameObject);
    }
}