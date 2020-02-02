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
                Dictionary<string, string> takeDictionary = controller.interactableItems.Take(seperatedInputWords);
                if (takeDictionary != null)
                {

                    if (controller.GetComponent<RoomNavigation>().CurrentRoom == controller.ladderlocation && seperatedInputWords[1] == "ladder")
                    {
                        controller.GotLadder = true;
                        controller.LadderPlaced = false;

                        controller.Player.GetComponent<Hero>().PickupGameObject(GameObject.Find("obj_ladder"));
                        controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));

                    }
                    else if (controller.GetComponent<RoomNavigation>().CurrentRoom == controller.broomLocation && seperatedInputWords[1] == "broom")
                    {
                        controller.GotBroom = true;
                        Debug.Log("grab");
                        controller.Player.GetComponent<Hero>().PickupGameObject(GameObject.Find("obj_broom"));
                        controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));
                    }
                    else
                    {
                        switch (seperatedInputWords[1])
                        {
                            case "beer":
                                {
                                    Debug.Log("here4");
                                    controller.Beers += 10;
                                    controller.actionlog.Insert(0, "Found 10 Bottles of Beer." + "\n");

                                    controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));

                                    var fridgeDoor = GameObject.Find("door");
                                    GameObject.Find("obj_fridge").GetComponent<AudioSource>().Play();


                                    controller.Player.GetComponent<Hero>().Interact(() =>
                                    {
                                        controller.Player.GetComponent<Hero>().AddTimeoutOnStep((p) =>
                                        {
                                            fridgeDoor.transform.localRotation = Quaternion.Euler(-90f, p * 135f, 0f);
                                        }, 1f);
                                    });

                                    break;
                                }
                            case "broom":
                                {
                                    if (controller.GotBroom == false)
                                    {
                                        Debug.Log("Test");
                                        controller.GotBroom = true;
                                        controller.Player.GetComponent<Hero>().PickupGameObject(GameObject.Find("obj_broom"));
                                        //controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));
                                        controller.actionlog.Insert(0, "Picked up the Broom");
                                    }

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
                                        controller.Player.GetComponent<Hero>().PickupGameObject(GameObject.Find("obj_ladder"));

                                        controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));

                                        controller.ladderlocation = controller.GetComponent<RoomNavigation>().CurrentRoom;
                                    }
                                    break;
                                }
                            default:
                                {
                                    Debug.Log("default");
                                    controller.LogStringWithReturn(controller.TestVerbDictionaryWithNoun(takeDictionary, seperatedInputWords[0], seperatedInputWords[1]));
                                    break;
                                }
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
