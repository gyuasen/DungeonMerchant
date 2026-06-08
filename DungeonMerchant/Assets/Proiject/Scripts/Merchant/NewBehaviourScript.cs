using UnityEngine;

public class MerchantData : MonoBehaviour
{
    [Header("Merchant Status")]
    [SerializeField] private int gold = 500;

    public int Gold => gold;

    public bool CanPay(int amount)
    {
        return gold >= amount;
    }

    public void PayGold(int amount)
    {
        if (!CanPay(amount))
        {
            Debug.Log("所持金が足りません");
            return;
        }

        gold -= amount;
        Debug.Log($"支払い成功：-{amount}G / 所持金：{gold}G");
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"獲得：+{amount}G / 所持金：{gold}G");
    }
}