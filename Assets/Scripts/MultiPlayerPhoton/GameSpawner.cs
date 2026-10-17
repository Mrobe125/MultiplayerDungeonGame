using UnityEngine;
using Photon.Pun;

public class GameSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawn Locations")]
    [SerializeField] private Transform[] spawnPoints;

    // Static flag prevents duplicates across script re-evaluations or scene reload events
    private static bool hasSpawned = false;

    private void Awake()
    {
        // Reset flag whenever the game scene loads afresh
        hasSpawned = false;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        // Stop execution if already spawned locally
        if (hasSpawned) return;

        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("AssignedPrefab", out object prefabObj))
        {
            Debug.LogError("[GameSpawner] No 'AssignedPrefab' custom property found for LocalPlayer!");
            return;
        }

        string prefabToSpawn = (string)prefabObj;

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex = Mathf.Abs(PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;

            if (spawnPoints[spawnIndex] != null)
            {
                spawnPosition = spawnPoints[spawnIndex].position;
                spawnRotation = spawnPoints[spawnIndex].rotation;
            }
        }
        else
        {
            Debug.LogWarning("[GameSpawner] No spawn points assigned in GameSpawner! Spawning at (0,0,0).");
        }

        // Lock spawn state BEFORE network instantiation
        hasSpawned = true;

        PhotonNetwork.Instantiate(
            prefabToSpawn,
            spawnPosition,
            spawnRotation
        );

        Debug.Log($"[GameSpawner] Successfully spawned {prefabToSpawn} for Actor {PhotonNetwork.LocalPlayer.ActorNumber}");
    }
}