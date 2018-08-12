using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

#pragma warning disable 414, 219

public class PlatformCharController : MonoBehaviour {


	public float Speed = 20f;
	public float JumpSpeed = 48f;
    public float DoubleJumpSpeed = 300f;

	[Tooltip("How much slow you need to be considered not moving")]
	public float MovError = 0.1f;

	[Tooltip("Attack duration, in seconds")]
	public float AttackDuration = 1f;

	[Tooltip("Bow attack duration, in seconds")]
	public float BowDuration = 1f;

    public float BowJumpDuration = 1.1f;

    public GameObject ArrowPrefab;
    public Vector2 ArrowOffset = Vector2.one;

    [Tooltip("When the damage hitbox appears, in seconds from the end of the attack")]
    public float DamageTime = .3f;

    [Tooltip("When the arrow starts, in seconds from the end of the bow animation")]
    public float ArrowTime = .1f;

    [Tooltip("When the arrow starts, in seconds from the end of the bow jump animation")]
    public float ArrowJumpTime = .26f;

    public float SneakHitboxOffset = -10f;
    public float SneakHitboxHeight = 10f;

    public float SlideTime = 2f;

    public State CurrentState = State.Running;

    public UnityEngine.UI.Image GameOverScreen;

	Rigidbody2D rb;
	Animator an;
	SpriteRenderer sr;
	BoxCollider2D hitbox;
	AudioSource audios;

	Collider2D damageHitbox;
	InventoryController inventory;
	GroundedCollider groundCheck;
	
	string[] _allStates;
	State _lastState = State.Jumping; // For animation purposes
	bool _inMenu; // Disables input while in menu

	float _attackDuration = 0f;
	float _bowDuration = 0f;
    float _bowJumpDuration = 0f;
    float _slideDuration = 0f;
    float _hitboxYOffset = 0f;
    float _hitboxYSize = 0f;

    bool _arrowShot = false;
    bool _doubleJumpPossible = false;
    
    bool _dead = false;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
		an = GetComponent<Animator>();
		sr = GetComponent<SpriteRenderer>();
		hitbox = GetComponent<BoxCollider2D>();
		audios = GetComponent<AudioSource>();
		inventory = GetComponent<InventoryController>();

		groundCheck = GetComponentInChildren<GroundedCollider>();
		damageHitbox = GameObject.FindGameObjectWithTag("Damage").GetComponent<Collider2D>();

		// Gets all the possible states in a string array, for animation purposes
		_allStates = ((State[])System.Enum.GetValues(typeof(State))).Select(x => x.ToString()).ToArray();

        _hitboxYOffset = hitbox.offset.y;
        _hitboxYSize = hitbox.bounds.size.y;
	}

	public Direction GetDirection() {
		return sr.flipX ? Direction.Left : Direction.Right;
	}

	public void SetDirection(Direction dir)
	{
		if (dir == GetDirection())
			return;

		sr.flipX = dir == Direction.Left;

		hitbox.offset = new Vector2(dir.ToFloat() * -Mathf.Abs(hitbox.offset.x), hitbox.offset.y);
		damageHitbox.offset = new Vector2(dir.ToFloat() * Mathf.Abs(damageHitbox.offset.x), damageHitbox.offset.y);
	}

	public void HitGround() {
		CurrentState = State.Running;
	}

	public void OpenMenu() {
		_inMenu = true;
        rb.velocity = new Vector2(0, rb.velocity.y);
	}

	public void CloseMenu() {
		_inMenu = false;
	}

	public bool MenuState() {
		return _inMenu;
	}

	bool HasPickup(PickUp type)
	{
		return inventory.HasPickUp(type);
	}

	void ResetState() {
		CurrentState = groundCheck.Grounded ? State.Running : State.Falling;
	}
	
	void Update () {
        if (_dead)
        {
            UpdateDeath();
            return;
        }
        
		float hMov = Input.GetAxis("Horizontal");
		float vMov = Input.GetAxis("Vertical");

		if (UpdateTimer(ref _attackDuration))
		{
			ResetState();
			damageHitbox.enabled = false;
		}
		else if (UpdateTimer(ref _bowDuration))
		{
			ResetState();
            _arrowShot = false;
        }
        else if (UpdateTimer(ref _bowJumpDuration))
        {
            ResetState();
            _arrowShot = false;
        }
        else if (UpdateTimer(ref _slideDuration))
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        if (CurrentState == State.Attacking && _attackDuration <= DamageTime)
        {
            damageHitbox.enabled = true;
        }

        // Arrow
        if ((CurrentState == State.Bow && _bowDuration <= ArrowTime && !_arrowShot) || (CurrentState == State.BowJump && _bowJumpDuration <= ArrowJumpTime && !_arrowShot))
            UpdateArrow();
        
        if (!_inMenu && _attackDuration <= 0f)
        {
            if (HasPickup(PickUp.DoubleJump))
                UpdateDoubleJump();

            if (HasPickup(PickUp.Sword))
                UpdateSword();

            if (HasPickup(PickUp.Bow))
                UpdateBow();

            if (HasPickup(PickUp.Sneak))
                UpdateSneak(hMov);

            if (!new State[] { State.Attacking, State.Bow, State.BowJump, State.Sneak }.Contains(CurrentState))
            {   
                UpdateMovement(hMov);
                UpdateJumping(hMov);
            }
        }

        if (CurrentState == State.BowJump)
        {
            rb.velocity = new Vector2(0, 0);
        }
        UpdateFalling();

        

        if (CurrentState == State.Running && !groundCheck.Grounded)
            CurrentState = State.Falling;

        UpdateAnimation(hMov);
    }

    void UpdateDeath() {
        rb.velocity = Vector2.zero;
        if (Input.GetButtonDown("Jump"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else if (Input.GetButtonDown("Bow"))
        {
            Application.Quit();
        }
    }

    void UpdateSword() {
        if (Input.GetButtonDown("Attack") && CurrentState == State.Running)
        {
            CurrentState = State.Attacking;
            _attackDuration = AttackDuration;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void UpdateSneak(float hMov)
    {
        if (_slideDuration <= 0)
        {
            if (Input.GetAxis("Sneak") == -1 && (CurrentState == State.Running || CurrentState == State.Sneak))
            {
                if (hitbox.offset.y != SneakHitboxHeight)
                {
                    hitbox.offset = new Vector2(hitbox.offset.x, SneakHitboxOffset);
                    hitbox.size = new Vector2(hitbox.size.x, SneakHitboxHeight);
                }

                CurrentState = State.Sneak;
                if (Mathf.Abs(hMov) > MovError)
                {
                    SetDirection(hMov.ToDirection());
                    _slideDuration = SlideTime;
                }
            }
            else if (CurrentState == State.Sneak) // TODO: CHECK FOR COLLISIONS
            {
                Vector2 botLeft = new Vector2(hitbox.bounds.min.x, hitbox.bounds.min.y + SneakHitboxHeight);
                Vector2 topRight = new Vector2(hitbox.bounds.max.x, hitbox.bounds.min.y + _hitboxYSize);

                Collider2D[] col = Physics2D.OverlapAreaAll(botLeft, topRight);

                if (col.Where(x => x.gameObject.tag == "Ground").Count() <= 0)
                {
                    ResetState();
                    hitbox.offset = new Vector2(hitbox.offset.x, _hitboxYOffset);
                    hitbox.size = new Vector2(hitbox.size.x, _hitboxYSize);
                }
            }
        }
        else
        {
            float mov = GetDirection().ToFloat() * Speed * Time.deltaTime * 60;

            rb.velocity = new Vector2(mov, rb.velocity.y);
        }
    }

    void UpdateBow()
    {
        if (Input.GetButtonDown("Bow"))
        {
            if (CurrentState == State.Running)
            {
                CurrentState = State.Bow;
                _bowDuration = BowDuration;
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else if (CurrentState == State.Falling || CurrentState == State.Jumping)
            {

                CurrentState = State.BowJump;
                _bowJumpDuration = BowJumpDuration;

            }
        }
    }

    void UpdateArrow()
    {
        GameObject arrow = Instantiate(ArrowPrefab);
        arrow.transform.position = transform.position + new Vector3(ArrowOffset.x * GetDirection().ToFloat(), ArrowOffset.y);

        if (GetDirection() == Direction.Left)
            arrow.GetComponent<SpriteRenderer>().flipX = true;

        _arrowShot = true;
    }

    void UpdateDoubleJump() {
        if (_doubleJumpPossible && (CurrentState == State.Jumping || CurrentState == State.Falling) && Input.GetButtonDown("Jump"))
        {
            CurrentState = State.Jumping;
            rb.velocity = new Vector2(rb.velocity.x, DoubleJumpSpeed);
            _doubleJumpPossible = false;
        }
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

	void UpdateMovement(float hMov) {
        float mov = hMov * Speed * Time.deltaTime * 60;
        
        rb.velocity = new Vector2(mov, rb.velocity.y);
        
		if (new State[] { State.Running, State.Falling, State.Jumping}.Contains(CurrentState) && Mathf.Abs(hMov) > MovError) 
			SetDirection(hMov.ToDirection());
    }

	void UpdateJumping(float hMov) {
		if (Input.GetButtonDown("Jump")) {
			if (CurrentState == State.Running) {
				rb.velocity += Vector2.up * JumpSpeed;
				CurrentState = State.Jumping;
                if (HasPickup(PickUp.DoubleJump))
                    _doubleJumpPossible = true;
				//SetDirection(hMov.ToDirection());
			}
		}
	}

	void UpdateFalling() {
		if (CurrentState == State.Jumping && rb.velocity.y < 0)
			CurrentState = State.Falling;
	}

	void ResetAllAnimation() {
		foreach (string anim in _allStates) {
			an.SetBool(anim, false);
		}
	}

	void UpdateAnimation(float hMov) {
        if (_dead)
        {
            ResetAllAnimation();
            return;
        }
		if (CurrentState != _lastState)
		{
			ResetAllAnimation();
			an.SetBool(CurrentState.ToString(), true);
			_lastState = CurrentState;
		}
		
		an.SetFloat("HSpeed", _inMenu ? 0 : _slideDuration > 0f ? 1 : Mathf.Abs(hMov));
		an.SetFloat("VSpeed", _inMenu ? 0 : rb.velocity.y);
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "DeathBarrier" || collision.gameObject.tag == "Enemy")
        {
            GameOver();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "EnemyDamage")
        {
            GameOver();
        }
    }

    void GameOver() {
        //GameOverText.enabled = true;
        GameOverScreen.GetComponent<Animation>().Play();
        hitbox.enabled = false;
        rb.gravityScale = 0;
        _dead = true;

        ResetAllAnimation();
        an.SetBool("Dead", true);
        //Time.timeScale = 0f;
        
        //Destroy(gameObject); // TODO
    }
}
