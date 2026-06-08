using UnityEngine;

public class MercenaryHireManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MerchantData merchantData;

    [Header("Hire Target")]
    [SerializeField] private MercenaryDataSO targetMercenary;
   
    public void HireMercenary()
    {
        if (merchantData == null)
        {
            Debug.LogError("MerchantData が設定されていません");
            return;
        }

        if (targetMercenary == null)
        {
            Debug.LogError("雇用対象の傭兵データが設定されていません");
            return;
        }

        int hireCost = targetMercenary.hireCost;

        if (!merchantData.CanPay(hireCost))
        {
            Debug.Log($"{targetMercenary.mercenaryName}を雇用できません。所持金が足りません。");
            return;
        }

        merchantData.PayGold(hireCost);

        Debug.Log($"{targetMercenary.mercenaryName}を雇用しました");
    }
}