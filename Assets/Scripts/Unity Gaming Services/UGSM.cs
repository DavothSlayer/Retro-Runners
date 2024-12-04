using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;
using V3CTOR;
using Unity.Services.CloudSave.Models;

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
        public UnityEvent AutoPartDelivered;
        #endregion

        #region Unity Methods
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
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);

            print("CloudData saved.");

            if(firstSave)
                CloudDataSuccess?.Invoke();
        }

        public async void LoadCloudData()
        {
            try 
            { 
                Dictionary<string, Item> request = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { cloudDataKey });

                if (request.TryGetValue(cloudDataKey, out var item))
                {
                    cloudData = item.Value.GetAs<CloudData>();

                    CloudDataSuccess?.Invoke();

                    print("CloudData loaded.");
                }
                else
                {
                    print($"CloudData failed.");
                }
            }
            catch(CloudSaveException ex) 
            {
                CloudDataFailed?.Invoke();

                print($"CloudData failed: {ex}"); 
            }
        }

        public async void CheckCloudData()
        {
            var keyList = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

            if(keyList.Count == 0)
            {
                print("No CloudData found. Creating new Save...");

                CloudData cd = new CloudData();

                cd.inventoryDict["bane"]["topspeed"].isLooted = true;
                cd.inventoryDict["bane"]["torque"].isLooted = true;
                cd.inventoryDict["bane"]["handling"].isLooted = true;
                cd.inventoryDict["bane"]["health"].isLooted = true;
                cd.inventoryDict["bane"]["power"].isLooted = true;

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

        #region Keeping Track of Deliveries
        [HideInInspector]
        public Dictionary<string, Dictionary<string, DateTime>> deliveryDictionary = new Dictionary<string, Dictionary<string, DateTime>>(0);
        [HideInInspector]
        public Dictionary<string, Dictionary<string, byte>> lootingDictionary = new Dictionary<string, Dictionary<string, byte>>(0);

        public void CheckDeliveryTimers()
        {
            List<string> carNames = new List<string>(cloudData.inventoryDict.Keys);

            // GO THROUGH ALL UNLOCKED CARS //
            for (int i = 0; i < cloudData.inventoryDict.Keys.Count; i++)
            {
                // GO THROUGH THE PARTS CLASSES //
                for (int j = 0; j < 5; j++)
                {
                    AutoPartData partDataIndex = cloudData.inventoryDict[carNames[i]][EXMET.IntToCompClass(j)];

                    // PART IS ORDERED? //
                    if (partDataIndex.CurrentLevel < partDataIndex.OrderedLevel)
                    {
                        if (!partDataIndex.isDelivered)
                        {
                            // IS THE CAR ALREADY IN THE DICTIONARY? //
                            if (!deliveryDictionary.ContainsKey(carNames[i]))
                                deliveryDictionary.Add(carNames[i], new Dictionary<string, DateTime> { });

                            // ADD IT TO DELIVERY DICTIONARY, WITH EXPECTED DELIVERY DATE. //
                            if (!deliveryDictionary[carNames[i]].ContainsKey(EXMET.IntToCompClass(j)))
                                deliveryDictionary[carNames[i]].Add(EXMET.IntToCompClass(j), partDataIndex.PurchaseDate.AddHours(compDataLists[j].Comps[partDataIndex.OrderedLevel].TimeToDeliver));

                            // PART IS ALREADY DELIVERED? //
                            DateTime currentTime = DateTime.UtcNow;
                            DateTime expectedDeliveryDate = deliveryDictionary[carNames[i]][EXMET.IntToCompClass(j)];

                            TimeSpan difference = expectedDeliveryDate - currentTime;

                            // PART DELIVERED, FINALIZE THE PART AS SUCH //
                            if (difference.TotalSeconds <= 0)
                            {
                                print($"{EXMET.IntToCompClass(j)} for {carNames[i]} delivered.");

                                // FINALIZE THE DELIVERY, THEN REMOVE IT FROM THE DELIVERY DICTIONARY //
                                cloudData.inventoryDict[carNames[i]][EXMET.IntToCompClass(j)].FinalizeDelivery();

                                // REMOVE THE DELIVERED PART FROM THE DELDICT, AND REMOVE CAR ALTOGETHER IF NO PARTS LEFT FOR THAT CAR //
                                deliveryDictionary[carNames[i]].Remove(EXMET.IntToCompClass(j));
                                if (deliveryDictionary[carNames[i]].Keys.Count == 0) deliveryDictionary.Remove(carNames[i]);

                                SaveCloudData(false);
                                AutoPartDelivered?.Invoke();
                            }
                        }

                        // DELIVERY FINALIZED, IS IT LOOTED? //
                        CheckAvailableLoot(partDataIndex, carNames[i], j);
                    }
                }
            }

            carNames.Clear();
        }

        private void HandleDeliveryTimers()
        {
            List<string> carNames = new List<string>(deliveryDictionary.Keys);

            // GO THROUGH ALL CARS WITH DELIVERIES //
            for(int i = 0; i < deliveryDictionary.Keys.Count; i++)
            {
                // GO THROUGH THE PARTS CLASSES //
                for(int j = 0; j < 5; j++)
                {
                    AutoPartData partDataIndex = cloudData.inventoryDict[carNames[i]][EXMET.IntToCompClass(j)];

                    if (!deliveryDictionary[carNames[i]].ContainsKey(EXMET.IntToCompClass(j))) continue;

                    DateTime currentTime = DateTime.UtcNow;
                    DateTime expectedDeliveryDate = deliveryDictionary[carNames[i]][EXMET.IntToCompClass(j)];

                    TimeSpan difference = expectedDeliveryDate - currentTime;

                    // PART DELIVERED, FINALIZE THE PART AS SUCH //
                    if(difference.TotalSeconds <= 0)
                    {
                        print($"{EXMET.IntToCompClass(j)} for {carNames[i]} delivered.");

                        // FINALIZE THE DELIVERY, THEN REMOVE IT FROM THE DELIVERY DICTIONARY //
                        cloudData.inventoryDict[carNames[i]][EXMET.IntToCompClass(j)].FinalizeDelivery();

                        // DELIVERY FINALIZED, IS IT LOOTED? //
                        CheckAvailableLoot(partDataIndex, carNames[i], j);

                        // REMOVE THE DELIVERED PART FROM THE DELDICT, AND REMOVE CAR ALTOGETHER IF NO PARTS LEFT FOR THAT CAR //
                        deliveryDictionary[carNames[i]].Remove(EXMET.IntToCompClass(j));
                        if (deliveryDictionary[carNames[i]].Keys.Count == 0) deliveryDictionary.Remove(carNames[i]);

                        SaveCloudData(false);
                        AutoPartDelivered?.Invoke();
                    }
                }
            }
        }

        public void CheckAvailableLoot(AutoPartData partData, string carName, int compStateIndex)
        {
            // PART IS LOOTED? //
            if (!partData.isLooted && partData.isDelivered)
            {
                // IS THE CAR ALREADY IN THE DICTIONARY? //
                if (!lootingDictionary.ContainsKey(carName))
                    lootingDictionary.Add(carName, new Dictionary<string, byte> { });

                // ADD IT TO DELIVERY DICTIONARY, WITH EXPECTED DELIVERY DATE. //
                if (!lootingDictionary[carName].ContainsKey(EXMET.IntToCompClass(compStateIndex)))
                    lootingDictionary[carName].Add(EXMET.IntToCompClass(compStateIndex), partData.OrderedLevel);
            }
        }

        public void CollectAvailableLoot()
        {
            List<string> carNames = new List<string>(lootingDictionary.Keys);

            // GO THROUGH ALL CARS IN DICTIONARY //
            for (int i = 0; i < lootingDictionary.Keys.Count; i++)
            {
                // GO THROUGH THE PARTS CLASSES //
                for (int j = 0; j < 5; j++)
                {
                    AutoPartData partDataIndex = cloudData.inventoryDict[carNames[i]][EXMET.IntToCompClass(j)];

                    if (lootingDictionary[carNames[i]].ContainsKey(EXMET.IntToCompClass(j)))
                    {
                        cloudData.inventoryDict[carNames[i]][EXMET.IntToCompClass(j)].FinalizeLooting();

                        // REMOVE THE LOOTED PART FROM THE LOOTDICT, AND REMOVE CAR ALTOGETHER IF NO PARTS LEFT FOR THAT CAR //
                        lootingDictionary[carNames[i]].Remove(EXMET.IntToCompClass(j)); continue;
                        //if (lootingDictionary[carNames[i]].Keys.Count == 0) lootingDictionary.Remove(carNames[i]); break;
                    }                   
                }

                if (lootingDictionary[carNames[i]].Keys.Count == 0) lootingDictionary.Remove(carNames[i]);
            }

            carNames.Clear();

            SaveCloudData(false);
        }
        #endregion
    }

    public class CloudData
    {
        public int RetroDollars = 500;
        public int HighScore = 0;
        public int HighestNearMissCount = 0;
        public int MostCOPsDestroyed = 0;
        public int MostNPCsDestroyed = 0;
        public byte LastSelectedCarIndex = 0;
        public byte TierBossDefeated = 0;
        public byte TierUnlocked = 0;
        public Dictionary<int, float> ActiveQuests = new();

        public Dictionary<string, Dictionary<string, AutoPartData>> inventoryDict = new()
        {
            {"bane", new Dictionary<string, AutoPartData>{
                {"topspeed", new AutoPartData(0, DateTime.UtcNow) },
                {"torque", new AutoPartData(0, DateTime.UtcNow) },
                {"handling", new AutoPartData(0, DateTime.UtcNow) },
                {"health", new AutoPartData(0, DateTime.UtcNow) },
                {"power", new AutoPartData(0, DateTime.UtcNow) },
            } },
            /*{"Next Car...", new Dictionary<string, AutoPartData>{
                {"engine", new AutoPartData(0, DateTime.UtcNow) },
                {"power", new AutoPartData(0, DateTime.UtcNow) },
                {"handling", new AutoPartData(0, DateTime.UtcNow) },
                {"health", new AutoPartData(0, DateTime.UtcNow) },
            } }*/
        };

        public void PurchaseCar(string itemCode, byte carIndex)
        {
            inventoryDict.Add(itemCode, new()
            {
                {"topspeed", new AutoPartData(0, DateTime.UtcNow) },
                {"torque", new AutoPartData(0, DateTime.UtcNow) },
                {"handling", new AutoPartData(0, DateTime.UtcNow) },
                {"health", new AutoPartData(0, DateTime.UtcNow) },
                {"power", new AutoPartData(0, DateTime.UtcNow) },
            });
        }
    }

    public class AutoPartData
    {
        public byte CurrentLevel { get; set; }
        public byte OrderedLevel { get; set; }
        public byte EquippedLevel { get; set; }
        public DateTime PurchaseDate { get; set; }
        public bool isDelivered { get; set; }
        public bool isLooted { get; set; }           

        public AutoPartData(byte currentLevel, DateTime orderDate)
        {
            this.CurrentLevel = currentLevel;
            this.OrderedLevel = currentLevel;
            this.EquippedLevel = currentLevel;
            this.PurchaseDate = orderDate;
            this.isDelivered = true;
            this.isLooted = true;                   
        }

        public void OrderPart(byte newLevel, DateTime orderDate)
        {
            this.OrderedLevel = newLevel;
            this.PurchaseDate = orderDate;
            this.isDelivered = false;
            this.isLooted = false;
        }

        public byte NextLevel()
        {
            if(CurrentLevel == 4)
                return 4;
            else
                return (byte)(this.CurrentLevel + 1);
        }

        public void FinalizeDelivery()
        {
            this.isDelivered = true;
            this.isLooted = false;
        }

        public void FinalizeLooting()
        {
            this.CurrentLevel = this.OrderedLevel;
            this.EquippedLevel = this.CurrentLevel;
            this.isLooted = true;
        }
    }

    [Serializable]
    public class ItemDatalist
    {
        public List<ItemData> Comps = new List<ItemData>(0);
    }
}
