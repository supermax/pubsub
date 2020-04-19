#region

using System;
using SuperMaxim.Core.Extensions;
using SuperMaxim.Messaging;
using SuperMaxim.Messaging.Components;
using UnityEngine;
using UnityEngine.UI;

#endregion

/// <summary>
/// Chat Controller Class
/// </summary>
public class ChatController : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    private const int MaxChatTextLength = 3000;

    private readonly ThreadQueue<string> _threadQueue = new ThreadQueue<string>();

    [SerializeField] private Text _chatText;

    [SerializeField] private InputField _inputField;

    [SerializeField] private bool _isMultiThreadingOn;

    [SerializeField] private Button _sendButton;

    [SerializeField] private string _text;

    [SerializeField] private string _userId;

    [SerializeField] private Text _userIdText;

    private void Start()
    {
        _userIdText.text = _userId;

        Messenger.Default.Subscribe<ChatPayload>(OnChatMessage, ChatMessagePredicate)
            .Subscribe<PayloadCommand>(OnPayloadCommand, PayloadCommandPredicate);
    }

    private bool PayloadCommandPredicate(PayloadCommand payload)
    {
        const string key = nameof(MultiThreadingToggle);
        var res = payload.Id == key && payload.Data is MultiThreadingToggle;
        return res;
    }

    private void OnPayloadCommand(PayloadCommand payload)
    {
        var data = (MultiThreadingToggle) payload.Data;
        _isMultiThreadingOn = data.IsMultiThreadingOn;

        if (_isMultiThreadingOn)
            _threadQueue.Start();
        else
            _threadQueue.Stop();
    }

    private bool ChatMessagePredicate(ChatPayload payload)
    {
        var isSameId = payload.UserId == _userId;
        if (isSameId) return false;

        var isEmpty = payload.Text.IsNullOrEmpty();
        return !isEmpty;
    }

    private void OnChatMessage(ChatPayload payload)
    {
        if (_chatText.text.Length > MaxChatTextLength) _chatText.text = string.Empty;

        _chatText.text += $"\r\n{DateTime.Now:t} {payload.UserId}: {payload.Text}";
    }

    public void OnTextChanged(string text)
    {
        _text = text;

        _sendButton.enabled = !_inputField.text.IsNullOrEmpty();
    }

    public void OnTextEndEdit(string text)
    {
        if (text.IsNullOrEmpty()) return;

        if (_isMultiThreadingOn)
            _threadQueue.Enqueue(PublishMessage, text);
        else
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
        Messenger.Default.Publish(payload);
    }

    public void KillMe()
    {
        Debug.LogFormat("Killing {0}", gameObject);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _threadQueue.Dispose();
        Debug.LogFormat("{0} - \"{1}\" destroyed", name, _userId);
    }
}