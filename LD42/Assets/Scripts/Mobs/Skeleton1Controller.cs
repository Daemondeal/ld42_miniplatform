using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Skeleton1Controller : MonoBehaviour {

    public Vector2 FirstStop;
    public Vector2 SecondStop;
    public bool ShowGizmos = false;

    [Tooltip("Distance from new checkpoint")]
    public float PositionError = 1f;
    public float Speed = 50f;

    public float DeathDuration = 1f;

    [Tooltip("How near should the player be for the skelton to notice")]
    public float PlayerDistance = 50f;

    [Tooltip("How near the player should be for the skeleton to attack")]
    public float SkeletonAttackRange = 30f;

    public float AxeOnTime = .4f;

    public float AxeOffTime = .2f;

    public float AttackDuration = 1f;

    public float AttackCooldown = 1f;

    public SkeletonState CurrentState = SkeletonState.Walking;

    Rigidbody2D rb;
    Collider2D hitbox;
    SpriteRenderer sr;
    Animator an;

    Collider2D axeHitbox;
    Transform playerTransform;

    int _patrolIndex = 1;
    float _deathCount = 0f;
    float _attackCount = 0f;
    float _attackCooldownDuration = 0f;

    string[] _allStates;

    SkeletonState _lastState;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        an = GetComponent<Animator>();

        axeHitbox = GetComponentsInChildren<Collider2D>().Where(x => x.gameObject.name == "AxeDamage").FirstOrDefault();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        playerTransform = player.transform;

        _allStates = ((SkeletonState[])System.Enum.GetValues(typeof(SkeletonState))).Select(x => x.ToString()).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (UpdateTimer(ref _deathCount))
        {
            Destroy(gameObject);
        }
        if (UpdateTimer(ref _attackCount))
        {
            CurrentState = SkeletonState.Walking;
            _attackCooldownDuration = AttackCooldown;
        }

        UpdateTimer(ref _attackCooldownDuration);

        float hMov = 0;

        if (CurrentState == SkeletonState.Attacking && _attackCount <= AxeOnTime)
        {
            if (_attackCount <= AxeOffTime)
                axeHitbox.enabled = false;
            else
                axeHitbox.enabled = true;
            
        }

        if (CurrentState != SkeletonState.Attacking && _deathCount <= 0f)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance < PlayerDistance)
            {
                if (distance < SkeletonAttackRange && _attackCooldownDuration <= 0f) // Attacks
                {
                    _attackCount = AttackDuration;
                    CurrentState = SkeletonState.Attacking;
                }
                else // Waits For Player
                {
                    CurrentState = SkeletonState.WaitingForPlayer;
                    SetDirection(Mathf.Clamp(playerTransform.position.x - transform.position.x, -1, 1).ToDirection());
                }
            }
            else
            {
                CurrentState = SkeletonState.Walking;
                // Walk Routine
                Vector2 nextPosition = _patrolIndex == 0 ? FirstStop : SecondStop;

                hMov = Mathf.Clamp(nextPosition.x - transform.position.x, -1, 1);

                if (Vector2.Distance(transform.position, nextPosition) < PositionError)
                {
                    _patrolIndex = _patrolIndex == 1 ? 0 : 1;
                }

                SetDirection(hMov.ToDirection());
            }
        }
        rb.velocity = new Vector2(hMov * Time.deltaTime * 60 * Speed, rb.velocity.y);

        UpdateAnimation();
    }

    bool UpdateTimer(ref float timer)
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0f;
                return true;
            }
        }
        return false;
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

        hitbox.offset = new Vector2(dir.ToFloat() * -Mathf.Abs(hitbox.offset.x), hitbox.offset.y);
        axeHitbox.offset = new Vector2(dir.ToFloat() * Mathf.Abs(axeHitbox.offset.x), axeHitbox.offset.y);
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
        axeHitbox.enabled = false;
        rb.gravityScale = 0;
        _attackCount = 0;
        rb.velocity = Vector2.zero;
        _deathCount = DeathDuration;
    }

    void ResetAllAnimation()
    {
        foreach (string anim in _allStates)
        {
            an.SetBool(anim, false);
        }
    }

    void UpdateAnimation()
    {
        if (CurrentState != _lastState)
        {
            ResetAllAnimation();
            an.SetBool(CurrentState.ToString(), true);
            _lastState = CurrentState;
        }
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
