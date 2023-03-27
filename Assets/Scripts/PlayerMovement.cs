using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    private BoxCollider2D coll;

    [SerializeField] private LayerMask jumpableGround;

    private float dirX;
    [SerializeField] private float playerSpeed = 14f;
    [SerializeField] private float jumpForce = 14f;

    private enum MovementState {idle, running, jumping, falling}

    [SerializeField] private AudioSource jumpSound;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        dirX = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(dirX * playerSpeed, rb.velocity.y);

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            jumpSound.Play();
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
            

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {

        MovementState state;
        if (dirX > 0)
        {
            state = MovementState.running;
            sprite.flipX = false;
        }
        else if (dirX < 0)
        {
            state = MovementState.running;
            sprite.flipX = true;
        }
        // When dirX is 0, the player is not moving.
        else
        {
            state = MovementState.idle;
        }

        // This if statement is checking if the player is in the air.
        // We check if Y velocity is greater than .1f and not 0 because
        // in the normal state the velocity is always greater than 0 on the 
        // ground.
        if (rb.velocity.y > .1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -.1f)
        {
            state = MovementState.falling;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        /**
         * With the Box cast we are creating a box around the Player.
         * coll.bouds.center and coll.bounds.size are making a box.
         * 0f is the rotation of the box which is zero as we don't want to rotate the box.
         * The Vector2.down makes the box to be a little under the player to only detect the
         * collisions happening below and not on right and left sides. 
         */
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }
}
