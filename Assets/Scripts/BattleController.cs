using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DamageNumbersPro;

//Class is the main component in combat. It handles turn switching and all inputs and UI changes necessary
//It also handles all player actions as well as telling when the AI to do a turn
public class BattleController : MonoBehaviour
{
    //General information tied to ensuring a turn-based system
    private enum Turn { P1, P2, AI1, AI2 };    //Which Endimon turn is it?

    //What UI menu is the most recently opened for the player
    private enum Screen { Main, AttackMoveSelection, AttackTargetSelection, SwitchTeamSelection, SwitchFieldSelection, ItemSelection, ItemTargetSelection, Quit, None };   //What menu is the user in?
    private Turn ActiveTurn;                    //Whose turn it is
    private Screen ActiveScreen;                //The most recent screen in the UI opened
    private Character MainPlayer;               //The Player
    private AI MainAI;                          //The AI in the game
    private Endimon ActiveEndimon;              //The Endimon taking their turn at this very moment
    private int Selection;                      //The value to determine the option the player has selected in a menu
    private CameraController CamCont;           //The script to control the camera
    private string TurnIndicator = "Player";    //Variable helps indicate to camera cutscenes the phase of battle (different from turn/ values include "Player" "MidTurn" "AI")
    private AudioSource AudioPlayer;            //Audio component to play audio from

    //Cords for the highlighter placement, indexes are in this order: (0-3 = MainScreen | 4-6 = ObjectScreen | 7-8 = TargetScreen)
    private int[] XValues = { -24, 19, -24, 19, -34, -34, -34, -29, -29 };  //X Values for all places the  the highlighter will go
    private int[] YValues = { 6, 6, -26, -26, 19, -3, -25, 10, -16 };        //Y Values for all the places the highlighter will go 

    //Main Panels on the screen that will show/hide
    public GameObject MainScreenPanel;
    public GameObject ObjectScreenPanel;
    public GameObject TargetScreenPanel;
    public GameObject ControlsPanel;
    public GameObject P1EndimonPanel;
    public GameObject P2EndimonPanel;
    public GameObject AI1EndimonPanel;
    public GameObject AI2EndimonPanel;
    public GameObject StatusesPanel;
    public GameObject BattleTextPanel;
    public TextMeshProUGUI BattleText;

    //Components inside Menu Selection boxes including buttons, text, and highlighters
    public Button[] MainScreenButtons;
    public Button[] ObjectSelectionButtons;
    public Button[] TargetSelectionButtons;
    public TextMeshProUGUI ObjSelectionTitleText;
    public TextMeshProUGUI TargetSelectionTitleText;
    public TextMeshProUGUI MainMenuText;
    public TextMeshProUGUI[] ObjectSelectionText;
    public TextMeshProUGUI[] TargetSelectionText;
    public GameObject[] Highlighters;               //The highlighters for the menu boxes (Main|Selection|Target)
    private RectTransform[] HighlightMovers;        //The highlighters transforms that give it the ability to move

    //Components for the 4 Endimon boxes towards the top (0 & 1 are the human player index, 2 & 3 AI)
    public Image[] ActiveEndimonPictures;
    public TextMeshProUGUI[] ActiveEndimonNames;
    public TextMeshProUGUI[] ActiveEndimonHealth;

    //These two lists contain the status boxes for all Endimon as well as the global statuses. They should stay linked in this order
    //0-1 P1 | 2-3 P2 | 4-5 AI1 | 6-7 AI2 | 8-9 Globals
    public Image[] StatusesBoxes;
    public TextMeshProUGUI[] StatusesText;
    public Sprite[] BoxSprites; //10 Icons: DmgUp/DefUp/Heal/Confusion/Poison/Sleep/Paralyze/PyroG/FrostG/ShadowG

    //The 4 Endimon Models in the game, their preset locations, as well as their animatiors
    public GameObject[] ActiveEndimonLocations;
    private Models CreatedEndimonModels;
    public GameObject[] ActiveEndimonModels;
    private Animator[] EndimonAnims;

    //Arrays that hold the particle effects for all actions in the game (public as other functions may utilize these at will)
    //9 Effects (0-8): Pyro/Frost/Electro/Earth/Shadow/Hit/Dying/Active/New
    public ParticleSystem[] AttackEffectParticles;
    //12 Effects (0-11): UseItem/Sleep/Poison/Paralyze/Confuse/HealOT/HealOneTime/AttackUp/DefenseUp/Fire/Blizzard/Shadow Globals
    public ParticleSystem[] StatusEffectParticles;
    //8 Types (0-7): Pyro/Frost/Electro/Earth/Shadow/Healing/Poison/Blizzard/Normal
    public DamageNumber[] DmgNumbers;

    //Saved particles that will persist throughout the game (4 slots for if an effect is on each Endimon)
    private ParticleSystem ActiveTurnParticle;
    private ParticleSystem[] PositiveParticle;
    private ParticleSystem[] NegativeParticle;
    private ParticleSystem[] GlobalParticle;


    //Temporary Variables that store current Selections in the menus (Cleared on a new turn)
    private Endimon tempEndimon;
    private Item tempItem;
    private Move tempMove;
    private SpecialMove tempSpecialMove;
    private int tempSelection;  //Holds last selection of the user as an integer (Determines animation number)

    void Start()
    {
        //Obtain list of models for the game
        CreatedEndimonModels = GameObject.Find("Controller").GetComponent<Models>();

        //Grab the transforms of the highlighters
        HighlightMovers = new RectTransform[3];
        for (int i = 0; i < 3; i++)
        {
            HighlightMovers[i] = Highlighters[i].GetComponent<RectTransform>();
        }
        ObjectScreenPanel.SetActive(false);
        TargetScreenPanel.SetActive(false);

        //Will loop through and set button handlers to all buttons, each will know the selection number they should have
        for (int i = 0; i < MainScreenButtons.Length; i++)
        {
            int temp = i;   //Have to work around how delegate works so must use a temp variable to insert
            MainScreenButtons[i].onClick.AddListener(delegate { SelectionWasMade(temp); });
            if (i < ObjectSelectionButtons.Length)
            {
                ObjectSelectionButtons[i].onClick.AddListener(delegate { SelectionWasMade(temp); });
            }
            if (i < TargetSelectionButtons.Length)
            {
                TargetSelectionButtons[i].onClick.AddListener(delegate { SelectionWasMade(temp); });
            }
        }

        //Set the Endimon on the field (Defaults to the first 2 on each team)
        MainPlayer = GameProfile.GetCurrentCharacter(); //Make a copy of the main player in the game to reference
        MainAI = GameProfile.GetCurrentAI();            //Make a copy of the AI player

        //Set the starting Endimon, their turn status, and which box in the UI they will be in
        MainPlayer.SetActiveEndimon1(0);
        MainPlayer.SetActiveEndimon2(1);
        MainAI.SetActiveEndimon1(0);
        MainAI.SetActiveEndimon2(1);
        MainPlayer.GetActiveEndimon1().SetTurnStatus(false);
        MainPlayer.GetActiveEndimon2().SetTurnStatus(false);
        MainAI.GetActiveEndimon1().SetTurnStatus(false);
        MainAI.GetActiveEndimon2().SetTurnStatus(false);
        MainPlayer.GetActiveEndimon1().SetActiveNumber(0);
        MainPlayer.GetActiveEndimon2().SetActiveNumber(1);
        MainAI.GetActiveEndimon1().SetActiveNumber(2);
        MainAI.GetActiveEndimon2().SetActiveNumber(3);

        //Inser these models into the positions of the set gameobjects
        ActiveEndimonModels = new GameObject[4];
        ActiveEndimonModels[0] = InsertModel(MainPlayer.GetActiveEndimon1().GetModelNumber(), ActiveEndimonLocations[0]);
        ActiveEndimonModels[1] = InsertModel(MainPlayer.GetActiveEndimon2().GetModelNumber(), ActiveEndimonLocations[1]);
        ActiveEndimonModels[2] = InsertModel(MainAI.GetActiveEndimon1().GetModelNumber(), ActiveEndimonLocations[2]);
        ActiveEndimonModels[3] = InsertModel(MainAI.GetActiveEndimon2().GetModelNumber(), ActiveEndimonLocations[3]);

        //Insert the Effects into the arrays
        AttackEffectParticles = new ParticleSystem[9];
        StatusEffectParticles = new ParticleSystem[12];

        BoxSprites = new Sprite[10];

        //Load up effects & Icons
        AttackEffectParticles[0] = Resources.Load("ParticleEffects/FireHit", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[1] = Resources.Load("ParticleEffects/FrostHit", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[2] = Resources.Load("ParticleEffects/ElectroHit", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[3] = Resources.Load("ParticleEffects/EarthHit", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[4] = Resources.Load("ParticleEffects/ShadowHit", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[5] = Resources.Load("ParticleEffects/EndimonHit", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[6] = Resources.Load("ParticleEffects/EndimonDied", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[7] = Resources.Load("ParticleEffects/ActiveTurn", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[8] = Resources.Load("ParticleEffects/NewEndimon", typeof(ParticleSystem)) as ParticleSystem;

        StatusEffectParticles[0] = Resources.Load("ParticleEffects/UseItem", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[1] = Resources.Load("ParticleEffects/Sleeping", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[2] = Resources.Load("ParticleEffects/Poison", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[3] = Resources.Load("ParticleEffects/Paralyzed", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[4] = Resources.Load("ParticleEffects/Confusion", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[5] = Resources.Load("ParticleEffects/HealingOT", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[6] = Resources.Load("ParticleEffects/HealingOneTime", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[7] = Resources.Load("ParticleEffects/AttackUp", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[8] = Resources.Load("ParticleEffects/DefenseUp", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[9] = Resources.Load("ParticleEffects/FireGlobal", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[10] = Resources.Load("ParticleEffects/BlizzardGlobal", typeof(ParticleSystem)) as ParticleSystem;
        StatusEffectParticles[11] = Resources.Load("ParticleEffects/DarkGlobal", typeof(ParticleSystem)) as ParticleSystem;

        BoxSprites[0] = Resources.Load("StatusIcons/DamageUp", typeof(Sprite)) as Sprite;
        BoxSprites[1] = Resources.Load("StatusIcons/DefenseUp", typeof(Sprite)) as Sprite;
        BoxSprites[2] = Resources.Load("StatusIcons/Healing", typeof(Sprite)) as Sprite;
        BoxSprites[3] = Resources.Load("StatusIcons/Confusion", typeof(Sprite)) as Sprite;
        BoxSprites[4] = Resources.Load("StatusIcons/Poison", typeof(Sprite)) as Sprite;
        BoxSprites[5] = Resources.Load("StatusIcons/Sleep", typeof(Sprite)) as Sprite;
        BoxSprites[6] = Resources.Load("StatusIcons/Paralyze", typeof(Sprite)) as Sprite;
        BoxSprites[7] = Resources.Load("StatusIcons/FireGlobal", typeof(Sprite)) as Sprite;
        BoxSprites[8] = Resources.Load("StatusIcons/BlizzardGlobal", typeof(Sprite)) as Sprite;
        BoxSprites[9] = Resources.Load("StatusIcons/ShadowGlobal", typeof(Sprite)) as Sprite;

        //Grab Animators for each active endimon in the game
        EndimonAnims = new Animator[4];
        for (int i = 0; i < 4; i++)
        {
            EndimonAnims[i] = ActiveEndimonModels[i].GetComponent<Animator>();
        }

        ActiveTurn = Turn.P1;                           //Start with the player's turn first
        ActiveEndimon = MainPlayer.GetActiveEndimon1(); //Player's first Endimon is the first to go
        ActiveScreen = Screen.Main;                     //Main menu should be the active screen
        Selection = 0;                                  //The user's selection on the menu
        MainMenuText.text = ActiveEndimon.GetName() + "'s turn";    //Change text to active Endimon's turn

        //Put the particle effect on the active Endimon (The player's first Endimon)
        ActiveTurnParticle = Instantiate(AttackEffectParticles[7], ActiveEndimonLocations[ActiveEndimon.GetActiveNumber()].transform);

        PositiveParticle = new ParticleSystem[4];
        NegativeParticle = new ParticleSystem[4];
        GlobalParticle = new ParticleSystem[2];

        //Fill in each Endimon on the fields values into the corresponding UI
        for (int i = 0; i < 2; i++)
        {
            ActiveEndimonPictures[i].sprite = MainPlayer.GetEndimon(i).GetEndimonLargeImage();
            ActiveEndimonNames[i].text = MainPlayer.GetEndimon(i).GetName();
            ActiveEndimonHealth[i].text = "HP: " + MainPlayer.GetEndimon(i).GetCurrentHP();
        }
        for (int i = 0; i < 2; i++)
        {
            //Boxes are 2 & 3 for AI so add an offset to account for this
            ActiveEndimonPictures[i + 2].sprite = MainAI.GetEndimon(i).GetEndimonLargeImage();
            ActiveEndimonNames[i + 2].text = MainAI.GetEndimon(i).GetName();
            ActiveEndimonHealth[i + 2].text = "HP: " + MainAI.GetEndimon(i).GetCurrentHP();
        }

        CamCont = GameObject.Find("MainCamera").GetComponent<CameraController>();
        AudioPlayer = GameObject.Find("MainCamera").GetComponent<AudioSource>();
    }

    //Cycles through checking for inputs or to see if the AI should take their turn
    void Update()
    {
        //Keep checking to see if the AI should have a turn yet
        if ((ActiveTurn == Turn.AI1 || ActiveTurn == Turn.AI2) && TurnIndicator == "AI")
        {
            ToggleBottomUI(false, false, false, false);
            int tempHealth1 = MainPlayer.GetActiveEndimon1().GetCurrentHP();
            int tempHealth2 = MainPlayer.GetActiveEndimon2().GetCurrentHP();
            int tempHealth3 = ActiveEndimon.GetCurrentHP(); //Confusion status

            CharacterSelectController.DifficultySelection diff = MainAI.GetAIDifficulty();
            ActiveEndimon.SetTurnStatus(true);
            if (diff == CharacterSelectController.DifficultySelection.Easy)
            {
                //Call easy function
                StartCoroutine(MainAI.DecidingActionEasy(this, ActiveEndimon, EndimonAnims, StatusesBoxes));
            }
            else if (diff == CharacterSelectController.DifficultySelection.Medium)
            {
                //Call medium function (Tossup between easy or hard)
                int rand = Random.Range(1, 10);
                if (rand > 5)
                {
                    StartCoroutine(MainAI.DecidingActionEasy(this, ActiveEndimon, EndimonAnims, StatusesBoxes));
                }
                else
                {
                    StartCoroutine(MainAI.DecidingActionHard(this, ActiveEndimon, EndimonAnims, StatusesBoxes));
                }
            }
            else
            {
                //Call hard function
                StartCoroutine(MainAI.DecidingActionHard(this, ActiveEndimon, EndimonAnims, StatusesBoxes));
            }
            TurnIndicator = "MidTurn";  //Make sure to stop the AI from taking multiple turns through update

            //Target 1 was hit, assign it to check status of health
            if (tempHealth1 != MainPlayer.GetActiveEndimon1().GetCurrentHP())
            {
                tempEndimon = MainPlayer.GetActiveEndimon1();
            }
            //Target 2 was hit
            else if (tempHealth2 != MainPlayer.GetActiveEndimon2().GetCurrentHP())
            {
                tempEndimon = MainPlayer.GetActiveEndimon1();
            }
            //A target was not attacked, no need to check for deaths
            else if (tempHealth3 != ActiveEndimon.GetCurrentHP())
            {
                tempEndimon = ActiveEndimon;
            }
            else
            {
                tempEndimon = null;
            }
        }

        //Otherwise, we are awaiting for user input
        else
        {
            //UP ARROW SHOULD MOVE THE CURRENT SELECTION UPWARDS IF ITS THE PLAYER'S TURN
            if (Input.GetKeyDown(KeyCode.UpArrow) && (ActiveTurn == Turn.P1 || ActiveTurn == Turn.P2))
            {
                //If on the main screen, this means we move up if we can, values are horizontal so jump by 2
                if (ActiveScreen == Screen.Main && Selection > 1)
                {
                    Selection = Selection - 2;

                }
                //For 3 options, we set the indexes from 0-2
                else if (ActiveScreen == Screen.AttackMoveSelection || ActiveScreen == Screen.ItemSelection)
                {
                    Selection--;
                    //If we are on a screen with 3 options, make sure it stays between 0-2
                    if (Selection < 0)
                    {
                        Selection = 2;
                    }
                }
                //Only 2 options 0-1
                else if (ActiveScreen == Screen.AttackTargetSelection || ActiveScreen == Screen.ItemTargetSelection || ActiveScreen == Screen.SwitchFieldSelection
                    || ActiveScreen == Screen.SwitchTeamSelection || ActiveScreen == Screen.Quit)
                {
                    Selection--;
                    if (Selection < 0)
                    {
                        Selection = 1;
                    }
                }
                UpdateHighlighter();
            }

            //DOWN WILL MOVE THE SELECTION DOWN, ASSUMING IT IS THE PLAYER'S TURN
            if (Input.GetKeyDown(KeyCode.DownArrow) && (ActiveTurn == Turn.P1 || ActiveTurn == Turn.P2))
            {
                //If on the main screen, this means we move down if we can, values are horizontal so jump by 2
                if (ActiveScreen == Screen.Main && Selection < 2)
                {
                    Selection = Selection + 2;
                }
                //For 3 options, we set the indexes from 0-2
                else if (ActiveScreen == Screen.AttackMoveSelection || ActiveScreen == Screen.ItemSelection)
                {
                    Selection++;
                    if (Selection > 2)
                    {
                        Selection = 0;
                    }
                }
                //Only 2 options 0-1
                else if (ActiveScreen == Screen.AttackTargetSelection || ActiveScreen == Screen.ItemTargetSelection || ActiveScreen == Screen.SwitchFieldSelection
                    || ActiveScreen == Screen.SwitchTeamSelection || ActiveScreen == Screen.Quit)
                {
                    Selection++;
                    if (Selection > 1)
                    {
                        Selection = 0;
                    }
                }
                UpdateHighlighter();
            }

            //LEFT ARROW WILL MOVE SELECTION LEFT FOR THE MAIN MENU, ASSUMING IT CAN AND ITS THE PLAYER'S TURN
            if (Input.GetKeyDown(KeyCode.LeftArrow) && (ActiveTurn == Turn.P1 || ActiveTurn == Turn.P2))
            {
                if (ActiveScreen == Screen.Main)
                {
                    if (Selection == 1)
                    {
                        Selection = 0;
                    }
                    else if (Selection == 3)
                    {
                        Selection = 2;
                    }
                    UpdateHighlighter();
                }
            }

            //RIGHT ARROW WILL MOVE SELECTION RIGHT FOR THE MAIN MENU, ASSUMING IT CAN AND ITS THE PLAYER'S TURN
            if (Input.GetKeyDown(KeyCode.RightArrow) && (ActiveTurn == Turn.P1 || ActiveTurn == Turn.P2))
            {
                if (ActiveScreen == Screen.Main)
                {
                    if (Selection == 0)
                    {
                        Selection = 1;
                    }
                    else if (Selection == 2)
                    {
                        Selection = 3;
                    }
                    UpdateHighlighter();
                }
            }

            //SPACEBAR WILL REPRESENT A SELECTION IN THE MENUS, WE WILL ACCORDINGLY CALL THE FUNCTION TO HANDLE THE ACTION
            if (Input.GetKeyDown(KeyCode.Space))
            {           
                if (ActiveScreen == Screen.SwitchFieldSelection || ActiveScreen == Screen.ItemTargetSelection || ActiveScreen == Screen.AttackTargetSelection)
                {
                    AudioPlayer.PlayOneShot(Audio.ButtonClick, 5);
                }
                StartCoroutine(SelectionWasMade(Selection));
            }

            //BACKSPACE WILL TAKE YOU BACK ONE SCREEN ACCORDINGLY, ASSUMING YOU ARE NOT ON THE MAIN MENU
            if (Input.GetKeyDown(KeyCode.Backspace) && ActiveScreen != Screen.Main)
            {
                BackspacePressed();
            }
        }
    }

    //Function will handle if either the backspace was pressed or if a back button was clicked
    //Depending on the screen, the previously opened screen will appear and the previous selection will be deleted
    public void BackspacePressed()
    {
        AudioPlayer.PlayOneShot(Audio.ButtonCancel, 5);
        if (ActiveScreen == Screen.ItemSelection || ActiveScreen == Screen.AttackMoveSelection || ActiveScreen == Screen.SwitchTeamSelection || ActiveScreen == Screen.Quit)
        {
            ActiveScreen = Screen.Main;
            ResetAnySelections();
            Selection = 0;
            ObjectScreenPanel.SetActive(false);
            TargetScreenPanel.SetActive(false);
        }
        else if (ActiveScreen == Screen.AttackTargetSelection)
        {
            ActiveScreen = Screen.AttackMoveSelection;
            tempMove = null;
            Selection = 0;
            TargetScreenPanel.SetActive(false);
            ObjectScreenPanel.SetActive(true);
        }
        else if (ActiveScreen == Screen.ItemTargetSelection)
        {
            ActiveScreen = Screen.ItemSelection;
            tempItem = null;
            Selection = 0;
            TargetScreenPanel.SetActive(false);
            ObjectScreenPanel.SetActive(true);
        }
        else if (ActiveScreen == Screen.SwitchFieldSelection)
        {
            ActiveScreen = Screen.SwitchTeamSelection;
            tempEndimon = null;
            Selection = 0;
            UpdateSelectionOptions();   //Selections will change as we reuse the same screen in this sequence of events
        }
        UpdateHighlighter();
    }

    //FOLLOWING are button presses that will direct the selection that was made to do what the button said to do

    public void AttackBtnPressed()
    {
        if (ActiveScreen == Screen.Main)
        {
            AudioPlayer.PlayOneShot(Audio.ButtonClick, 5);
            StartCoroutine(SelectionWasMade(0));
        }
    }

    public void ItemBtnPressed()
    {
        if (ActiveScreen == Screen.Main)
        {
            AudioPlayer.PlayOneShot(Audio.ButtonClick, 5);
            StartCoroutine(SelectionWasMade(2));
        }
    }

    public void SwapBtnPressed()
    {
        if (ActiveScreen == Screen.Main)
        {
            AudioPlayer.PlayOneShot(Audio.ButtonClick, 5);
            StartCoroutine(SelectionWasMade(1));
        }
    }

    public void GiveUpBtnPressed()
    {
        if (ActiveScreen == Screen.Main)
        {
            AudioPlayer.PlayOneShot(Audio.ButtonClick, 5);
            StartCoroutine(SelectionWasMade(3));
        }
    }

    public void ObjectBtn1Pressed()
    {
        if (ActiveScreen == Screen.ItemSelection || ActiveScreen == Screen.AttackMoveSelection)
        {
            AudioPlayer.PlayOneShot(Audio.ButtonClick, 5);
            StartCoroutine(SelectionWasMade(0));
        }
    }

    public void ObjectBtn2Pressed()
    {
        if (ActiveScreen == Screen.ItemSelection || ActiveScreen == Screen.AttackMoveSelection)
        {
            AudioPlayer.PlayOneShot(Audio.ButtonClick, 5);
            StartCoroutine(SelectionWasMade(1));
        }
    }

    public void ObjectBtn3Pressed()
    {
        if (ActiveScreen == Screen.ItemSelection || ActiveScreen == Screen.AttackMoveSelection)
        {
            AudioPlayer.PlayOneShot(Audio.ButtonClick, 5);
            StartCoroutine(SelectionWasMade(2));
        }
    }

    public void TargetBtn1Pressed()
    {
        if (ActiveScreen == Screen.ItemTargetSelection || ActiveScreen == Screen.AttackTargetSelection || ActiveScreen == Screen.SwitchFieldSelection
            || ActiveScreen == Screen.SwitchTeamSelection || ActiveScreen == Screen.Quit)
        {
            AudioPlayer.PlayOneShot(Audio.ButtonClick, 5);
            StartCoroutine(SelectionWasMade(0));
        }
    }

    public void TargetBtn2Pressed()
    {
        if (ActiveScreen == Screen.ItemTargetSelection || ActiveScreen == Screen.AttackTargetSelection || ActiveScreen == Screen.SwitchFieldSelection
            || ActiveScreen == Screen.SwitchTeamSelection || ActiveScreen == Screen.Quit)
        {
            AudioPlayer.PlayOneShot(Audio.ButtonClick, 5);
            StartCoroutine(SelectionWasMade(1));
        }
    }

    //Takes a new Endimon and inserts its current stats into a UI box of choice
    public void SwitchEndimonUI(Endimon EndimonToPutIn, int UIBox)
    {
        //Debug.Log("I decided on putting: " + EndimonToPutIn.GetName() + " in box number: " + UIBox);
        ActiveEndimonPictures[UIBox].sprite = EndimonToPutIn.GetEndimonLargeImage();
        ActiveEndimonNames[UIBox].text = EndimonToPutIn.GetName();
        ActiveEndimonHealth[UIBox].text = "HP: " + EndimonToPutIn.GetCurrentHP();
        Destroy(ActiveEndimonModels[EndimonToPutIn.GetActiveNumber()]);
        ActiveEndimonModels[EndimonToPutIn.GetActiveNumber()] =
            InsertModel(EndimonToPutIn.GetModelNumber(), ActiveEndimonLocations[EndimonToPutIn.GetActiveNumber()]);
        EndimonAnims[EndimonToPutIn.GetActiveNumber()] = ActiveEndimonModels[EndimonToPutIn.GetActiveNumber()].GetComponent<Animator>();
        Instantiate(AttackEffectParticles[8], ActiveEndimonLocations[EndimonToPutIn.GetActiveNumber()].transform.position, ActiveEndimonLocations[EndimonToPutIn.GetActiveNumber()].transform.rotation);
        AttachParticles(EndimonToPutIn);
        AudioSource.PlayClipAtPoint(Audio.SwapIn, GameObject.Find("MainCamera").transform.position);
        EndimonToPutIn.DecreaseStatusEffectTurns();
        UpdateStatusEffectBoxes(EndimonToPutIn, -1, false);
        UpdateHealthValues();
    }

    //Update the health values on the display for each Endimon
    public void UpdateHealthValues()
    {
        ActiveEndimonHealth[0].text = "HP: " + MainPlayer.GetActiveEndimon1().GetCurrentHP();
        ActiveEndimonHealth[1].text = "HP: " + MainPlayer.GetActiveEndimon2().GetCurrentHP();
        ActiveEndimonHealth[2].text = "HP: " + MainAI.GetActiveEndimon1().GetCurrentHP();
        ActiveEndimonHealth[3].text = "HP: " + MainAI.GetActiveEndimon2().GetCurrentHP();
    }

    //Function will take an Endimon and update all UI components involved with effects accordingly
    public void UpdateStatusEffectBoxes(Endimon e, int index, bool harmful)
    {
        int offset = 0; //Determine the added offset needed to reach the correct indexes in the status effect boxes
        if (e != null)
        {
            int[] turns = e.GetStatusTurns();

            if (e == MainPlayer.GetActiveEndimon1())
            {
                offset = 0;
            }
            else if (e == MainPlayer.GetActiveEndimon2())
            {
                offset = 2;
            }
            else if (e == MainAI.GetActiveEndimon1())
            {
                offset = 4;
            }
            else if (e == MainAI.GetActiveEndimon2())
            {
                offset = 6;
            }

            //If there are any turns left on the bonus effect, keep the box on 
            if (turns[0] > 0)
            {
                StatusesBoxes[offset].gameObject.SetActive(true);
                StatusesText[offset].gameObject.SetActive(true);
                if (index != -1 && !harmful)
                {
                    StatusesBoxes[offset].sprite = ConvertIndexToIcon(index);
                    StatusesBoxes[offset].color = new Color32(4, 127, 12, 255);
                }
                StatusesText[offset].text = turns[0].ToString();
            }

            //Otherwise, turn the box off
            else
            {
                StatusesBoxes[offset].gameObject.SetActive(false);
                StatusesText[offset].gameObject.SetActive(false);
                if (PositiveParticle[offset / 2] != null)
                {
                    PositiveParticle[offset / 2].Stop();
                    PositiveParticle[offset / 2].Clear();
                }
            }

            //If there are any turns left on the harmful effect, keep the box on 
            if (turns[1] > 0)
            {
                StatusesBoxes[offset + 1].gameObject.SetActive(true);
                StatusesText[offset + 1].gameObject.SetActive(true);
                if (index != -1 && harmful)
                {
                    StatusesBoxes[offset + 1].sprite = ConvertIndexToIcon(index);
                    StatusesBoxes[offset + 1].color = new Color32(174, 3, 17, 255);
                }
                StatusesText[offset + 1].text = turns[1].ToString();
            }

            //Otherwise, turn the box off
            else
            {
                StatusesBoxes[offset + 1].gameObject.SetActive(false);
                StatusesText[offset + 1].gameObject.SetActive(false);
                if (NegativeParticle[offset / 2] != null)
                {
                    NegativeParticle[offset / 2].Stop();
                    NegativeParticle[offset / 2].Clear();
                }
            }
        }
    }

    //Function ensures that any user selection that was in progress is reset
    public void ResetAnySelections()
    {
        tempMove = null;
        tempEndimon = null;
        tempItem = null;
        tempSpecialMove = null;
    }

    //Function will change the current turn order. This is called after every end turn unless we have gone through every turn in the rotation
    public void ChangeTurnOrder()
    {
        //If it was the player's turn, it should now be the AIs
        if (ActiveTurn == Turn.P1 || ActiveTurn == Turn.P2)
        {
            //If the first Endimon has not yet gone, it should go, otherwise the second should
            if (MainAI.GetActiveEndimon1().GetEndimonTurnTaken() == false)
            {
                //Debug.Log("AI1 turn");
                TurnIndicator = "AI";   //Allows the AI to take a single turn
                ActiveTurn = Turn.AI1;
                ActiveEndimon = MainAI.GetActiveEndimon1();
            }
            else if (MainAI.GetActiveEndimon2().GetEndimonTurnTaken() == false)
            {
                //Debug.Log("AI2 turn");
                TurnIndicator = "AI";   //Allows the AI to take a single turn
                ActiveTurn = Turn.AI2;
                ActiveEndimon = MainAI.GetActiveEndimon2();
            }
            else
            {
                //Debug.Log("No available turn for AI, giving it to player");
                ActiveTurn = Turn.AI1;
                ChangeTurnOrder();
            }

            if (ActiveEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Sleep)
            {
                TurnIndicator = "MidTurn";
                ActiveEndimon.SetTurnStatus(true);
                StartCoroutine(EndTurnStatuses());
            }
        }
        //Otherwise, the turn should be the players
        else
        {
            //Debug.Log("Was an AIs turn, now it'll be the players");
            Selection = 0;
            //If the first Endimon has not yet gone, it should go, otherwise the second should
            if (MainPlayer.GetActiveEndimon1().GetEndimonTurnTaken() == false)
            {
                //Debug.Log("P1's turn");
                ActiveTurn = Turn.P1;
                ActiveEndimon = MainPlayer.GetActiveEndimon1();
            }
            else if (MainPlayer.GetActiveEndimon2().GetEndimonTurnTaken() == false)
            {
                //Debug.Log("P2's turn");
                ActiveTurn = Turn.P2;
                ActiveEndimon = MainPlayer.GetActiveEndimon2();
            }
            else
            {
                //Debug.Log("No available turn for player, giving it to AI");
                ActiveTurn = Turn.P1;
                ChangeTurnOrder();
            }


            if (ActiveEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Sleep)
            {
                //Debug.Log("The Endimon that was going to get a turn is sleeping");
                ActiveEndimon.SetTurnStatus(true);
                StartCoroutine(EndTurnStatuses());
            }

            ActiveScreen = Screen.Main;
            Selection = 0;
            MainMenuText.text = ActiveEndimon.GetName();
            UpdateHighlighter();
        }
        ActiveTurnParticle.transform.position = ActiveEndimonLocations[ActiveEndimon.GetActiveNumber()].transform.position;
    }

    //THE FOLLOWING FUNCTIONS ARE TURN ENDING FUNCTIONS, EACH WILL CALL ONE ANOTHER IN ORDER TO KEEP DELAYS WORKING

    //In order to avoid Monobehaviors and incorporate Coroutines, the AI end turn will just call a routine or EndTurn here
    public void EndAITurn()
    {
        StartCoroutine(EndTurnStatuses());
    }

    //End turn will do all the necessary updating needed at the completion of a turn
    public IEnumerator EndTurnStatuses()
    {
        ActiveEndimon.SetTurnStatus(true);

        if (ActiveEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Sleep)
        {
            CameraController.SetGameStatus("Defending", ActiveEndimon);
            BattleTextPanel.SetActive(true);
            BattleText.text = BattleTextController.OvertimeEffectText(ActiveEndimon, Endimon.StatusEffects.Sleep, "");
            ToggleBottomUI(false, false, false, false);
            yield return new WaitForSeconds(1f);

            AudioSource.PlayClipAtPoint(Audio.Sleep, GameObject.Find("MainCamera").transform.position);

            //Waiting action to finish up then switching camera back
            yield return new WaitForSeconds(2.5f);
            BattleTextPanel.SetActive(false);
            CameraController.SetGameStatus("PlayerAwaitTurn", null);
        }

        //Overtime effect of Synthesis will heal an Endimon 15 health per turn its on
        if (ActiveEndimon.GetEndimonPostiveEffect() == Endimon.StatusEffects.Synthesis)
        {
            //Prepare to look at Endimon
            CameraController.SetGameStatus("Defending", ActiveEndimon);
            BattleTextPanel.SetActive(true);
            BattleText.text = BattleTextController.OvertimeEffectText(ActiveEndimon, Endimon.StatusEffects.Synthesis, "");
            ToggleBottomUI(false, false, false, false);
            yield return new WaitForSeconds(1f);

            //Do the healing
            AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
            SpawnText("Healing", 25, ActiveEndimon);
            ActiveEndimon.TakeDamage(-25);
            UpdateHealthValues();

            //Waiting action to finish up then switching camera back
            yield return new WaitForSeconds(3f);
            BattleTextPanel.SetActive(false);
            CameraController.SetGameStatus("PlayerAwaitTurn", null);
        }

        if (ActiveEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Poison)
        {
            CameraController.SetGameStatus("Defending", ActiveEndimon);
            BattleTextPanel.SetActive(true);
            BattleText.text = BattleTextController.OvertimeEffectText(ActiveEndimon, Endimon.StatusEffects.Poison, "");
            ToggleBottomUI(false, false, false, false);
            yield return new WaitForSeconds(1f);

            //Do the healing
            AudioSource.PlayClipAtPoint(Audio.Poison, GameObject.Find("MainCamera").transform.position);
            EndimonAnims[ActiveEndimon.GetActiveNumber()].Play(ActiveEndimon.GetAnimationName(3));
            SpawnText("Poison", 25, ActiveEndimon);
            ActiveEndimon.TakeDamage(25);
            ActiveEndimon.SetDefense(-10);
            UpdateHealthValues();

            //Waiting action to finish up then switching camera back
            yield return new WaitForSeconds(3f);
            BattleTextPanel.SetActive(false);
            CameraController.SetGameStatus("PlayerAwaitTurn", null);
        }

        //Overtime effect of blizzard will damage Endimon if its type is not "Frost"
        Color32 blue = new Color32(11, 199, 195, 255);
        if (ActiveEndimon.GetEndimonType() != Endimon.Endimontypes.Frost && (StatusesBoxes[8].color == blue || StatusesBoxes[9].color == blue))
        {
            //Prepare to look at Endimon
            CameraController.SetGameStatus("Defending", ActiveEndimon);
            ToggleBottomUI(false, false, false, false);
            BattleTextPanel.SetActive(true);
            BattleText.text = BattleTextController.OvertimeEffectText(ActiveEndimon, Endimon.StatusEffects.Nothing, "Blizzard");
            yield return new WaitForSeconds(1f);

            //Play animations and apply damage
            EndimonAnims[ActiveEndimon.GetActiveNumber()].Play(ActiveEndimon.GetAnimationName(3));  //Take damage animation
            AudioSource.PlayClipAtPoint(Audio.GlobalBlizzard, GameObject.Find("MainCamera").transform.position);
            SpawnText("Blizzard", 25, ActiveEndimon);
            ActiveEndimon.TakeDamage(25);
            UpdateHealthValues();

            //Waiting action to finish up then switching camera back
            yield return new WaitForSeconds(3f);
            BattleTextPanel.SetActive(false);
            CameraController.SetGameStatus("PlayerAwaitTurn", null);
        }
        StartCoroutine(EndTurnDeath());
    }

    //Function looks to see if the Endimon that was hit just died
    public IEnumerator EndTurnDeath()
    {
        Endimon deadE = null;
        int deadIndex = -1;
        Endimon newE = null;
        int OneOrTwo = -1;

        while ((MainPlayer.GetActiveEndimon1().GetCurrentHP() <= 0 && !MainPlayer.GetActiveEndimon1().GetDead()) || (MainPlayer.GetActiveEndimon2().GetCurrentHP() <= 0 && !MainPlayer.GetActiveEndimon2().GetDead())
            || (MainAI.GetActiveEndimon1().GetCurrentHP() <= 0 && !MainAI.GetActiveEndimon1().GetDead()) || (MainAI.GetActiveEndimon2().GetCurrentHP() <= 0 && !MainAI.GetActiveEndimon2().GetDead()))
        {
            //Figure out who specifically died
            if (MainPlayer.GetActiveEndimon1().GetCurrentHP() <= 0 && !MainPlayer.GetActiveEndimon1().GetDead())
            {
                MainPlayer.GetActiveEndimon1().SetDead();
                deadE = MainPlayer.GetActiveEndimon1();
                deadIndex = 0;
            }
            else if (MainPlayer.GetActiveEndimon2().GetCurrentHP() <= 0 && !MainPlayer.GetActiveEndimon2().GetDead())
            {
                MainPlayer.GetActiveEndimon2().SetDead();
                deadE = MainPlayer.GetActiveEndimon2();
                deadIndex = 1;
            }
            else if (MainAI.GetActiveEndimon1().GetCurrentHP() <= 0 && !MainAI.GetActiveEndimon1().GetDead())
            {
                MainAI.GetActiveEndimon1().SetDead();
                deadE = MainAI.GetActiveEndimon1();
                deadIndex = 2;
            }
            else if (!MainAI.GetActiveEndimon2().GetDead())
            {
                MainAI.GetActiveEndimon2().SetDead();
                deadE = MainAI.GetActiveEndimon2();
                deadIndex = 3;
            }

            if (deadE != null)
            {

                //Play the death animation for the Endimon that died
                CameraController.SetGameStatus("Defending", deadE);
                ToggleBottomUI(false, false, false, false);
                BattleTextPanel.SetActive(true);
                BattleText.text = BattleTextController.DeathText(deadE);
                yield return new WaitForSeconds(2f);

                if (deadE.GetActiveNumber() == 0 || deadE.GetActiveNumber() == 1)
                {
                    PlayParticleAtLocation(deadE, true, 6, 2f, 5f, 0);
                    OneOrTwo = MainPlayer.SwapEndimonOnDeath(deadE);
                }
                else
                {
                    PlayParticleAtLocation(deadE, true, 6, 2f, -5f, 0);
                    OneOrTwo = MainAI.SwapEndimonOnDeath(deadE);
                }
                AudioSource.PlayClipAtPoint(Audio.Death, GameObject.Find("MainCamera").transform.position);
                EndimonAnims[deadE.GetActiveNumber()].Play(deadE.GetAnimationName(4));

                yield return new WaitForSeconds(1f);

                //Basd upon the index who the one who died, figure out which slot has the new Endimon
                if (deadIndex == 0)
                {
                    newE = MainPlayer.GetActiveEndimon1();
                }
                else if (deadIndex == 1)
                {
                    newE = MainPlayer.GetActiveEndimon2();
                }
                else if (deadIndex == 2)
                {
                    newE = MainAI.GetActiveEndimon1();
                }
                else
                {
                    newE = MainAI.GetActiveEndimon2();
                }

                //Determine if we switched out an AI or player
                if (OneOrTwo == 1 || OneOrTwo == 2)
                {
                    //Switching out AI
                    if (deadIndex > 1)
                    {
                        OneOrTwo += 2;
                    }
                    SwitchEndimonUI(newE, OneOrTwo - 1);
                    EndimonAnims[newE.GetActiveNumber()] = ActiveEndimonModels[newE.GetActiveNumber()].GetComponent<Animator>();
                    BattleText.text = BattleTextController.SwappingText(deadE, newE);
                    yield return new WaitForSeconds(2.5f);
                }

                else
                {
                    if (deadIndex == 0)
                    {
                        P1EndimonPanel.SetActive(false);
                        ActiveEndimonModels[0].SetActive(false);
                        MainPlayer.GetActiveEndimon1().SetTurnStatus(true);
                    }
                    else if (deadIndex == 1)
                    {
                        P2EndimonPanel.SetActive(false);
                        ActiveEndimonModels[1].SetActive(false);
                        MainPlayer.GetActiveEndimon2().SetTurnStatus(true);
                    }
                    else if (deadIndex == 2)
                    {
                        AI1EndimonPanel.SetActive(false);
                        ActiveEndimonModels[2].SetActive(false);
                        MainAI.GetActiveEndimon1().SetTurnStatus(true);
                    }
                    else
                    {
                        AI2EndimonPanel.SetActive(false);
                        ActiveEndimonModels[3].SetActive(false);
                        MainAI.GetActiveEndimon2().SetTurnStatus(true);
                    }

                    if (PositiveParticle[deadE.GetActiveNumber()] != null)
                    {
                        PositiveParticle[deadE.GetActiveNumber()].Stop();
                        PositiveParticle[deadE.GetActiveNumber()].Clear();
                    }
                    if (NegativeParticle[deadE.GetActiveNumber()] != null)
                    {
                        NegativeParticle[deadE.GetActiveNumber()].Stop();
                        NegativeParticle[deadE.GetActiveNumber()].Clear();
                    }
                }
            }
        }
        //Reset cam
        CameraController.SetGameStatus("PlayerAwaitTurn", null);
        BattleTextPanel.SetActive(false);
        ToggleBottomUI(true, false, false, true);
        StartCoroutine(EndTurnGameStatus());
    }

    //Checked at the end of every turn, if one player has lost all 4 Endimon the game will end accordingly
    public IEnumerator EndTurnGameStatus()
    {
        //If team is fully dead, then the player loses
        if (!MainPlayer.IsTeamAlive())
        {
            ToggleBottomUI(false, false, false, false);
            CameraController.SetGameStatus("Loser", null);
            BattleTextPanel.SetActive(true);
            BattleText.text = BattleTextController.GameOverText(false);

            yield return new WaitForSeconds(4f);

            SceneManager.LoadScene("MainMenu");
        }

        //If the AIs team is defeated, then the player has won
        if (!MainAI.IsTeamAlive())
        {
            ToggleBottomUI(false, false, false, false);
            CameraController.SetGameStatus("Winner", null);
            BattleTextPanel.SetActive(true);
            BattleText.text = BattleTextController.GameOverText(true);
            if (GameProfile.PlayingACampaignBattle && GameProfile.HighestFinishedLevel == GameProfile.CurrentCampaignBattle)
            {
                GameProfile.BeatALevel();
            }

            yield return new WaitForSeconds(4f);

            SceneManager.LoadScene("MainMenu");
        }
        EndTurnSwitch();
    }

    //Function finalizes the ending turn process and will switch turns
    public void EndTurnSwitch()
    {
        //Debug.Log("Turn ended for Endimon: " + ActiveEndimon.GetName());
        ActiveScreen = Screen.None;                   //User is no longer able to make selections
        ActiveEndimon.DecreaseStatusEffectTurns();    //Subtract the duration of the effects on this Endimon
        UpdateStatusEffectBoxes(ActiveEndimon, -1, false);   //Update these values of effects on the UI
        ResetAnySelections();                         //Reset any selections made by the player/AI

        //See if we should end the round now (Has everyone gone?)
        
        //Debug.Log("Turn order after swap: " + MainPlayer.GetActiveEndimon1().GetEndimonTurnTaken() + "/" + MainPlayer.GetActiveEndimon2().GetEndimonTurnTaken()
        //    + "/" + MainAI.GetActiveEndimon1().GetEndimonTurnTaken() + "/" + MainAI.GetActiveEndimon2().GetEndimonTurnTaken());

        if (MainPlayer.GetActiveEndimon1().GetEndimonTurnTaken() && MainPlayer.GetActiveEndimon2().GetEndimonTurnTaken() &&
            MainAI.GetActiveEndimon1().GetEndimonTurnTaken() && MainAI.GetActiveEndimon2().GetEndimonTurnTaken())
        {
            EndRound();
        }

        //If not everyone has gone, determine the next turn
        else
        {
            ChangeTurnOrder();
        }
    }

    //Determined that each Endimon on the field has gotten the chance to have a turn, reset the turn order and begin again with player's first Endimon
    public void EndRound()
    {
        //First, reset all turns assuming that the Endimon on the field, as long they are not dead
        //A dead Endimon remains on the field if there is nothing to swap to, so we must not reset their turn
        if (MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon1())) { MainPlayer.GetActiveEndimon1().SetTurnStatus(false); }
        if (MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon2())) { MainPlayer.GetActiveEndimon2().SetTurnStatus(false); }
        if (MainAI.IsEndimonAlive(MainAI.GetActiveEndimon1())) { MainAI.GetActiveEndimon1().SetTurnStatus(false); }
        if (MainAI.IsEndimonAlive(MainAI.GetActiveEndimon2())) { MainAI.GetActiveEndimon2().SetTurnStatus(false); }

        UpdateGlobalEffects();

        //Start the turn order over, set the turn to the AI so that player goes first
        ActiveTurn = Turn.AI2;
        ChangeTurnOrder();
    }

    //THIS IS THE END OF TURN ENDING FUNCTIONS

    //Function will move the highlighter into the correct position based upon the active screen.
    public void UpdateHighlighter()
    {
        AudioPlayer.PlayOneShot(Audio.ButtonHover, 5);
        //Determine index of the highlighter to move, then match the array based upon the selection
        if (ActiveScreen == Screen.Main)
        {
            HighlightMovers[0].localPosition = new Vector3(XValues[Selection], YValues[Selection], 0);
        }
        else if (ActiveScreen == Screen.ItemSelection || ActiveScreen == Screen.AttackMoveSelection)
        {
            //+4 to reach the correct indexes
            HighlightMovers[1].localPosition = new Vector3(XValues[Selection + 4], YValues[Selection + 4], 0);
        }
        else if (ActiveScreen != Screen.None)
        {
            //+7 to reach the correct indexes
            HighlightMovers[2].localPosition = new Vector3(XValues[Selection + 7], YValues[Selection + 7], 0);
        }
    }

    //Function handles the casting of the elemental effects (Pyro, Frost, etc.)
    public void CastElementalEffectParticles(Move UsedMove, Endimon CurrentEndimon)
    {
        //Find out the effect this hit should be
        int effectIndex = -1;
        float abilityHeight = 1f;
        if (UsedMove.GetMoveType() == Endimon.Endimontypes.Pyro)
        {
            effectIndex = 0;
            AudioSource.PlayClipAtPoint(Audio.PyroAttack, GameObject.Find("MainCamera").transform.position);
        }
        else if (UsedMove.GetMoveType() == Endimon.Endimontypes.Frost)
        {
            effectIndex = 1;
            abilityHeight += 1.5f;
            AudioSource.PlayClipAtPoint(Audio.FrostAttack, GameObject.Find("MainCamera").transform.position);
        }
        else if (UsedMove.GetMoveType() == Endimon.Endimontypes.Electro)
        {
            effectIndex = 2;
            AudioSource.PlayClipAtPoint(Audio.ElectroAttack, GameObject.Find("MainCamera").transform.position);
            abilityHeight += 2f;
        }
        else if (UsedMove.GetMoveType() == Endimon.Endimontypes.Earth)
        {
            effectIndex = 3;
            AudioSource.PlayClipAtPoint(Audio.EarthAttack, GameObject.Find("MainCamera").transform.position);
        }
        else if (UsedMove.GetMoveType() == Endimon.Endimontypes.Shadow)
        {
            effectIndex = 4;
            abilityHeight += 3f;
            AudioSource.PlayClipAtPoint(Audio.ShadowAttack, GameObject.Find("MainCamera").transform.position);
        }

        if (effectIndex != -1)
        {
            if (CurrentEndimon.GetActiveNumber() == 0 || CurrentEndimon.GetActiveNumber() == 1)
            {
                PlayParticleAtLocation(CurrentEndimon, true, effectIndex, abilityHeight, 5f, 0);
            }
            else
            {
                PlayParticleAtLocation(CurrentEndimon, true, effectIndex, abilityHeight, -5f, 0);
            }
        }
    }

    //Function will create a status effect on a selected Endimon based upon the effect (Also returns the index of the particl effect)
    public int CastItemEffect(Item UsedItem, Endimon TargetEndimon)
    {
        float side = 0f;
        float height = 2f;
        float z = 0f;
        if (TargetEndimon.GetActiveNumber() == 0 || TargetEndimon.GetActiveNumber() == 1)
        {
            side = 2.5f;
        }
        else
        {
            side = -2.5f;
        }


        int itemIndex = 0;

        if (PositiveParticle[TargetEndimon.GetActiveNumber()] != null && UsedItem.GetUsabilityTeam())
        {
            PositiveParticle[TargetEndimon.GetActiveNumber()].Stop();
            PositiveParticle[TargetEndimon.GetActiveNumber()].Clear();
        }

        if (NegativeParticle[TargetEndimon.GetActiveNumber()] != null && !UsedItem.GetUsabilityTeam())
        {
            NegativeParticle[TargetEndimon.GetActiveNumber()].Stop();
            NegativeParticle[TargetEndimon.GetActiveNumber()].Clear();
        }

        if (UsedItem.GetItemName() == "Power-Up Candy")
        {
            itemIndex = 7;
        }
        else if (UsedItem.GetItemName() == "Bulk-Up Candy")
        {
            itemIndex = 8;
            z = -5f;
        }
        else if (UsedItem.GetItemName() == "Paralyze Candy")
        {
            itemIndex = 3;
        }
        else if (UsedItem.GetItemName() == "Poison Candy")
        {
            itemIndex = 2;
        }
        else if (UsedItem.GetItemName() == "Sleep Candy")
        {
            itemIndex = 1;
        }
        else if (UsedItem.GetItemName() == "Confusion Candy")
        {
            itemIndex = 4;
        }

        if (UsedItem.GetUsabilityTeam())
        {
            PositiveParticle[TargetEndimon.GetActiveNumber()] = PlayParticleAtLocation(TargetEndimon, false, itemIndex, height, side, z);
        }
        else
        {
            NegativeParticle[TargetEndimon.GetActiveNumber()] = PlayParticleAtLocation(TargetEndimon, false, itemIndex, height, side, z);
        }
        return itemIndex;
    }

    //After using a special ability, a particle effect will be attached accordingly if there is a status
    public void CastAbilityEffect(int particleIndex, Endimon defender, Endimon attacker)
    {
        float side = 0f;
        float height = 1.75f;
        float z = 0f;
        if (defender.GetActiveNumber() == 0 || defender.GetActiveNumber() == 1)
        {
            side = 2.5f;
            if (particleIndex == 8)
            {
                z = -5f;
            }
        }
        else
        {
            side = -2.5f;
            if (particleIndex == 8)
            {
                z = 5f;
            }
        }

        if (attacker.GetEndimonMove3().GetHarmful())
        {
            if (NegativeParticle[defender.GetActiveNumber()] != null)
            {
                NegativeParticle[defender.GetActiveNumber()].Stop();
                NegativeParticle[defender.GetActiveNumber()].Clear();
            }

            //Some Endimon miss attacks causing no particles to be necessary
            if (particleIndex > -1)
            {
                NegativeParticle[defender.GetActiveNumber()] = PlayParticleAtLocation(defender, false, particleIndex, height, side, z);
            }
        }
        else
        {
            //Index 6 is a instant cast heal, all other abilities should override what is already casted
            if (PositiveParticle[defender.GetActiveNumber()] != null && particleIndex != 6)
            {
                PositiveParticle[defender.GetActiveNumber()].Stop();
                PositiveParticle[defender.GetActiveNumber()].Clear();
            }

            //Ensure that this was a valid index in the first place
            if (particleIndex > -1)
            {
                PositiveParticle[defender.GetActiveNumber()] = PlayParticleAtLocation(defender, false, particleIndex, height, side, z);
            }
        }
    }

    //This function runs when an AI wants to cast an item effect (Return the particle index if an item was used)
    //In order to successfully place particle effects, the AI will call this to cast the item animation on itself and the Endimon the item is on
    public int FindLocationForItemParticle(Endimon UserEndimon, Endimon TargetEndimon, Item UsedItem)
    {
        if (UsedItem.GetItemDuration() == 0)
        {
            if (TargetEndimon.GetActiveNumber() == 0 || TargetEndimon.GetActiveNumber() == 1)
            {
                PlayParticleAtLocation(TargetEndimon, false, 6, 1.75f, 2.5f, 0);
            }
            else
            {
                PlayParticleAtLocation(TargetEndimon, false, 6, 1.75f, -2.5f, 0);
            }
        }
        else
        {
            return CastItemEffect(UsedItem, TargetEndimon);
        }
        return -1;
    }

    //Function used to display a particle effect at a specific Endimon location
    public ParticleSystem PlayParticleAtLocation(Endimon TargetEndimon, bool isAttackParticles, int particleIndex, float heightAdjustment, float sideAdjustment, float zAdjustment)
    {
        if (isAttackParticles)
        {
            Vector3 location = new Vector3(ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.x + sideAdjustment, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.y + heightAdjustment,
                ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.z + zAdjustment);
            return Instantiate(AttackEffectParticles[particleIndex], location, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.rotation);
        }
        else
        {
            Vector3 location = new Vector3(ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.x + sideAdjustment, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.y + heightAdjustment,
                ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.z);
            if (particleIndex == 8)
            {
                if (TargetEndimon.GetActiveNumber() == 0 || TargetEndimon.GetActiveNumber() == 1)
                {
                    location = new Vector3(location.x, location.y, location.z - 5f);
                }
                else
                {
                    location = new Vector3(location.x, location.y, location.z + 5f);
                }
            }
            return Instantiate(StatusEffectParticles[particleIndex], location, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.rotation);
        }
    }

    //Function is in charge of replacing particles and status icon stuff when an Endimon is put back in
    public void AttachParticles(Endimon e)
    {
        float side = 0;
        if (e.GetActiveNumber() == 0 || e.GetActiveNumber() == 1)
        {
            side = 2.5f;
        }
        else
        {
            side = -2.5f;
        }

        int[] turns = e.GetStatusTurns();
        int offset = -1;

        if (e == MainPlayer.GetActiveEndimon1())
        {
            offset = 0;
        }
        else if (e == MainPlayer.GetActiveEndimon2())
        {
            offset = 2;
        }
        else if (e == MainAI.GetActiveEndimon1())
        {
            offset = 4;
        }
        else if (e == MainAI.GetActiveEndimon2())
        {
            offset = 6;
        }

        if (e.GetEndimonPostiveEffect() != Endimon.StatusEffects.Nothing)
        {
            if (e.GetEndimonPostiveEffect() == Endimon.StatusEffects.AttackUp || e.GetEndimonPostiveEffect() == Endimon.StatusEffects.HeatUp || e.GetEndimonPostiveEffect() == Endimon.StatusEffects.Screech)
            {
                PositiveParticle[e.GetActiveNumber()] = PlayParticleAtLocation(ActiveEndimon, false, 7, 1.75f, side, 0);
                StatusesBoxes[offset].sprite = ConvertIndexToIcon(7);
            }
            else if (e.GetEndimonPostiveEffect() == Endimon.StatusEffects.DefenseUp || e.GetEndimonPostiveEffect() == Endimon.StatusEffects.IcicleBaracade || e.GetEndimonPostiveEffect() == Endimon.StatusEffects.Speedray)
            {
                float z = 0f;
                if (e.GetActiveNumber() == 0 || e.GetActiveNumber() == 1)
                {
                    z = -5f;
                }
                else
                {
                    z = 5f;
                }
                PositiveParticle[e.GetActiveNumber()] = PlayParticleAtLocation(ActiveEndimon, false, 8, 1.5f, side, z);
                StatusesBoxes[offset].sprite = ConvertIndexToIcon(8);
            }
            else if (e.GetEndimonPostiveEffect() == Endimon.StatusEffects.Synthesis)
            {
                PositiveParticle[e.GetActiveNumber()] = PlayParticleAtLocation(ActiveEndimon, false, 5, 1.5f, side, 0);
                StatusesBoxes[offset].sprite = ConvertIndexToIcon(5);
            }
            StatusesBoxes[offset].color = new Color32(4, 127, 12, 255);
        }

        if (e.GetEndimonNegativeEffect() != Endimon.StatusEffects.Nothing)
        {
            if (e.GetEndimonNegativeEffect() == Endimon.StatusEffects.Paralyze)
            {
                NegativeParticle[e.GetActiveNumber()] = PlayParticleAtLocation(ActiveEndimon, false, 3, 1f, side, 0);
                StatusesBoxes[offset + 1].sprite = ConvertIndexToIcon(3);
            }
            else if (e.GetEndimonNegativeEffect() == Endimon.StatusEffects.Sleep)
            {
                NegativeParticle[e.GetActiveNumber()] = PlayParticleAtLocation(ActiveEndimon, false, 1, 1f, side, 0);
                StatusesBoxes[offset + 1].sprite = ConvertIndexToIcon(1);
            }
            else if (e.GetEndimonNegativeEffect() == Endimon.StatusEffects.Confusion)
            {
                NegativeParticle[e.GetActiveNumber()] = PlayParticleAtLocation(ActiveEndimon, false, 4, 1f, side, 0);
                StatusesBoxes[offset + 1].sprite = ConvertIndexToIcon(4);
            }
            else if (e.GetEndimonNegativeEffect() == Endimon.StatusEffects.Poison)
            {
                NegativeParticle[e.GetActiveNumber()] = PlayParticleAtLocation(ActiveEndimon, false, 2, 1f, side, 0);
                StatusesBoxes[offset + 1].sprite = ConvertIndexToIcon(2);
            }
            StatusesBoxes[offset + 1].color = new Color32(174, 3, 17, 255);
        }
    }

    //Spawn the bubble damage text based upon the type of text requested
    public void SpawnText(string typeOfText, int damage, Endimon target)
    {
        Vector3 location = new Vector3(ActiveEndimonLocations[target.GetActiveNumber()].transform.position.x, ActiveEndimonLocations[target.GetActiveNumber()].transform.position.y + 8.5f,
            ActiveEndimonLocations[target.GetActiveNumber()].transform.position.z);
        if (typeOfText == "Pyro")
        {
            DmgNumbers[0].Spawn(location, damage);
        }
        else if (typeOfText == "Frost")
        {
            DmgNumbers[1].Spawn(location, damage);
        }
        else if (typeOfText == "Electro")
        {
            DmgNumbers[2].Spawn(location, damage);
        }
        else if (typeOfText == "Earth")
        {
            DmgNumbers[3].Spawn(location, damage);
        }
        else if (typeOfText == "Shadow")
        {
            DmgNumbers[4].Spawn(location, damage);
        }
        else if (typeOfText == "Healing")
        {
            DmgNumbers[5].Spawn(location, damage);
        }
        else if (typeOfText == "Poison")
        {
            DmgNumbers[6].Spawn(location, damage);
        }
        else if (typeOfText == "Blizzard")
        {
            DmgNumbers[7].Spawn(location, damage);
        }
        else if (typeOfText == "Normal")
        {
            DmgNumbers[8].Spawn(location, damage);
        }
    }

    //Converts a partcle indext to the correct icon to match
    public Sprite ConvertIndexToIcon(int index)
    {
        if (index == 9)
        {
            return BoxSprites[7];
        }
        if (index == 10)
        {
            return BoxSprites[8];
        }
        if (index == 11)
        {
            return BoxSprites[9];
        }
        if (index == 1)
        {
            return BoxSprites[5];
        }
        if (index == 2)
        {
            return BoxSprites[4];
        }
        if (index == 3)
        {
            return BoxSprites[6];
        }
        if (index == 4)
        {
            return BoxSprites[3];
        }
        if (index == 5)
        {
            return BoxSprites[2];
        }
        if (index == 8)
        {
            return BoxSprites[1];
        }
        if (index == 7)
        {
            return BoxSprites[0];
        }
        return null;
    }

    //Adds a new global effect to the field, will replace the one in the first slot if 2 are already present
    public void AddGlobalEffect(int particleIndex)
    {
        StatusesPanel.SetActive(true);
        Vector3 location;
        Color32 boxColor;

        Quaternion rot = new Quaternion(0, 0, 0, 0);
        //This means there are already 2 global effects running, we need to get rid of the old version
        if (GlobalParticle[1] != null && GlobalParticle[0] != null)
        {
            GlobalParticle[0].Stop();
            GlobalParticle[0].Clear();
            GlobalParticle[0] = null;
        }

        //Figure out the location/color based upon the particle
        if (particleIndex == 10)
        {
            location = new Vector3(0, 75, 47);
            boxColor = new Color32(11, 199, 195, 255);
        }
        else if (particleIndex == 11)
        {
            location = new Vector3(0, 5, 42);
            boxColor = Color.white;
        }
        else
        {
            location = new Vector3(0, 15, 0);
            boxColor = new Color32(174, 3, 17, 255);
        }
        if (GlobalParticle[0] == null)
        {
            GlobalParticle[0] = Instantiate(StatusEffectParticles[particleIndex], location, rot);
            StatusesBoxes[8].gameObject.SetActive(true);
            StatusesText[8].gameObject.SetActive(true);
            StatusesText[8].text = "3";
            StatusesBoxes[8].color = boxColor;
            StatusesBoxes[8].sprite = ConvertIndexToIcon(particleIndex);
        }
        else
        {
            GlobalParticle[1] = Instantiate(StatusEffectParticles[particleIndex], location, rot);
            StatusesBoxes[9].gameObject.SetActive(true);
            StatusesText[9].gameObject.SetActive(true);
            StatusesText[9].text = "3";
            StatusesBoxes[9].color = boxColor;
            StatusesBoxes[9].sprite = ConvertIndexToIcon(particleIndex);
        }
    }

    //At the end of each round, the global effects will be reduced down by a turn and removed if necessary
    public void UpdateGlobalEffects()
    {
        int num;
        if (GlobalParticle[0] != null)
        {
            int.TryParse(StatusesText[8].text, out num);
            num--;
            StatusesText[8].text = num.ToString();
            if (num == 0)
            {
                StatusesBoxes[8].gameObject.SetActive(false);
                StatusesText[8].gameObject.SetActive(false);
                StatusesBoxes[8].sprite = null;
                StatusesBoxes[8].color = Color.white;
                GlobalParticle[0].Stop();
                GlobalParticle[0].Clear();
                GlobalParticle[0] = null;
            }
        }

        if (GlobalParticle[1] != null)
        {
            int.TryParse(StatusesText[9].text, out num);
            num--;
            StatusesText[9].text = num.ToString();
            if (num == 0)
            {
                StatusesBoxes[9].gameObject.SetActive(false);
                StatusesText[9].gameObject.SetActive(false);
                StatusesBoxes[9].sprite = null;
                StatusesBoxes[9].color = Color.white;
                GlobalParticle[1].Stop();
                GlobalParticle[1].Clear();
                GlobalParticle[1] = null;
            }
        }

        if (GlobalParticle[0] == null && GlobalParticle[1] == null)
        {
            StatusesPanel.SetActive(false);
        }
    }

    //Update the text values in the appropriate screen that is active
    public void UpdateSelectionOptions()
    {
        //Move screen will place all three move names in
        if (ActiveScreen == Screen.AttackMoveSelection)
        {
            Color32[] EColors = ActiveEndimon.GetEndimonTextColors();

            ObjSelectionTitleText.text = "Choose an attack";    //Change title text

            //Change all 3 options to the correct attack name and type color
            ObjectSelectionText[0].text = ActiveEndimon.GetEndimonMove1().GetMoveName();
            ObjectSelectionText[0].color = EColors[1];
            ObjectSelectionText[1].text = ActiveEndimon.GetEndimonMove2().GetMoveName();
            ObjectSelectionText[1].color = EColors[2];
            ObjectSelectionText[2].text = ActiveEndimon.GetEndimonMove3().GetMoveName();
            ObjectSelectionText[2].color = EColors[0];
        }

        //Determine the two targets based upon the move used. We will hide options if an Endimon is dead/not choosable
        else if (ActiveScreen == Screen.AttackTargetSelection)
        {
            TargetSelectionText[0].alpha = 255;
            TargetSelectionText[1].alpha = 255;
            TargetSelectionTitleText.text = "Use move on who?"; //Change title text

            //Clear out old text
            TargetSelectionText[0].text = "";
            TargetSelectionText[1].text = "";

            //Find out what the move is doing, does it harm an enemy or will it help an ally
            if (tempMove != null && tempMove.GetDoesDamage())
            {
                //USED A DAMAGE MOVE OR A MOVE THAT DOES DAMAGE
                TargetSelectionText[0].text = MainAI.GetActiveEndimon1().GetName();
                TargetSelectionText[0].color = MainAI.GetActiveEndimon1().GetPrimaryEndimonColor();
                TargetSelectionText[1].text = MainAI.GetActiveEndimon2().GetName();
                TargetSelectionText[1].color = MainAI.GetActiveEndimon2().GetPrimaryEndimonColor();

                //Find out if an Endimon on the field is dead (only 1 remaining Endimon)
                if (MainAI.GetActiveEndimon1().GetCurrentHP() <= 0)
                {
                    TargetSelectionText[0].alpha = 0;
                }

                if (MainAI.GetActiveEndimon2().GetCurrentHP() <= 0)
                {
                    TargetSelectionText[1].alpha = 0;
                }
            }

            else if (tempSpecialMove != null && tempSpecialMove.GetHarmful() == false)
            {
                //USED SPECIAL MOVE, IT'S FOR ALLIES SO TARGET PLAYER'S ENDIMON
                TargetSelectionText[0].text = MainPlayer.GetActiveEndimon1().GetName();
                TargetSelectionText[0].color = MainPlayer.GetActiveEndimon1().GetPrimaryEndimonColor();
                TargetSelectionText[1].text = MainPlayer.GetActiveEndimon2().GetName();
                TargetSelectionText[1].color = MainPlayer.GetActiveEndimon2().GetPrimaryEndimonColor();

                //Find out if an Endimon on the field is dead (only 1 remaining Endimon)
                if (MainPlayer.GetActiveEndimon1().GetCurrentHP() <= 0)
                {
                    TargetSelectionText[0].alpha = 0;
                }

                if (MainPlayer.GetActiveEndimon2().GetCurrentHP() <= 0)
                {
                    TargetSelectionText[1].alpha = 0;
                }
            }
            else if (tempSpecialMove != null && tempSpecialMove.GetHarmful())
            {
                //USED SPECIAL MOVE, ITS CASTED ON AN ENEMY SO TARGET THEIR ENDIMON
                TargetSelectionText[0].text = MainAI.GetActiveEndimon1().GetName();
                TargetSelectionText[0].color = MainAI.GetActiveEndimon1().GetPrimaryEndimonColor();
                TargetSelectionText[1].text = MainAI.GetActiveEndimon2().GetName();
                TargetSelectionText[1].color = MainAI.GetActiveEndimon2().GetPrimaryEndimonColor();

                //Find out if an Endimon on the field is dead (only 1 remaining Endimon)
                if (MainAI.GetActiveEndimon1().GetCurrentHP() <= 0)
                {
                    TargetSelectionText[0].alpha = 0;
                }

                if (MainAI.GetActiveEndimon2().GetCurrentHP() <= 0)
                {
                    TargetSelectionText[1].alpha = 0;
                }
            }
        }

        //Pops up the three items the user selected, will hide that text if an item is already used
        else if (ActiveScreen == Screen.ItemSelection)
        {
            ObjSelectionTitleText.text = "Choose an item to use";  //Change title text
            Item[] playerItems = MainPlayer.GetItems();
            //Change the text of the menus to the corresponding text and colors
            for (int i = 0; i < 3; i++)
            {
                if (playerItems[i] != null)
                {
                    ObjectSelectionText[i].color = Color.white;
                    ObjectSelectionText[i].text = playerItems[i].GetItemName();
                }
                else
                {
                    ObjectSelectionText[i].text = "";
                }
            }
        }

        //Determines which Endimon to show according to how the item works, hides all options that aren't choosable
        else if (ActiveScreen == Screen.ItemTargetSelection)
        {
            TargetSelectionTitleText.text = "Use item on who?"; //Change title text

            //Clear out text just in case
            TargetSelectionText[0].text = "";
            TargetSelectionText[1].text = "";

            //Input names of the active Endimon depending on if the item can be used on the player's team or not
            if (tempItem.GetUsabilityTeam())
            {
                if (MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon1()))
                {
                    TargetSelectionText[0].text = MainPlayer.GetActiveEndimon1().GetName();
                    TargetSelectionText[0].color = MainPlayer.GetActiveEndimon1().GetPrimaryEndimonColor();
                }
                if (MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon2()))
                {
                    TargetSelectionText[1].text = MainPlayer.GetActiveEndimon2().GetName();
                    TargetSelectionText[1].color = MainPlayer.GetActiveEndimon2().GetPrimaryEndimonColor();
                }
            }
            else
            {
                if (MainAI.IsEndimonAlive(MainAI.GetActiveEndimon1()))
                {
                    TargetSelectionText[0].text = MainAI.GetActiveEndimon1().GetName();
                    TargetSelectionText[0].color = MainAI.GetActiveEndimon1().GetPrimaryEndimonColor();
                }
                if (MainAI.IsEndimonAlive(MainAI.GetActiveEndimon2()))
                    TargetSelectionText[1].text = MainAI.GetActiveEndimon2().GetName();
                TargetSelectionText[1].color = MainAI.GetActiveEndimon2().GetPrimaryEndimonColor();
            }
        }

        //First figures out what Endimon are alive but not on the field, then displays them
        else if (ActiveScreen == Screen.SwitchTeamSelection)
        {
            TargetSelectionTitleText.text = "Select Endimon to switch in";  //Change title text

            //Find out which 2 Endimon are not on the field as those are the ones to switch in
            string E1 = MainPlayer.GetActiveEndimon1().GetName();
            string E2 = MainPlayer.GetActiveEndimon2().GetName();
            int slot = 0;   //Determines which target text slot to put the text in

            //Clear out the text first
            TargetSelectionText[0].text = "";
            TargetSelectionText[1].text = "";

            for (int i = 0; i < 4; i++)
            {
                if (E1 != MainPlayer.GetEndimon(i).GetName() && MainPlayer.GetEndimon(i).GetName() != E2 &&
                    MainPlayer.IsEndimonAlive(MainPlayer.GetEndimon(i)))
                {
                    TargetSelectionText[slot].text = MainPlayer.GetEndimon(i).GetName();
                    TargetSelectionText[slot].color = MainPlayer.GetEndimon(i).GetPrimaryEndimonColor();
                    slot++;
                }
            }
        }

        //Grabs the Endimon on the field and displays them, assuming they are alive
        else if (ActiveScreen == Screen.SwitchFieldSelection)
        {
            TargetSelectionTitleText.text = "Select Endimon to take out";   //Change title text

            //Clear out old text just in case
            TargetSelectionText[0].text = "";
            TargetSelectionText[1].text = "";
            //Change the target text the correct Endimon name and type color
            if (MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon1()))
            {
                TargetSelectionText[0].text = MainPlayer.GetActiveEndimon1().GetName();
                TargetSelectionText[0].color = MainPlayer.GetActiveEndimon1().GetPrimaryEndimonColor();
            }

            if (MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon2()))
            {
                TargetSelectionText[1].text = MainPlayer.GetActiveEndimon2().GetName();
                TargetSelectionText[1].color = MainPlayer.GetActiveEndimon2().GetPrimaryEndimonColor();
            }
        }

        //Bring up options to quit the game
        else if (ActiveScreen == Screen.Quit)
        {
            TargetSelectionTitleText.text = "Are you sure you want to give up?";
            TargetSelectionText[0].text = "No";
            TargetSelectionText[0].color = new Color32(174, 3, 17, 255);
            TargetSelectionText[1].text = "Yes";
            TargetSelectionText[1].color = new Color32(4, 127, 12, 255);
        }
    }

    //Function takes what screen is active and the selection, then determines the course of action to be taken
    //Typically this either pops open screens and waits for more input or takes all input and will handle the correct action
    public IEnumerator SelectionWasMade(int theSelection)
    {
        Selection = theSelection;   //Used for when a button is clicked
        //User was on the main screen when selecting, move to the correct next menu
        if (ActiveScreen == Screen.Main)
        {
            if (Selection == 0)
            {
                //Open the Attack Screen
                ActiveScreen = Screen.AttackMoveSelection;
                ObjectScreenPanel.SetActive(true);
            }
            else if (Selection == 1)
            {
                //Open team menu to choose an Endimon not on the field
                ActiveScreen = Screen.SwitchTeamSelection;
                TargetScreenPanel.SetActive(true);
            }
            else if (Selection == 2)
            {
                //Open Item Selection screen
                ActiveScreen = Screen.ItemSelection;
                ObjectScreenPanel.SetActive(true);
            }
            else if (Selection == 3)
            {
                //Open Forfeit screen
                ActiveScreen = Screen.Quit;
                TargetScreenPanel.SetActive(true);
            }
            Selection = 0;
            UpdateHighlighter();
            UpdateSelectionOptions();
        }

        //User was picking a move, so now bring up a menu to target or use the move
        //If a special move with no target was used, we will handle this accordingly here
        else if (ActiveScreen == Screen.AttackMoveSelection)
        {
            ActiveScreen = Screen.AttackTargetSelection;
            ObjectScreenPanel.SetActive(false);
            TargetScreenPanel.SetActive(true);
            if (Selection == 0)
            {
                tempMove = ActiveEndimon.GetEndimonMove1();
                tempSelection = Selection;
                Selection = 0;
                UpdateHighlighter();
                UpdateSelectionOptions();
            }
            else if (Selection == 1)
            {
                tempMove = ActiveEndimon.GetEndimonMove2();
                tempSelection = Selection;
                Selection = 0;
                UpdateHighlighter();
                UpdateSelectionOptions();
            }
            else if (Selection == 2)
            {
                tempSpecialMove = ActiveEndimon.GetEndimonMove3();
                //If this move can't hit anyone, then we need to deal with it now
                if (!tempSpecialMove.GetTargetable())
                {
                    ToggleBottomUI(false, false, false, false);
                    CameraController.SetGameStatus("Attacking", ActiveEndimon);
                    BattleTextPanel.SetActive(true);
                    BattleText.text = BattleTextController.SpecialAbilityText(ActiveEndimon, tempSpecialMove, tempEndimon);
                    yield return new WaitForSeconds(1.5f);

                    //Play cast animation and then change angles
                    EndimonAnims[ActiveEndimon.GetActiveNumber()].Play(ActiveEndimon.GetAnimationName(2));
                    yield return new WaitForSeconds(.5f);
                    CameraController.SetGameStatus("Globals", null);

                    //All non-target moves are globals, so we will be casting a global effect
                    int particleIndex = ActiveEndimon.UseSpecialMove(ActiveEndimon, null, ActiveEndimon.GetEndimonMove3(), this); //No target so null defender
                    AddGlobalEffect(particleIndex);
                    ActiveEndimon.SetTurnStatus(true);
                    BattleText.text = BattleTextController.GlobalText(tempSpecialMove.GetMoveName());
                    yield return new WaitForSeconds(3f);
                    CameraController.SetGameStatus("PlayerAwaitTurn", null);
                    BattleTextPanel.SetActive(false);
                    Selection = 0;
                    UpdateHighlighter();
                    StartCoroutine(EndTurnStatuses());
                }
                //Otherwise, we will act as if any type of move has been selected
                else
                {
                    tempSelection = Selection;
                    Selection = 0;
                    UpdateHighlighter();
                    UpdateSelectionOptions();
                }
            }
        }

        //User was selecting a target for the move, use the move now then go to the next turn
        //Check to see if the selection is valid, otherwise let the user try aagin 
        else if (ActiveScreen == Screen.AttackTargetSelection)
        {
            tempEndimon = null;
            if (Selection == 0)
            {
                if (((tempMove != null && tempMove.GetDoesDamage()) || (tempSpecialMove != null && tempSpecialMove.GetHarmful())) && MainAI.IsEndimonAlive(MainAI.GetActiveEndimon1()))
                {
                    tempEndimon = MainAI.GetActiveEndimon1();
                }
                else if (tempSpecialMove != null && tempSpecialMove.GetHarmful() == false && MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon1()))
                {
                    tempEndimon = MainPlayer.GetActiveEndimon1();
                }
                else
                {
                    yield return null; //Let the user try again since this was not a valid target
                }
            }
            else if (Selection == 1)
            {
                if (((tempMove != null && tempMove.GetDoesDamage()) || (tempSpecialMove != null && tempSpecialMove.GetHarmful())) && MainAI.IsEndimonAlive(MainAI.GetActiveEndimon2()))
                {
                    tempEndimon = MainAI.GetActiveEndimon2();
                }
                else if ((tempSpecialMove != null && tempSpecialMove.GetHarmful() == false) && MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon2()))
                {
                    tempEndimon = MainPlayer.GetActiveEndimon2();
                }
                else
                {
                    yield return null; //Let the user try again since this was not a valid target
                }
            }

            if (tempMove != null && tempEndimon != null)
            {
                ToggleBottomUI(false, false, false, false);
                CameraController.SetGameStatus("Attacking", ActiveEndimon);
                BattleTextPanel.SetActive(true);

                if (ActiveEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Confusion)
                {
                    //Roll a chance to see if Endimon will hit itself (30%)
                    int rand = Random.Range(1, 10);
                    if (rand > 7)
                    {
                        tempEndimon = ActiveEndimon;
                    }
                }

                BattleText.text = BattleTextController.AttackDamageText(ActiveEndimon, tempMove, tempEndimon);
                ActiveEndimon.SetTurnStatus(true);
                yield return new WaitForSeconds(1.5f);

                //Play animation of the move from both Endimon + Effect
                EndimonAnims[ActiveEndimon.GetActiveNumber()].Play(ActiveEndimon.GetAnimationName(tempSelection));
                CastElementalEffectParticles(tempMove, ActiveEndimon);

                //Switching camera angle
                yield return new WaitForSeconds(.5f);
                CameraController.SetGameStatus("Defending", tempEndimon);
                yield return new WaitForSeconds(.5f);

                //Damage & Damaged Animation
                int damage = ActiveEndimon.UseDamageMove(ActiveEndimon, tempMove, tempEndimon, StatusesBoxes, false);
                BattleText.text = BattleTextController.DefendDamageText(ActiveEndimon, tempMove, tempEndimon, damage, CheckShadowCastStatus());
                SpawnText(tempMove.GetMoveType().ToString(), damage, tempEndimon);
                bool died = tempEndimon.TakeDamage(damage);
                UpdateHealthValues();
                AudioSource.PlayClipAtPoint(Audio.BeenHit, GameObject.Find("MainCamera").transform.position);
                EndimonAnims[tempEndimon.GetActiveNumber()].Play(tempEndimon.GetAnimationName(3));

                //Determine which slot on the field the Endimon is hit to play the effect
                if (tempEndimon.GetActiveNumber() == 0 || tempEndimon.GetActiveNumber() == 1)
                {
                    PlayParticleAtLocation(tempEndimon, true, 5, 7f, 5f, 0);
                }
                else
                {
                    PlayParticleAtLocation(tempEndimon, true, 5, 7f, -5f, 0);
                }

                //Setting things back to normal
                yield return new WaitForSeconds(2f);
                BattleTextPanel.SetActive(false);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                StartCoroutine(EndTurnStatuses());
            }
            else if (tempSpecialMove != null && tempEndimon != null)
            {
                //Setting up for special move
                ToggleBottomUI(false, false, false, false);
                ActiveEndimon.SetTurnStatus(true);
                CameraController.SetGameStatus("Attacking", ActiveEndimon);
                BattleTextPanel.SetActive(true);
                BattleText.text = BattleTextController.SpecialAbilityText(ActiveEndimon, tempSpecialMove, tempEndimon);
                yield return new WaitForSeconds(1.5f);

                //Use animation
                EndimonAnims[ActiveEndimon.GetActiveNumber()].Play(ActiveEndimon.GetAnimationName(2));
                yield return new WaitForSeconds(.5f);
                CameraController.SetGameStatus("Defending", tempEndimon);
                yield return new WaitForSeconds(.5f);

                //Place particle effect onto the AI Endimon
                int particleIndex = ActiveEndimon.UseSpecialMove(ActiveEndimon, tempEndimon, ActiveEndimon.GetEndimonMove3(), this);
                if (particleIndex == -1)
                {
                    BattleText.text = "The attack failed";
                }
                CastAbilityEffect(particleIndex, tempEndimon, ActiveEndimon);

                yield return new WaitForSeconds(1.5f);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                BattleTextPanel.SetActive(false);
                UpdateHealthValues();
                UpdateStatusEffectBoxes(tempEndimon, particleIndex, ActiveEndimon.GetEndimonMove3().GetHarmful());
                StartCoroutine(EndTurnStatuses());
            }
        }

        //We must find the Endimon the user selected and make it the target. Since all we have is a string value, it must be connected
        else if (ActiveScreen == Screen.SwitchTeamSelection)
        {
            //We must find the endimon on the team that corresponds to their name, then assign it as the temp endimon to "target"
            string nameOfSelection = "";
            if (Selection == 0)
            {
                nameOfSelection = TargetSelectionText[0].text;
            }
            else if (Selection == 1)
            {
                nameOfSelection = TargetSelectionText[1].text;
            }

            //Search for this endimon on the team and grab it's endimon class
            for (int i = 0; i < 4; i++)
            {
                if (MainPlayer.GetEndimon(i).GetName() == nameOfSelection && MainPlayer.IsEndimonAlive(MainPlayer.GetEndimon(i)))
                {
                    tempEndimon = MainPlayer.GetEndimon(i);
                    break;
                }
            }

            if (tempEndimon != null)
            {
                ActiveScreen = Screen.SwitchFieldSelection;
                TargetScreenPanel.SetActive(true);
                Selection = 0;
                UpdateHighlighter();
                UpdateSelectionOptions();
            }
        }

        //User has picked the Endimon to take out, so now we switch in the held Endimon with the one they just picked
        //We must also make sure what option they just picked was an alive Endimon on the battlefield
        else if (ActiveScreen == Screen.SwitchFieldSelection)
        {
            if (Selection == 0 && MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon1()))
            {
                //Setting stage for switch
                ToggleBottomUI(false, false, false, false);
                CameraController.SetGameStatus("Attacking", MainPlayer.GetActiveEndimon1());
                ActiveEndimon.SetTurnStatus(true);
                BattleTextPanel.SetActive(true);
                BattleText.text = BattleTextController.SwappingText(MainPlayer.GetActiveEndimon1(), tempEndimon);
                yield return new WaitForSeconds(1.5f);

                //Doing the switch
                Endimon oldEndimon = MainPlayer.GetActiveEndimon1();
                MainPlayer.SwapEndimonOnTurn(1, tempEndimon);
                if (ActiveEndimon == oldEndimon)
                {
                    ActiveEndimon = tempEndimon;
                }
                SwitchEndimonUI(MainPlayer.GetActiveEndimon1(), MainPlayer.GetActiveEndimon1().GetActiveNumber());
                yield return new WaitForSeconds(1f);

                //Returning back
                BattleTextPanel.SetActive(false);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                StartCoroutine(EndTurnStatuses());
            }
            else if (Selection == 1 && MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon2()))
            {
                //Setting stage for switch
                ToggleBottomUI(false, false, false, false);
                CameraController.SetGameStatus("Attacking", MainPlayer.GetActiveEndimon2());
                ActiveEndimon.SetTurnStatus(true);
                BattleTextPanel.SetActive(true);
                BattleText.text = BattleTextController.SwappingText(MainPlayer.GetActiveEndimon2(), tempEndimon);
                yield return new WaitForSeconds(1.5f);

                //Doing the switch
                Endimon oldEndimon = MainPlayer.GetActiveEndimon2();
                MainPlayer.SwapEndimonOnTurn(2, tempEndimon);
                if (ActiveEndimon == oldEndimon)
                {
                    ActiveEndimon = tempEndimon;
                }
                SwitchEndimonUI(MainPlayer.GetActiveEndimon2(), MainPlayer.GetActiveEndimon2().GetActiveNumber());
                yield return new WaitForSeconds(1f);

                //Returning back
                BattleTextPanel.SetActive(false);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                StartCoroutine(EndTurnStatuses());
            }
        }

        //User selected an item, first make sure that the item wasn't already used, and if it wasn't keep hold of it
        else if (ActiveScreen == Screen.ItemSelection)
        {
            if (Selection == 0 || Selection == 1 || Selection == 2)
            {
                tempItem = MainPlayer.GetAnItem(Selection);
                if (tempItem != null)
                {
                    ActiveScreen = Screen.ItemTargetSelection;
                    TargetScreenPanel.SetActive(true);
                    ObjectScreenPanel.SetActive(false);
                    Selection = 0;
                    UpdateHighlighter();
                    UpdateSelectionOptions();
                }
            }
        }

        //User picked a target for this item to be used on, so now take the item and use its effect on the Endimon picked
        else if (ActiveScreen == Screen.ItemTargetSelection)
        {
            if (Selection == 0)
            {
                if (tempItem.GetUsabilityTeam() && MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon1()))
                {
                    tempEndimon = MainPlayer.GetActiveEndimon1();
                }
                else if (!tempItem.GetUsabilityTeam() && MainAI.IsEndimonAlive(MainAI.GetActiveEndimon1()))
                {
                    tempEndimon = MainAI.GetActiveEndimon1();
                }
                else
                {
                    yield return null; //Let user pick another target, this target was already dead
                }
            }
            else if (Selection == 1)
            {
                if (tempItem.GetUsabilityTeam() && MainPlayer.IsEndimonAlive(MainPlayer.GetActiveEndimon2()))
                {
                    tempEndimon = MainPlayer.GetActiveEndimon2();
                }
                else if (!tempItem.GetUsabilityTeam() && MainAI.IsEndimonAlive(MainAI.GetActiveEndimon2()))
                {
                    tempEndimon = MainAI.GetActiveEndimon2();
                }
                else
                {
                    yield return null; //Let user pick another target, this target was already dead
                }
            }
            //To keep full effect of an item you use on yourself, add an extra turn to it
            if (tempEndimon == ActiveEndimon && tempItem.GetUsabilityTeam() && tempItem.GetItemDuration() > 0)
            {
                tempItem.SetItemDuration(tempItem.GetItemDuration() + 1);
            }

            //Setting stage for item usage
            ToggleBottomUI(false, false, false, false);
            CameraController.SetGameStatus("Attacking", ActiveEndimon);
            BattleTextPanel.SetActive(true);
            BattleText.text = BattleTextController.ItemUsedText(ActiveEndimon, tempItem, tempEndimon);
            yield return new WaitForSeconds(1.5f);

            //Use Item particle effect + sound
            PlayParticleAtLocation(ActiveEndimon, false, 0, 2f, 3f, 0);
            AudioSource.PlayClipAtPoint(Audio.UseItem, GameObject.Find("MainCamera").transform.position);

            yield return new WaitForSeconds(.5f);
            //Transfer perspective to the target of said item
            CameraController.SetGameStatus("Defending", tempEndimon);
            yield return new WaitForSeconds(.5f);

            //Cast the effect's actual particles, healing if there is no duration. Otherwise, we will handle adding a new one now
            int particleIndex = -1;
            if (tempItem.GetItemDuration() == 0)
            {
                PlayParticleAtLocation(tempEndimon, false, 6, 1.5f, 2.5f, 0);
            }
            else
            {
                particleIndex = CastItemEffect(tempItem, tempEndimon);
            }

            MainPlayer.UseItem(tempItem, tempEndimon, this);
            MainPlayer.RemoveItem(tempItem);
            UpdateStatusEffectBoxes(tempEndimon, particleIndex, !tempItem.GetUsabilityTeam());
            yield return new WaitForSeconds(1.5f);

            BattleTextPanel.SetActive(false);
            CameraController.SetGameStatus("PlayerAwaitTurn", null);
            ActiveEndimon.SetTurnStatus(true);
            StartCoroutine(EndTurnStatuses());
        }

        //If the user is on the quit screen, see if they want to quit or not, if they don't just return them to the home page
        else if (ActiveScreen == Screen.Quit)
        {
            if (Selection == 0)
            {
                ActiveScreen = Screen.Main;
                ResetAnySelections();
                Selection = 0;
                TargetScreenPanel.SetActive(false);
                UpdateHighlighter();
            }
            else if (Selection == 1)
            {
                //*Display losing animation and other stuff to indicate the player lost
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    //Turns on/off the bottom half of the UI
    public void ToggleBottomUI(bool ActionPanel, bool ItemAttackPanel, bool TargetPanel, bool ContPanel)
    {
        MainScreenPanel.SetActive(ActionPanel);
        TargetScreenPanel.SetActive(TargetPanel);
        ObjectScreenPanel.SetActive(ItemAttackPanel);
        ControlsPanel.SetActive(ContPanel);
    }

    //Function will take a placeholder object and transform it to the corresponding model on the Endimon
    public GameObject InsertModel(int modelNum, GameObject obj)
    {
        if (modelNum == 1)
        {
            return Instantiate(CreatedEndimonModels.Corerosion, obj.transform.position, obj.transform.rotation);
        }
        else if (modelNum == 2)
        {
            return Instantiate(CreatedEndimonModels.Serpbolt, obj.transform.position, obj.transform.rotation);
        }
        else if (modelNum == 3)
        {
            return Instantiate(CreatedEndimonModels.Scorcher, obj.transform.position, obj.transform.rotation);
        }
        else if (modelNum == 4)
        {
            return Instantiate(CreatedEndimonModels.Snowshade, obj.transform.position, obj.transform.rotation);
        }
        else if (modelNum == 5)
        {
            return Instantiate(CreatedEndimonModels.Demonican, obj.transform.position, obj.transform.rotation);
        }
        else if (modelNum == 6)
        {
            return Instantiate(CreatedEndimonModels.Fruitfly, obj.transform.position, obj.transform.rotation);
        }
        else if (modelNum == 7)
        {
            return Instantiate(CreatedEndimonModels.Froghost, obj.transform.position, obj.transform.rotation);
        }
        else if (modelNum == 8)
        {
            return Instantiate(CreatedEndimonModels.Prickly, obj.transform.position, obj.transform.rotation);
        }
        else if (modelNum == 9)
        {
            return Instantiate(CreatedEndimonModels.Coalfire, obj.transform.position, obj.transform.rotation);
        }
        else if (modelNum == 10)
        {
            return Instantiate(CreatedEndimonModels.Zapcat, obj.transform.position, obj.transform.rotation);
        }
        return null;
    }

    //Function will allow anyone to set the Endimon they have targeted (This is necessary for checking the health of an Endimon
    public void SetTempEndimon(Endimon e)
    {
        tempEndimon = e;
    }

    //To check if the shadow cast aura is on the field, this method will be called (Helps figure out if attacks should/could miss)
    public bool CheckShadowCastStatus()
    {
        if (StatusesBoxes[8].color == new Color32(30, 30, 30, 255))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
