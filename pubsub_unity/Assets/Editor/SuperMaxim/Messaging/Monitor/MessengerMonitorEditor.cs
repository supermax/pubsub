#if UNITY_EDITOR
using UnityEditor;

namespace SuperMaxim.Messaging.Monitor
{
    [CustomEditor(typeof(MessengerMonitor))]
    public class MessengerMonitorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var monitor = (MessengerMonitor)target;
            // TODO draw subscribers and etc

            base.OnInspectorGUI();
        }
    }
}
#endif