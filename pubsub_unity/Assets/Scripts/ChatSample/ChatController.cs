using System;
using SuperMaxim.Core.Extensions;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    [SerializeField]
    private string _userId;

    [SerializeField]
    private string _text;

    [SerializeField]
    private InputField _inputField;

    [SerializeField]
    private Text _chatText;

    [SerializeField]
    private Button _sendButton;

    [SerializeField]
    private Text _userIdText;

    private const int MaxChatTextLength = 3000;

    private void Start()
    {
        _userIdText.text = _userId;

        Messenger.Default.Subscribe<ChatPayload>(OnChatMessage, ChatMessagePredicate);
    }

    private bool ChatMessagePredicate(ChatPayload payload)
    {
        var isSameId = payload.UserId == _userId;
        if(isSameId)
        {
            return false;
        }

        var isEmpty = payload.Text.IsNullOrEmpty();
        return !isEmpty;
    }

    private void OnChatMessage(ChatPayload payload)
    {
        if(_chatText.text.Length > MaxChatTextLength)
        {
            _chatText.text = string.Empty;
        }

        _chatText.text += string.Format("\r\n{0:t} {1}: {2}", 
                                    DateTime.Now, payload.UserId, payload.Text);
    }

    public void OnTextChanged(string text)
    {
        _text = text;

        _sendButton.enabled = !_inputField.text.IsNullOrEmpty();
    }

    public void OnTextEndEdit(string text)
    {
        if(text.IsNullOrEmpty())
        {
            return;
        }

        var payload = new ChatPayload 
                            {    
                                UserId = _userId,
                                Text = text
                            };
        Messenger.Default.Publish(payload);

        _inputField.enabled = false;
        _inputField.text = string.Empty;
        _inputField.enabled = true;
    }

    public void KillMe()
    {
        GameObject.Destroy(gameObject);
        Debug.LogFormat("Killing {0}", gameObject);
    }

    private void OnDestroy()
    {
        Debug.LogFormat("{0} destoyed", this);
    }
}
