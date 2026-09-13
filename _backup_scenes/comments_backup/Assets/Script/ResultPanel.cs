using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 死亡/胜利结算面板，只负责标题和战绩文字。
/// 返回按钮交给 BackToMenuButton——它自带一个高 sortingOrder 的独立 Canvas，
/// 不会出现被别的全屏面板盖住、点了没反应的情况。
/// </summary>
public class ResultPanel : MonoBehaviour
{
    public static ResultPanel Instance;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;

    // 老按钮，代码里拆掉，场景不用动
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

        // 场景里那个 BackButton 已经不用了，留着只会在结算时露出来
        if (backButton != null) Destroy(backButton.gameObject);

        // 场景里 ResultPanel 上误挂了一个 RSP，在战斗场景里只会刷「_roleList 未绑定」的假报错
        RSP strayRsp = GetComponent<RSP>();
        if (strayRsp != null) Destroy(strayRsp);

        // 文字默认 raycastTarget=true，会铺在按钮上把点击吃掉
        foreach (TextMeshProUGUI t in GetComponentsInChildren<TextMeshProUGUI>(true))
            t.raycastTarget = false;

        // 不关折行的话，文本框偏窄时 "存活波次：3 / 5" 会从空格处窜行
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
            // 胜利时 currentWaveIndex 已经 == total（越界），+1 后会多 1，所以要限制在 total 内
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

        // 单局生命值峰值，给「公牛」的解锁条件用
        if (Player.Instance != null)
            SaveManager.NoteMaxHp(Mathf.RoundToInt(Player.Instance.maxHp));

        Time.timeScale = 0;

        gameObject.SetActive(true);

        if (backToMenu == null) backToMenu = BackToMenuButton.Ensure();
        if (backToMenu != null) backToMenu.Show();
    }
}
