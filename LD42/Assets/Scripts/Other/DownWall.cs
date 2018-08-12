using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownWall : ActivableItem {

    public Vector2[] Positions;
    public float Speed = 10f;
    public bool ShowGizmos = true;

    SpriteRenderer sr;

    int _index = 0;

	// Use this for initialization
	void Start () {
        sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.MoveTowards(transform.position, Positions[_index], Speed);
	}

    public override void Activate() {
        _index++;
    }

    private void OnDrawGizmos()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (ShowGizmos && Positions.Length > 0)
        {
            foreach (Vector2 position in Positions)
            {
                Gizmos.DrawWireCube(position + Vector2.up * sr.bounds.size.y/2, new Vector3(10f, 0f));
            }
        }
    }
}
