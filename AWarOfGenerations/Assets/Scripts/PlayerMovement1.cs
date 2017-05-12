using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement1 : MonoBehaviour {
	public float speed;
	public float jumpForce;
	public float gravity;

	private float upVelocity = 0;
	private float horizontalVelocity = 0;
	private Vector3 startPos;
	private bool isControlled = true;

	private CharacterController cc;
	private List<inputMemo> savedInput;
	private int currentSavedInput;
	private bool canProceedWithInput;

	public struct inputMemo {
		public float duration;
		public float direction;
		public bool jump;

		public inputMemo(float d, float i) {
			duration = d;
			direction = i;
			jump = false;
		}
	}

	// Use this for initialization
	void Start () {
		savedInput = new List<inputMemo> ();
		cc = GetComponent<CharacterController> ();
		startPos = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		// Set upvelocity
		upVelocity -= gravity * Time.deltaTime;

		// Instruct player to move
		if (isControlled) {
			GetInput ();
		} else {
			FollowInstructions ();
		}
		// Move
		cc.Move(new Vector3 (horizontalVelocity, upVelocity));
		// Zero movement
		horizontalVelocity = 0f;
		if (cc.isGrounded) {
			upVelocity = 0f;
		}

		if (Input.GetKeyDown (KeyCode.R)) {
			RepositionAndRestart ();
		}
	}

	void GetInput() {
		inputMemo memo = new inputMemo (Time.deltaTime, 0f);
		float direction = Input.GetAxis ("Horizontal");
		if (Input.GetButton ("Jump")) {
			memo.jump = true;
		}
		if (direction != 0) {
			memo.direction = direction;
		}
		HandleInput (memo);
		savedInput.Add (memo);
	}

	void HandleInput(inputMemo input) {
		// Set vertical
		horizontalVelocity = input.direction * input.duration * speed;
		// Set upVelocity
		if (cc.isGrounded && input.jump) {
			upVelocity = jumpForce;
		}
	}

	void FollowInstructions() {
		if (currentSavedInput < savedInput.Count && canProceedWithInput) {
			canProceedWithInput = false;
			StartCoroutine (DoSavedInput(savedInput [currentSavedInput]));
			currentSavedInput++;
		}
	}

	IEnumerator DoSavedInput(inputMemo memo) {
		HandleInput (memo);
		// Move chatacter
		yield return new WaitForSeconds (memo.duration);
		canProceedWithInput = true;
	}

	void RepositionAndRestart() {
		this.transform.position = startPos;
		isControlled = false;
		currentSavedInput = 0;
		canProceedWithInput = true;
	}
}
