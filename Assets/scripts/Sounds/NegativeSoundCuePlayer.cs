using UnityEngine;
using System.Collections.Generic;

/* The NegativeSoundCuePlayer is much like a SoundCuePlayer, except that it plays a negative sound if not enough rewards are achieved.
*/

public class NegativeSoundCuePlayer : ShrinkingRewardSubscriber {

    //Counts rewards to possibly prevent the negative sound
    public RewardCounter rewardCounter = new RewardCounter();

    //The sound to play upon not receiving a reward quickly enough
    public AudioSource negativeCue;

    //How many rewards are necessary
    public int minRewardsBeforeNegativeSound;

    //Are any sounds stopped when this sound plays?
    public List<AudioSource> soundsToStop = new List<AudioSource>();

    protected override void Start()
    {
        base.Start();
        rewardCounter.MaxRewardCount = minRewardsBeforeNegativeSound;
    }

    protected override void ActivateReward()
    {
        base.ActivateReward();

        rewardCounter.GetReward();
    }

    public override void ResetSubscriber()
    {
        base.ResetSubscriber();
        negativeCue.Stop();
    }

    public override void NegateReward()
    {
        base.NegateReward();

        if (!Subscribed)
            return;

        if (!rewardCounter.IsRewardedEnough())
        {
            if (!negativeCue.isPlaying)
            {
                foreach (AudioSource audioCue in soundsToStop)
                {
                    audioCue.Stop();
                }
                negativeCue.Play();
            }                
        }            

        rewardCounter.ResetCounter();
    }
}
