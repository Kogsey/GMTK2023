using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public const float SubSpace = 10f;

	[Header("Base")]
	public Rigidbody2D RigidBody;
	public SpriteRenderer SpriteRenderer;

	[Header("Movement")]
	private float MoveStateTimer;
	public float ShortDashTimerMax = 0.5f;
	public float PreJumpTimerMax = 0.4f;
	public float JumpTimerMax = 0.4f;

	[Space(SubSpace)]
	public float BaseSpeedForce;
	public float BaseJumpImpulse;
	public float BaseDashImpulse;

	[Space(SubSpace)]
	public float SlowDownMultiplier;

	[Space(SubSpace)]
	public float FloatSpeedCap;
	public float GroundSpeedCap;

	[Header("Gravity")]
	public float GravityScale;
	public float ExtendedJumpGravityScale;

	[Header("Animation")]
	public Sprite walkFrame;
	public Sprite[] JumpFrames;
	public Sprite FloatFrame;
	public Sprite FallFrame;

	private bool IsGrounded;
	private bool InAir => !IsGrounded;
	private bool CanJump => IsGrounded;
	private float FallSpeedCap => 10 * FloatSpeedCap;
	private float AirSpeedCap => 1.5f * GroundSpeedCap;

	/// <summary>
	/// -1 for left, 1 for right
	/// </summary>
	private int FaceDirection { get => SpriteRenderer.flipX ? 1 : -1; set => SpriteRenderer.flipX = value == 1; }

	private bool IsPreJumping;
	private bool IsExtendedJumping;
	private bool IsFloating;
	private int DashesLeft;
	private readonly int maxDashes = 1;

	// Start is called before the first frame update
	private void Start()
	{
	}

	// Update is called once per frame
	private void Update()
	{
		RigidBody.velocity -= new Vector2(0, GravityScale * 9.81f);

		ControllerLogic();

		CapSpeed();
	}

	public void ControllerLogic()
	{
		GroundMovement();
		OtherMovement();
		ChooseFrame();
	}

	public void GroundMovement()
	{
		IsFloating = false;

		if (Input.GetKey(StateManager.Left))
		{
			RigidBody.velocityX += BaseSpeedForce;
			FaceDirection = -1;
		}
		else if (RigidBody.velocityX < 0)
			RigidBody.velocityX -= MathF.Min(BaseSpeedForce * SlowDownMultiplier, RigidBody.velocityX);

		if (Input.GetKey(StateManager.Right))
		{
			RigidBody.velocityX -= BaseSpeedForce;
			FaceDirection = 1;
		}
		else if (RigidBody.velocityX > 0)
			RigidBody.velocityX += MathF.Min(BaseSpeedForce * SlowDownMultiplier, RigidBody.velocityX);

		if (Input.GetKeyDown(StateManager.Jump)) // If press jump key
		{
			if (CanJump) // and can jump
			{
				IsPreJumping = true; // Start jump animation
				MoveStateTimer = PreJumpTimerMax; // Jump animation timer
			}
		}
		else if (Input.GetKey(StateManager.Jump) && InAir) // If press jump key
			IsFloating = true;

		if (IsPreJumping && MoveStateTimer <= 0) // If jump animation is over
		{
			RigidBody.velocity += Vector2.up * BaseJumpImpulse; // Boost velocity
			IsPreJumping = false; // Stop jump animation
			IsExtendedJumping = true;
			MoveStateTimer = JumpTimerMax;
		}

		if (IsExtendedJumping)
		{
			RigidBody.gravityScale = 0.1f;
			if (MoveStateTimer <= 0 || !Input.GetKey(StateManager.Jump))
			{
				IsExtendedJumping = false;
				RigidBody.gravityScale = GravityScale;
			}
		}

		MoveStateTimer -= Time.deltaTime; // Decrement timer by frame time
	}

	public void OtherMovement()
	{
		if (DashesLeft > 0 && Input.GetKeyDown(StateManager.Dash))
		{
			DashesLeft--;
			RigidBody.velocity += new Vector2(FaceDirection, 0) * BaseDashImpulse;
			MoveStateTimer = ShortDashTimerMax;
		}

		if (IsGrounded && MoveStateTimer <= 0)
			DashesLeft = maxDashes;
	}

	public void CapSpeed()
	{
		if (InAir)
		{
			CapFall(IsFloating ? FloatSpeedCap : FallSpeedCap);
			CapMovement(AirSpeedCap);
		}
		else if (IsGrounded)
			CapMovement(GroundSpeedCap);
	}

	public void CapFall(float speed)
	{
		if (RigidBody.velocityY < speed)
			RigidBody.velocityY = speed;
	}

	public void CapMovement(float speed)
	{
		if (RigidBody.velocityX < -speed)
			RigidBody.velocityX = -speed;
		else if (RigidBody.velocityX > speed)
			RigidBody.velocityX = speed;
	}

	private void ChooseFrame()
	{
		if (IsPreJumping)
			SetCurrentSprite(JumpFrames[0]);
		else if (InAir)
		{
			if (RigidBody.velocityY > 0)
				SetCurrentSprite(JumpFrames[1]);
			else if (IsFloating)
				SetCurrentSprite(FloatFrame);
			else
				SetCurrentSprite(FallFrame);
		}
		else
			SetCurrentSprite(walkFrame);
	}

	private void SetCurrentSprite(Sprite value)
		=> SpriteRenderer.sprite = value;

	public void OnCollisionEnter2D(Collision2D collision)
	{
	}

	public void OnCollisionStay2D(Collision2D collision)
	{
		IsGrounded = true;
	}

	public void OnCollisionExit2D(Collision2D collision)
	{
		IsGrounded = false;
	}
}