using UnityEngine;

public class PickUp : MonoBehaviour
{
    bool canPick = false;
    bool isHolding = false;
    [SerializeField] GameObject hand; // el ortası konum
    [SerializeField] GameObject handsObject;
    [SerializeField] GameObject targetPosition; // Objenin bırakılacağı hedef konum
    GameObject heldObject = null;
    bool isInDropArea = false;

    private Vector3 initialPosition; // Objenin başlangıç pozisyonu
    private Quaternion initialRotation; // Objenin başlangıç rotasyonu

    [SerializeField] PickUpUI pickUpUI; // UI Script referansı
    [SerializeField] SalesUI salesUI; // SalesUI referansı

    void Start()
    {
        handsObject.SetActive(false);
    }

    void Update()
    {
        // E tuşu ile objeyi al
        if (Input.GetKeyDown(KeyCode.E) && canPick && !isHolding)
        {
            PickObject();
        }

        // F tuşu ile objeyi başlangıç yerine bırak
        if (Input.GetKeyDown(KeyCode.F) && isHolding)
        {
            ReturnToInitialPosition();
        }

        // G tuşu ile objeyi bırak
        if (Input.GetKeyDown(KeyCode.G) && isHolding && isInDropArea)
        {
            DropObject();
        }
    }

    void PickObject()
    {
        // Raycast ile objeyi seç
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        float maxDistance = 5f; // Maksimum mesafe (örneğin 5 birim)

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (hit.collider.CompareTag("Pickable")) // Sadece "Pickable" tag'ine sahip objeleri al
            {
                heldObject = hit.collider.gameObject;

                // Objenin başlangıç pozisyonunu ve rotasyonunu kaydet
                initialPosition = heldObject.transform.position;
                initialRotation = heldObject.transform.rotation;

                heldObject.transform.parent = hand.transform; // Objeyi Hand'e bağla
                heldObject.transform.localPosition = Vector3.zero; // Hand'in merkezine yerleştir
                heldObject.transform.localEulerAngles = Vector3.zero; // Rotasyonu sıfırla
                heldObject.GetComponent<Rigidbody>().isKinematic = true; // Fizik etkileşimini kapat
                isHolding = true; // Objeyi tutuyoruz

                // Eller objesini görünür yap
                handsObject.SetActive(true);

                // UI mesajını güncelle
                pickUpUI.ShowBirakMessage(); // "F: Başlangıç Yerine Bırak" mesajını göster
            }
        }
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            // Objeyi satış alanına bırak
            heldObject.transform.parent = null;
            heldObject.transform.position = targetPosition.transform.position;
            heldObject.transform.rotation = targetPosition.transform.rotation;
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            isHolding = false;

            // Satış panelini aç
            Product product = heldObject.GetComponent<Product>();
            if (product != null)
            {
                salesUI.ShowSalesPanel(product); // Satış panelini göster
            }

            heldObject = null;

            // Eller objesini gizle
            handsObject.SetActive(false);

            // UI mesajını gizle
            pickUpUI.HideAllMessages();
        }
    }

    void ReturnToInitialPosition()
    {
        if (heldObject != null)
        {
            // Objeyi başlangıç pozisyonuna ve rotasyonuna geri döndür
            heldObject.transform.parent = null;
            heldObject.transform.position = initialPosition; // Başlangıç pozisyonu
            heldObject.transform.rotation = initialRotation; // Başlangıç rotasyonu
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            isHolding = false;
            heldObject = null;

            // Eller objesini gizle
            handsObject.SetActive(false);

            // UI mesajını gizle
            pickUpUI.HideAllMessages();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickable")) // Objeye yaklaştığımızda
        {
            canPick = true;

            if (!isHolding) // Elinde obje yoksa "E: Pick Up" mesajını göster
            {
                pickUpUI.ShowPickUpMessage(); // "E: Pick Up" mesajını göster
            }
        }

        if (other.gameObject.name == "DropArea") // Bırakma alanına girdiğimizde
        {
            isInDropArea = true;
            Debug.Log("Bırakma alanına girdiniz.");

            if (isHolding)
            {
                pickUpUI.ShowDropAreaMessage(); // "G: Drop" mesajını göster
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pickable")) // Objeden uzaklaştığımızda
        {
            canPick = false;
            pickUpUI.HideAllMessages(); // UI mesajını gizle
        }

        if (other.gameObject.name == "DropArea") // Bırakma alanından çıktığımızda
        {
            isInDropArea = false;
            Debug.Log("Bırakma alanından çıktınız.");

            pickUpUI.HideAllMessages(); // UI mesajını gizle
        }
    }
}