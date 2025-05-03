using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class WalletLoginRedFox : MonoBehaviour
{
    public Text info;
    public GameObject networkManager;
    NetworkManager NetworkManager;
    [SerializeField] bool testCase;
    private void Start() => NetworkManager = networkManager.GetComponent<NetworkManager>();
    public async void OnLogin()
    {
        if (testCase)
        {
            networkManager.SetActive(true);
            info.text = "Loading Up Game!!!";
            return;
        }
        
        // get current timestamp
        var timestamp = (int)System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
        // set expiration time
        var expirationTime = timestamp + 60;
        // set message
        var message = expirationTime.ToString();
        // sign message
        var signature = "";
        // verify account
        //var account = SignVerifySignature(signature, message);
        var now = (int)System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
        // validate
        // if (account.Length == 42 && expirationTime >= now)
        // {
        //     info.text = "Signed In.";
        //     Debug.Log("Signed In");
        //     //PlayerPrefs.SetString("Account", account);
        //     // load next scene
        //     networkManager.SetActive(true);
        //     NetworkManager.message = message;
        //     NetworkManager.userAddress = account;
        //     NetworkManager.userSignature = signature;

        //     Debug.Log(signature);
        //     info.text = message + " " + account;
        // }
    }

    // public string SignVerifySignature(string signatureString, string originalMessage)
    // {
    //     var msg = "\x19" + "Ethereum Signed Message:\n" + originalMessage.Length + originalMessage;
    //     var msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));
    //     var signature = MessageSigner.ExtractEcdsaSignature(signatureString);
    //     var key = EthECKey.RecoverFromSignature(signature, msgHash);
    //     return key.GetPublicAddress();
    // }
}