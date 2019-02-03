using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        public AnimatorController GenerateGenericController(string pathAndName = null)
        {
            if (pathAndName == null)
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (path == "")
                {
                    path = "Assets";
                }
                else if (Path.GetExtension(path) != "")
                {
                    path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
                }

                pathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Motion Animator Controller.controller");

            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(pathAndName);

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

            return controller;
        }

        public void GenerateAnimatorFromMotionStates()
        {
            var path = AssetDatabase.GetAssetPath(this);
            path = path.Replace("/" + this.name + ".asset", "");
            var animatorName = GetAnimatorKeyname();

            
            var animPathFmt = "{0}/Controllers/{1}";
            // Creates the controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(string.Format(animPathFmt, path, animatorName));
            if (controller == null)
            {
                controller = GenerateGenericController(string.Format(animPathFmt, path, animatorName));

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
                    motionSm.name = m.State.ToString();

                    c += w;
                    countC++;
                    if (countC >= maxC)
                    {
                        c = startC;
                        r += h;
                    }
                    // add entry transition
                    foreach (AnimatorStateTransition t in motionSm.anyStateTransitions)
                    {
                        var enterTransition = rootStateMachine.AddAnyStateTransition(t.destinationState);
                        enterTransition.conditions = t.conditions;
                        //TODO: probably set other important transition values here
/*
                        enterTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, (int)m.State, "MotionState");
                        enterTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 1, "ChangeMotionState");
                        */
                    }
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
