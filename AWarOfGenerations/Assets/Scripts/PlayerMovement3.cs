using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement3 : MonoBehaviour {
	// Public
	public float speed;
	public float jumpForce;
	public float gravity;
	public GameObject arrow;

	// Movement
	private Vector3 startPos;
	private bool isControlled = true;
	private CharacterController cc;
	private Animator anim;

	// Movement - controlled
	private float upVelocity = 0;
	private float horizontalVelocity = 0;

	// Movement - not controlled
	private Vector3 ghostVelocity = Vector3.zero;
	private List<Vector2> savedPosition;
	private int savedPositionIterator;

	// Shots
	private float timeFromSpawn;
	private List<ShootingMemo> savedShots;
	private int savedShotsIterator;

	// Animator hashes
	private int animIdle = Animator.StringToHash("Archer1_Idle");
	private int animWalk = Animator.StringToHash ("Archer1_Walk");

	// ShotingMemo is used to track shot history
	public struct ShootingMemo {
		public float time;
		public float direction;

		public ShootingMemo (float t, float d) {
			time = t;
			direction = d;
		}
	}

	// Use this for initialization
	void Start () {
		// Components
		cc = GetComponent<CharacterController> ();
		anim = GetComponent<Animator> ();
		// For ghost memo
		savedPosition = new List<Vector2> ();
		savedShots = new List<ShootingMemo> ();
		startPos = this.transform.position;
		timeFromSpawn = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		// Instruct player to move
		if (isControlled) {
			// Set upvelocity
			upVelocity -= gravity * Time.deltaTime;
			// Move and animate
			HandleInput ();
			cc.Move(new Vector3 (horizontalVelocity, upVelocity));
			SetAnimation (horizontalVelocity);
			// Zero movement
			horizontalVelocity = 0f;
			if (cc.isGrounded) {
				upVelocity = 0f;
			}
		}
		// Restart and replay
		if (Input.GetKeyDown (KeyCode.R)) {
			RepositionAndRestart ();
		}
		// Take controll of character
		if (Input.GetKeyDown (KeyCode.T)) {
			isControlled = true;
		}
	}

	// Called once per 0.02 s. Used to have fixed delta time for ghosts
	void FixedUpdate() {
		if (isControlled) {
			// Save position
			savedPosition.Add (new Vector2(transform.position.x, transform.position.y));
		} else {
			FollowMovementInstructions ();
			FollowShotInstructions ();
			SetAnimation (ghostVelocity.x);
		}
		// Timer count
		timeFromSpawn += Time.fixedDeltaTime;
	}

	// Handle input from controller
	void HandleInput() {
		// Set vertical
		horizontalVelocity = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
		// Set upVelocity
		if (cc.isGrounded && Input.GetButton("Jump")) {
			upVelocity = jumpForce;
		}
		// Shooting
		if (Input.GetButtonDown("Fire1")) {
			// TODO: make controller more comfortable
			float aimingAngle = Vector2.Angle(new Vector2 (1,0), new Vector2 (Input.GetAxisRaw("HorizontalAim"), Input.GetAxisRaw("VerticalAim")));
			if (Input.GetAxisRaw ("VerticalAim") < 0) {
				aimingAngle = -aimingAngle;
			}
			// Save time and direction
			savedShots.Add(new ShootingMemo (timeFromSpawn, aimingAngle));
			// Shoot
			Shoot(aimingAngle);
		}
	}

	// Move according to saved positions
	void FollowMovementInstructions() {
		if (savedPositionIterator < savedPosition.Count) {
			MoveToSavedPosition (savedPosition [savedPositionIterator]);
			savedPositionIterator++;
		} else if (savedPositionIterator == savedPosition.Count) {
			ghostVelocity = Vector3.zero;
		}
	}

	// Shoot according to saved shots
	void FollowShotInstructions() {
		if (savedShots.Count > 0) {
			if (savedShots [savedShotsIterator].time == timeFromSpawn) {
				Shoot (savedShots [savedShotsIterator].direction);
				if (savedShotsIterator < savedShots.Count - 1) {
					savedShotsIterator++;
				}
			}
		}
	}

	// Moves to saved position
	void MoveToSavedPosition(Vector2 target) {
		transform.position = Vector3.SmoothDamp(transform.position, target, ref ghostVelocity, Time.fixedDeltaTime);
	}

	// Shoot an arrow in direction angle from this pos
	void Shoot (float angle) {
		Instantiate (arrow, transform.position, Quaternion.Euler (new Vector3 (angle, 90, 0)));
	}

	void SetAnimation(float v) {
		if (v != 0) {
			anim.Play (animWalk);
		} else {
			anim.Play (animIdle);
		}
		if (v > 0) {
			transform.localRotation = Quaternion.Euler (new Vector3 (0, 0));
		} else if (v < 0) {
			transform.localRotation = Quaternion.Euler (new Vector3 (0, 180));
		}
	}
		
	void RepositionAndRestart() {
		// Return to start pos
		this.transform.position = startPos;

		// If it's the player, duplicate
		if (isControlled) {
			Instantiate (this.gameObject);
		}
		// Let it become a ghost
		isControlled = false;
		savedPositionIterator = 0;
		savedShotsIterator = 0;
		timeFromSpawn = 0f;
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		print (hit.gameObject.tag);
		if (hit.gameObject.CompareTag("Character")) {
			Physics.IgnoreCollision(hit.collider, GetComponent<Collider>());
		}
	}
}
