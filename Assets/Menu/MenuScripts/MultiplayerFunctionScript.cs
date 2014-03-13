
using UnityEngine;
using System.Collections;

public class MultiplayerFunctionScript : MonoBehaviour
{
    //USAGE:
    // Change masterserverGamename, defaultport, connectTimeoutValue
    // Then implement the various calls in your multiplayer scripts at will
    public string masterserverGameName = "Battleship"; 
    public int defaultServerPort = 20000;   
    public int connectTimeoutValue = 10;   
	
    //
    // SETUP 
    //
    public static MultiplayerFunctionScript SP;
    public delegate void VoidDelegate();
    private static bool hasLoadedMasterserverSettings = false;
    private static HostData[] hostData = null; 


    void Awake()
    {
        if (SP != null && this != SP)
        {
            //There is already a copy of this script running
            Destroy(this);
            return;
        }
        SP = this;        
        StartCoroutine(TestConnection());
        StartCoroutine(SetupMasterServer());
    }


    public bool ReadyLoading()
    {
        return (hasLoadedMasterserverSettings && hasTestedNAT);
    }

    IEnumerator SetupMasterServer()
    {
        // Set ports & IP
        /*
       //Try to resolve the masterserver IP instead of hardcoding an IP.
       //Do note that a DNS resolve adds some plugins to the webplayer.
       //Call a webpage containing the right IP(s)ipt to reduce filesize
       MasterServer.port = 30303;
        Network.connectionTesterPort = 50505;
        Network.natFacilitatorPort = 40404;
        Network.natFacilitatorIP =Network.connectionTesterIP = MasterServer.ipAddress = "85.17.139.32";
        */

        //If we don't set any IP/ports we use the public&free Unity masterserver.

        hasLoadedMasterserverSettings = true;
        FetchHostList();
        yield break;
    }



    //
    // CONNECTING
    //
    private bool connecting = false;
    private HostData lastConnectHostData;
    private string[] lastConnectIP = null;
    private int lastConnectPort;
    private string lastConnectPW = "";
    private float lastConnectStarted;
    private bool lastConnectUsedNAT = false;
    private VoidDelegate lastConnectionFailDelegate;
    private bool lastConnectMayRetry = true;
    private NetworkConnectionError lastConnectionError;


    public void HostDataConnect(HostData hData, string password, bool doRetryConnection, VoidDelegate failDelegate)
    {
        StartedConnecting();

        lastConnectHostData = hData;
        lastConnectUsedNAT = lastConnectHostData.useNat;
        lastConnectMayRetry = doRetryConnection;
        if (password == "")
            Network.Connect(hData);
        else
            Network.Connect(hData, password);
        Invoke("ConnectTimeout", connectTimeoutValue);

        lastConnectIP = hData.ip;
        lastConnectPort = hData.port;
        lastConnectPW = password;
        lastConnectStarted = Time.time;
        if (failDelegate != null)
            lastConnectionFailDelegate = failDelegate;

        connecting = true;
    }
    public void DirectConnect(string IP, int port, string password, bool doRetryConnection, VoidDelegate failDelegate)
    {
        string[] ips = new string[1];
        ips[0] = IP;
        DirectConnect(ips, port, password, doRetryConnection, failDelegate);
    }
    public void DirectConnect(string[] IP, int port, string password, bool doRetryConnection, VoidDelegate failDelegate)
    {
        StartedConnecting();
        lastConnectMayRetry = doRetryConnection;
        lastConnectUsedNAT = false;
        if (password == "")
            Network.Connect(IP, port);
        else
            Network.Connect(IP, port, password);
        Invoke("ConnectTimeout", connectTimeoutValue);

        connecting = true;
        lastConnectHostData = null;
        lastConnectIP = IP;
        lastConnectPort = port;
        lastConnectPW = password;
        lastConnectStarted = Time.time;
        if (failDelegate != null)
            lastConnectionFailDelegate = failDelegate;
    }
    private void StartedConnecting()
    {
        CancelInvoke("ConnectTimeout");
        connecting = true;
        lastConnectStarted = Time.realtimeSinceStartup;
    }

    public void CancelConnection()
    {
        CancelInvoke("ConnectTimeout");
        lastConnectMayRetry = false;
        connecting = false;
    }

    void ConnectTimeout()
    {
        Debug.Log("Connect timeout");
        OnFailedToConnect(NetworkConnectionError.NoError);
    }

    void OnFailedToConnect(NetworkConnectionError info)
    {
        CancelInvoke("ConnectTimeout");
        if (lastConnectIP != null)
        {
            Debug.Log("Failed to connect [" + lastConnectIP[0] + ":" + lastConnectPort + " ] info:" + info);
            StartCoroutine(FailedConnectRetry(info));
        }
        else
        {
            Debug.Log("Failed to connect, no data:S " + info);
            connecting = false;
        }
    }

    void OnConnectedToServer()
    {
        CancelInvoke("ConnectTimeout");
    }

    //Try again with some different settings..mainly try connecting without NAT
    IEnumerator FailedConnectRetry(NetworkConnectionError info)
    {
        lastConnectionError = info;
        if (!lastConnectMayRetry || info == NetworkConnectionError.TooManyConnectedPlayers || info == NetworkConnectionError.InvalidPassword)
        {
            //Stop retrying
        }
        else if (lastConnectUsedNAT || lastConnectPort != defaultServerPort)
        {
            //Retry (without NAT) on default port!
            yield return 0; //Workaround against "too many open connections"
            DirectConnect(lastConnectIP, MultiplayerFunctionScript.SP.defaultServerPort, lastConnectPW, true, lastConnectionFailDelegate);
            yield break;
        }

        //Finished: failed
        connecting = false;
        if (lastConnectionFailDelegate != null)
        {
            lastConnectionFailDelegate();
        }
    }

    public bool IsConnecting()
    {
        return connecting;
    }


    //
    // MASTERSERVER
    //

    private float lastMSRetry = 0;
    private int msRetries = 0;

    void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        //Two possible causes: FetchHostList OR RegisterHost failed to connect
        Debug.LogWarning("OnFailedToConnectToMasterServer: " + info);
        int retryTime = 5 + 5 * msRetries * msRetries; // 5,10,20,50,.. to not overload the masterserver when its down
        if (lastMSRetry < Time.time - retryTime)
        {
            lastMSRetry = Time.time;
            FetchHostList();
        }
    }

    private float lastHostListRequest = -999;

    public void FetchHostList()
    {
        if (!hasLoadedMasterserverSettings)
        {
            Debug.LogError("Calling FetchHostList but we havent loaded MS settings yet");
            return;
        }
        hostData = MasterServer.PollHostList();
        if (lastHostListRequest < Time.realtimeSinceStartup - 1)
        {
            lastHostListRequest = Time.realtimeSinceStartup;
            MasterServer.RequestHostList(masterserverGameName);
        }

    }

    private VoidDelegate currentHostListDelegate;

    public void SetHostListDelegate(VoidDelegate newD)
    {
        currentHostListDelegate = newD;
    }

    public HostData[] GetHostData()
    {
        return hostData;
    }

    private bool hasReceivedHostListResponse = false;

    public bool HasReceivedHostList()
    {
        return hasReceivedHostListResponse;
    }

    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        Debug.Log(Time.realtimeSinceStartup+" OnMasterEevent: "+msEvent);
        switch (msEvent)
        {
            case MasterServerEvent.HostListReceived:
                //WARNING: It does 1 call per item in the full host list!
                //A list of 100 items generates 100 calls. The 34th call contains 34 items total etc. 
                StartCoroutine(WaitForAllHostData());
                break;
            case MasterServerEvent.RegistrationFailedGameName:
            case MasterServerEvent.RegistrationFailedGameType:
            case MasterServerEvent.RegistrationFailedNoServer:
                Debug.Log("Masterserver error: " + msEvent);
                break;
            case MasterServerEvent.RegistrationSucceeded:
                break;
            default:
                break;
        }
    }

    private bool awaitingHostList = false;
    private IEnumerator WaitForAllHostData()
    {
        if (awaitingHostList) yield break;
        awaitingHostList = true;
        hostData = MasterServer.PollHostList();
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            if (MasterServer.PollHostList().Length == hostData.Length)
            {
                break;
            }
            hostData = MasterServer.PollHostList();
        }
        if (currentHostListDelegate != null) { currentHostListDelegate(); }
        hasReceivedHostListResponse = true;
        awaitingHostList = false;
    }


    //
    // SERVER
    //

    public void StartServer(string password, int port, int maxConnections, bool enableSecurity)
    {
        if (port <= 1024) Debug.LogError("StartServer tries to use port <=1024. This will probably not work and is illegal! Use a different port!");
        if (password != "")
            Network.incomingPassword = password;
        if (enableSecurity)
            Network.InitializeSecurity();
        Network.InitializeServer(maxConnections, port, GetNATStatus());
    }


    public void RegisterHost(string serverTitle, string comment)
    {
        if (!Network.isServer)
        {
            Debug.LogError("RegisterServer: is not a server!");
            return;
        }
        if (!ReadyLoading()) Debug.LogWarning("RegisterHost; wasn't ready loading network settings yet though!");
        MasterServer.RegisterHost(masterserverGameName, serverTitle, comment);
    }

    public void UnregisterHost()
    {
        if (!Network.isServer)
        {
            Debug.LogError("RegisterServer: is not a server!");
            return;
        }
        MasterServer.UnregisterHost();
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        if (Network.isServer)
            UnregisterHost();
    }

    //
    // CONNECTIONTESTER
    //

    public bool GetNATStatus()
    {
        if (!hasTestedNAT)
            Debug.LogWarning("Calling GetNATStatus, but we havent finished testing yet!");
        return testedUseNat;
    }

    private static bool hasTestedNAT = false;
    private static bool testedUseNat = false;

    IEnumerator TestConnection()
    {
        if (hasTestedNAT)
            yield break;

        while (!hasLoadedMasterserverSettings)
            yield return 0;

        testedUseNat = !Network.HavePublicAddress();
        ConnectionTesterStatus connectionTestResult = ConnectionTesterStatus.Undetermined;
        float timeoutAt = Time.realtimeSinceStartup + 10;
        float timer = 0;
        bool probingPublicIP = false;
        string testMessage = "";
        while (!hasTestedNAT)
        {
            yield return 0;
            if (Time.realtimeSinceStartup >= timeoutAt)
            {
                Debug.LogWarning("TestConnect NAT test aborted; timeout");
                break;
            }
            connectionTestResult = Network.TestConnection();
            switch (connectionTestResult)
            {
                case ConnectionTesterStatus.Error:
                    testMessage = "Problem determining NAT capabilities";
                    hasTestedNAT = true;
                    break;

                case ConnectionTesterStatus.Undetermined:
                    testMessage = "Undetermined NAT capabilities";
                    hasTestedNAT = false;
                    break;

                case ConnectionTesterStatus.PublicIPIsConnectable:
                    testMessage = "Directly connectable public IP address.";
                    testedUseNat = false;
                    hasTestedNAT = true;
                    break;

                // This case is a bit special as we now need to check if we can 
                // circumvent the blocking by using NAT punchthrough
                case ConnectionTesterStatus.PublicIPPortBlocked:
                    testMessage = "Non-connectble public IP address (port " + defaultServerPort + " blocked), running a server is impossible.";
                    hasTestedNAT = false;
                    // If no NAT punchthrough test has been performed on this public IP, force a test
                    if (!probingPublicIP)
                    {
                        Debug.Log("Testing if firewall can be circumvented");
                        connectionTestResult = Network.TestConnectionNAT();
                        probingPublicIP = true;
                        timer = Time.time + 10;
                    }
                    // NAT punchthrough test was performed but we still get blocked
                    else if (Time.time > timer)
                    {
                        probingPublicIP = false; 		// reset
                        testedUseNat = true;
                        hasTestedNAT = true;
                    }
                    break;
                case ConnectionTesterStatus.PublicIPNoServerStarted:
                    testMessage = "Public IP address but server not initialized, it must be started to check server accessibility. Restart connection test when ready.";
                    break;
                case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
                    testMessage = "Limited NAT punchthrough capabilities. Cannot connect to all types of NAT servers. Running a server is ill adviced as not everyone can connect.";
                    testedUseNat = true;
                    hasTestedNAT = true;
                    break;
                case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
                    testMessage = "Limited NAT punchthrough capabilities. Cannot connect to all types of NAT servers. Running a server is ill adviced as not everyone can connect.";
                    testedUseNat = true;
                    hasTestedNAT = true;
                    break;
                case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
                case ConnectionTesterStatus.NATpunchthroughFullCone:
                    testMessage = "NAT punchthrough capable. Can connect to all servers and receive connections from all clients. Enabling NAT punchthrough functionality.";
                    testedUseNat = true;
                    hasTestedNAT = true;
                    break;

                default:
                    testMessage = "Error in test routine, got " + connectionTestResult;
                    break;
            }
        }
        hasTestedNAT = true;
        Debug.Log("TestConnection result: testedUseNat=" + testedUseNat + " connectionTestResult=" + connectionTestResult + " probingPublicIP=" + probingPublicIP + " hasTestedNAT=" + hasTestedNAT + " testMessage=" + testMessage);
    }

    //
    // Fancy output functions
    //

    public string ConnectingToAddress()
    {
        return lastConnectIP[0] + ":" + lastConnectPort;
    }

    public string[] LastIP()
    {
        return lastConnectIP;
    }

    public int LastPort()
    {
        return lastConnectPort;
    }

    public NetworkConnectionError LastConnectionError()
    {
        return lastConnectionError;
    }

    public float TimeSinceLastConnect()
    {
        return Time.realtimeSinceStartup - lastConnectStarted;
    }
   
    //Send/receive large amounds of data via byte[]
    public static byte[] StringToBytes(string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }
    public static string BytesToString(byte[] by)
    {
        return System.Text.Encoding.UTF8.GetString(by);
    }
}