using System;
using UnityEngine;

namespace V3CTOR
{
    public class CharDemo : MonoBehaviour
    {
        public string cloudData = "";
        public string autoCode = "";

        // ALGORITHM TO FIND AUTO LEVELS IN CLOUD... //
        // LOAD THE CLOUD CODE FOR THE CAR, //
        // SWITCH LAST CHAR WITH NEW ONE (LEVEL) //
        // SAVE IT BACK TO THE CLOUD... //
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.K)) { return; }

            print(UnlockedAutoLevel(autoCode));
            LevelUpAuto(autoCode);
        }

        private int UnlockedAutoLevel(string autoName)
        {
            int startIndex = cloudData.IndexOf(autoName, StringComparison.CurrentCultureIgnoreCase);
            string autoSubString = cloudData.Substring(startIndex, autoName.Length + 1);
            string levelSubString = autoSubString.Substring(autoName.Length);

            return int.Parse(levelSubString);
        }

        private void LevelUpAuto(string autoName)
        {
            if(UnlockedAutoLevel(autoName) == 3) { print("Auto at Max Level."); return; }

            int startIndex = cloudData.IndexOf(autoName, StringComparison.CurrentCultureIgnoreCase);
            int? newLevel = UnlockedAutoLevel(autoName) + 1;

            // REPLACE OLD AUTO & LEVEL WITH NEW LEVEL //
            string newCloudData = cloudData.Remove(startIndex, autoName.Length + 1);
            newCloudData += autoName.ToUpper() + newLevel.ToString();
            cloudData = newCloudData;

            // SAVE TO CLOUD //

            print(cloudData);
        }
    }
}
