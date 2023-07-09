using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroText : MonoBehaviour
{
	[TextArea][SerializeField] private string[] itemInfo;
	[SerializeField] private float textSpeed = 0.01f;

	[Header("UI Elements")]
	[SerializeField] private TextMeshProUGUI itemInfoText;
	private readonly int currentlyDisplayingText = 0;

	public GameObject PlayButton;

	public void ActivateText()
	{
		PlayButton.SetActive(false);
		StartCoroutine(AnimateText());
	}

	private IEnumerator AnimateText()
	{
		for (int i = 0; i < itemInfo[currentlyDisplayingText].Length + 1; i++)
		{
			if (Input.GetKeyDown(Settings.CurrentSettings.Jump) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Mouse0))
				PlayButton.SetActive(true);

			itemInfoText.text = itemInfo[currentlyDisplayingText][..i];
			yield return new WaitForSeconds(textSpeed);
		}

		PlayButton.SetActive(true);
	}
}