using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TextAdventure/InputActions/Drop")]
public class Drop : InputAction
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
                switch (seperatedInputWords[1])
                {
                    case "beer":
                        {
                            controller.Beers = 0;
                            controller.Player.GetComponent<Hero>().DropCurrentGameObject();
                            break;
                        }
                    case "broom":
                        {
                            
                            controller.GotBroom = false;
                            controller.Player.GetComponent<Hero>().DropCurrentGameObject();
                            controller.broomLocation = controller.GetComponent<RoomNavigation>().CurrentRoom;


                            controller.interactableItems.nounsInroom.Add(seperatedInputWords[1]);
                            controller.interactableItems.nounsInInventory.Remove(seperatedInputWords[1]);
                            controller.interactableItems.RemoveActionResponsesToUsedictionary();
                            
                            break;
                        }
                    case "gloves":
                        {
                            controller.GotGlove = false;
                            break;
                        }
                    case "ladder":
                        {
                            controller.GotLadder = false;
                            controller.LadderPlaced = true;
                            controller.Player.GetComponent<Hero>().PlaceLadder();
                            controller.ladderlocation = controller.GetComponent<RoomNavigation>().CurrentRoom;
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
