using UnityEngine;
using UnityEngine.SceneManagement;

public class A_UnloadScene : ActionInterface {

    public string SceneName;

    private AsyncOperation unloading;

    public A_UnloadScene(string sceneName) : base() {
        name = $"UnloadScene({sceneName})";
        SceneName = sceneName;
    }

    public override bool Init() {
        if (!SceneManager.GetSceneByName(SceneName).isLoaded) {
            return false; //action is over
        }
        unloading = SceneManager.UnloadSceneAsync(SceneName);
        return true;
    }

    public override void PostWait() { }


    public override void IUpdate(float dt) {

        if (unloading == null) return;

        if (unloading.isDone) {
            State = ActionState.Done;
            Debug.Log("... DONE ! Unloading Scene ...");
            MasterController.Singleton.actionListsDirty = true;
            return;
        }
        Debug.Log("...Unloading Scene...");
    }

    public override void Exit() { }

}
