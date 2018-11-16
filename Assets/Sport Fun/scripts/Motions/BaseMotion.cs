using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SportFun;

namespace SportFun.Motions
{
    public abstract class BaseMotion : ScriptableObject
    {
        protected CommotionController myController;
        protected bool enabled = false;

        #region metadata stuff
        public string Name;
        public MotionState State;
        public RuntimeAnimatorController motionAnimator;
        private bool usingCustomAnimator = false;
        private RuntimeAnimatorController originalAnimator;
        public MotionStateHitboxDefinition[] HitboxDefinitions;
        #endregion

        #region substate changes
        protected MotionSubState currentSubState;
        protected void SetSubState(MotionSubState s)
        {
            currentSubState = s;
            myController.myAnim.SetInteger("MotionSubState", (int)s);
            if (s != MotionSubState._UNKNOWN)
            {
                myController.myAnim.SetTrigger("ChangeMotionSubState");
            }
        }
        #endregion

        #region Action changes
        protected bool canComboAction = false;
        public void SetCanCombo(bool can)
        {
            canComboAction = can;
        }
        protected int currentAction = 0;
        protected bool currentActionRunning = false;
        public virtual void ActionStateUpdate(float normalizedTime, bool isTransitioningOut)
        {
            //currentActionRunning = normalizedTime > 0f && !isTransitioningOut; // care about transitioning?
        }

        public bool ActionIdle = true;

        public virtual void InitState(ActionStateBehavior b)
        {
            b.Init(currentAction);
        }

        public virtual void CurrentActionStarted(int a)
        {
            Debug.Log("Action Started");
            currentActionRunning = true;
        }
        public virtual void CurrentActionStopped(int a)
        {
            Debug.Log("Action Stopping");
            SetAction(0);
            currentActionRunning = false;
            Debug.Log("Action Stopped");
        }
        protected void SetAction(int a, bool update = false)
        {
            currentAction = a;
            myController.myAnim.SetInteger("MotionAction", a);
            if (update)
            {
                myController.myAnim.SetTrigger("ChangeMotionAction");
            }
            else
            {
                myController.myAnim.ResetTrigger("ChangeMotionAction");
            }

        }
        #endregion

        public virtual void DrawGizmos()
        {

        }

        public virtual void Init(CommotionController cc)
        {
            myController = cc;
        }

        public virtual MotionState ShouldChangeTo(Vector3 move, Vector3 rotation)
        {
            return MotionState._UNKNOWN;
        }

        public virtual bool CanStart()
        {
            return false;
        }

        public virtual void Start()
        {
            if (motionAnimator != null)
            {
                originalAnimator = myController.myAnim.runtimeAnimatorController;
                myController.myAnim.runtimeAnimatorController = motionAnimator;
                usingCustomAnimator = true;
            }
            enabled = true;
        }

        public virtual void Stop()
        {
            enabled = false;
            if (usingCustomAnimator)
            {
                myController.myAnim.runtimeAnimatorController = originalAnimator;
                usingCustomAnimator = false;
            }
        }

        public virtual void Move(Vector3 move, Vector3 rotation)
        {

        }

        public virtual void Update()
        {

        }


    }

    [System.Serializable]
    public class MotionStateHitboxDefinition
    {
        public Vector3 HitboxScale;
        public Vector3 HitboxRotation;
    }
}
