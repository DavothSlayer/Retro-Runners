using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.PlayerAccounts;

public class SimplePlayerAccountLinking : MonoBehaviour
{

    #region UI GAMEOBJECTS TO WIRE UP //////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField]
    public Text m_Authenticated;
    [SerializeField]
    public Text m_PlayerID;
    [SerializeField]
    public Button m_SignInButton;
    [SerializeField]
    public Button m_SignOutButton;
    [SerializeField]
    public Button m_LinkButton;
    [SerializeField]
    public Button m_UnLinkButton;
    #endregion


    #region UNITY GAMEOBJECT LIFECYCLE EVENTS /////////////////////////////////////////////////////////////////////////////////////
    async void Start()
    {
        // INITIALIZE UNITY GAMING SERVICES
        await UnityServices.InitializeAsync();
        Debug.Log($"Unity Services : {UnityServices.State}");

        // REGISTER PLAYER ACCOUNT HANDLERS
        PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;
        PlayerAccountService.Instance.SignedOut += OnPlayerAccountSignedOut;
        PlayerAccountService.Instance.SignInFailed += OnPlayerAccountSignInFailed;

        // REGISTER AUTHENTICATION SERVICE HANDLERS
        AuthenticationService.Instance.SignedIn += OnAuthSignedIn;
        AuthenticationService.Instance.SignedOut += OnAuthSignedOut;
        AuthenticationService.Instance.SignInFailed += OnAuthSignInFailed;
        AuthenticationService.Instance.Expired += OnAuthSignInExpired;

        // SIGN IN ANONYMOUSLY IF SESSION TOKEN EXISTS
        Debug.Log("Signing in anonymously...");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await AuthenticationService.Instance.UpdatePlayerNameAsync("DavothSlayer");

        Debug.Log($"Authentication Is Signed In : {AuthenticationService.Instance.IsSignedIn}");
        UpdateUI();
    }

    private void OnDestory()
    {
        // UN-REGISTER PLAYER ACCOUNT SIGN IN HANDLERS
        PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn;
        PlayerAccountService.Instance.SignedOut -= OnPlayerAccountSignedOut;
        PlayerAccountService.Instance.SignInFailed -= OnPlayerAccountSignInFailed;

        // UN-REGISTER AUTHENTICATION  HANDLERS
        AuthenticationService.Instance.SignedIn -= OnAuthSignedIn;
        AuthenticationService.Instance.SignedOut -= OnAuthSignedOut;
        AuthenticationService.Instance.SignInFailed -= OnAuthSignInFailed;
        AuthenticationService.Instance.Expired -= OnAuthSignInExpired;
    }
    #endregion


    #region UPDATE UI //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private async void UpdateUI()
    {
        // UPDATE AUTHENTICATION STATUS & PLAYER ID TEXT
        m_Authenticated.text = AuthenticationService.Instance.IsSignedIn ? "Authenticated":"Not Authenticatied";
        m_PlayerID.text = AuthenticationService.Instance.PlayerId != null ? AuthenticationService.Instance.PlayerId : ""; 

        // UPDATE UNITY PLAYER ACCOUNT BUTTONS
        bool isPlayerAccountSignedIn = PlayerAccountService.Instance.IsSignedIn; 
        Debug.Log($"Player Account Signed In : {isPlayerAccountSignedIn}");
        m_SignInButton.interactable = !isPlayerAccountSignedIn;
        m_SignOutButton.interactable = isPlayerAccountSignedIn;

        // UPDATE UNITY PLAYER ACCOUNT LINK BUTTONS
        PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
        bool isLinkedToPlayerAccounts = playerInfo.Identities.Find((Identity i) => i.TypeId.ToLowerInvariant().Equals("unity")) != null;
        Debug.Log($"Player Account Linked : {isLinkedToPlayerAccounts}");
        m_LinkButton.interactable = !isLinkedToPlayerAccounts && isPlayerAccountSignedIn;
        m_UnLinkButton.interactable = isLinkedToPlayerAccounts;
    }
    #endregion


    #region UI EVENT HANDLERS /////////////////////////////////////////////////////////////////////////////////////////////////////
    public async void SignIn_Button_Clicked()
    {
        // START PLAYER ACCOUNTS SIGN IN PROCESS
        Debug.Log("Player Accounts - Sign In Button Clicked");
        await PlayerAccountService.Instance.StartSignInAsync();       
    }

    public void SignOut_Button_Clicked()
    {
        // START PLAYER ACCOUNTS SIGN OUT
        Debug.Log("Player Accounts - Sign Out Button Clicked");
        PlayerAccountService.Instance.SignOut();        
    }

    public async void Link_Button_Clicked()
    {
        // LINK TO UNITY PLAYER ACCOUNT
        Debug.Log("Link Button Clicked");
        await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
        UpdateUI();
    }

    public async void UnLink_Button_Clicked()
    {
        // UNLINK FROM UNITY PLAYER ACCOUNT
        Debug.Log("UnLink Button Clicked");
        await AuthenticationService.Instance.UnlinkUnityAsync();
        UpdateUI();
    }
    #endregion


    #region PLAYER ACCOUNTS ACTION HANDLERS //////////////////////////////////////////////////////////////////////////////////////
    private void OnPlayerAccountSignedIn()
    {
        // UNITY PLAYER ACCOUNTS SIGN IN SUCCESSFULL
        string accessToken = PlayerAccountService.Instance.AccessToken;
        if (!string.IsNullOrEmpty(accessToken)) {
            Debug.Log($"Attempting SignIn with PlayerAccounts Access Token - {accessToken}");
            AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
        }
        UpdateUI();
    }

    private void OnPlayerAccountSignedOut()
    {       
        UpdateUI();
    }

    private void OnPlayerAccountSignInFailed(RequestFailedException requestFailedException)
    {
        // UNITY PLAYER ACCOUNT SIGN IN FAILED
        Debug.Log($"Player Account Sign In Error : {requestFailedException.ErrorCode} : {requestFailedException.Message}");
        UpdateUI();
    }
    #endregion


    #region AUTHENTICATION SERVICE ACTION HANDLERS //////////////////////////////////////////////////////////////////////////////
    private void OnAuthSignedIn()
    {
        // AUTHENTICATION SIGNED IN SUCCESSFULLY
        Debug.Log($"Authentication Service Signed In with PlayerID - {AuthenticationService.Instance.PlayerId}");
    }

    private void OnAuthSignedOut()
    {
        // AUTHENTICATION SIGNED OUT
        Debug.Log("Authentication Service Signed Out"); 
    }

    private void OnAuthSignInFailed(RequestFailedException requestFailedException)
    {
        // AUTHENTICATION SIGNED IN FAILED
        Debug.Log($"Authentication SignIn Failed : {requestFailedException.ErrorCode} : {requestFailedException.Message}"); 
    }

    private void OnAuthSignInExpired()
    {
        // AUTHENTICATION TOKEN EXPIRED
        Debug.Log("Authentication Service Token Expired");        
    }
    #endregion
}
