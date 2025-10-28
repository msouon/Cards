using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �Ҧ��d�P����H�����O�A�~�� ScriptableObject �K��b Unity ������
/// </summary>
public abstract class CardBase : ScriptableObject
{
    [Header("�d�P���ݩ�")]
    public string cardName;         // �d�P�W��
    public int cost;                // ��q����
    [TextArea] public string description;   // �ԭz��r
    public Sprite cardImage;        // �d���ϥ� (�i��)


    [Header("�d�P����")]
    public CardType cardType;

    /// <summary>
    /// ����d�P�ĪG (�Ѥl���O��@)
    /// </summary>
    /// <param name="player">���a</param>
    /// <param name="enemy">�ؼмĤH(����)</param>
    public abstract void ExecuteEffect(Player player, Enemy enemy);

    /// <summary>
    /// (�i��) �����ʥd�νd������d�ϥΪ��X�R
    /// �Ҧp�����d�ݭn���w��l�νd��
    /// </summary>
    /// 

    public virtual void ExecuteOnPosition(Player player, Vector2Int targetGridPos)
    {
        // �w�]�����ơA���ʥd�i�H�мg
    }
}
