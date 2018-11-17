using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SportFun.Motions
{

    [CreateAssetMenu(menuName = "Harko Games/Motions/Jump")]
    public class Jump : BaseMotion
    {
        private float jumpTime = 0f;
        private float fallTime = 0f;
        private float recoveryTime = 0f;
        public float recoveryInterval = 2f;
        private bool leftGround;
        private bool jumpComplete;
        private bool falling;

        public override void Init(CommotionController cc)
        {
            State = MotionState.Jump;
            base.Init(cc);
        }

        public override MotionState ShouldChangeTo(Vector3 move, Vector3 rotation)
        {
            if (jumpComplete)
            {
                return MotionState.Locomotion;
            }
            else if (falling)
            {
                return MotionState.Fall;
            }
            return base.ShouldChangeTo(move, rotation);
        }

        public override bool CanStart()
        {
            return true;
            /*
            if (myController.myMotor.IsGrounded)
            {
                return true;
            }
            return false;
            */
        }

        public override void Start()
        {
            jumpComplete = leftGround = falling = false;
            jumpTime = fallTime = 0f;
            SetSubState(MotionSubState.JumpingUp);
            myController.myMotor.Jump();
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void Move(Vector3 move, Vector3 rotation)
        {
            move.Set(0, 0, 0);
            myController.myMotor.Move(move, rotation, true);
        }

        public override void Update()
        {
            var grounded = myController.myMotor.IsGrounded;
            if (!leftGround && !grounded)
            {
                leftGround = true;
            }
            var upVel = myController.myMotor.Velocity.y;
            jumpTime += Time.deltaTime;
            switch (currentSubState)
            {
                case MotionSubState.JumpingUp:
                case MotionSubState._UNKNOWN: // started jumping
                    if (upVel < .025f)
                    {
                        SetSubState(MotionSubState.JumpingMidair);
                    }
                    else if (upVel < .2f) // fix this for gravity direction changes
                    {
                        myController.myAnim.SetTrigger("ChangeMotionSubState"); // tweak the anim a little without changing sub states
                    }
                    break;
                case MotionSubState.JumpingMidair: // midair
                    if (upVel > 0) // fix this for gravity direction changes
                    {
                        jumpTime += Time.deltaTime;
                    }
                    else
                    {
                        fallTime += Time.deltaTime;
                    }

                    if (fallTime > 1)
                    {
                        falling = true;
                    }
                    break;
                case MotionSubState.Recovery:
                    recoveryTime += Time.deltaTime; // vary recovery interval by falltime
                    if (recoveryTime >= recoveryInterval)
                    {
                        jumpComplete = true;
                        SetSubState(MotionSubState._UNKNOWN);
                    }
                    break;
            }
            if ((currentSubState != MotionSubState.Recovery) && leftGround && grounded)
            {
                recoveryTime = 0f;
                SetSubState(MotionSubState.Recovery);
            }
            base.Update();
        }
    }
}