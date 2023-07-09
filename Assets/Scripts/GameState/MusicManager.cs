using UnityEngine;

public class MusicManager : MonoBehaviour
{
	private static MusicManager instance;
	public static MusicManager Instance => instance;

	public AudioSource AudioSource;

	public AudioClip[] PossibleSongs;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);
		ReloadMusic();
	}

	public bool MuteMusic { get => Settings.CurrentSettings.MuteMusic; set => Settings.CurrentSettings.MuteMusic = value; }

	public void ToggleMusic()
	{
		MuteMusic = !MuteMusic;
		ReloadMusic();
	}

	public void TryStopMusic()
	{
		if (AudioSource.isPlaying)
			AudioSource.Stop();
	}

	public void TryStartMusic()
	{
		if (AudioSource.clip != null && !MuteMusic)
			AudioSource.Play();
	}

	public void ReloadMusic(bool newSong = true)
	{
		TryStopMusic();
		if (newSong)
			AudioSource.clip = PossibleSongs[Random.Range(0, PossibleSongs.Length)];
		TryStartMusic();
	}
}