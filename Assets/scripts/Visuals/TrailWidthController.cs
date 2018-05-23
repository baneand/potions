using UnityEngine;
using System.Collections.Generic;

/* The TrailWidthController controls the width of the TrailingObjects that it keeps track of.
*/
public class TrailWidthController : ShrinkingRewardSubscriber
{

    //The list of trailing objects from which to apply lineWidth.
    public List<TrailRenderer> TrailingObjects;

    //How much to increase or decrease line width on each reward.
    public float lineWidthChangeValue;

    public float maxLineWidth;

    public float minLineWidth;

    //How quickly to interpolate to a set line width.
    public float lineWidthInterpSpeed;

    //Current convergence value
    private float currentLineWidth = 0;

    protected override void Start()
    {
        base.Start();

        //Set line width to middle of 
        currentLineWidth = (maxLineWidth + minLineWidth) / 2.0f;
        UpdateLineWidthValues();

    }

    protected override void ActivateReward()
    {
        base.ActivateReward();

        currentLineWidth += lineWidthChangeValue;

        if (currentLineWidth > maxLineWidth)
            currentLineWidth = maxLineWidth;

    }

    public override void ResetSubscriber()
    {
        base.ResetSubscriber();

        currentLineWidth = (maxLineWidth + minLineWidth) / 2.0f;

        UpdateLineWidthValues();

    }

    private void Update()
    {
        if (!Subscribed)
            return;

        foreach (TrailRenderer trailRender in TrailingObjects)
        {
            trailRender.startWidth = Mathf.Lerp(trailRender.startWidth, currentLineWidth, Time.deltaTime * lineWidthInterpSpeed);
            trailRender.endWidth = Mathf.Lerp(trailRender.endWidth, currentLineWidth, Time.deltaTime * lineWidthInterpSpeed);
        }
    }

    public override void NegateReward()
    {
        base.NegateReward();

        if (!Subscribed)
            return;

        currentLineWidth -= lineWidthChangeValue;

        if (currentLineWidth < minLineWidth)
            currentLineWidth = minLineWidth;

    }

    //Takes the current line width and applies it to all TrailingObjects that it keeps track of
    private void UpdateLineWidthValues()
    {
        foreach (TrailRenderer trailRender in TrailingObjects)
        {
            trailRender.startWidth = currentLineWidth;
            trailRender.endWidth = currentLineWidth;
        }
    }
}
