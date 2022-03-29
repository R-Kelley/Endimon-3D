using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CampaignMapController : MonoBehaviour
{
    private int LevelSelected;              //The currently selected battle
    private Trainer SelectedTrainer;        //The selected trainer (corresponds with the selected battle)
    private Trainer[] AllTrainers;          //Copied list of all Trainers to battle
    public GameObject LevelHover;           //The GO of the highlighter
    private RectTransform LevelHoverMover;  //The transform of the highlighter

    //All components on the screen that will be updated in code
    public TextMeshProUGUI TName;
    public TextMeshProUGUI TBio;
    public TextMeshProUGUI TDifficulty;
    public TextMeshProUGUI LevelStatus;
    public Image TImage;
    public Button ChallengeButton;

    //Will populate the default level 1 trainer, as well as figure out your current progress by loading it
    void Start()
    {
        AllTrainers = GameProfile.Trainers;
        LevelHoverMover = LevelHover.GetComponent<RectTransform>();
        LevelSelected = 1;
        SelectedTrainer = AllTrainers[0];
        UpdateScreen();
    }

    void Update()
    { 
        //PRESSING LEFT MOVES CURSOR TO THE LEFT, ASSUMING IT CAN
        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
            //Determine if you can go left (can be on level 2, 4 and 5)
            if(LevelSelected == 2 || LevelSelected == 4 || LevelSelected == 5)
            {
                AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
                LevelSelected--;
                SelectedTrainer = AllTrainers[LevelSelected - 1];
                UpdateScreen();
            }
        }

        //PRESSING RIGHT MOVES THE CURSOR TO THE RIGHT, ASSUMING IT CAN
        if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            //Determine if you can go right (can be level 1, 3, 4)
            if(LevelSelected == 1 || LevelSelected == 3 || LevelSelected == 4)
            {
                AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
                LevelSelected++;
                SelectedTrainer = AllTrainers[LevelSelected - 1];
                UpdateScreen();
            }
        }

        //PRESS UP MOVES THE CURSOR UPWARDS, ASSUMING IT CAN
        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            //Determine if you can go up (can be level 3 and 5)
            if(LevelSelected == 3)
            {
                Level2Clicked();
            }
            else if(LevelSelected == 5)
            {
                Level6Clicked();
            }
        }

        //PRESSING DOWN MOVES THE CURSOR DOWNWARDS, ASSUMING IT CAN
        if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            //Determine if you can go down (can be level 2 or 6)
            if(LevelSelected == 2)
            {
                Level3Clicked();
            }
            else if(LevelSelected == 6)
            {
                Level5Clicked();
            }
        }

        //PRESSING ENTER INITIATES THE BATTLE
        if(Input.GetKeyDown(KeyCode.Return))
        {
            ChallengeBtnClicked();
        }

        //PRESSING BACKSPACE RETURNS THE PLAYER TO THE HOME SCREEN
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            BackBtnClicked();
        }
    }

    //Level 1 button pressed, display info and move selection
    public void Level1Clicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        LevelSelected = 1;
        SelectedTrainer = AllTrainers[0];
        UpdateScreen();
    }

    //Level 2 button pressed, display info and move selection
    public void Level2Clicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        LevelSelected = 2;
        SelectedTrainer = AllTrainers[1];
        UpdateScreen();
    }

    //Level 3 button pressed, display info and move selection
    public void Level3Clicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        LevelSelected = 3;
        SelectedTrainer = AllTrainers[2];
        UpdateScreen();
    }

    //Level 4 button pressed, display info and move selection
    public void Level4Clicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        LevelSelected = 4;
        SelectedTrainer = AllTrainers[3];
        UpdateScreen();
    }

    //Level 5 button pressed, display info and move selection
    public void Level5Clicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        LevelSelected = 5;
        SelectedTrainer = AllTrainers[4];
        UpdateScreen();
    }

    //Level 6 button pressed, display info and move selection
    public void Level6Clicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        LevelSelected = 6;
        SelectedTrainer = AllTrainers[5];
        UpdateScreen();
    }

    //Player has selected to battle this trainer, tell the program to play the game with a preset AI
    public void ChallengeBtnClicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        if (LevelSelected <= GameProfile.HighestFinishedLevel + 1)
        {
            //*WE NEED TO ACTUALLY DEVELOP THE TEAM AT SOME POINT
            GameProfile.PlayingACampaignBattle = true;
            GameProfile.ConvertTrainerToAI(LevelSelected-1);    //We want the index here so subtract one
            SceneManager.LoadScene("CharacterSelection");
        }
    }

    //Player wants back to the Main Menu
    public void BackBtnClicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
        SceneManager.LoadScene("MainMenu");
    }

    //Change around all the trainer text once a new one is highlighted, as well as telling the button highlighter to move
    public void UpdateScreen()
    {
        TName.SetText(AllTrainers[LevelSelected-1].GetTrainerName());
        TBio.SetText(AllTrainers[LevelSelected - 1].GetTrainerBio());
        TDifficulty.SetText(AllTrainers[LevelSelected - 1].GetTrainerDifficulty().ToString());
        TImage.sprite = AllTrainers[LevelSelected - 1].GetTrainerImage();
        

        if(LevelSelected <= GameProfile.HighestFinishedLevel+1)
        {
            ChallengeButton.enabled = true;
            if (LevelSelected == GameProfile.HighestFinishedLevel + 1)
            {
                LevelStatus.text = "Status: Incomplete";
            }
            else {
                LevelStatus.text = "Status: Completed";
            }
        }
        else
        {
            ChallengeButton.enabled = false;
            LevelStatus.text = "Status: Incomplete";
        }

        if(LevelSelected == 1)
        {
            MoveHover(-811, 285);
        }
        if (LevelSelected == 2)
        {
            MoveHover(-465, 285);
        }
        if (LevelSelected == 3)
        {
            MoveHover(-465, -26);
        }
        if (LevelSelected == 4)
        {
            MoveHover(5, -26);
        }
        if (LevelSelected == 5)
        {
            MoveHover(423, -26);
        }
        if (LevelSelected == 6)
        {
            MoveHover(423, 285);
        }
    }

    //Change the position of the box highlighting the button the player is on
    public void MoveHover(int xChange, int yChange)
    {
        LevelHoverMover.localPosition = new Vector3(xChange, yChange, 0);
    }
}
