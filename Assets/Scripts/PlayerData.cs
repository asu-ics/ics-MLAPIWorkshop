using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

// has to be network behavior;
// server call happens on server, not on client
public class PlayerData : NetworkBehaviour
{
    // so that all clients can view all other clients current wereabouts (used for multi-scene)
    // without having to ask either the owner client or the server
    // c_room is the network variable; Room is a local variable used to access c_room
    [SerializeField] private NetworkVariableString c_Room = new NetworkVariableString("Lobby");
    public NetworkVariableString Room => c_Room;

    // same thing but w/ username
    [SerializeField] private NetworkVariableString c_Username = new NetworkVariableString("Name");
    public NetworkVariableString Username => c_Username;

    // can only be viewed by the local client
    [SerializeField] private string clientUsername;
    [SerializeField] private ulong clientID;

    public ulong GetClientID()
    {
        return clientID;
    }

    public string GetClientUsername()
    {
        return clientUsername;
    }

    // sets data for local client as well as makes an RPC call
    public void SetData(string room, string username, ulong id)
    {
        clientUsername = username;
        clientID = id;
        // RPC call; basically client telling server to run specified lines of code on their(server's) side
        SetDataServerRpc(room, clientUsername, id);
    }

    // server call; function happens on server, not on client!!!!!
    [ServerRpc]
    public void SetDataServerRpc(string room, string userName, ulong id)
    {
        //Firstly, looks finds client on server w/ ID of local client; sets client's Room variable
        //REMEMBER: network variables can only be changed on server!!
        NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerData>().Room.Value = "LobbyClient";
        NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerData>().Username.Value = userName;

        // call function to log client entry into database 
        NetworkManager.GetComponent<NetworkData>().createNewEntry(userName, "LobbyClient", id);
    }
}
