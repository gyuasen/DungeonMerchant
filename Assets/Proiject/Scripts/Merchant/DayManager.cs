using System;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int currentDay = 1;

    public int CurrentDay => currentDay;

    public event Action<int> DayChanged;
    public event Action<int> DayChangeFinalized;
    // 1回の AdvanceDays でまとめて進めた全日の処理が終わった後に発火する。
    // 引数は進めた日数。複数日を1画面のリザルトへ連結するために使う。
    public event Action<int> DaysAdvanceCompleted;

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

        if (daysToAdvance > 0)
        {
            DaysAdvanceCompleted?.Invoke(daysToAdvance);
        }
    }

    public void SetCurrentDay(int value)
    {
        currentDay = Mathf.Max(1, value);
        DayChanged?.Invoke(currentDay);
        DayChangeFinalized?.Invoke(currentDay);
    }
}
