using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkeletonBossController : MonoBehaviour {

    public Vector2 ArenaLeft;
    public Vector2 ArenaRight;
    public bool ShowGizmos = false;

    [Tooltip("Distance from new checkpoint")]
    public float PositionError = 1f;
    public float Speed = 50f;

    public float DeathDuration = 1f;

    [Tooltip("How near should the player be for the skelton to notice")]
    public float PlayerDistance = 300f;

    [Tooltip("How near the player should be for the skeleton to attack")]
    public float SkeletonAttackRange = 30f;

    public float ArrowDistance = 200f;

    public float SwordOnTime = .4f;

    public float SwordOffTime = .2f;

    public float AttackDuration = 1f;

    public float AttackCooldown = 1f;

    public StateSkeletonBoss CurrentState = StateSkeletonBoss.Walking;

    Rigidbody2D rb;
    Collider2D hitbox;
    SpriteRenderer sr;
    Animator an;

    Collider2D swordHitbox;
    Transform playerTransform;

    Vector2 _startingPosition;

    int _patrolIndex = 1;
    float _deathCount = 0f;
    float _attackCount = 0f;
    float _attackCooldownDuration = 0f;

    string[] _allStates;

    StateSkeletonBoss _lastState;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        an = GetComponent<Animator>();

        swordHitbox = GetComponentsInChildren<Collider2D>().Where(x => x.gameObject.name == "SwordHitbox").FirstOrDefault();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        playerTransform = player.transform;

        _allStates = ((StateSkeletonBoss[])System.Enum.GetValues(typeof(StateSkeletonBoss))).Select(x => x.ToString()).ToArray();
        _startingPosition = transform.position;
    }

    void UpdateArrowCounter()
    {
        GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");

        if (arrows.Length > 0)
        {
            foreach(GameObject arrow in arrows)
            {
                if (Vector2.Distance(arrow.transform.position, transform.position) < ArrowDistance && CurrentState != StateSkeletonBoss.Attacking)
                {
                    Attack();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        //CurrentState = StateSkeletonBoss.Attacking;
        if (CurrentState == StateSkeletonBoss.Walking || CurrentState == StateSkeletonBoss.Idle)
            UpdateArrowCounter();

        if (UpdateTimer(ref _deathCount))
        {
            Destroy(gameObject);
        }
        if (UpdateTimer(ref _attackCount))
        {
            CurrentState = StateSkeletonBoss.Walking;
            _attackCooldownDuration = AttackCooldown;
        }

        UpdateTimer(ref _attackCooldownDuration);

        float hMov = 0;

        if (CurrentState == StateSkeletonBoss.Attacking && _attackCount <= SwordOnTime)
        {
            if (_attackCount <= SwordOffTime)
                swordHitbox.enabled = false;
            else
                swordHitbox.enabled = true;

        }

        if (CurrentState != StateSkeletonBoss.Attacking && _deathCount <= 0f)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance < PlayerDistance)
            {
                if (distance < SkeletonAttackRange && _attackCooldownDuration <= 0f) // Attacks
                {
                    Attack();
                }
                else 
                {
                    if (PlayerWithinArena()) // Walks Towards Player
                    {
                        CurrentState = StateSkeletonBoss.Walking;
                        hMov = Mathf.Clamp(playerTransform.position.x - transform.position.x, -1, 1);
                        SetDirection(hMov.ToDirection());
                    }
                    else // Goes back to starting position
                    {
                        if (Vector2.Distance(transform.position, _startingPosition) < PositionError)
                        {
                            CurrentState = StateSkeletonBoss.Idle;
                            SetDirection(Mathf.Clamp(playerTransform.position.x - transform.position.x, -1, 1).ToDirection());
                        }
                        else
                        {
                            hMov = Mathf.Clamp(_startingPosition.x - transform.position.x, -1, 1);
                            SetDirection(hMov.ToDirection());
                            CurrentState = StateSkeletonBoss.Walking;
                        }
                    }
                }
            }
            else
            {
                CurrentState = StateSkeletonBoss.Idle;
                /*
                // Walk Routine
                Vector2 nextPosition = _patrolIndex == 0 ? ArenaLeft : ArenaRight;

                hMov = Mathf.Clamp(nextPosition.x - transform.position.x, -1, 1);

                if (Vector2.Distance(transform.position, nextPosition) < PositionError)
                {
                    _patrolIndex = _patrolIndex == 1 ? 0 : 1;
                }

                SetDirection(hMov.ToDirection());*/
            }
        }
        rb.velocity = new Vector2(hMov * Time.deltaTime * 60 * Speed, rb.velocity.y);

        UpdateAnimation();
    }

    void Attack() {
        _attackCount = AttackDuration;
        CurrentState = StateSkeletonBoss.Attacking;
    }

    bool PlayerWithinArena() {
        return playerTransform.position.x > ArenaLeft.x && playerTransform.position.x < ArenaRight.x;
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
        swordHitbox.offset = new Vector2(dir.ToFloat() * Mathf.Abs(swordHitbox.offset.x), swordHitbox.offset.y);
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
        return;

        an.SetBool("Dead", true);
        hitbox.enabled = false;
        swordHitbox.enabled = false;
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
            Gizmos.color = Color.green;

            Gizmos.DrawWireCube(ArenaLeft, Vector3.right * 5f + Vector3.up * 15f);
            Gizmos.DrawWireCube(ArenaRight, Vector3.right * 5f + Vector3.up * 15f);

            Gizmos.DrawWireSphere(transform.position, ArrowDistance);
        }
    }
}
