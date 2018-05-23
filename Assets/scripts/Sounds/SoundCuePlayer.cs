using UnityEngine;
using UnityEngine.Serialization;

/* This class responds to a reward event by playing a sound cue. Can be added to any set of reward events.
*/

public class SoundCuePlayer : RewardSubscriber
{
    //The sound to play upon receiving a reward.
    [SerializeField]
    [FormerlySerializedAs("rewardCue")]
    private AudioSource m_RewardCue;

    //Are any sounds stopped when this sound plays?
    [SerializeField]
    [FormerlySerializedAs("soundsToStop")]
    private AudioSource[] m_SoundsToStop;

    public override void ResetSubscriber()
    {
        base.ResetSubscriber();
        m_RewardCue.Stop();
    }

    protected override void ActivateReward()
    {
        //Make sure you assign the cue to a proper source!
        //Also, don't play when paused
        if (m_RewardCue == null || Time.timeScale <= 0.0f || m_RewardCue.isPlaying)
            return;

        foreach(AudioSource audioCue in m_SoundsToStop)
        {
            audioCue.Stop();
        }
        m_RewardCue.Play();
    }
}
