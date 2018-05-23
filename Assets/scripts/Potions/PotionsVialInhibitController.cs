using JetBrains.Annotations;
using UnityEngine;

public class PotionsVialInhibitController : PotionsVialController
{
    [SerializeField] private float m_SpeedLerpMultiplier = 2f;
    private float m_PriorValue;
    private float m_DesiredValue;

    public override void UpdateValue([NotNull] Float01 newValue)
    {
        m_DesiredValue = newValue.GetValue();
    }

    protected override void Update()
    {
        m_PriorValue = Mathf.Lerp(m_PriorValue, m_DesiredValue, Time.deltaTime * m_SpeedLerpMultiplier);
        if (Mathf.Abs(m_PriorValue - m_DesiredValue) > .005f)
        {
            SetPercentage(m_PriorValue);
        }
        base.Update();
    }

    public override StrandType IndicatorStrandType
    {
        get { return StrandType.Inhibit; }
    }
}
