using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �@�گu�� (���m+�U�������ˮ`+2)
/// </summary>
[CreateAssetMenu(fileName = "Skill_HuWoZhenShen", menuName = "Cards/Skill/護我真身")]
public class Skill_HuWoZhenShen : CardBase
{
    public int blockValue = 8;
    public int nextAttackBoost = 2;

    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // +���m
        player.AddBlock(blockValue);
        // �U�i�����ˮ`+2 => �i�H�b Player ���]�� buffs.nextAttackPlus = nextAttackBoost;
        player.buffs.nextAttackPlus = nextAttackBoost; // �ݧA�b Player �����w�q buffs
    }
}
