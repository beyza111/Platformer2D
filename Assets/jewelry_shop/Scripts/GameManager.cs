using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Data")]
    public float totalMoney = 0f;
    public List<TransactionRecord> transactionHistory = new List<TransactionRecord>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadGame();
    }

    public void AddMoney(float amount)
    {
        totalMoney += amount;
        SaveGame();
        SalesUI.Instance?.UpdateTotalMoney(totalMoney);
    }



    void SaveGame()
    {
        PlayerPrefs.SetFloat("TotalMoney", totalMoney);
        // TransactionHistory'i JSON olarak kaydet
        string historyJson = JsonUtility.ToJson(new Wrapper<TransactionRecord> { Items = transactionHistory.ToArray() });
        PlayerPrefs.SetString("TransactionHistory", historyJson);
        PlayerPrefs.Save();
    }

    void LoadGame()
    {
        totalMoney = PlayerPrefs.GetFloat("TotalMoney", 0f);

        // TransactionHistory'i yükle
        string historyJson = PlayerPrefs.GetString("TransactionHistory", "");
        if (!string.IsNullOrEmpty(historyJson))
        {
            Wrapper<TransactionRecord> wrapper = JsonUtility.FromJson<Wrapper<TransactionRecord>>(historyJson);
            transactionHistory = new List<TransactionRecord>(wrapper.Items);
        }
    }

    public void AddTransactionToHistory(string productName, float costPrice, float salePrice)
    {
        TransactionRecord newRecord = new TransactionRecord(productName, costPrice, salePrice);
        transactionHistory.Add(newRecord);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

    [System.Serializable]
    public class TransactionRecord
    {
        public string productName;
        public float costPrice;
        public float salePrice;

        public TransactionRecord(string name, float cost, float sale)
        {
            productName = name;
            costPrice = cost;
            salePrice = sale;
        }
    }
}