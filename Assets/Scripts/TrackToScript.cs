using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackToScript : MonoBehaviour
{
	public Transform tracked;
	public Vector3 Offset;

	// Start is called before the first frame update
	private void Start()
	{
	}

	// Update is called once per frame
	private void Update()
	{
		transform.position = tracked.position + Offset;
	}
}