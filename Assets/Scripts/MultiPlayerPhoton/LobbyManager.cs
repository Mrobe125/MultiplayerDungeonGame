using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Available Prefabs (Must be inside Resources/ folder)")]
    [SerializeField] private GameObject[] availablePlayerPrefabs;

    private void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            AssignPlayerPrefab();
        }
    }

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

        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        // Deterministic Index: Actor 1 -> Index 0 | Actor 2 -> Index 1
        int playerIndex = (actorNum - 1) % availablePlayerPrefabs.Length;
        string assignedPrefabName = availablePlayerPrefabs[playerIndex].name;

        Hashtable customProps = new Hashtable
        {
            { "AssignedPrefab", assignedPrefabName }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);

        Debug.Log($"[LobbyManager] Local Actor {actorNum} assigned Prefab [{playerIndex}]: {assignedPrefabName}");
    }

    public void StartGame(string gameSceneName)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!AreAllPlayersAssigned())
            return;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    private bool AreAllPlayersAssigned()
    {
        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!player.CustomProperties.TryGetValue("AssignedPrefab", out object prefabObj) || string.IsNullOrEmpty((string)prefabObj))
            {
                return false;
            }
        }
        return true;
    }
}