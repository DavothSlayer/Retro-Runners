using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroCode
{
    public class DevMenu : MonoBehaviour
    {
        [SerializeField]
        private UGSM gamingServicesManager;

        public void AddMoneyMethod()
        {
            int dollars = gamingServicesManager.cloudData.RetroDollars;
            dollars += 5000;

            gamingServicesManager.cloudData.RetroDollars = dollars;

            gamingServicesManager.SaveCloudData(false);

            print("5000$ Added to Account.");
        }

        public void RemoveMoneyMethod()
        {
            int dollars = gamingServicesManager.cloudData.RetroDollars;
            dollars -= 500;

            gamingServicesManager.cloudData.RetroDollars = dollars;

            gamingServicesManager.SaveCloudData(false);

            print("$500 Removed from Account.");
        }

        public void DeleteDataMethod()
        {
            //gm.DeleteData();
        }
    }
}
