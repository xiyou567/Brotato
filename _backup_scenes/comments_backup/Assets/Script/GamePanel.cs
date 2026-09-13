using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    public static GamePanel Instance;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text moneyCount;
    [SerializeField] private TMP_Text expCount;
    [SerializeField] private TMP_Text hpCount;
    [SerializeField] private TMP_Text countDown;
    [SerializeField] private TMP_Text waveCount;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // 这个 Image 是全屏而且是全透明的容器，留着 raycast 会把返回按钮的点击全吃掉
        Image bg = GetComponent<Image>();
        if (bg != null) bg.raycastTarget = false;
    }

    void Start()
    {
        RenewMoney();
        RenewExp();
        RenewHp();
    }

    public void RenewHp()
    {
        if (Player.Instance == null) return;
        if (hpCount != null)
            hpCount.text = Player.Instance.hp + "/" + Player.Instance.maxHp;
        if (hpSlider != null)
            hpSlider.value = Player.Instance.hp / Player.Instance.maxHp;
    }

    public void RenewExp()
    {
        if (Player.Instance == null) return;
        if (expSlider != null)
            expSlider.value = Player.Instance.exp / Player.Instance.needExp;
        if (expCount != null)
            expCount.text = "LV." + Player.Instance.level;
    }

    public void RenewMoney()
    {
        if (Player.Instance == null) return;
        if (moneyCount != null)
            moneyCount.text = Player.Instance.money.ToString();
    }

    public void RenewCountDown(float seconds)
    {
        if (countDown == null) return;
        int s = Mathf.CeilToInt(seconds);
        countDown.text = s + "s";
    }

    public void RenewWave(int current, int total)
    {
        if (waveCount == null) return;
        waveCount.text = "第" + current + "/" + total + "波";
    }
}
