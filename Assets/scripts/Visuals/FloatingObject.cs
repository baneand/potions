using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class FloatingObject : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("rBody")] private Rigidbody m_Rigidbody;
    [SerializeField]
    private float m_MaxVelocity = .5f;
    [SerializeField]
    [FormerlySerializedAs("meshRender")] private MeshRenderer m_MeshRender;

    [SerializeField]
    [FormerlySerializedAs("glowLight")] private Light m_GlowLight;

    //The original color of the floating ball, along with a color currently applied to the object
    private Color m_EmissiveColor;
    //And a color for fading out
    [SerializeField]
    [FormerlySerializedAs("fadedColor")]
    private Color m_FadedColor;
    private Color m_CurrentColor;

    public Color CurrentColor
    {
        get { return m_CurrentColor; }
        set
        {
            m_CurrentColor = value;
            m_MeshRender.material.SetColor("_EmissionColor", m_CurrentColor);
        }
    }

    //Fade out the glowing color over time
    [SerializeField]
    [FormerlySerializedAs("fadeSpeed")]
    private float m_ColorFadeSpeed;

    //How high up does the object float before being stopped?
    [Header("World position for max height")]
    [FormerlySerializedAs("stopY")]
    [SerializeField]
    private float m_MaxHeight;

    [SerializeField]
    private ParticleSystem[] m_ParticleSystems;

    public bool IsSwaying
    {
        get { return m_SwayCoroutine != null && m_SwayCoroutine.IsActive; }
    }

    //Is the ball currently floating up?
    public bool IsFloatingUp
    {
        get { return m_IsFloatingUp; }
        set
        {
            if (m_IsFloatingUp == value)
            {
                return;
            }
            m_IsFloatingUp = value;
            if (m_ParticleSystems != null)
            {
                foreach (var emmitter in m_ParticleSystems)
                {
                    if (emmitter == null) continue;
                    if (value)
                    {
                        emmitter.Play();
                    }
                    else
                    {
                        emmitter.Stop();
                    }
                }
            }
            if (m_GlowLight != null)
            {
                //And turn on the glowing light
                m_GlowLight.enabled = value;
            }
        }
    }

    //While floating, how much time to sink for a swaying motion
    [SerializeField]
    [FormerlySerializedAs("timeToSink")]
    private float m_SwayTime;
    private bool m_IsFloatingUp;
    private Coroutine<bool> m_SwayCoroutine;
    //Allows for a smoother change in velocity when switching
    private const float SwitchVelocityReductionRate = 2f;

    private void Awake()
    {
        //Get the emissive color from the glow material and fade it out 
        m_EmissiveColor = m_MeshRender.sharedMaterial.GetColor("_EmissionColor");
    }

    void Start()
    {
        EegerCommand.Instance.Register(CommandReceiver.PAUSE, HandlePause);
        EegerCommand.Instance.Register(CommandReceiver.RUN, HandleRun);
    }

    private void HandlePause(object[] args)
    {
        Sway();
    }

    private void HandleRun(object[] args)
    {
        CancelSway();
    }

	public void Float()
    {
        //Don't float if it's already sinking
        if (IsFloatingUp)
            return;

        IsFloatingUp = true;

        //Also apply the glow material
        CurrentColor = m_EmissiveColor;
    }

    public void Defloat()
    {
        //Don't defloat if it's already sinking
        if (!IsFloatingUp)
            return;

        IsFloatingUp = false;
        CancelSway();
    }

    private void CancelSway()
    {
        if (m_SwayCoroutine != null && m_SwayCoroutine.IsActive)
        {
            m_SwayCoroutine.Cancel();
        }
    }

    //Start alternating switch forces for a swaying motion
    private void Sway()
    {
        Debug.Log("Starting Sway");
        m_SwayCoroutine = this.StartCoroutine<bool>(CoSway());
    }

    //Allows an up/down swaying motion so the ball doesn't always stay at a certain point when floating
    protected IEnumerator CoSway()
    {
        bool swayValue = false;
        while (true)
        {
            yield return swayValue;
            yield return new WaitForSeconds(m_SwayTime);
            swayValue = !swayValue;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    public void Update()
    {
        //Stop it if it goes high enough
        if (transform.position.y > m_MaxHeight && IsFloatingUp && !IsSwaying)
        {
            Sway();
        }

        bool desiredDirection = IsSwaying ? m_SwayCoroutine.Value : IsFloatingUp;
        float desiredVelocity = m_MaxVelocity*(desiredDirection ? 1f : -1f); 
        if (!Mathf.Approximately(desiredVelocity, m_Rigidbody.velocity.y))
        {
            m_Rigidbody.velocity = new Vector3(0,
                Mathf.Lerp(m_Rigidbody.velocity.y, desiredVelocity, Time.deltaTime*SwitchVelocityReductionRate), 0);
        }
        //If gravity is enabled, fade out the color over time
        if(!IsFloatingUp)
        {
            CurrentColor = Color.Lerp(CurrentColor, m_FadedColor, Time.deltaTime * m_ColorFadeSpeed);
        }
    }

    public void SetColors(Color ballonColor, Color fadedColor)
    {
        m_EmissiveColor = ballonColor;
        m_FadedColor = fadedColor;
        CurrentColor = IsFloatingUp ? ballonColor : fadedColor;
    }
}
