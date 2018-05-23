using UnityEngine;

[ExecuteInEditMode]
public class ParticleSystemScaler : MonoBehaviour 
{
#if UNITY_EDITOR
    // ReSharper disable once InconsistentNaming
    public float particlesScale = 1.0f;

    private float m_OldScale;

    private void Start()
	{
		m_OldScale = particlesScale;
	}

    private void Update () 
	{
		if (Mathf.Abs(m_OldScale - particlesScale) > 0.0001f && particlesScale > 0)
		{
			transform.localScale = new Vector3(particlesScale, particlesScale, particlesScale);
			float scale = particlesScale / m_OldScale;
			var ps = GetComponentsInChildren<ParticleSystem>();
			
			foreach (ParticleSystem particles in ps)
			{
                var main = particles.main;
			    main.startSizeMultiplier *= scale;
			    main.startSpeedMultiplier *= scale;
			    main.gravityModifierMultiplier *= scale;				
				var serializedObject = new UnityEditor.SerializedObject(particles);
				serializedObject.FindProperty("ClampVelocityModule.magnitude.scalar").floatValue *= scale;
				serializedObject.FindProperty("ClampVelocityModule.x.scalar").floatValue *= scale;
				serializedObject.FindProperty("ClampVelocityModule.y.scalar").floatValue *= scale;
				serializedObject.FindProperty("ClampVelocityModule.z.scalar").floatValue *= scale;
				serializedObject.FindProperty("VelocityModule.x.scalar").floatValue *= scale;
				serializedObject.FindProperty("VelocityModule.y.scalar").floatValue *= scale;
				serializedObject.FindProperty("VelocityModule.z.scalar").floatValue *= scale;
				serializedObject.FindProperty("ColorBySpeedModule.range").vector2Value *= scale;
				serializedObject.FindProperty("RotationBySpeedModule.range").vector2Value *= scale;
				serializedObject.FindProperty("ForceModule.x.scalar").floatValue *= scale;
				serializedObject.FindProperty("ForceModule.y.scalar").floatValue *= scale;
				serializedObject.FindProperty("ForceModule.z.scalar").floatValue *= scale;
				serializedObject.FindProperty("SizeBySpeedModule.range").vector2Value *= scale;
				
				serializedObject.ApplyModifiedProperties();
			}

			var trails = GetComponentsInChildren<TrailRenderer>();
			foreach (TrailRenderer trail in trails)
			{
				trail.startWidth *= scale;
				trail.endWidth *= scale;
			}
			m_OldScale = particlesScale;
		}
	}
#endif
}