using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	public int bulletSpeed, damage, recoilStrength;
	public float shotCooldown;
	public AudioClip fireSound;
	public TrailRenderer trail;
	public GameObject muzzle;
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
