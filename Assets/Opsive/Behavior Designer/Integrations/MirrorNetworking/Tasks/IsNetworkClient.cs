#if MIRROR || MIRROR_1726_OR_NEWER || MIRROR_3_0_OR_NEWER
using Mirror;

namespace BehaviorDesigner.Runtime.Tasks.MirrorNetworking
{
    [TaskCategory("Mirror Networking")]
    [TaskDescription("Internally asks the Network subsystem if it is running in client mode. Returns false if we are not.")]
    public class IsNetworkClient : Conditional
    {
        public override TaskStatus OnUpdate()
        {
            return (!NetworkServer.active && NetworkClient.active) ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
#endif