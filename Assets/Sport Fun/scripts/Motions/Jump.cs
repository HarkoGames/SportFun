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

        public override void Init(CommotionController cc)
        {
            jumpTime = fallTime = 0f;
            State = MotionState.Jump;
            base.Init(cc);
        }

        public override MotionState ShouldChangeTo(Vector3 move, Vector3 rotation)
        {
            if (myController.myMotor.IsGrounded && currentSubState == MotionSubState._UNKNOWN)
            {
                return MotionState.Locomotion;
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
            SetSubState(MotionSubState.JumpingUp);
            myController.myMotor.Impulse(Vector3.up * 10);
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void Move(Vector3 move, Vector3 rotation)
        {
            //myController.myMotor.Move(move, rotation);
        }

        public override void Update()
        {
            var upVel = myController.myMotor.Velocity.y;
            switch (currentSubState)
            {
                case MotionSubState._UNKNOWN: // started jumping
                    if (upVel > .1f) // fix this for gravity direction changes
                    {
                        jumpTime += Time.deltaTime;
                    }
                    else
                    {
                        SetSubState(MotionSubState.JumpingUp);
                    }
                    break;
                case MotionSubState.JumpingUp:
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
                        SetSubState(MotionSubState.Falling);
                    }
                    break;
                case MotionSubState.Falling: // falling
                    if (myController.myMotor.IsGrounded)
                    {
                        SetSubState(MotionSubState._UNKNOWN);
                    }
                    break;
            }
            base.Update();
        }
    }
}