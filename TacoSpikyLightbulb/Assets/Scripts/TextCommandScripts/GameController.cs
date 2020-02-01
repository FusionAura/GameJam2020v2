﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameController : MonoBehaviour
{
    [HideInInspector]
    public RoomNavigation roomNavigation;
    public InputField inputfield;
    public Text displayText;
    public GameObject Player;
    [HideInInspector]
    public List<string> InteractionDescriptionInRoom = new List<string>();

    [HideInInspector]
    public InteractableItems interactableItems;

    public InputAction[] inputActions;

    public List<string> actionlog = new List<string>();
    public int historyVal = 1;

    void Awake()
    {
        roomNavigation = GetComponent<RoomNavigation>();
        interactableItems = GetComponent<InteractableItems>();
    }
    private void Start()
    {
        DisplayRoomText();
        DisplayLoggedtext();
    }

    void Update()
    {
        if (actionlog.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && historyVal < actionlog.Count-1)
            {
                historyVal += 1;
                string previous = actionlog[historyVal];
                

                displayText.text = previous;
                inputfield.ActivateInputField();
                inputfield.text = null;
                
            }
            if (Input.GetKeyDown(KeyCode.DownArrow)&& historyVal > 0)
            {
                historyVal -= 1;
                string previous = actionlog[historyVal];
                

                displayText.text = previous;
                inputfield.ActivateInputField();
                inputfield.text = null;
            }
        }
    }

    private void UnPackRoom()
    {
        roomNavigation.UnPackExitsInRoom();
        PrepareObjectsToTakeOrExamine(roomNavigation.CurrentRoom);
    }

    private void PrepareObjectsToTakeOrExamine(Room currentRoom)
    {
        for (int i = 0; i < currentRoom.interactableObjectsInRoom.Length; i++)
        {
            string descriptionNotInverntory = interactableItems.GetObjectsNotInInventory(currentRoom, i);
            if (descriptionNotInverntory != null)
            {
                InteractionDescriptionInRoom.Add(descriptionNotInverntory);
            }
            InteractableObject interactableInRoom = currentRoom.interactableObjectsInRoom[i];
            for (int j = 0; j < interactableInRoom.interactions.Length; j++)
            {
                Interaction interaction = interactableInRoom.interactions[j];
                foreach (string a in interaction.InputAction.keyword)
                {
                    if (a == "examine")
                    {
                        interactableItems.ExamineDictionary.Add(interactableInRoom.noun, interaction.textResponse);
                    }
                    if (a == "take")
                    {
                        interactableItems.TakeDictionary.Add(interactableInRoom.noun, interaction.textResponse);
                    }
                }
            }
        }
        
    }

    public string TestVerbDictionaryWithNoun(Dictionary<string,string> VerbDictionary,string verb, string noun)
    {
        if (VerbDictionary.ContainsKey(noun))
        {
            return VerbDictionary[noun];
        }
        return "You Can't " + verb + " " + noun;
    }

    void ClearCollectionForNewRoom()
    {
        interactableItems.ClearCollections();
        InteractionDescriptionInRoom.Clear();
        roomNavigation.ClearExits();
    }

    public void DisplayRoomText()
    {
        ClearCollectionForNewRoom();

        UnPackRoom();
        string JoinedInteractionDescription = string.Join("\n", InteractionDescriptionInRoom.ToArray());
        string combinedText = roomNavigation.CurrentRoom.description + JoinedInteractionDescription;
        LogStringWithReturn(combinedText);
    }

    public void DisplayLoggedtext()
    {
        string logAsText = string.Join("\n", actionlog.ToArray());
        displayText.text = logAsText;
    }
    public void LogStringWithReturn(string _stringToAdd)
    {
        actionlog.Insert(0,_stringToAdd + "\n");
    }

}
