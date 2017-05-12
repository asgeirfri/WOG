using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour {
	public float speed;
	public float startOffset;
	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		//transform.position += transform.position.forward * 1;
	}
	
	// Update is called once per frame
	void Update () {
		rb.transform.position += rb.transform.forward * speed * Time.deltaTime;
	}
}
