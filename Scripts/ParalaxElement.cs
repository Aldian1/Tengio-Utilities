using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParalaxElement : MonoBehaviour
{
	public Transform camTransform ;
	
	[RangeAttribute(-1f, 1f)]
	public float paralaxMultiplier = 0f;
	
	
	// Use this for initialization
	void Start ()
	{
		if (camTransform == null) {
			camTransform = Camera.main.transform;
		}
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		Vector3 pos = transform.position;
		pos.x = camTransform.position.x * paralaxMultiplier;
		transform.position = pos;
	}
}
