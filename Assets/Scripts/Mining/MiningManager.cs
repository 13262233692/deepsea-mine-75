using System.Collections.Generic;
using UnityEngine;

public class MiningManager : MonoBehaviour
{
    public static MiningManager Instance { get; private set; }

    [Header("Mining Stats")]
    public int totalNodulesCollected = 0;
    public int totalMineralValue = 0;
    public float miningEfficiency = 1f;

    [Header("Events")]
    public event System.Action<int> OnNodulesCollected;
    public event System.Action<int> OnMineralValueChanged;

    private List<MiningMachineController> miningMachines = new List<MiningMachineController>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterMiningMachine(MiningMachineController machine)
    {
        if (!miningMachines.Contains(machine))
        {
            miningMachines.Add(machine);
        }
    }

    public void UnregisterMiningMachine(MiningMachineController machine)
    {
        miningMachines.Remove(machine);
    }

    public void AddCollectedNodule(int value)
    {
        totalNodulesCollected++;
        totalMineralValue += Mathf.RoundToInt(value * miningEfficiency);

        OnNodulesCollected?.Invoke(totalNodulesCollected);
        OnMineralValueChanged?.Invoke(totalMineralValue);
    }

    public float GetTotalMiningIntensity()
    {
        float totalIntensity = 0f;
        foreach (var machine in miningMachines)
        {
            totalIntensity += machine.GetMiningIntensity();
        }
        return totalIntensity;
    }

    public void ResetStats()
    {
        totalNodulesCollected = 0;
        totalMineralValue = 0;

        OnNodulesCollected?.Invoke(totalNodulesCollected);
        OnMineralValueChanged?.Invoke(totalMineralValue);
    }
}
