using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Resource Settings")]
    [SerializeField] private GameObject resourcePrefab;
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
        for(float x = negativePos.x; x < positivePos.x; x += _distanceBewteenCheck)
        {
            for (float z = negativePos.y; z < positivePos.y; z += _distanceBewteenCheck)
            {
                RaycastHit hit;
                if(Physics.Raycast(new Vector3(x, heightOfCheck,z),Vector3.down,out hit,rangeofCheck, groundLayer))
                {
                    if(resourceCount > Random.Range(0,101))
                    {
                        Instantiate(resourcePrefab, hit.point, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)), transform);
                    }
                }
            }
        }
    }
}