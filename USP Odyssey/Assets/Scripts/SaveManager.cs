using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

public static class SaveManager
{
    private static readonly string saveFilePath = Path.Combine(Application.persistentDataPath, "savedata.json");
    private static readonly string tempFilePath = saveFilePath + ".tmp";
    private static readonly string backupFilePath = saveFilePath + ".bak";

    public static bool SaveGame(PlayerData data)
    {
        return SaveGameAsync(data).GetAwaiter().GetResult();
    }

    public static async Task<bool> SaveGameAsync(PlayerData data)
    {
        try
        {
            string directory = Path.GetDirectoryName(saveFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(data, true);

            using (FileStream tempStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            using (StreamWriter writer = new StreamWriter(tempStream))
            {
                await writer.WriteAsync(json);
            }

            if (File.Exists(saveFilePath))
            {
                await Task.Run(() => File.Copy(saveFilePath, backupFilePath, true));
            }

            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            File.Move(tempFilePath, saveFilePath);
            Debug.Log($"Game data saved to {saveFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save game data: {ex.Message}");
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (Exception cleanupEx)
                {
                    Debug.LogWarning($"Unable to clean up temporary save file: {cleanupEx.Message}");
                }
            }
            return false;
        }
    }

    public static PlayerData LoadGame()
    {
        try
        {
            if (TryReadFile(saveFilePath, out PlayerData dataFromPrimary))
            {
                return dataFromPrimary;
            }

            if (TryReadFile(backupFilePath, out PlayerData dataFromBackup))
            {
                Debug.LogWarning("Primary save file was unavailable; loaded from backup.");
                File.Copy(backupFilePath, saveFilePath, true);
                return dataFromBackup;
            }

            Debug.LogWarning("Save file not found or is empty. Returning null.");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load game data: {ex.Message}");
            return null;
        }
    }

    private static bool TryReadFile(string path, out PlayerData data)
    {
        data = null;

        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            data = JsonUtility.FromJson<PlayerData>(json);
            if (data == null)
            {
                return false;
            }

            Debug.Log($"Game data loaded from {path}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Unable to load data from {path}: {ex.Message}");
            return false;
        }
    }
}
