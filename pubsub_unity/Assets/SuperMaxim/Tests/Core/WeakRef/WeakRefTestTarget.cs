using System.Diagnostics;

namespace SuperMaxim.Tests.Core.WeakRef
{
    public class WeakRefTestTarget
    {
        public void TestVoidCallback()
        {
            Debug.WriteLine("{0} invoked", nameof(TestVoidCallback));
        }
    }
}
