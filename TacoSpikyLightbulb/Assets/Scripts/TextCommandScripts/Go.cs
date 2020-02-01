using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TextAdventure/InputActions/Go")]
public class Go : InputAction
{
    public override void RespondToInput(GameController controller, string[] seperatedInputWords)
    {
        if (controller.Player.GetComponent<PlayerBehaviour>().alive == true)
        {
            if (seperatedInputWords.Length > 1)
            {
                controller.roomNavigation.AttemptToChangeRooms(seperatedInputWords[1], seperatedInputWords[0]);
            }
            else
            {
                controller.actionlog.Insert(0, seperatedInputWords[0] + " " + "where?" + "\n");
            }
        }
    }
}
