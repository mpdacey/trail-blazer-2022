using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 1;
    public float jumpforce = 5;
    public LayerMask groundLayer;
    public Joystick mobileJoyStick;
    public Button mobileJump;
    public bool isMobile = true;
    public bool HasJumped
    {
        get { return jumpPressed; }
    }
    public bool SetControl
    {
        set { hasControl = value; }
    }

    Rigidbody rb;
    float horizontalInput = 0;
    float verticalInput = 0;
    bool jumpPressed = false;
    bool isGrounded = true;
    bool hasJumped = false;
    bool hasControl = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (isMobile) mobileJump.onClick.AddListener(() => jumpPressed = true);
    }

    // Update is called once per frame
    void Update()
    {
        if (isMobile)
        {
            horizontalInput = mobileJoyStick.Horizontal;
            verticalInput = mobileJoyStick.Vertical;
        }
        else
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
            jumpPressed = Input.GetButton("Jump");
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);

        if (hasControl)
        {
            //Scary Math Reference: https://stackoverflow.com/a/32391780
            float xCircle = horizontalInput * Mathf.Sqrt(1 - 0.5f * (horizontalInput * horizontalInput));
            float zCircle = verticalInput * Mathf.Sqrt(1 - 0.5f * (verticalInput * verticalInput));

            rb.MovePosition(transform.position + new Vector3(xCircle, 0, zCircle) * Time.fixedDeltaTime * speed);

            if (jumpPressed && isGrounded && !hasJumped)
            {
                GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayPlayerAudioClip(0);
                StartCoroutine("JumpCooldown");
                GetComponent<PlayerLife>().ChangeFlame = -4;
                rb.AddForce(new Vector3(0, jumpforce * (Physics.gravity.y / -9.81f)), ForceMode.Impulse);
                isGrounded = false;
                jumpPressed = false;
            }

            //Thanks Michael Hinz :)
            isGrounded = Physics.Linecast(transform.position, transform.position + Vector3.down / 5.5f, groundLayer);
            //Debug.DrawLine(transform.position, transform.position + Vector3.down / 6.5f, Color.blue);
        }
    }

    IEnumerator JumpCooldown()
    {
        hasJumped = true;
        yield return new WaitForSeconds(0.5f);
        hasJumped = false;
    }
}
