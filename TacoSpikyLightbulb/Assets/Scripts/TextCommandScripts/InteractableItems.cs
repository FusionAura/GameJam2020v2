using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItems : MonoBehaviour
{
    public string FailResponse = "Nothing Happened you fuckin Druggo";
    private GameController controller;
    [HideInInspector]
    public List<string> nounsInroom = new List<string>();
    public List<string> nounsInInventory = new List<string>();
    public Dictionary<string, string> ExamineDictionary = new Dictionary<string, string>();
    public Dictionary<string, string> TakeDictionary = new Dictionary<string, string>();
    public List<InteractableObject> UseableItemList;
    private Dictionary<string, ActionResponse> useDictionary = new Dictionary<string, ActionResponse>();

    private void Awake()
    {
        controller = GetComponent<GameController>();
    }

    public string GetObjectsNotInInventory(Room CurrentRoom,int i)
    {
        InteractableObject interactableInRoom = CurrentRoom.interactableObjectsInRoom[i];
        if (!nounsInInventory.Contains(interactableInRoom.noun))
        {
            nounsInroom.Add(interactableInRoom.noun);
            return interactableInRoom.description;
        }
        else
        {
            return null;
        }
    }

    public void AddActionResponsesToUsedictionary()
    {
        for (int i = 0; i < nounsInInventory.Count; i++)
        {
            string noun = nounsInInventory[i];
            InteractableObject interactableObjectInInventory = GetInteractableObjectFromUsableList(noun);
            if (interactableObjectInInventory == null)
                continue;
            for (int j = 0; j < interactableObjectInInventory.interactions.Length; j++)
            {
                Interaction interaction = interactableObjectInInventory.interactions[j];
                if (interaction.actionResponse == null)
                    continue;
                if (!useDictionary.ContainsKey(noun))
                {
                    useDictionary.Add(noun, interaction.actionResponse);
                }
            }
        }
    }

    public void RemoveActionResponsesToUsedictionary()
    {
        for (int i = 0; i < nounsInInventory.Count; i++)
        {
            string noun = nounsInInventory[i];
            InteractableObject interactableObjectInInventory = GetInteractableObjectFromUsableList(noun);
            if (interactableObjectInInventory == null)
                continue;
            for (int j = 0; j < interactableObjectInInventory.interactions.Length; j++)
            {
                Interaction interaction = interactableObjectInInventory.interactions[j];
                if (interaction.actionResponse == null)
                    continue;
                if (!useDictionary.ContainsKey(noun))
                {
                    useDictionary.Remove(noun);
                }
            }
        }
    }

    public InteractableObject GetInteractableObjectFromUsableList(string noun)
    {
        for (int i = 0; i < UseableItemList.Count; i++)
        {
            if (UseableItemList[i].noun == noun)
            {
                return UseableItemList[i];
            }
        }
        return null;
    }

    public void ClearCollections()
    {
        ExamineDictionary.Clear();
        nounsInroom.Clear();
        TakeDictionary.Clear();
    }
    public Dictionary<string,string>Take(string[] seperatedInputWords)
    {
        string noun = seperatedInputWords[1];

        if (nounsInroom.Contains(noun))
        {
            nounsInInventory.Add(noun);
            AddActionResponsesToUsedictionary();
            nounsInroom.Remove(noun);
            return TakeDictionary;
        }
        else
        {
            
            controller.LogStringWithReturn("There is no "+ noun + " here to take.");
            return null;
            
        }
    }

    public void UseItem(string[] seperatedInputWords)
    {
        string nounToUse = seperatedInputWords[1];

        if (nounsInInventory.Contains(nounToUse))
        {
            if (useDictionary.ContainsKey(nounToUse))
            {

                bool actionResult = useDictionary[nounToUse].DoActionResponse(controller);
                if (!actionResult)
                {
                    controller.LogStringWithReturn(FailResponse);
                }
            }
            else
            {
                controller.LogStringWithReturn("You can't use the " + nounToUse);
            }   
        }
        else
        {
            controller.LogStringWithReturn("There is no " + nounToUse + " in your inventory to use.");
        }
    }

    public void DisplayInventory()
    {
        controller.LogStringWithReturn("You are Holding: ");
        for (int i = 0; i < nounsInInventory.Count; i++)
        {
            controller.LogStringWithReturn(nounsInInventory[i]);
        }
    }
}
