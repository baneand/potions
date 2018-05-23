using UnityEngine;

public interface IRewardSubscriber
{
    void ActivateReward();
    void SetIsSubscribed(bool subscribed);
    void ResetSubscriber();
}

/* This object is subscribed to a specific set of reward events, set to trigger when a reward is triggered.
*/
public abstract class RewardSubscriber : MonoBehaviour, IRewardSubscriber
{
    //Is the subscriber currently subscribed to an event object?
    public bool Subscribed { get; private set; }

    void IRewardSubscriber.SetIsSubscribed(bool isSubscribed)
    {
        Subscribed = isSubscribed;
    }

    protected void Subscribe()
    {
        EegerCommand.Instance.AddSubscriber(this);
    }

    protected void UnSubscribe()
    {
        EegerCommand.Instance.RemoveSubscriber(this);
    }

    //Code used to react to a reward event
    void IRewardSubscriber.ActivateReward ()
    {
	    ActivateReward();
	}

    protected abstract void ActivateReward();
    //This resets the behavior when unsubscribing it from the controller so it does not contribute to the game when unsubscribed
    public virtual void ResetSubscriber()
    {

    }

    protected virtual void Start()
    {
        Subscribe();
    }
}
