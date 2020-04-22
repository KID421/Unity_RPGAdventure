using UnityEngine;

[CreateAssetMenu(fileName = "怪物資料", menuName = "KID/怪物資料")]
public class EnemyData : ScriptableObject
{
    [Header("血量"), Range(50, 5000)]
    public float hp = 100;
    [Header("攻擊力"), Range(10, 500)]
    public float attack = 50;
    [Header("攻擊冷卻時間"), Range(1, 10)]
    public float attackCD = 5;
    [Header("移動速度"), Range(1, 50)]
    public float speed = 2.5f;
    [Header("追蹤範圍"), Range(1, 100)]
    public float rangeTrack = 10;
    [Header("攻擊範圍"), Range(1, 30)]
    public float rangeAttack = 5;
}
