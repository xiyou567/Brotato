using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button progressButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text recordText;
    void Start()
    {
        SaveManager.LoadAll();

        
        if (startButton == null) startButton = FindButton("StartButton");
        if (progressButton == null) progressButton = FindButton("ProgressButton");
        if (settingButton == null) settingButton = FindButton("SettingButton");
        if (exitButton == null) exitButton = FindButton("ExitButton");
        if (recordText == null) recordText = FindText("RecordText");

        if (startButton != null)
            startButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("02-LevelSelect");
            });

        if (progressButton != null && recordText != null)
            progressButton.onClick.AddListener(() =>
            {
                recordText.gameObject.SetActive(!recordText.gameObject.activeSelf);
            });

        if (settingButton != null)
            settingButton.onClick.AddListener(() =>
            {
                SettingsPanel.Toggle();
            });
        else
            Debug.LogWarning("MainMenuPanel：没找到 SettingButton");

        
        if (exitButton != null)
            exitButton.onClick.AddListener(() =>
            {
                Debug.Log("退出游戏");
                Application.Quit();
            });
        else
            Debug.LogWarning("MainMenuPanel：没找到 ExitButton");

        if (recordText != null)
            recordText.text = "最高波次：" + SaveManager.bestWave
                                      + "\n总击杀：" + SaveManager.totalKill
                                      + "\n总金币：" + SaveManager.totalMoney
                                      + "\n总局数：" + SaveManager.playCount;
        else
            Debug.LogWarning("MainMenuPanel：没找到 RecordText");
    }

   
    static Button FindButton(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go.GetComponent<Button>() : null;
    }

    static TMP_Text FindText(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go.GetComponent<TMP_Text>() : null;
    }
}
