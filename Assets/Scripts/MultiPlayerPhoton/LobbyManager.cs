using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Available Prefabs (Must be inside Resources/ folder)")]
    [SerializeField] private GameObject[] availablePlayerPrefabs;

    public override void OnJoinedRoom()
    {
        AssignPlayerPrefab();
    }

    private void AssignPlayerPrefab()
    {
        if (availablePlayerPrefabs == null || availablePlayerPrefabs.Length == 0)
        {
            Debug.LogError("[LobbyManager] No player prefabs assigned in the Inspector!");
            return;
        }

        // Photon ActorNumber starts at 1:
        // Player 1 -> (1 - 1) % length = Index 0 (First Prefab)
        // Player 2 -> (2 - 1) % length = Index 1 (Second Prefab)
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        int playerIndex = (actorNum - 1) % availablePlayerPrefabs.Length;

        string assignedPrefabName = availablePlayerPrefabs[playerIndex].name;

        Hashtable customProps = new Hashtable
        {
            { "AssignedPrefab", assignedPrefabName }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);

        // Verification Log
        Debug.Log($"[LobbyManager] Local Actor {actorNum} (Player #{actorNum}) assigned Prefab [{playerIndex}]: {assignedPrefabName}");
    }

    public void StartGame(string gameSceneName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }
}