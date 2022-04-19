using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class is a global data structure to keep track of information needed across scenes
public static class GameProfile
{
    public static Character CurrentCharacter;       //The character being used for the battle
    public static AI CurrentAI;                     //The AI being used for the battle
    public static int HighestFinishedLevel;         //Highest completed camapaign battle (1-6)
    public static bool PlayingACampaignBattle;      //If the player has picked to play a campaign battle

    //Premade Objects to display
    public static Endimon[] Roster;                 //Array of the entire Endimon roster
    public static Item[] Items;                     //Array of the entire list of items
    public static Trainer[] Trainers;               //Array of all the scripted battles

    //Create the data for all the Endimon in the game
    public static void CreateRoster()
    {
        Roster = new Endimon[10];
        for (int i = 0; i < 10; i++)
        {
            Roster[i] = CreateEndimonInstance(i);
        }
    }

    //Create the data for all the items in the game
    public static void CreateItems()
    {
        Items = new Item[8];
        for (int i = 0; i < 8; i++)
        {
            Items[i] = CreateItemInstance(i);
        }
    }

    //Create the data for all the Endimon in the game
    public static void CreateTrainers()
    {
        Trainers = new Trainer[6]; 
        for (int i = 0; i < 6; i++)
        {
            Trainers[i] = CreateTrainerInstance(i);
        }
    }

    //Creates a single new Endimon instance for the player's team based upon the index passed in
    public static Endimon CreateEndimonInstance(int selection)
    {
        if(selection == 0)
        {
            return new Endimon("Corerosion", 1, Endimon.Endimontypes.Earth, "Utility", 30, 35, 400, "Bite", "Rollout",
            "Rejuvenation", Endimon.Endimontypes.Normal, Endimon.Endimontypes.Earth, 45, 50, "Restores a part of an Endimon's missing HP", false, false, true, false);
        }
        else if(selection == 1)
        {
            return new Endimon("Serpbolt", 2, Endimon.Endimontypes.Electro, "Utility", 40, 30, 400, "Gnaw", "Electric Chomp",
            "Noxious Fumes", Endimon.Endimontypes.Normal, Endimon.Endimontypes.Electro, 40, 45, "Has a chance to apply sleep status to enemy Endimon", false, false, true, true);
        }
        else if(selection == 2)
        {
            return new Endimon("Scorcher", 3, Endimon.Endimontypes.Pyro, "Attacker", 50, 30, 300, "Blaze Slash", "Smoldering Clap",
        "Ring of Fire", Endimon.Endimontypes.Pyro, Endimon.Endimontypes.Shadow, 50, 45, "Global Effect: Pyro moves receive a damage boost", true, false, false, false);
        }
        else if (selection == 3)
        {
            return new Endimon("Snowshade", 4, Endimon.Endimontypes.Frost, "Tank", 40, 40, 350, "Frozen Jab", "Dry Ice",
            "Icicle Baracade", Endimon.Endimontypes.Frost, Endimon.Endimontypes.Pyro, 45, 40, "Boosts the defense of the target for two turns", true, false, true, false);
        }
        else if (selection == 4)
        {
            return new Endimon("Demonican", 5, Endimon.Endimontypes.Shadow, "Attacker", 45, 25, 350, "Toxic Grasp", "Deathly Spin",
            "Shadowcast", Endimon.Endimontypes.Earth, Endimon.Endimontypes.Shadow, 40, 50, "Global Effect: All non-shadow moves have a chance of missing", false, true, false, false);
        }
        else if (selection == 5)
        {
            return new Endimon("Fruitfly", 6, Endimon.Endimontypes.Shadow, "Tank", 35, 30, 400, "Poison Spit", "Fang",
            "Screech", Endimon.Endimontypes.Shadow, Endimon.Endimontypes.Normal, 40, 45, "Boosts the attack of the target for two rounds", false, false, true, false);
        }
        else if (selection == 6)
        {
            return new Endimon("Froghost", 7, Endimon.Endimontypes.Frost, "Balanced", 40, 30, 350, "Bash", "Snowy Breath",
            "Blizzard", Endimon.Endimontypes.Normal, Endimon.Endimontypes.Frost, 45, 45, "Global Effect: Creates a blizzard that does damage to all all non-frost type Endimon", false, false, false, false);
        }
        else if (selection == 7)
        {
            return new Endimon("Prickly", 8, Endimon.Endimontypes.Earth, "Attacker", 50, 25, 350, "Volt Charge", "Thorn Blast",
            "Synthesis", Endimon.Endimontypes.Electro, Endimon.Endimontypes.Earth, 50, 60, "Applies a healing aura that heals a small bit over 4 turns", false, true, true, false);
        }
        else if (selection == 8)
        {
            return new Endimon("Coalfire", 9, Endimon.Endimontypes.Pyro, "Balanced", 40, 30, 350, "Scratch", "Flame Ball",
            "Heating Up", Endimon.Endimontypes.Normal, Endimon.Endimontypes.Pyro, 45, 50, "Targetted Endimon's next attack will do drastically more damage", false, false, true, false);
        }
        else if (selection == 9)
        {
            return new Endimon("Zapcat", 10, Endimon.Endimontypes.Electro, "Attacker", 55, 20, 300, "Lightning Dash", "Freeze Flash",
            "Speedray", Endimon.Endimontypes.Electro, Endimon.Endimontypes.Frost, 50, 40, "Target has chance to dodge incoming attacks for next three rounds", true, false, true, false);
        }
        Debug.Log("Null returned");
        return null;
    }

    public static Item CreateItemInstance(int selection) 
    {
        if (selection == 0)
        {
            return new Item("Power-Up Candy", "Gives a small attack boost to the targetted Endimon for 3 turns", 3, true, false, Endimon.StatusEffects.AttackUp);
        }
        else if(selection == 1)
        {
            return new Item("Bulk-Up Candy", "Gives a small defense boost to the targetted Endimon for 3 turns", 3, true, false, Endimon.StatusEffects.DefenseUp);
        }
        else if (selection == 2)
        {
            return new Item("Paralyze Candy", "Applies paralysis status which negates all non-normal type damage for 2 turns", 2, false, false, Endimon.StatusEffects.Paralyze);
        }
        else if (selection == 3)
        {
            return new Item("Poison Candy", "Applies poison status which reduces HP and defense overtime for 3 turns", 3, false, false, Endimon.StatusEffects.Poison);
        }
        else if (selection == 4)
        {
            return new Item("Sleep Candy", "Applies sleep status which renders the target turn useless", 1, false, false, Endimon.StatusEffects.Sleep);
        }
        else if (selection == 5)
        {
            return new Item("Confusion Candy", "Applies confusion status that has chance to make target attack itself for 2 turns", 2, false, false, Endimon.StatusEffects.Confusion);
        }
        else if (selection == 6)
        {
            return new Item("Berry Candy", "Restores a quarter of the Endimon's max HP", 0, true, true, Endimon.StatusEffects.HealthRestore);
        }
        else if (selection == 7)
        {
            return new Item("Spice-Berry Candy", "Restores half of Endimon's max HP but permanetly lowers defense", 0, true, true, Endimon.StatusEffects.LargeHealthRestore);
        }
        Debug.Log("Returned a null item");
        return null;
    }

    //Create the data for all the trainers in the game
    public static Trainer CreateTrainerInstance(int selection)
    {
        Endimon[] Team = new Endimon[4];
        Item[] Items;
        if (selection == 0)
        {
            Team = CreateTeam(CreateEndimonInstance(0), CreateEndimonInstance(5), CreateEndimonInstance(1), CreateEndimonInstance(6));
            Items = new Item[1];
            Items[0] = null;
            return new Trainer("Connor", "Even though he is still learning how to play, Connor's diverse team is sure to cover its weaknesses. Prioritizing Endimon with normal type moves, he plans to keep things simple.",
                CharacterSelectController.DifficultySelection.Easy, Team, Items);
        }
        else if (selection == 1)
        {
            Team = CreateTeam(CreateEndimonInstance(2), CreateEndimonInstance(8), CreateEndimonInstance(6), CreateEndimonInstance(3));
            Items = new Item[1];
            Items[0] = null;
            return new Trainer("Bonnie", "Although still new to battling, Bonnie has developed a strong team. With a large focus on Pyro and Frost Endimon, plan for a bit of melting.",
                CharacterSelectController.DifficultySelection.Easy, Team, Items);
        }
        else if (selection == 2)
        {
            Items = new Item[3];
            Items[0] = CreateItemInstance(6);
            Items[1] = CreateItemInstance(1);
            Items[2] = CreateItemInstance(0);
            Team = CreateTeam(CreateEndimonInstance(7), CreateEndimonInstance(5), CreateEndimonInstance(8), CreateEndimonInstance(2));
            return new Trainer("Wes", "After a few months of dueling, Wes has become a worthy opponent. With harder hitting Endimon, Wes plans to beef up his diverse team in order to beat any challenger.",
                CharacterSelectController.DifficultySelection.Medium, Team, Items);
        }
        else if (selection == 3)
        {
            Items = new Item[3];
            Items[0] = CreateItemInstance(6);
            Items[1] = CreateItemInstance(6);
            Items[2] = CreateItemInstance(0);
            Team = CreateTeam(CreateEndimonInstance(0), CreateEndimonInstance(9), CreateEndimonInstance(7), CreateEndimonInstance(1));
            return new Trainer("Laurel", "Her wisdom in battling is proven in the amount of battles she has won. Her core of hard hitting Endimon tend to be tough to take down with her all of her healing.",
                CharacterSelectController.DifficultySelection.Medium, Team, Items);
        }
        else if (selection == 4)
        {
            Items = new Item[5];
            Items[0] = CreateItemInstance(6);
            Items[1] = CreateItemInstance(7);
            Items[2] = CreateItemInstance(3);
            Items[3] = CreateItemInstance(1);
            Items[4] = CreateItemInstance(1);
            Team = CreateTeam(CreateEndimonInstance(4), CreateEndimonInstance(7), CreateEndimonInstance(3), CreateEndimonInstance(0));
            return new Trainer("Michaela", "With years of experience, Michaela is a hard opponent to take down. With a very well rounded team, she expects to inflict high damage with her strengthened up Endimon.",
                CharacterSelectController.DifficultySelection.Hard, Team, Items);
        }
        else if (selection == 5)
        {
            Items = new Item[5];
            Items[0] = CreateItemInstance(6);
            Items[1] = CreateItemInstance(7);
            Items[2] = CreateItemInstance(3);
            Items[3] = CreateItemInstance(5);
            Items[4] = CreateItemInstance(4);
            Team = CreateTeam(CreateEndimonInstance(4), CreateEndimonInstance(9), CreateEndimonInstance(7), CreateEndimonInstance(2));
            return new Trainer("Frank", "Being one of the strongest opponents out there, Frank is nearly impossible to take down. Already having a team that counters most of what you could throw at it, Frank's usage of items ensures that anything you come up with won't work.",
                CharacterSelectController.DifficultySelection.Hard, Team, Items);
        }
        else
        {
            Debug.Log("Error, no trainer was created");
            return null;
        }
    }

    //Player's game is saved at various points to keep track of camapaign progress
    public static void SaveGame()
    {
        PlayerPrefs.SetInt("Campaign-Level", HighestFinishedLevel);
    }

    //The player completed a campaign battle
    public static void BeatALevel()
    {
        if (HighestFinishedLevel < 6)
        {
            HighestFinishedLevel++;
            SaveGame();
        }
    }

    //Function takes the selected Trainer to assign it as an AI
    public static void ConvertTrainerToAI(int TrainerIndex)
    {
        CurrentAI = new AI(Trainers[TrainerIndex].GetTrainerDifficulty());
        CurrentAI.SetAIName(Trainers[TrainerIndex].GetTrainerName());
        CurrentAI.SetAITeam(Trainers[TrainerIndex].GetTrainerTeam());
        CurrentAI.SetAIItems(Trainers[TrainerIndex].GetTrainerItems());
    }

    public static Endimon[] CreateTeam(Endimon e1, Endimon e2, Endimon e3, Endimon e4)
    {
        Endimon[] newTeam = new Endimon[4];
        newTeam[0] = e1;
        newTeam[1] = e2;
        newTeam[2] = e3;
        newTeam[3] = e4;
        return newTeam;
    }

    //SETTERS
    public static void SetCurrentCharacter(Character c) { CurrentCharacter = c; }
    public static void SetCurrentAI(AI ai) { CurrentAI = ai; }
    public static void SetHighestFinishedLevel(int l) { HighestFinishedLevel = l; }
    public static void SetPlayingACampaignBattle(bool i) { PlayingACampaignBattle = i; }

    //GETTERS
    public static Character GetCurrentCharacter() { return CurrentCharacter; }
    public static AI GetCurrentAI() { return CurrentAI; }
    public static int GetHighestFinishedLevel() { return HighestFinishedLevel; }
    public static bool GetPlayingACampaignBattle() { return PlayingACampaignBattle; }
}
