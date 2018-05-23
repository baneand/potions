using System;
using UnityEngine;
using CustomLinq;

public class SoundCustomization : MonoBehaviour, IOptionSection, IDefSection
{
    [Serializable]
    private class SoundOption
    {
        private SoundOption()
        {
            Name = "None";
            Clip = null;
        }

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        public string Name;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        public AudioClip Clip;
    }

    [SerializeField]
    private SoundOption[] m_Options;
    [SerializeField]
    private AudioSource m_AudioSource;

    [Header("Game def helpers")]
    [SerializeField]
    private string m_DefName;
    [SerializeField]
    private string m_HelpString;
    [SerializeField]
    private int m_GameDefOrder;

    private void Start()
    {
        if (m_AudioSource == null)
        {
            m_AudioSource = GetComponent<AudioSource>();
        }
        OptionsManager.Instance.RegisterOptionChanged(m_DefName, OnOptionSet);
    }

    private void OnOptionSet(string s)
    {
        if (m_AudioSource == null)
        {
            return;
        }
        var selected = m_Options.First(option => option != null && string.Equals(option.Name, s));
        if (selected != null)
        {
            Debug.Log("Selected custom sound \"" + s + "\" on " + this);
            m_AudioSource.clip = selected.Clip;
        }
        else
        {
            Debug.Log("Could not find audio option \"" + s + "\" in SoundCustomization " + this);
        }
    }

    #region Gamedef Option/Def

    string IOptionSection.GamedefName
    {
        get { return m_DefName; }
    }

    string IOptionSection.OptionLayout
    {
        get
        {
            var firstOrDefault = m_Options.FirstOrDefault();
            var defaultName = firstOrDefault != null ? firstOrDefault.Name : string.Empty;
            return "T,,defs." + ((IDefSection)this).GamedefName + "," + m_GameDefOrder + "," + defaultName;
        }
    }

    string IOptionSection.HelpString { get { return m_HelpString; } }

    string IDefSection.GamedefName
    {
        get { return m_DefName + "_choices"; }
    }

    string IDefSection.Type
    {
        get { return "T,,,,"; }
    }

    string[] IDefSection.Options
    {
        get
        {
            return m_Options.Select(option => option.Name).ToArray();
        }
    }

    #endregion
}
