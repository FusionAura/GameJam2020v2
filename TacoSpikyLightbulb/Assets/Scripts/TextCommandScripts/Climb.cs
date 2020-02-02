using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TextAdventure/InputActions/Climb")]
public class Climb : InputAction
{
    public override void RespondToInput(GameController controller, string[] seperatedInputWords)
    {
        if (controller.Player.GetComponent<PlayerBehaviour>().alive == true)
        {
            if (seperatedInputWords.Length > 1)
            {
                if (controller.ladderstate == false)
                {
                    controller.ladderstate = true;
                    controller.Player.GetComponent<PlayerBehaviour>().Destination = controller.LadderDestination.transform;
                    controller.Player.GetComponent<PlayerBehaviour>().MoveToTarget = true;
                    if (controller.roomNavigation.CurrentRoom == controller.lightlocation)
                    {
                        controller.Player.GetComponent<Hero>().AddTimeout(() =>
                        {

                            controller.Player.GetComponent<Hero>().ChangeLightbulb();
                        }, 2f);
                    }
                }
                else
                {
                    controller.Player.GetComponent<PlayerBehaviour>().Destination = controller.LadderDestinationBottom.transform;
                    controller.Player.GetComponent<PlayerBehaviour>().MoveToTarget = true;
                    controller.ladderstate = false;
                }
            }
            else
            {
                controller.actionlog.Insert(0, seperatedInputWords[0] + " " + "where?" + "\n");
            }
        }
    }
}
