using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;//Instance Menu Script
    [SerializeField] public Menu[] menus;//Ref All Menus in the Scene
    public RectTransform taskSpecificContent;
    public GameObject gamePlayManager;

    void Awake()
    {
        Instance = this;//Make this an Instance
        Time.timeScale = 1f;//Set TIme Scale to 1 i.e Active
    }

    public void OpenMenu(string menuName)//To Open Menu
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                menus[i].Open();
            }
            else if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    public void ConfirmHealth()
    {
        if (NetworkManager.Instance.livesLeft > 0) 
        { 
            OpenMenu("game");
            gamePlayManager.SetActive(true);
            GameplayManager.Instance.SetUpNewGame();
        }
        else OpenMenu("noLivesLeft");
    }

    public void OpenMenu(Menu menu)//Confirm Menu Opened
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                CloseMenu(menus[i]);//Close every othe menu
            }
        }
        menu.Open();//Call the Menu Open Function
    }

    public void CloseMenu(Menu menu)//Close the Menu
    {
        menu.Close();
    }

    public void Play(string Game)
    {
        SceneManager.LoadScene(Game);
    }

    public void MainMenu()//Got to Main Menu Scene
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void Pause()//Pause the Game
    {
        Time.timeScale = 0f;
    }

    public void Resume()//Resume the Game
    {
        Time.timeScale = 1f;
    }

    public void Restart()//Restart the Game
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);//Activate the Game Scene Again
    }

    public void Quit()//Quit the Game App
    {
        Application.Quit();
    }

    public void SetRectVertPos(float posY)
    {
        taskSpecificContent.position = new Vector3(taskSpecificContent.position.x, posY, taskSpecificContent.position.z);
    }
}