* **DON’T** use Messenger if you have a direct access to shared code parts to invoke events/methods in the same module/class.
* **ALWAYS** ensure that you unsubscribe when you’re done with the consuming of payloads.
* **DON’T** publish payloads in endless or in a long running loops.
* **PREFER** using Filtered subscriptions.