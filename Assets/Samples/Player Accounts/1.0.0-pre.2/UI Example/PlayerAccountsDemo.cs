using System;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.PlayerAccounts.Samples
{
    class PlayerAccountsDemo : MonoBehaviour
    {
        [SerializeField]
        Text m_AccessTokenText;
        [SerializeField]
        Text m_StatusText;
        [SerializeField]
        GameObject m_TokenPanel;
        [SerializeField]
        Button m_LoginButton;

        async void Awake()
        {
            await UnityServices.InitializeAsync();
            PlayerAccountService.Instance.SignedIn +=  UpdateUI;
        }
        
        public async void StartSignInAsync()
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        }

        public async void RefreshToken()
        {
            await PlayerAccountService.Instance.RefreshTokenAsync();
            UpdateUI();
        }

        public void SignOut()
        {
            PlayerAccountService.Instance.SignOut();
            m_TokenPanel.SetActive(false);
            m_LoginButton.interactable = !m_LoginButton.interactable;
            m_StatusText.text = "";
        }

        public void OpenAccountPortal()
        {
            Application.OpenURL(PlayerAccountSettings.AccountPortalUrl);
        }

        void UpdateUI()
        {
            m_TokenPanel.SetActive(true);
            m_LoginButton.interactable = false;
            m_AccessTokenText.text = "<b>Access Token :</b> \n" + PlayerAccountService.Instance.AccessToken + "\n";
            m_StatusText.text = "<b>Request Successful!</b>";
        }
    }
}
