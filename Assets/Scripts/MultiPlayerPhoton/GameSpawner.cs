using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawn Locations")]
    [Tooltip("Element 0 = Player 1, Element 1 = Player 2")]
    [SerializeField] private Transform[] spawnPoints;

    private bool hasSpawned = false;

    private void Start()
    {
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

    // Listens for custom property updates when scene loads or properties sync
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer.IsLocal && changedProps.ContainsKey("AssignedPrefab"))
        {
            TrySpawnPlayer();
        }
    }

    private IEnumerator TrySpawnWithRetry()
    {
        float timeout = 15f;
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
            Debug.LogError($"[GameSpawner] Failed to spawn Actor {PhotonNetwork.LocalPlayer.ActorNumber}. 'AssignedPrefab' property was not set in CustomProperties.");
        }
    }

    private void TrySpawnPlayer()
    {
        if (hasSpawned) return;

        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom) return;

        // Strictly retrieve assigned prefab name from local player CustomProperties
        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("AssignedPrefab", out object prefabObj) || prefabObj == null)
        {
            Debug.LogWarning($"[GameSpawner] Waiting for 'AssignedPrefab' custom property for Actor {PhotonNetwork.LocalPlayer.ActorNumber}...");
            return;
        }

        string prefabToSpawn = (string)prefabObj;

        if (string.IsNullOrEmpty(prefabToSpawn))
        {
            Debug.LogError($"[GameSpawner] 'AssignedPrefab' property is empty for Actor {PhotonNetwork.LocalPlayer.ActorNumber}!");
            return;
        }

        // Determine Spawn Point: Master/Player 1 -> Index 0 | Player 2 -> Index 1
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            bool isPlayer1 = PhotonNetwork.IsMasterClient || PhotonNetwork.LocalPlayer.ActorNumber == 1;
            int spawnIndex = isPlayer1 ? 0 : 1;

            if (spawnIndex >= spawnPoints.Length)
            {
                spawnIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
            }

            if (spawnPoints[spawnIndex] != null)
            {
                spawnPosition = spawnPoints[spawnIndex].position;
                spawnRotation = spawnPoints[spawnIndex].rotation;
            }
        }

        hasSpawned = true;

        // Instantiate player prefab from Resources folder across the network
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

        Debug.Log($"[GameSpawner] Successfully spawned strictly assigned prefab '{prefabToSpawn}' for Actor {PhotonNetwork.LocalPlayer.ActorNumber}");
    }
}