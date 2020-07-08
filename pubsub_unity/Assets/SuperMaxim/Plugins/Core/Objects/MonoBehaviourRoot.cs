using UnityEngine;

namespace SuperMaxim.Core.Objects
{
    internal static class MonoBehaviourRoot
    {
        private const string SingletonsRootName = "[SINGLETONS]";
        
        private static GameObject _root;

        internal static GameObject GetRoot()
        {
            // create root game object for "[SINGLETONS]"
            if (_root != null)
            {
                return _root;
            }
            
            var root = GameObject.Find(SingletonsRootName);
            if(root == null)
            {
                root = new GameObject(SingletonsRootName);
            }
            _root = root;
            return _root;
        }
    }
}