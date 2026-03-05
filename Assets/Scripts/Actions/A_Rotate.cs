//---------------------------------------------------------
// file:	A_Rotate.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	An action to manipulation the rotation of an object.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class A_Rotate : A_LerpVector3 {

    public A_Rotate(float targetAngle, float? _speed = null, EaseType? _easing = null, float? _duration = null, float? _delay = null, bool? _blocking = null)
        : base(new Vector3(0f, 0f, -targetAngle), _speed, _easing, _duration, _delay, _blocking) {
        name = "Rotate";
    }

    protected override Vector3 GetCurrent() {
        return Owner.transform.rotation.eulerAngles;
    }

    protected override void SetCurrent(Vector3 val) {
        Vector3 current = Owner.transform.rotation.eulerAngles;
        current.z = val.z; // Only modify Z axis
        Owner.transform.rotation = Quaternion.Euler(current);
    }

    protected override Vector3 Interpolate(Vector3 from, Vector3 to, float t) {
        float z = Mathf.LerpAngle(from.z, to.z, t);
        return new Vector3(from.x, from.y, z); //the `from` values are ignored via set current
    }

}
