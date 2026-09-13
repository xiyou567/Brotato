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
    public Transform _roleList;
    public GameObject role_Prefab;

    [Header("详情面板 UI")]
    public TextMeshProUGUI _roleName;
    public Image _avatar;
    public TextMeshProUGUI _roleDescribe;
    public TextMeshProUGUI _text3;

    [Header("随机选角")]
    public Button _randomButton;

    private readonly List<Role> spawned = new List<Role>();
    private Image _randomAvatar;

    private void Awake()
    {

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

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
