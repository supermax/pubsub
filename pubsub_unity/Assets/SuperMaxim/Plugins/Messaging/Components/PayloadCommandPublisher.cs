using System;
using UnityEngine;

namespace SuperMaxim.Messaging.Components
{
    // TODO add Drag&Drop support in scene
    public class PayloadCommandPublisher : MonoBehaviour
    {
        [SerializeField]
        private PayloadCommand _payload;

        private void Awake()
        {
            if(_payload == null)
            {
                Debug.AssertFormat(_payload == null, "Payload is not assigned");
            }
        }

        public void Publish()
        {
            if(_payload == null)
            {
                Debug.AssertFormat(_payload == null, "Payload is not assigned");
                return;
            }
            Messenger.Default.Publish(_payload);
        }
    }
}