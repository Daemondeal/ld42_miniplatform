using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedCollider : MonoBehaviour {

    public bool Grounded = false;

    PlatformCharController pc;

	void Start () {
        pc = GetComponentInParent<PlatformCharController>();
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Grounded = true;
        if (collision.gameObject.tag == "Ground")
        {
            pc.HitGround();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Grounded = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Grounded = false;
    }
}
