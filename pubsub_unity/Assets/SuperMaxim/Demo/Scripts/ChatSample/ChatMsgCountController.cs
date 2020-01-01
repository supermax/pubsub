using System;
using System.Collections;
using System.Collections.Generic;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class ChatMsgCountController : MonoBehaviour
{
    [SerializeField]
    private Text _counterText;

    private int _msgCount;

    private void Awake()
    {
        Debug.AssertFormat(_counterText != null, "_counterText is not assigned");
    }

    private void Start()
    {
        Messenger.Default.Subscribe<ChatPayload>(OnChatMessageReceived);
    }

    private void OnDestroy()
    {
        Debug.LogFormat("{0} destoyed", this);
    }

    private void OnChatMessageReceived(ChatPayload payload)
    {
        Debug.LogFormat("Received: {0}", payload);

        if(_counterText == null)
        {
            return;
        }

        _counterText.text = string.Format("Message Count: {0}", ++_msgCount);        
    }

    public void KillMe()
    {
        GameObject.Destroy(gameObject);
    }
}