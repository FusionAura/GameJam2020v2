using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class RoomNavigation : MonoBehaviour
{
    public Room CurrentRoom;
    private GameController controller;

    public GameObject Destination;


    Dictionary<string, Room> exitDictionary = new Dictionary<string, Room>();

    public List<Room> Rooms = new List<Room>();
    public List<GameObject> RoomNodes = new List<GameObject>();
    public Dictionary<Room, GameObject> RoomLocations = new Dictionary<Room, GameObject>();

    private void Awake()
    {
        for (int i = 0; i < Rooms.Capacity; i++)
        {
            RoomLocations.Add(Rooms[i], RoomNodes[i]);
        }
        controller = GetComponent<GameController>();
    }

    public void UnPackExitsInRoom()
    {
        for (int i = 0; i < CurrentRoom.exits.Length; i++)
        {
            exitDictionary.Add(CurrentRoom.exits[i].keyString, CurrentRoom.exits[i].valueRoom);
            controller.InteractionDescriptionInRoom.Add(CurrentRoom.exits[i].exitDescription);
        }
    }

    public void AttemptToChangeRooms(string DirectionNoun)
    {
        if (exitDictionary.ContainsKey(DirectionNoun))
        {
            CurrentRoom = exitDictionary[DirectionNoun];
            controller.LogStringWithReturn("Dubalom heads off to " + DirectionNoun);
            controller.DisplayRoomText();

            Destination = RoomLocations[CurrentRoom].gameObject;
            controller.Player.GetComponent<PlayerBehaviour>().Destination = Destination.transform;
            controller.Player.GetComponent<PlayerBehaviour>().MoveToTarget = true;
            

        }
        else
        {
            controller.LogStringWithReturn("There is no path to the " + DirectionNoun);
        }
    }

    public void ClearExits()
    {
        exitDictionary.Clear();
    }


}
