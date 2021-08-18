using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class EnteredTheScene : NetworkBehaviour
{
    // when a new client enters the game, updates htier textboxes on local client's side
    public void UpdateNames()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            //skips itself; only runs for other clients
            if (player.GetComponent<PlayerData>().GetClientID() != NetworkManager.Singleton.LocalClientId)
            {
                player.GetComponentInChildren<TextMesh>().text = player.GetComponent<PlayerData>().Username.Value;
            }
        }

        UpdateNamesServerRpc(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerData>().GetClientUsername());
    }

    [ServerRpc]
    public void UpdateNamesServerRpc(string name)
    {
        UpdateNamesClientRpc(name);
    }

    // cleint call; function happens on all clients
    [ClientRpc]
    public void UpdateNamesClientRpc(string name)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        //finds the players who just entered and changes their box to match
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerData>().Username.Value.Equals(name))
            {
                player.GetComponentInChildren<TextMesh>().text = player.GetComponent<PlayerData>().Username.Value;
            }
        }
    }
}
