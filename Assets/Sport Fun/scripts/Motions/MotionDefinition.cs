﻿using SportFun.Motions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Harko Games/MotionDefinition")]
public class MotionDefinition : ScriptableObject {
    public RuntimeAnimatorController animator;
    public BaseMotion motion;
    public MotionStateHitboxDefinition[] HitboxDefinitions;
}

