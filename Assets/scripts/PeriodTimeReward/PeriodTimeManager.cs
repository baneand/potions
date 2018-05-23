using UnityEngine;
using System.Collections.Generic;

// Subscribe/Unsubscribe to rewards from Eeger
// Count the total number of rewards/time period, reset the counter etc.
[System.Serializable]
public class Period : IRewardSubscriber
{
    public Period(int position)
    {
        Position = position;
    }

    public int RewardCount { get; private set; }
    public float TimeInSeconds { get; private set; }
    public readonly int Position;

    void IRewardSubscriber.ActivateReward()
    {
        RewardCount ++;
    }

    void IRewardSubscriber.SetIsSubscribed(bool subscribed)
    {

    }

    void IRewardSubscriber.ResetSubscriber() { }

    public void AddTime(float deltaTime)
    {
        TimeInSeconds += deltaTime;
    }
}

public class PeriodTimeManager : Singleton<PeriodTimeManager>
{
    public int TotalRewards
    {
        get
        {
            int total = 0;
            foreach (var period in m_AllPeriods)
            {
                if (period != null)
                {
                    total += period.RewardCount;
                }
            }
            return total;
        }
    }

    public float TotalTime
    {
        get
        {
            float total = 0;
            foreach (var period in m_AllPeriods)
            {
                if (period != null)
                {
                    total += period.TimeInSeconds;
                }
            }
            return total;
        }
    }

    public Period CurrentPeriod { get; private set; }

    private readonly List<Period> m_AllPeriods = new List<Period>();

    void Start()
    {
        EegerCommand.Instance.Register(CommandReceiver.PAUSE, args => StopCurrentPeriod());
        EegerCommand.Instance.Register(CommandReceiver.RUN, args => StartNewPeriod());
    }

    /// <summary>
    /// Returns the last maxDesiredPeriods periods, will include the current running period
    /// </summary>
    /// <param name="maxDesiredPeriods">The maximum number of periods to recieve</param>
    /// <returns>An array of the periods with sorted newest -> oldest</returns>
    public Period[] GetLastPeriods(int maxDesiredPeriods)
    {
        int maxValue = Mathf.Min(m_AllPeriods.Count, maxDesiredPeriods);
        var lastPeriods = new Period[maxValue];
        for (int i = 0; i < maxValue; i++)
        {
            lastPeriods[i] = m_AllPeriods[m_AllPeriods.Count - i - 1];
        }
        return lastPeriods;
    }

    public void StartNewPeriod()
    {
        if (CurrentPeriod != null)
        {
            //take away the current reward tracker
            EegerCommand.Instance.RemoveSubscriber(CurrentPeriod);
        }
        CurrentPeriod = new Period(m_AllPeriods.Count);
        //add the next reward tracker
        EegerCommand.Instance.AddSubscriber(CurrentPeriod);
        m_AllPeriods.Add(CurrentPeriod);
    }

    public void StopCurrentPeriod()
    {
        CurrentPeriod = null;
    }

    private void Update()
    {
        if (CurrentPeriod != null)
        {
            CurrentPeriod.AddTime(Time.deltaTime);
        }
    }

}
