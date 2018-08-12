using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingWall : ActivableItem {

    Animation an;
    Collider2D col;
    SpriteRenderer sr;

    void Start()
    {
        an = GetComponent<Animation>();
        col = GetComponent<Collider2D>();
    }

    public override void Activate()
    {
        an.Play();
        col.enabled = false;
    }
}
