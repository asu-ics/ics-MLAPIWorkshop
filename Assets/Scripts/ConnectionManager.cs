
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.PhotonRealtime;
using Photon.Realtime;


public class ConnectionManager : MonoBehaviour
{
    string room;
    public string username;
    public GameObject connectionPanel;

    void Start()
    {
        room = "default";
        username = "guest";

        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    public void OnDestroy()
    {
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;

    }

    public void OnCreateLobbyPressed()
    {
        room = GenerateRoomCode();
        Debug.Log(room);
        HostLobby(room);
        connectionPanel.SetActive(false);
    }


    public void OnJoinLobbyPressed()
    {
        JoinLobby(room);
        connectionPanel.SetActive(false);
    }

    void HostLobby(string roomName)
    {
        //Right now, I am implementing the "lobby code" using Photon's rooms. 
        //When "Create Lobby" is pressed, a random six letter string is generated, and the Photon room is set to that string
        //I don't believe there is a way to check if the room code already exists, but honestly I'm not gonna bother for this tutorial
        //it is a 1 in 308,915,776 chance of two people randomly generating the same sequence, of characters, and this is a demo
        //smh

        PhotonRealtimeTransport photonTransport = NetworkManager.Singleton.gameObject.GetComponent<PhotonRealtimeTransport>();

        photonTransport.RoomName = roomName;
        PhotonAppSettings.Instance.AppSettings.FixedRegion = "usw";

        NetworkManager.Singleton.StartHost();
    }

    void JoinLobby(string roomName)
    {



        PhotonRealtimeTransport photonTransport = NetworkManager.Singleton.gameObject.GetComponent<PhotonRealtimeTransport>();

        photonTransport.RoomName = room;
        PhotonAppSettings.Instance.AppSettings.FixedRegion = "usw";

        NetworkManager.Singleton.StartClient();
    }

    public void ChangeRoomName(string roomName)
    {
        room = roomName;
    }

    public void ChangeUsername(string name)
    {
        username = name;
    }

    private string GenerateRoomCode()
    {
        var roomCode = "";
        int length = 6;

        for (int i = 0; i < length; i++)
        {
            roomCode += ((char)(Random.Range(1, 26) + 64)).ToString().ToUpper();
        }

        return roomCode;

    }

    //_________ FOR ALL CONNECTIONS/DISCONNECTS
    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server works!");

            // calls newly created client and sets data
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerData>().SetData("Lobby", username, NetworkManager.Singleton.LocalClientId);
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<EnteredTheScene>().UpdateNames();
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<EnteredTheScene>().GoToNewSceneServerRpc(NetworkManager.Singleton.LocalClientId, "Lobby");
        }
    }

    private void HandleClientConnected(ulong clientID)
    {
        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Connected!");

            // calls newly created client and sets data
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerData>().SetData("Lobby", username, NetworkManager.Singleton.LocalClientId);
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<EnteredTheScene>().UpdateNames();
            NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<EnteredTheScene>().GoToNewSceneServerRpc(NetworkManager.Singleton.LocalClientId, "Lobby");
        }
    }

    private void HandleClientDisconnected(ulong clientID)
    {
        return;
    }

}