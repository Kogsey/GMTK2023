using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities.UI
{
	public class TextTyper : MonoBehaviour
	{
		[TextArea][SerializeField] private string itemInfo;

		[Header("UI Elements")]
		[SerializeField] private TextMeshProUGUI itemInfoText;

		public UnityEvent OnTextFinished;
		private Coroutine coroutine;

		public void StartTyping()
			=> coroutine = StartCoroutine(AnimateText());

		private IEnumerator AnimateText()
		{
			for (int i = 0; i < itemInfo.Length + 1; i++)
			{
				itemInfoText.text = itemInfo[..i];
				yield return new WaitForSeconds(Globals.TextSpeedSecondsPerChar); ;
			}
			OnTextFinished.Invoke();
		}

		private void Update()
		{
			if (Input.GetKeyDown(Settings.CurrentSettings.Jump) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Mouse0))
			{
				OnTextFinished.Invoke();
				if (coroutine != null)
					StopCoroutine(coroutine);
				itemInfoText.text = itemInfo;
			}
		}
	}
}