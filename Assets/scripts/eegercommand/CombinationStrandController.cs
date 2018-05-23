using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Serialization;

public class CombinationStrandController : MonoBehaviour
{
    private enum CombinationChannelType
    {
        [UsedImplicitly] LowInhibit,
        HighInhibit,
        Reward
    }

    [SerializeField] private CombinationChannelType m_ChannelType;
    //TODO figure out how we want to decide this color based off of the strands it relates too
    [SerializeField] private Color m_FillColor;
    [FormerlySerializedAs("m_Indicator")] [SerializeField] private MonoBehaviour m_IndicatorMonoBehaviour;
    private List<StrandsManager.GameStrand> m_ChannelsToListen;
    private IStrandIndicator m_Indicator;

    protected void Awake()
    {
        m_Indicator = (IStrandIndicator) m_IndicatorMonoBehaviour;
        StrandsManager.Instance.RegisterLayoutChanged(OnItemsChanged);
        StrandsManager.Instance.RegisterFrequencyChanged(OnItemsChanged);
        StrandsManager.Instance.RegisterAmplitudeChanged(UpdateImage);
        if (m_Indicator != null)
        {
            m_Indicator.SetFillColor(m_FillColor);
        }
        UpdateImage();
    }

    private void OnItemsChanged(StrandsManager.GameStrand[] gameStrands)
    {
        if (gameStrands == null)
        {
            return;
        }
        m_ChannelsToListen = new List<StrandsManager.GameStrand>();
        foreach (var gameStrand in gameStrands)
        {
            if (gameStrand == null)
            {
                continue;
            }
            if (m_ChannelType == CombinationChannelType.Reward)
            {
                if (gameStrand.Strand == StrandType.Reward)
                {
                    m_ChannelsToListen.Add(gameStrand);
                }
            }
            else if (gameStrand.Strand == StrandType.Inhibit &&
                     m_ChannelType == CombinationChannelType.HighInhibit ==
                     gameStrand.IsHighInhibit)
            {
                m_ChannelsToListen.Add(gameStrand);
            }
        }
    }

    private void UpdateImage()
    {
        if (m_Indicator == null || m_ChannelsToListen == null)
        {
            return;
        }
        float maxValue = float.MinValue;
        Float01 maxFloat01 = null;
        foreach (var channelAmplitude in m_ChannelsToListen)
        {
            if (channelAmplitude.AverageValue.GetValue() > maxValue)
            {
                maxFloat01 = channelAmplitude.AverageValue;
                maxValue = channelAmplitude.AverageValue.GetValue();
            }
        }
        if (maxFloat01 != null)
        {
            m_Indicator.UpdateValue(maxFloat01);
        }
    }
}
