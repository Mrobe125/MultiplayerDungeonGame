using UnityEngine;

public class SpawnScriptManager : MonoBehaviour
{
    public GameObject NetworkManager;

    private void Awake()
    {
        if (FindAnyObjectByType<NetworkManager>() == null)
            Instantiate(NetworkManager);
    }
}
