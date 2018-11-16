using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SportFun.Motions
{
    public class ActionStateBehavior : StateMachineBehaviour
    {
        private BaseMotion myMotion;
        private int myActionIndex;
        private bool setup = false;
        private bool initialized = false;
        public void Setup(BaseMotion motion)
        {
            if (setup) return;
            myMotion = motion;
            setup = true;
        }

        public void Init(int actionIndex)
        {
            if (initialized) return;
            myActionIndex = actionIndex;
            initialized = true;
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (!setup) { Debug.LogWarning("Actionstate not set up"); return; }
            if (!initialized) myMotion.InitState(this);
            myMotion.CurrentActionStarted(myActionIndex);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (!setup) { Debug.LogWarning("Actionstate not set up"); return; }
            var isTransitioningOut = stateInfo.normalizedTime > .5f && animator.IsInTransition(layerIndex);
            myMotion.ActionStateUpdate(stateInfo.normalizedTime, isTransitioningOut);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (!setup) { Debug.LogWarning("Actionstate not set up"); return; }
            myMotion.CurrentActionStopped(myActionIndex);
        }

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}
    }
}
