using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleController : MonoBehaviour
{
    //General information tied to ensuring a turn-based system
    private enum Turn { P1, P2, AI1, AI2 };                                  //Which Endimon turn is it?
    //What UI menu is the most recently opened for the player
    private enum Screen { Main, AttackMoveSelection, AttackTargetSelection, SwitchTeamSelection, SwitchFieldSelection, ItemSelection, ItemTargetSelection, Quit, None };   //What menu is the user in?
    private Turn ActiveTurn;
    private Screen ActiveScreen;
    private Character MainPlayer;
    private AI MainAI;
    private Endimon ActiveEndimon;              //The Endimon taking their turn at this very moment
    private bool ContinueGame;                  //Determine if the game should stop or not
    private int Selection;                      //The value to determine the option the player has selected in a menu
    private CameraController CamCont;           //The script to control the camera
    private string TurnIndicator = "Player";    //Variable helps indicate to camera cutscenes the phase of battle (different from turn/ values include "Player" "MidTurn" "AI")

    //Cords for the highlighter placement, indexes are in this order: (0-3 = MainScreen | 4-6 = ObjectScreen | 7-8 = TargetScreen)
    private int[] XValues = { -254, 222, -254, 222, -369, -369, -369, -314, -314 };  //X Values for all places the  the highlighter will go
    private int[] YValues = { 190, 190, -91, -91, 163, -34, -228, 95, -138 };        //Y Values for all the places the highlighter will go 

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
    private bool IsBottomUIActive;
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

    //The 4 Endimon Models in the game, their preset locations, as well as their animatiors
    public GameObject[] ActiveEndimonLocations;
    private Models CreatedEndimonModels;
    public GameObject[] ActiveEndimonModels;
    private Animator[] EndimonAnims;

    //Arrays that hold the particle effects for all actions in the game (public as other functions may utilize these at will)
    //9 Effects: Pyro/Frost/Electro/Nature/Shadow/Hit/Dying/Active/New
    public ParticleSystem[] AttackEffectParticles;
    //12 Effects: UseItem/Sleep/Poison/Paralyze/Confuse/HealOT/HealOneTime/AttackUp/DefenseUp/Fire/Blizzard/Shadow Globals
    public ParticleSystem[] StatusEffectParticles;

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
        IsBottomUIActive = false;

        //Will loop through and set button handlers to all buttons, each will know the selection number they should have
        for (int i = 0; i < MainScreenButtons.Length; i++)
        {
            int temp = i;   //Have to work around how delegate works so must use a temp variable to insert
            MainScreenButtons[i].onClick.AddListener(delegate { SelectionWasMade(temp); });
            if (i < ObjectSelectionButtons.Length) {
                ObjectSelectionButtons[i].onClick.AddListener(delegate { SelectionWasMade(temp); });
            }
            if (i < TargetSelectionButtons.Length) {
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

        AttackEffectParticles[0] = Resources.Load("ParticleEffects/FireHit", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[1] = Resources.Load("ParticleEffects/FrostHit", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[2] = Resources.Load("ParticleEffects/ElectroHit", typeof(ParticleSystem)) as ParticleSystem;
        AttackEffectParticles[3] = Resources.Load("ParticleEffects/NatureHit", typeof(ParticleSystem)) as ParticleSystem;
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

        //Grab Animators for each active endimon in the game
        EndimonAnims = new Animator[4];
        for (int i = 0; i < 4; i++)
        {
            EndimonAnims[i] = ActiveEndimonModels[i].GetComponent<Animator>();
        }

        ActiveTurn = Turn.P1;                           //Start with the player's turn first
        ActiveEndimon = MainPlayer.GetActiveEndimon1(); //Player's first Endimon is the first to go
        ActiveScreen = Screen.Main;                     //Main menu should be the active screen
        ContinueGame = true;                            //Only set to false when the game is over
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
            ActiveEndimonHealth[i].text = MainPlayer.GetEndimon(i).GetCurrentHP() + "/" + MainPlayer.GetEndimon(i).GetHealth();
        }
        for (int i = 0; i < 2; i++) {
            //Boxes are 2 & 3 for AI so add an offset to account for this
            ActiveEndimonPictures[i + 2].sprite = MainAI.GetEndimon(i).GetEndimonLargeImage();
            ActiveEndimonNames[i + 2].text = MainAI.GetEndimon(i).GetName();
            ActiveEndimonHealth[i + 2].text = MainAI.GetEndimon(i).GetCurrentHP() + "/" + MainAI.GetEndimon(i).GetHealth();
        }

        CamCont = GameObject.Find("MainCamera").GetComponent<CameraController>();
    }

    //Cycles through checking for inputs or to see if the AI should take their turn
    void Update()
    {
        //Keep checking to see if the AI should have a turn yet
        if ((ActiveTurn == Turn.AI1 || ActiveTurn == Turn.AI2) && TurnIndicator == "AI")
        {
            ToggleBottomUI(false, false, false, false);
            Debug.Log("AI now making a move");
            int tempHealth1 = MainPlayer.GetActiveEndimon1().GetCurrentHP();
            int tempHealth2 = MainPlayer.GetActiveEndimon2().GetCurrentHP();

            CharacterSelectController.DifficultySelection diff = MainAI.GetAIDifficulty();
            ActiveEndimon.SetTurnStatus(true);
            if (diff == CharacterSelectController.DifficultySelection.Easy)
            {
                //Call easy function
                StartCoroutine(MainAI.DecidingActionEasy(this, ActiveEndimon, EndimonAnims, StatusesBoxes));
            }
            else if (diff == CharacterSelectController.DifficultySelection.Medium)
            {
                //Call medium function
                StartCoroutine(MainAI.DecidingActionMedium(this, ActiveEndimon, EndimonAnims, StatusesBoxes));
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
                    AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
                    Selection = Selection - 2;

                }
                //For 3 options, we set the indexes from 0-2
                else if (ActiveScreen == Screen.AttackMoveSelection || ActiveScreen == Screen.ItemSelection)
                {
                    AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
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
                    AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
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
                    AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
                    Selection = Selection + 2;
                }
                //For 3 options, we set the indexes from 0-2
                else if (ActiveScreen == Screen.AttackMoveSelection || ActiveScreen == Screen.ItemSelection)
                {
                    AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
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
                    AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
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
                        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
                        Selection = 0;
                    }
                    else if (Selection == 3)
                    {
                        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
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
                        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
                        Selection = 1;
                    }
                    else if (Selection == 2)
                    {
                        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
                        Selection = 3;
                    }
                    UpdateHighlighter();
                }
            }

            //SPACEBAR WILL REPRESENT A SELECTION IN THE MENUS, WE WILL ACCORDINGLY CALL THE FUNCTION TO HANDLE THE ACTION
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(SelectionWasMade(Selection));
            }

            //BACKSPACE WILL TAKE YOU BACK ONE SCREEN ACCORDINGLY, ASSUMING YOU ARE NOT ON THE MAIN MENU
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                Debug.Log("Going back");
                BackspacePressed();
            }
        }
    }

    //Function will handle if either the backspace was pressed or if a back button was clicked
    //Depending on the screen, the previously opened screen will appear and the previous selection will be deleted
    public void BackspacePressed()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
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

    //Takes a new Endimon and inserts its current stats into a UI box of choice
    public void SwitchEndimonUI(Endimon EndimonToPutIn, int UIBox)
    {
        Debug.Log("I decided on putting: " + EndimonToPutIn.GetName() + " in box number: " + UIBox);
        ActiveEndimonPictures[UIBox].sprite = EndimonToPutIn.GetEndimonLargeImage();
        ActiveEndimonNames[UIBox].text = EndimonToPutIn.GetName();
        ActiveEndimonHealth[UIBox].text = EndimonToPutIn.GetCurrentHP() + "/" + EndimonToPutIn.GetHealth();
        Destroy(ActiveEndimonModels[EndimonToPutIn.GetActiveNumber()]);
        ActiveEndimonModels[EndimonToPutIn.GetActiveNumber()] =
            InsertModel(EndimonToPutIn.GetModelNumber(), ActiveEndimonLocations[EndimonToPutIn.GetActiveNumber()]);
        EndimonAnims[EndimonToPutIn.GetActiveNumber()] = ActiveEndimonModels[EndimonToPutIn.GetActiveNumber()].GetComponent<Animator>();
        Instantiate(AttackEffectParticles[8], ActiveEndimonLocations[EndimonToPutIn.GetActiveNumber()].transform.position, ActiveEndimonLocations[EndimonToPutIn.GetActiveNumber()].transform.rotation);
        AudioSource.PlayClipAtPoint(Audio.SwapIn, GameObject.Find("MainCamera").transform.position);
        UpdateStatusEffectBoxes(EndimonToPutIn);
        EndimonToPutIn.DecreaseStatusEffectTurns();
    }

    //Update the health values on the display for each Endimon
    public void UpdateHealthValues()
    {
        ActiveEndimonHealth[0].text = MainPlayer.GetActiveEndimon1().GetCurrentHP() + "/" + MainPlayer.GetActiveEndimon1().GetHealth();
        ActiveEndimonHealth[1].text = MainPlayer.GetActiveEndimon2().GetCurrentHP() + "/" + MainPlayer.GetActiveEndimon2().GetHealth();
        ActiveEndimonHealth[2].text = MainAI.GetActiveEndimon1().GetCurrentHP() + "/" + MainAI.GetActiveEndimon1().GetHealth();
        ActiveEndimonHealth[3].text = MainAI.GetActiveEndimon2().GetCurrentHP() + "/" + MainAI.GetActiveEndimon2().GetHealth();
    }

    //Function will take an Endimon and update all UI components involved with effects accordingly
    public void UpdateStatusEffectBoxes(Endimon e)
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
                Debug.Log("Effect: " + e.GetEndimonPostiveEffect() + " is still active for " + turns[0] + " turns");
                StatusesBoxes[offset].gameObject.SetActive(true);
                StatusesText[offset].gameObject.SetActive(true);
                StatusesBoxes[offset].color = new Color32(4, 127, 12, 255);
                StatusesText[offset].text = turns[0].ToString();
            }

            //Otherwise, turn the box off
            else
            {
                Debug.Log("Positive effect is now off/isn't present");
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
                Debug.Log("Effect: " + e.GetEndimonPostiveEffect() + " is still active for " + turns[1] + " turns");
                StatusesBoxes[offset + 1].gameObject.SetActive(true);
                StatusesText[offset + 1].gameObject.SetActive(true);
                StatusesBoxes[offset + 1].color = new Color32(174, 3, 17, 255);
                StatusesText[offset + 1].text = turns[1].ToString();
            }

            //Otherwise, turn the box off
            else
            {
                Debug.Log("Bad effect is now off/isn't present");
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
                Debug.Log("AI1 turn");
                TurnIndicator = "AI";   //Allows the AI to take a single turn
                ActiveTurn = Turn.AI1;
                ActiveEndimon = MainAI.GetActiveEndimon1();
            }
            else if (MainAI.GetActiveEndimon2().GetEndimonTurnTaken() == false)
            {
                Debug.Log("AI2 turn");
                TurnIndicator = "AI";   //Allows the AI to take a single turn
                ActiveTurn = Turn.AI2;
                ActiveEndimon = MainAI.GetActiveEndimon2();
            }
            else
            {
                Debug.Log("No available turn for AI, giving it to player");
                ActiveTurn = Turn.AI1;
                ChangeTurnOrder();
            }

            if (ActiveEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Sleep)
            {
                Debug.Log("The Endimon that was going to get a turn is sleeping");
                TurnIndicator = "MidTurn";
                ActiveEndimon.SetTurnStatus(true);
                StartCoroutine(EndTurnStatuses());
            }
        }
        //Otherwise, the turn should be the players
        else
        {
            Debug.Log("Was an AIs turn, now it'll be the players");
            Selection = 0;
            //If the first Endimon has not yet gone, it should go, otherwise the second should
            if (MainPlayer.GetActiveEndimon1().GetEndimonTurnTaken() == false)
            {
                Debug.Log("P1's turn");
                ActiveTurn = Turn.P1;
                ActiveEndimon = MainPlayer.GetActiveEndimon1();
            }
            else if (MainPlayer.GetActiveEndimon2().GetEndimonTurnTaken() == false)
            {
                Debug.Log("P2's turn");
                ActiveTurn = Turn.P2;
                ActiveEndimon = MainPlayer.GetActiveEndimon2();
            }
            else
            {
                Debug.Log("No available turn for player, giving it to AI");
                ActiveTurn = Turn.P1;
                ChangeTurnOrder();
            }


            if (ActiveEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Sleep)
            {
                Debug.Log("The Endimon that was going to get a turn is sleeping");
                ActiveEndimon.SetTurnStatus(true);
                StartCoroutine(EndTurnStatuses());
            }

            ActiveTurnParticle.transform.position = ActiveEndimonLocations[ActiveEndimon.GetActiveNumber()].transform.position;
            ActiveScreen = Screen.Main;
            Selection = 0;
            MainMenuText.text = ActiveEndimon.GetName();
            UpdateHighlighter();
        }
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
        Debug.Log("Ending turn");
        ActiveEndimon.SetTurnStatus(true);

        //*ADD HERE THAT THE ENDIMON YOU WERE LOOKING AT WAS SLEEPING!!!!!
        if(ActiveEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Sleep)
        {
            CameraController.SetGameStatus("Defending", ActiveEndimon);
            BattleTextPanel.SetActive(true);
            BattleText.text = BattleTextController.OvertimeEffectText(ActiveEndimon, Endimon.StatusEffects.Sleep, "");
            ToggleBottomUI(false, false, false, false);
            yield return new WaitForSeconds(1f);

            Debug.Log("Sleep status, turn skip");
            AudioSource.PlayClipAtPoint(Audio.Sleep, GameObject.Find("MainCamera").transform.position);

            //Waiting action to finish up then switching back
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
            Debug.Log("Synthesis heals for 15");
            AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
            ActiveEndimon.TakeDamage(-15);
            UpdateHealthValues();

            //Waiting action to finish up then switching back
            yield return new WaitForSeconds(2.5f);
            BattleTextPanel.SetActive(false);
            CameraController.SetGameStatus("PlayerAwaitTurn", null);
        }

        if(ActiveEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Poison)
        {
            CameraController.SetGameStatus("Defending", ActiveEndimon);
            BattleTextPanel.SetActive(true);
            BattleText.text = BattleTextController.OvertimeEffectText(ActiveEndimon, Endimon.StatusEffects.Poison, "");
            ToggleBottomUI(false, false, false, false);
            yield return new WaitForSeconds(1f);

            //Do the healing
            Debug.Log("Posion overtime effect dealing 20 dmg");
            AudioSource.PlayClipAtPoint(Audio.Poison, GameObject.Find("MainCamera").transform.position);
            EndimonAnims[ActiveEndimon.GetActiveNumber()].Play(ActiveEndimon.GetAnimationName(3));
            ActiveEndimon.TakeDamage(-20);
            ActiveEndimon.SetDefense(10);
            UpdateHealthValues();
            Debug.Log("New defense: " + ActiveEndimon.GetDefense());

            //Waiting action to finish up then switching back
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

            Debug.Log("Blizzard hit for 20");
            EndimonAnims[ActiveEndimon.GetActiveNumber()].Play(ActiveEndimon.GetAnimationName(3));  //Take damage animation
            AudioSource.PlayClipAtPoint(Audio.GlobalBlizzard, GameObject.Find("MainCamera").transform.position);
            ActiveEndimon.TakeDamage(20);
            UpdateHealthValues();

            //Waiting action to finish up then switching back

            yield return new WaitForSeconds(3f);
            BattleTextPanel.SetActive(false);
            CameraController.SetGameStatus("PlayerAwaitTurn", null);
        }
        StartCoroutine(EndTurnDeath());
    }

    //Function looks to see if the Endimon that was hit just died
    public IEnumerator EndTurnDeath()
    {
        if (tempEndimon != null && tempEndimon.GetCurrentHP() <= 0)
        {
            //Play the death animation for the Endimon that died
            CameraController.SetGameStatus("Defending", tempEndimon);
            ToggleBottomUI(false, false, false, false);
            BattleTextPanel.SetActive(true);
            BattleText.text = BattleTextController.DeathText(tempEndimon);
            yield return new WaitForSeconds(2f);

            if (tempEndimon.GetActiveNumber() == 0 || tempEndimon.GetActiveNumber() == 1)
            {
                PlayParticleAtLocation(tempEndimon, true, 6, 2f, 5f);
            }
            else
            {
                PlayParticleAtLocation(tempEndimon, true, 6, 2f, -5f);
            }
            AudioSource.PlayClipAtPoint(Audio.Death, GameObject.Find("MainCamera").transform.position);

            Debug.Log("Endimon that was hit is dead");
            EndimonAnims[tempEndimon.GetActiveNumber()].Play(tempEndimon.GetAnimationName(4));
            int OneOrTwo;

            yield return new WaitForSeconds(1f);

            //If it was the AI's turn, then a player's Endimon just died
            if (ActiveTurn == Turn.AI1 || ActiveTurn == Turn.AI2)
            {
                //Check which Endimon died, we will recieve back a slot in which to put a new Endimon in
                //Also update the model (when changing UI) and grab the new animator
                Endimon e = tempEndimon;    //Save the current Endimon to display in the textbox
                OneOrTwo = MainPlayer.SwapEndimonOnDeath(tempEndimon);
                if (OneOrTwo == 1)
                {
                    SwitchEndimonUI(MainPlayer.GetActiveEndimon1(), OneOrTwo - 1);
                    EndimonAnims[MainPlayer.GetActiveEndimon1().GetActiveNumber()] =
                        ActiveEndimonModels[MainPlayer.GetActiveEndimon1().GetActiveNumber()].GetComponent<Animator>();
                    BattleText.text = BattleTextController.SwappingText(e, MainPlayer.GetActiveEndimon1());
                    yield return new WaitForSeconds(2.5f);
                }
                else if (OneOrTwo == 2)
                {
                    SwitchEndimonUI(MainPlayer.GetActiveEndimon2(), OneOrTwo - 1);
                    EndimonAnims[MainPlayer.GetActiveEndimon2().GetActiveNumber()] =
                        ActiveEndimonModels[MainPlayer.GetActiveEndimon2().GetActiveNumber()].GetComponent<Animator>();
                    BattleText.text = BattleTextController.SwappingText(e, MainPlayer.GetActiveEndimon2());
                    yield return new WaitForSeconds(2.5f);
                }
                else
                {
                    Debug.Log("No Endimon left");
                    if (tempEndimon.GetName() == MainPlayer.GetActiveEndimon1().GetName())
                    {
                        P1EndimonPanel.SetActive(false);
                        ActiveEndimonModels[0].SetActive(false);
                        MainPlayer.GetActiveEndimon1().SetTurnStatus(true);
                    }
                    else
                    {
                        P2EndimonPanel.SetActive(false);
                        ActiveEndimonModels[1].SetActive(false);
                        MainPlayer.GetActiveEndimon2().SetTurnStatus(true);
                    }
                }
            }

            //Must have been the Player's turn and they killed an AI Endimon
            else
            {
                //Check which Endimon died, we will recieve back a slot in which to put a new Endimon in
                Endimon e = tempEndimon;
                OneOrTwo = MainAI.SwapEndimonOnDeath(tempEndimon);
                if (OneOrTwo == 1)
                {
                    SwitchEndimonUI(MainAI.GetActiveEndimon1(), OneOrTwo + 1);
                    EndimonAnims[MainAI.GetActiveEndimon1().GetActiveNumber()] =
                        ActiveEndimonModels[MainAI.GetActiveEndimon1().GetActiveNumber()].GetComponent<Animator>();
                    BattleText.text = BattleTextController.SwappingText(e, MainAI.GetActiveEndimon1());
                    yield return new WaitForSeconds(2.5f);
                }
                else if (OneOrTwo == 2)
                {
                    SwitchEndimonUI(MainAI.GetActiveEndimon2(), OneOrTwo + 1);
                    EndimonAnims[MainAI.GetActiveEndimon2().GetActiveNumber()] =
                        ActiveEndimonModels[MainAI.GetActiveEndimon2().GetActiveNumber()].GetComponent<Animator>();
                    BattleText.text = BattleTextController.SwappingText(e, MainAI.GetActiveEndimon2());
                    yield return new WaitForSeconds(2.5f);
                }
                else
                {
                    Debug.Log("No Endimon left");
                    if (tempEndimon.GetName() == MainAI.GetActiveEndimon1().GetName())
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
                }
            }
        }
        //Reset cam
        CameraController.SetGameStatus("PlayerAwaitTurn", tempEndimon);
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
            GameProfile.BeatALevel();

            yield return new WaitForSeconds(4f);

            SceneManager.LoadScene("MainMenu");
        }
        EndTurnSwitch();
    }

    //Function finalizes the ending turn process and will switch turns
    public void EndTurnSwitch()
    {
        Debug.Log("Turn ended for Endimon: " + ActiveEndimon.GetName());
        ActiveScreen = Screen.None;                   //User is no longer able to make selections
        ActiveEndimon.DecreaseStatusEffectTurns();    //Subtract the duration of the effects on this Endimon
        UpdateStatusEffectBoxes(ActiveEndimon);       //Update these values of effects on the UI
        ResetAnySelections();                         //Reset any selections made by the player/AI

        //See if we should end the round now (Has everyone gone?)
        Debug.Log("Turn order at the end of this turn: " + MainPlayer.GetActiveEndimon1().GetEndimonTurnTaken() + "/" + MainPlayer.GetActiveEndimon2().GetEndimonTurnTaken()
            + "/" + MainAI.GetActiveEndimon1().GetEndimonTurnTaken() + "/" + MainAI.GetActiveEndimon2().GetEndimonTurnTaken());

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
        Debug.Log("Ended round, order will reset now, all turns have been reset too");
        ChangeTurnOrder();
    }

    //THIS IS THE END OF TURN ENDING FUNCTIONS

    //Function will move the highlighter into the correct position based upon the active screen.
    public void UpdateHighlighter()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
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

    public void CastElementalEffect(Move UsedMove, Endimon CurrentEndimon)
    {
        //Find out the effect this hit should be
        int effectIndex = -1;
        if (UsedMove.GetMoveType() == Endimon.Endimontypes.Pyro) {
            effectIndex = 0;
            AudioSource.PlayClipAtPoint(Audio.PyroAttack, GameObject.Find("MainCamera").transform.position);
        }
        else if (UsedMove.GetMoveType() == Endimon.Endimontypes.Frost) {
            effectIndex = 1;
            AudioSource.PlayClipAtPoint(Audio.FrostAttack, GameObject.Find("MainCamera").transform.position);
        }
        else if (UsedMove.GetMoveType() == Endimon.Endimontypes.Electro) {
            effectIndex = 2;
            AudioSource.PlayClipAtPoint(Audio.ElectroAttack, GameObject.Find("MainCamera").transform.position);
        }
        else if (UsedMove.GetMoveType() == Endimon.Endimontypes.Earth) {
            effectIndex = 3;
            AudioSource.PlayClipAtPoint(Audio.EarthAttack, GameObject.Find("MainCamera").transform.position);
        }
        else if (UsedMove.GetMoveType() == Endimon.Endimontypes.Shadow) {
            effectIndex = 4;
            AudioSource.PlayClipAtPoint(Audio.ShadowAttack, GameObject.Find("MainCamera").transform.position);
        }

        if (effectIndex != -1)
        {
            if (CurrentEndimon.GetActiveNumber() == 0 || CurrentEndimon.GetActiveNumber() == 1)
            {
                PlayParticleAtLocation(CurrentEndimon, true, effectIndex, 1f, 5f);
            }
            else
            { 
                PlayParticleAtLocation(CurrentEndimon, true, effectIndex, 1f, -5f);
            }
        }
    }

    //Function will create a status effect on a selected Endimon based upon the effect
    public void CastItemEffect(Item UsedItem, Endimon TargetEndimon)
    {
        Vector3 location = new Vector3(ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.x,
                      ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.y + 1.75f, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.z);

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
            PositiveParticle[TargetEndimon.GetActiveNumber()] = Instantiate(StatusEffectParticles[itemIndex], location, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.rotation);
        }
        else
        {
            NegativeParticle[TargetEndimon.GetActiveNumber()] = Instantiate(StatusEffectParticles[itemIndex], location, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.rotation);
        }
    }

    //After using a special ability, a particle effect will be attached accordingly if there is a status
    public void CastAbilityEffect(int particleIndex, Endimon e)
    {
        Vector3 location= new Vector3(ActiveEndimonLocations[e.GetActiveNumber()].transform.position.x,
               ActiveEndimonLocations[e.GetActiveNumber()].transform.position.y + 1.75f, ActiveEndimonLocations[e.GetActiveNumber()].transform.position.z);

        if (e.GetEndimonMove3().GetHarmful())
        {
            if (NegativeParticle[e.GetActiveNumber()] != null)
            {
                NegativeParticle[e.GetActiveNumber()].Stop();
                NegativeParticle[e.GetActiveNumber()].Clear();
            }
            //Some Endimon miss attacks causing no particles to be necessary
            if (particleIndex > -1)
            {
                NegativeParticle[e.GetActiveNumber()] = Instantiate(StatusEffectParticles[particleIndex], location, ActiveEndimonLocations[e.GetActiveNumber()].transform.rotation);
            }
        }
        else
        {
            //Index 6 is a instant cast heal, all other abilities should override what is already casted
            if (PositiveParticle[e.GetActiveNumber()] != null && particleIndex != 6)
            {
                PositiveParticle[e.GetActiveNumber()].Stop();
                PositiveParticle[e.GetActiveNumber()].Clear();
            }
            //Ensure that this was a valid index in the first place
            if (particleIndex > 0)
            {
                PositiveParticle[e.GetActiveNumber()] = Instantiate(StatusEffectParticles[particleIndex], location, ActiveEndimonLocations[e.GetActiveNumber()].transform.rotation);
            }
        }
    }

    //This function runs when an AI wants to cast an item effect
    //In order to successfully place particle effects, the AI will call this to cast the item animation on itself and the Endimon the item is on
    public void FindLocationForItemParticle(Endimon UserEndimon, Endimon TargetEndimon, Item UsedItem)
    {
        if (UsedItem.GetItemDuration() == 0)
        {
            if (TargetEndimon.GetActiveNumber() == 0 || TargetEndimon.GetActiveNumber() == 1)
            {
                PlayParticleAtLocation(TargetEndimon, false, 6, 1.5f, 5f);
            }
            else
            {
                PlayParticleAtLocation(TargetEndimon, false, 6, 1.5f, -5f);
            }
        }
        else
        {
            CastItemEffect(UsedItem, TargetEndimon);
        }
    }

    //Function used to display a particle effect at a specific Endimon location
    public void PlayParticleAtLocation(Endimon TargetEndimon, bool isAttackParticles, int particleIndex, float heightAdjustment, float sideAdjustment)
    {
        if (isAttackParticles)
        {
            Vector3 location = new Vector3(ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.x + sideAdjustment, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.y + heightAdjustment,
                ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.z);
            Instantiate(AttackEffectParticles[particleIndex], location, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.rotation);
        }
        else
        {
            Vector3 location = new Vector3(ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.x, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.y + heightAdjustment,
                ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.position.z);
            Instantiate(StatusEffectParticles[particleIndex], location, ActiveEndimonLocations[TargetEndimon.GetActiveNumber()].transform.rotation);
        }
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
            location = new Vector3(0, 45, 40);
            boxColor = new Color32(11, 199, 195, 255);
        }
        else if (particleIndex == 11)
        {
            location = new Vector3(0, 5, 42);
            boxColor = new Color32(30, 30, 30, 255);
            rot = new Quaternion(-90, 0, 0, 0);
        }
        else
        {
            location = new Vector3(0, 15, -9);
            boxColor = new Color32(174, 3, 17, 255);
        }
        if (GlobalParticle[0] == null)
        {
            GlobalParticle[0] = Instantiate(StatusEffectParticles[particleIndex], location, rot);
            StatusesBoxes[8].gameObject.SetActive(true);
            StatusesText[8].gameObject.SetActive(true);
            StatusesText[8].text = "3";
            StatusesBoxes[8].color = boxColor;
            if (particleIndex == 11)
            {
                GlobalParticle[0].transform.Rotate(-90, 0, 0);
            }
        }
        else
        {
            GlobalParticle[1] = Instantiate(StatusEffectParticles[particleIndex], location, rot);
            StatusesBoxes[9].gameObject.SetActive(true);
            StatusesText[9].gameObject.SetActive(true);
            StatusesText[9].text = "3";
            StatusesBoxes[9].color = boxColor;
            if (particleIndex == 11)
            {
                GlobalParticle[1].transform.Rotate(-90, 0, 0);
            }
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
            else if (tempSpecialMove != null && tempSpecialMove.GetHarmful()) {
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
            AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
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
                AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
                tempSelection = Selection;
                Selection = 0;
                UpdateHighlighter();
                UpdateSelectionOptions();
            }
            else if (Selection == 1)
            {
                tempMove = ActiveEndimon.GetEndimonMove2();
                AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
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
                    AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);

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
                    int particleIndex = ActiveEndimon.UseSpecialMove(ActiveEndimon, null, ActiveEndimon.GetEndimonMove3()); //No target so null defender
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
                    AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
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
                Debug.Log("Player attacked with a damage move");
                CameraController.SetGameStatus("Attacking", ActiveEndimon);
                BattleTextPanel.SetActive(true);
                BattleText.text = BattleTextController.AttackDamageText(ActiveEndimon, tempMove, tempEndimon);
                ActiveEndimon.SetTurnStatus(true);
                yield return new WaitForSeconds(1.5f);

                //Play animation of the move from both Endimon + Effect
                EndimonAnims[ActiveEndimon.GetActiveNumber()].Play(ActiveEndimon.GetAnimationName(tempSelection));
                CastElementalEffect(tempMove, ActiveEndimon);

                //Switching camera angle
                yield return new WaitForSeconds(.5f);
                CameraController.SetGameStatus("Defending", tempEndimon);
                yield return new WaitForSeconds(.5f);

                //Damage & Damaged Animation
                int damage = ActiveEndimon.UseDamageMove(ActiveEndimon, tempMove, tempEndimon, StatusesBoxes);
                BattleText.text = BattleTextController.DefendDamageText(ActiveEndimon, tempMove, tempEndimon, damage, CheckShadowCastStatus());
                bool died = tempEndimon.TakeDamage(damage);
                UpdateHealthValues();
                AudioSource.PlayClipAtPoint(Audio.BeenHit, GameObject.Find("MainCamera").transform.position);
                EndimonAnims[tempEndimon.GetActiveNumber()].Play(tempEndimon.GetAnimationName(3));

                //Determine which slot on the field the Endimon is hit to play the effect
                if (tempEndimon.GetActiveNumber() == 0 || tempEndimon.GetActiveNumber() == 1)
                {
                    PlayParticleAtLocation(tempEndimon, true, 5, 7f, 5f);
                }
                else
                {
                    PlayParticleAtLocation(tempEndimon, true, 5, 7f, -5f);
                }

                //Setting things back to normal
                yield return new WaitForSeconds(1.5f);
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

                //Use animation and switch
                EndimonAnims[ActiveEndimon.GetActiveNumber()].Play(ActiveEndimon.GetAnimationName(2));
                yield return new WaitForSeconds(.5f);
                CameraController.SetGameStatus("Defending", tempEndimon);
                yield return new WaitForSeconds(.5f);

                //Place particle effect onto the AI Endimon
                int particleIndex = ActiveEndimon.UseSpecialMove(ActiveEndimon, tempEndimon, ActiveEndimon.GetEndimonMove3());
                CastAbilityEffect(particleIndex, tempEndimon);

                yield return new WaitForSeconds(1.5f);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                BattleTextPanel.SetActive(false);
                UpdateHealthValues();
                UpdateStatusEffectBoxes(tempEndimon);
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

            //If somehow the selected option returned an Endimon not on the team, let the user try again
            if (tempEndimon == null)
            {
                Debug.Log("Endimon was not selectable");
                yield return null;
            }
            else
            {
                AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
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
                MainPlayer.SwapEndimonOnTurn(1, tempEndimon);
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
                MainPlayer.SwapEndimonOnTurn(2, tempEndimon);
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
            if (Selection == 0 || Selection == 1 || Selection == 2) {
                tempItem = MainPlayer.GetAnItem(Selection);
                if (tempItem == null)
                {
                    yield return null; //Let the user pick again, this was not a valid item to use
                }
                AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
                ActiveScreen = Screen.ItemTargetSelection;
                TargetScreenPanel.SetActive(true);
                ObjectScreenPanel.SetActive(false);
                Selection = 0;
                UpdateHighlighter();
                UpdateSelectionOptions();
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
            PlayParticleAtLocation(ActiveEndimon, false, 0, 1f, 0f);
            AudioSource.PlayClipAtPoint(Audio.UseItem, GameObject.Find("MainCamera").transform.position);

            yield return new WaitForSeconds(.5f);
            //Transfer perspective to the target of said item
            CameraController.SetGameStatus("Defending", tempEndimon);
            yield return new WaitForSeconds(.5f);

            //Cast the effect's actual particles, healing if there is no duration. Otherwise, we will handle adding a new one now
            if (tempItem.GetItemDuration() == 0)
            {
                PlayParticleAtLocation(tempEndimon, false, 6, 1.5f, 0f);
            }
            else
            {
                CastItemEffect(tempItem, tempEndimon);
            }

            MainPlayer.UseItem(tempItem, tempEndimon);
            MainPlayer.RemoveItem(tempItem);
            UpdateStatusEffectBoxes(tempEndimon);
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
        Debug.Log("Model returned was null");
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

/*********************
BUGS
- 

LOOK OUT FOR
- Continue Adjusting timing of transistions for readability
- Hard AI Swapping problems (Maybe with textbox?)
- Anything out of the ordinary

PRIORITY OF ADDING THINGS
- Improve Battlefield Design
***********************/
