using UnityEngine;
using Utilities.Timing;

namespace Utilities.Damage
{
	public class DamageHandler : MonoBehaviour
	{
		private SpriteRenderer spriteRenderer;

		[Header("Base")]
		public GUI GUI;

		[Header("Health")]
		public const int MaxHitPoints = 3;
		public int HitPoints;
		private int ImmunityFlasher;

		[Header("Immunity")]
		public CountDownTimer ImmunityTimer;
		public bool Immune => !ImmunityTimer.Check();

		private void Start()
			=> spriteRenderer = GetComponent<SpriteRenderer>();

		private void Update()
			=> UpdateImmunity();

		public void OnHit(Enemy attacker)
		{
			if (Immune)
				return;

			attacker.OnHitPlayer();
			if (GUI != null)
				GUI.RemoveCog();
			HitPoints--;
			ImmunityTimer.Reset();

			if (HitPoints <= 0)
				StateManager.GameOver();
		}

		private void UpdateImmunity()
		{
			ImmunityTimer.Tick();
			ImmunityFlasher++;

			spriteRenderer.color = Immune ? new Color(1, 0, 0, ImmunityFlasher % 2 == 0 ? 0.5f : 1f) : Color.white;
		}

		#region Collision

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
		}

		#endregion Collision
	}
}