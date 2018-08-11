using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 414, 219

public class PlatformCharController : MonoBehaviour {


    public float Speed = 20f;
    public float JumpSpeed = 48f;

    public State CurrentState = State.Running;

    Rigidbody2D rb;
    Animator an;
    SpriteRenderer sr;
    Collider2D hitbox;
    AudioSource audios;

    GroundedCollider groundCheck;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        an = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<Collider2D>();
        audios = GetComponent<AudioSource>();

        groundCheck = GetComponentInChildren<GroundedCollider>();
	}

    public Direction GetDirection() {
        return sr.flipX ? Direction.Left : Direction.Right;
    }

    public void SetDirection(Direction dir)
    {
        if (dir == GetDirection())
            return;

        sr.flipX = dir == Direction.Left;
    }

    public void HitGround() {
        CurrentState = State.Running;
    }

    void ResetState() {
        CurrentState = groundCheck.Grounded ? State.Running : State.Falling;
    }

	// Update is called once per frame
	void Update () {
        float hMov = Input.GetAxis("Horizontal");
        float yMov = Input.GetAxis("Vertical");

        
        UpdateMovement(hMov);
        UpdateJumping(hMov);

        if (CurrentState == State.Running && !groundCheck.Grounded)
            CurrentState = State.Falling;
	}

    void UpdateMovement(float hMov) {
        rb.velocity = new Vector2(hMov * Speed, rb.velocity.y);
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
}
