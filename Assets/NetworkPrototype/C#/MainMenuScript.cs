using UnityEngine;
using System.Collections;

public class MainMenuScript : MonoBehaviour
{
    public static MainMenuScript SP;

    private JoinMenuScript joinMenuScript;
    private GameLobbyScript gameLobbyScript;
    private MultiplayerMenuScript multiplayerScript;

    private bool requirePlayerName = false;
    private string playerNameInput = "";

    void Awake()
    {
        SP = this;

        playerNameInput = PlayerPrefs.GetString("playerName", "");
        requirePlayerName = true;


        joinMenuScript = GetComponent<JoinMenuScript>() as JoinMenuScript;
        gameLobbyScript = GetComponent<GameLobbyScript>() as GameLobbyScript;
        multiplayerScript = GetComponent<MultiplayerMenuScript>() as MultiplayerMenuScript;

        OpenMenu("multiplayer");
    }
    void OnGUI()
    {
        if (requirePlayerName)
        {
            GUILayout.Window(9, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 100), NameMenu, "Please enter a name:");
        }
    }

    public void OpenMenu(string newMenu)
    {
        if (requirePlayerName)
        {
            return;
        }

        if (newMenu == "multiplayer-host")
        {
            gameLobbyScript.EnableLobby();

        }
        else if (newMenu == "multiplayer-join")
        {
            joinMenuScript.EnableMenu();

        }
        else if (newMenu == "multiplayer")
        {
            multiplayerScript.EnableMenu();

        }
        else
        {
            Debug.LogError("Wrong menu:" + newMenu);

        }
    }
    void NameMenu(int id)
    {

        GUILayout.BeginVertical();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Please enter your name");
        GUILayout.Space(10);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        playerNameInput = GUILayout.TextField(playerNameInput);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();



        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (playerNameInput.Length >= 1)
        {
            if (GUILayout.Button("Save"))
            {
                requirePlayerName = false;
                PlayerPrefs.SetString("playerName", playerNameInput);
                OpenMenu("multiplayer");
            }
        }
        else
        {
            GUILayout.Label("Enter a name to continue...");
        }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();


        GUILayout.EndVertical();
    }
}