using System;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int currentDay = 1;

    public int CurrentDay => currentDay;

    public event Action<int> DayChanged;

    public void AdvanceDay()
    {
        currentDay++;
        Debug.Log($"Day advanced: {currentDay}");
        DayChanged?.Invoke(currentDay);
    }

    public void SetCurrentDay(int value)
    {
        currentDay = Mathf.Max(1, value);
        DayChanged?.Invoke(currentDay);
    }
}
