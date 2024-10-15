using Unity.Services.Authentication;
using Unity.Services.Core;
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
                print(ex.ErrorCode);

                // SESSIONTOKEN INVALID, ACCOUNT DELETED AND TRYING TO SIGN IN FOR THE 1ST TIME //

                if(ex.ErrorCode == 10007)
                {
                    print("account fucked.");
                    AuthenticationService.Instance.ClearSessionToken();
                    await Authenticate();
                }

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

                CloudData cd = new CloudData();

                cd.inventoryDict["bane"]["engine"].isLooted = true;
                cd.inventoryDict["bane"]["power"].isLooted = true;
                cd.inventoryDict["bane"]["handling"].isLooted = true;
                cd.inventoryDict["bane"]["health"].isLooted = true;

                cloudData = cd;

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

    public class CloudData
    {
        public int retroDollars = 500;
        public int highScore = 0;
        public int highestNearMissCombo = 0;
        public int mostCOPsDestroyed = 0;
        public int lastSelectedCarInt = 0;

        public Dictionary<string, Dictionary<string, AutoPartData>> inventoryDict = new()
        {
            {"bane", new Dictionary<string, AutoPartData>{
                {"engine", new AutoPartData(0, DateTime.UtcNow) },
                {"power", new AutoPartData(0, DateTime.UtcNow) },
                {"handling", new AutoPartData(0, DateTime.UtcNow) },
                {"health", new AutoPartData(0, DateTime.UtcNow) },
            } },
            /*{"Next Car...", new Dictionary<string, AutoPartData>{
                {"engine", new AutoPartData(0, DateTime.UtcNow) },
                {"power", new AutoPartData(0, DateTime.UtcNow) },
                {"handling", new AutoPartData(0, DateTime.UtcNow) },
                {"health", new AutoPartData(0, DateTime.UtcNow) },
            } }*/
        };
    }

    public class AutoPartData
    {
        public int currentLevel { get; set; }
        public int orderedLevel { get; set; }
        public DateTime purchaseDate { get; set; }
        public bool isDelivered { get; set; }
        public bool isLooted { get; set; }           

        public AutoPartData(int currentLevel, DateTime purchaseDate)
        {
            this.currentLevel = currentLevel;
            this.orderedLevel = currentLevel;
            this.purchaseDate = purchaseDate;
            this.isDelivered = true;
            this.isLooted = false;                   
        }

        public void OrderPart(int newLevel, DateTime orderDate)
        {
            this.orderedLevel = newLevel;
            this.purchaseDate = orderDate;
            this.isDelivered = false;
        }

        public int NextLevel()
        {
            if(orderedLevel == 4)
                return 4;
            else
                return this.currentLevel + 1;
        }

        public void FinalizeDelivery()
        {
            if (!isDelivered)
            {
                this.currentLevel = this.orderedLevel;
                this.isDelivered = true;
            }
        }
    }
}
