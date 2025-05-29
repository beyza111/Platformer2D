using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class MenuManager : MonoBehaviour
{
    [Header("TMP Butonlar")]
    public TMP_Text newGameButtonText;
    public TMP_Text loadGameButtonText;

    void Start()
    {
        
        if (newGameButtonText != null)
            newGameButtonText.text = "YENİ OYUN";

        if (loadGameButtonText != null)
            loadGameButtonText.text = "OYUNU YÜKLE";
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(1);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("TotalMoney"))
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            // TMP ile hata mesajı gösterme (opsiyonel)
            if (loadGameButtonText != null)
                loadGameButtonText.text = "KAYIT BULUNAMADI!";
        }
    }
}
