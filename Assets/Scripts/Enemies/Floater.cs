// Ignore Spelling: Collider

using System.Collections;
using UnityEngine;

public class Floater : Enemy
{
	[Header("Base Floater Control")]
	public float BobCycleOffset = 0f;

	public virtual float BobMagnitude => 0.5f;
	public virtual float BobCycleLength => 2f;
	public override Vector2 ColliderSize => new(0.375f, 0.4375f);
	public override float ColliderEdgeRadius => 0.12f;
	public override float Gravity => 0f;
	public override bool ConstrainXPosition => true;
	public override float Drag => 0f;
	public override EnemyCollisionTypes CollisionTypes => base.CollisionTypes & EnemyCollisionTypes.Stand;
	public virtual bool FloaterOnRails => true;
	public Vector2 StartPos { get; set; }
	public virtual Vector2 RailedPosition => StartPos;
	private Vector2 RailedBobDifference => new(0, Mathf.Cos((Time.fixedTime + BobCycleOffset) * (2 * Mathf.PI) / BobCycleLength) * BobMagnitude / 2);
	private Vector2 FinalRailedPosition => RailedPosition + RailedBobDifference;

	public override void InternalStart()
	{
		base.InternalStart();
		StartPos = RigidBody.position;
	}

	public override void InternalFixedUpdate()
	{
		if (FloaterOnRails)
			RigidBody.MovePosition(FinalRailedPosition);
		else
			RigidBody.MovePosition(RigidBody.position + RailedBobDifference * Time.deltaTime);
	}

	private readonly float SpinCount = 10f;
	private readonly float SpinSpeedRadians = Mathf.PI * 5;
	private float Spin;

	public override void OnHitPlayer()
		=> StartCoroutine(SpinOutAnim());

	private IEnumerator SpinOutAnim()
	{
		RigidBody.constraints &= RigidbodyConstraints2D.FreezeRotation;

		while (Spin < SpinCount * 2 * Mathf.PI)
		{
			Spin += SpinSpeedRadians * Time.deltaTime;
			RigidBody.rotation = Spin * Mathf.Rad2Deg;
			yield return null;
		}

		RigidBody.rotation = 0;
		Spin = 0;
		RigidBody.constraints |= RigidbodyConstraints2D.FreezeRotation;
	}
}