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
    public Vector2 positivePos, negativePos;

    private void Start()
    {
        
        SpawnResource();

    }

    void SpawnResource()
    {
        if (resourcePrefabs == null || resourcePrefabs.Count == 0)
        {
            Debug.LogWarning("No resource prefabs assigned to ResourceSpawner.");
            return;
        }

        for (float x = negativePos.x; x < positivePos.x; x += _distanceBewteenCheck)
        {
            for (float z = negativePos.y; z < positivePos.y; z += _distanceBewteenCheck)
            {
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(x, heightOfCheck, z), Vector3.down, out hit, rangeofCheck, groundLayer))
                {
                    if (resourceCount > Random.Range(0, 101))
                    {
                        int randomIndex = Random.Range(0, resourcePrefabs.Count);
                        GameObject selectedPrefab = resourcePrefabs[randomIndex];

                        Instantiate(selectedPrefab, hit.point, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);
                    }
                }
            }
        }
    }
}