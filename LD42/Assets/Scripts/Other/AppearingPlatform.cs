using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearingPlatform : ActivableItem {

    public bool ShowOutline = false;

    Animation an;
    Collider2D col;
    SpriteRenderer sr;

	void Start () {
        an = GetComponent<Animation>();
        col = GetComponent<Collider2D>();
	}

    public override void Activate()
    {
        an.Play();
        col.enabled = true;
    }

    private void OnDrawGizmos()
    {
        if (ShowOutline)
        {
            if (sr == null)
                sr = GetComponent<SpriteRenderer>();
            
            Gizmos.DrawWireCube(transform.position, sr.size * transform.localScale);
        }
    }
}
