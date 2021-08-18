using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// is monobehavior because there are no RPC calls; all functions happen on the same computer (in this case, the server/host)
public class NetworkData : MonoBehaviour
{
    [SerializeField] public List<PlayerDataEntries> playerDataList = new List<PlayerDataEntries>();

    public void createNewEntry(string name, string room, ulong id)
    {
        playerDataList.Add(new PlayerDataEntries(name, room, id));
    }
}

// class for storing user data
// can also use struct but because there's a lot of data being stored-- class was better option 
[System.Serializable]
public class PlayerDataEntries
{
    [SerializeField] private string PlayerUsername;
    [SerializeField] private List<string> PlayerRooms = new List<string>();
    [SerializeField] private ulong PlayerUniqueID;
    [SerializeField] private int Score;
    [SerializeField] private int Health;

    public PlayerDataEntries(string username, string room, ulong uniqueID)
    {
        PlayerUsername = username;
        PlayerRooms.Add(room);
        PlayerUniqueID = uniqueID;
        Score = 0;
        Health = 0;
    }

    public string GetPlayerUsername()
    {
        return PlayerUsername;
    }

    public List<string> GetPlayerRoom()
    {
        return PlayerRooms;
    }

    public void SetPlayerRoom(string room)
    {
        PlayerRooms.Add(room);
    }

    public ulong GetPlayerUniqueID()
    {
        return PlayerUniqueID;
    }

    // for when client gets hit
    public void HitSomeone()
    {
        Score += 1;
    }

    // for when client hits someone 
    public void GotHit(int hit)
    {
        Health += 1;
    }
}

