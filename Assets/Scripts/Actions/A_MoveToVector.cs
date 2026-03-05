//---------------------------------------------------------
// file:	A_MoveToVector.cs
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

public class A_MoveToVector : A_LerpVector3 {

    public A_MoveToVector(Vector3 destination, float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null)
        : base(destination, _speed, _easing, _duration, _delay, _blocking) {
        name = "MoveToVector";
    }

    protected override Vector3 GetCurrent() {
        return Owner.transform.position;
    }

    protected override void SetCurrent(Vector3 val) {
        Owner.transform.position = val;
    }

}
