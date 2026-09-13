using System;
using System.Collections.Generic;

[Serializable]
public class WaveEnemyData
{
    public string enemyName;
    public int timeAxis;
    public int count;
    public int elite;
}

[Serializable]
public class WaveData
{
    public int id;
    public int waveTimer;
    public List<WaveEnemyData> enemys;
}
