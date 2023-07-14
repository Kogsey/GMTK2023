using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public TextMeshProUGUI MusicText;

	public void ButtonPlayGame()
		=> SceneManager.LoadScene(StateManager.GameScreen);

	public void ButtonOpenMenu()
		=> SceneManager.LoadScene(StateManager.MainMenuScreen);

	public void ButtonToggleSFX()
	{
		MusicManager.Instance.ToggleMusic();
		MusicText.text = "Music: " + (MusicManager.Instance.MuteMusic ? "Off" : "On");
	}

	public void SetKeyBindingCeleste()
		=> Settings.CurrentSettings.CopyControlsFrom(Settings.CelesteStyleSettings);

	public void SetKeyBindingDefault()
		=> Settings.CurrentSettings.CopyControlsFrom(Settings.DefaultSettings);

	public static KeyCode? GetAnyKeyDown()
	{
		foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown(keyCode))
				return keyCode;
		}
		return null;
	}
}