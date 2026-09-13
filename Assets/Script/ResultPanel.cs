using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    public static ResultPanel Instance;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;

    [SerializeField] private Button backButton;

    private BackToMenuButton backToMenu;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        gameObject.SetActive(false);

        if (backButton != null) Destroy(backButton.gameObject);

        RSP strayRsp = GetComponent<RSP>();
        if (strayRsp != null) Destroy(strayRsp);

        foreach (TextMeshProUGUI t in GetComponentsInChildren<TextMeshProUGUI>(true))
            t.raycastTarget = false;

        if (statsText != null)
            statsText.enableWordWrapping = false;

        backToMenu = BackToMenuButton.Ensure();
    }

    public void Show(bool win)
    {
        Debug.Log("ResultPanel：结算面板显示，win=" + win);

        if (titleText != null)
            titleText.text = win ? "胜利！" : "失败";

        int wave = 0;
        int total = 0;
        if (LevelController.Instance != null)
        {
            int current = LevelController.Instance.CurrentWaveIndex;
            total = LevelController.Instance.TotalWaves;

            wave = Mathf.Clamp(current + 1, 1, total);
        }

        int killCount = 0;
        int money = 0;
        if (Player.Instance != null)
        {
            killCount = Player.Instance.killCount;
            money = Player.Instance.money;
        }

        if (statsText != null)
            statsText.text = "存活波次：" + wave + " / " + total
                + "\n击杀数：" + killCount
                + "\n金币：" + money;

        SaveManager.SaveResult(wave, killCount, money);

        if (Player.Instance != null)
            SaveManager.NoteMaxHp(Mathf.RoundToInt(Player.Instance.maxHp));

        Time.timeScale = 0;

        gameObject.SetActive(true);

        if (backToMenu == null) backToMenu = BackToMenuButton.Ensure();
        if (backToMenu != null) backToMenu.Show();
    }
}
