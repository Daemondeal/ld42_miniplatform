using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Rect Bounds;
    public bool ShowGizmos = true;
    public int VisibleParallaxes = 1;
    public Transform Background;

    float leftBound;
    float rightBound;
    float botBound;
    float topBound;

    SpriteRenderer backgroundSprite;

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

        backgroundSprite = Background.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        cam.transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, leftBound, rightBound),
            Mathf.Clamp(transform.position.y, botBound, topBound),
            cam.transform.position.z
        );

        /*Vector2 min = new Vector2(leftBound, botBound);
        Vector2 max = new Vector2(rightBound, topBound);

        //Vector2 ratio = Divide(((Vector2)cam.transform.position - min), max - min);

        float xSize = backgroundSprite.sprite.rect.size.x;
        float camSize = Camera.main.orthographicSize * Camera.main.aspect * 2;


        float ratio = (transform.position.x - leftBound) / (leftBound - rightBound);

        
        Background.localPosition = Vector3.right * (ratio - 0.5f) * (xSize - camSize) + Background.localPosition.z * Vector3.forward;*/
    }

    Vector2 Divide(Vector2 l, Vector2 r)
    {
        return new Vector2(l.x / r.x, l.y / r.y);
    }

    void OnDrawGizmos()
    {
        if (!ShowGizmos)
            return;
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }
}
