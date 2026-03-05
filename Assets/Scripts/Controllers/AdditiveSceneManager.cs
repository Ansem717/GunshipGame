//---------------------------------------------------------
// file:	AdditiveSceneManager.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	A scene manager to seamlessly transition between scenes.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneManager : MonoBehaviour {
    public static AdditiveSceneManager Singleton;

    void Awake() {
        if (Singleton != null) {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        DontDestroyOnLoad(Camera.main);
        if (TryGetComponent(out ActionList actionList)) actionList.PushFront(new A_LoadScene("Showcase"));
    }

    public void SwapToSceneShowcase() {

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Showcase"));

        SceneManager.GetSceneByName("Showcase").GetRootGameObjects()[0].SetActive(true);

        //Move the background to the left to simulate camera movement.
        GameObject bg = GameObject.Find("MM_Background");
        if (bg.TryGetComponent(out ActionList backgroundAL)) {

            Vector3 bgPos = bg.transform.position;
            bgPos.x -= 100;

            backgroundAL.PushBack(new A_MoveToVector(
                destination: bgPos,
                _easing: EaseType.EaseInOut,
                _duration: 1.75f
            ));
        }

        //also move the main menu Screen.width to the left.
        GameObject mMenu = GameObject.Find("MainMenu");
        if (mMenu != null) {
            if (mMenu.transform.GetChild(0).TryGetComponent(out ActionList mMenuAL)) {
                Vector3 menuPos = mMenuAL.transform.position;
                menuPos.x -= 100;

                mMenuAL.PushBack(new A_MoveToVector(
                    destination: menuPos,
                    _easing: EaseType.EaseInOut,
                    _duration: 1.75f
                ));
            }
        }
    }

}
