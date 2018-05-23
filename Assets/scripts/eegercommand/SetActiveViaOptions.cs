using System;
using UnityEngine;

public class SetActiveViaOptions : MonoBehaviour, IOptionSection
{
    [SerializeField] private string m_OptionName;
    [SerializeField] private string m_DesiredValue;
    [Header("Game def file variables")]
    [SerializeField] private int m_GameDefOrder;
    [SerializeField] private bool m_GameDefDefaultValue;
    [SerializeField] private string m_GameDefHelpString;

	private void Start ()
    {
	    OptionsManager.Instance.RegisterOptionChanged(m_OptionName, OnOptionSet);
	}

    private void OnOptionSet(string s)
    {
        gameObject.SetActive(string.Equals(s, m_DesiredValue, StringComparison.OrdinalIgnoreCase));
    }

    #region Gamedef Option

    string IOptionSection.GamedefName
    {
        get { return m_OptionName; }
    }

    string IOptionSection.OptionLayout
    {
        get { return "B,,," + m_GameDefOrder + "," + (m_GameDefDefaultValue ? "1" : "0"); }
    }

    string IOptionSection.HelpString
    {
        get { return m_GameDefHelpString; }
    }

    #endregion
}
