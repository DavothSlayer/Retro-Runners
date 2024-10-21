using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;
using V3CTOR;

namespace RetroCode
{
    public class UGSM : MonoBehaviour
    {
        public CloudData cloudData;

        [SerializeField]
        private List<ItemDatalist> compDataLists;

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
        [HideInInspector]
        public Dictionary<string, Dictionary<string, DateTime>> deliveryDictionary = new Dictionary<string, Dictionary<string, DateTime>>(0);
        private async void Start()
        {
            // INITIALIZE UNITY GAMING SERVICES //
            await UnityServices.InitializeAsync();

            // REGISTER AUTHENTICATION SERVICE HANDLERS //
            AuthenticationService.Instance.SignedIn += OnAuthSignedIn;
            AuthenticationService.Instance.SignedOut += OnAuthSignedOut;
            AuthenticationService.Instance.SignInFailed += OnAuthSignInFailed;
            AuthenticationService.Instance.Expired += OnAuthSignInExpired;

            // SIGN IN, IF NECESSARY //
            if (!AuthenticationService.Instance.IsSignedIn)
                await Authenticate();
            else
                LoadCloudData();
        }

        private void Update()
        {
            if (deliveryDictionary.Count != 0) HandleDeliveryTimers();            
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

                // SESSIONTOKEN INVALID OR ACCOUNT DELETED AND TRYING TO SIGN IN AGAIN //
                if(ex.ErrorCode == 10007)
                {
                    print("Account not found. Creating new one...");
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
        #endregion

        #region Keeping Track of Player Data
        public void CheckDeliveryTimers()
        {
            List<string> carNames = new List<string>(cloudData.inventoryDict.Keys);

            // GO THROUGH ALL UNLOCKED CARS //
            for (int i = 0; i < cloudData.inventoryDict.Keys.Count; i++)
            {
                // GO THROUGH THE PARTS CLASSES //
                for (int j = 0; j < cloudData.inventoryDict[carNames[i]].Keys.Count; j++)
                {
                    AutoPartData partDataIndex = cloudData.inventoryDict[carNames[i]][EXMET.IntToCompClass(j)];

                    // PART IS ORDERED? //
                    if (!partDataIndex.isDelivered && partDataIndex.currentLevel != partDataIndex.orderedLevel)
                    {
                        // ADD IT TO DELIVERY DICTIONARY, WITH EXPECTED DELIVERY DATE. //
                        if (!deliveryDictionary.ContainsKey(carNames[i]))
                        {
                            deliveryDictionary.Add(carNames[i], new Dictionary<string, DateTime>
                            {
                                { EXMET.IntToCompClass(j), partDataIndex.purchaseDate.AddHours(compDataLists[j].Comps[partDataIndex.orderedLevel].TimeToDeliver) }
                            });
                        }
                        else
                        {
                            deliveryDictionary[carNames[i]].Add(EXMET.IntToCompClass(j), partDataIndex.purchaseDate.AddHours(compDataLists[j].Comps[partDataIndex.orderedLevel].TimeToDeliver));
                        }
                    }
                }
            }

            carNames.Clear();
        }

        private void HandleDeliveryTimers()
        {
            List<string> carNames = new List<string>(deliveryDictionary.Keys);

            // GO THROUGH ALL CARS WITH DELIVERIES //
            for(int i = 0; i < carNames.Count; i++)
            {
                // GO THROUGH THE PARTS CLASSES //
                for(int j = 0; j < deliveryDictionary[carNames[i]].Keys.Count; j++)
                {
                    AutoPartData partDataIndex = cloudData.inventoryDict[carNames[i]][EXMET.IntToCompClass(j)];

                    DateTime currentTime = DateTime.UtcNow;
                    DateTime expectedDeliveryDate = deliveryDictionary[carNames[i]][EXMET.IntToCompClass(j)];

                    TimeSpan difference = expectedDeliveryDate - currentTime;

                    //print($"{carNames[i]}: {partDataIndex}: {difference.ToString("HH:mm:ss")}");

                    // PART DELIVERED, FINALIZE THE PART AS SUCH //
                    if(difference.TotalSeconds <= 0)
                    {
                        print($"{EXMET.IntToCompClass(j)} for {carNames[i]} delivered.");

                        cloudData.inventoryDict[carNames[i]][EXMET.IntToCompClass(j)].FinalizeDelivery();

                        SaveCloudData(false);

                        // REMOVE THE DELIVERED PART FROM THE DELDICT, AND REMOVE CAR ALTOGETHER IF NO PARTS LEFT FOR THAT CAR //
                        deliveryDictionary[carNames[i]].Remove(EXMET.IntToCompClass(j));
                        if (deliveryDictionary[carNames[i]].Keys.Count == 0) deliveryDictionary.Remove(carNames[i]);
                    }
                }
            }
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
        public byte currentLevel { get; set; }
        public byte orderedLevel { get; set; }
        public byte equippedLevel { get; set; }
        public DateTime purchaseDate { get; set; }
        public bool isDelivered { get; set; }
        public bool isLooted { get; set; }           

        public AutoPartData(byte currentLevel, DateTime orderDate)
        {
            this.currentLevel = currentLevel;
            this.orderedLevel = currentLevel;
            this.equippedLevel = currentLevel;
            this.purchaseDate = orderDate;
            this.isDelivered = true;
            this.isLooted = true;                   
        }

        public void OrderPart(byte newLevel, DateTime orderDate)
        {
            this.orderedLevel = newLevel;
            this.purchaseDate = orderDate;
            this.isDelivered = false;
            this.isLooted = false;
        }

        public byte NextLevel()
        {
            if(orderedLevel == 4)
                return (byte)4;
            else
                return (byte)(this.currentLevel + 1);
        }

        public void FinalizeDelivery()
        {
            this.isDelivered = true;
            this.isLooted = false;
        }

        public void FinalizeLooting()
        {
            this.currentLevel = this.orderedLevel;
            this.isLooted = true;
        }
    }

    [Serializable]
    public class ItemDatalist
    {
        public List<ItemData> Comps = new List<ItemData>(0);
    }
}
