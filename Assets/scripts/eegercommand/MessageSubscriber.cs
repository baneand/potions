using UnityEngine;

public enum EegerMessageType { INVALID, THRESHOLD, AMPLITUDE, TOTAL };

public interface IMessageSubscriber
{
    void UpdateData(float value, EegerMessageType eegerMessageType);
}

public class MessageSubscriber : MonoBehaviour, IMessageSubscriber
{
    [SerializeField]
    private bool m_AutoRegister;

    protected float thresholdValue = 1.0f;
    protected float amplitudeValue = 1.0f;

	public virtual void UpdateData(float value, EegerMessageType valueType)
    {
        switch(valueType)
        {
            case EegerMessageType.AMPLITUDE:
                amplitudeValue = value;
                break;

            case EegerMessageType.THRESHOLD:
                thresholdValue = value;
                break;

            default:
                break;
        }
    }

    protected virtual void Start()
    {
        if(m_AutoRegister)
        {
            var command = EegerCommand.Instance;
            if(command != null)
            {
                command.AddSubscriber(this);
            }
        }
    }
	
}
