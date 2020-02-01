using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TextAdventure/InputActions/Use")]
public class Use : InputAction
{
    public override void RespondToInput(GameController controller, string[] seperatedInputWords)
    {
        if (seperatedInputWords.Length > 1)
        {
            Debug.Log("here");
            controller.interactableItems.UseItem(seperatedInputWords);
        }
        else
        {
            controller.actionlog.Insert(0, seperatedInputWords[0] + " " + "what?" + "\n");
        }
    }
}
