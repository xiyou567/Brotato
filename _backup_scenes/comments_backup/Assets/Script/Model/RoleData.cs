using System;

[Serializable]

public class RoleData
{
    public int id;
    public string name;
    public string avatar;
    public string describe;
    public int slot;
    public int record;
    public int unlock;
    public string unlockConditions;

    // unlockConditions 是给人看的那句话，这两个才是能判定的规则
    public string conditionType;
    public int conditionValue;
    public PassiveData passive;
}
