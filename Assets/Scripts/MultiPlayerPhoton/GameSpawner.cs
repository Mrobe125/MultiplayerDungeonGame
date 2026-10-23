using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class GameSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawn Locations")]
    [SerializeField] private Transform[] spawnPoints;

    private bool hasSpawned = false;

    private void Start()
    {
        TrySpawnPlayer();
    }

    public override void OnJoinedRoom()
    {
        TrySpawnPlayer();
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer.IsLocal && changedProps.ContainsKey("AssignedPrefab"))
        {
            TrySpawnPlayer();
        }
    }

    private void TrySpawnPlayer()
    {
        if (hasSpawned) return;

        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom) return;

        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("AssignedPrefab", out object prefabObj))
        {
            Debug.LogWarning($"[GameSpawner] Waiting for 'AssignedPrefab' custom property to sync for Actor {PhotonNetwork.LocalPlayer.ActorNumber}...");
            return;
        }

        string prefabToSpawn = (string)prefabObj;

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;

            if (spawnPoints[spawnIndex] != null)
            {
                spawnPosition = spawnPoints[spawnIndex].position;
                spawnRotation = spawnPoints[spawnIndex].rotation;
            }
        }

        hasSpawned = true;

        GameObject playerObj = PhotonNetwork.Instantiate(
            prefabToSpawn,
            spawnPosition,
            spawnRotation
        );

        Debug.Log($"[GameSpawner] Successfully spawned {prefabToSpawn} for Actor {PhotonNetwork.LocalPlayer.ActorNumber}");
    }
}