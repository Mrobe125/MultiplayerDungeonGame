using System.Collections;
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
        // Try spawning immediately on Start
        StartCoroutine(TrySpawnWithRetry());
    }

    public override void OnEnable()
    {
        base.OnEnable();
        TrySpawnPlayer();
    }

    public override void OnJoinedRoom()
    {
        TrySpawnPlayer();
    }

    //public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, System.Collections.Hashtable changedProps)
    //{
    //    // Check if the update belongs to local player and contains the prefab key
    //    if (targetPlayer.IsLocal && changedProps.ContainsKey("AssignedPrefab"))
    //    {
    //        TrySpawnPlayer();
    //    }
    //}

    private IEnumerator TrySpawnWithRetry()
    {
        // Retry loop to handle race conditions during scene load
        float timeout = 10f;
        float timer = 0f;

        while (!hasSpawned && timer < timeout)
        {
            TrySpawnPlayer();

            if (hasSpawned) break;

            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }

        if (!hasSpawned)
        {
            Debug.LogError($"[GameSpawner] Failed to spawn Actor {PhotonNetwork.LocalPlayer.ActorNumber} after 10 seconds. Check if 'AssignedPrefab' custom property is set.");
        }
    }

    private void TrySpawnPlayer()
    {
        if (hasSpawned) return;

        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom) return;

        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("AssignedPrefab", out object prefabObj))
        {
            Debug.LogWarning($"[GameSpawner] Waiting for 'AssignedPrefab' custom property for Actor {PhotonNetwork.LocalPlayer.ActorNumber}...");
            return;
        }

        string prefabToSpawn = (string)prefabObj;

        if (string.IsNullOrEmpty(prefabToSpawn))
        {
            Debug.LogError($"[GameSpawner] 'AssignedPrefab' property was empty for Actor {PhotonNetwork.LocalPlayer.ActorNumber}!");
            return;
        }

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

        // Instantiate across the network for the local client
        GameObject playerObj = PhotonNetwork.Instantiate(
            prefabToSpawn,
            spawnPosition,
            spawnRotation
        );

        PlayerMovement movementScript = playerObj.GetComponent<PlayerMovement>();
        if (movementScript != null)
        {
            movementScript.InitializePlayer(PhotonNetwork.LocalPlayer);
        }

        Debug.Log($"[GameSpawner] Successfully spawned '{prefabToSpawn}' for Actor {PhotonNetwork.LocalPlayer.ActorNumber}");
    }
}