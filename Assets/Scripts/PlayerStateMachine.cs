using System;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerStateMachine : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float jumpYVelocity = 8f;
    [SerializeField] float runXVelocity = 4f;
    [SerializeField] float raycastDistance = 0.7f;
    [SerializeField] private float _attackDuration = 1f;
    [SerializeField] private float _currentAttack;
    [SerializeField] private float _slideDuration = 1f;
    [SerializeField] private float _currentSlide;
    [SerializeField] private float _standDuration = 0.5f;
    [SerializeField] private float _currentStand;
    [SerializeField] private float _hurtDuration = 0.2f;
    [SerializeField] private float _currentHurt;
    [SerializeField] private int _life = 3;
    [SerializeField] LayerMask collisionMask;

    Animator animator;
    Rigidbody2D physics;
    SpriteRenderer sprite;

    enum State { Idle, Run, Jump, Glide, Attack, Crouch, Slide, Death, Stand, Hurt }

    State state = State.Idle;
    bool isGrounded = false;
    bool jumpInput = false;
    bool isAttack = false;
    bool isCrouch = false;
    bool isHit = false;
    bool isDead = false;


    float horizontalInput = 0f;

    private void Update()
    {

    }
    void FixedUpdate()
    {
        // get player input
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, collisionMask).collider != null;
        jumpInput = Input.GetKey(KeyCode.Space);
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isAttack = Input.GetKey(KeyCode.E);
        if (Input.GetAxisRaw("Vertical") < 0)
        {
            isCrouch = true;
        }
        else
        {
            isCrouch = false;
        }


        // flip sprite based on horizontal input
        if (horizontalInput > 0f && !isDead)
        {
            sprite.flipX = false;
        }
        else if (horizontalInput < 0f && !isDead)
        {
            sprite.flipX = true;
        }

        // run current state
        switch (state)
        {
            case State.Idle: IdleState(); break;
            case State.Run: RunState(); break;
            case State.Jump: JumpState(); break;
            case State.Glide: GlideState(); break;
            case State.Attack: AttackState(); break;
            case State.Crouch: CrouchState(); break;
            case State.Slide: SlideState(); break;
            case State.Stand: StandState(); break;
            case State.Death: DeathState(); break;
            case State.Hurt: HurtState(); break;
        }
    }



    void IdleState()
    {
        // actions
        animator.Play("Idle");

        // transitions
        if (isGrounded)
        {
            if (isAttack && horizontalInput == 0f)
                state = State.Attack;
            if (isCrouch)
                state = State.Crouch;
            if (jumpInput)
            {
                state = State.Jump;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Run;
            }
            if (_life <= 1 && isHit)
            {
                state = State.Death;
            }
            else if (isHit)
            {
                state = State.Hurt;
            }

        }
    }

    void RunState()
    {
        // actions
        animator.Play("Run");
        physics.velocity = new Vector2(runXVelocity * horizontalInput, physics.velocity.y);

        // transitions
        if (isGrounded && jumpInput)
        {
            state = State.Jump;
        }
        else if (horizontalInput == 0f)
        {
            state = State.Idle;
        }
        if (_life <= 1 && isHit)
        {
            state = State.Death;
        }
        else if (isHit)
        {
            state = State.Hurt;
        }
    }

    void JumpState()
    {
        // actions
        animator.Play("Jump");
        physics.velocity = runXVelocity * horizontalInput * Vector2.right + jumpYVelocity * Vector2.up;

        // transitions
        state = State.Glide;
        if (_life <= 1 && isHit)
        {
            state = State.Death;
        }
        else if (isHit)
        {
            state = State.Hurt;
        }

    }

    void GlideState()
    {
        // actions
        if (physics.velocity.y > 0f)
        {
            animator.Play("Jump");
        }
        else
        {
            animator.Play("Fall");
        }

        physics.velocity = physics.velocity.y * Vector2.up + runXVelocity * horizontalInput * Vector2.right;

        // transitions
        if (isGrounded)
        {
            if (horizontalInput != 0f)
            {
                state = State.Run;
            }
            else
            {
                state = State.Idle;
            }
            if (_life <= 1 && isHit)
            {
                state = State.Death;
            }
            else if (isHit)
            {
                state = State.Hurt;
            }
        }
        if (_life <= 1 && isHit)
        {
            state = State.Death;
        }
        else if (isHit)
        {
            state = State.Hurt;
        }
    }
    void AttackState()
    {
        animator.Play("Attack");
        _currentAttack += Time.fixedDeltaTime;
        // transitions
        if (_currentAttack > _attackDuration)
        {
            _currentAttack = 0f;
            state = State.Idle;

        }
        if (_life <= 1 && isHit)
        {
            state = State.Death;
        }
        else if (isHit)
        {
            state = State.Hurt;
        }
    }
    void CrouchState()
    {
        animator.Play("Crouch");
        if (!isCrouch)
            state = State.Idle;
        if (jumpInput)
            state = State.Slide;
        if (_life <= 1 && isHit)
        {
            state = State.Death;
        }
        else if (isHit)
        {
            state = State.Hurt;
        }
    }
    void SlideState()
    {
        animator.Play("Slide");
        _currentSlide += Time.fixedDeltaTime;
        if (sprite.flipX == false)
            physics.velocity = new Vector2(runXVelocity * 1, physics.velocity.y);
        if (sprite.flipX == true)
            physics.velocity = new Vector2(runXVelocity * -1, physics.velocity.y);
        if (_currentSlide > _slideDuration)
        {
            _currentSlide = 0f;
            physics.velocity = new Vector2(0, physics.velocity.y);
            state = State.Stand;
        }
        if (_life <= 1 && isHit)
        {
            state = State.Death;
        }
        else if (isHit)
        {
            state = State.Hurt;
        }
    }
    private void StandState()
    {
        animator.Play("Stand");
        _currentStand += Time.fixedDeltaTime;
        if (_currentStand > _standDuration)
        {
            _currentStand = 0f;
            if (horizontalInput != 0f)
                state = State.Run;
            else if (isCrouch)
                state = State.Crouch;
            else
                state = State.Idle;
        }
        if (_life <= 1 && isHit)
        {
            state = State.Death;
        }
        else if (isHit)
        {
            state = State.Hurt;
        }
    }
    private void DeathState()
    {
        animator.Play("Death");
        _life = 0;
        isDead = true;
    }
    private void HurtState()
    {
        animator.Play("Hurt");
        _currentHurt += Time.fixedDeltaTime;
        isHit = false;
        if (sprite.flipX == false)
            physics.velocity = new Vector2(runXVelocity * -0.5f, physics.velocity.y);
        if (sprite.flipX == true)
            physics.velocity = new Vector2(runXVelocity * 0.5f, physics.velocity.y);
        if (_currentHurt > _hurtDuration)
        {
            _life--;
            _currentHurt = 0f;
            if (horizontalInput != 0f)
                state = State.Run;
            else if (isCrouch)
                state = State.Crouch;
            else if (_life == 0)
                state = State.Death;
            else
                state = State.Idle;

        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == 11)
        {
            isHit = true;

        }
    }


    void Awake()
    {
        animator = GetComponent<Animator>();
        physics = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }
}
