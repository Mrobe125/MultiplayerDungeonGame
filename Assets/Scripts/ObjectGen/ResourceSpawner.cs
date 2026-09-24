using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Resource Settings")]
    [SerializeField] private List<GameObject> resourcePrefabs = new List<GameObject>(); 
    [SerializeField] private int resourceCount;

    [Header("Spawn Area Settings")]
    [SerializeField] private float _distanceBewteenCheck;
    [SerializeField]private float heightOfCheck = 10f, rangeofCheck =30f;
    [SerializeField] private LayerMask groundLayer;

    //[Header("Gizmo Visualization")]
    //[SerializeField] private bool showGizmos = true;
    //[SerializeField] private Color areaGizmoColor = new Color(0f, 1f, 0f, 0.3f);
    //[SerializeField] private Color rayGizmoColor = Color.cyan;

    private Bounds roomBounds;

    private void Start()
    {
        CalculateBounds();
        SpawnResource();

    }

    private void CalculateBounds()
    {
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

    void SpawnResource()
    {
        if (resourcePrefabs == null || resourcePrefabs.Count == 0)
        {
            Debug.LogWarning("No resource prefabs assigned to ResourceSpawner.");
            return;
        }

        for (float x = roomBounds.min.x; x <= roomBounds.max.x; x += _distanceBewteenCheck)
        {
            for (float z = roomBounds.min.z; z <= roomBounds.max.z; z += _distanceBewteenCheck)
            {
                Vector3 rayOrigin = new Vector3(x, roomBounds.max.y + heightOfCheck, z);

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rangeofCheck, groundLayer))
                {
                    if (Random.Range(0, 100) < resourceCount)
                    {
                        int randomIndex = Random.Range(0, resourcePrefabs.Count);
                        GameObject selectedPrefab = resourcePrefabs[randomIndex];

                        Instantiate(selectedPrefab, hit.point, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);
                    }
                }
            }
        }
    }

    //private void OnDrawGizmosSelected()
    //{
    //    if (!showGizmos) return;

    //    CalculateBounds();

    //    Gizmos.color = areaGizmoColor;
    //    Gizmos.DrawWireCube(roomBounds.center, roomBounds.size);

    //    Gizmos.color = rayGizmoColor;
    //    if (_distanceBewteenCheck <= 0.1f) return; 

    //    for (float x = roomBounds.min.x; x <= roomBounds.max.x; x += _distanceBewteenCheck)
    //    {
    //        for (float z = roomBounds.min.z; z <= roomBounds.max.z; z += _distanceBewteenCheck)
    //        {
    //            Vector3 rayOrigin = new Vector3(x, roomBounds.max.y + heightOfCheck, z);
    //            Gizmos.DrawRay(rayOrigin, Vector3.down * rangeofCheck);
    //        }
    //    }
    //}
}