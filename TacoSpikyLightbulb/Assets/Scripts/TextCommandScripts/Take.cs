using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "TextAdventure/InputActions/Take")]
public class Take : InputAction
{
    public override void RespondToInput(GameController controller, string[] seperatedInputWords)
    {
        if (controller.Player.GetComponent<PlayerBehaviour>().alive == true)
        {
            if (seperatedInputWords.Length > 1)
            {
                Debug.Log("here2");
                Dictionary<string, string> takeDictionary = controller.interactableItems.Take(seperatedInputWords);
                if (takeDictionary != null)
                {
                    Debug.Log("here3");
                    switch (seperatedInputWords[1])
                    {
                        case "beer":
                            {
                                Debug.Log("here4");
                                controller.Beers += 10;
                                controller.actionlog.Insert(0, "Found 10 Bottles of Beer." + "\n");

                                controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));

                                controller.Player.GetComponent<Hero>().Interact(() =>
                                {
                                    var fridgeDoor = GameObject.Find("door");

                                    controller.Player.GetComponent<Hero>().AddTimeoutOnStep((p) =>
                                    {
                                        fridgeDoor.transform.localRotation = Quaternion.Euler(-90f, p * 135f, 0f);
                                    }, 1f);
                                });
                                
                            break;
                            }
                        case "broom":
                            {
                                controller.GotBroom = true;
                                controller.Player.GetComponent<Hero>().PickupGameObject(GameObject.Find("obj_broom"));
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
                                if (controller.GotBroom == false && controller.LadderReachable == true)
                                {
                                    controller.GotLadder = true;
                                    controller.Player.GetComponent<Hero>().HitLadder();
                                    controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));
                                }
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
}
