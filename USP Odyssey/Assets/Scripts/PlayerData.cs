using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public Vector3 position;
    public int coins;
    public int lives;
    public string playerName;
    public List<string> collectedCollectibleIds = new List<string>();
}
