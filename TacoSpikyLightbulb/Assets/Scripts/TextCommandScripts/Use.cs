using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TextAdventure/InputActions/Use")]
public class Use : InputAction
{
    public override void RespondToInput(GameController controller, string[] seperatedInputWords)
    {
        Room rmName1 = Resources.Load("Rooms/1") as Room;
        Room rmName2 = Resources.Load("Rooms/2") as Room;
        Room rmName3 = Resources.Load("Rooms/3") as Room;
        Room rmName4 = Resources.Load("Rooms/4") as Room;
        Room rmName5 = Resources.Load("Rooms/5") as Room;
        Room rmName6 = Resources.Load("Rooms/6") as Room;
        Room rmName7 = Resources.Load("Rooms/7") as Room;
        if (controller.Player.GetComponent<PlayerBehaviour>().alive == true)
        {
            if (seperatedInputWords.Length > 1)
            {
                Debug.Log("here");
                //
                switch (seperatedInputWords[1])
                {
                    case "beer":
                        {
                            if (controller.Beers > 0)
                            {
                                controller.standardDrinks += 1;
                                controller.Beers -= 1;
                                controller.actionlog.Insert(0, "Drunk a bottle of beer." + "\n");
                                if (controller.standardDrinks > 5)
                                {

                                }
                            }
                            else
                            {
                                controller.interactableItems.UseItem(seperatedInputWords);
                            }
                            break;
                        }
                    case "broom":
                        {

                            if (controller.GotBroom == true && controller.GetComponent<RoomNavigation>().CurrentRoom == rmName3)
                            {
                                // Temp. Set the position to be directly under the light.
                                var lightbulb = GameObject.Find("obj_lightbulb");
                                Vector3 underTheLightbulb = new Vector3(lightbulb.transform.position.x, controller.Player.GetComponent<Hero>().transform.position.y, lightbulb.transform.position.z);
                                controller.Player.GetComponent<Hero>().transform.position = underTheLightbulb;

                                controller.Player.GetComponent<Hero>().PlayAnimation("hit-up", () =>
                                {
                                    lightbulb.GetComponent<Rigidbody>().useGravity = true;
                                }, -0.2f);

                            }
                            break;
                        }
                    case "gloves":
                        {
                            break;
                        }
                    case "ladder":
                        {
                            Debug.Log("here");

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
}
