using UnityEngine;
using System;
using UnityEngine.Events;
//using GooglePlayGames;
//using GooglePlayGames.BasicApi;
//using GooglePlayGames.BasicApi.SavedGame;
using V3CTOR;

namespace RetroCode
{
    public class GoogleManager : MonoBehaviour
    {
        //PLAYER DATA FROM CLOUD
        public CloudData cloudData;
        [Space]
        public UnityEvent CloudDataSuccess;
        public UnityEvent CloudDataFailed;
        [Space]
        public UnityEvent AuthResult;
        public UnityEvent AuthFailed;

        /*private void Start()
        {
            if (Social.localUser.authenticated)
            {
                LoadCloudData();
                return;
            }
            
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        }

        internal void ProcessAuthentication(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                print("Authentication Successful. Accessing Saved Data...");
                
                AuthResult.Invoke();
            }
            else
            {
                print("Authentication Failed.");

                AuthFailed.Invoke();
            }
        }

        #region UI Buttons
        public void ManualAuth()
        {
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
        }
        #endregion

        #region Cloud Save

        // SHOW DATA UI //
        public void ShowSelectUI()
        {
            uint maxNumToDisplay = 15;
            bool allowCreateNew = false;
            bool allowDelete = true;

            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            savedGameClient.ShowSelectSavedGameUI("Select saved game",
                maxNumToDisplay,
                allowCreateNew,
                allowDelete,
                OnSavedGameSelected);
        }

        private void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata game)
        {
            if (status == SelectUIStatus.SavedGameSelected)
            {
                // handle selected game save
            }
            else
            {
                // handle cancel or error
            }
        }
        // SHOW DATA UI //

        // SAVING DATA // 
        public void SaveCloudData()
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            if (Social.localUser.authenticated)
            {
                savedGameClient.OpenWithAutomaticConflictResolution("UserData", DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, SaveDataMethod);

                print("SaveCloudData...");
            }
            else
            {
                print("SaveCloudData failed, not authenticated.");
            }
        }

        private void SaveDataMethod(SavedGameRequestStatus status, ISavedGameMetadata meta)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                byte[] data = EXMET.ToBytes(cloudData);

                SavedGameMetadataUpdate updateForMetadata = new SavedGameMetadataUpdate.Builder().WithUpdatedDescription($"Game updated: {DateTime.Now}").Build();

                PlayGamesPlatform.Instance.SavedGame.CommitUpdate(meta, updateForMetadata, data, SaveDataCallBack);

                print("Saving Data successful.");
            }
            else
            {
                print($"Saving Data Failed: {status}");
            }
        }

        private void SaveDataCallBack(SavedGameRequestStatus status, ISavedGameMetadata meta)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                print("SaveData callback successful.");
            }
            else
            {
                print("SaveData callback Failed.");
            }
        }
        // SAVING DATA //

        // LOADING DATA //
        public void LoadCloudData()
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            if (Social.localUser.authenticated)
            {
                savedGameClient.OpenWithAutomaticConflictResolution("UserData", DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, LoadDataMethod);

                print("LoadCloudData...");
            }
            else
            {
                print("LoadCloudData failed, not authenticated.");

                CloudDataFailed.Invoke();
            }
        }

        private void LoadDataMethod(SavedGameRequestStatus status, ISavedGameMetadata meta)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            savedGameClient.ReadBinaryData(meta, ReadData);
        }

        private void ReadData(SavedGameRequestStatus status, byte[] data)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                print("ReadData successful.");

                if (EXMET.FromBytes(data) == null)
                {
                    print("Loaded data is empty.");

                    cloudData.highScore = "0";
                    cloudData.retroDollars = "500";
                    cloudData.highestNearMissCombo = "0";
                    cloudData.unlockedCarsCode = "BANE1";
                    cloudData.lastSelectedCarInt = "0";
                }
                else
                {
                    CloudData readData = EXMET.FromBytes(data);
                    cloudData.highScore = readData.highScore;
                    cloudData.retroDollars = readData.retroDollars;
                    cloudData.highestNearMissCombo = readData.highestNearMissCombo;
                    cloudData.unlockedCarsCode = readData.unlockedCarsCode;
                    cloudData.lastSelectedCarInt = readData.lastSelectedCarInt;
                }

                CloudDataSuccess.Invoke();
            }
            else
            {
                print("ReadData failed.");

                CloudDataFailed.Invoke();
            }
        }
        // LOADING DATA //

        // DELETING DATA //
        public void DeleteData()
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            if (Social.localUser.authenticated)
            {
                savedGameClient.OpenWithAutomaticConflictResolution(
                    "UserData", 
                    DataSource.ReadCacheOrNetwork, 
                    ConflictResolutionStrategy.UseLongestPlaytime, 
                    DeleteDataCallback
                    );

                print("Deleting Data...");
            }
            else
            {
                print("Deleting Data Failed: Not Authenticated.");
            }
        }

        private void DeleteDataCallback(SavedGameRequestStatus status, ISavedGameMetadata meta)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            if (status == SavedGameRequestStatus.Success)
            {
                savedGameClient.Delete(meta);

                print("Deleting Data Successful.");
            }
            else
            {
                print("Deleting Data Failed.");
            }
        }
        // DELETING DATA //

        #endregion*/
    }
}
