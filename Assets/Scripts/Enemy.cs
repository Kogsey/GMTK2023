// Ignore Spelling: Collider

using System;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
	public Rigidbody2D RigidBody;
	public SpriteRenderer SpriteRenderer;

	public abstract Vector2 ColliderSize { get; }
	public abstract float ColliderEdgeRadius { get; }
	public abstract float Gravity { get; }
	public abstract float Drag { get; }
	public virtual bool ConstrainXPosition => false;
	public virtual bool ConstrainYPosition => false;

	public virtual EnemyCollisionTypes CollisionTypes => EnemyCollisionTypes.Hurt;

	// Start is called before the first frame update
	private void Start()
	{
		Setup();
		InternalStart();
	}

	// Update is called once per frame
	private void Update()
		=> InternalUpdate();

	// This function is called every fixed framerate frame, if the MonoBehaviour is enabled
	private void FixedUpdate()
		=> InternalFixedUpdate();

	public void Setup()
	{
		BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
		boxCollider2D.size = ColliderSize;
		boxCollider2D.edgeRadius = ColliderEdgeRadius;

		RigidBody.gravityScale = Gravity;
		RigidBody.drag = Drag;
		RigidBody.angularDrag = 0f;

		RigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
		if (ConstrainXPosition)
			RigidBody.constraints |= RigidbodyConstraints2D.FreezePositionX;
		if (ConstrainYPosition)
			RigidBody.constraints |= RigidbodyConstraints2D.FreezePositionY;

	}

	public virtual void InternalStart()
	{
	}

	public virtual void InternalUpdate()
	{
	}

	public virtual void InternalFixedUpdate()
	{
	}

	public virtual void OnHitPlayer()
	{

	}
}

[Flags]
public enum EnemyCollisionTypes
{
	Hurt,
	Stand,
}