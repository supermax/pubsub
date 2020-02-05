using System;
using System.Threading;
using SuperMaxim.Core.Extensions;
using SuperMaxim.Messaging;
using SuperMaxim.Messaging.Components;
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

    [SerializeField] 
    private bool _isMultithreadingOn;

    private const int MaxChatTextLength = 3000;

    private void Start()
    {
        _userIdText.text = _userId;

        Messenger.Default.
            Subscribe<ChatPayload>(OnChatMessage, ChatMessagePredicate).
            Subscribe<PayloadCommand>(OnPayloadCommand, PayloadCommandPredicate);
    }

    private bool PayloadCommandPredicate(PayloadCommand payload)
    {
        var key = typeof(MultiThreadingToggle).Name;
        var res = payload.Id == key && payload.Data is MultiThreadingToggle;
        return res;
    }

    private void OnPayloadCommand(PayloadCommand payload)
    {
        var data = (MultiThreadingToggle)payload.Data;
        _isMultithreadingOn = data.IsMultiThreadingOn;
        // create new thread
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

        _chatText.text += $"\r\n{DateTime.Now:t} {payload.UserId}: {payload.Text}";
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

        PublishMessage(text);

        _inputField.enabled = false;
        _inputField.text = string.Empty;
        _inputField.enabled = true;
    }

    private void PublishMessage(string text)
    {
        var payload = new ChatPayload
                        {
                            UserId = _userId,
                            Text = text
                        };
        //ThreadPool.QueueUserWorkItem()
        Messenger.Default.Publish(payload);
    }

    public void KillMe()
    {
        GameObject.Destroy(gameObject);
        Debug.LogFormat("Killing {0}", gameObject);
    }

    private void OnDestroy()
    {
        Debug.LogFormat("{0} destroyed", this);
    }
}
