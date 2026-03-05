using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButton : ActionList, IPointerEnterHandler, IPointerExitHandler {
    private Button button;
    private Image image;

    public Color hoverColor = Color.white;
    public Color normalColor = Color.white;
    public Color disabledColor = new Color(0.2f, 0.2f, 0.2f);

    private bool wasInteractable;
    private bool initialized = true;

    private float changeDuration = 0.25f;

    void Awake() {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        wasInteractable = button.interactable;
        if (initialized && !wasInteractable) image.color = disabledColor;
    }

    public void DelayInitialization() {
        initialized = false;
    }

    public void Initialize() {
        initialized = true;
        // Force re-evaluation of interactable state
        if (!button.interactable && !wasInteractable) {
            PushBack(new A_ColorShift(targetColor: disabledColor, _easing: EaseType.None, _duration: changeDuration));
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!button.interactable) return;
        if (!initialized) return;
        PushBack(new A_ColorShift(targetColor: hoverColor, _easing: EaseType.None, _duration: changeDuration));
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!button.interactable) return;
        if (!initialized) return;
        PushBack(new A_ColorShift(targetColor: normalColor, _easing: EaseType.None, _duration: changeDuration));
    }

    protected override void Update() {

        base.Update();

        if (!initialized) return;

        if (!button.interactable) {
            if (wasInteractable) {
                //It WAS interactable but no longer is interactable.
                PushBack(new A_ColorShift(targetColor: disabledColor, _easing: EaseType.None, _duration: changeDuration));
                wasInteractable = false;
            }
            return;
        }

        if (wasInteractable == false) {
            //It IS interactable now but it WAS NOT interactable
            PushBack(new A_ColorShift(targetColor: normalColor, _easing: EaseType.None, _duration: changeDuration));
            wasInteractable = true;
        }


    }
}
