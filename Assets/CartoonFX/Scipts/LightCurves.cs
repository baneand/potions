using UnityEngine;

public class LightCurves : MonoBehaviour
{
    public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float GraphScaleX = 1, GraphScaleY = 1;

    private float m_StartTime;
    private Light m_LightSource;

    private void Start()
    {
        m_LightSource = GetComponent<Light>();
    }

    private void OnEnable()
    {
        m_StartTime = Time.time;
    }

    private void Update()
    {
        var time = Time.time - m_StartTime;
        if (time <= GraphScaleX)
        {
            var eval = LightCurve.Evaluate(time/GraphScaleX)*GraphScaleY;
            m_LightSource.intensity = eval;
        }
    }
}
