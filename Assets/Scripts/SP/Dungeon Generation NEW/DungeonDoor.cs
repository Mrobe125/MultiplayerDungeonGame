using UnityEngine;

public class DungeonDoor : MonoBehaviour
{
    public GameObject closedWall;
    public bool IsConnected { get; private set; }

    public void Connect()
    {
        IsConnected = true;
        if (closedWall != null) closedWall.SetActive(false);
    }

    public void Disconnect()
    {
        IsConnected = false;
        if (closedWall != null) closedWall.SetActive(true);
    }
}