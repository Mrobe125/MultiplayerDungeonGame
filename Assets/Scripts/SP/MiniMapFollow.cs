using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player;
    public float followSpeed;

    private void LateUpdate()
    {
        Vector3 pos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.position = Vector3.Lerp(transform.position, pos, followSpeed * Time.deltaTime);
    }
}
