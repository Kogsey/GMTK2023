// Ignore Spelling: Mult HitPoints Collider

using UnityEngine;
using Utilities.Damage;
using Utilities.Movement;
using Utilities.Timing;

[RequireComponent(typeof(GroundCheck), typeof(Rigidbody2D), typeof(SpriteRenderer))]
[RequireComponent(typeof(DamageHandler))]
public class PlayerController : MonoBehaviour
{
	//public CountDownTimer CountDownTimer;
	private GroundCheck groundCheck;
	private PhysicsVelocity2D Velocity;
	private Rigidbody2D rigidBody;
	private SpriteRenderer spriteRenderer;

	private float MoveStateTimer;
	private float FloatTimeLeft;

	[Header("Movement")]
	public float ShortDashTimerMax = 0.5f;
	public float PreJumpTimerMax = 0.4f;
	public float JumpDelayTimerMax = 0.5f;
	public float ExtendedJumpTimerMax = 0.4f;
	public float FloatTimerMax = 3f;

	[Space]
	public float BaseSpeedForce;
	public float BaseJumpImpulse;
	public float BaseDashImpulse;

	[Space]
	public float FloatSpeedCap;
	public float GroundSpeedCap;

	[Space]
	public float HighJumpMinSpeed;
	public float HighJumpForceMult;
	public float HighJumpTimeMult;

	[Header("Environment")]
	public float GravityScale;
	public float ExtendedJumpGravityScale;

	[Range(0f, 1f)]
	public float XDrag;

	[Range(0f, 1f)]
	public float XDragWhenNotMoving;

	[Range(0f, 1f)]
	public float YDrag;

	[Header("Animation")]
	public Sprite walkFrame;
	public Sprite[] JumpFrames;
	public Sprite FloatFrame;
	public Sprite FallFrame;

	private bool CanJump => groundCheck.GetGrounded();
	private bool OnGround => groundCheck.GetGroundedPure();
	private float FallSpeedCap => 10 * FloatSpeedCap;
	private float AirSpeedCap => 1.5f * GroundSpeedCap;

	/// <summary>
	/// -1 for left, 1 for right
	/// </summary>
	private int FaceDirection { get => spriteRenderer.flipX ? 1 : -1; set => spriteRenderer.flipX = value == 1; }

	private int DashesLeft;
	private readonly int maxDashes = 1;

	#region Movement

	// Start is called before the first frame update
	private void Start()
	{
		rigidBody = GetComponent<Rigidbody2D>();
		groundCheck = GetComponent<GroundCheck>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		rigidBody.gravityScale = GravityScale;
	}

	// Update is called once per frame
	private void FixedUpdate()
	{
		Velocity = new PhysicsVelocity2D(rigidBody.velocity);
		UpdateTimers();

		Movement();

		FinalPhysicsUpdate();
		ChooseFrame();
	}

	private void Update()
	{
		if (Input.GetKeyDown(Settings.CurrentSettings.Jump))
			QueueJumpTimer.Reset();
	}

	public void UpdateTimers()
	{
		MoveStateTimer -= Velocity.DeltaTime; // Important to use internalVelocity's delta time
		FloatTimeLeft -= Velocity.DeltaTime;
		JumpStateTimer -= Velocity.DeltaTime;
	}

	public void Movement()
	{
		if (Input.GetKey(Settings.CurrentSettings.Left) ^ Input.GetKey(Settings.CurrentSettings.Right)) //Exclusive or so do nothing if both held
			MoveInDirection(Input.GetKey(Settings.CurrentSettings.Left) ? -1 : 1); // Directional movement

		JumpMovement();
		//DashMovement();
	}

	public void MoveInDirection(int direction)
	{
		FaceDirection = direction;
		Velocity += BaseSpeedForce * FaceDirection * Vector2.right;
	}

	private enum Jump
	{
		None, // Moves to PreJump or Floating
		PreJump, // Moves to HighJump
		HighJump, // Moves to JumpDelay
		Floating, // Moves to none
		JumpDelay, //Moves to none
	}

	private Jump _playerJumpState = Jump.None;

	private Jump JumpState
	{
		get => _playerJumpState;
		set
		{
			switch (value)
			{
				case Jump.None:
					JumpStateTimer = 0;
					break;

				case Jump.PreJump:
					JumpStateTimer = PreJumpTimerMax;
					break;

				case Jump.HighJump:
					JumpStateTimer = ExtendedJumpTimerMax * (Mathf.Abs(Velocity.x) > HighJumpMinSpeed ? HighJumpTimeMult : 1f);
					break;

				case Jump.JumpDelay:
					JumpStateTimer = JumpDelayTimerMax;
					break;
			}
			_playerJumpState = value;
		}
	}

	private float JumpStateTimer = -1;
	public CountUpTimer QueueJumpTimer;

	public void JumpMovement()
	{
		bool jumpKey = Input.GetKey(Settings.CurrentSettings.Jump);

		switch (JumpState)
		{
			case Jump.None:
				if (CanJump && !QueueJumpTimer.Check())// If press jump key and can jump
				{
					JumpState = Jump.PreJump; // Start jump animation
					FloatTimeLeft = FloatTimerMax;
					QueueJumpTimer.Force();
				}
				else if (FloatTimeLeft > 0 && jumpKey && !OnGround) // else if can float and jump key down
					JumpState = Jump.Floating;

				break;

			case Jump.PreJump:
				if (JumpStateTimer <= 0)
				{
					Velocity += (Mathf.Abs(Velocity.x) > HighJumpMinSpeed ? HighJumpForceMult : 1f) * BaseJumpImpulse * Vector2.up; // Boost internalVelocity
					JumpState = Jump.HighJump;
				}
				break;

			case Jump.HighJump:
				//Velocity.Gravity = 0.1f * 9.81f;
				rigidBody.gravityScale = 0.1f;
				if (JumpStateTimer <= 0 || !jumpKey || OnGround)
				{
					JumpState = Jump.JumpDelay;
					//Velocity.Gravity = GravityScale * 9.81f;
					rigidBody.gravityScale = GravityScale;
				}
				break;

			case Jump.Floating:
				if (!jumpKey || OnGround)
					JumpState = Jump.None;
				break;

			case Jump.JumpDelay:
				if (JumpStateTimer < 0)
					JumpState = Jump.None;
				break;
		}

		QueueJumpTimer.Tick();
	}

	public void DashMovement()
	{
		if (DashesLeft > 0 && Input.GetKeyDown(Settings.CurrentSettings.Dash))
		{
			DashesLeft--;
			Velocity += BaseDashImpulse * FaceDirection * Vector2.right;
			MoveStateTimer = ShortDashTimerMax;
		}

		if (CanJump && MoveStateTimer <= 0)
			DashesLeft = maxDashes;
	}

	#region FinalUpdate

	public void FinalPhysicsUpdate()
	{
		Velocity.x *= GetXDrag();
		Velocity.y *= YDrag;

		CapSpeed();
		if (OnGround)
			Velocity.OnGround();
		//Velocity.StepThenApplyTo(transform);
		rigidBody.velocity = Velocity.GetTotalVelocity();
	}

	public float GetXDrag()
		=> (Input.GetKey(Settings.CurrentSettings.Left) || Input.GetKey(Settings.CurrentSettings.Right)) // If pressing movement key
			?	(((Velocity.x < 0 && Input.GetKey(Settings.CurrentSettings.Right)) || (Velocity.x > 0 && Input.GetKey(Settings.CurrentSettings.Left))) // and if pressing the opposite direction to movement
				?	XDragWhenNotMoving // High drag when moving in opposite direction to velocity
				:	XDrag) // Low drag when moving in same dir as velocity
				:	XDragWhenNotMoving; // High drag when pressing nothing

	public void CapSpeed()
	{
		if (!CanJump)
		{
			CapFall(JumpState == Jump.Floating ? FloatSpeedCap : FallSpeedCap);
			CapMovement(AirSpeedCap);
		}
		CapMovement(GroundSpeedCap);
	}

	public void CapFall(float speed)
	{
		if (Velocity.y < speed)
			Velocity.y = speed;
	}

	public void CapMovement(float speed)
	{
		if (Velocity.x < -speed)
			Velocity.x = -speed;
		else if (Velocity.x > speed)
			Velocity.x = speed;
	}

	#endregion FinalUpdate

	#endregion Movement

	#region Sprites

	private void ChooseFrame()
	{
		if (JumpState == Jump.PreJump)
			SetCurrentSprite(JumpFrames[0]);
		else if (!CanJump)
		{
			if (Velocity.y > 0)
				SetCurrentSprite(JumpFrames[1]);
			else if (JumpState == Jump.Floating)
				SetCurrentSprite(FloatFrame);
			else
				SetCurrentSprite(FallFrame);
		}
		else
			SetCurrentSprite(walkFrame);
	}

	private void SetCurrentSprite(Sprite value)
		=> spriteRenderer.sprite = value;

	#endregion Sprites

	//public bool IsUpFacing(Collision2D collision)
	//	=> collision.contactCount > 0 && Vector2.Dot(collision.GetContact(0).normal, Vector2.up) > 0.5;
}