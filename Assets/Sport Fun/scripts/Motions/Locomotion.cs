using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SportFun.Motions
{

    [CreateAssetMenu(menuName = "Harko Games/Motions/Locomotion")]
    public class Locomotion : BaseMotion
    {
        private float idleTime = 0f;

        public override void Init(CommotionController cc)
        {
            State = MotionState.Locomotion;
            base.Init(cc);
        }

        public override MotionState ShouldChangeTo(Vector3 move, Vector3 rotation)
        {
            if (move.magnitude <= .01f)
            {
                idleTime += Time.deltaTime;
                if (idleTime > 1f)
                {
                    idleTime = 0f;
                    return MotionState.Idle;
                }
            }
            return base.ShouldChangeTo(move, rotation);
        }

        public override bool CanStart()
        {
            if (myController.myMotor.IsGrounded)
            {
                return true;
            }
            return false;
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void Move(Vector3 move, Vector3 rotation)
        {
            myController.myMotor.Move(move, rotation);
        }
    }
}