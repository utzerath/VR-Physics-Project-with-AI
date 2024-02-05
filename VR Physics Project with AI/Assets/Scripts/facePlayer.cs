using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    public Transform playerTransform;

    void Update()
    {
        if (playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

            // Calculate the rotation towards the player
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

            // Adjust the rotation by adding 180 degrees around the up axis
            lookRotation *= Quaternion.Euler(0, 180f, 0);

            // Apply the rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }
}
