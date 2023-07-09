// Ignore Spelling: Mult Hitpoints

using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public const float SubSpace = 10f;

	[Header("Base")]
	public Rigidbody2D RigidBody;
	public SpriteRenderer SpriteRenderer;

	[Header("Movement")]
	private float MoveStateTimer;
	private float AirFloatTimer;
	public float ShortDashTimerMax = 0.5f;
	public float PreJumpTimerMax = 0.4f;
	public float JumpTimerMax = 0.4f;
	public float FloatTimerMax = 3f;

	[Space(SubSpace)]
	public float BaseSpeedForce;
	public float BaseJumpImpulse;
	public float BaseDashImpulse;

	[Space(SubSpace)]
	public float FloatSpeedCap;
	public float GroundSpeedCap;

	[Space(SubSpace)]
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
	public int Hitpoints;
	public float MaxImmunity;
	private int ImmunityFlasher;
	public bool Immune => ImmunityTimer >= 0;

	public void OnHit(Enemy attacker)
	{
		if (Immune)
			return;

		attacker.OnHitPlayer();
		Hitpoints--;
		ImmunityTimer = MaxImmunity;
	}

	private void UpdateImmunity()
	{
		ImmunityTimer -= Time.deltaTime;
		ImmunityFlasher++;

		if (Immune)
			SpriteRenderer.color = SpriteRenderer.color = Color.red.WithAlpha(ImmunityFlasher % 2 == 0 ? 0.5f : 1f);
		else
			SpriteRenderer.color = SpriteRenderer.color = Color.white;
	}

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
	private float ImmunityTimer;

	// Start is called before the first frame update
	private void Start()
	{
		RigidBody.gravityScale = GravityScale;
	}

	// Update is called once per frame
	private void Update()
	{
		ControllerLogic();
		ChooseFrame();

		UpdateImmunity();
	}

	private void FixedUpdate()
	{
		CapSpeed();
		RigidBody.velocityX *= (Input.GetKey(Settings.CurrentSettings.Left) || Input.GetKey(Settings.CurrentSettings.Right) ? XDrag : XDragWhenNotMoving);
		RigidBody.velocityY *= YDrag;
	}

	public void ControllerLogic()
	{
		GroundMovement();
		OtherMovement();
		MoveStateTimer -= Time.deltaTime; // Decrement timer by frame time
	}

	public void GroundMovement()
	{
		IsFloating = false;

		if (Input.GetKey(Settings.CurrentSettings.Left))
		{
			RigidBody.AddForce(Vector2.left * BaseSpeedForce);
			FaceDirection = -1;
		}
		if (Input.GetKey(Settings.CurrentSettings.Right))
		{
			RigidBody.AddForce(Vector2.right * BaseSpeedForce);
			FaceDirection = 1;
		}

		if (Input.GetKeyDown(Settings.CurrentSettings.Jump) && CanJump) // If press jump key and can jump
		{
			IsPreJumping = true; // Start jump animation
			MoveStateTimer = PreJumpTimerMax; // Jump animation timer
		}
		else if (Input.GetKey(Settings.CurrentSettings.Jump) && InAir) // If press jump key
		{
			IsFloating = true;
			AirFloatTimer -= Time.deltaTime;
		}

		if (IsPreJumping && MoveStateTimer <= 0) // If jump animation is over
		{
			RigidBody.AddForce((RigidBody.velocity.magnitude > HighJumpMinSpeed ? HighJumpForceMult : 1f) * BaseJumpImpulse * Vector2.up, ForceMode2D.Impulse); // Boost velocity
			IsPreJumping = false; // Stop jump animation
			IsExtendedJumping = true;
			MoveStateTimer = JumpTimerMax * (RigidBody.velocity.magnitude > HighJumpMinSpeed ? HighJumpTimeMult : 1f);
		}

		if (IsExtendedJumping)
		{
			AirFloatTimer = FloatTimerMax;
			RigidBody.gravityScale = 0.1f;
			if (MoveStateTimer <= 0 || !Input.GetKey(Settings.CurrentSettings.Jump) || IsGrounded)
			{
				IsExtendedJumping = false;
				RigidBody.gravityScale = GravityScale;
			}
		}
	}

	public void OtherMovement()
	{
		if (DashesLeft > 0 && Input.GetKeyDown(Settings.CurrentSettings.Dash))
		{
			DashesLeft--;
			RigidBody.AddForceX(FaceDirection * BaseDashImpulse);
			MoveStateTimer = ShortDashTimerMax;
		}

		if (IsGrounded && MoveStateTimer <= 0)
			DashesLeft = maxDashes;
	}

	public void CapSpeed()
	{
		if (InAir)
		{
			CapFall(IsFloating && AirFloatTimer <= 0 ? FloatSpeedCap : FallSpeedCap);
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

	public void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.gameObject.TryGetComponent(out Enemy enemy))
		{
			if (enemy.CollisionTypes == EnemyCollisionTypes.Hurt)
				OnHit(enemy);
		}
		else if (CheckIsGround(collision))
			IsGrounded = true;
	}

	public bool CheckIsGround(Collision2D collision)
		=> collision.contacts.Length > 0 && Vector2.Dot(collision.contacts[0].normal, Vector2.up) > 0.5;

	public void OnCollisionExit2D(Collision2D collision)
	{
		IsGrounded = false;
	}
}