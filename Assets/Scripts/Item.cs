using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    private string ItemName;            //Name of item
    private string ItemDescription;     //A description of what the item does
    private Sprite ItemImage;
    private int ItemDuration;           //The amount of turns the effect lasts (0 for instant effect)
    private bool IsUsableOnTeam;        //Will this be casted on an enemy or ally?
    private bool IsHealing;             //Determine if this item has healing capabilities
    Endimon.StatusEffects Effect;       //What effect does this cast on an Endimon

    //Makes an Item
    public Item(string theName, string desc, int turns, bool onTeam, bool healing, Endimon.StatusEffects eff)
    {
        ItemName = theName;
        ItemDescription = desc;
        ItemDuration = turns;
        IsUsableOnTeam = onTeam;
        IsHealing = healing;
        Effect = eff;
        DetermineImage();
    }

    public void DetermineImage()
    {
        if(ItemName == "Power-Up Candy")
        {
            ItemImage = Resources.Load("CandyIcons/AttackUp", typeof(Sprite)) as Sprite;
        }
        else if(ItemName == "Bulk-Up Candy")
        {
            ItemImage = Resources.Load("CandyIcons/BulkUp", typeof(Sprite)) as Sprite;
        }
        else if(ItemName == "Paralyze Candy")
        {
            ItemImage = Resources.Load("CandyIcons/Paralyze", typeof(Sprite)) as Sprite;
        }
        else if (ItemName == "Poison Candy")
        {
            ItemImage = Resources.Load("CandyIcons/Poison", typeof(Sprite)) as Sprite;
        }
        else if (ItemName == "Sleep Candy")
        {
            ItemImage = Resources.Load("CandyIcons/Sleep", typeof(Sprite)) as Sprite;
        }
        else if (ItemName == "Confusion Candy")
        {
            ItemImage = Resources.Load("CandyIcons/Confusion", typeof(Sprite)) as Sprite;
        }
        else if (ItemName == "Berry Candy")
        {
            ItemImage = Resources.Load("CandyIcons/Berry", typeof(Sprite)) as Sprite;
        }
        else if (ItemName == "Spice-Berry Candy")
        {
            ItemImage = Resources.Load("CandyIcons/SpiceBerry", typeof(Sprite)) as Sprite;
        }
        else
        {
            Debug.Log("Error, no image match");
        }
    }

    //Takes in an item name and determine which audio effect should play for the matching item
    public static AudioClip DetermineAudioClip(string itemName)
    {
        if(itemName == "Power-Up Candy")
        {
            return Audio.AttackUp;
        }
        else if(itemName == "Bulk-Up Candy")
        {
            return Audio.DefenseUp;
        }
        else if(itemName == "Paralyze Candy")
        {
            return Audio.Paralyze;
        }
        else if(itemName == "Poison Candy")
        {
            return Audio.Poison;
        }
        else if(itemName == "Sleep Candy")
        {
            return Audio.Sleep;
        }
        else if(itemName == "Confusion Candy")
        {
            return Audio.Confusion;
        }
        else
        {
            Debug.Log("Audio finder error, no audio for item");
            return null;
        }
    }

    //GETTERS
    public string GetItemName() { return ItemName; }
    public string GetItemDescription() { return ItemDescription; }
    public int GetItemDuration() { return ItemDuration; }
    public bool GetUsabilityTeam() { return IsUsableOnTeam; }
    public bool GetHealing() { return IsHealing; }
    public Sprite GetItemImage() { return ItemImage; }
    public Endimon.StatusEffects GetEffect() { return Effect; }

    //SETTERS
    public void SetItemName(string s) { ItemName = s; }
    public void SetItemDescription(string s) { ItemDescription = s; }
    public void SetItemDuration(int n) { ItemDuration = n; }
    public void SetUsabilityTeam(bool b) { IsUsableOnTeam = b; }
    public void SetEffect(Endimon.StatusEffects e) { Effect = e; }
}
