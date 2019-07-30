using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DoomtrinityFPSPrototype.Weapons;

namespace DoomtrinityFPSPrototype.PrefabPoolSystem {
// Place one gameobject with this script attached to prespawn a specific number of "shoot" entities.
public class PrefabPoolLoader : MonoBehaviour {

	// ========================================================================================================================
	// Instance variables

	[SerializeField] private Projectile _projectile;
	[SerializeField] private Shell _shell;
	[SerializeField] private ParticleSystem _impactEffect;

	[SerializeField] private int fireStuffIstances = 5;

	// ========================================================================================================================
	// Static properties

	public static GameObject ProjectilesParent{ get; private set;}
	public static GameObject ShellsParent{ get; private set;}
	public static GameObject PrjImpactParent{ get; private set;}
	public static GameObject EnemiesParent{ get; private set;}

	// ========================================================================================================================
	// Awake

	private void Awake() {
		ProjectilesParent = new GameObject ("Projectiles");
		ShellsParent = new GameObject ("Shells");
		PrjImpactParent = new GameObject ("Impacts");
		EnemiesParent = new GameObject ("Enemies");
	}

	// ========================================================================================================================
	// Start

	private void Start () {
		
		List<GameObject> projectiles;
		List<GameObject> shells;
		List<GameObject> impactEffects;

		projectiles = PrefabPoolingSystem.Prespawn(_projectile.gameObject, fireStuffIstances);
		shells = PrefabPoolingSystem.Prespawn(_shell.gameObject, fireStuffIstances);
		impactEffects = PrefabPoolingSystem.Prespawn(_impactEffect.gameObject, fireStuffIstances);

		for(int i=0; i < fireStuffIstances; i++) {
			(projectiles [i]).transform.parent = ProjectilesParent.transform;
			(shells [i]).transform.parent = ShellsParent.transform;
			(impactEffects [i]).transform.parent = PrjImpactParent.transform;
		}
    }

	// ========================================================================================================================
}
}