
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Special Rooms")]
    public DungeonRoom startRoomPrefab;
    public DungeonRoom endRoomPrefab;

    [Header("Normal Room Pool")]
    public List<DungeonRoom> normalRoomPrefabs = new List<DungeonRoom>();

    [Header("Generation Settings")]
    public int minRooms = 6;
    public int maxRooms = 20;
    public int attemptsPerRoom = 30;

    private List<DungeonRoom> placedRooms = new List<DungeonRoom>();
    public List<DungeonRoom> PlacedRooms => placedRooms;

    private void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        ClearDungeon();

        if (!ValidateSettings()) return;

        DungeonRoom start = Instantiate(startRoomPrefab, transform.position, transform.rotation, transform);
        start.ResetDoors();
        placedRooms.Add(start);

        while (placedRooms.Count < maxRooms)
        {
            if (placedRooms.Count >= minRooms && TryPlaceAtAvailableDoor(endRoomPrefab))
            {
                UpdateDoorWalls();
                return;
            }

            if (placedRooms.Count >= maxRooms - 1 || !TryPlaceRandomNormalRoom()) break;
        }

        if (placedRooms.Count < maxRooms && placedRooms.Count >= minRooms && TryPlaceAtAvailableDoor(endRoomPrefab))
        {
            UpdateDoorWalls();
            return;
        }

        UpdateDoorWalls();
    }

    private bool TryPlaceRandomNormalRoom()
    {
        if (normalRoomPrefabs.Count == 0) return false;

        for (int attempt = 0; attempt < attemptsPerRoom; attempt++)
        {
            DungeonRoom prefab = normalRoomPrefabs[Random.Range(0, normalRoomPrefabs.Count)];
            if (prefab != null && TryPlaceAtAvailableDoor(prefab)) return true;
        }

        return false;
    }

    private bool TryPlaceAtAvailableDoor(DungeonRoom prefab)
    {
        if (prefab == null) return false;

        List<DungeonDoor> availableDoors = new List<DungeonDoor>();

        foreach (DungeonRoom room in placedRooms)
        {
            foreach (DungeonDoor door in room.doors)
            {
                if (door != null && !door.IsConnected) availableDoors.Add(door);
            }
        }

        Shuffle(availableDoors);

        int attempts = 0;

        foreach (DungeonDoor targetDoor in availableDoors)
        {
            if (attempts >= attemptsPerRoom) break;
            attempts++;

            List<int> doorIndices = new List<int>();

            for (int i = 0; i < prefab.doors.Length; i++)
            {
                if (prefab.doors[i] != null) doorIndices.Add(i);
            }

            Shuffle(doorIndices);

            foreach (int doorIndex in doorIndices)
            {
                DungeonRoom candidate = Instantiate(prefab, transform.position, transform.rotation, transform);
                candidate.ResetDoors();

                DungeonDoor candidateDoor = candidate.doors[doorIndex];
                SnapRoom(candidate.transform, candidateDoor.transform, targetDoor.transform);
                Physics.SyncTransforms();

                if (Fits(candidate))
                {
                    targetDoor.Connect();
                    candidateDoor.Connect();
                    placedRooms.Add(candidate);
                    return true;
                }

                RemoveCandidate(candidate);
            }
        }

        return false;
    }

    private void SnapRoom(Transform room, Transform newDoor, Transform targetDoor)
    {
        Quaternion desiredRotation = Quaternion.LookRotation(-targetDoor.forward, targetDoor.up);
        Quaternion rotationDelta = desiredRotation * Quaternion.Inverse(newDoor.rotation);
        room.rotation = rotationDelta * room.rotation;
        room.position += targetDoor.position - newDoor.position;
    }

    private bool Fits(DungeonRoom candidate)
    {
        if (candidate.footprint == null)
        {
            return false;
        }

        Transform footprintTransform = candidate.footprint.transform;
        Vector3 center = footprintTransform.TransformPoint(candidate.footprint.center);
        Vector3 scale = footprintTransform.lossyScale;
        Vector3 halfExtents = Vector3.Scale(candidate.footprint.size * 0.5f, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));

        Collider[] hits = Physics.OverlapBox(center, halfExtents, footprintTransform.rotation, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            if (hit == null || hit.transform.IsChildOf(candidate.transform)) continue;

            bool isDoorWall = false;

            foreach (DungeonRoom existingRoom in placedRooms)
            {
                foreach (DungeonDoor door in existingRoom.doors)
                {
                    if (door == null || door.closedWall == null) continue;

                    Transform wall = door.closedWall.transform;
                    if (hit.transform == wall || hit.transform.IsChildOf(wall))
                    {
                        isDoorWall = true;
                        break;
                    }
                }

                if (isDoorWall) break;
            }

            if (isDoorWall) continue;

            foreach (DungeonRoom existingRoom in placedRooms)
            {
                if (hit.transform == existingRoom.transform || hit.transform.IsChildOf(existingRoom.transform)) return false;
            }
        }

        return true;
    }

    private void UpdateDoorWalls()
    {
        foreach (DungeonRoom room in placedRooms)
        {
            foreach (DungeonDoor door in room.doors)
            {
                if (door != null && door.closedWall != null)
                    door.closedWall.SetActive(!door.IsConnected);
            }
        }
    }

    private bool ValidateSettings()
    {
        if (startRoomPrefab == null || endRoomPrefab == null)
        {
            return false;
        }

        if (normalRoomPrefabs.Count == 0)
        {
            return false;
        }

        if (maxRooms < minRooms)
        {
            return false;
        }

        return true;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void RemoveCandidate(DungeonRoom candidate)
    {
        foreach (Collider col in candidate.GetComponentsInChildren<Collider>()) col.enabled = false;

        if (Application.isPlaying) Destroy(candidate.gameObject);
        else DestroyImmediate(candidate.gameObject);
    }


    public void ClearDungeon()
    {
        placedRooms.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }
}