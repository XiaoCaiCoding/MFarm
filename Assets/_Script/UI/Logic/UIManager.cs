using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameObject menuCanvas;
    public GameObject menuPrefab;
    public Button settingButton;
    public GameObject settingPanel;
    public Slider volumeSlider;
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;

    }
    private void Awake()
    {
        settingButton.onClick.AddListener(TogglePausePanel);
        volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
    }
    private void Start()
    {
        menuCanvas = GameObject.FindWithTag("MenuCanvas");
        Instantiate(menuPrefab, menuCanvas.transform);
    }

    private void OnAfterSceneLoadedEvent()
    {
        if (menuCanvas.transform.childCount > 0)
            Destroy(menuCanvas.transform.GetChild(0).gameObject);
    }

    private void TogglePausePanel()
    {
        bool isOpen = settingPanel.activeInHierarchy;
        if (isOpen)
        {
            settingPanel.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            System.GC.Collect();
            settingPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    public void ReturnMenuCanvas()
    {
        Time.timeScale = 1;
        StartCoroutine(BackToMenu());
    }
    private IEnumerator BackToMenu()
    {
        settingPanel.SetActive(false);
        EventHandler.CallEndGameEvent();
        yield return new WaitForSeconds(1f);
        Instantiate(menuPrefab, menuCanvas.transform);
    }
}
