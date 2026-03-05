using UnityEngine;
using UnityEngine.SceneManagement;

public class A_LoadScene : ActionInterface {

    public string SceneName;

    private AsyncOperation loading;

    public A_LoadScene(string sceneName, float delay = 0f) : base(_blocking: true, _delay: delay) {
        name = $"LoadScene({sceneName})";
        SceneName = sceneName;
    }

    public override bool Init() {
        return !SceneManager.GetSceneByName(SceneName).isLoaded; //if scene is loaded, return false to exit action early
    }

    public override void PostWait() { }


    public override void IUpdate(float dt) {

        loading ??= SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);

        if (loading.isDone) {
            Debug.Log("...DONE ! Loading Scene...");
            MasterController.Singleton.actionListsDirty = true;
            State = ActionState.Done;
            return;
        }
        Debug.Log("...Loading Scene...");
    }

    public override void Exit() { }

}
