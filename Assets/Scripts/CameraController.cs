using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is used to control the camera for the main combat scene
//The camera has multiple states that will either control it on its on or do something specific that the code requires of it
public class CameraController : MonoBehaviour
{
    public Camera MainCamera;           //Camera to move (assigned in editor)
    private static string GameStatus;   //The status of the game (Used to determine what it should be doing)
    private static float Timer;         //Used to identify camera switches if its in a free state
    private float Speed = 3.5f;         //Speed of movement
    private static bool ZoomedIn;       //Determines if the camera is currently zoomed in on a specific Endimon
    private int rand;                   //Random int
    private static Endimon MainEndimon; //Endimon that is being focused on

    //Location and Rotation of camera
    private Vector3 Location;
    private Quaternion Rotation;

    //Holds all Locations/Rotations of Endimon on the field
    private Vector3[] EndimonLocations;
    private Quaternion[] EndimonRotations;

    //Holds values for where it should relocate based upon certain actions
    private Vector3 PanLocationLeft;
    private Quaternion PanRotationLeft;
    private Vector3 PanLocationRight;
    private Quaternion PanRotationRight;
    private Vector3 PanLocationDown;
    private Quaternion PanRotationDown;
    private Vector3 GlobalLocation;

    
    void Start()
    {
        //Assign starting values
        GameStatus = "PlayerAwaitTurn";
        Timer = 8;
        ZoomedIn = true;    //Just for set-up purposes
        MainEndimon = null;

        PanLocationLeft = new Vector3(-10f, 40.1f, -35.2f);
        PanRotationLeft = new Quaternion(20, 0, 0, 0);
        PanLocationRight = new Vector3(27.7f, 40.1f, -35.2f);
        PanRotationRight = new Quaternion(20, 0, 0, 0);
        PanLocationDown = new Vector3(2.4f, 63.4f, 33f);
        PanRotationDown = new Quaternion(90, 0, 0, 0);

        GlobalLocation = new Vector3(.4f, 55f, -55f);

        EndimonLocations = new Vector3[4];
        EndimonRotations = new Quaternion[4];
        EndimonLocations[0] = new Vector3(-8.6f, 11.8f, 54.9f);
        EndimonRotations[0] = new Quaternion(10, -50, 0, 0);
        EndimonLocations[1] = new Vector3(-4.42f, 13.71f, 9.34f);
        EndimonRotations[1] = new Quaternion(10, -60, 0, 0);
        EndimonLocations[2] = new Vector3(0.49f, 16.19f, 14.74f);
        EndimonRotations[2] = new Quaternion(10, 70, 0, 0);
        EndimonLocations[3] = new Vector3(3.64f, 14.68f, 59.6f);
        EndimonRotations[3] = new Quaternion(10, 70, 0, 0);
    }

    //Keep the camera moving or in the spot that it should be based upon the parameters
    void Update()
    {   
        //Zoom in on a random Endimon when the timer hits 0, otherwise choose a way to pan the screen
        if (GameStatus == "PlayerAwaitTurn" && Timer > 0)
        {
            if(ZoomedIn == true)
            {
                rand = Random.Range(0, 3);
                if (rand == 0)
                {
                    MainCamera.transform.position = PanLocationLeft;
                    MainCamera.transform.rotation = Quaternion.Euler(PanRotationLeft.x, PanRotationLeft.y, PanRotationLeft.z);
                }
                else if(rand == 1)
                {
                    MainCamera.transform.position = PanLocationRight;
                    MainCamera.transform.rotation = Quaternion.Euler(PanRotationRight.x, PanRotationRight.y, PanRotationRight.z);
                }
                else
                {
                    MainCamera.transform.position = PanLocationDown;
                    MainCamera.transform.rotation = Quaternion.Euler(PanRotationDown.x, PanRotationDown.y, PanRotationDown.z);
                }
                ZoomedIn = false;
                    
            }

            //Choose the direction of the camera transform, corresponds with pan location
            else
            {
                if(rand == 0 )
                {
                    MainCamera.transform.Translate(Vector3.right * Speed * Time.deltaTime);
                }
                else if(rand == 1)
                {
                    MainCamera.transform.Translate(Vector3.left * Speed * Time.deltaTime);
                }
                else
                {
                    MainCamera.transform.Translate(Vector3.up * Speed * Time.deltaTime);
                }
            }
        }

        //When the player wins, zoom in on the Endimon they have on the field
        else if(GameStatus == "Winner")
        {
            MainCamera.transform.position = new Vector3(12.84f, 26.17f, 47f);
            MainCamera.transform.rotation = Quaternion.Euler(20, -90, 0);
        }
        //When a player loses, zoom in on the AI Endimon on the field
        else if(GameStatus == "Loser")
        {
            MainCamera.transform.position = new Vector3(-13.23f, 26.17f, 47f);
            MainCamera.transform.rotation = Quaternion.Euler(20, 90, 0);
        }

        //If the pan is over, let's zoom in on a random Endimon
        else if(GameStatus == "PlayerAwaitTurn" && Timer < 0 && Timer > -6)
        {
            if(ZoomedIn == false)
            {
                rand = Random.Range(0, 4);
                MainCamera.transform.position = EndimonLocations[rand];
                MainCamera.transform.rotation = Quaternion.Euler(EndimonRotations[rand].x, EndimonRotations[rand].y, EndimonRotations[rand].z);
                ZoomedIn = true;
            }
        }

        //If the game status is either attacking or defending, we will zoom in on the specific Endimon
        else if((GameStatus == "Attacking" || GameStatus == "Defending") && MainEndimon != null)
        {
            int value = MainEndimon.GetActiveNumber();
            //The AI values are flipped so they will be reversed if we are dealing with one of them
            if(value == 2)
            {
                value = 3;
            }
            else if(value == 3)
            {
                value = 2;
            }

            MainCamera.transform.position = EndimonLocations[value];
            MainCamera.transform.rotation = Quaternion.Euler(EndimonRotations[value].x, EndimonRotations[value].y, EndimonRotations[value].z);
            MainEndimon = null; //Eliminate this code running many times in Update
        }

        //If the status is globals, we zoom out to watch the casting of the particles
        else if(GameStatus == "Globals")
        {
            MainCamera.transform.position = GlobalLocation;
            MainCamera.transform.rotation = Quaternion.Euler(PanRotationLeft.x, PanRotationLeft.y, PanRotationLeft.z);
        }

        //If the timer zooms past -6, we will go back to a pan mode instead of zooming in
        else if(Timer < -6)
        {
            Timer = 8f;
        }
        Timer -= Time.deltaTime;
    }

    //Allows any part of the game to change the camera view (Resets timer)
    public static void SetGameStatus(string stat, Endimon e)
    {
        GameStatus = stat;
        Timer = 8f;
        MainEndimon = e;
        ZoomedIn = true;
    }
}
