using UnityEngine;

/* The ParticleRateChanger increases the amount of particles in the sky, such as increasing/decreasing cloud cover
*/
public class ParticleRateChanger : ShrinkingRewardSubscriber
{
    //This is the particle system that the script applies the rate to
    public ParticleSystem pSystem;

    //How much the particle rate changes over reward or lack of reward.
    public float rateChangeValue;

    //The maximum of how much to emit particles
    public float maxEmitValue;

    //The rate at which particles form
    public float currentEmitRate;

    //The non-subscribed and reset value of the particle emission
    private const float resetRateValue = 0.05f;

    //Used in case of cloud cover - set the value if it exists
    public Rainfall rainFall;

    protected override void Start()
    {
        base.Start();

        currentEmitRate = resetRateValue;
        UpdateParticleRateChange();
    }

    protected override void ActivateReward()
    {
        base.ActivateReward();

        currentEmitRate -= rateChangeValue;

        if (currentEmitRate < 0)
            currentEmitRate = 0;

        UpdateParticleRateChange();
    }

    public override void NegateReward()
    {
        base.NegateReward();

        currentEmitRate += rateChangeValue;

        if (currentEmitRate > maxEmitValue)
            currentEmitRate = maxEmitValue;

        UpdateParticleRateChange();
    }

    public override void ResetSubscriber()
    {
        base.ResetSubscriber();

        currentEmitRate = resetRateValue;
        UpdateParticleRateChange();
    }

    //Sets the min/max emission for a constant emission rate
    private void UpdateParticleRateChange()
    {
        var pEmission = pSystem.emission;

        //Create a new curve rate to apply to the current emission
        var emitRate = new ParticleSystem.MinMaxCurve
        {
            constantMin = currentEmitRate,
            constantMax = currentEmitRate
        };

        pEmission.rateOverTime = emitRate;
        if (rainFall)
            rainFall.SetCloudCoverValue(currentEmitRate);
    }
}
