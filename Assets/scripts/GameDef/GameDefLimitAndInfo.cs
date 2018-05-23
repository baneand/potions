using UnityEngine;

public class GameDefLimitAndInfo : MonoBehaviour, ILimitSection, IIdentSection
{
    [SerializeField] private string m_IdentTitle;
    [SerializeField] private string m_IdentName;
    [SerializeField] private string m_IdentCode;
    [SerializeField] private LimitInfo[] m_LimitInfos;

    public LimitInfo[] GetLimitInfo()
    {
        return m_LimitInfos;
    }

    public string Title
    {
        get { return m_IdentTitle; }
    }

    public string Name
    {
        get { return m_IdentName; }
    }

    public string Code
    {
        get { return m_IdentCode; }
    }

#if UNITY_EDITOR
    private void Reset()
    {
        m_LimitInfos = LimitInfo.GetDefaultLimitSetup();
    }
#endif
}
