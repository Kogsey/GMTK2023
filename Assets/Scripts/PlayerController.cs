// Ignore Spelling: Mult Hitpoints Collider

using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("Base")]
	public PhysicsVelocity2D Velocity;
	public SpriteRenderer SpriteRenderer;
	public GUI GUI;

	[Header("Movement")]
	private float MoveStateTimer;
	private float FloatTimeLeft;
	public float CoyoteTimer;
	public float ShortDashTimerMax = 0.5f;
	public float PreJumpTimerMax = 0.4f;
	public float ExtendedJumpTimerMax = 0.4f;
	public float FloatTimerMax = 3f;
	public float CoyoteTimeMax = 0.1f;

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

	[Header("Health")]
	public const int MaxHitPoints = 3;
	public int Hitpoints;
	public float MaxImmunity;
	private int ImmunityFlasher;
	public bool Immune => ImmunityTimer >= 0;

	public void OnHit(Enemy attacker)
	{
		if (Immune)
			return;

		attacker.OnHitPlayer();
		if (GUI != null)
			GUI.RemoveCog();
		Hitpoints--;
		ImmunityTimer = MaxImmunity;

		if (Hitpoints <= 0)
			StateManager.GameOver();
	}

	private void UpdateImmunity()
	{
		ImmunityTimer -= Time.deltaTime;
		ImmunityFlasher++;

		if (Immune)
			SpriteRenderer.color = new Color(1, 0, 0, ImmunityFlasher % 2 == 0 ? 0.5f : 1f);
		else
			SpriteRenderer.color = Color.white;
	}

	private bool OnGround { get => CoyoteTimer >= 0; }
	private float FallSpeedCap => 10 * FloatSpeedCap;
	private float AirSpeedCap => 1.5f * GroundSpeedCap;

	/// <summary>
	/// -1 for left, 1 for right
	/// </summary>
	private int FaceDirection { get => SpriteRenderer.flipX ? 1 : -1; set => SpriteRenderer.flipX = value == 1; }

	private int DashesLeft;
	private readonly int maxDashes = 1;
	private float ImmunityTimer;

	// Start is called before the first frame update
	private void Start()
	{
		Velocity.Gravity = GravityScale * 9.81f;
	}

	public Vector2 AdjacentItemVelocity;

	private void FixedUpdate()
		=> GetComponent<Rigidbody2D>().AddForce((Vector2)Velocity / Velocity.DeltaTime);

	// Update is called once per frame
	private void Update()
	{
		UpdateTimers();

		Movement();

		UpdateImmunity();
		FinalPhysicsUpdate();
	}

	// OnPreRender is called before a camera starts rendering the scene
	private void OnPreRender()
	{
		ChooseFrame();
	}

	public void UpdateTimers()
	{
		MoveStateTimer -= Velocity.DeltaTime; // Important to use internalVelocity's delta time
		FloatTimeLeft -= Velocity.DeltaTime;
		CoyoteTimer -= Velocity.DeltaTime;
		JumpStateTimer -= Velocity.DeltaTime;
	}

	public void FinalPhysicsUpdate()
	{
		Velocity.x *= Input.GetKey(Settings.CurrentSettings.Left) || Input.GetKey(Settings.CurrentSettings.Right) ? XDrag : XDragWhenNotMoving;
		Velocity.y *= YDrag;

		CapSpeed();
		if (OnGround)
			Velocity.OnGround();
		Velocity.Step();
		//Velocity.StepThenApplyTo(transform);
	}

	public void Movement()
	{
		GeneralMovement();
		JumpMovement();
		//DashMovement();
	}

	public void GeneralMovement()
	{
		if (Input.GetKey(Settings.CurrentSettings.Left) ^ Input.GetKey(Settings.CurrentSettings.Right)) //Exclusive or so do nothing if both held
		{
			if (Input.GetKey(Settings.CurrentSettings.Left))
				MoveInDirection(-1);
			else
				MoveInDirection(1);
		}
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
		HighJump, // Moves to none
		Floating, // Moves to none
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
			}
			_playerJumpState = value;
		}
	}

	private float JumpStateTimer;

	public void JumpMovement()
	{
		bool jumpKeyDown = Input.GetKeyDown(Settings.CurrentSettings.Jump);

		switch (JumpState)
		{
			case Jump.None:
				if (jumpKeyDown) // If press jump key and can jump
				{
					if (OnGround)
					{
						JumpState = Jump.PreJump; // Start jump animation
						FloatTimeLeft = FloatTimerMax;
					}
					else if (FloatTimeLeft > 0)
						JumpState = Jump.Floating;
				}
				break;

			case Jump.PreJump:
				if (JumpStateTimer <= 0)
				{
					Velocity += (Mathf.Abs(Velocity.x) > HighJumpMinSpeed ? HighJumpForceMult : 1f) * BaseJumpImpulse * Vector2.up; // Boost internalVelocity
					JumpState = Jump.HighJump;
				}
				break;

			case Jump.HighJump:
				Velocity.Gravity = 0.1f * 9.81f;
				if (JumpStateTimer <= 0 || !Input.GetKey(Settings.CurrentSettings.Jump) || OnGround)
				{
					JumpState = Jump.None;
					Velocity.Gravity = GravityScale * 9.81f;
				}
				break;

			case Jump.Floating:
				if (Input.GetKeyUp(Settings.CurrentSettings.Jump))
					JumpState = Jump.None;
				break;
		}
	}

	public void DashMovement()
	{
		if (DashesLeft > 0 && Input.GetKeyDown(Settings.CurrentSettings.Dash))
		{
			DashesLeft--;
			Velocity += BaseDashImpulse * FaceDirection * Vector2.right;
			MoveStateTimer = ShortDashTimerMax;
		}

		if (OnGround && MoveStateTimer <= 0)
			DashesLeft = maxDashes;
	}

	public void CapSpeed()
	{
		if (!OnGround)
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

	private void ChooseFrame()
	{
		if (JumpState == Jump.PreJump)
			SetCurrentSprite(JumpFrames[0]);
		else if (!OnGround)
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
		=> SpriteRenderer.sprite = value;

	public void OnCollisionEnter2D(Collision2D collision)
		=> WhileColliding(collision);

	public void OnCollisionStay2D(Collision2D collision)
		=> WhileColliding(collision);

	public void WhileColliding(Collision2D collision)
	{
		if (collision.gameObject.TryGetComponent(out Enemy enemy))
		{
			if (enemy.CollisionTypes == EnemyCollisionTypes.Hurt)
				OnHit(enemy);
		}
		else if (IsUpFacing(collision))
			CoyoteTimer = CoyoteTimeMax;
	}

	public bool IsUpFacing(Collision2D collision)
		=> collision.contactCount > 0 && Vector2.Dot(collision.GetContact(0).normal, Vector2.up) > 0.5;
}