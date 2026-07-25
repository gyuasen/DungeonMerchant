using System;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int currentDay = 1;

    public int CurrentDay => currentDay;

    public event Action<int> DayChanged;
    public event Action<int> DayChangeFinalized;

    public void AdvanceDay()
    {
        AdvanceDays(1);
    }

    public void AdvanceDays(int amount)
    {
        int daysToAdvance = Mathf.Max(0, amount);
        for (int day = 0; day < daysToAdvance; day++)
        {
            currentDay++;
            Debug.Log($"Day advanced: {currentDay}");
            DayChanged?.Invoke(currentDay);
            DayChangeFinalized?.Invoke(currentDay);
        }
    }

    public void SetCurrentDay(int value)
    {
        currentDay = Mathf.Max(1, value);
        DayChanged?.Invoke(currentDay);
        DayChangeFinalized?.Invoke(currentDay);
    }
}
