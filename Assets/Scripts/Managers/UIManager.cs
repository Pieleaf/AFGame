using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [ReadOnly]
    public bool paused = false;

    public InputActionAsset inputActions;

    [Header("Start Menu")]
    public GameObject StartUI;
    public Button ContinueButton;

    [Header("Tutorial")]
    public GameObject TutorialUI;

    [Header("Pause")]
    public GameObject PauseUI;

    [Header("HUD")]
    public GameObject HudUI;
    public TMP_Text  HudCollectedNumber;

    [Header("Shop")]
    public GameObject ShopUI;
    public Button ShopBirdButton;
    public Button ShopCollectionButton;
    public TMP_Text ShopBirdCost;
    public TMP_Text ShopCollectionCost;

    [Header("Settings")]
    public GameObject SettingsUI;

    public Slider MasterVolSlider;
    public Slider SFXVolSlider;
    public Slider MusicVolSlider;

    private void Awake()
    {
        Instance = this;

        ShopUI.SetActive(false);
        PauseUI.SetActive(false);
        SettingsUI.SetActive(false);
        HudUI.SetActive(true);
        StartUI.SetActive(false);
        TutorialUI.SetActive(false);

        SetPaused(true);
    }

    public void OpenStartMenu()
    {
        StartUI.SetActive(true);
        SetPaused(true);
    }

    void Start()
    {
        MasterVolSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        SFXVolSlider.onValueChanged.AddListener(   AudioManager.Instance.SetSFXVolume);
        MusicVolSlider.onValueChanged.AddListener( AudioManager.Instance.SetMusicVolume);

        MasterVolSlider.value = AudioManager.Instance.getMasterVolume();
        SFXVolSlider.value    = AudioManager.Instance.getSFXVolume();
        MusicVolSlider.value  = AudioManager.Instance.getMusicVolume();

        if (SaveManager.Instance.loadedData == null)
            ContinueButton.interactable = false;
    }

    public void NewGame()
    {
        StartUI.SetActive(false);
        GameManager.Instance.StartGame();

        //SetShowTutorial(true); // We rely on TutorialSpot to show tutorial instead
    }

    public void ContinueGame()
    {
        SaveManager.Instance.ApplyLoadedData();

        StartUI.SetActive(false);
        GameManager.Instance.StartGame();
    }

    public void SetShowTutorial(bool showTutorial)
    {
        Debug.Log($"SetShowTutorial({showTutorial})");
        TutorialUI.SetActive(showTutorial);
    }

    public void ShopPurchasedBird()
    {
        ShopManager.Instance.PurchaseBird();
    }

    public void ShopPurchasedCollector()
    {
        ShopManager.Instance.UpgradeCollector();
    }

    public void OnShop()
    {
        if (!GameManager.Instance.gameStarted)
            return;

        if (ShopUI.activeSelf)
        {
            ShopUI.SetActive(false);
            SetPaused(false);
        }
        else
        {
            ShopUI.SetActive(true);
            SetPaused(true);
        }
    }

    public void SetPaused(bool paused)
    {
        Debug.Log($"{this} SetPaused({paused})");

        this.paused = paused;
        Time.timeScale = paused ? 0 : 1;
        SetCursorLocked(!paused);

        if (paused)
            inputActions.FindActionMap("Player").Disable();
        else
            inputActions.FindActionMap("Player").Enable();
    }

    public void Resume()
    {
        if (SettingsUI.activeSelf)
        {
            SettingsUI.SetActive(false);

            if (!StartUI.activeSelf) { 
                PauseUI.SetActive(true);
            }
            return;
        }
        ShopUI.SetActive(false);
        PauseUI.SetActive(false);
        SetPaused(false);
    }

    public void OnPause() // Toggle pause menus
    {
        Debug.Log($"{this} OnPause");

        if (!GameManager.Instance.gameStarted)
            return;

        if (ShopUI.activeSelf)
        {
            Resume();
            return;
        }
        if (SettingsUI.activeSelf)
        {
            SettingsUI.SetActive(false);
            PauseUI.SetActive(true);
            return;
        }

        if (paused)
            Resume();
        else
        {
            SetPaused(true);
            PauseUI.SetActive(true);
        }
    }

    public void OnSettings()
    {
        Debug.Log("OnSettings");
        SettingsUI.SetActive(true);
    }

    public void OnExit()
    {
        Debug.Log("OnExit");

        if (GameManager.Instance.gameStarted)
            SaveManager.Instance.SaveData();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void UpdateUI()
    {
        HudCollectedNumber.text = $"{SnakeManager.Instance.snake.Count}";
        ShopManager.Instance.UpdateShopUI();
    }

    void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
