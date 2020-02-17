#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace SuperMaxim.Core.Threading
{
    [CustomEditor(typeof(MainThreadDispatcher))]
    public class MainThreadDispatcherEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var dispatcher = (MainThreadDispatcher)target;
            GUILayout.Label($"Main Thread ID: {dispatcher.ThreadId}");
            GUILayout.Label($"Tasks Count: {dispatcher.TasksCount}");

            base.OnInspectorGUI();
        }
    }
}
#endif