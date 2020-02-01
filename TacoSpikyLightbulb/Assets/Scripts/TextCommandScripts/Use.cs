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
            //
            switch(seperatedInputWords[1])
            {
                case "beer":
                    {
                        if (controller.Beers > 0)
                        {
                            controller.standardDrinks += 1;
                            controller.Beers -= 1;
                            controller.actionlog.Insert(0, "Drunk a bottle of beer." + "\n");
                        }
                        else
                        {
                            controller.interactableItems.UseItem(seperatedInputWords);
                        }
                        break;
                    }
                default:
                    {
                        controller.interactableItems.UseItem(seperatedInputWords);
                        break;
                    }
            }
        }
        else
        {
            controller.actionlog.Insert(0, seperatedInputWords[0] + " " + "what?" + "\n");
        }
    }
}
