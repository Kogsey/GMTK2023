using UnityEngine;

namespace Utilities.Timing
{
	public readonly struct DeltaTimeSelector
	{
		public DeltaTimeSelector(DeltaTimeType deltaTimeType = DeltaTimeType.Default) => CurrentDeltaTimeType = deltaTimeType;

		public enum DeltaTimeType
		{
			DeltaTime,
			FixedDeltaTime,
			Default = DeltaTime,
		}

		public readonly DeltaTimeType CurrentDeltaTimeType { get; }

		public float DeltaTime => CurrentDeltaTimeType switch
		{
			DeltaTimeType.DeltaTime => Time.deltaTime,
			DeltaTimeType.FixedDeltaTime => Time.fixedDeltaTime,
			_ => Time.deltaTime,
		};

		public static implicit operator float(DeltaTimeSelector deltaTimeSelector) => deltaTimeSelector.DeltaTime;
	}
}