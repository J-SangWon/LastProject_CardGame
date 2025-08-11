using UnityEngine;
using TMPro;

public class CostManager : MonoBehaviour
{
    public static CostManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("UI - Cost")]
    public TextMeshProUGUI playerCostText;
    public TextMeshProUGUI enemyCostText;

    [Header("Cost Settings")]
    public int maxCostLimit = 10;

    private int playerCurrentCost = 0;
    private int playerMaxCost = 0;
    private int enemyCurrentCost = 0;
    private int enemyMaxCost = 0;


    public void StartPlayerTurn()
    {
        if (playerMaxCost < maxCostLimit)
            playerMaxCost++;

        playerCurrentCost = playerMaxCost;
        UpdateCostUI();
    }

   
    public void StartEnemyTurn()
    {
        if (enemyMaxCost < maxCostLimit)
            enemyMaxCost++;

        enemyCurrentCost = enemyMaxCost;
        UpdateCostUI();
    }

    public bool SpendPlayerCost(int amount)
    {
        if (playerCurrentCost >= amount)
        {
            playerCurrentCost -= amount;
            UpdateCostUI();
            return true;
        }
        return false;
    }

    public bool SpendEnemyCost(int amount)
    {
        if (enemyCurrentCost >= amount)
        {
            enemyCurrentCost -= amount;
            UpdateCostUI();
            return true;
        }
        return false;
    }
    public void UpdateCostUI()
    {
        if (playerCostText != null)
            playerCostText.text = $"Player Cost: {playerCurrentCost}/{playerMaxCost}";

        if (enemyCostText != null)
            enemyCostText.text = $"Enemy Cost: {enemyCurrentCost}/{enemyMaxCost}";
    }
}
