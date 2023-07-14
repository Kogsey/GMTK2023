// Ignore Spelling: Collider

using UnityEngine;

public class WinGameCollider : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.TryGetComponent(out PlayerController _))
			StateManager.WinGame();
	}
}