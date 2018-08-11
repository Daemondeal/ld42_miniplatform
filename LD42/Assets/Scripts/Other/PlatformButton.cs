using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformButton : MonoBehaviour {

    public GameObject platform;

    Animation platformAn;
    Collider2D platformCollider;

	// Use this for initialization
	void Start () {
        platformAn = platform.GetComponent<Animation>();
        platformCollider = platform.GetComponent<Collider2D>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Arrow")
        {
            platformAn.Play();
            platformCollider.enabled = true;
            Destroy(gameObject);
        }
    }
}
