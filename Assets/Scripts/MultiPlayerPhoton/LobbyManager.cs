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

        // Use ActorNumber to guarantee unique assignment (Player 1 -> Index 0, Player 2 -> Index 1)
        int playerIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % availablePlayerPrefabs.Length;
        string assignedPrefabName = availablePlayerPrefabs[playerIndex].name;

        // Save selection to local player's network properties
        Hashtable customProps = new Hashtable
        {
            { "AssignedPrefab", assignedPrefabName }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
        Debug.Log($"[Lobby] Actor {PhotonNetwork.LocalPlayer.ActorNumber} assigned prefab: {assignedPrefabName}");
    }

    // Call from UI "Start Game" Button (Master Client only)
    public void StartGame(string gameSceneName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }
}