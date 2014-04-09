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
	public Texture backgroundTexture;

    public static string user = "", name = "";
    private string password = "", rePass = "", message = "";
    
    private bool register = false;

    void Awake()
    {
        SP = this;

        user = PlayerPrefs.GetString("playerName", "");
        requirePlayerName = true;


        joinMenuScript = GetComponent<JoinMenuScript>() as JoinMenuScript;
        gameLobbyScript = GetComponent<GameLobbyScript>() as GameLobbyScript;
        multiplayerScript = GetComponent<MultiplayerMenuScript>() as MultiplayerMenuScript;

        OpenMenu("multiplayer");
    }
    void OnGUI()
    {
        // if (requirePlayerName)
        // {
        // if (message != "")
        //     GUILayout.Box(message);
		GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),backgroundTexture);
        // GUILayout.Window(9, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 100), NameMenu, "Register/Login");
        // }
        if (message != "")
            GUILayout.Box(message);
        if (requirePlayerName) {
            if (register)
            {
				GUILayout.BeginArea (new Rect((Screen.width/2)-50, (Screen.height/3) , 100, 400));
				GUILayout.FlexibleSpace();
                GUILayout.Label("Username");
                user = GUILayout.TextField(user);
                GUILayout.Label("password");
                password = GUILayout.PasswordField(password, "*"[0]);
                GUILayout.Label("Re-password");
                rePass = GUILayout.PasswordField(rePass, "*"[0]);
				GUILayout.Button("HellO");
                //GUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Back")) {

                    register = false;
				}
                if (GUILayout.Button("Register"))
                {
                    message = "";
                    
                    if (user == "" || password == "")
                        message += "Please enter all the fields \n";
                    else
                    {
                        if (password == rePass)
                        {
                            WWWForm form = new WWWForm();
                            form.AddField("user", user);
                            form.AddField("password", password);
                            WWW w = new WWW("http://battlefield361.dx.am/register.php", form);
                            StartCoroutine(registerFunc(w));
                        }
                        else
                            message += "Your Password does not match \n";
                    }
                }
                
               // GUILayout.EndHorizontal();
				GUILayout.EndArea();

            }
            else
            {
				GUILayout.BeginArea (new Rect((Screen.width/2)-100, 0 , 200, 200));
				GUILayout.FlexibleSpace();

                GUILayout.Label("User:");
                user = GUILayout.TextField(user);
                GUILayout.Label("Password:");
                password = GUILayout.PasswordField(password, "*"[0]);
                
                //GUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Login"))
                {
                    message = "";
                    
                    if (user == "" || password == "")
                        message += "Please enter all the fields \n";
                    else
                    {
                        WWWForm form = new WWWForm();
                        form.AddField("user", user);
                        form.AddField("password", password);
                        WWW w = new WWW("http://battlefield361.dx.am/login.php", form);
                        StartCoroutine(login(w));
                    }
                }
                
                if (GUILayout.Button("Register")){
                    register = true;
				}
				if (GUILayout.Button("Quit")) {
					Application.Quit();
				}
              //  GUILayout.EndHorizontal();
				GUILayout.EndArea();

            }
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

    void registerMenu(int id)
    {
		GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginVertical();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Username: ");      
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        user = GUILayout.TextField(user);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Password: ");      
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        password = GUILayout.PasswordField(password, "*"[0]);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Enter Password Again: ");      
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        rePass = GUILayout.PasswordField(rePass, "*"[0]);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Register"))
        {
            message = "";
            
            if (user == "" || password == "")
                message += "Please enter all the fields \n";
            else
            {
                if (password == rePass)
                {
                    WWWForm form = new WWWForm();
                    form.AddField("user", user);
                    form.AddField("password", password);
                    WWW w = new WWW("http://battlefield361.dx.am/register.php", form);
                    StartCoroutine(registerFunc(w));
                }
                else
                    message += "Your Password does not match \n";
            }
        }

        if (GUILayout.Button("Login")) {
            register = false;
        
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
		GUILayout.EndArea();
    }

    void loginMenu(int id)
    {
		GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));

        GUILayout.BeginVertical();
        GUILayout.Space(10);


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Username: ");      
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        user = GUILayout.TextField(user);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Password: ");      
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        password = GUILayout.PasswordField(password, "*"[0]);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();


        if (GUILayout.Button("Login"))
        {
            message = "";
            
            if (user == "" || password == "")
                message += "Please enter all the fields \n";
            else
            {
                WWWForm form = new WWWForm();
                form.AddField("user", user);
                form.AddField("password", password);
                WWW w = new WWW("http://battlefield361.dx.am/login.php", form);
                StartCoroutine(login(w));
            }
        } 

        if (GUILayout.Button("Register")) {
            register = true;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
		GUILayout.EndArea();
    }


    void NameMenu(int id)
    {

        if (register) {
            GUILayout.BeginVertical();
            GUILayout.Space(10);


            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Username: ");      
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            user = GUILayout.TextField(user);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Password: ");      
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            password = GUILayout.PasswordField(password, "*"[0]);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Enter Password Again: ");      
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            rePass = GUILayout.PasswordField(rePass, "*"[0]);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Register"))
            {
                message = "";
                
                if (user == "" || password == "")
                    message += "Please enter all the fields \n";
                else
                {
                    if (password == rePass)
                    {
                        WWWForm form = new WWWForm();
                        form.AddField("user", user);
                        form.AddField("password", password);
                        WWW w = new WWW("http://battlefield361.dx.am/register.php", form);
                        StartCoroutine(registerFunc(w));
                    }
                    else
                        message += "Your Password does not match \n";
                }
            }

            if (GUILayout.Button("Login")) {
                register = false;
            
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        } else {
            GUILayout.BeginVertical();
            GUILayout.Space(10);


            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Username: ");      
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            user = GUILayout.TextField(user);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Password: ");      
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            password = GUILayout.PasswordField(password, "*"[0]);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();


            if (GUILayout.Button("Login"))
            {
                message = "";
                
                if (user == "" || password == "")
                    message += "Please enter all the fields \n";
                else
                {
                    WWWForm form = new WWWForm();
                    form.AddField("user", user);
                    form.AddField("password", password);
                    WWW w = new WWW("http://battlefield361.dx.am/login.php", form);
                    StartCoroutine(login(w));
                }
            } 

            if (GUILayout.Button("Register")) {
                register = true;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        }

    }

        IEnumerator login(WWW w)
    {
        yield return w;
        if (w.error == null)
        {
            string[] temp = w.text.Split("*".ToCharArray()); 

            if (temp[0] == "login-SUCCESS")
            {
                int pUID =int.Parse(temp[1]);
				int pPlayed = int.Parse(temp[2]);
				int pWon = int.Parse(temp[3]);
                PlayerPrefs.SetInt("UID",pUID);
				Debug.Log ("Login: " + user + " and: " + pUID);

                requirePlayerName = false;
                PlayerPrefs.SetString("playerName",user);
				PlayerPrefs.SetInt("Played",pPlayed);
				PlayerPrefs.SetInt("Won",pWon);
                OpenMenu("multiplayer");
            }
            else
                message += w.text;
        }
        else
        {
            message += "ERROR: " + w.error + "\n";
            Debug.Log (w.error);
        }
    }
    
    IEnumerator registerFunc(WWW w)
    {
        yield return w;
        if (w.error == null)
        {
           string[] temp = w.text.Split("*".ToCharArray()); 
            if (temp[0] != "AlreadyExist")
            {
                  Debug.Log ("Reg" + w.text);
            int pUID = int.Parse(temp[1]);
            requirePlayerName = false;
           PlayerPrefs.SetString("playerName",user);
           PlayerPrefs.SetInt("UID",pUID);
				PlayerPrefs.SetInt("Played",0);
				PlayerPrefs.SetInt("Won",0);
            OpenMenu("multiplayer");
            } else {
                message += "User Already Exists";
            }
          
        }
        else
        {
            message += "ERROR: " + w.error + "\n";
            Debug.Log (w.error);

        }
    }

}