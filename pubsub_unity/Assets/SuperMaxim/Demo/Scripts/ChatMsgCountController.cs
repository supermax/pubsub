#region

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
        Messenger.Default.Subscribe<ChatPayload>(OnChatMessageReceived, ChatMessageFilter);
    }

    private bool ChatMessageFilter(ChatPayload payload)
    {
        var accepted = payload != null && _counterText != null;
        return accepted;
    }

    private void OnDestroy()
    {
        Debug.LogFormat("{0} destroyed", nameof(ChatMsgCountController));
    }

    private void OnChatMessageReceived(ChatPayload payload)
    {
        Debug.LogFormat("Received: {0}", payload);
        _counterText.text = $"Message Count: {++_msgCount}";
    }

    public void KillMe()
    {
        Debug.LogFormat("Killing {0}", gameObject);
        Destroy(gameObject);
    }
}