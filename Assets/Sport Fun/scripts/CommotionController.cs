using SportFun.Motions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SportFun
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterMotor))]
    public class CommotionController : MonoBehaviour
    {

        private Dictionary<int, bool> mActionKeys;

        public void ClearActionKeys()
        {
            foreach (var ak in mActionKeys.Keys)
            {
                mActionKeys[ak] = false;
            }
            myMotor.ClearActionKeys();
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
            myMotor.SetActionKey(ak, val);
        }

        public Animator myAnim;
        public CharacterMotor myMotor { get; set; }
        public MotionSet MotionSet;
//        public BaseMotion[] Motions;
        private Dictionary<MotionState, BaseMotion> availableStates;
        private BaseMotion currentMotion;
        [HideInInspector]
        public MotionState currentState;
        /// <summary>
        /// optional state to return to once another state has finished (like jump, fall, etc)
        /// not really implemented yet
        /// </summary>
        private MotionState returnState;

        public float turnSpeed = 180 / 60; //10f;
        public float walkSpeed = 2f;
        public float runSpeed = 5f;
        public float crouchSpeed = 1f;
        public float slowDown = 1f;
        public float jumpPower = 5f;

        private void Awake()
        {
            mActionKeys = new Dictionary<int, bool>();
            availableStates = new Dictionary<MotionState, BaseMotion>();
            myAnim = GetComponent<Animator>();
            if (myAnim == null)
                myAnim = GetComponentInChildren<Animator>();
            myMotor = GetComponent<CharacterMotor>();
            myMotor.Init(new CharacterMotorSettings()
            {
                TurnSpeed = turnSpeed,
                WalkSpeed = walkSpeed,
                RunSpeed = runSpeed,
                CrouchSpeed = crouchSpeed,
                SlowDown = slowDown,
                JumpPower = jumpPower
            });
            RegisterMotions();
            DefaultToState();
        }

        private void SetState(MotionState s)
        {
            if (availableStates.ContainsKey(s))
            {
                if (availableStates[s].CanStart())
                {
                    StartCoroutine(TransitionToState(s));
                }
            }
            else
            {
                Debug.LogWarningFormat("No motion availble for state {0}", s);
            }
        }

        private IEnumerator TransitionToState(MotionState s)
        {
            if (currentMotion != null)
            {
                myAnim.SetTrigger("Exit");
                /*
                while (myAnim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 < 0.95f)// myAnim.GetBool("Exit"))
                {
                    yield return null;
                }
                */
                currentMotion.Stop();
            }
            currentMotion = availableStates[s];
            currentMotion.Start();
            currentState = s;
            myAnim.SetInteger("MotionState", (int)s);
            myAnim.SetTrigger("ChangeMotionState");
            yield return null;
        }

        private void RegisterMotions()
        {
            /*
            var idle = new Idle();
            idle.Init(this);
            availableStates.Add(MotionState.Idle, idle);
            var loco = new Locomotion();
            loco.Init(this);
            availableStates.Add(MotionState.Locomotion, loco);
            var meleeUnarmed = new MeleeUnarmed();
            meleeUnarmed.Init(this);
            availableStates.Add(MotionState.MeleeFist, meleeUnarmed);
            */
            myAnim.runtimeAnimatorController = MotionSet.myAnimController;
            foreach (BaseMotion b in MotionSet.motions)
            {
                if (!availableStates.ContainsKey(b.State))
                {
                    var instance = Object.Instantiate(b) as BaseMotion;
                    availableStates.Add(b.State, instance);
                    instance.Init(this);
                }
                else
                {
                    Debug.LogWarningFormat("Motion {0} not added, because a motion already exists for state {1}", b.Name, b.State);
                }
            }
            
        }

        private void DefaultToState()
        {
            if (currentState == MotionState._UNKNOWN)
            {
                if (availableStates.ContainsKey(MotionState.Idle)) SetState(MotionState.Idle);
                else if (availableStates.ContainsKey(MotionState.Locomotion)) SetState(MotionState.Locomotion);
            }
        }

        private void OnDrawGizmos()
        {
            if (currentMotion != null)
            {
                currentMotion.DrawGizmos();
            }
        }

        public void Move(Vector3 move, Vector3 rotation)
        {
            if (currentMotion != null)
            {
                var newMotion = currentMotion.ShouldChangeTo(move, rotation);
                if (newMotion != MotionState._UNKNOWN)
                {
                    SetState(newMotion);
                }
                else
                {
                    currentMotion.Move(move, rotation);
                }
            }
        }

        private void Update()
        {
            if (GetActionKey(ActionKeys.Fight))
            {
                if (currentState != MotionState.MeleeFist) SetState(MotionState.MeleeFist);
                else SetState(MotionState.Locomotion);
            }
            else if (GetActionKey(ActionKeys.Jump))
            {
                if (currentState != MotionState.Jump) SetState(MotionState.Jump);
            }

            if (!myMotor.IsGrounded && 
                (currentState != MotionState.Fall && currentState != MotionState.Jump))
            {
                SetState(MotionState.Fall);
            }

            if (currentMotion != null)
            {
                currentMotion.Update();
            }
        }
    }
}
