using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class UIController : MonoBehaviour
{

    public static UIController instance;
    
    public TMP_Text overheatedMessage;
    public Slider weaponTempSlider; // Weapon Temperature
    public Image crosshair;

    public GameObject deathScreen;
    public TMP_Text deathText;

    public Slider healthSlider;

    public TMP_Text killsText;
    public TMP_Text deathsText;

    public GameObject leaderboard;
    public LeaderboardPlayer leaderboardPlayerDisplay;

    public GameObject endScreen;

    public TMP_Text timeRemainingText;
    public TMP_Text timerText;

    public GameObject optionsScreen;
    public bool isPaused;



    //public TMP_Text whichMapWePlaying;


    private void Awake()
    {

        if(instance == null)
        {

            instance = this;

        } else
        {

            Destroy(this);

        }

    }

    private void Start()
    {
        
    }

    private void Update()
    {
     
        if(Input.GetKeyDown(KeyCode.Escape))
        {
             
            ShowHideOptions();
    
        }

        if(optionsScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
        
    }

    public void ShowHideOptions()
    {

        if(!optionsScreen.activeInHierarchy)
        {
            optionsScreen.SetActive(true);
            isPaused = true;

        } else
        {

            optionsScreen.SetActive(false);
            isPaused = false;

        }


    }
    
    public void ReturnToMainMenu()
    {

        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
        isPaused = false;

    }

    public void QuitGame()
    {

        Application.Quit();

    }


}
