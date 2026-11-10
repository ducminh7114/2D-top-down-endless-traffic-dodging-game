using UnityEngine;
using System.Collections;

public class KeepObjectOnScreen : MonoBehaviour
{
    public Camera MainCamera;
    private float objectHalfWidth;
    private float objectHalfHeight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (MainCamera != null)
        {
            return;
        }

        Vector3 position = transform.position;

        Vector3 min = MainCamera.ViewportToWorldPoint(new Vector3(0, 0, MainCamera.nearClipPlane));

        Vector3 max = MainCamera.ViewportToWorldPoint(new Vector3(1,1, MainCamera.nearClipPlane));

        position.x = Mathf.Clamp(position.x, min.x + objectHalfWidth, max.x - objectHalfWidth);

        position.y = Mathf.Clamp(position.y,min.y + objectHalfHeight,max.y - objectHalfHeight);

        transform.position = position;
    }
}
