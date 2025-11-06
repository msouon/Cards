using System.Collections.Generic;                      // 引用 List<>
using UnityEngine;                                     // 引用 Unity 的基礎類型

// 建立一個可以在 Unity 專案中建立的 ScriptableObject，放在「Run/Encounter Definition」選單下
[CreateAssetMenu(menuName = "Run/Encounter Definition", fileName = "RunEncounter")]
public class RunEncounterDefinition : ScriptableObject // 這個資產代表「一場戰鬥的定義」
{
    // 戰鬥的識別碼，可選填。用來唯一標示這一場戰鬥，方便存檔或除錯
    [SerializeField] private string encounterId;
    // 這一場戰鬥要生成的敵人群組清單，一場戰鬥可以有多個 EnemySpawnConfig
    [SerializeField] private List<EnemySpawnConfig> enemyGroups = new List<EnemySpawnConfig>();

    // 對外的唯讀屬性：如果 encounterId 沒填，就用這個 ScriptableObject 的名字當作 ID
    public string EncounterId => string.IsNullOrEmpty(encounterId) ? name : encounterId;
    // 對外的唯讀屬性：讓別人可以看到這場戰鬥有哪些敵人配置
    public IReadOnlyList<EnemySpawnConfig> EnemyGroups => enemyGroups;
}
