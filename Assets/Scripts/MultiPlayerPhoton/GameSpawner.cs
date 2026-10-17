using UnityEngine;
using Photon.Pun;

public class GameSpawner : MonoBehaviour
{
    [Header("Spawn Locations")]
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("AssignedPrefab", out object prefabObj))
        {
            Debug.LogError("No assigned prefab property found for LocalPlayer!");
            return;
        }

        string prefabToSpawn = (string)prefabObj;

        // Fallback to transform position if no spawn points are assigned in Inspector
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;

            // Ensure array slot isn't null
            if (spawnPoints[spawnIndex] != null)
            {
                spawnPosition = spawnPoints[spawnIndex].position;
                spawnRotation = spawnPoints[spawnIndex].rotation;
            }
        }
        else
        {
            Debug.LogWarning("No spawn points assigned in GameSpawner! Spawning at (0,0,0).");
        }

        // Instantiate over the network
        PhotonNetwork.Instantiate(
            prefabToSpawn,
            spawnPosition,
            spawnRotation
        );
    }
}