using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region �����P1-4�G�����]�p

/// <summary>
/// �X�� (�C�O�}�W�q)
/// </summary>
[CreateAssetMenu(fileName = "Attack_QuXie", menuName = "Cards/Attack/�X��")]
public class Attack_QuXie : CardBase
{
    public int damage = 4;
    public int dispelCount = 1; // �i�X���ĤH�W�q�h��

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int dmg = player.CalculateAttackDamage(damage);
        enemy.TakeDamage(dmg);
        // �̻ݨD�A�����ĤH���W�qbuff
        enemy.DispelBuff(dispelCount);
    }
}

/// <summary>
/// �u�V (�Y���^�X�ϥιL���m�P, �h+�B�~�ˮ`)
/// </summary>
[CreateAssetMenu(fileName = "Attack_ZhenXun", menuName = "Cards/Attack/�u�V")]
public class Attack_ZhenXun : CardBase
{
    public int baseDamage = 10;
    public int bonusDamage = 4;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // ���] Player ���� bool usedDefenseThisTurn ������
        // �� check block>0 �]�i�H
        bool usedDefense = (player.block > 0); // ²�ƥ�: �Y��block, ���ܥιL���m
        int totalDamage = baseDamage;
        if (usedDefense)
        {
            totalDamage += bonusDamage;
        }

        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);
    }
}

#endregion

#region �����P5-10�G�s�W�]�p

/// <summary>
/// �F�����
/// </summary>
[CreateAssetMenu(fileName = "Attack_LingQiaoChuanCi", menuName = "Cards/Attack/�F�����")]
public class Attack_LingQiaoChuanCi : CardBase
{
    public int baseDamage = 6;
    public int bonusDamageIfDiscard = 3;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // �ˬd���^�X�O�_��P
        bool hasDiscarded = player.hasDiscardedThisTurn;
        int totalDamage = baseDamage;
        if (hasDiscarded)
        {
            totalDamage += bonusDamageIfDiscard;
        }
        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);
    }
}

/// <summary>
/// �U�ɱ� (�i��1�i�P�A���@��)
/// </summary>
[CreateAssetMenu(fileName = "Attack_RanJinZhan", menuName = "Cards/Attack/�U�ɱ�")]
public class Attack_RanJinZhan : CardBase
{
    public int damage = 5;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int dmg1 = player.CalculateAttackDamage(damage);
        enemy.TakeDamage(dmg1);

        // �� 1 �i�P -> �Y���\�A�A������
        bool hasDiscard = player.DiscardOneCard();
        if (hasDiscard)
        {
            int dmg2 = player.CalculateAttackDamage(damage);
            enemy.TakeDamage(dmg2);
        }
    }
}

/// <summary>
/// �H�ҽ��� (�}���Ĥ賡�����m)
/// </summary>
[CreateAssetMenu(fileName = "Attack_SuiJiaChongJi", menuName = "Cards/Attack/�H�ҽ���")]
public class Attack_SuiJiaChongJi : CardBase
{
    public int damage = 8;
    public int reduceBlock = 5;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // ���}���Ĥ� block
        enemy.ReduceBlock(reduceBlock);
        // �A�y���ˮ`
        int dmg = player.CalculateAttackDamage(damage);
        enemy.TakeDamage(dmg);
    }
}

/// <summary>
/// ���� (�Y���^�X��� >= N , �B�~�ˮ`)
/// </summary>
[CreateAssetMenu(fileName = "Attack_DunJi", menuName = "Cards/Attack/����")]
public class Attack_DunJi : CardBase
{
    public int baseDamage = 4;
    public int blockThreshold = 6;
    public int bonusDamage = 4;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int totalDamage = baseDamage;
        if (player.block >= blockThreshold)
        {
            totalDamage += bonusDamage;
        }
        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);
    }
}

/// <summary>
/// �ìy��̼C (��P���ƶV�h, �ˮ`�V��)
/// </summary>
[CreateAssetMenu(fileName = "Attack_LuanLiuShuriken", menuName = "Cards/Attack/�ìy��̼C")]
public class Attack_LuanLiuShuriken : CardBase
{
    public int baseDamagePerDiscard = 2;
    public int baseDamageIfNoDiscard = 2; // �Y�L��P����, ���ӫO���ˮ`

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // �Y���^�X��P�L�X�i? ���ܽd�u�O�� hasDiscardedThisTurn (bool)
        // �Y�Q���, �i�H�b Player �̬��� discardCountThisTurn (int)
        bool hasDiscarded = player.hasDiscardedThisTurn;
        int discardCount = player.discardCountThisTurn; // ���]�A�b Player �����F���ܼ�
        if (discardCount < 0) discardCount = 0;

        int totalDamage = 0;
        if (!hasDiscarded)
        {
            // �L��P���p
            totalDamage = baseDamageIfNoDiscard;
        }
        else
        {
            // ����P, �̱�P�ƭp��
            totalDamage = discardCount * baseDamagePerDiscard;
            if (totalDamage <= 0) totalDamage = baseDamageIfNoDiscard;
        }

        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);
    }
}

/// <summary>
/// �F�N��ŧ (��1��1, �Y�󱼪��O�ޯ�P�h�l�[�ˮ`)
/// </summary>
[CreateAssetMenu(fileName = "Attack_PianShuTuXi", menuName = "Cards/Attack/�F�N��ŧ")]
public class Attack_PianShuTuXi : CardBase
{
    public int baseDamage = 9;
    public int bonusDamage = 3;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // �y����¦�ˮ`
        int dmg = player.CalculateAttackDamage(baseDamage);
        enemy.TakeDamage(dmg);

        // ��1�i
        player.DrawCards(1);
        // ��1�i(���B²��, ������̫�@�i)
        CardBase lastCard = null;
        if (player.Hand.Count > 0)
        {
            BattleManager manager = FindObjectOfType<BattleManager>();
            for (int i = player.Hand.Count - 1; i >= 0; i--)
            {
                CardBase candidate = player.Hand[i];
                if (manager != null && manager.IsGuaranteedMovementCard(candidate))
                {
                    continue;
                }

                lastCard = candidate;
                player.Hand.RemoveAt(i);
                player.discardPile.Add(lastCard);
                player.hasDiscardedThisTurn = true;
                player.discardCountThisTurn++; // �ݽT�O��������
                break;
            }
        }

        // �Y�󱼪��P�O�ޯ�P, �h��ĤH�A�y�� bonusDamage
        if (lastCard != null && lastCard.cardType == CardType.Skill)
        {
            int bonus = player.CalculateAttackDamage(bonusDamage);
            enemy.TakeDamage(bonus);
        }

    }
}

#endregion

