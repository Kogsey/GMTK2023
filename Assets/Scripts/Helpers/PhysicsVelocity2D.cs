using UnityEngine;

public struct PhysicsVelocity2D
{
	public PhysicsVelocity2D(Vector2 vector)
	{
		internalVelocity = vector;
		gravityVelocity = 0f;
		Gravity = 0;
		currentDeltaTimeType = null;
	}

	private Vector2 internalVelocity;
	private float gravityVelocity;
	public float Magnitude => internalVelocity.magnitude;

	public float Gravity { get; set; }

	public void Step()
		=> gravityVelocity += Gravity * DeltaTime;

	public void OnGround()
		=> gravityVelocity = 0f;

	public readonly Vector2 GetTotalVelocity() => internalVelocity + Vector2.down * gravityVelocity;

	public void StepThenApplyTo(Transform transform)
	{
		Step();
		transform.position += (Vector3)(GetTotalVelocity()) * DeltaTime;
	}

#pragma warning disable IDE1006 // Naming Styles
	public float x { readonly get => internalVelocity.x; set => internalVelocity.x = value; }
	public float y { readonly get => internalVelocity.y; set => internalVelocity.y = value; }
#pragma warning restore IDE1006 // Naming Styles

	public enum DeltaTimeType
	{
		DeltaTime,
		FixedDeltaTime,
		Default = DeltaTime,
	}

	public DeltaTimeType CurrentDeltaTimeType { readonly get => currentDeltaTimeType ?? DeltaTimeType.Default; set => currentDeltaTimeType = value; }
	private DeltaTimeType? currentDeltaTimeType;

	public readonly float DeltaTime => currentDeltaTimeType switch
	{
		DeltaTimeType.DeltaTime => Time.deltaTime,
		DeltaTimeType.FixedDeltaTime => Time.fixedDeltaTime,
		_ => Time.deltaTime,
	};

	#region Operators

	public static PhysicsVelocity2D operator +(PhysicsVelocity2D physicsVector2D, Vector2 vector)
	{
		physicsVector2D.internalVelocity += vector * physicsVector2D.DeltaTime;
		return physicsVector2D;
	}

	public static PhysicsVelocity2D operator -(PhysicsVelocity2D physicsVector2D, Vector2 vector)
		=> physicsVector2D + -vector;

	public static PhysicsVelocity2D operator *(PhysicsVelocity2D physicsVector2D, Vector2 vector)
	{
		physicsVector2D.internalVelocity *= vector/* * physicsVector2D.DeltaTime*/;
		return physicsVector2D;
	}

	public static PhysicsVelocity2D operator /(PhysicsVelocity2D physicsVector2D, Vector2 vector)
		=> physicsVector2D * new Vector2(1f / vector.x, 1f / vector.y);

	public static PhysicsVelocity2D operator *(PhysicsVelocity2D physicsVector2D, float value)
	{
		physicsVector2D.internalVelocity *= value;
		return physicsVector2D;
	}

	public static PhysicsVelocity2D operator /(PhysicsVelocity2D physicsVector2D, float value)
		=> physicsVector2D * (1f / value);

	public static explicit operator Vector2(PhysicsVelocity2D physicsVector2D)
		=> physicsVector2D.GetTotalVelocity();

	public static explicit operator Vector3(PhysicsVelocity2D physicsVector2D)
		=> (Vector3)(Vector2)physicsVector2D;

	#endregion Operators
}