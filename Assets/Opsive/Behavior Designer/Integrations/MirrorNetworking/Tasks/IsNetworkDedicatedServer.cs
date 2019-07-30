#if MIRROR || MIRROR_1726_OR_NEWER || MIRROR_3_0_OR_NEWER
using Mirror;

namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityNetwork
{
    [TaskCategory("Mirror Networking")]
    [TaskDescription("Internally asks the Network subsystem if it is running in dedicated LAN Server mode. Returns false if we are not running in dedicated LAN Server mode, otherwise returns true.")]
    public class IsNetworkDedicatedServer : Conditional
    {
        public override TaskStatus OnUpdate()
        {
            return (NetworkServer.active && !NetworkClient.active) ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
#endif