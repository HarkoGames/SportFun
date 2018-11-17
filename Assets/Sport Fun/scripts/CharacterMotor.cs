using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SportFun
{
    public enum ActionKeys { Run, Jump, Fight, Attack, Dodge, Block }

    public class CharacterMotorSettings
    {
        public float TurnSpeed { get; set; }
        public float WalkSpeed { get; set; }
        public float RunSpeed { get; set; }
        public float CrouchSpeed { get; set; }
        public float SlowDown { get; set; }
        public float JumpPower { get; set; }
    }

    public class CharacterMotor : MonoBehaviour
    {
        private Animator myAnim;
        public bool UseRootMotion;
        [HideInInspector]
        public float TurnSpeed = 10f;
        [HideInInspector]
        public float WalkSpeed = 2f;
        [HideInInspector]
        public float RunSpeed = 5f;
        [HideInInspector]
        public float CrouchSpeed = 1f;
        [HideInInspector]
        public float SlowDown = 1f;
        [HideInInspector]
        public float JumpPower = 5f;

        private bool mIsGrounded = false;
        //public bool IsGrounded { get { return mIsGrounded; } }
        public bool IsGrounded { get { return CheckGrounded(); } }
        private float distToGround;
        public float AirTimeError = .5f;

        private Rigidbody _rigidbody;
        public void SetCollisionDetectionMode(CollisionDetectionMode mode)
        {
            _rigidbody.collisionDetectionMode = mode;
        }

        public Vector3 Velocity
        {
            get
            {
                return _rigidbody != null ? _rigidbody.velocity : Vector3.zero;
            }
        }

        private Dictionary<int, bool> mActionKeys;

        public void ClearActionKeys()
        {
            foreach (var ak in mActionKeys.Keys)
            {
                mActionKeys[ak] = false;
            }
        }

        public bool GetActionKey(ActionKeys ak)
        {
            bool outAction = false;
            mActionKeys.TryGetValue((int)ak, out outAction);
            return outAction;
        }

        public void SetActionKey(ActionKeys ak, bool val)
        {
            if (!mActionKeys.ContainsKey((int)ak))
            {
                mActionKeys.Add((int)ak, false);
            }
            mActionKeys[(int)ak] = val;
        }

        private void Awake()
        {
            mActionKeys = new Dictionary<int, bool>();
            _rigidbody = GetComponent<Rigidbody>();
            var c = GetComponent<CapsuleCollider>();
            distToGround = c.bounds.extents.y;
        }

        public void Init(CharacterMotorSettings s)
        {
            TurnSpeed = s.TurnSpeed;
            WalkSpeed = s.WalkSpeed;
            RunSpeed = s.RunSpeed;
            CrouchSpeed = s.CrouchSpeed;
            SlowDown = s.SlowDown;
            JumpPower = s.JumpPower;
        }

        private void OnEnable()
        {
            mActionKeys.Clear();
        }

        private void OnDisable()
        {

        }
        // Use this for initialization
        void Start()
        {
            myAnim = GetComponent<Animator>(); // can be null
           // myAnim.SetBool("InCombat", true);
        }

        bool CheckGrounded()
        {
            var checkPos = transform.position + (Vector3.up * .25f);
            Debug.DrawLine(checkPos, checkPos + (Vector3.down  * (distToGround + AirTimeError)), Color.red, 1f);
            return Physics.Raycast(checkPos, -Vector3.up, distToGround + AirTimeError);
        }

        string message;

        void OnGUI()
        {
            if (!this.CompareTag("Player"))
            {
                //GUI.Label(new Rect(50, 200, 1200, 200), string.Format("character motor: {0}", message));
            }
        }

        public Vector3 moveInput;
        public Vector3 rotationInput;
        float turnAmount;
        float forwardAmount;
        public float maxVelocityChange = 10f;
        private float mInputMagnitude;

        private void FixedUpdate_disabled()
        {
            mIsGrounded = CheckGrounded();
        }

        public void Jump()
        {
            this.moveInput = Vector3.up * JumpPower;
            _rigidbody.velocity += this.moveInput;
//            GroundedVelocity(true);
        }

        public void Move(Vector3 move, Vector3 rotation, bool additive = false)
        {
            // 'this' to make clear it's assigning a member variable ?
            this.moveInput = move;
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
            this.rotationInput = rotation;
            mInputMagnitude = moveInput.magnitude;
            var adjustedMoveInput = transform.InverseTransformDirection(moveInput);
            Debug.DrawRay(transform.position + (Vector3.up * 1.05f), transform.rotation * Vector3.forward, Color.green);
            Debug.DrawRay(transform.position + (Vector3.up * 1.05f), moveInput, Color.magenta);
            var facingDiff = Utilities.GetFacingDifference(transform.rotation * Vector3.forward, moveInput);

            message = "";
            /*
            if (UseRootMotion)
            {
                UpdateAnimator(adjustedMoveInput);
                GroundedRotation();
                GroundedVelocity();
            }
            else
            {
            */
                GroundedRotation();
                GroundedVelocity(additive);
            UpdateAnimator(adjustedMoveInput, facingDiff);// transform.InverseTransformDirection(mVelocity.normalized));
            /*
            }
            */
        }

        private Quaternion mRotation;
        private Vector3 mVelocity;
        public float walkClamp = .75f;
        public float runClamp = 1f;

        public void UpdateAnimator(Vector3 moveInput, float facingDiff,  bool jump = false, bool crouch = false)
        {
            if (myAnim != null)
            {
                var magnitude = Mathf.Clamp(mInputMagnitude, 0f, walkClamp); // walk clamp
                if (GetActionKey(ActionKeys.Run)) magnitude = Mathf.Clamp(mInputMagnitude, 0f, runClamp); // run clamp
                myAnim.SetFloat("vMagnitude", magnitude, .1f, Time.deltaTime);
                myAnim.SetFloat("zSpeed", moveInput.z, .1f, Time.deltaTime);
                myAnim.SetFloat("xSpeed", moveInput.x, .1f, Time.deltaTime);
                myAnim.SetFloat("FacingDifference", facingDiff);
            }
        }

        void OnAnimatorMove()
        {
            //if (!UseRootMotion) return;

            //moveInput = myAnim.deltaPosition;
            message += string.Format("\nMoveInput for animator: {0}\n", myAnim.deltaPosition);

        }
        void GroundedRotation()
        {
            mRotation = Utilities.CalculateFacingDirection(transform, rotationInput, TurnSpeed);
            transform.rotation = mRotation;
            message += string.Format("Rotation Input: {0} New Rotation: {1}\n", rotationInput, mRotation);
        }


        private float tempSpeed;

        void GroundedVelocity(bool additive = false)
        {
            Vector3 velocityChange = Vector3.zero;

            Vector3 drawPosition2 = transform.position + new Vector3(0, 1, 0);
            Debug.DrawLine(drawPosition2, drawPosition2 + moveInput, Color.red);

            Vector3 velocity = Velocity;
           // if (moveInput.magnitude > 1f) moveInput.Normalize(); // new
            if (moveInput.magnitude > 0)
            {
                // convert the world relative moveInput vector into a local-relative
                // turn amount and forward amount required to head in the desired
                // direction. 
                /*
                Vector3 localMove = transform.InverseTransformDirection(moveInput);
                turnAmount = Mathf.Atan2(localMove.x, localMove.z);
                forwardAmount = localMove.z;
                */
                /* this need to move to the stat based character specific file*/
                moveInput = ((transform.position + moveInput) - transform.position).normalized;
                if (GetActionKey(ActionKeys.Run))
                {
                    tempSpeed = RunSpeed;
                }
                else
                {
                    tempSpeed = WalkSpeed;
                }
                moveInput *= (tempSpeed * mInputMagnitude);

                // Apply a force that attempts to reach our target velocity
                if (!additive)
                {
                    velocityChange = (moveInput - velocity);
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                    //velocityChange.y = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                }
                else // add additional force to our current velocity
                {
                    velocityChange = moveInput;
                }

                //rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
            }

            else if (velocity != Vector3.zero) // if no input, but we're still moving
            {
                //try to stop
                //            Vector3 stopDir = 2.0f * transform.position - velocity;
                //            velocityChange = stopDir * tempSlowDown;
                velocityChange = Vector3.down * SlowDown;
            }
            //            GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.Force);
            message += string.Format("moveInput: {0} VelocityChange {1} Velocity {2}", moveInput, velocityChange, GetComponent<Rigidbody>().velocity);
            //message += "\ntargetVelocity: " + moveInput.ToString() + " velocityChange: " + velocityChange.ToString();
            if (!UseRootMotion)
            {
                message += "\n Changing Velocity";
                GetComponent<Rigidbody>().velocity += velocityChange * mInputMagnitude;
            }
            mVelocity = velocityChange;
//            mVelocity = GetComponent<Rigidbody>().velocity; // not sure which it would be
        }


        /*
        public void OnAnimatorMove()
        {
            Animator animator = GetComponent<Animator>();
            if (animator)
            {
                Vector3 newPosition = transform.position;
                newPosition.z += animator.deltaPosition.z;
                newPosition.x += animator.deltaPosition.x;
                transform.position = newPosition;
            }
        }
        */
        #region utilities
        #endregion
    }
}
