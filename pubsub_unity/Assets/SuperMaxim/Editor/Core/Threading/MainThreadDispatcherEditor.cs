#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace SuperMaxim.Core.Threading
{
    [CustomEditor(typeof(UnityMainThreadDispatcher))]
    public class MainThreadDispatcherEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var dispatcher = (UnityMainThreadDispatcher)target;
            GUILayout.Label($"Main Thread ID: {dispatcher.ThreadId}");
            GUILayout.Label($"Tasks Count: {dispatcher.TasksCount}");

            base.OnInspectorGUI();
        }
    }
}
#endif