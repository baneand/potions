using UnityEngine;
using System.Collections;

/* This class functions as an add-on to subscribers, counting rewards over a set period
*/
public class RewardCounter
{
    public int MaxRewardCount { get; set; }

    private int currentRewardCount = 0;

    public void GetReward()
    {
        currentRewardCount++;
    }

    public bool IsRewardedEnough()
    {
        return MaxRewardCount <= currentRewardCount;
    }

    public void ResetCounter()
    {
        currentRewardCount = 0;
    }
}
