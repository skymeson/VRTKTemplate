using UnityEngine;

namespace DoomtrinityFPSPrototype.Character {
public interface IDamageable {
    void Damage(float damage, Vector3 hitPoint, Vector3 hitDir);
    void Damage(float damage);
}
}