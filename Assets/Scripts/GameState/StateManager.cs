using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public record Settings
{
	public KeyCode Left = KeyCode.A;
	public KeyCode Right = KeyCode.D;
	public KeyCode Up = KeyCode.W;
	public KeyCode Down = KeyCode.S;
	public KeyCode Jump = KeyCode.Space;
	public KeyCode Dash = KeyCode.LeftShift;

	public float Volume = 1f;
	public bool MuteMusic = false;

	public static readonly Settings DefaultSettings = new();
	public static readonly Settings CelesteStyleSettings = new()
	{
		Left = KeyCode.LeftArrow,
		Right = KeyCode.RightArrow,
		Up = KeyCode.UpArrow,
		Down = KeyCode.DownArrow,
		Jump = KeyCode.C,
		Dash = KeyCode.X,
	};
	public static Settings CurrentSettings { get; set; } = DefaultSettings;

	public void CopyControlsFrom(Settings settings)
	{
		Left = settings.Left;
		Right = settings.Right;
		Up = settings.Up;
		Down = settings.Down;

		Jump = settings.Jump;
		Dash = settings.Dash;
	}
}

public class StateManager : MonoBehaviour
{
	public const string MainMenuScreen = "Menu";
	public const string GameScreen = "Primary Level";
	public const string WinScreen = "Win Game";
	public const string LoseScreen = "Lose Game";

	public GameObject MusicManagerPrefab;

	private StateManager instance;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Destroy(this);
			return;
		}
		DontDestroyOnLoad(gameObject);
		Instantiate(MusicManagerPrefab);
		Settings.CurrentSettings = SaveSystem.Load();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (SceneManager.GetActiveScene().name == MainMenuScreen)
				QuitGame();
			else if (SceneManager.GetActiveScene().name == GameScreen)
				SpawnQuitGamePlayPopUp();
		}
	}

	public void SpawnQuitGamePlayPopUp() // TODO: low priority: end game pop-up
	{
		SceneManager.LoadScene(MainMenuScreen);
	}

	public static void GameOver()
		=> SceneManager.LoadScene(LoseScreen);

	public static void WinGame()
		=> SceneManager.LoadScene(WinScreen);

	public static void QuitGame()
	{
		SaveSystem.Save();
		Application.Quit();
	}
}