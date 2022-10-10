using System;
using UnityEngine;

namespace SuperMaxim.Messaging.Components
{
    [Serializable]
    public class PayloadCommand : IPayloadCommand
    {
        [SerializeField]
        private string _id;

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        [SerializeField]
        private ScriptableObject _data;

        public ScriptableObject Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }
    }
}