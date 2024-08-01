using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_zoom : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minSize = 3f;
    [SerializeField] private float maxSize = 20f;
    public GameObject blocker;

    void Update()
    {
        if(blocker != null && blocker.activeSelf) return;
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        float size = Camera.main.orthographicSize;
        size -= scrollData * zoomSpeed;
        size = Mathf.Clamp(size, minSize, maxSize);
        Camera.main.orthographicSize = size;
    }
}
