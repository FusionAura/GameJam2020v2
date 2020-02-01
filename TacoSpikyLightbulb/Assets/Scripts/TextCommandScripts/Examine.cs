using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "TextAdventure/InputActions/Examine")]
public class Examine : InputAction
{
    public override void RespondToInput(GameController controller, string[] seperatedInputWords)
    {
        if (seperatedInputWords.Length > 1)
        {
            controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(controller.interactableItems.ExamineDictionary, seperatedInputWords[0], seperatedInputWords[1]));
        }
        else
        {
            controller.actionlog.Insert(0, seperatedInputWords[0] + " " + "what?" + "\n");
        }
    }
}
