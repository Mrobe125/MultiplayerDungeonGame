using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab Inspector Slots (Must be in Resources/ folder)")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    private void Awake()
    {
        // Crucial: Must be set true on ALL clients BEFORE joining rooms/loading levels
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer.ActorNumber > 0)
        {
            AssignPlayerPrefab();
        }
    }

    public override void OnJoinedRoom()
    {
        // Double check valid Actor ID before setting custom properties
        if (PhotonNetwork.LocalPlayer.ActorNumber > 0)
        {
            AssignPlayerPrefab();
        }
    }

    private void AssignPlayerPrefab()
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        if (actorNum <= 0) return; // Prevent setting properties with invalid actor -1

        // Master Client / First Player = player1Prefab; Client 2 = player2Prefab
        bool isPlayer1 = PhotonNetwork.IsMasterClient || actorNum == 1;
        GameObject selectedPrefab = isPlayer1 ? player1Prefab : player2Prefab;

        if (selectedPrefab == null)
        {
            Debug.LogError($"[LobbyManager] Missing prefab assignment for {(isPlayer1 ? "Player 1" : "Player 2")}!");
            return;
        }

        Hashtable customProps = new Hashtable
        {
            { "AssignedPrefab", selectedPrefab.name }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);

        Debug.Log($"[LobbyManager] Local Actor {actorNum} (IsMaster: {PhotonNetwork.IsMasterClient}) assigned Prefab: {selectedPrefab.name}");
    }

    public void StartGame(string gameSceneName)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!AreAllPlayersAssigned())
        {
            Debug.LogWarning("[LobbyManager] Cannot start: Waiting for all players to assign properties.");
            return;
        }

        // Master loads level; AutomaticallySyncScene will force all connected clients to switch scene automatically
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    private bool AreAllPlayersAssigned()
    {
        if (PhotonNetwork.CurrentRoom == null) return false;

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