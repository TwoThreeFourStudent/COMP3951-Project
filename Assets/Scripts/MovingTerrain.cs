using UnityEngine;

public class MovingTerrain : MonoBehaviour
{
    [SerializeField] private float moveDownDistance = 1.0f;

    public void MoveDown()
    {
        Vector3 currentPosition = transform.position;
        Vector3 newPosition = currentPosition + new Vector3(0, -moveDownDistance, 0);
        transform.position = newPosition;
    }
}