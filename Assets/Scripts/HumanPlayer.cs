using UnityEngine;
using System.Collections;

public class HumanPlayer : MonoBehaviour {

	public int walkSpeed, runSpeed, crouchSpeed, jumpSpeed, gravity,equippedWeapon;
	public float gravityRate;
	public Vector2 rotateSpeed, camHeight;
	public GameObject cameraMain,model,holster, pickupText, bullet;
	public Transform head;
	public GameObject[] weapon;
	int moveSpeed = 0;
	float ySpeed = 0;
	Vector3 moveDirection, cameraRot, initCamPos;
	CharacterController cc;
	public Rigidbody[] ragdoll;
	public Weapon currentWeapon;
	bool fire = true;
	GameObject lastBullet;
	
	public enum physicsState
	{
		idle,
		walk,
		crouch,
		run,
		jump,
		climb,
		dead
	}
	
	physicsState physicsFlag;
	
	
	// Use this for initialization
	void Awake () 
	{
		initCamPos = cameraMain.transform.localPosition;
		physicsFlag = physicsState.idle;
		cameraRot = Vector3.zero;
		Screen.lockCursor = true;
		cc = GetComponent<CharacterController>();
		RagdollSwitch(false);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(physicsFlag != physicsState.dead)
		{
			Movement();
			CameraControl();
			MoveSpeed();
		}
		else
		{
			cc.Move(moveDirection*Time.deltaTime);
			moveDirection *= .99f;//slow down as ragdoll is happening
		}
		PlayerInput();
	}
	
	void RagdollSwitch(bool state)
	{
		if(state)
		{
			physicsFlag = physicsState.dead;
			model.animation.Stop();
			cameraMain.transform.parent = head;
		}
		for(int i=0; i< ragdoll.Length;i++)
		{
			ragdoll[i].isKinematic = !state;
			model.animation.playAutomatically = !state;
			model.animation.animatePhysics = state;
		}
	}
	
	void PlayerInput()
	{
		if(Input.GetMouseButton(0))
			Screen.lockCursor = true;
		if(Input.GetKey(KeyCode.R))
			Application.LoadLevel(Application.loadedLevel);
		if(Input.GetKey(KeyCode.Q))
			RagdollSwitch(true);
		if(Input.GetAxis("Mouse ScrollWheel") != 0)
			WeaponSwap();
		if(Input.GetButton("Fire"))
			WeaponFire();
	}
	
	void CameraControl()
	{
		
		cameraRot.y += Input.GetAxis("Mouse X")*rotateSpeed.x;//Horizontal control
		cameraRot.x += -Input.GetAxis("Mouse Y")*rotateSpeed.y;//Vertical control
		
		//Keep rotations within -360 and 360
//		if(cameraRot.y > 360) cameraRot.y -= 360;
//		if(cameraRot.y < -360) cameraRot.y += 360;
		
		//Regulate camera height
		if (cameraRot.x < camHeight.x) cameraRot.x = camHeight.x;
		if (cameraRot.x > camHeight.y) cameraRot.x = camHeight.y;
		
		//Set camera rotation
		cameraMain.transform.eulerAngles = cameraRot;
		model.transform.eulerAngles = new Vector3(transform.eulerAngles.x, cameraRot.y ,transform.eulerAngles.z);//Make model face what the cam faces
	}
	
	void Movement()
	{
		PhysicsFlags();
		
		if(ySpeed > gravity)
			ySpeed += gravityRate;
		moveDirection.y = 0;
		moveDirection.Normalize();
		moveDirection = new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
		moveDirection = cameraMain.transform.TransformDirection(new Vector3(moveDirection.x,transform.eulerAngles.y,moveDirection.z)*moveSpeed);
		moveDirection = new Vector3(moveDirection.x, ySpeed ,moveDirection.z);
		cc.Move(moveDirection*Time.deltaTime);
	}
	
	void PhysicsFlags()
	{
		if(cc.isGrounded)
		{
			if(moveDirection != Vector3.zero)//If moving
			{
				if(Input.GetButton("Run") && !Input.GetButton("Crouch"))//If run is pressed but not crouch
					physicsFlag = physicsState.run;
				else if(!Input.GetButton("Run") && Input.GetButton("Crouch"))//If crouch is pressed but not run
					Crouch ();
				else
				{
					UnCrouch();
					physicsFlag = physicsState.walk;
				}
			}
			else
				physicsFlag = physicsState.idle;
			
			if(Input.GetButton("Jump"))
				Jump();
		}
	}
	
	void Jump()
	{
		moveDirection.y = jumpSpeed;
		physicsFlag = physicsState.jump;
		ySpeed = 0;
		ySpeed += jumpSpeed;
	}
	
	void Crouch()
	{
		cameraMain.transform.position = new Vector3(cameraMain.transform.position.x, head.transform.position.y, cameraMain.transform.position.z);
		physicsFlag = physicsState.crouch;
		//Revise character controller height
		if(cc.height > 1.4)
			cc.height *= .99f;
		//Revise character controller center
		if(cc.center.y > 0.7)
			cc.center = new Vector3(cc.center.x,cc.center.y*.99f,cc.center.z);
	}
	
	void UnCrouch()
	{
		if(cameraMain.transform.position.y < transform.position.y + initCamPos.y)
			cameraMain.transform.position = new Vector3(cameraMain.transform.position.x, cameraMain.transform.position.y*1.05f, cameraMain.transform.position.z);
			
		if(cc.height != 2 || cc.center != new Vector3(0,1,0))
		{
			cc.height = 2;
			cc.center = new Vector3(0,1,0);
		}
	}
	
	void MoveSpeed()
	{
				//Change movespeed
		switch(physicsFlag)
		{
			case physicsState.run:	
			{
				if(moveDirection.x != 0 || moveDirection.z != 0)
					model.animation.Play("run");
				moveSpeed = runSpeed;
				break;
			}
			case physicsState.crouch:	
			{
				if(moveDirection.x != 0 || moveDirection.z != 0)
					model.animation.Play("crouchWalk");
				else
					model.animation.Play("crouch");
				moveSpeed = crouchSpeed;
				break;
			}
			default:	
			{
				if(moveDirection.x != 0 || moveDirection.z != 0)
					model.animation.Play("walk");
				else
					model.animation.Play("idle");
				moveSpeed = walkSpeed;
				break;
			}
		};	
	}
		
	void WeaponSwap()
	{
		if(weapon[0] != null && weapon[1] != null)
		{
			weapon[equippedWeapon].SetActive(false);
			if(equippedWeapon == 1)
				equippedWeapon = 0;
			else
				equippedWeapon = 1;
			weapon[equippedWeapon].SetActive(true);
			currentWeapon = weapon[0].GetComponent<Weapon>();
		}
	}
	
	void WeaponPickup(Collider other)
	{
		print("pickup");
		pickupText.SetActive(false);
		other.transform.parent = holster.transform;
		other.transform.localPosition = Vector3.zero;
		other.transform.localRotation = Quaternion.identity;
		other.collider.enabled = false;
		other.gameObject.layer = 8;
		if(weapon[0]!= null)
		{
			weapon[1] = weapon[0];
			weapon[0].SetActive(false);
		}
		weapon[0] = other.gameObject;
		currentWeapon = weapon[0].GetComponent<Weapon>();
	}
	
	void WeaponFire()
	{
		if(weapon[0] != null)
		{
			if(fire)
			{
				lastBullet = Instantiate(bullet, currentWeapon.muzzle.transform.position,currentWeapon.transform.rotation) as GameObject;
				lastBullet.rigidbody.AddForce(lastBullet.transform.forward * (currentWeapon.bulletSpeed + moveDirection.magnitude));
				StartCoroutine(WeaponCooldown(currentWeapon.shotCooldown));
			}
		}
	}
	
	IEnumerator WeaponCooldown(float cooldown)
	{
		fire = false;
		yield return new WaitForSeconds(cooldown);
		fire = true;
	}
	
	void OnTriggerStay(Collider other)
	{
		print("hit");
		if(other.tag == "Weapon")
		{
			pickupText.SetActive(true);
			print("weapon");
			if(Input.GetButton("Pickup"))
			{
				WeaponPickup(other);
			}
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if(other.tag == "Weapon")
		{
			pickupText.SetActive(false);
		}
	}
}
