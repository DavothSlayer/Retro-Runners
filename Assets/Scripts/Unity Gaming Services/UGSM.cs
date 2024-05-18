using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.PlayerAccounts;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;

namespace RetroCode
{
    public class UGSM : MonoBehaviour
    {
        public CloudData cloudData;

        #region CloudData UI
        [SerializeField]
        private string cloudDataKey;
        #endregion

        #region Unity Events UI
        [Header("Events")]
        [Space]
        public UnityEvent CloudDataSuccess;
        public UnityEvent CloudDataFailed;
        public UnityEvent AuthResult;
        public UnityEvent AuthFailed;
        #endregion

        #region Unity Methods
        private async void Start()
        {
            // INITIALIZE UNITY GAMING SERVICES //
            await UnityServices.InitializeAsync();
            print($"Unity Services: {UnityServices.State}");

            // REGISTER PLAYER ACCOUNT HANDLERS //
            PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;
            PlayerAccountService.Instance.SignedOut += OnPlayerAccountSignedOut;
            PlayerAccountService.Instance.SignInFailed += OnPlayerAccountSignInFailed;

            // REGISTER AUTHENTICATION SERVICE HANDLERS //
            AuthenticationService.Instance.SignedIn += OnAuthSignedIn;
            AuthenticationService.Instance.SignedOut += OnAuthSignedOut;
            AuthenticationService.Instance.SignInFailed += OnAuthSignInFailed;
            AuthenticationService.Instance.Expired += OnAuthSignInExpired;

            // SIGN IN, IF NECESSARY //
            print("Signing in...");
            if (!AuthenticationService.Instance.IsSignedIn)
                await Authenticate();
            else
                LoadCloudData();

            print($"Authentication Is Signed In : {AuthenticationService.Instance.IsSignedIn}");
            //UpdateUI();
        }

        private void OnDestory()
        {
            // UN-REGISTER PLAYER ACCOUNT SIGN IN HANDLERS //
            PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn;
            PlayerAccountService.Instance.SignedOut -= OnPlayerAccountSignedOut;
            PlayerAccountService.Instance.SignInFailed -= OnPlayerAccountSignInFailed;

            // UN-REGISTER AUTHENTICATION  HANDLERS //
            AuthenticationService.Instance.SignedIn -= OnAuthSignedIn;
            AuthenticationService.Instance.SignedOut -= OnAuthSignedOut;
            AuthenticationService.Instance.SignInFailed -= OnAuthSignInFailed;
            AuthenticationService.Instance.Expired -= OnAuthSignInExpired;
        }
        #endregion

        #region Update UI Method
        /*private async void UpdateUI()
        {
            // UPDATE AUTHENTICATION STATUS & PLAYER ID TEXT //
            authenticated.text = AuthenticationService.Instance.IsSignedIn ? "Authenticated" : "Not Authenticatied";
            playerID.text = AuthenticationService.Instance.PlayerId != null ? AuthenticationService.Instance.PlayerId : "";
            usernameInput.gameObject.SetActive(AuthenticationService.Instance.IsSignedIn);
            username.text = AuthenticationService.Instance.PlayerName;
            username.gameObject.SetActive(AuthenticationService.Instance.IsSignedIn);
            loadCloudButton.SetActive(AuthenticationService.Instance.IsSignedIn);
            saveCloudButton.SetActive(AuthenticationService.Instance.IsSignedIn);

            // UPDATE UNITY PLAYER ACCOUNT BUTTONS //
            bool isPlayerAccountSignedIn = PlayerAccountService.Instance.IsSignedIn;
            Debug.Log($"Player Account Signed In : {isPlayerAccountSignedIn}");
            signInButton.SetActive(!isPlayerAccountSignedIn);
            signOutButton.SetActive(isPlayerAccountSignedIn);          

            // UPDATE UNITY PLAYER ACCOUNT LINK BUTTONS //
            PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
            bool isLinkedToPlayerAccounts = playerInfo.Identities.Find((Identity i) => i.TypeId.ToLowerInvariant().Equals("unity")) != null;
            Debug.Log($"Player Account Linked : {isLinkedToPlayerAccounts}");
            linkButton.SetActive(!isLinkedToPlayerAccounts && isPlayerAccountSignedIn);
            unlinkButton.SetActive(isLinkedToPlayerAccounts);
        }*/
        #endregion

        #region UI Event Handlers
        public async void PlayerAccountStartSignIn()
        {
            // START PLAYER ACCOUNTS SIGN IN PROCESS
            Debug.Log("Player Accounts - Sign In Button Clicked");
            await PlayerAccountService.Instance.StartSignInAsync();
        }

        public void PlayerAccountSignOut()
        {
            // START PLAYER ACCOUNTS SIGN OUT
            Debug.Log("Player Accounts - Sign Out Button Clicked");
            PlayerAccountService.Instance.SignOut();
        }

        public async void PlayerAccountLink()
        {
            // LINK TO UNITY PLAYER ACCOUNT
            Debug.Log("Link Button Clicked");
            await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
            //UpdateUI();
        }

        public async void PlayerAccountUnlink()
        {
            // UNLINK FROM UNITY PLAYER ACCOUNT
            Debug.Log("UnLink Button Clicked");
            await AuthenticationService.Instance.UnlinkUnityAsync();
            //UpdateUI();
        }
        #endregion

        #region Player Accounts Event Handlers
        private void OnPlayerAccountSignedIn()
        {
            // UNITY PLAYER ACCOUNTS SIGN IN SUCCESSFULL
            string accessToken = PlayerAccountService.Instance.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                Debug.Log($"Attempting SignIn with PlayerAccounts Access Token - {accessToken}");
                AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
            }

            //UpdateUI();
        }

        private void OnPlayerAccountSignedOut()
        {
            //UpdateUI();
        }

        private void OnPlayerAccountSignInFailed(RequestFailedException requestFailedException)
        {
            // UNITY PLAYER ACCOUNT SIGN IN FAILED
            Debug.Log($"Player Account Sign In Error : {requestFailedException.ErrorCode} : {requestFailedException.Message}");
            //UpdateUI();
        }
        #endregion

        #region Authentication Service Action Handlers
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

        private async void OnAuthSignInFailed(RequestFailedException requestFailedException)
        {
            // AUTHENTICATION SIGNED IN FAILED //
            Debug.Log($"Authentication SignIn Failed : {requestFailedException.ErrorCode} : {requestFailedException.Message}");
        }

        private void OnAuthSignInExpired()
        {
            // AUTHENTICATION TOKEN EXPIRED
            Debug.Log("Authentication Service Token Expired");
        }
        #endregion

        #region Username
        /*public async void SetUsername()
        {
            if(usernameInput.text == "")
            {
                print("Username failed to update.");
                return;
            }
            
            await AuthenticationService.Instance.UpdatePlayerNameAsync(usernameInput.text);
            print("Username updated.");

            //UpdateUI();
        }*/
        #endregion

        #region Auth Related Tasks
        private async Task Authenticate()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                AuthResult?.Invoke();
            }
            catch (AuthenticationException ex)
            {
                print(ex);

                AuthFailed?.Invoke();
            }
            catch (RequestFailedException ex)
            {
                print(ex);
                
                AuthFailed?.Invoke();
            }
        }
        #endregion

        #region Unity Cloud Save
        public async void SaveCloudData(bool firstSave)
        {
            var data = new Dictionary<string, object> { { cloudDataKey, cloudData } };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);

            print("CloudData saved.");

            if(firstSave)
                CloudDataSuccess?.Invoke();
        }

        public async void LoadCloudData()
        {
            try 
            { 
                var request = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { cloudDataKey });

                string loadedDataString = request[cloudDataKey];
                CloudData deserializedData = JsonConvert.DeserializeObject<CloudData>(loadedDataString);
                cloudData = deserializedData;

                print("CloudData loaded.");

                CloudDataSuccess?.Invoke();
            }
            catch(CloudSaveException ex) 
            {
                CloudDataFailed?.Invoke();

                print($"CloudData failed: {ex}"); 
            }
        }

        public async void CheckCloudData()
        {
            List<string> keyList = await CloudSaveService.Instance.Data.RetrieveAllKeysAsync();

            if(keyList.Count == 0)
            {
                print("No CloudData found. Creating new Save...");

                cloudData.unlockedCarsDict.Add("Bane", new CloudAutoData());

                cloudData.unlockedCarsDict["Bane"].compCodes.Add("Engine_LVL1");
                cloudData.unlockedCarsDict["Bane"].compCodes.Add("Gearbox_LVL1");
                cloudData.unlockedCarsDict["Bane"].compCodes.Add("Tires_LVL1");
                cloudData.unlockedCarsDict["Bane"].compCodes.Add("Armor_LVL1");

                for (int i = 0; i < 4; i++)
                    cloudData.unlockedCarsDict["Bane"].lastSelectedCompList.Add(0);

                SaveCloudData(true);
            }
            else
            {
                print("CloudData found. Loading...");

                LoadCloudData();
            }
        }

        public void AddCloudCarCode(string AutoCode)
        {

        }

        public void AddCloudCompCode(string CompCode)
        {

        }
        #endregion
    }

    [Serializable]
    public class CloudData
    {
        public int retroDollars = 500;
        [Space]
        public int highScore = 0;
        public int highestNearMissCombo = 0;
        public int mostCOPsDestroyed = 0;
        [Space]
        public Dictionary<string, CloudAutoData> unlockedCarsDict = new Dictionary<string, CloudAutoData>();
        public int lastSelectedCarInt = 0;
    }

    [Serializable]
    public class CloudAutoData
    {
        public List<string> compCodes = new List<string>();
        public List<int> lastSelectedCompList = new List<int>();
    }
}
