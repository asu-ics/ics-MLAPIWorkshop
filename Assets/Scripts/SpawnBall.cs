using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;

public class SpawnBall : NetworkBehaviour
{
    [SerializeField] private Transform RightHand;
    [SerializeField] private GameObject Ball;

    // Update is called once per frame
    void Update()
    {
        //We check if the local player is the one pressing the spawn ball key
        if (IsLocalPlayer)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                SpawnBallServerRpc(NetworkManager.Singleton.LocalClientId);
            }

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                //This needs to be the position of the camera
                Vector3 start = Vector3.zero;
                //This needs to be the direction the camera is facing
                Vector3 direction = Vector3.forward;
                RaycastHit hit;
                if(Physics.Raycast(start, direction, out hit))
                {
                    if(hit.collider.gameObject.name == "Sphere");
                }
                EquipBallServerRpc(NetworkManager.Singleton.LocalClientId, hit.collider.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }

    [ServerRpc]
    private void EquipBallServerRpc(ulong netID, ulong itemID)
    {
        NetworkObject netObj = NetworkSpawnManager.SpawnedObjects[itemID];   
        netObj.ChangeOwnership(netID);

        //Spawn the object in front of the player's hand
        netObj.gameObject.transform.position = RightHand.position;
        netObj.gameObject.transform.SetParent(RightHand);

    }

    [ServerRpc]
    private void SpawnBallServerRpc(ulong netID)
    {
        GameObject _ball = Instantiate(Ball);
        //The Spawn() method is inside the NetworkObject that we attach to the Ball prefab
        _ball.GetComponent<NetworkObject>().SpawnWithOwnership(netID);

        //Get the unique item id for the new ball we just spawned and send it to every client
        ulong itemID = _ball.GetComponent<NetworkObject>().NetworkObjectId;

        SpawnOverNetworkClientRpc(itemID);
    }

    [ClientRpc]
    private void SpawnOverNetworkClientRpc(ulong itemID)
    {
        //Spawned an object on all clients
        NetworkObject netObj = NetworkSpawnManager.SpawnedObjects[itemID];   

        //Drop the object at 0,0,0 or...
        // netObj.gameObject.transform.position = Vector3.zero; 
        // netObj.gameObject.transform.rotation = Quaternion.identity;

        //Spawn the object in front of the player's hand
        netObj.gameObject.transform.position = RightHand.position + new Vector3(2,0,0);
      
    }
}
