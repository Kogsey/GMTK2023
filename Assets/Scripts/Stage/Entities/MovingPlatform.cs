using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	public bool GenerateRandomVariant;
	public bool IsVariant;
	public Sprite VariantSprite;

	public float PathLength;
	public float PathAngle;
	public float StartOffsetLength;
	public float MoveSpeed;

	private Vector2 HomePositon;
	private Vector2 VectorAngle => new(Mathf.Cos(PathAngle * Mathf.Deg2Rad), Mathf.Sin(PathAngle * Mathf.Deg2Rad));
	private float StartOffsetChecked => Mathf.Min(StartOffsetLength, PathLength);
	private float DistanceMoved;
	private float DampedDistance => Mathf.Max(1f, PathLength / 2);

	private int MovementDirection = 1;

	// Start is called before the first frame update
	private void Start()
	{
		//Velocity.gravityScale = 0f;
		//Velocity.constraints = RigidbodyConstraints2D.FreezeRotation;
		HomePositon = transform.position;
		DistanceMoved = StartOffsetChecked;

		if (GenerateRandomVariant)
			IsVariant = Random.Range(0, 2) == 0;
		if (IsVariant)
			GetComponent<SpriteRenderer>().sprite = VariantSprite;
	}

	// This function is called every fixed framerate frame, if the MonoBehaviour is enabled
	private void FixedUpdate()
	{
		DistanceMoved += Time.deltaTime * MoveSpeed * MovementDirection;
		if (DistanceMoved > PathLength)
		{
			MovementDirection = -1;
			DistanceMoved = PathLength;
		}

		if (DistanceMoved < 0f)
		{
			MovementDirection = 1;
			DistanceMoved = 0f;
		}

		transform.position = GetDampedPosition();
	}

	protected Vector2 GetDampedPosition()
	{
		float dampedDistanceMoved = DistanceMoved;
		if (DistanceMoved <= DampedDistance)
			dampedDistanceMoved = Mathf.Lerp(0f, DistanceMoved, DistanceMoved / DampedDistance);
		else if (PathLength - DistanceMoved <= DampedDistance)
			dampedDistanceMoved = Mathf.Lerp(PathLength, DistanceMoved, (PathLength - DistanceMoved) / DampedDistance);
		return HomePositon + VectorAngle * dampedDistanceMoved;
	}

	public void OnCollisionEnter2D(Collision2D collision)
		=> WhileColliding(collision);

	public void OnCollisionStay2D(Collision2D collision)
		=> WhileColliding(collision);

	public void WhileColliding(Collision2D collision)
	{
		if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController playerController))
			playerController.AdjacentItemVelocity = Time.deltaTime * MoveSpeed * MovementDirection * VectorAngle;
	}
}