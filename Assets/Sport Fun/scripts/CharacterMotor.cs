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
        //[HideInInspector]
        public float SlowDown = 1f;
        [HideInInspector]
        public float JumpPower = 5f;

        private bool mIsGrounded = false;
        private bool mIsGroundedNoError = false;
        public bool IsGrounded { get { return mIsGrounded; } }
//        public bool IsGrounded { get { return CheckGrounded(); } }
        public bool IsGroundedNoError { get { return mIsGroundedNoError; } }
 //       public bool IsGroundedNoError { get { return CheckGrounded(true); } }
        private float slope;
        private Vector3 slopeNormal;
        public Transform groundCheck;
        private float distToGround;
        public float AirTimeError = .5f;
        private Collider footCollider;
        private Collider torsoCollider; 

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
            torsoCollider = GetComponent<CapsuleCollider>();
            footCollider = GetComponent<SphereCollider>();
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
        }

        void CheckGrounded()
        {
            distToGround = footCollider.bounds.extents.y;
//            AirTimeError = .05f;
            var error = AirTimeError;
            var checkPos = groundCheck.position;
            Debug.DrawLine(checkPos, checkPos + (Vector3.down * (distToGround + error)), Color.red, 1f);
            Debug.DrawLine(checkPos, checkPos + (Vector3.down * (distToGround)), Color.green, 1f);
            RaycastHit hit;
            Ray r = new Ray(checkPos, Vector3.down);
            mIsGrounded = false;
            slope = 0f;
            slopeNormal = Vector3.zero;
            if( Physics.Raycast(r, out  hit, distToGround + error))
            {
                slopeNormal = hit.normal;
                slope = Vector3.Angle(hit.normal, Vector3.up);
                Debug.DrawRay(transform.position, hit.normal, Color.gray);

                mIsGrounded = true;
            }
            mIsGroundedNoError = false;
            if (Physics.SphereCast(checkPos,distToGround,Vector3.down, out hit))
            {
                mIsGroundedNoError = true;
            }
        }

        string message;

        void OnGUI()
        {
            if (!this.CompareTag("Player"))
            {
                //GUI.Label(new Rect(50, 200, 1200, 200), string.Format("character motor: {0}", message));
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(groundCheck.position, distToGround);
        }

        public Vector3 moveInput;
        public Vector3 rotationInput;
        float turnAmount;
        float forwardAmount;
        public float maxVelocityChange = 10f;
        private float mInputMagnitude;

        private void FixedUpdate()
        {
            CheckGrounded();
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
            moveInput = move;
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
            rotationInput = rotation;
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
            Vector3 drawPosition2 = transform.position + new Vector3(0, 1, 0);
            Vector3 velocityChange = Vector3.zero;

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

                // adjust the input by the slope, so that going up/ downhill is smoother
                var rot = Quaternion.FromToRotation(Vector3.up, slopeNormal);

                moveInput = rot * moveInput;
                Debug.DrawLine(drawPosition2, drawPosition2 + moveInput, Color.black);

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
                }
                else // add additional force to our current velocity
                {
                    velocityChange = moveInput;
                }
            }

            else if (velocity != Vector3.zero) // if no input, but we're still moving
            {
                //try to stop
                //            Vector3 stopDir = 2.0f * transform.position - velocity;
                //            velocityChange = stopDir * tempSlowDown;
                velocityChange = Vector3.down * SlowDown;
            }
            
            if (IsGrounded)
            {
                //velocityChange += Vector3.down * .05f;//SlowDown;
            }
            
            //            GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.Force);
            message += string.Format("moveInput: {0} VelocityChange {1} Velocity {2}", moveInput, velocityChange, GetComponent<Rigidbody>().velocity);
            //message += "\ntargetVelocity: " + moveInput.ToString() + " velocityChange: " + velocityChange.ToString();
            Debug.DrawLine(drawPosition2, drawPosition2 + velocityChange, Color.cyan);
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
