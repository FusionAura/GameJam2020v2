using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextInput : MonoBehaviour
{
    public InputField inputfield;
    private GameController controller;

    private void Awake()
    {
        controller = GetComponent<GameController>();
        inputfield.onEndEdit.AddListener(AcceptStringInput);
    }

    void AcceptStringInput(string UserInput)
    {
        UserInput = UserInput.ToLower();
        controller.LogStringWithReturn(UserInput);

        char[] delimitercharacters = { ' ' };
        string[] seperatedInputWords = UserInput.Split(delimitercharacters);

        for (int i = 0; i < controller.inputActions.Length; i++)
        {
            InputAction inputAction = controller.inputActions[i];
            foreach (string a in inputAction.keyword)
            {
                if (a == seperatedInputWords[0])
                {
                    inputAction.RespondToInput(controller, seperatedInputWords);
                }
            }
        }

        InputComplete();
        
    }

    void InputComplete()
    {
        controller.DisplayLoggedtext();
        inputfield.ActivateInputField();
        inputfield.text = null;
    }

}
