#region

using UnityEngine;

#endregion

namespace SuperMaxim.Demo
{
    [CreateAssetMenu(fileName = "MultiThreadingToggle", menuName = "Demo/Scriptable Objects/Multi-Threading Toggle")]
    public class MultiThreadingToggle : ScriptableObject
    {
        [SerializeField] private bool _isMultiThreadingOn = true;

        public bool IsMultiThreadingOn
        {
            get
            {
                return _isMultiThreadingOn;
            }
            set
            {
                _isMultiThreadingOn = value;
            }
        }
    }
}