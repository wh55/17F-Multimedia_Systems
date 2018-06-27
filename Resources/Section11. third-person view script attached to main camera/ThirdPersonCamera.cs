using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

	public Transform target;
	public float dstFromTarget = 4.0f;
	
	void LateUpdate () {

		transform.position = target.position - transform.forward * dstFromTarget;
	}
}
