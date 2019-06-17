using SuperMaxim.Core.Extensions;
using UnityEngine;

namespace SuperMaxim.Core.Objects
{
    public class MonoBehaviourSingleton<TInterface, TImplementation> : MonoBehaviour
        where TImplementation : MonoBehaviour, TInterface
    {
        private static TInterface _default;

        private const string SingletonsRootName = "[SINGLETONS]";

        private static GameObject _root;

        public static TInterface Default
        {
            get
            {
                var res = InvalidateInstance();
                return res;
            }
        }

        private void Awake()
        {
            InvalidateInstance();
        }

        private static TInterface InvalidateInstance()
        {
            if(!Equals(_default, default(TImplementation)))
            {
                return _default;
            }

            var typeInterface = typeof(TInterface);
            var objects = FindObjectsOfType<TImplementation>();
            if(!objects.IsNullOrEmpty())
            {
                foreach (var obj in objects)
                {
                    if(typeInterface.IsAssignableFrom(obj.GetType()))
                    {
                        _default = obj;
                    }
                }
            }

            var typeImplementation = typeof(TImplementation);
            if (Equals(_default, default(TImplementation)))
            {
                // create root game object for "[SINGLETONS]"
                if(_root == null)
                {
                    var root = GameObject.Find(SingletonsRootName);
                    if(root == null)
                    {
                        root = new GameObject(SingletonsRootName);
                    }
                    _root = root;
                }

                var go = new GameObject(string.Format("[{0}]", typeImplementation.Name));
                go.transform.SetParent(_root.transform);

                _default = go.AddComponent<TImplementation>();
            }
            return _default;
        }
    }
}