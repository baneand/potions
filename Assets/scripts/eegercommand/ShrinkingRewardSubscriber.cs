using UnityEngine;
using System.Collections;

/* This is a special type of RewardSubscriber that imposes a negative (reversal, ideally) effect after a certain time.
*/
public class ShrinkingRewardSubscriber : ReversibleRewardSubscriber
{
    //Delay before rewards are negated
    public float timeBeforeRewardNegation;

    //Keycode for manual negation of rewards
    public KeyCode manualNegationKey;

    //How much time does the coroutine need?
    public float timeForNegationProtection;

    private Coroutine<bool> m_WaitingToNegate;

    //this will keep running and negating every tiemBeforeRewardNegation 
    protected IEnumerator WaitAndThenNegate()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBeforeRewardNegation);
            NegateReward();
        }
    }

    private void Update()
    {
        //Manually negate events for testing
        if(Input.GetKeyDown(manualNegationKey))
        {
            NegateReward();
        }
    }

    protected override void ActivateReward()
    {
        //Restart the negation protection coroutine on every reward
        if (m_WaitingToNegate != null && m_WaitingToNegate.IsActive)
        {
            m_WaitingToNegate.Cancel();
        }
        m_WaitingToNegate = this.StartCoroutine<bool>(WaitAndThenNegate());
    }

    //Negates and counteracts rewards that subscriber gives in responsee to reward events
    public virtual void NegateReward()
    {

    }
}
