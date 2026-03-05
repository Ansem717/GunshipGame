//---------------------------------------------------------
// file:	A_MoveInDirection.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action to manipulation the movement of an object.
//          Movement is linear. 
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_MoveInDirection : A_LerpVector3 {

    private Vector3 rD;

    public A_MoveInDirection(Vector3 relativeDirection, float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null)
        : base(Vector3.zero, _speed, _easing, _duration, _delay, _blocking) {
        name = "MoveInDir";
        rD = relativeDirection;
    }

    public override bool Init() {
        Vector3 p = GetCurrent();
        To = p + rD;
        return base.Init();
    }

    protected override Vector3 GetCurrent() {
        return Owner.transform.position;
    }

    protected override void SetCurrent(Vector3 val) {
        Owner.transform.position = val;
    }

}
