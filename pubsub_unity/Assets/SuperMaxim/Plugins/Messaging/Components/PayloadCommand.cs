using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperMaxim.Messaging.Components
{
    [Serializable]
    public class PayloadCommand : IPayloadCommand
    {
        [SerializeField]
        private string _id;

        public string Id { get; set;}

        [SerializeField]
        private Dictionary<string, string> _data;

        public IDictionary<string, string> Data { get; set;}
    }
}