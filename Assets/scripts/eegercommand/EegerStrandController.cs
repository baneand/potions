using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;


// Sets the color of GameObject
public interface IStrandIndicator
{
    void UpdateValue([NotNull] Float01 newValue);
    // ReSharper disable once InconsistentNaming
    GameObject gameObject { get; }
    StrandType IndicatorStrandType { get; }
    void SetFillColor(Color fillColor);
}

public abstract class EegerStrandController : MonoBehaviour
{
    private readonly Dictionary<int, IStrandIndicator> m_StrandIndicators = new Dictionary<int, IStrandIndicator>();

    protected virtual void Awake()
    {
        StrandsManager.Instance.RegisterLayoutChanged(OnLayoutChanged);
    }

    private void OnLayoutChanged(StrandsManager.GameStrand[] gameStrands)
    {
        foreach (var strandIndicator in m_StrandIndicators.Values)
        {
            if (strandIndicator != null && strandIndicator.gameObject != null)
            {
                Destroy(strandIndicator.gameObject);
            }
        }
        m_StrandIndicators.Clear();
        if (gameStrands == null)
        {
            return;
        }
     
        for (int i = 0; i < gameStrands.Length; i++)
        {
            var indicator = CreateIndicatorForType(gameStrands[i]);
            if (indicator != null)
            {
                gameStrands[i].RegisterAmplitudeCallback(indicator.UpdateValue);
                indicator.SetFillColor(gameStrands[i].GetStrandColor());
                m_StrandIndicators[i] = indicator;
            }
        }

    }

    protected abstract IStrandIndicator CreateIndicatorForType(StrandsManager.GameStrand strandType);
}
