using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Role : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image _backImage;
    public Image _avatar;
    public Button _button;
    public RoleData roleData;

    private static Role currentSelected;

    public static Role CurrentSelected => currentSelected;
    public bool IsUnlocked => roleData != null && roleData.unlock == 1;

    private void Awake()
    {
        _backImage = GetComponent<Image>();
        Transform avatar = transform.Find("Avatar");
        if (avatar != null) _avatar = avatar.GetComponent<Image>();
        _button = GetComponent<Button>();
    }

    public void SetData(RoleData roleData)
    {
        this.roleData = roleData;

        if (_avatar != null && roleData != null && !string.IsNullOrEmpty(roleData.avatar))
        {
            _avatar.sprite = Resources.Load<Sprite>(roleData.avatar);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_backImage != null)
            _backImage.color = new Color(207/255f, 207/255f, 207/255f);
        if(roleData != null)
            RenewUI(roleData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_backImage == null) return;

        if (currentSelected == this)
            _backImage.color = new Color(0.4f, 0.7f, 0.4f);
        else
            _backImage.color = new Color(34/255f, 34/255f, 34/255f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Select();
    }

    public void Select()
    {
        if (roleData == null) return;

        if (roleData.unlock == 0)
        {
            Debug.Log("角色未解锁: " + roleData.unlockConditions);
            Toast.Show("角色未解锁：" + roleData.unlockConditions);
            return;
        }

        GameData.selectedRoleId = roleData.id;
        Debug.Log("选择角色: " + roleData.name + " (id=" + roleData.id + ")");

        if (currentSelected != null && currentSelected != this && currentSelected._backImage != null)
            currentSelected._backImage.color = new Color(34/255f, 34/255f, 34/255f);

        if (_backImage != null)
            _backImage.color = new Color(0.4f, 0.7f, 0.4f);
        currentSelected = this;
    }

    public void RenewUI(RoleData r)
    {
        if (RSP.Instance == null || r == null) return;

        if (r.unlock == 0)
        {
            RSP.Instance._roleName.text = "?";
            RSP.Instance._avatar.sprite = Resources.Load<Sprite>("Image/UI/锁");
            RSP.Instance._roleDescribe.text = r.unlockConditions;
            RSP.Instance._text3.text = "尚无记录";
        }
        else
        {
            RSP.Instance._roleName.text = r.name;
            RSP.Instance._avatar.sprite = Resources.Load<Sprite>(r.avatar);
            RSP.Instance._roleDescribe.text = r.describe;
            RSP.Instance._text3.text = GetRecord(r.record);
        }
    }

    public string GetRecord(int rRecord)
    {
        switch (rRecord)
        {
            case -1:
                return "尚无记录";
            case 0:
                return "通关危险0";
            case 1:
                return "通关危险1";
            case 2:
                return "通关危险2";
            case 3:
                return "通关危险3";
            case 4:
                return "通关危险4";
            case 5:
                return "通关危险5";
            default:
                return "尚无记录";
        }
    }
}
