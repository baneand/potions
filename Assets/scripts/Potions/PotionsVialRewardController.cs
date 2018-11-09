using System.Collections;
using UnityEngine;

public abstract class PotionsVialController : MonoBehaviour, IStrandIndicator
{
    [SerializeField] private Transform m_FillTransform;
    [SerializeField] private float m_MaxValue;
    [SerializeField] private float m_MinValue;
    [SerializeField] private float m_RotationLerpSpeed = 30;

    protected float CurrPosition { get; private set; }

    protected virtual void Start()
    {
        SetPercentage(0);
    }

    protected void SetPercentage(float percent)
    {
        percent = Mathf.Clamp01(percent);
        if (m_FillTransform == null)
        {
            return;
        }
        CurrPosition = percent;
        var originalPosition = m_FillTransform.localPosition;
        originalPosition.y = Mathf.Lerp(m_MinValue, m_MaxValue, CurrPosition);
        m_FillTransform.localPosition = originalPosition;
    }

    protected virtual void Update()
    {
        var originalRotation = m_FillTransform.localRotation * Quaternion.Euler(0f, 0f, CurrPosition * Time.deltaTime * m_RotationLerpSpeed);
        m_FillTransform.localRotation = originalRotation;
    }

    public abstract void UpdateValue(Float01 newValue);
    public abstract StrandType IndicatorStrandType { get; }

    public void SetFillColor(Color fillColor)
    {
    }
}

public class PotionsVialRewardController : PotionsVialController
{
    [SerializeField] private float m_TimeBeforeExplosion;
    [SerializeField] private float m_LerpSpeed;
    [SerializeField] private ParticleSystem m_FireSystem;
    [SerializeField] private GameObject m_ExtraExplosionPrefab;
    [SerializeField] private GameObject m_ExplosionPrefab;
    [SerializeField] private AudioSource m_RewardAudioSource;
    [SerializeField] private int m_EventCode;
    [SerializeField] private float m_PercentBeforeRewardState = .75f;
    [SerializeField] private float m_ExtraRewardDropState = .8f;
    private float m_TimeActive;
    private bool m_IsActive;

    protected override void Start()
    {
        EegerCommand.Instance.Register(CommandReceiver.REWARD, HandleReward);
        EegerCommand.Instance.Register(CommandReceiver.EVENT, HandleEvent);
        UpdateFireSystemScale(false);
    }

    private void HandleEvent(params object[] args)
    {
        if (args == null || args.Length != 1)
        {
            return;
        }
        int code = (int) args[0];
        if (code == m_EventCode)
        {
            StartExtraReward();
        }
    }

    private void HandleReward(object[] args)
    {
        if (m_ExplosionPrefab != null)
        {
            var copy = m_ExplosionPrefab.transform.Duplicate();
            StartCoroutine(CoDelayedDestroy(copy, 1.5f));
        }
        if (m_RewardAudioSource != null)
        {
            var copy = m_RewardAudioSource.transform.Duplicate();
            StartCoroutine(CoDelayedDestroy(copy, 1.5f));
        }
    }

    protected override void Update()
    {
        base.Update();
        var currValue = Mathf.Lerp(m_CurrFireScale, m_DesiredFireScale, m_LerpSpeed*Time.deltaTime);
        if (Mathf.Abs(currValue - m_CurrFireScale) > .001f)
        {
            m_CurrFireScale = currValue;
            SetFireSystemScale(currValue);
        }
        var currTime = m_TimeActive;
        if (m_IsActive)
        {
            if (m_TimeActive >= m_TimeBeforeExplosion && m_ExtraExplosionPrefab != null)
            {
                currTime = m_TimeBeforeExplosion * m_ExtraRewardDropState;
                StartExtraReward();
            }
            currTime += Time.deltaTime;
            if (currTime/m_TimeBeforeExplosion > m_PercentBeforeRewardState)
            {
                if (!StrandsManager.Instance.IsInRewardableState)
                {
                    currTime = m_TimeActive -= Time.deltaTime;
                }
            }
        }
        else
        {
            currTime -= Time.deltaTime;
        }
        m_TimeActive = Mathf.Clamp(currTime, 0f, m_TimeBeforeExplosion);
        var maxPosition = m_TimeActive / m_TimeBeforeExplosion;
        SetPercentage(maxPosition);
    }

    IEnumerator CoDelayedDestroy(Transform copy, float timeBeforeDestroy)
    {
        yield return new WaitForSeconds(timeBeforeDestroy);
        Destroy(copy.gameObject);
    }

    [SerializeField] private float m_MinFireScale = .1f;
    [SerializeField] private float m_MaxFireScale = 1f;
    private float m_CurrFireScale = .1f;
    private float m_DesiredFireScale = .1f;

    public override void UpdateValue(Float01 value)
    {
        var newValue = value.GetValue();
        if (newValue < .5f)
        {
            m_IsActive = false;
            UpdateFireSystemScale(false);
        }
        else
        {
            m_IsActive = true;
            UpdateFireSystemScale(true);
        }
    }

    private void UpdateFireSystemScale(bool isOn)
    {
        m_DesiredFireScale = isOn ? m_MaxFireScale : m_MinFireScale;
    }

    private void SetFireSystemScale(float value)
    { 
        var lifetime = m_FireSystem.sizeOverLifetime;
        lifetime.sizeMultiplier = value;
        var main = m_FireSystem.main;
        main.startSizeMultiplier = value;
    }

    public override StrandType IndicatorStrandType
    {
        get { return StrandType.Reward; }
    }

    private void StartExtraReward()
    {
        var copy = m_ExtraExplosionPrefab.transform.Duplicate();
        copy.transform.localScale = m_ExtraExplosionPrefab.transform.localScale;
        copy.transform.position = m_ExtraExplosionPrefab.transform.position;
        StartCoroutine(CoDelayedDestroy(copy, 5f));
    }
}
