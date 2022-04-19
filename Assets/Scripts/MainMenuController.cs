using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//This class controls the functions of the Main Menu UI
//This should be the first screen the user sees upon loading the game (Fix GameProfile setup otherwise!)
public class MainMenuController : MonoBehaviour
{
    public GameObject HowToPlay;        //How to Play Screen
    public GameObject MainScreen;       //The main screen
    public GameObject Credits;          //Credits Screen
    public GameObject Arrow;            //Arrow pointer image
    private RectTransform ArrowMover;   //The transform of the pointer so it can be told to move
    private GameObject Screen;          //The screen
    private Image ScreenImage;          //The Image component of the screen

    private int[] XValues;      //Hold all x cords for the Arrow (Campaign->Custom->HTP->Credits) 
    private int[] YValues;      //Hold all y cords for the Arrow (Campaign->Custom->HTP->Credits) 

    //Following holds onto what the current selection of the user is (whatever is being hovered)
    private enum Selection { Campaign, Custom, HowTo, Credits };
    private Selection UserSelection;
    private const int NumberOfSelections = 4;   //The number of values in the Selection enum (Update as necessary)

    void Start()
    {
        //First runs the functions to create all the data in the game
        GameProfile.CreateRoster();
        GameProfile.CreateTrainers();
        GameProfile.CreateItems();
        GameProfile.SetHighestFinishedLevel(PlayerPrefs.GetInt("Campaign-Level"));
        if(GameProfile.GetHighestFinishedLevel() == 0)
        {
            GameProfile.SetHighestFinishedLevel(1);
        }
        GameProfile.SetPlayingACampaignBattle(false);

        //Values are given to arrays to tell the Arrow where to position
        YValues = new int[] { -7, -66, -129, -190 };

        UserSelection = Selection.Campaign;  //Default selection
        ArrowMover = Arrow.GetComponent<RectTransform>();
        Screen = GameObject.Find("MainPanel").gameObject;
        ScreenImage = Screen.GetComponent<Image>();
    }

    //Will scan for user inputs from the keyboard
    void Update()
    {

        //PRESSING DOWN MOVES ARROW DOWN, OR BACK TO TOP
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            UserSelection += 1;
            if ((int)UserSelection == NumberOfSelections)
            {
                UserSelection = Selection.Campaign;
            }
            ChangeHover();
        }

        //PRESSING UP MOVES SELECTION UP OR TO THE VERY BOTTOM
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            UserSelection -= 1;
            if (UserSelection < 0)
            {
                UserSelection = Selection.Credits;
            }
            ChangeHover();
        }

        //PRESSING ENTER WILL CLICK THE CURRENT SELECTION
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(UserSelection == Selection.Campaign)
            {
                CampaignButtonClicked();
            }
            else if(UserSelection == Selection.Custom)
            {
                CustomBattleButtonClicked();
            }
            else if(UserSelection == Selection.HowTo)
            {
                HowToPlayButtonClicked();
            }
            else if(UserSelection == Selection.Credits)
            {
                CreditsButtonsClicked();
            }
        }

        //PRESSING BACKSPACE WILL RETURN YOU OUT OF EITHER CREDITS OR HOW_TO_PLAY
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
            BackButtonPress();
        }

        //CHEATING BUTTON, PRESSING Z WILL ADD A LEVEL COMPLETED TO YOUR "ACCOUNT"
        if(Input.GetKeyDown(KeyCode.Z))
        {
            GameProfile.BeatALevel();
        }

        //CHEATING BUTTON, PRESSING X WILL DECREASE THE LEVEL YOU BEAT TO YOUR "ACCOUNT"
        if(Input.GetKeyDown(KeyCode.X))
        {
            GameProfile.HighestFinishedLevel--;
        }
    }

    //Load Campaign screen
    public void CampaignButtonClicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        SceneManager.LoadScene("CampaignMap");
    }

    //Load Character Selection screen
    public void CustomBattleButtonClicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        SceneManager.LoadScene("CharacterSelection");
    }

    //Changes text on screen to display basic rules
    public void HowToPlayButtonClicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        ScreenImage.color = new Color32(0, 0, 0, 210);
        HowToPlay.SetActive(true);
        MainScreen.SetActive(false);
    }

    //Changes text on screen to display the credits (for assets)
    public void CreditsButtonsClicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        ScreenImage.color = new Color32(0, 0, 0, 210);
        Credits.SetActive(true);
        MainScreen.SetActive(false);
    }

    //Called when a user presses an Arrow key to change selection, the function will accordingly call that button's hover event
    public void ChangeHover()
    {
        if (UserSelection == Selection.Campaign)
        {
            CampaignHovered();
        }
        else if (UserSelection == Selection.Custom)
        {
            CustomBattleHovered();
        }
        else if (UserSelection == Selection.HowTo)
        {
            HowToPlayHovered();
        }
        else if(UserSelection == Selection.Credits)
        {
            CreditsHovered();
        }
        else
        {
            Debug.Log("Error: UserSelection was not selected in ChangeHover");
        }
    }

    //If on a screen other than the main menu, display the main screen
    public void BackButtonPress()
    {
        HowToPlay.SetActive(false);
        Credits.SetActive(false);
        MainScreen.SetActive(true);
        ScreenImage.color = new Color32(0, 0, 0, 150);
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
    }

    //Set position of Arrow to Campaign
    public void CampaignHovered()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        UserSelection = Selection.Campaign;
        MovePointer(127, YValues[0]);
    }

    //Set position of Arrow to CB
    public void CustomBattleHovered()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        UserSelection = Selection.Custom;
        MovePointer(127, YValues[1]);
    }

    //Set position of Arrow to HTP
    public void HowToPlayHovered()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        UserSelection = Selection.HowTo;
        MovePointer(127, YValues[2]);
    }

    //Set position of Arrow to Credits
    public void CreditsHovered()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        UserSelection = Selection.Credits;
        MovePointer(127, YValues[3]);
    }

    //Change the posistion of the pointer based upon the selection
    public void MovePointer(int xChange, int yChange)
    {
        ArrowMover.localPosition = new Vector3(xChange, yChange, 0);
    }
}
