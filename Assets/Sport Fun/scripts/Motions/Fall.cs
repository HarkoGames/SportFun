using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SportFun.Motions
{

    [CreateAssetMenu(menuName = "Harko Games/Motions/Fall")]
    public class Fall : BaseMotion
    {
        private float fallTime = 0f;
        private float recoveryTime = 0f;
        public float recoveryInterval = 2f;
        private bool fallComplete;

        public override void Init(CommotionController cc)
        {
            State = MotionState.Jump;
            base.Init(cc);
        }

        public override MotionState ShouldChangeTo(Vector3 move, Vector3 rotation)
        {
            if (fallComplete)
            {
                //return base.ShouldChangeTo(move, rotation);
                return MotionState.Locomotion;
            }
            return base.ShouldChangeTo(move, rotation);
        }

        public override bool CanStart()
        {
            return true;
        }

        public override void Start()
        {
            fallComplete = false;
            fallTime = 0f;
            SetSubState(MotionSubState.Falling);
            myController.myMotor.SetCollisionDetectionMode(CollisionDetectionMode.Continuous);
            base.Start();
        }

        public override void Stop()
        {
            myController.myMotor.SetCollisionDetectionMode(CollisionDetectionMode.Discrete);
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
            var upVel = myController.myMotor.Velocity.y;
            fallTime += Time.deltaTime;
            switch (currentSubState)
            {
                case MotionSubState.Falling:
                    fallTime += Time.deltaTime;
                    break;
                case MotionSubState.Recovery:
                    recoveryTime += Time.deltaTime; // vary recovery interval by falltime
                    if (recoveryTime >= recoveryInterval)
                    {
                        fallComplete = true;
                        SetSubState(MotionSubState._UNKNOWN);
                    }
                    break;
            }
            if ((currentSubState != MotionSubState.Recovery) && grounded)
            {
                recoveryTime = 0f;
                SetSubState(MotionSubState.Recovery);
            }
            base.Update();
        }
    }
}