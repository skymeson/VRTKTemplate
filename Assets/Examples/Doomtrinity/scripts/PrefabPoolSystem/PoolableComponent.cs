using UnityEngine;
using System.Collections;

namespace DoomtrinityFPSPrototype.PrefabPoolSystem {
public interface IPoolableComponent {
	void Spawned();
	void Despawned();
}

// Actually unused.
public class PoolableComponent : MonoBehaviour, IPoolableComponent {

	// ========================================================================================================================
	// Spawned

	public virtual void Spawned() {
		Debug.Log (string.Format("Object spawned: {0}", gameObject.name));
	}

	// ========================================================================================================================
	// Despawned

	public virtual void Despawned() {
		// handle any destruction operations here
		Debug.Log (string.Format("Object despawned: {0}", gameObject.name));
	}

	// ========================================================================================================================

}
}