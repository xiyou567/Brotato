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

    public string conditionType;
    public int conditionValue;
    public PassiveData passive;
}
