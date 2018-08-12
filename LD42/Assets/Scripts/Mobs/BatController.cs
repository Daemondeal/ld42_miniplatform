using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatController : MonoBehaviour
{

    public Vector2 FirstStop;
    public Vector2 SecondStop;
    public bool ShowGizmos = false;

    [Tooltip("Distance from new checkpoint")]
    public float PositionError = 1f;
    public float Speed = 50f;

    public float DeathDuration = 1f;

    Rigidbody2D rb;
    Collider2D hitbox;
    SpriteRenderer sr;
    Animator an;

    int _patrolIndex = 1;
    float _deathCount = 0f;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        an = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_deathCount > 0f)
        {
            _deathCount -= Time.deltaTime;
            if (_deathCount <= 0)
                Destroy(gameObject);
            return;
        }

        Vector2 nextPosition = _patrolIndex == 0 ? FirstStop : SecondStop;

        float hMov = Mathf.Clamp(nextPosition.x - transform.position.x, -1, 1);

        if (Vector2.Distance(transform.position, nextPosition) < PositionError)
        {
            _patrolIndex = _patrolIndex == 1 ? 0 : 1;
        }

        rb.velocity = new Vector2(hMov * Time.deltaTime * 60 * Speed, rb.velocity.y);

        SetDirection(hMov.ToDirection());
    }

    public Direction GetDirection()
    {
        return sr.flipX ? Direction.Left : Direction.Right;
    }

    public void SetDirection(Direction dir)
    {
        if (dir == GetDirection())
            return;

        sr.flipX = dir == Direction.Left;

        //hitbox.offset = new Vector2(dir.ToFloat() * Mathf.Abs(hitbox.offset.x), hitbox.offset.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Damage" || collision.gameObject.tag == "Arrow")
            Die(); // Horribly
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Damage" || collision.gameObject.tag == "Arrow")
            Die(); // Horribly
    }

    void Die()
    {
        an.SetBool("Dead", true);
        hitbox.enabled = false;
        rb.gravityScale = 100;
        rb.velocity = Vector2.zero;
        _deathCount = DeathDuration;
    }

    private void OnDrawGizmos()
    {
        if (ShowGizmos)
        {
            Gizmos.DrawWireCube(FirstStop, Vector3.right * 5f + Vector3.up * 15f);
            Gizmos.DrawWireCube(SecondStop, Vector3.right * 5f + Vector3.up * 15f);
        }
    }
}
