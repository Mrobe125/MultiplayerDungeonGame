using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Resource Settings")]
    [SerializeField] private List<GameObject> resourcePrefabs = new List<GameObject>();
    [SerializeField] private int resourceCount;

    [Header("Spawn Area Settings")]
    [SerializeField] private float _distanceBewteenCheck;
    [SerializeField] private float heightOfCheck = 10f;
    [SerializeField] private float rangeofCheck = 30f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Manual Bounds Settings")]
    [SerializeField] private bool useManualBounds = false;
    [SerializeField] private Vector3 customBoundsSize = new Vector3(10f, 5f, 10f);

    [Header("Spawn Inset")]
    [SerializeField] private float wallPadding = 1.0f;

    [Header("Spawn Spacing")]
    [SerializeField] private float spawnRadius = 0.75f;
    [SerializeField] private LayerMask resourceLayer;

    private Bounds roomBounds;

    private void Start()
    {
        CalculateBounds();
        SpawnResource();
    }

    private void CalculateBounds()
    {
        if (useManualBounds)
        {
            roomBounds = new Bounds(transform.position, customBoundsSize);
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            roomBounds = new Bounds(transform.position, Vector3.one * 5f);
            return;
        }

        roomBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            roomBounds.Encapsulate(renderers[i].bounds);
        }
    }

    private void SpawnResource()
    {
        if (resourcePrefabs == null || resourcePrefabs.Count == 0)
            return;

        if (_distanceBewteenCheck <= 0.01f)
            return;

        float minX = roomBounds.min.x + wallPadding;
        float maxX = roomBounds.max.x - wallPadding;
        float minZ = roomBounds.min.z + wallPadding;
        float maxZ = roomBounds.max.z - wallPadding;

        for (float x = minX; x <= maxX; x += _distanceBewteenCheck)
        {
            for (float z = minZ; z <= maxZ; z += _distanceBewteenCheck)
            {
                Vector3 rayOrigin = new Vector3(
                    x,
                    roomBounds.max.y + heightOfCheck,
                    z
                );

                if (!Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out RaycastHit hit,
                    rangeofCheck,
                    groundLayer))
                {
                    continue;
                }

                if (Physics.CheckSphere(
                    hit.point,
                    spawnRadius,
                    resourceLayer))
                {
                    continue;
                }

                if (Random.Range(0, 100) < resourceCount)
                {
                    int randomIndex = Random.Range(0, resourcePrefabs.Count);
                    GameObject selectedPrefab = resourcePrefabs[randomIndex];

                    Quaternion rotation = GetWallFacingRotation(hit.point);

                    Instantiate(
                        selectedPrefab,
                        hit.point,
                        rotation,
                        transform
                    );
                }
            }
        }
    }

    private Quaternion GetWallFacingRotation(Vector3 spawnPosition)
    {
        float distanceToNorthWall = Mathf.Abs(roomBounds.max.z - spawnPosition.z);
        float distanceToSouthWall = Mathf.Abs(spawnPosition.z - roomBounds.min.z);
        float distanceToEastWall = Mathf.Abs(roomBounds.max.x - spawnPosition.x);
        float distanceToWestWall = Mathf.Abs(spawnPosition.x - roomBounds.min.x);

        float closestDistance = Mathf.Min(
            distanceToNorthWall,
            distanceToSouthWall,
            distanceToEastWall,
            distanceToWestWall
        );

        if (closestDistance == distanceToNorthWall)
        {
            return Quaternion.Euler(0f, 180f, 0f);
        }

        if (closestDistance == distanceToSouthWall)
        {
            return Quaternion.Euler(0f, 0f, 0f);
        }

        if (closestDistance == distanceToEastWall)
        {
            return Quaternion.Euler(0f, -90f, 0f);
        }

        return Quaternion.Euler(0f, 90f, 0f);
    }
}

