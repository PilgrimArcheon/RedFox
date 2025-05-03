using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_WEBGL
public class WebLoginRedFox : MonoBehaviour
{
    public Text info; 
    public GameObject networkManager;
    NetworkManager NetworkManager;

    [DllImport("__Internal")]
    private static extern void Web3Connect();

    [DllImport("__Internal")]
    private static extern string ConnectAccount();

    [DllImport("__Internal")]
    private static extern void SetConnectAccount(string value);

    private int expirationTime;
    private string account;
    
    void Start()
    {
        NetworkManager = networkManager.GetComponent<NetworkManager>();
    }

    public void OnLogin()
    {
        Web3Connect();
        OnConnected();
    }

    async private void OnConnected()
    {
        account = ConnectAccount();
        while (account == "")
        {
            await new WaitForSeconds(1f);
            account = ConnectAccount();
        };

        NetworkManager.userAddress = account;
        Debug.Log("Account " + account);
        //SetConnectAccount("");
        // load next scene
        OnSignMessage();
    }

    async public void OnSignMessage()
    {
        try 
        {
            info.text = "Signing In..."; 
            // get current timestamp
            int timestamp = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
            // set expiration time
            int expirationTime = timestamp + 60;
            // set message
            
            string message = expirationTime.ToString();
            string signature = await Web3GL.Sign(message);
            NetworkManager.message = message;
            
            NetworkManager.userSignature = signature;
            info.text = "Signed In."; 
            Debug.Log("Signed In");
            networkManager.SetActive(true);
            Debug.Log(signature);
            info.text = message + " " + account; 
        } catch (Exception e) {
            info.text = "Failed! Trying Again."; 
            OnSignMessage();
            Debug.LogException(e, this);
        }
    }

    public void OnSkip()
    {
        // burner account for skipped sign in screen
        PlayerPrefs.SetString("Account", "");
        // move to next scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
#endif