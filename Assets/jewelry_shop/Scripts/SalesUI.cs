using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;
using System.Collections;


public class SalesUI : MonoBehaviour
{
    public static SalesUI Instance;

    [Header("UI Elements")]
    public GameObject salesPanel;
    public GameObject priceAdjustPanel;
    public GameObject historyPanel;
    public TMP_Text priceText;
    public TMP_Text totalMoneyText;
    public TMP_InputField priceInput;

    [Header("Buttons")]
    public Button yesButton;
    public Button noButton;
    public Button closeButton;
    public Button confirmButton;
    public Button historyButton;

    [Header("History System")]
    public Transform historyContent;
    public GameObject historyItemPrefab;

    [Header("Customer Reaction")]
    public TMP_Text customerReactionText;


    private Product currentProduct;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
    }

    void InitializeUI()
    {
        salesPanel.SetActive(false);
        priceAdjustPanel.SetActive(false);
        historyPanel.SetActive(false);
        closeButton.gameObject.SetActive(false);
        UpdateTotalMoney(GameManager.Instance.totalMoney);
    }

    void SetupButtonListeners()
    {
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
        closeButton.onClick.AddListener(CloseAllPanels);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        historyButton.onClick.AddListener(ToggleHistory);
    }

    void Update()
    {
        bool anyPanelActive = salesPanel.activeSelf ||
                            priceAdjustPanel.activeSelf ||
                            historyPanel.activeSelf;
        closeButton.gameObject.SetActive(anyPanelActive);
    }

    public void ShowSalesPanel(Product product)
    {
        currentProduct = product;
        priceText.text = $"{product.productName}\nCost: ${product.GetCostPrice():0.00}\nSale: ${product.GetSalePrice():0.00}";
        salesPanel.SetActive(true);
        SetCursorVisibility(true);
    }

    void OnYesClicked()
    {
        CompleteSale(currentProduct.GetCostPrice(), currentProduct.GetSalePrice());
        Destroy(currentProduct.gameObject);
    }

    void OnNoClicked()
    {
        priceAdjustPanel.SetActive(true);
        priceInput.text = currentProduct.GetSalePrice().ToString("0.00", CultureInfo.InvariantCulture);
    }

    void OnConfirmClicked()
    {
        if (float.TryParse(priceInput.text, NumberStyles.Any, CultureInfo.InvariantCulture, out float newSalePrice))
        {
            CompleteSale(currentProduct.GetCostPrice(), newSalePrice);
            Destroy(currentProduct.gameObject);
        }
        else
        {
            Debug.LogWarning("Invalid price format!");
        }
    }

    void CompleteSale(float costPrice, float salePrice)
    {
        currentProduct.SetSalePrice(salePrice);

        // Satışı gerçekleştiren müşteri olmasa da, doğrudan toplam parayı güncelle
        float payment = salePrice; // Ödeme, satış fiyatı üzerinden hesaplanıyor

        // Toplam parayı güncelle
        GameManager.Instance.AddMoney(payment);

        Debug.Log($"[SalesUI] Ürün başarıyla satıldı: {currentProduct.productName}, Ödeme: ${payment:0.00} 💰");
        // Satış geçmişi kaydetme
        GameManager.Instance.AddTransactionToHistory(currentProduct.productName, costPrice, salePrice);

        Debug.Log($"[SalesUI] Ürün başarıyla satıldı: {currentProduct.productName}, Ödeme: ${payment:0.00} 💰");


        // Uygun müşteri bulunamadıysa bile, ürünün satış işlemi tamamlanmış sayılır
        if (FindMatchingCustomer(currentProduct) == null)
        {
            Debug.LogWarning("[SalesUI] Uygun müşteri bulunamadı, ancak ürün satıldı.");
        }

        // Satış işlemi tamamlandı
        CloseAllPanels();
    }

    Customer FindMatchingCustomer(Product product)
    {
        Debug.Log($"[SalesUI] Aranan ürün tipi: {product.GetProductType()}");
        Customer[] customers = FindObjectsOfType<Customer>();
        foreach (var c in customers)
        {
            // Sadece istenen ürün tipi ve daha önce servis edilmemiş müşteri aranacak
            if (!c.IsServed && c.desiredProductType == product.GetProductType())
            {
                return c;
            }
        }
        return null;
    }





    void ToggleHistory()
    {
        bool shouldOpenHistory = !historyPanel.activeSelf;
        historyPanel.SetActive(shouldOpenHistory);
        salesPanel.SetActive(!shouldOpenHistory);

        if (shouldOpenHistory)
        {
            RefreshHistoryUI();
        }
    }

    public void RefreshHistoryUI()
    {
        foreach (Transform child in historyContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var record in GameManager.Instance.transactionHistory)
        {
            CreateHistoryItem(record);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(historyContent as RectTransform);
    }

    void CreateHistoryItem(GameManager.TransactionRecord record)
    {
        if (!historyItemPrefab || !historyContent) return;

        GameObject item = Instantiate(historyItemPrefab, historyContent);
        if (item.TryGetComponent(out HistoryItem historyItem))
        {
            historyItem.Setup(
                record.productName,
                record.costPrice,
                record.salePrice
            );
        }
    }

    void CloseAllPanels()
    {
        salesPanel.SetActive(false);
        priceAdjustPanel.SetActive(false);
        historyPanel.SetActive(false);
        SetCursorVisibility(false);
        closeButton.gameObject.SetActive(false);
    }

    void SetCursorVisibility(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void UpdateTotalMoney(float amount)
    {
        if (totalMoneyText)
        {
            totalMoneyText.text = $"Total: ${amount:0.00}";
        }
    }

    public void ShowCustomerReaction(bool isHappy, Customer.CustomerType customerType)
    {
        string message = "";
        Color color = Color.white;

        if (isHappy)
        {
            message = customerType switch
            {
                Customer.CustomerType.VIP => "Mükemmel seçim!",
                Customer.CustomerType.Bargainer => "Fena değil...",
                Customer.CustomerType.Impulsive => "Tam istediğim gibi!",
                _ => "Teşekkürler!"
            };
            color = Color.green;
        }
        else
        {
            message = customerType switch
            {
                Customer.CustomerType.VIP => "Bu benim istediğim değil!",
                Customer.CustomerType.Bargainer => "Bunu daha ucuza almalıyım...",
                Customer.CustomerType.Impulsive => "Zamanımı boşa harcadın!",
                _ => "Yanlış ürün..."
            };
            color = Color.red;
        }

        customerReactionText.text = message;
        customerReactionText.color = color;
        StartCoroutine(ShowReactionCoroutine());
    }
    private IEnumerator ShowReactionCoroutine()
    {
        customerReactionText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        customerReactionText.gameObject.SetActive(false);
    }

}