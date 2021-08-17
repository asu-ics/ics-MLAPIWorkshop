using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class DoorEntered : MonoBehaviour
{
    public string RoomName;

    // runs when something hits the door
    // if a player, changes player's room / updates other clients on player's wereabouts 
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // only if client that goes through door is host
            if (other.GetComponent<PlayerData>().GetIsHost())
            {
                NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<EnteredTheScene>().GoToNewScene(other, RoomName);
            }
            // for all other clients 
            else
            {
                if (!NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerData>().GetIsHost())
                {
                    ulong temp = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerData>().GetClientID();
                    if (temp == other.GetComponent<PlayerData>().GetClientID())
                    {
                        NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<EnteredTheScene>().GoToNewScene(other, RoomName);
                    }
                }
            }
        }
    }
}