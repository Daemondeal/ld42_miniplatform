using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ArrowController : MonoBehaviour {

    public float Speed = 200f;
    public float Lifespan = 10f;

    float _remainingLife;

    Rigidbody2D rb;
    SpriteRenderer sr;

	// Use this for initialization
	void Start () {
        _remainingLife = Lifespan;
        Collider2D player = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), player);

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        rb.velocity = new Vector2(Speed * (sr.flipX ? -1 : 1), 0);

        _remainingLife -= Time.deltaTime;
        if (_remainingLife <= 0f)
            Destroy(gameObject); // Your time has ended
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (new string[]{ "Ground", "Enemy", "EnemyDamage"}.Contains(collision.gameObject.tag))
            Destroy(gameObject);

        if (collision.gameObject.tag == "EnemyDamage")
            Debug.Log("Arrow Destroyed");
    }
}
