using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Timing
{
	public interface ITimer
	{
		public DeltaTimeSelector DeltaTime { get; }
		public float Time { get; set; }

		public float MaxTime { get; set; }

		public abstract void Tick();

		public abstract bool Check();

		public abstract void Reset();

		public abstract void Force();
	}

	[Serializable]
	public struct CountDownTimer : ITimer
	{
		public CountDownTimer(float maxTime)
		{
			Time = maxTime;
			MaxTime = maxTime;
			DeltaTime = new(DeltaTimeSelector.DeltaTimeType.Default);
		}

		[field: SerializeField]
		public float Time { get; set; }

		[field: SerializeField]
		public float MaxTime { get; set; }

		public DeltaTimeSelector DeltaTime { get; }

		public readonly bool Check() => Time <= 0;

		public void Reset() => Time = MaxTime;

		public void Tick() => Time -= DeltaTime;

		public void Force() => Time = -1;
	}

	[Serializable]
	public struct CountUpTimer : ITimer
	{
		public CountUpTimer(float maxTime)
		{
			Time = maxTime;
			MaxTime = maxTime;
			DeltaTime = new(DeltaTimeSelector.DeltaTimeType.Default);
		}

		[field: SerializeField]
		public float Time { get; set; }

		[field: SerializeField]
		public float MaxTime { get; set; }

		public DeltaTimeSelector DeltaTime { get; }

		public readonly bool Check() => Time >= MaxTime;

		public void Reset() => Time = 0;

		public void Tick() => Time += DeltaTime;

		public void Force() => Time = MaxTime + 1;
	}

	[Serializable]
	public class TimerController
	{
		[SerializeField]
		private readonly List<ITimer> Timers;

		public void AddTimer(ITimer timer) => Timers.Add(timer);

		public void Tick() => Timers.ForEach((timer) => timer.Tick());
	}
}