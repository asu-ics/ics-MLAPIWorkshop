using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine.SceneManagement;

public class EnteredTheScene : NetworkBehaviour
{
    //___________________________ FOR UPDATING TEXTBOX NAMES ____________________________

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

    //___________________________ FOR CHANGING SCENES ____________________________

    public void GoToNewScene(Collider other, string RoomName)
    {
        // this should only be ran on the client side so this if statement should always run
        ulong temp = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerData>().GetClientID();
        if (temp == other.GetComponent<PlayerData>().GetClientID())
        {
            StartCoroutine(changePlayerScene(other, RoomName));
        }
    }
    // changes scene on client side; adds new scene
    // at most 2 scenes running; lobby + new scene 
    public IEnumerator changePlayerScene(Collider other, string RoomName)
    {
        // 3 situations -> from lobby to new room, from new room to new room, from new room to lobby
        // situation 1: lobby
        Scene pastScene = SceneManager.GetSceneByName(other.GetComponent<PlayerData>().GetCurrentRoom()); 
        if (pastScene.name != "Lobby")
        {
            // for client going from new room to lobby
            if (RoomName.Equals("Lobby"))
            {
                // unloads old scene, moves everything back to lobby
                //SceneManager.SetActiveScene(SceneManager.GetSceneByName(RoomName));       // CANNOT DO

                SceneManager.MoveGameObjectToScene(other.gameObject, SceneManager.GetSceneByName(RoomName));
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(pastScene);
                while (!asyncUnload.isDone)
                {
                    yield return null;
                }
                other.GetComponent<PlayerData>().SetCurrentRoom(RoomName); 
                Scene current = SceneManager.GetSceneByName(other.GetComponent<PlayerData>().GetCurrentRoom()); 
                SetSceneObjects(current, true); 
            }
            // for client going from new room to new room
            else
            {
                // sees if scene has already been loaded; loads scene
                Scene sceneToLoad = SceneManager.GetSceneByName(RoomName);
                if (sceneToLoad.name == null)
                {
                    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(RoomName, LoadSceneMode.Additive);
                    while (!asyncLoad.isDone)
                    {
                        yield return null;
                    }
                }
                // moves everything over to new scene/unloads old scene 
                //SceneManager.SetActiveScene(SceneManager.GetSceneByName(RoomName));
                other.GetComponent<PlayerData>().SetCurrentRoom(RoomName);
                SceneManager.MoveGameObjectToScene(other.gameObject, SceneManager.GetSceneByName(RoomName));
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(pastScene);
                while (!asyncUnload.isDone)
                {
                    yield return null;
                }
                //SetSceneObjects(SceneManager.GetActiveScene(), true);
            }
        }

        // for client going from lobby to new room 
        if (pastScene.name == "Lobby")
        {
            Scene sceneToLoad = SceneManager.GetSceneByName(RoomName);
            if (sceneToLoad.name == null)
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(RoomName, LoadSceneMode.Additive);
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }
            //SceneManager.SetActiveScene(SceneManager.GetSceneByName(RoomName));
            other.GetComponent<PlayerData>().SetCurrentRoom(RoomName);
            SceneManager.MoveGameObjectToScene(other.gameObject, SceneManager.GetSceneByName(RoomName));

            //SetSceneObjects(SceneManager.GetActiveScene(), true);
            SetSceneObjects(pastScene, false);
        }

        other.GetComponent<PlayerData>().SetCurrentRoom(RoomName);
        GoToNewSceneServerRpc(NetworkManager.Singleton.LocalClientId, RoomName);




        /* without bells and whistles 

        // sees if scene has already been loaded
        Scene sceneToLoad = SceneManager.GetSceneByName(RoomName);
        if (sceneToLoad.name == null)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(RoomName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }


        SceneManager.SetActiveScene(SceneManager.GetSceneByName(RoomName));
        SceneManager.MoveGameObjectToScene(other.gameObject, SceneManager.GetSceneByName(RoomName));
        GoToNewSceneServerRpc(NetworkManager.Singleton.LocalClientId, RoomName);


        */
    }

    public void SetSceneObjects(Scene scene, bool setActiceBool)
    {
        GameObject[] objects = scene.GetRootGameObjects();
        foreach (var obj in objects)
        {
            if(obj.tag != "Player" && obj.name != "EventSystem")
            {
                obj.SetActive(setActiceBool);
            }
        }
    }

    // server call
    // updates variables 
    [ServerRpc]
    public void GoToNewSceneServerRpc(ulong hitUniqueID, string RoomName)
    {
        // updates network variables / adds room to server database 
        NetworkManager.Singleton.ConnectedClients[hitUniqueID].PlayerObject.GetComponent<PlayerData>().Room.Value = RoomName;
        foreach (PlayerDataEntries players in NetworkManager.GetComponent<NetworkData>().playerDataList)
        {
            if (players.GetPlayerUniqueID() == hitUniqueID)
            {
                players.SetPlayerRoom(RoomName);
            }
        }
        GoToNewSceneClientRpc(hitUniqueID, RoomName);
    }

    public IEnumerator checkSynch(ulong hitUniqueID, string RoomName)
    {
        string currentRoom = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerData>().Room.Value;

        // loop through all players on each client; render/hide others depending on room status
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            // for making sure that client has updated value of ServerRPC variable change for player that's changing rooms
            if ((ushort)hitUniqueID == player.GetComponent<PlayerData>().ClientID.Value)
            {
                bool endLoop = false;
                while (!endLoop)
                {
                    if (player.GetComponent<PlayerData>().Room.Value.Equals(RoomName))
                    {
                        endLoop = true;
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }

        // if it's the client that hit the door; udpdates current room incase it was changed
        currentRoom = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerData>().Room.Value;

        foreach (GameObject player in players)
        {
            // now that all variables are synched, sees which players are visible/invisible to client 
            MeshRenderer[] Renderers = player.GetComponentsInChildren<MeshRenderer>();
            Collider[] Colliders = player.GetComponentsInChildren<Collider>();
            if (player.GetComponent<PlayerData>().Room.Value.Equals(currentRoom)){
                foreach (MeshRenderer rend in Renderers){
                    rend.enabled = true;
                }
                foreach (Collider coll in Colliders){
                    coll.enabled = true;
                }
            }
            else{
                foreach (MeshRenderer rend in Renderers){
                    rend.enabled = false;
                }
                foreach (Collider coll in Colliders){
                    coll.enabled = false;
                }
            }
        }
    }
    [ClientRpc]
    // sees which clients are in which room; for each client -> renders other clients that are in the same room/disables clients not in the same room
    public void GoToNewSceneClientRpc(ulong hitUniqueID, string RoomName)
    {   // idea: loop through, if clients in same room, be visible; else be invisible
        // problem: when network variable updated in server RPC, latency for updating clients w/ new information
        //      ie- ClientA goes RedRoom -> BlueRoom, ClientB(in RedRoom) runs visible/invisible then recieves that ClientA is in BlueRoom
                        // ClientB says that A should be visible -> wrong; need to wait for update before starting
        StartCoroutine(checkSynch(hitUniqueID, RoomName));
    }
}
