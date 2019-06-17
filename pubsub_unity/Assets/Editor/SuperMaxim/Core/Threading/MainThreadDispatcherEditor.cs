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
            GUILayout.Label(string.Format("Main Thread ID: {0}", dispatcher.MainThreadId));
            GUILayout.Label(string.Format("Tasks Count: {0}", dispatcher.TasksCount));

            base.OnInspectorGUI();
        }
    }
}
#endif