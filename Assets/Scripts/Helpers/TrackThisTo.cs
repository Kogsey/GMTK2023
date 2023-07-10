using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackThisTo : MonoBehaviour
{
	public Transform tracked;
	public Vector3 Offset;

	// Update is called once per frame
	private void Update() => transform.position = tracked.position + Offset;
}