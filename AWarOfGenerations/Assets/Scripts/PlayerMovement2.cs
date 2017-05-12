using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2 : MonoBehaviour {
	public float speed;
	public float jumpForce;
	public float gravity;
	public float positionTimeOffset;

	private float upVelocity = 0;
	private float horizontalVelocity = 0;
	private Vector3 ghostVelocity = Vector3.zero;
	private Vector3 startPos;
	private bool isControlled = true;

	private CharacterController cc;
	private List<Vector3> savedPosition;
	private int currentSavedInput;
	private bool canProceedWithInput;
	private bool canSave;
	private Animator anim;

	// Use this for initialization
	void Start () {
		savedPosition = new List<Vector3> ();
		cc = GetComponent<CharacterController> ();
		startPos = this.transform.position;
		canSave = true;
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		// Set upvelocity
		upVelocity -= gravity * Time.deltaTime;

		// Instruct player to move
		if (isControlled) {
			HandleInput ();
			// Move
			cc.Move(new Vector3 (horizontalVelocity, upVelocity));
			SetAnimation (horizontalVelocity);
			if (canSave) {
				canSave = false;
				StartCoroutine (SavePosition ());
			}
			// Zero movement
			horizontalVelocity = 0f;
			if (cc.isGrounded) {
				upVelocity = 0f;
			}
		} else {
			FollowInstructions ();
			SetAnimation (ghostVelocity.x);
		}

		// Restart and replay
		if (Input.GetKeyDown (KeyCode.R)) {
			RepositionAndRestart ();
		}
	}

	void FixedUpdate() {
		// Todo use fixed update to store position;
	}

	void HandleInput() {
		// Set vertical
		horizontalVelocity = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
		// Set upVelocity
		if (cc.isGrounded && Input.GetButton("Jump")) {
			upVelocity = jumpForce;
		} 
	}

	void SetAnimation(float v) {
		if (v != 0) {
			anim.Play ("Archer1_Walk");
		} else {
			anim.Play ("Archer1_Idle");
		}
		if (v > 0) {
			transform.localRotation = Quaternion.Euler (new Vector3 (0, 0));
		} else if (v < 0) {
			transform.localRotation = Quaternion.Euler (new Vector3 (0, 180));
		}
	}

	void FollowInstructions() {
		if (currentSavedInput < savedPosition.Count && canProceedWithInput) {
			canProceedWithInput = false;
			StartCoroutine (DoSavedInput (savedPosition [currentSavedInput]));
			currentSavedInput++;
		} else if (currentSavedInput == savedPosition.Count) {
			ghostVelocity = Vector3.zero;
		}
	}

	IEnumerator DoSavedInput(Vector3 target) {
		transform.position = Vector3.SmoothDamp(transform.position, target, ref ghostVelocity, positionTimeOffset);
		// Move chatacter
		yield return new WaitForSeconds (positionTimeOffset);
		canProceedWithInput = true;
	}

	IEnumerator SavePosition() {
		savedPosition.Add (this.transform.position);
		yield return new WaitForSeconds (positionTimeOffset);
		canSave = true;
	}

	void RepositionAndRestart() {
		this.transform.position = startPos;
		if (isControlled) {
			Instantiate (this.gameObject);
		}
		isControlled = false;
		currentSavedInput = 0;
		canProceedWithInput = true;
	}
}
