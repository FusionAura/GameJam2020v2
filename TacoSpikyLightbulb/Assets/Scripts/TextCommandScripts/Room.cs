using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="TextAdventure/Room")]
public class Room : ScriptableObject
{
    [TextArea]
    public string description; //room description
    public string roomname;
    public Exit[] exits;
    public InteractableObject[] interactableObjectsInRoom;
}
