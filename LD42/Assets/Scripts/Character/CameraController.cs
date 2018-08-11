using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Rect Bounds;
    public bool ShowGizmos = true;
    public int VisibleParallaxes = 1;

    float leftBound;
    float rightBound;
    float botBound;
    float topBound;

    List<Transform> leftParallax;

    GameObject cam;

    private void Awake()
    {
        Camera.main.aspect = 16f / 9f;
    }

    private void Start()
    {
        cam = Camera.main.gameObject;

        float vertExtent = Camera.main.orthographicSize;
        float horzExtent = vertExtent * Camera.main.aspect;

        leftBound = horzExtent + Bounds.xMin;
        rightBound = Bounds.xMax - horzExtent;
        botBound = vertExtent + Bounds.yMin;
        topBound = Bounds.yMax - vertExtent;
    }

    private void Update()
    {
        cam.transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, leftBound, rightBound),
            Mathf.Clamp(transform.position.y, botBound, topBound),
            cam.transform.position.z
        );
    }

    void OnDrawGizmos()
    {
        if (!ShowGizmos)
            return;
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }
}
