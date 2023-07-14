using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Utilities.Damage;

public class GUI : MonoBehaviour
{
	public float SpacingXEach;
	public Vector2 SpacingInitial;
	public float ExitDegreesPerSecond;
	private const float CogRadius = 0.5f;
	public int StartCogCount => DamageHandler.MaxHitPoints;

	public List<Transform> Cogs = new();
	public Transform CogPrefab;
	public Camera Camera;
	public PixelPerfectCamera PixelPerfectCamera;
	public Vector2 ScreenTopLeft => Camera.ScreenToWorldPoint(new Vector3(0f, Screen.height));

	// Start is called before the first frame update
	private void Start()
	{
		for (int i = 0; i < StartCogCount; i++)
		{
			Cogs.Add(Instantiate(CogPrefab, transform));
			Cogs[i].position = new Vector3(GetCogPos(i).x, GetCogPos(i).y, Cogs[i].position.z);
		}
	}

	public Vector2 GetCogPos(int index)
		=> ScreenTopLeft + SpacingInitial + new Vector2(SpacingXEach * index, 0f);

	public void RemoveCog()
	{
		if (Cogs.Count <= 0)
			return;
		StartCoroutine(RollOutCog(Cogs[^1]));
		Cogs.Remove(Cogs[^1]);
	}

	public IEnumerator RollOutCog(Transform cog)
	{
		float timer = 10f;
		while (timer >= 0)
		{
			cog.Rotate(new Vector3(0, 0, ExitDegreesPerSecond * Time.unscaledDeltaTime));
			cog.transform.position -= new Vector3(2 * Mathf.PI * CogRadius * (ExitDegreesPerSecond * Time.unscaledDeltaTime / 360), 0, 0);
			timer -= Time.unscaledDeltaTime;
			yield return null;
		}

		Destroy(cog.gameObject);
	}
}