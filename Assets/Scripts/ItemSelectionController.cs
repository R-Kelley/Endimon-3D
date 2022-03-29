using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ItemSelectionController : MonoBehaviour
{
    private int ItemNumber;                     //1-8 Values Make sure to subtract when necessary
    private Item[] ListOfItems;                 //The copied list of items to display on screen
    private Item[] PlayerItemSelections;        //The selections made by the user
    private RectTransform HighlightMover;       //The transform object of the highlighter
    public GameObject Highlighter;              //The GO of the highlighter

    //UI components necessary to change and keep track of
    public TextMeshProUGUI ItemDescriptionText; 
    public Button[] Items;
    public Image[] ItemImages;
    public Image[] SelectedItemImages;
    public TextMeshProUGUI[] SelectedItems;
    public TextMeshProUGUI[] ItemTexts;
    public TextMeshProUGUI[] EndimonOnTeam;

    //Highlighter positions
    private int[] XValues = { 65 };
    private int[] YValues = { 38, 28, 17, 7, -5, -16, -27, -38 };

    void Start()
    {
        HighlightMover = Highlighter.GetComponent<RectTransform>();
        ItemNumber = 1; //The default selection
        ListOfItems = GameProfile.Items; //Grab a copy of the items
        PlayerItemSelections = new Item[3];

        //Insert listeners in each button so that it's easier to determine which button was clicked
        for(int i = 0; i < Items.Length; i++)
        {
            int temp = i;   //Have to work around how delegate works so must use a temp variable to insert
            Items[i].onClick.AddListener(delegate { ItemButtonClicked(temp); });
        }

        Endimon[] Endimon = GameProfile.CurrentCharacter.GetTeam(); //Grab copy of the player's team
        
        //Put the Endimon in each text box
        for (int i = 0; i < 4; i++)
        {
            EndimonOnTeam[i].text = Endimon[i].GetName();
            EndimonOnTeam[i].color = Endimon[i].GetPrimaryEndimonColor();   //Get the standard color of the Endimon
        }

        //Insert the items into the textboxes
        for (int i = 0; i < ListOfItems.Length; i++)
        {
            ItemImages[i].sprite = ListOfItems[i].GetItemImage();
            ItemTexts[i].text = ListOfItems[i].GetItemName();
        }
        UpdateUI();
    }

    //Check for user input
    void Update()
    {
        //UP ARROW WILL MOVE THE HIGHLIGHTER UP IN THE MENU OR MOVE IT TO THE VERY BOTTOM
        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            ItemNumber--;
            if(ItemNumber < 1)
            {
                ItemNumber = ListOfItems.Length;
            }
            ItemDescriptionText.text = ListOfItems[ItemNumber-1].GetItemDescription();
            MoveHighlighter();
        }
        
        //DOWN ARROW WILL MOVE THE HIGHLIGHTER DOWN IN THE MENU OR ALL THE WAY TO THE TOP
        if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            ItemNumber++;
            if (ItemNumber > ListOfItems.Length)
            {
                ItemNumber = 1;
            }
            ItemDescriptionText.text = ListOfItems[ItemNumber-1].GetItemDescription();
            MoveHighlighter();
        }

        //BACKSPACE WILL SUBTRACT THE MOST RECENT ITEM ADDED INTO THE MENU, OR PUT YOU BACK IN CHARACTER SELECT
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            int counter = GameProfile.GetCurrentCharacter().CheckPlayerItems();
            if (counter > 0)
            {
                GameProfile.GetCurrentCharacter().RemoveItem();
                UpdateUI();
            }
            else
            {
                BackButtonClicked();
            }
        }

        //RETURN BUTTON IS CLICKED TO PROCEED INTO BATTLE
        if(Input.GetKeyDown(KeyCode.Return))
        {
            ContinueBtnClicked();
        }

        //SPACE BUTTON WILL TAKE HOVERED OVER ITEM AND PUT IT INTO THE PLAYER'S LIST OF ITEMS
        if(Input.GetKeyDown(KeyCode.Space))
        {
            TakeItemBtnClicked();
        }
    }

    //Updates the screen to put the corrsponding item that was picked into a box on the screen
    //Also checks to see if any items were removed and acts accordingly
    public void UpdateUI()
    {
        Item[] CurrentItems = GameProfile.GetCurrentCharacter().GetItems();

        ItemDescriptionText.text = ListOfItems[ItemNumber-1].GetItemDescription();

        for(int i = 0; i < SelectedItems.Length; i++)
        {
            if(CurrentItems[i] != null)
            {
                SelectedItemImages[i].sprite = CurrentItems[i].GetItemImage();
                SelectedItemImages[i].color = Color.white;
                SelectedItems[i].text = CurrentItems[i].GetItemName();
            }
            else
            {
                SelectedItemImages[i].sprite = null;
                SelectedItemImages[i].color = new Color32(89, 88, 88, 255);
                SelectedItems[i].text = "";
            }
        }
    }
    //Updates the on screen description based upon the selected item
    void ItemButtonClicked(int ItemNum)
    {
        ItemNumber = ItemNum+1;   //Add a point because they are one off between the array and our specific labeling
        ItemDescriptionText.text = ListOfItems[ItemNumber-1].GetItemDescription();
        MoveHighlighter();
    }

    //Removes the first slotted item
    public void RemoveSelectedItem1()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
        GameProfile.GetCurrentCharacter().RemoveItem(0);
        UpdateUI();
    }

    //Removes the second slotted item
    public void RemoveSelectedItem2()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
        GameProfile.GetCurrentCharacter().RemoveItem(1);
        UpdateUI();
    }

    //Removes the third slotted item
    public void RemoveSelectedItem3()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
        GameProfile.GetCurrentCharacter().RemoveItem(2);
        UpdateUI();
    }

    //Proceed to the battle screen once done picking items
    public void ContinueBtnClicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        SceneManager.LoadScene("Battleground");
    }

    //Go back to character select
    public void BackButtonClicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonCancel, GameObject.Find("MainCamera").transform.position);
        SceneManager.LoadScene("CharacterSelection");
    }

    //Takes the hovered item and adds it to the users list of items in code (updates UI after)
    public void TakeItemBtnClicked()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonClick, GameObject.Find("MainCamera").transform.position);
        GameProfile.GetCurrentCharacter().AddItem(ListOfItems[ItemNumber-1]);
        UpdateUI();
    }

    //Moves the highlighter to the object it show be hovering over
    public void MoveHighlighter()
    {
        AudioSource.PlayClipAtPoint(Audio.ButtonHover, GameObject.Find("MainCamera").transform.position);
        HighlightMover.localPosition = new Vector3(XValues[0], YValues[ItemNumber - 1], 0);
    }
}
