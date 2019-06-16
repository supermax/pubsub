using System;
using System.Collections;
using System.Collections.Generic;
using SuperMaxim.Core.Extensions;
using SuperMaxim.Messaging;
using UnityEngine;

public class ChatController : MonoBehaviour
{
    [SerializeField]
    private int _userId;

    [SerializeField]
    private string _text;

    [SerializeField]
    private UnityEngine.UI.InputField _inputField;

    [SerializeField]
    private UnityEngine.UI.Text _chatText;

    private void Start()
    {
        Messenger.Default.Subscribe<ChatPayload>(OnChatMessage, ChatMessagePredicate);
    }

    private bool ChatMessagePredicate(ChatPayload payload)
    {
        var isSameId = payload.UserId == _userId;
        if(!isSameId)
        {
            return false;
        }

        var isEmpty = payload.Text.IsNullOrEmpty();
        return !isEmpty;
    }

    private void OnChatMessage(ChatPayload payload)
    {
        _chatText.text += string.Format("/r/n{0}", payload.Text);
    }

    public void OnTextChanged(string text)
    {
        _text = text;

        _inputField.enabled = !_text.IsNullOrEmpty();
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
    }
}
