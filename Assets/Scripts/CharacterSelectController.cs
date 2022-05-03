using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectController : MonoBehaviour
{
    private Character Player;       //The human Player
    private AI AIPlayer;            //The AI the Player will play against
    private Endimon[] Roster;       //The roster of Endimon that is available to the players
    private int EndimonNumber;      //The currently hovered Endimon on screen

    //Stat Text to display to the human selecting characters
    public TextMeshProUGUI EName;
    public Image EImage;
    public TextMeshProUGUI EType;
    public TextMeshProUGUI ERole;
    public TextMeshProUGUI EAtt;
    public TextMeshProUGUI EDef;
    public TextMeshProUGUI EHealth;
    public TextMeshProUGUI M1Name;
    public TextMeshProUGUI M2Name;
    public TextMeshProUGUI M3Name;
    public TextMeshProUGUI M1Dmg;
    public TextMeshProUGUI M2Dmg;
    public TextMeshProUGUI M3Desc;
    public TextMeshProUGUI Endimon1Text;
    public TextMeshProUGUI Endimon2Text;
    public TextMeshProUGUI Endimon3Text;
    public TextMeshProUGUI Endimon4Text;
    public TextMeshProUGUI TeamTotalText;

    //Buttons to enable/disable
    public Button ContinueButton;
    public Button SelectButton;
    
    //Arrow Pointer for the difficulties
    public GameObject Arrow;
    private RectTransform ArrowMover;
    Color32[] SelectedEndimonColors;

    //Values for the pointer to move
    private int[] XValues;      //Hold all x cords for the Arrow (Easy-Med-Hard) 
    private int[] YValues;      //Hold all y cords for the Arrow (Easy-Med-Hard) 

    //Difficulty selector to decide how hard the AI will be
    public enum DifficultySelection { Easy, Medium, Hard };
    private DifficultySelection UserSelection;
    private const int NumberOfSelections = 3;   //Number of Difficulty selections, update when necessary


    void Start()
    {
        //Create default values for the on screen to display
        Player = new Character();
        Roster = GameProfile.Roster;
        SelectedEndimonColors = new Color32[3];     //Colors of an Endimon's type as well as their 2 move's type (move 3 is always their own)
        XValues = new int[] { -28 };
        YValues = new int[] { 20, -9, -36 };
        ArrowMover = Arrow.GetComponent<RectTransform>();
        EndimonNumber = 0;
        ChangeCharacterHover();                     //Display the first Endimon's stats
        SetInitialDifficulty();                     //Sets the default difficulty (easy unless in a campaign)
        ContinueButton.interactable = false;        //Must select 4 Endimon first
    }

    //Listens for user keyboard presses
    void Update()
    {
        //PRESSING DOWN WILL MOVE THE DIFFICULTY BUTTON DOWN OR TO THE TOP
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            UserSelection += 1;
            if ((int)UserSelection == NumberOfSelections)
            {
                UserSelection = DifficultySelection.Easy;
            }
            ChangeDifficultyHover();
        }

        //PRESSING UP WILL MOVE THE DIFFICULTY BUTTON UP OR THE VERY BOTTOM
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            UserSelection -= 1;
            if (UserSelection < 0)
            {
                UserSelection = DifficultySelection.Hard;
            }
            ChangeDifficultyHover();
        }

        //LEFT ARROW WILL CHANGE SELECTION IN THE LEFT DIRECTION
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            LeftArrowClick();
        }

        //RIGHT ARROW WILL CHANGE SELECTION IN THE RIGHT DIRECTION
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            RightArrowClick();
        }

        //PRESSING SPACE WILL ADD AN ENDIMON TO THE TEAM
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (Player.CheckPlayerTeam() < 4)
            {
                SelectionBtnClick();
            }
        }

        //PRESSING ENTER WILL TRY AND CONTINUE (ONLY IF 4 ENDIMON IN TEAM)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(Player.CheckPlayerTeam() >= 4)
            {
                ContinueBtnClick();
            }
        }

        //PRESSING BACKSPACE WILL REMOVE MOST RECENT ENDIMON, OR GO BACK TO MAIN_MENU IF THERE ARE 0
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
            if (Player.CheckPlayerTeam() > 0)
            {
                Player.RemoveEndimon();
                UpdateTeamUI();
            }
            else
            {
                BackBtnClick();
            }
        }
    }

    //Left Arrow was clicked, change selection in descending order
    public void LeftArrowClick()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        EndimonNumber -= 1;
        if (EndimonNumber < 0)
        {
            EndimonNumber = 9;
        }
        ChangeCharacterHover();
    }

    //Right Arrow was clicked, change selection in ascending order
    public void RightArrowClick()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        EndimonNumber += 1;
        if (EndimonNumber >= Roster.Length)
        {
            EndimonNumber = 0;
        }
        ChangeCharacterHover();
    }

    //Add the endimon highlighted to the team
    public void SelectionBtnClick()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        //Check to see if there is a slot open, and if that selected endimon is not already on the player's team before adding
        if (Player.CheckPlayerTeam() != -1 && !Player.FindEndimonOnTeam(Roster[EndimonNumber].GetName()))
        {
            Player.AddEndimon(GameProfile.CreateEndimonInstance(EndimonNumber));
            UpdateTeamUI();
            SelectButton.interactable = false;  //Can't select the same one again
        }
    }

    //Continue to the item selection screen, make sure the player and AI are set
    public void ContinueBtnClick()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        if (!GameProfile.PlayingACampaignBattle)
        {
            AIPlayer = new AI(UserSelection);
            GameProfile.CurrentAI = AIPlayer;
        }
        GameProfile.CurrentCharacter = Player;
        SceneManager.LoadScene("ItemSelection");
    }

    //Move pointer to the easy button
    public void EasyBtnClick()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        if (!GameProfile.PlayingACampaignBattle)
        {
            UserSelection = DifficultySelection.Easy;
            MovePointer(XValues[0], YValues[0]);
        }
    }

    //Move pointer to the medium button
    public void MediumBtnClick()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        if (!GameProfile.PlayingACampaignBattle)
        {
            UserSelection = DifficultySelection.Medium;
            MovePointer(XValues[0], YValues[1]);
        }
    }

    //Move pointer to the hard button
    public void HardBtnClick()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        if (!GameProfile.PlayingACampaignBattle)
        {
            UserSelection = DifficultySelection.Hard;
            MovePointer(XValues[0], YValues[2]);
        }
    }

    //Button to remove the first Endimon in the party
    public void Remove1Click()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
        Player.RemoveEndimon(0);
        UpdateTeamUI();
    }

    //Button to remove the second Endimon in the party
    public void Remove2Click()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
        Player.RemoveEndimon(1);
        UpdateTeamUI();
    }

    //Button to remove the third Endimon in the party
    public void Remove3Click()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
        Player.RemoveEndimon(2);
        UpdateTeamUI();
    }

    //Button to remove the fourth Endimon in the party
    public void Remove4Click()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
        Player.RemoveEndimon(3);
        UpdateTeamUI();
    }

    //Button takes you back to the Main Menu, saving nothing here (Even if coming from Campaign)
    public void BackBtnClick()
    {
        if(GameProfile.PlayingACampaignBattle)
        {
            GameProfile.PlayingACampaignBattle = false;
            GameProfile.CurrentCampaignBattle = 0;
        }
        SceneManager.LoadScene("MainMenu");
    }

    public void ButtonHighlight()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
    }

    //Sets the starting difficulty upon load. Normally set to easy as default but must act accordingly if it's a camapaign battle
    public void SetInitialDifficulty()
    {
        if(GameProfile.PlayingACampaignBattle)
        {
            //We must handle pointer on our own as it will be disabled otherwise
            UserSelection = GameProfile.CurrentAI.GetAIDifficulty();
            if(UserSelection == DifficultySelection.Easy) { MovePointer(XValues[0], YValues[0]); }
            else if(UserSelection == DifficultySelection.Medium) { MovePointer(XValues[0], YValues[1]); }
            else if(UserSelection == DifficultySelection.Hard) { MovePointer(XValues[0], YValues[2]); }
        }
        else
        {
            UserSelection = DifficultySelection.Easy;
            MovePointer(XValues[0], YValues[0]);
        }
    }

    //For keyboard presses, this will determine what the selection is and adjust the Arrow
    public void ChangeDifficultyHover()
    {
        if (UserSelection == DifficultySelection.Easy)
        {
            EasyBtnClick();
        }
        else if (UserSelection == DifficultySelection.Medium)
        {
            MediumBtnClick();
        }
        else if (UserSelection == DifficultySelection.Hard)
        {
            HardBtnClick();
        }
    }

    //After performing an action on your team, the screen will update the roster
    public void UpdateTeamUI()
    {
        //First, update the roster and determine which slots have endimon so we can display their names
        //Also give them color based upon their type
        Endimon[] CurrentEndimon = Player.GetTeam();
        if (CurrentEndimon[0] != null) {
            Endimon1Text.SetText(CurrentEndimon[0].GetName());
            Endimon1Text.color = CurrentEndimon[0].GetPrimaryEndimonColor();
        }
        else { Endimon1Text.SetText(""); }

        if (CurrentEndimon[1] != null) {
            Endimon2Text.SetText(CurrentEndimon[1].GetName());
            Endimon2Text.color = CurrentEndimon[1].GetPrimaryEndimonColor();
        }
        else { Endimon2Text.SetText(""); }

        if (CurrentEndimon[2] != null) {
            Endimon3Text.SetText(CurrentEndimon[2].GetName());
            Endimon3Text.color = CurrentEndimon[2].GetPrimaryEndimonColor();
        }
        else { Endimon3Text.SetText(""); }

        if (CurrentEndimon[3] != null) {
            Endimon4Text.SetText(CurrentEndimon[3].GetName());
            Endimon4Text.color = CurrentEndimon[3].GetPrimaryEndimonColor();
        }
        else { Endimon4Text.SetText(""); }

        //Update the team total and see if you have a full party
        TeamTotalText.SetText("Team " + Player.CheckPlayerTeam() + "/4");
        if(Player.CheckPlayerTeam() >= 4)
        {
            ContinueButton.interactable = true;
        }
        else
        {
            ContinueButton.interactable = false;
        }
    }

    //When changing the Endimon you hover, the stats will adjust, also calling functions to update colors
    public void ChangeCharacterHover()
    {
        SelectedEndimonColors = Roster[EndimonNumber].GetEndimonTextColors();

        //Text Changes
        EName.SetText(Roster[EndimonNumber].GetName());
        EImage.sprite = Roster[EndimonNumber].GetEndimonLargeImage(); 
        EType.SetText(Roster[EndimonNumber].GetEndimonTypeText(Roster[EndimonNumber].GetEndimonType()));
        ERole.SetText(Roster[EndimonNumber].GetRole());
        EAtt.SetText("Attack: " + Roster[EndimonNumber].GetAttack().ToString());
        EDef.SetText("Defense: " + Roster[EndimonNumber].GetDefense().ToString());
        EHealth.SetText("Health: " + Roster[EndimonNumber].GetHealth().ToString());
        M1Name.SetText(Roster[EndimonNumber].GetEndimonMove1().GetMoveName());
        M2Name.SetText(Roster[EndimonNumber].GetEndimonMove2().GetMoveName());
        M3Name.SetText(Roster[EndimonNumber].GetEndimonMove3().GetMoveName());
        M1Dmg.SetText(Roster[EndimonNumber].GetEndimonMove1().GetDamage().ToString() + " Dmg");
        M2Dmg.SetText(Roster[EndimonNumber].GetEndimonMove2().GetDamage().ToString() + " Dmg");
        M3Desc.SetText(Roster[EndimonNumber].GetEndimonMove3().GetMoveDescription());

        //Check to see if there is a boosted move (if 0 nothing will update)
        if (Roster[EndimonNumber].GetEndimonMove1().GetDoesBoost())
        {
            M1Dmg.SetText(Roster[EndimonNumber].GetEndimonMove1().GetDamage().ToString() + " Dmg + 20");
        }
        else if (Roster[EndimonNumber].GetEndimonMove2().GetDoesBoost())
        {
            M2Dmg.SetText(Roster[EndimonNumber].GetEndimonMove2().GetDamage().ToString() + " Dmg + 20");
        }

        //Color Highlighting
        EType.color = SelectedEndimonColors[0];
        M3Name.color = SelectedEndimonColors[0];
        M1Name.color = SelectedEndimonColors[1];
        M2Name.color = SelectedEndimonColors[2];


        //Determine if you can select this Endimon using the button
        if (Player.FindEndimonOnTeam(Roster[EndimonNumber].GetName()))
        {
            SelectButton.interactable = false;
        }
        else
        {
            SelectButton.interactable = true;
        }
    }

    //Change the posistion of the pointer based upon the selection
    public void MovePointer(int xChange, int yChange)
    {
        ArrowMover.localPosition = new Vector3(xChange, yChange, 0);
    }
}
