using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Converts to an AI upon being selected
public class Trainer
{
    private string TrainerName;     //Name of Trainer
    private string TrainerBio;      //A short description about the said Trainer
    private Sprite TrainerImage;    //Image of the specific trainer
    private Endimon[] Team;         //The 4 preset Endimon they will have
    private Item[] Items;           //List of items the scripted trainer has
    private CharacterSelectController.DifficultySelection TrainerDifficulty;    //Difficulty of the AI
    public Color ImageColor;        //Background color of the trainer

    //Construct a Trainer
    public Trainer(string theName, string bio, CharacterSelectController.DifficultySelection diff, Endimon[] theTeam, Item[] theItems) 
    {
        TrainerName = theName;
        TrainerBio = bio;
        TrainerDifficulty = diff;
        Team = theTeam;
        Items = theItems;
        DetermineTrainer();
    }

    //Function sets the image, team and items of a specific trainer
    public void DetermineTrainer()
    {
        if(TrainerName == "Connor")
        {
            TrainerImage = Resources.Load("TrainerImages/Trainer1", typeof(Sprite)) as Sprite;
        }
        else if(TrainerName == "Bonnie")
        {
            TrainerImage = Resources.Load("TrainerImages/Trainer2", typeof(Sprite)) as Sprite;
        }
        else if (TrainerName == "Wes")
        {
            TrainerImage = Resources.Load("TrainerImages/Trainer3", typeof(Sprite)) as Sprite;
        }
        else if (TrainerName == "Laurel")
        {
            TrainerImage = Resources.Load("TrainerImages/Trainer4", typeof(Sprite)) as Sprite;
        }
        else if (TrainerName == "Michaela")
        {
            TrainerImage = Resources.Load("TrainerImages/Trainer5", typeof(Sprite)) as Sprite;
        }
        else if(TrainerName == "Frank")
        {
            TrainerImage = Resources.Load("TrainerImages/Trainer6", typeof(Sprite)) as Sprite;
        }
        else
        {
            Debug.Log("Error, no set image for trainer");
        }
    }

    //SETTERS
    public void SetTrainerName(string theName) { TrainerName = theName; }
    public void SetTrainerBio(string bio) { TrainerBio = bio; }
    public void SetTrainerDifficulty(CharacterSelectController.DifficultySelection diff) { TrainerDifficulty = diff; }
    public void SetTrainerTeam(Endimon[] theTeam) { Team = theTeam; }
    public void SetTrainerItems(Item[] theItems) { Items = theItems; }
    public void SetTrainerImage(Sprite img) { TrainerImage = img; }

    //GETTERS
    public string GetTrainerName() { return TrainerName; }
    public string GetTrainerBio() { return TrainerBio; }
    public CharacterSelectController.DifficultySelection GetTrainerDifficulty() { return TrainerDifficulty; }
    public Endimon[] GetTrainerTeam() { return Team; }
    public Item[] GetTrainerItems() { return Items; }
    public Sprite GetTrainerImage() { return TrainerImage; }
}
