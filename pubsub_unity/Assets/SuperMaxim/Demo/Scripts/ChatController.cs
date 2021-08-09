#region

using System;
using System.Collections;
using SuperMaxim.Core.Extensions;
using SuperMaxim.Logging;
using SuperMaxim.Messaging;
using SuperMaxim.Messaging.Components;
using UnityEngine;
using UnityEngine.UI;
using LogType = SuperMaxim.Logging.LogType;

#endregion

/// <summary>
/// Chat Controller Class
/// </summary>
public class ChatController : MonoBehaviour
{
    /// <summary>
    /// max len. of text per single msg
    /// </summary>
    private const int MaxChatTextLength = 250;

    /// <summary>
    /// multi-threaded queue handler to be used to send messages on new thread that is not main thread 
    /// </summary>
    private readonly ThreadQueue<string> _threadQueue = new ThreadQueue<string>();

    /// <summary>
    /// text component to present incoming messages
    /// </summary>
    [SerializeField] private Text _chatText;

    /// <summary>
    /// input text field
    /// </summary>
    [SerializeField] private InputField _inputField;

    /// <summary>
    /// if true, each msg will be sent on a separate thread (diff from main thread)
    /// </summary>
    [SerializeField] private bool _isMultiThreadingOn;

    /// <summary>
    /// capture button instance to lock during msg sending
    /// </summary>
    [SerializeField] private Button _sendButton;

    /// <summary>
    /// capture last entered text for debugging in game object
    /// </summary>
    [SerializeField] private string _text;

    /// <summary>
    /// capture user ID for msg filtering and debugging
    /// </summary>
    [SerializeField] private string _userId;

    /// <summary>
    /// user ID text label
    /// </summary>
    [SerializeField] private Text _userIdText;

    /// <summary>
    /// Invoked before 1st call Update()
    /// </summary>
    private void Start()
    {
        // print user ID in UI label
        _userIdText.text = _userId;

        // subscribe to chat msg payload with filter
        Messenger.Default.Subscribe<ChatPayload>(OnChatMessage, ChatMessagePredicate)
            // subscribe to generic payload with filter
            .Subscribe<PayloadCommand>(OnPayloadCommand, PayloadCommandPredicate);
    }

    /// <summary>
    /// Returns 'true' if payload can be accepted (filter function)/>
    /// </summary>
    /// <param name="payload">Given filter</param>
    /// <returns>Returns 'true' if payload can be accepted</returns>
    private static bool PayloadCommandPredicate(PayloadCommand payload)
    {
        const string key = nameof(MultiThreadingToggle);
        // check if payload ID == MultiThreadingToggle and data is of type MultiThreadingToggle
        var res = payload.Id == key && payload.Data is MultiThreadingToggle;
        return res;
    }

    /// <summary>
    /// Callback for <see cref="PayloadCommand"/> (receiver function)
    /// </summary>
    /// <param name="payload"><see cref="PayloadCommand"/></param>
    private void OnPayloadCommand(PayloadCommand payload)
    {
        // cast payload data to MultiThreadingToggle
        var data = (MultiThreadingToggle) payload.Data;
        _isMultiThreadingOn = data.IsMultiThreadingOn;

        // check if multithreading flag is on
        if (_isMultiThreadingOn)
        {
            // start thread queue
            _threadQueue.Start();
        }
        else
        { 
            // stop thread queue
            _threadQueue.Stop();
        }
    }
    
    /// <summary>
    /// Returns 'true' if payload can be accepted (filter function)/>
    /// </summary>
    /// <param name="payload">Given filter</param>
    /// <returns>Returns 'true' if payload can be accepted</returns>
    private bool ChatMessagePredicate(ChatPayload payload)
    {
        // check if message is from the same user ID bound to this instance
        var isSameId = payload.UserId == _userId;
        // if same user ID, ignore message
        if (isSameId) return false;

        // check if message text is null or empty
        var isEmpty = payload.Text.IsNullOrEmpty();
        return !isEmpty;
    }

    /// <summary>
    /// Callback for <see cref="ChatPayload"/> (receiver function)
    /// </summary>
    /// <param name="payload"><see cref="ChatPayload"/></param>
    private void OnChatMessage(ChatPayload payload)
    {
        // reset chat text stack if it has reached its maximum
        if (_chatText.text.Length + payload.Text.Length > MaxChatTextLength)
        {
            _chatText.text = string.Empty;
        }

        // copy text and ensure that within max allowed range
        var txt = payload.Text.Length > MaxChatTextLength ? 
                            payload.Text.Substring(0, MaxChatTextLength) : 
                            payload.Text;
        // append text to current chat texts
        _chatText.text += $"\r\n{DateTime.Now:t} {payload.UserId}: {txt.Trim()}";
    }

    /// <summary>
    /// Called on chat message text change
    /// </summary>
    /// <param name="text">message text</param>
    public void OnTextChanged(string text)
    {
        // capture for the debug in inspector
        _text = text;
        // if text field is empty, disable "send" button
        _sendButton.enabled = !_inputField.text.IsNullOrEmpty();
    }

    /// <summary>
    /// Called when text was edited by user
    /// </summary>
    /// <remarks>When pressed enter or text field lost focus</remarks>
    /// <param name="text">recent text</param>
    public void OnTextEndEdit(string text)
    {
        if (text.IsNullOrEmpty()) return;

        if (_isMultiThreadingOn)
        {
            _threadQueue.Enqueue(PublishMessage, text);
        }
        else
        {
            PublishMessage(text);
        }

        // disable text field to avoid input lock
        _inputField.enabled = false;
        // reset text and reenable async
        StartCoroutine(EnableInputFieldCoroutine());
    }
    
    /// <summary>
    /// Enable Input Field after one frame
    /// </summary>
    /// <returns><see cref="IEnumerator"/></returns>
    private IEnumerator EnableInputFieldCoroutine()
    {
        // wait one frame
        yield return null;
        
        // clear text field
        _inputField.SetTextWithoutNotify(string.Empty);
        // reenable text field
        _inputField.enabled = true;

        _inputField.ActivateInputField();
    }
    
    /// <summary>
    /// Publish text message
    /// </summary>
    /// <param name="text">The text from input text box</param>
    private void PublishMessage(string text)
    {
        var payload = new ChatPayload
                            {
                                UserId = _userId, // pass current user ID
                                Text = text
                            };
        // publish payload
        Messenger.Default.Publish(payload);
    }

    /// <summary>
    /// Destroys chat panel prefab
    /// </summary>
    public void KillMe()
    {
        if (!gameObject)
        {
            Loggers.Console.LogError($"cannot destroy {nameof(gameObject)}");
            return;
        }
        Loggers.Console.LogInfo("Killing {0}", gameObject.ToString());
        DestroyImmediate(gameObject);
    }

    /// <summary>
    /// Called when script is destroyed
    /// </summary>
    private void OnDestroy()
    {
        // dispose thread queue
        _threadQueue.Dispose();
        Loggers.Console.LogInfo("{0} - \"{1}\" destroyed", name, _userId);
    }
}