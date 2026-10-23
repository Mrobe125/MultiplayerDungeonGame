using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class NetworkDungeonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private DungeonGenerator dungeonGenerator;

    private const string SEED_KEY = "DungeonSeed";

    private void Start()
    {
        if (dungeonGenerator == null)
            dungeonGenerator = GetComponent<DungeonGenerator>();

        if (PhotonNetwork.IsMasterClient)
        {
            // MasterClient generates a random seed and shares it via Room Custom Properties
            int seed = Random.Range(int.MinValue, int.MaxValue);

            Hashtable props = new Hashtable
            {
                { SEED_KEY, seed }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            // Generate locally on MasterClient
            dungeonGenerator.GenerateDungeon(seed);
        }
        else
        {
            // Client checks if the seed was already set before loading into the room
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(SEED_KEY, out object seedObj))
            {
                dungeonGenerator.GenerateDungeon((int)seedObj);
            }
        }
    }

    // Handles late-joining clients or property synchronization events
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!PhotonNetwork.IsMasterClient && propertiesThatChanged.ContainsKey(SEED_KEY))
        {
            int seed = (int)propertiesThatChanged[SEED_KEY];
            dungeonGenerator.GenerateDungeon(seed);
        }
    }
}