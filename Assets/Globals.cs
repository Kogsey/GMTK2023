using UnityEngine;

public static class Globals
{
	public static float TextSpeedCharsPerSecond { get; set; } = 100f;

	public static float TextSpeedSecondsPerChar
	{
		get => 1f / TextSpeedCharsPerSecond;
		set => TextSpeedCharsPerSecond = 1f / value;
	}
}