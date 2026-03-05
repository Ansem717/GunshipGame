//---------------------------------------------------------
// file:	GameMaster.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	The "main" class that runs the game.
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using UnityEngine;

public class GameMaster : MonoBehaviour {

    public enum CardType { FOE, SKILL }
    [Space]
    public CardType type;

    private Deck deckOfCards;
    private ActionList thisList;

    public static int Turn; //a "global" variable, used for autoplay

    private static int _playerIndex; //same
    public static int PlayerIndex {
        get => _playerIndex;
        set {
            _playerIndex = value;

            int minimum = 0;
            int maximum = 3;

            if (_playerIndex > maximum) _playerIndex = minimum;
            if (_playerIndex < minimum) _playerIndex = maximum;
            //if the value ever goes above maximum, it wraps back down to minimum
            //and vise versa. 
        }
    }

    private void Start() {

        Helpers.DisablePlayerHand();

        deckOfCards = FindFirstObjectByType<Deck>();
        if (type == CardType.SKILL) Helpers.GenerateSkillDeck(deckOfCards);
        else Helpers.GenerateFoeDeck(deckOfCards);

        MasterController.Singleton.actionListsDirty = true;

        if (TryGetComponent(out thisList)) {

            Vector3 dst = new Vector3(-20, 0, 0);

            thisList.PushBack(new A_Callback(
                action: new A_MoveToVector(
                    destination: dst,
                    _duration: 0.5f,
                    _delay: 1.0f,
                    _easing: EaseType.EaseOut
                ),
                callback: ContinueAfterMove
            ));
        } else {
            Debug.LogError("Could not get action list from Game Manager");
        }
    }


    void ContinueAfterMove() {

        deckOfCards.Shuffle();

        if (thisList) {
            thisList.PushBack(new A_Callback(
                action: new A_Deal(deck: deckOfCards, dealCount: 8),
                callback: () => PushBackAction(new A_AutoplayRequired())
            ));
        } else {
            Debug.LogError("Could not get action list from Game Manager");
        }
    }

    private void Update() {
    }

    public void PushBackAction(ActionInterface action) {
        if (TryGetComponent(out ActionList al)) {
            al.PushBack(action);
        }
    }

    public void PushFrontAction(ActionInterface action) {
        if (TryGetComponent(out ActionList al)) {
            al.PushFront(action);
        }
    }

    public static bool CheckTrickTake(Zone playArea) => Random.value < 0.1 * playArea.Count();
}
