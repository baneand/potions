using UnityEngine;
using UnityEngine.UI;

public class GenericStrandIndicator : MonoBehaviour, IStrandIndicator
{
    [SerializeField] private StrandType m_ChannelType;

    public StrandType IndicatorStrandType
    {
        get { return m_ChannelType; }
    }

    [SerializeField] private Image m_FillImage;
    [SerializeField] private Color m_FillColor;

    private void Awake()
    {
        if (m_FillImage != null)
        {
            m_FillImage.color = m_FillColor;
        }
    }

    public void UpdateValue(Float01 newValue)
    {
        if (m_FillImage != null)
        {
            m_FillImage.fillAmount = newValue.GetValue();
        }
    }

    public void SetFillColor(Color fillColor)
    {
        m_FillColor = fillColor;
        if (m_FillImage != null)
        {
            m_FillImage.color = fillColor;
        }
    }
}
