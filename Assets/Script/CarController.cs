using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public float followSpeed = 5f;

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z = 10f;

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);

        worldPoint.z = 0f;

        transform.position = Vector3.Lerp(
            transform.position,
            worldPoint,
            followSpeed * Time.deltaTime
        );
    }
}