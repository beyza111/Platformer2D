using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    public enum CustomerType { Normal, VIP, Bargainer, Impulsive }
    public CustomerType customerType;

    [Header("Core Settings")]
    public Product.ProductType desiredProductType;
    public float basePatienceTime = 30f;
    public float basePaymentMultiplier = 1f;
    public bool ReadyToCheckout { get; set; }
    public bool IsLeaving { get; private set; }

    [Header("Visuals")]
    public Image desiredProductImage;
    public Product[] allProducts;  // Tüm ürün prefab'ları burada olacak
    public Sprite[] productIcons;

    private float currentPatience;
    public bool IsServed { get; private set; }
    public System.Action OnCustomerLeft;

    void Start()
    {
        InitializeCustomer();
    }
    void Awake()
    {
        allProducts = FindObjectsOfType<Product>();  // Sahnedeki tüm Product objelerini bulur
    }


    void InitializeCustomer()
    {
        // Rastgele müşteri tipi belirle
        customerType = (CustomerType)Random.Range(0, System.Enum.GetValues(typeof(CustomerType)).Length);
        Debug.Log($"[Customer] Customer type set to: {customerType}");

        // Müşteri özelliklerini yapılandır
        switch (customerType)
        {
            case CustomerType.VIP: ConfigureCustomer(1.5f, 45f); break;
            case CustomerType.Bargainer: ConfigureCustomer(0.7f, 40f); break;
            case CustomerType.Impulsive: ConfigureCustomer(1f, 15f); break;
            default: ConfigureCustomer(1f, 30f); break;
        }

        // Rastgele istenen ürün türü seç
        desiredProductType = (Product.ProductType)Random.Range(0, System.Enum.GetValues(typeof(Product.ProductType)).Length);
        Debug.Log($"[Customer] Desired product type: {desiredProductType} (Index: {(int)desiredProductType})");

        // Ürünü bul
        Product desiredProduct = GetProductByType(desiredProductType);
        if (desiredProductImage != null && desiredProduct != null)
        {
            Sprite productSprite = desiredProduct.GetProductIcon();
            desiredProductImage.sprite = productSprite;

            if (productSprite != null)
                Debug.Log($"[Customer] Assigned sprite: {productSprite.name} to desiredProductImage.");
            else
                Debug.LogWarning("[Customer] WARNING: Product sprite is null!");
        }
        else
        {
            Debug.LogError("[Customer] desiredProductImage or desiredProduct is NULL!");
        }


        // Müşterinin sabır değerini ayarla
        currentPatience = basePatienceTime;
    }
    Product GetProductByType(Product.ProductType type)
    {
        // Tüm ürünleri gez ve doğru ürünü bul
        foreach (var product in allProducts)
        {
            if (product.productType == type)
                return product;
        }
        return null;  // Eğer ürün bulunmazsa null döner
    }



    void ConfigureCustomer(float paymentMult, float patience)
    {
        basePaymentMultiplier = paymentMult;
        basePatienceTime = patience;
    }

    public bool IsActiveInScene { get; set; } = false; // Müşteri sahnedeyse true olacak

    public bool TrySellProduct(Product product)
    {
        Debug.Log("[DEBUG] TrySellProduct çağrıldı."); // Satış denemesi başladı
        Debug.Log($"[DEBUG] Müşteri Tipi: {customerType}, İstenen Ürün Tipi: {desiredProductType}");
        Debug.Log($"[DEBUG] Verilen Ürün Adı: {product.productName}, Tipi: {product.GetProductType()}");

        // 🚫 Müşteri sahnede değilse veya zaten işlem gördüyse satış yapılamaz
        if (!IsActiveInScene || IsServed || IsLeaving)
        {
            Debug.LogWarning("[WARN] Müşteri aktif değil, zaten hizmet aldı veya ayrılıyor.");
            return false;
        }

        IsServed = true;

        // Ürün tipi kontrolü
        bool isCorrect = product.GetProductType() == desiredProductType;
        Debug.Log($"[DEBUG] Ürün Tipi Kontrolü: İstenen = {desiredProductType}, Verilen = {product.GetProductType()}");
        Debug.Log($"[DEBUG] Eşleşme Durumu: {(isCorrect ? "✅ Başarılı" : "❌ Başarısız")}");

        float payment = CalculatePayment(product.GetSalePrice(), isCorrect);

        GameManager.Instance.AddMoney(payment);
        SalesUI.Instance?.ShowCustomerReaction(isCorrect, customerType);

        if (!isCorrect)
        {
            if (TryGetComponent(out CustomerAI customerAI))
                customerAI.TriggerAngryReaction();

            // ❌ Yanlış ürünse ürün sahneden 2 saniye sonra silinir
            Debug.Log($"[SATIŞ REDDEDİLDİ] Yanlış ürün verildi: {product.productName} ({product.GetProductType()})");
            Object.Destroy(product.gameObject, 2f);
        }
        else
        {
            if (TryGetComponent(out CustomerAI customerAI))
                customerAI.OnCheckoutComplete();

            product.SellProduct();

            // ✅ Doğru ürün satıldığında istek görseli gizlenir
            if (desiredProductImage != null)
                desiredProductImage.gameObject.SetActive(false);

            Debug.Log($"[SATIŞ TAMAMLANDI] {product.productName} ({product.GetProductType()}) ürünü {customerType} müşterisine başarıyla satıldı. Ödeme: ${payment:0.00} 💰");
        }

        return isCorrect;
    }





    float CalculatePayment(float basePrice, bool isCorrect)
    {
        float multiplier = isCorrect ? basePaymentMultiplier : basePaymentMultiplier * 0.5f;

        if (isCorrect)
        {
            if (customerType == CustomerType.Impulsive)
                multiplier *= 1.2f;
            else if (customerType == CustomerType.VIP)
                multiplier *= 1.1f;
        }
        else if (customerType == CustomerType.Bargainer && Random.value < 0.3f)
        {
            multiplier *= 0.9f;
        }

        return basePrice * multiplier;
    }

    void Update()
    {
        if (IsServed || ReadyToCheckout || IsLeaving) return;

        currentPatience -= Time.deltaTime;

        if (currentPatience <= 0f)
        {
            LeaveShop();
        }
    }

    public void LeaveShop()
    {
        if (IsLeaving) return;

        IsLeaving = true;
        OnCustomerLeft?.Invoke();

        GetComponent<CustomerAI>().ForceLeave();
    }

    public void FinalizeExit()
    {
        Destroy(gameObject);
    }
}
