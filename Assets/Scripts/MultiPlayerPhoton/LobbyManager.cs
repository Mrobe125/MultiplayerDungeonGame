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

        // Calculate unique index based on ActorNumber (Player 1 -> Index 0, Player 2 -> Index 1)
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        int playerIndex = Mathf.Abs(actorNum - 1) % availablePlayerPrefabs.Length;
        string assignedPrefabName = availablePlayerPrefabs[playerIndex].name;

        Hashtable customProps = new Hashtable
        {
            { "AssignedPrefab", assignedPrefabName }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
        Debug.Log($"[Lobby] Local Actor {actorNum} assigned prefab: {assignedPrefabName}");
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