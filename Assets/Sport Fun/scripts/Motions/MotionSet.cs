using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SportFun.Motions
{
    [CreateAssetMenu(menuName = "Harko Games/Motion Set Definition")]
    public class MotionSet : ScriptableObject
    {
        public string Name;

        public int Version;

        private string AnimatorPath;

        public BaseMotion[] motions;

        public AnimatorController myAnimController;

        public string GetAnimatorKeyname()
        {
            var fmt = "commotion-set-controller-{0}-{1}.controller";
            return string.Format(fmt, Name, Version);
        }

        public void GenerateAnimator()
        {
            var path = AssetDatabase.GetAssetPath(this);
            path = path.Replace(this.name + ".asset", "");
            var animatorName = GetAnimatorKeyname();

            path = "Assets/Test";

            var controllerPathGuids = AssetDatabase.FindAssets(animatorName, new string[] { path });

            foreach (var guid in controllerPathGuids)
            {
                Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            }
            //var animPathFmt = "{0}/Controllers/{1}";
            var animPathFmt = "{0}/{1}";
            // Creates the controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(string.Format(animPathFmt, path, animatorName));
            if (controller == null)
            {
                //            var controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/Test/test.controller");
                controller = AnimatorController.CreateAnimatorControllerAtPath(string.Format(animPathFmt, path, animatorName));

                // Add parameters
                controller.AddParameter("xSpeed", AnimatorControllerParameterType.Float);
                controller.AddParameter("zSpeed", AnimatorControllerParameterType.Float);
                controller.AddParameter("vMagnitude", AnimatorControllerParameterType.Float);
                controller.AddParameter("FacingDifference", AnimatorControllerParameterType.Float);

                controller.AddParameter("MotionState", AnimatorControllerParameterType.Int);
                controller.AddParameter("MotionSubState", AnimatorControllerParameterType.Int);
                controller.AddParameter("MotionAction", AnimatorControllerParameterType.Int);

                controller.AddParameter("ChangeMotionState", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("ChangeMotionSubState", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("ChangeMotionAction", AnimatorControllerParameterType.Trigger);

                controller.AddParameter("Exit", AnimatorControllerParameterType.Trigger);

                // Add motion state machines
                var rootStateMachine = controller.layers[0].stateMachine;

                int h = 50, w = 200, startC = 300;
                int r = (h * -2), c = startC, maxC = 2, countC = 0;

                foreach (var m in motions)
                {
                    var pos = new Vector3(c, r, 0);
                    // get state machine from sub animator
                    var motionSm = m.motionAnimator.layers[0].stateMachine;
                    rootStateMachine.AddStateMachine(motionSm, pos);

                    c += w;
                    countC++;
                    if (countC >= maxC)
                    {
                        c = startC;
                        r += h;
                    }

                    // add entry transition
                    var enterTransition = rootStateMachine.AddAnyStateTransition(motionSm);
                    enterTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, (int)m.State, "MotionState");
                    enterTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 1, "ChangeMotionState");
                    if (m.State == MotionState.Idle)
                    {
                        rootStateMachine.defaultState = motionSm.defaultState;
                    }
                }
            }
            myAnimController = controller;
        }
    }
}
