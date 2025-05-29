using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] customerPrefabs; // Changed to array for multiple prefabs
    public Transform[] spawnPoints;
    public Transform[] waitPoints;
    public Transform checkoutPoint;
    public Transform exitPoint;
    public float minSpawnTime = 45f;
    public float maxSpawnTime = 90f;
    public int maxActiveCustomers = 5;

    [Header("Difficulty Curve")]
    public AnimationCurve spawnTimeCurve;
    public float gameTimeMultiplier = 0.1f;

    private List<GameObject> activeCustomers = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnCustomers());
    }

    IEnumerator SpawnCustomers()
    {
        while (true)
        {
            float difficulty = Mathf.Clamp01(Time.timeSinceLevelLoad * gameTimeMultiplier);
            float spawnDelay = Mathf.Lerp(minSpawnTime, maxSpawnTime, spawnTimeCurve.Evaluate(difficulty));

            yield return new WaitForSeconds(spawnDelay);

            if (ShouldSpawnCustomer())
            {
                SpawnNewCustomer();
            }
        }
    }

    bool ShouldSpawnCustomer()
    {
        return activeCustomers.Count < maxActiveCustomers; // Paraya bakmadan spawn
    }

    void SpawnNewCustomer()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject randomPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Length)]; // Random prefab selection
        GameObject newCustomer = Instantiate(randomPrefab, spawnPoint.position, spawnPoint.rotation);

        CustomerAI customerAI = newCustomer.GetComponent<CustomerAI>();
        customerAI.SetWaypoints(
            spawnPoint,
            waitPoints[Random.Range(0, waitPoints.Length)],
            checkoutPoint,
            exitPoint
        );

        activeCustomers.Add(newCustomer);
        newCustomer.GetComponent<Customer>().OnCustomerLeft += () => activeCustomers.Remove(newCustomer);
    }

    public void ClearAllCustomers()
    {
        foreach (var customer in activeCustomers.ToArray())
        {
            if (customer != null)
            {
                customer.GetComponent<Customer>().LeaveShop();
            }
        }
        activeCustomers.Clear();
    }
}