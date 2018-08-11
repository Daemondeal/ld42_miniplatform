using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#pragma warning disable 414, 219

public class PlatformCharController : MonoBehaviour {


	public float Speed = 20f;
	public float JumpSpeed = 48f;

    [Tooltip("How much slow you need to be considered not moving")]
    public float MovError = 0.1f;

    [Tooltip("Attack duration, in seconds")]
    public float AttackDuration = 1f;

	public State CurrentState = State.Running;

	Rigidbody2D rb;
	Animator an;
	SpriteRenderer sr;
	Collider2D hitbox;
	AudioSource audios;

    Collider2D damageHitbox;
    InventoryController inventory;
	GroundedCollider groundCheck;
    
    string[] _allStates;
    State _lastState; // For animation purposes
    bool _inMenu; // Disables input while in menu
    float _attackDuration = 0f;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
		an = GetComponent<Animator>();
		sr = GetComponent<SpriteRenderer>();
		hitbox = GetComponent<Collider2D>();
		audios = GetComponent<AudioSource>();
        inventory = GetComponent<InventoryController>();

		groundCheck = GetComponentInChildren<GroundedCollider>();
        damageHitbox = GameObject.FindGameObjectWithTag("Damage").GetComponent<Collider2D>();

        // Gets all the possible states in a string array, for animation purposes
        _allStates = ((State[])System.Enum.GetValues(typeof(State))).Select(x => x.ToString()).ToArray();
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
		float hMov = Input.GetAxis("Horizontal");
		float yMov = Input.GetAxis("Vertical");

        if (UpdateTimer(ref _attackDuration))
        {
            ResetState();
            damageHitbox.enabled = false;
        }        

        if (!_inMenu && CurrentState != State.Attacking && _attackDuration <= 0f)
        {
            UpdateMovement(hMov);
            UpdateJumping(hMov);
        }
        UpdateFalling();

        if (HasPickup(PickUp.Sword) && Input.GetButtonDown("Attack") && CurrentState == State.Running)
        {
            CurrentState = State.Attacking;
            _attackDuration = AttackDuration;
            damageHitbox.enabled = true;
        }

		if (CurrentState == State.Running && !groundCheck.Grounded)
			CurrentState = State.Falling;

        UpdateAnimation(hMov);
	}

    bool UpdateTimer(ref float timer)
    {
        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            timer = 0f;
            return true;
        }
        return false;
    }

	void UpdateMovement(float hMov) {
		rb.velocity = new Vector2(hMov * Speed, rb.velocity.y);

        if (CurrentState == State.Running && Mathf.Abs(hMov) > MovError) 
            SetDirection(hMov.ToDirection());
	}

	void UpdateJumping(float hMov) {
		if (Input.GetButtonDown("Jump")) {
			if (CurrentState == State.Running) {
				rb.velocity += Vector2.up * JumpSpeed;
				CurrentState = State.Jumping;
				SetDirection(hMov.ToDirection());
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
        if (CurrentState != _lastState)
        {
            ResetAllAnimation();
            an.SetBool(CurrentState.ToString(), true);
            _lastState = CurrentState;
        }

        
        an.SetFloat("HSpeed", _inMenu ? 0 : Mathf.Abs(hMov));
        an.SetFloat("VSpeed", _inMenu ? 0 : rb.velocity.y);
    }
}
