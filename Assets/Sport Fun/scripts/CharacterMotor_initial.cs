using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
//[RequireComponent(typeof(Animator))]
public class CharacterMotor_initial : MonoBehaviour
{
    public enum MovementFB
    {
        Back = -1,
        None = 0,
        Forward = 1
    };

    public enum MovementLR
    {
        Left = -1,
        None = 0,
        Right = 1
    };


    // make private later
    public float AirTime = 0;
    public float AirTimeError = .25f; // for some reason it gets airtime just standing around.  idle anim breaking contact with the ground?

    private Transform myTransform;
    //movement stuff
    private Vector3 moveDirection;
    private MovementLR controlRotation;
    private MovementFB controlFB;
    private MovementLR controlLR;
    private Vector3 desiredRotation;
    private Vector3 mouseLookRotation;
    private Vector3 desiredVelocity;
    public Vector3 DesiredVelocity
    {
        get
        {
            return desiredVelocity;
        }
        set
        {
            desiredVelocity = value;
        }
    }

    public Vector3 targetVelocity;
    private bool movementJump;

    public float RotateSpeed = 250;
    public float WalkSpeed = 5;
    public float StrafeSpeed = 2.5f;
    public float RunMultiplier = 4;
    public float FallTime = .5f;	// time in the air until you're 'falling'
    public float JumpHeight = 9;
    public float JumpTime = 1.5f;

    public float speed;
    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    //public float jumpHeight = 2.0f;
    private bool grounded;
    public Vector3 center { get { return GetComponent<CapsuleCollider>().center; } }


    private bool isJumping;
    private bool isRunning;
    private bool isFalling;
    private bool InCombat;
    public bool hitByForce;



    private Animator myAnim;

    void Awake()
    {
        hitByForce = false;
        myAnim = GetComponent<Animator>();
        myTransform = transform;

        GetComponent<Rigidbody>().freezeRotation = true;
        GetComponent<Rigidbody>().useGravity = false;
    }

    private void Setup()
    {

        moveDirection = Vector3.zero;

        controlRotation = MovementLR.None;
        controlFB = MovementFB.None;
        controlLR = MovementLR.None;

        movementJump = isRunning = InCombat = false; // crc.IsFalling = crc.IsDecending = crc.IsGrounded = InCombat = false;
    }

    private void Init()
    {
    }

    public void SetDesiredVelocity(Vector3 dv)
    {
        desiredVelocity = dv;
    }

    public void SetDesiredRotation(Vector3 dr)
    {
        desiredRotation = dr;
    }

    public void SetMouseLookRotation(Vector3 dr)
    {
        mouseLookRotation = dr;
    }

    public void ControlRun(bool r)
    {
        isRunning = r;
    }

    public void ControlJump(bool j)
    {
        movementJump = j;
    }

    private bool attackPressed;
    public void ControlAttack(bool a)
    {
        attackPressed = a;
    }

    public void SetCombatMode(bool combat)
    {
        InCombat = combat;
    }

    private string message;
    #region utilities
    private Quaternion CalculateFacingDirection(Vector3 dr)
    {
        Quaternion facingDir = new Quaternion();
        if (dr != Vector3.zero) // only do rotation if there is actually some input
        {
            /* handle facing direction - taken from charactermotor code in gta shooter example*/
            dr = (dr.magnitude > 1 ? dr.normalized : dr);
            // Calculate which way character should be facing
            float facingWeight = dr.magnitude;
            Vector3 combinedFacingDirection = (
                transform.rotation * dr * (1 - facingWeight)
                + dr * facingWeight
            );
            //message += " combined Facing Direction: " + combinedFacingDirection.ToString();
            combinedFacingDirection = combinedFacingDirection - Vector3.Project(combinedFacingDirection, transform.up); //Util.ProjectOntoPlane(combinedFacingDirection, transform.up);
            combinedFacingDirection = .5f * combinedFacingDirection; // alignCorrection .5f ?

            if (combinedFacingDirection.sqrMagnitude > 0.01f)
            {
                Vector3 newForward = Vector3.Slerp(transform.forward, combinedFacingDirection, RotateSpeed * Time.deltaTime);//Util.ConstantSlerp(transform.forward, combinedFacingDirection, RotateSpeed * Time.deltaTime);
                newForward = newForward - Vector3.Project(newForward, transform.up);//Util.ProjectOntoPlane(newForward, transform.up);   

                Vector3 drawPosition = transform.position + new Vector3(0, 2, 0);
                Debug.DrawLine(drawPosition, drawPosition + newForward, Color.yellow);

                facingDir.SetLookRotation(newForward, transform.up);
            }
        }
        else
            //facingDir = myAnim.bodyRotation; // don't change if no input vector was sent
            facingDir = transform.rotation; // don't change if no input vector was sent

        //message += "dr: " + dr + " facingDir: " + facingDir.ToString();
        return facingDir;
    }

    private float GetFacingDifference(Quaternion rotA, Quaternion rotB)
    {
        // found on unity answers http://answers.unity3d.com/questions/26783/how-to-get-the-signed-angle-between-two-quaternion.html
        float diff = 0f;

        // get a "forward vector" for each rotation
        var forwardA = rotA * Vector3.forward;
        var forwardB = rotB * Vector3.forward;

        // get a numeric angle for each vector, on the X-Z plane (relative to world forward)
        var angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
        var angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;

        // get the signed difference in these angles
        diff = Mathf.DeltaAngle(angleA, angleB);

        return diff;
    }
    
    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        //return Mathf.Sqrt(2 * (JumpHeight + (myToon.GetPrimaryAttribute(AttributeName.Might).AdjustedBaseValue * .01f) * gravity));
        return Mathf.Sqrt(2 * JumpHeight * gravity);
    }

    #endregion
    void OnGUI()
    {
        GUI.Label(new Rect(50, 10, 1200, 60), message);
    }
    /*
    void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, -Vector3.up);
    }
    */
    private void ActionPicker()
    {
        message = "";
        float rotateValue = (int)controlRotation; //myTurn; //Input.GetAxis ("RotateCharacter");
        /*
        if (rotateValue != 0)
        {
            myTransform.Rotate(0, rotateValue * Time.deltaTime * RotateSpeed, 0);
        }
        */
        Quaternion newRotation = new Quaternion();
        if (InCombat && tag == "Player")
        {
            newRotation = CalculateFacingDirection(mouseLookRotation);
            RotateSpeed = 5;
        }
        else
        {
            newRotation = CalculateFacingDirection(desiredRotation);
            //RotateSpeed = myToon.GetPrimaryAttribute(AttributeName.Nimbleness).AdjustedBaseValue * .1f;
        }

        myTransform.rotation = newRotation;

        Vector3 drawPosition2 = transform.position + new Vector3(0, 1, 0);
        Debug.DrawLine(drawPosition2, drawPosition2 + desiredVelocity, Color.red);

        Vector3 moveDir = ((myTransform.position + desiredVelocity) - myTransform.position).normalized;
        float moveForwardValue = Vector3.Dot(moveDir, myTransform.forward);//(int)controlFB; //Input.GetAxis ("MoveForward");
        float strafeValue = Vector3.Dot(moveDir, myTransform.right);//(int)controlLR; //Input.GetAxis ("Strafe");

        //message = "r: " + rotateValue + " m: " + moveForwardValue + " s: " + strafeValue;

        //message += " moveDir: " + moveDir.ToString() + " FB: " + moveForwardValue + " strafe: " + strafeValue;

        if (grounded) // grounding doublecheck, just to make sure it's not collider jitter
            grounded = Physics.Raycast(transform.position, -Vector3.up, AirTimeError);

        if (grounded)
        {

            /*
            moveDirection = new Vector3(strafeValue,0,moveForwardValue);
            moveDirection = myTransform.TransformDirection(moveDirection).normalized; // normalized makes it a vector of length 1, 
                                                                    //so we can multiply it by the speeds and such
             */
            moveDirection = moveDir;//desiredVelocity;

            speed = WalkSpeed;
            if (moveForwardValue > 0) // || strafeValue != 0)//(Input.GetButton("MoveForward"))
            {
            }
            else if (moveForwardValue < 0)
            {
                speed *= .5f; // backwards is slower
            }

            //crc.IsRunning = isRunning;
            if (isRunning)
            {
                //speed *= myToon.ExtendedAttributes[ExtendedAttributeName.RunSpeed];
            }
            moveDirection *= speed;

            myAnim.SetBool("Moving", (moveDir != Vector3.zero));
            if (tag == "Player")
            {
                myAnim.SetBool("Run", isRunning);
            }
            //myAnim.SetVector("MovementVector", moveDirection);
            myAnim.SetFloat("MovementSpeed", moveForwardValue); // *(!isRunning ? .5f : 1);
            myAnim.SetFloat("StrafeLeftRightSpeed", strafeValue);
            myAnim.SetFloat("TurnLeftRightSpeed", rotateValue, .5f, Time.deltaTime);
        }

        JumpUpdate();
        if (tag == "Player")
        {
            //message += " attack counter: " + myToon.MeleeAttackTimer;
            //message += "Name: " + myToon.CharacterName + " moveDir: " + moveDir.ToString() + " FB: " + moveForwardValue + " strafe: " + strafeValue + " rotate: " + rotateValue;
            message += " grounded: " + grounded;
            //message += "\n moveDirection is " + moveDirection.ToString() + "speed: " + speed + " velocity: " + rigidbody.velocity.ToString();

            // replace direct call with message?
            //if (message != "") myHUD.UpdateLabel("l_Movement", message);
        }
    }

    private float jumpArc;
    bool jumpStart = false;
    bool jumpLand = false;

    void JumpUpdate()
    {

        Vector3 vel = GetComponent<Rigidbody>().velocity;

        if (movementJump)
        {
            jumpArc = 0f;
            if (grounded && AirTime < AirTimeError)
            {
                isJumping = true;
                jumpStart = true;
            }
            else
            {
                //double jump?
            }
        }
        else // separate the jump starting from the actual jumping frame
        {
            if (!grounded)
            {
                if (AirTime > AirTimeError)
                {
                    if (!isJumping) // && AirTime > FallTime) // we didn't initiate this, so we're falling
                    {
                        jumpStart = true; // transition to jump
                    }
                    else
                    {
                        jumpStart = false; // we initiated this jump, so now we unset this var.
                    }
                    jumpLand = false;
                    isJumping = true;
                    if (vel.y > 0)
                        jumpArc = Mathf.Clamp(jumpArc + Time.deltaTime, .01f, .5f);
                    else if (vel.y <= 0)
                        jumpArc = Mathf.Clamp(jumpArc + Time.deltaTime, .5f, 1f);
                }
                AirTime += Time.deltaTime;
            }
            else
            {
                if (isJumping & !jumpStart) // landed
                {
                    jumpLand = true;
                    AirTime = 0;
                    jumpArc = 0;
                    isJumping = false;
                }
            }
        }

        myAnim.SetBool("JumpStart", jumpStart);
        myAnim.SetFloat("JumpArc", jumpArc);
        myAnim.SetBool("JumpLand", jumpLand);
    }

    void FixedUpdate()
    {
            if (InCombat) myAnim.SetLayerWeight(1, 1); // blend in the arm layer
            else myAnim.SetLayerWeight(1, 0);

            AnimatorStateInfo currentAnimState0 = myAnim.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo[] animInfo = myAnim.GetCurrentAnimatorClipInfo(0);

            if (grounded && !hitByForce)
            {
                // Calculate how fast we should be moving
                //            Vector3 targetVelocity = new Vector3(Input.GetAxis("MoveForward"), 0, Input.GetAxis("Strafe"));
                //targetVelocity = transform.TransformDirection(targetVelocity); //transform from local space to world space
                /*
                targetVelocity = myAnim.GetVector("MovementVector");
                targetVelocity *= speed;
                */
                targetVelocity = moveDirection;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = GetComponent<Rigidbody>().velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;
                GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.VelocityChange);

                // Jump
                if (canJump && isJumping)// Input.GetButton("Jump"))
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                }
                if (tag == "Player")
                {
                    //message += "targetVelocity: " + targetVelocity.ToString() + " velocityChange: " + maxVelocityChange.ToString();
                }
            }

            // We apply gravity manually for more tuning control
            GetComponent<Rigidbody>().AddForce(new Vector3(0, -gravity * GetComponent<Rigidbody>().mass, 0));
    }
    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
        /*
        foreach (ContactPoint contact in other.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal * 10, Color.white);
        }
         */
    }


}
