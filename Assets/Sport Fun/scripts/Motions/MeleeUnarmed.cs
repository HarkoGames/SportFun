using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SportFun.Motions
{
    [CreateAssetMenu(menuName = "Harko Games/Motions/Melee Combat")]
    public class MeleeUnarmed : BaseMotion
    {

        private Timer attackIdle = new Timer(1);

        public override void Init(CommotionController cc)
        {
            base.Init(cc);
            State = MotionState.MeleeFist;
            SetSubState(MotionSubState._UNKNOWN);
            SetAction(0);
        }

        public override void DrawGizmos()
        {
            var basePosition = myController.transform.position; // + (Vector3.up * 2.25f);
            Gizmos.color = currentActionRunning ? canComboAction ? Color.yellow : Color.red : Color.green;
            Gizmos.DrawSphere(basePosition,.05f);
            if (currentSubState == MotionSubState.Attack)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(basePosition + (Vector3.left * .25f), new Vector3(.1f, .25f * attackIdle.Progress, .1f));
            }
        }

        public override MotionState ShouldChangeTo(Vector3 move, Vector3 rotation)
        {
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
            currentActionRunning = false;
            var attackBehaviors = myController.myAnim.GetBehaviours<ActionStateBehavior>();
            foreach (var ab in attackBehaviors)
            {
                ab.Setup(this);
            }
            /*
            var idleBehaviors = myController.myAnim.GetBehaviours<ActionIdleStateBehavior>();
            foreach (var aib in idleBehaviors)
            {
                aib.Setup(this);
            }
            */
        }

        public override void Stop()
        {
            base.Stop();
            attackIdle.Stop();
        }

        public override void Move(Vector3 move, Vector3 rotation)
        {
            if (currentSubState == MotionSubState._UNKNOWN)
            {
                myController.myMotor.Move(move, rotation);
            }
            else
            {
                myController.myMotor.Move(Vector3.zero, rotation);
            }
        }
        private Vector3 mHitboxScale = Vector3.zero;
        private void initHitBox(Vector3 scale)
        {
            mHitboxScale = scale;
        }

        public override void CurrentActionStarted(int behaviorActionIndex)
        {
            base.CurrentActionStarted(behaviorActionIndex);
            attackIdle.Stop();
            var index = behaviorActionIndex - 1;
            if (index < HitboxDefinitions.Length)
            {
                initHitBox(HitboxDefinitions[index].HitboxScale);
//                myController.myCombatant.meleeCollider.On(mHitboxScale);
            }
        }

        public override void CurrentActionStopped(int behaviorActionIndex)
        {
            canComboAction = false;
            if (currentAction == behaviorActionIndex) // the behavior exiting is the same one as the combo level
            {
                base.CurrentActionStopped(behaviorActionIndex);
                attackIdle.Start();
                initHitBox(Vector3.zero);
//                myController.myCombatant.meleeCollider.Off();
            }
        }

        private int maxCombo = 3;
        private bool comboActive = false;
        private void UpdateAttack()
        {
            if (myController.GetActionKey(ActionKeys.Attack))
            {
                if (!currentActionRunning) // can't attack if we already are, unless comboing
                {
                    Debug.Log("Attack clicked");
                    SetSubState(MotionSubState.Attack);
                    SetAction(1, true);
                }
                else if (canComboAction && currentAction < maxCombo)
                {
                    SetAction(currentAction + 1);
                    Debug.Log("Attack click combo.");
                }
                else
                {
                    //attackIdle.Start(); // make sure this keeps running if ignored attack clicks come in
                    Debug.Log("Attack click ignored.");
                }
            }

            if (attackIdle.Update(Time.deltaTime))
            {
                SetSubState(MotionSubState._UNKNOWN);
                attackIdle.Stop();
            }
        }

        public override void ActionStateUpdate(float normalizedTime, bool isTransitioningOut)
        {
            base.ActionStateUpdate(normalizedTime, isTransitioningOut);
            var comboStart = 0f;
            var comboEnd = 1f;
            SetCanCombo((normalizedTime >= comboStart && normalizedTime <= comboEnd) && !isTransitioningOut);
        }

        public override void Update()
        {
            UpdateAttack();
        }
    }
}