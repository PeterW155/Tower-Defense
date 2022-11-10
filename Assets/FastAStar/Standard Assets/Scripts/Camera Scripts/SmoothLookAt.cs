using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothLookAt : MonoBehaviour
{
	public Vector3 target;
	public float damping = 6.0f;
	public bool smooth = true;

	private void LateUpdate()
	{
		// Look at and dampen the rotation
		var rotation = Quaternion.LookRotation(target - transform.position);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
	}

}
