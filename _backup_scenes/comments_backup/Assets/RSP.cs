using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RSP : MonoBehaviour
{
    public static RSP Instance;

    public List<RoleData> roleDatas = new List<RoleData>();
    public TextAsset roleTextAsset;

    [Header("角色列表")]
    public Transform _roleList;               // 拖场景里的 "Role List" 物体（角色列表容器）
    public GameObject role_Prefab;            // 拖 Role 预制体

    [Header("详情面板 UI")]
    public TextMeshProUGUI _roleName;         // 拖角色名文本
    public Image _avatar;                     // 拖头像图片
    public TextMeshProUGUI _roleDescribe;     // 拖描述文本
    public TextMeshProUGUI _text3;            // 拖记录文本

    [Header("随机选角")]
    public Button _randomButton;              // 留空则按名字找场景里的 "Role Random"

    private readonly List<Role> spawned = new List<Role>();
    private Image _randomAvatar;              // Role Random 下面的头像，显示抽到的角色

    private void Awake()
    {
        // 单例安全写法
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // 动态资源用 Resources.Load 是合理的（Role 预制体和 role.json 都在 Resources 下）
        role_Prefab = Resources.Load<GameObject>("Prefabs/Role");
        roleTextAsset = Resources.Load<TextAsset>("Data/role");

        if (roleTextAsset != null)
        {
            roleDatas = JsonConvert.DeserializeObject<List<RoleData>>(roleTextAsset.text);
        }
        else
        {
            Debug.LogError("未找到 Resources/Data/role.json 文件！");
        }
    }

    void Start()
    {
        if (roleDatas == null || roleDatas.Count == 0)
        {
            Debug.LogWarning("角色数据列表为空！");
            return;
        }

        if (_roleList == null)
        {
            Debug.LogError("_roleList 未绑定！请在 Inspector 把角色列表容器拖到 _roleList 槽位");
            return;
        }

        if (role_Prefab == null)
        {
            Debug.LogError("Role 预制体加载失败！");
            return;
        }

        // 解锁状态按存档重算一遍，JSON 里的 unlock 只是初始值
        foreach (RoleData roleData in roleDatas)
            roleData.unlock = UnlockChecker.IsUnlocked(roleData) ? 1 : 0;

        spawned.Clear();
        foreach (RoleData roleData in roleDatas)
        {
            Role r = Instantiate(role_Prefab, _roleList).GetComponent<Role>();
            r.SetData(roleData);
            spawned.Add(r);
        }

        BindRandomButton();
    }

    /// <summary>「Role Random」在场景里只是个图片，Button 是这里补上的</summary>
    private void BindRandomButton()
    {
        if (_randomButton == null)
        {
            GameObject go = GameObject.Find("Role Random");
            if (go != null)
            {
                _randomButton = go.GetComponent<Button>();
                if (_randomButton == null) _randomButton = go.AddComponent<Button>();

                Image img = go.GetComponent<Image>();
                if (img != null)
                {
                    _randomButton.targetGraphic = img;
                    img.raycastTarget = true;
                }

                Transform avatar = go.transform.Find("Avatar");
                if (avatar != null) _randomAvatar = avatar.GetComponent<Image>();
            }
        }

        if (_randomButton == null)
        {
            Debug.LogWarning("RSP：场景里找不到 Role Random，随机选角不可用");
            return;
        }

        _randomButton.onClick.AddListener(SelectRandomRole);
    }

    /// <summary>在已解锁的角色里随机挑一个</summary>
    public void SelectRandomRole()
    {
        List<Role> pool = spawned.FindAll(r => r != null && r.IsUnlocked);
        if (pool.Count == 0)
        {
            Debug.LogWarning("RSP：没有已解锁的角色，随机不了");
            Toast.Show("还没有解锁任何角色");
            return;
        }

        Role picked = pool[Random.Range(0, pool.Count)];
        picked.Select();
        picked.RenewUI(picked.roleData);

        if (_randomAvatar != null && !string.IsNullOrEmpty(picked.roleData.avatar))
            _randomAvatar.sprite = Resources.Load<Sprite>(picked.roleData.avatar);
    }
}
