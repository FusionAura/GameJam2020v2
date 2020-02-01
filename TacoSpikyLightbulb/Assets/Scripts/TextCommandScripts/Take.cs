using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "TextAdventure/InputActions/Take")]
public class Take : InputAction
{
    public override void RespondToInput(GameController controller, string[] seperatedInputWords)
    {
        if (seperatedInputWords.Length > 1)
        {
            Dictionary<string, string> takeDictionary = controller.interactableItems.Take(seperatedInputWords);
            if (takeDictionary != null)
            {
                switch (seperatedInputWords[1])
                {
                    case "beer":
                        {
                            if (controller.Beers > 0)
                            {
                                controller.Beers += 10;
                                controller.actionlog.Insert(0, "Found 10 Bottles of Beer." + "\n");

                                controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));
                            }
                            break;
                        }
                    case "broom":
                        {
                            controller.GotBroom = true;
                            controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));
                            
                            break;
                        }
                    case "gloves":
                        {
                            controller.GotGlove = true;
                            controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));

                            break;
                        }
                    case "ladder":
                        {
                            controller.GotLadder = true;
                            controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));

                            break;
                        }
                    default:
                        {
                            controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));
                            break;
                        }
                }

            }
        }
        else
        {
            controller.actionlog.Insert(0, seperatedInputWords[0] + " " + "what?" + "\n");
        }

    }
}
