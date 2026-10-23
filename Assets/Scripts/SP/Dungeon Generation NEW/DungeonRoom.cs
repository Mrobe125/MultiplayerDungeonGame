using UnityEngine;

public enum DungeonRoomType
{
    Start,
    Normal,
    End
}

public class DungeonRoom : MonoBehaviour
{
    [Header("Room Settings")]
    public DungeonRoomType roomType = DungeonRoomType.Normal;

    [Tooltip("Room dimensions measured in grid cells.")]
    public Vector2Int gridSize = Vector2Int.one;

    [Header("Door Connections")]
    public DungeonDoor[] doors;

    [Header("Overlap Detection")]
    [Tooltip("BoxCollider covering the room's occupied floor area.")]
    public BoxCollider footprint;

    public void ResetDoors()
    {
        foreach (DungeonDoor door in doors)
        {
            if (door != null)
                door.Disconnect();
        }
    }

    public DungeonDoor GetRandomAvailableDoor()
    {
        int availableCount = 0;

        foreach (DungeonDoor door in doors)
        {
            if (door != null && !door.IsConnected)
                availableCount++;
        }

        if (availableCount == 0)
            return null;

        int chosen = Random.Range(0, availableCount);

        foreach (DungeonDoor door in doors)
        {
            if (door == null || door.IsConnected)
                continue;

            if (chosen == 0)
                return door;

            chosen--;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (footprint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.matrix = footprint.transform.localToWorldMatrix;

        Gizmos.DrawWireCube(
            footprint.center,
            footprint.size
        );
    }
}