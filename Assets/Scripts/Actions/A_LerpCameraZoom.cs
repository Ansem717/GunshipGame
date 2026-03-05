//---------------------------------------------------------
// file:	A_LerpCameraZoom.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	A script to change the camera's ortho zoom when Debug is showing
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

internal class A_LerpCameraZoom : ActionInterface {
    private float fromZoom;
    private float toZoom;
    private Camera cam;

    public A_LerpCameraZoom(float targetZoom, EaseType _easing = EaseType.None, float _duration = DefaultDuration)
        : base(_easing: _easing, _duration: _duration) {
        toZoom = targetZoom;
        name = "CamZoom";
    }

    public override bool Init() {
        cam = Camera.main;
        if (cam == null || !cam.orthographic) return false;

        fromZoom = cam.orthographicSize;
        return true;
    }

    public override void PostWait() { }


    public override void IUpdate(float dt) {
        if (duration <= 0f) {
            cam.orthographicSize = toZoom;
            State = ActionState.Done;
            return;
        }

        if (elapsed < duration) {
            float t = GetProgressWithEasing();
            cam.orthographicSize = Mathf.Lerp(fromZoom, toZoom, t);
        } else {
            cam.orthographicSize = toZoom;
            State = ActionState.Done;
        }
    }

    public override void Exit() {
        if (cam != null) {
            cam.orthographicSize = toZoom;
        }
    }
}