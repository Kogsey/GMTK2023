using UnityEngine;

public class PatrolFloater : Floater
{
	[Header("Patrol Floater Control")]
	public float PathLength;

	[Range(0f, 1f)]
	public float PathOffset;
	public float PathAngleDegrees;
	public float PathTime;

	public override bool ConstrainXPosition => false;
	public override bool ConstrainYPosition => false;
	private float PosLerp => Time.fixedTime * PathTime + PathOffset;
	private float PathAngleRadians => PathAngleDegrees * Mathf.Deg2Rad;
	private Vector2 PathAngleVector => new(Mathf.Cos(PathAngleRadians), Mathf.Sin(PathAngleRadians));
	private float FinalLerp => (Mathf.Cos(PosLerp / (2 * Mathf.PI)) + 1) / 2;
	public override Vector2 RailedPosition => StartPos + PathLength * FinalLerp * PathAngleVector;

}