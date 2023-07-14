using UnityEngine;
using Utilities.Timing;

namespace Utilities.Movement
{
	public class GroundCheck : MonoBehaviour
	{
		private bool onGround;
		private Rigidbody2D rigidBody;

		[Header("Collider Settings")]
		[SerializeField][Tooltip("Length of the ground-checking collider")] private float groundLength = 0.95f;
		[SerializeField][Tooltip("Distance between the ground-checking colliders")] private Vector3 colliderOffset;
		[SerializeField][Tooltip("Cannot be grounded if moving upwards")] private bool accountForVelocity;
		[SerializeField][Tooltip("The maximum speed that can be moved upwards and still be grounded")] private float maxVelocity;
		[SerializeField][Tooltip("the timer for coyote time")] private CountUpTimer groundedTimer;

		[Header("Layer Masks")]
		[SerializeField][Tooltip("Which layers are read as the ground")] private LayerMask groundLayer;

		private void Start()
		{
			rigidBody = GetComponent<Rigidbody2D>();
		}

		// Update is called once per frame
		private void Update()
		{
			groundedTimer.Tick();
			onGround = (Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer)) && (!accountForVelocity || rigidBody.velocityY < maxVelocity);
			if (onGround)
				groundedTimer.Reset();
		}

		private void OnDrawGizmos()
		{
			//Draw the ground colliders on screen for debug purposes
			if (onGround) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
			Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundLength);
			Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundLength);
		}

		//Send ground detection to other scripts
		public bool GetGrounded()
			=> groundedTimer.Time <= groundedTimer.MaxTime;

		public bool GetGroundedPure()
			=> onGround;

		public float TimeSinceGrounded()
			=> groundedTimer.Time;
	}
}