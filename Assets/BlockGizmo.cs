using UnityEngine;

public class BlockGizmo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f); // Her karenin merkezine kırmızı nokta koyar
    }
}