using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SportFun.Motions
{

    [CreateAssetMenu(menuName = "Harko Games/Motions/Idle")]
    public class Idle : BaseMotion
    {
        public override void Init(CommotionController cc)
        {
            State = MotionState.Idle;
            base.Init(cc);
        }

        public override MotionState ShouldChangeTo(Vector3 move, Vector3 rotation)
        {
            if (move.magnitude > .01f)
            {
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
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void Move(Vector3 move, Vector3 rotation)
        {
//            myController.myMotor.Move(move, rotation);
        }
    }
}