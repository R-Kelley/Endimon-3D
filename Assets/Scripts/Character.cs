using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    private Endimon[] PlayerTeam;           //Character's team of 4 Endimon
    private Item[] PlayerItems;             //Character's selected items
    private Endimon ActivePlayerEndimon1;   //Character's first Endimon in battle
    private Endimon ActivePlayerEndimon2;   //Character's second Endimon in battle

    //Constructor, allow for 4 Endimon and 3 items
    public Character()
    {
        PlayerTeam = new Endimon[4];
        PlayerItems = new Item[3];
    }

    //Will change an active endimon to one that is alive (function called when a death occurs) Gives back slot to place this Endimon in
    public int SwapEndimonOnDeath(Endimon e)
    {
        Endimon NewEndimon = null;
        for(int i = 0; i < 4; i++)
        {
            //If this Endimon is already out and has HP, this is our new Endimon to put in
            if(PlayerTeam[i] != ActivePlayerEndimon1 && PlayerTeam[i] != ActivePlayerEndimon2 && IsEndimonAlive(PlayerTeam[i]))
            {
                NewEndimon = PlayerTeam[i];
                break;
            }
        }

        //All the team's Endimon must be dead so there is nothing to give back
        if(NewEndimon == null)
        {
            return 0;
        }

        //Find the active endimon to replace, keeping it's slot and status in the turn rotation
        if (ActivePlayerEndimon1.GetName() == e.GetName())
        {
            if(ActivePlayerEndimon1.GetEndimonTurnTaken() == true)
            {
                NewEndimon.SetTurnStatus(true);
            }
            NewEndimon.SetActiveNumber(ActivePlayerEndimon1.GetActiveNumber());
            ActivePlayerEndimon1 = NewEndimon;
            return 1;
        }
        else
        {
            if (ActivePlayerEndimon2.GetEndimonTurnTaken() == true)
            {
                NewEndimon.SetTurnStatus(true);
            }
            NewEndimon.SetActiveNumber(ActivePlayerEndimon2.GetActiveNumber());
            ActivePlayerEndimon2 = NewEndimon;
            return 2;
        }
    }

    //Player willingly wants to switch, will make sure to put in an Endimon depending on which active Endimon/slot that they want
    public void SwapEndimonOnTurn(int slot, Endimon e)
    {
        if(slot == 1)
        {
            if (ActivePlayerEndimon1.GetEndimonTurnTaken() == true)
            {
                e.SetTurnStatus(true);
            }
            else
            {
                e.SetTurnStatus(false);
            }
            ActivePlayerEndimon1.SetTurnStatus(false);
            ActivePlayerEndimon1 = e;
            ActivePlayerEndimon1.SetActiveNumber(0);
        }
        else if(slot == 2)
        {
            if (ActivePlayerEndimon2.GetEndimonTurnTaken() == true)
            {
                e.SetTurnStatus(true);
            }
            else
            {
                e.SetTurnStatus(false);
            }
            ActivePlayerEndimon2.SetTurnStatus(false);
            ActivePlayerEndimon2 = e;
            ActivePlayerEndimon2.SetActiveNumber(1);
        }
    }

    //Adds an Endimon to the team (will first look for a slot to put it into)
    public void AddEndimon(Endimon e) 
    {
        int slot = FindEmptySlot();
        if (slot != -1)
        {
            PlayerTeam[slot] = e;
        }
    }

    //Removes an Endimon at a specific location in the team
    public void RemoveEndimon(int slot)
    {
        PlayerTeam[slot] = null;
    }

    //Removes an Endimon from the very back of the team
    public void RemoveEndimon()
    {
        for (int i = 3; i > -1; i--) {
            if(PlayerTeam[i] != null)
            {
                PlayerTeam[i] = null;
                break;
            }
        }
    }

    //Checks to see how many slots are full in the team (Can't check last slot as it can be out of order)
    public int CheckPlayerTeam()
    {
        int counter = 0;
        for (int i = 0; i < 4; i++)
        {
            if (PlayerTeam[i] != null)
            {
                counter++;
            }
        }
        return counter;
    }

    //Finds the first slot that is empty on the team (with removing, the array might be out of order)
    public int FindEmptySlot()
    {
        for (int i = 0; i < 4; i++)
        {
            if (PlayerTeam[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    //Looks to find if there is a specific Endimon already on the team (avoid duplicates)
    public bool FindEndimonOnTeam(string EndiName)
    {
        for(int i = 0; i < 4; i++)
        {
            if(PlayerTeam[i] != null && PlayerTeam[i].GetName() == EndiName)
            {
                return true;
            }
        }
        return false;
    }

    //Looks to see if the user has any Endimon with HP
    public bool IsTeamAlive()
    {
        for (int i = 0; i < 4; i++)
        {
            if (IsEndimonAlive(PlayerTeam[i]))
            {
                return true;
            }
        }
        return false;
    }

    //Helper function that takes an Endimon and tells you if it has any HP remaining
    public bool IsEndimonAlive(Endimon e)
    {
        if(e.GetCurrentHP() > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Adds an item to the array of items the player has
    public void AddItem(Item theItem)
    {
        for(int i = 0; i < PlayerItems.Length; i++)
        {
            if(PlayerItems[i] == null)
            {
                PlayerItems[i] = theItem;
                return; //Only add it once
            }
        }
    }

    //Removes an item from the array (Plucks first one)
    public void RemoveItem()
    {
        for(int i = PlayerItems.Length-1; i > -1; i--)
        {
            if(PlayerItems[i] != null)
            {
                PlayerItems[i] = null;
                break;
            }
        }
    }

    //Removes an item based upon the positon in the array
    public void RemoveItem(int pos)
    {
        PlayerItems[pos] = null;
    }

    //Removes an item based upon which item it is
    public void RemoveItem(Item item)
    {
        for(int i = 0; i < 3; i++)
        {
            if(PlayerItems[i] != null && item.GetItemName() == PlayerItems[i].GetItemName())
            {
                PlayerItems[i] = null;
                break;  //Do not erase all of the same named items, just one
            }
        }
    }

    //See if the player has a full list of items or not
    public int CheckPlayerItems()
    {
        int counter = 0;
        for (int i = 0; i < 3; i++)
        {
            if (PlayerItems[i] != null)
            {
                counter++;
            }
        }
        return counter;
    }

    //Grab an item from the player's list of items
    public Item GetAnItem(int index)
    {
        return PlayerItems[index];
    }

    //Player has chose to use an item during battle
    public void UseItem(Item UsedItem, Endimon TargettedEndimon, BattleController bc)
    {
        //If there is a duration, we will apply the particls, play audio, and put effect on
        if (UsedItem.GetItemDuration() > 0)
        {
            AudioClip itemClip = Item.DetermineAudioClip(UsedItem.GetItemName());
            AudioSource.PlayClipAtPoint(itemClip, GameObject.Find("MainCamera").transform.position);
            TargettedEndimon.AddStatusEffect(!UsedItem.GetUsabilityTeam(), UsedItem.GetEffect(), UsedItem.GetItemDuration());
        }

        //Otherwise this is an instant effect so no overtime particles needed
        else
        {
            //Small health restore will give 25% of HP back
            if(UsedItem.GetEffect() == Endimon.StatusEffects.HealthRestore)
            {
                //We will hurt the target with negative damage to heal
                int HealthToRestore = (int)(TargettedEndimon.GetHealth() * 0.25 * -1);
                if(TargettedEndimon.GetCurrentHP() + HealthToRestore == TargettedEndimon.GetHealth())
                {
                    HealthToRestore = (int)(TargettedEndimon.GetHealth() - TargettedEndimon.GetCurrentHP() * -1);
                }
                AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
                bc.SpawnText("Healing", HealthToRestore * -1, TargettedEndimon);
                TargettedEndimon.TakeDamage(HealthToRestore);
            }

            //Large health restore will give 50% of HP back and remove 15 defense
            else if(UsedItem.GetEffect() == Endimon.StatusEffects.LargeHealthRestore)
            {
                //We will hurt the target with negative damage to heal
                int HealthToRestore = (int)(TargettedEndimon.GetHealth() * 0.5 * -1);
                if (TargettedEndimon.GetCurrentHP() + HealthToRestore == TargettedEndimon.GetHealth())
                {
                    HealthToRestore = (int)(TargettedEndimon.GetHealth() - TargettedEndimon.GetCurrentHP() * -1);
                }
                AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
                bc.SpawnText("Healing", HealthToRestore * -1, TargettedEndimon);
                TargettedEndimon.TakeDamage(HealthToRestore);
                TargettedEndimon.SetDefense(-15);   //Endimon loses part of its defense for getting healed this amount
            }
        }
    }

    //Returns an Endimon based upon the index given
    public Endimon GetEndimon(int index)
    {
        return PlayerTeam[index];
    }

    //Getters
    public Endimon[] GetTeam() { return PlayerTeam; }
    public Item[] GetItems() { return PlayerItems; }
    public Endimon GetActiveEndimon1() { return ActivePlayerEndimon1; }
    public Endimon GetActiveEndimon2() { return ActivePlayerEndimon2; }

    //Setters
    public void SetActiveEndimon1(int index) { ActivePlayerEndimon1 = PlayerTeam[index]; }
    public void SetActiveEndimon2(int index) { ActivePlayerEndimon2 = PlayerTeam[index]; }
    public void SetActiveEndimon1(Endimon e) { ActivePlayerEndimon1 = e; }
    public void SetActiveEndimon2(Endimon e) { ActivePlayerEndimon2 = e; }
}