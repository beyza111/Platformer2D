using UnityEngine;

public class Product : MonoBehaviour
{
    public enum ProductType
    {
        // Yüzükler (10 adet)
        Ring1,
        Ring2,
        Ring3,
        Ring4,
        Ring5,
        Ring6,
        Ring7,
        Ring8,
        Ring9,
        Ring10,

        // Kolyeler (10 adet)
        Necklace1,
        Necklace2,
        Necklace3,
        Necklace4,
        Necklace5,
        Necklace6,
        Necklace7,
        Necklace8,
        Necklace9,
        Necklace10,

        // Bilezikler (6 adet)
        Bracelet1,
        Bracelet2,
        Bracelet3,
        Bracelet4,
        Bracelet5,
        Bracelet6,

        // Küpeler (5 adet)
        Earring1,
        Earring2,
        Earring3,
        Earring4,
        Earring5
    }
    public ProductType productType;

    [Header("Ürün Bilgileri")]
    public Sprite productIcon;
    public string productName = "Ürün";
    public float costPrice = 10f;
    public float salePrice = 15f;

    // Getter-Setter'lar
    public float GetCostPrice() => costPrice;
    public float GetSalePrice() => salePrice;
    public void SetSalePrice(float newPrice) => salePrice = newPrice;
    public ProductType GetProductType() => productType;

    public Sprite GetProductIcon()
    {
        return productIcon;
    }

    public bool TrySellToCustomer(Customer customer)
    {
        Debug.Log($"[Product] Ürün adı: {productName} | Tip: {productType} → Müşteriye satılmaya çalışılıyor.");
        return customer.TrySellProduct(this);
    }


    public void SellProduct()
    {
        Debug.Log($"[Product] SATIŞ BAŞARILI → {productName} | Tip: {productType} | Fiyat: {salePrice}");

        GameManager.Instance.AddMoney(salePrice);
        GameManager.Instance.transactionHistory.Add(
            new GameManager.TransactionRecord(
                productName,
                costPrice,
                salePrice
            )
        );

        SalesUI.Instance?.RefreshHistoryUI();
        Destroy(gameObject);
    }

}