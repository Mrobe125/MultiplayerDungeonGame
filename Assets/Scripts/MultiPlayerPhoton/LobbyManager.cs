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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[LobbyManager] Player {newPlayer.ActorNumber} ({newPlayer.NickName}) joined the lobby.");
    }

    private void AssignPlayerPrefab()
    {
        if (availablePlayerPrefabs == null || availablePlayerPrefabs.Length == 0)
        {
            Debug.LogError("[LobbyManager] No player prefabs assigned in the Inspector!");
            return;
        }

        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        int playerIndex = (actorNum - 1) % availablePlayerPrefabs.Length;
        string assignedPrefabName = availablePlayerPrefabs[playerIndex].name;

        Hashtable customProps = new Hashtable
        {
            { "AssignedPrefab", assignedPrefabName }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);

        Debug.Log($"[LobbyManager] Local Actor {actorNum} (Player #{actorNum}) assigned Prefab [{playerIndex}]: {assignedPrefabName}");
    }

    public void StartGame(string gameSceneName)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[LobbyManager] Only the Master Client (Host) can start the game.");
            return;
        }

        if (!AreAllPlayersAssigned())
        {
            Debug.LogWarning("[LobbyManager] Cannot start game! Not all players have assigned prefabs yet.");
            return;
        }

        Debug.Log("[LobbyManager] All players ready with assigned prefabs. Starting game...");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    private bool AreAllPlayersAssigned()
    {
        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!player.CustomProperties.TryGetValue("AssignedPrefab", out object prefabObj) || string.IsNullOrEmpty((string)prefabObj))
            {
                Debug.LogWarning($"[LobbyManager] Player {player.ActorNumber} ({player.NickName}) does NOT have a prefab assigned!");
                return false;
            }
        }

        return true;
    }
}