using System;
using UnityEngine;

/// <summary>
/// Attached to the missle, this class controls the physics of the missle.
/// - The Missle Prefab predefines physics settings
/// - The Missle will be "dropped", a small 0.5 second arming time, then a radial pulse will lock onto a target
/// - The Missle will have a burst force, and will rotate to face the target 
/// - The Missle will self-destruct if it stops moving
/// - The radial pusle has a range limit, and the missle self destructs after 3 scans
/// </summary>
public class MissleController : MonoBehaviour {

    //This is attached to the missle object itself
    //Physics Data is not loaded through a SO, it's provided from the physics child
    private CustomPhysics physics;

    private ActionList actionList;

    public float ArmingTime = 0.5f;

    void Start() {
        physics = GetComponentInChildren<CustomPhysics>();

        //Push actions into action list:
        // -- Top --
        //  * Searching (Blocking - looks for target or 3 pulses) (Delay: ArmingTime)
        //  * Seeking (Blocking - contacts with target or velocity hits 0)
        //  * Destroy (destroys the game object)
        // -- End --
        //

        if (TryGetComponent(out actionList)) {
        //    actionList.PushBack(searchingAI);
        //    actionList.PushBack(seekingAI);
        //    actionList.PushBack(destroyAI);
        }
    }


}
