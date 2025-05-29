using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization; // CultureInfo için eklendi

public class HistoryItem : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text productNameText;
    public TMP_Text priceDetailsText;
    public TMP_Text profitText;

    public void Setup(string name, float costPrice, float salePrice)
    {
        // Ürün adı (değişmedi)
        productNameText.text = name;

        // Alış-satış detayları (C2 kaldırıldı)
        priceDetailsText.text = $"Purchase: ${costPrice:0.00} | Sale: ${salePrice:0.00}";

        // Kar hesaplama (C2 kaldırıldı)
        float profit = salePrice - costPrice;
        string profitFormatted = profit >= 0 ?
            $"<color=green>Profit: +${Mathf.Abs(profit):0.00}</color>" :
            $"<color=red>Loss: -${Mathf.Abs(profit):0.00}</color>";

        profitText.text = profitFormatted;
    }
}