using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PickUpUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text pickUpText; // Sahneden sürükle
    [SerializeField] private TMP_Text birakText;
    [SerializeField] private TMP_Text dropText;
    [SerializeField] private Image backgroundImage;

    private void Start()
    {
        HideAllMessages();
    }

    // Sadece aktif/pasif yapma fonksiyonları
    public void ShowPickUpMessage() => ToggleUI(pickUpText.gameObject);
    public void ShowBirakMessage() => ToggleUI(birakText.gameObject);
    public void ShowDropAreaMessage() => ToggleUI(dropText.gameObject);
    public void HideAllMessages() => ToggleUI(null);

    private void ToggleUI(GameObject activeText)
    {
        pickUpText.gameObject.SetActive(activeText == pickUpText.gameObject);
        birakText.gameObject.SetActive(activeText == birakText.gameObject);
        dropText.gameObject.SetActive(activeText == dropText.gameObject);
        backgroundImage.gameObject.SetActive(activeText != null);
    }
}