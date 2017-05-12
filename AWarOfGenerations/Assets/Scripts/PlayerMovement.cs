using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
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
		public float input;

		public inputMemo(float d, float i) {
			duration = d;
			input = i;
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
			// Move chatacter
			cc.Move(new Vector3 (horizontalVelocity * Time.deltaTime * speed, upVelocity));
		} else {
			FollowInstructions ();
		}

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
		if (Input.GetButton ("Jump")) {
			HandleInput (2f);
			inputMemo memo = new inputMemo (Time.deltaTime, 2f);
			savedInput.Add (memo);
			print (memo.input);
		}
		else if (Input.GetAxis ("Horizontal") != 0) {
			HandleInput (Input.GetAxis ("Horizontal"));
			inputMemo memo = new inputMemo (Time.deltaTime, Input.GetAxis ("Horizontal"));
			savedInput.Add (memo);
		} else {
			inputMemo memo = new inputMemo (Time.deltaTime, 0f);
			savedInput.Add (memo);
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
		HandleInput (memo.input);
		// Move chatacter
		cc.Move(new Vector3 (horizontalVelocity * memo.duration * speed, upVelocity));
		yield return new WaitForSeconds (memo.duration);
		canProceedWithInput = true;
	}

	void HandleInput(float input) {
		// Set vertical
		if (Mathf.Abs (input) < 2 && input != 0) {
			horizontalVelocity = input;
			return;
		}
		// Set upVelocity
		if (cc.isGrounded && input == 2f) {
			upVelocity = jumpForce;
		}
	}

	void RepositionAndRestart() {
		this.transform.position = startPos;
		isControlled = false;
		currentSavedInput = 0;
		canProceedWithInput = true;
	}
}
