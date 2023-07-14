using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
	public static readonly string SavePath = Path.Combine(Application.persistentDataPath, "Saves.Values");

	public static void Save()
	{
		Debug.Log("Beginning save...");
		BinaryFormatter formatter = new();
		using FileStream stream = new(SavePath, FileMode.Create);
		Settings settingsSave = Settings.CurrentSettings;

		formatter.Serialize(stream, settingsSave);
		stream.Close();
		Debug.Log("Saved!");
	}

	public static Settings Load()
	{
		Debug.Log("Beginning load...");

		if (File.Exists(SavePath))
		{
			Debug.Log("File found...");

			try
			{
				Debug.Log("Starting Stream...");

				BinaryFormatter formatter = new();
				using FileStream stream = new(SavePath, FileMode.Open);
				Settings loadPacket = new();
				loadPacket = formatter.Deserialize(stream) as Settings;

				if (loadPacket == null)
				{
					loadPacket = Settings.CurrentSettings;
				}

				stream.Close();
				Debug.Log("Loaded!");
				return loadPacket;
			}
			catch (SerializationException)
			{
				Debug.Log("Load failed due to " + nameof(SerializationException) + ". Setting defaults.");
				File.Delete(SavePath);
			}
		}

		return Settings.CurrentSettings;
	}
}