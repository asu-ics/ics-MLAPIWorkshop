using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using MLAPI.NetworkVariable;
using MLAPI.Connection;

public class SpawnBall : NetworkBehaviour
{
    [SerializeField] private Transform RightHand;
    [SerializeField] private GameObject Ball;
    [SerializeField] private Camera playercamera;


    [SerializeField]
    private NetworkVariableBool equipped = new NetworkVariableBool();


    void Start()
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out NetworkClient networkClient))
        {
            return;
        }


        equipped.Value = false;
    }
    // Update is called once per frame
    void Update()
    {
        //We check if the local player is the one pressing the spawn ball key
        if (IsLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                SpawnBallServerRpc(NetworkManager.Singleton.LocalClientId);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (equipped.Value == false)
                {
                    //This needs to be the position of the camera
                    Vector3 start = new Vector3((playercamera.pixelWidth)/2, (playercamera.pixelHeight)/2, 0);
                    //This needs to be the direction the camera is facing
                    Vector3 direction = Vector3.forward;
                    RaycastHit hit;
                    Ray ray = playercamera.ScreenPointToRay(start);

                    if(Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        Debug.Log("ray is cast");
                        Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
                        if(hit.collider.gameObject.tag == "Ball")
                        {
                            Debug.Log("found ball");
                            EquipBallServerRpc(NetworkManager.Singleton.LocalClientId, hit.collider.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
                        }
                    }   
                } 
                else
                {
                    ThrowBallServerRpc(NetworkManager.Singleton.LocalClientId);
                }
            }
        }
    }

    [ServerRpc]
    private void EquipBallServerRpc(ulong netID, ulong itemID)
    {
        NetworkObject netObj = NetworkSpawnManager.SpawnedObjects[itemID];   
        netObj.ChangeOwnership(netID);

        EquipClientRpc(itemID);
        equipped.Value = true;

    }

    [ServerRpc]
    private void ThrowBallServerRpc(ulong netID)
    {
        Transform equippedBall = RightHand.GetChild(0);

        Debug.Log(equippedBall + " is the equipped ball");
        if (equippedBall == null)
        {
            return;
        }

       // equippedBall.gameObject.GetComponent<NetworkObject>().Despawn();
        NetworkManager.Destroy(equippedBall.gameObject);
        equipped.Value = false;
        Debug.Log("throwing ball");

    }

    [ServerRpc]
    private void SpawnBallServerRpc(ulong netID)
    {
        GameObject _ball = Instantiate(Ball, RightHand.transform.position, Quaternion.identity);
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
        netObj.gameObject.transform.position = RightHand.position;
    }

    [ClientRpc]
    private void EquipClientRpc(ulong itemID)
    {
        NetworkObject netObj = NetworkSpawnManager.SpawnedObjects[itemID];
        //Spawn the object in front of the player's hand
        netObj.gameObject.transform.position = RightHand.position;
        netObj.gameObject.transform.SetParent(RightHand);

    }
}
