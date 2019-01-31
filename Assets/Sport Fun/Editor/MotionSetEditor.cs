using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SportFun.Motions
{
    [CustomEditor(typeof(MotionSet))]
    public class MotionSetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            MotionSet myScript = (MotionSet)target;
            if (GUILayout.Button("Generate Animator"))
            {
                myScript.GenerateAnimator();
            }
        }
    }
}
