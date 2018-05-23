using UnityEngine;
using System.Collections;

/* The ReversibleRewardSubscriber has the option to apply a reverse version of the reward;
    Starts from another extreme and moves the other direction.
*/
public abstract class ReversibleRewardSubscriber : RewardSubscriber {

    protected bool isReverse = false;

    public virtual void ToggleReverse()
    {
        if (!Subscribed)
            return;
        isReverse = !isReverse;
    }

    public override void ResetSubscriber()
    {
        base.ResetSubscriber();

        isReverse = false;
    }

    //Applies a reverse to the reward (like a negation)
    protected virtual void ApplyReverseReward()
    {

    }

}
