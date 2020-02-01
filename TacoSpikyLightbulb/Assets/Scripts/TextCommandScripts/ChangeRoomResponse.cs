using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TextAdventure/ActionResponse/ChangeRoom")]
public class ChangeRoomResponse : ActionResponse
{
    public Room RoomToChangeTo;
    public override bool DoActionResponse(GameController controller)
    {
        if (controller.roomNavigation.CurrentRoom.roomname == requiredString)
        {
            controller.roomNavigation.CurrentRoom = RoomToChangeTo;
            controller.DisplayRoomText();
            return true;
        }
        return false;
    }
}
