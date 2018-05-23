using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {

    int count = 0;
    public Light light;
    float time = 0;



    public Dictionary<int, Color> m_LightColorSet = new Dictionary<int, Color>()
    {
        {0,  Utils.HexToRGB("f44242") },
        {1,  Utils.HexToRGB("f24696") },
        {2,  Utils.HexToRGB("f457f9") },
        {3,  Utils.HexToRGB("9257f9") },
        {4,  Utils.HexToRGB("5769f9") },
        {5,  Utils.HexToRGB("57b2f9") },
        {6,  Utils.HexToRGB("57f3f9") },
        {7,  Utils.HexToRGB("57f9cb") },
        {8,  Utils.HexToRGB("57f992") },
        {9,  Utils.HexToRGB("2cff07") },
      
    };


    // Use this for initialization
    void Start () {

        EegerCommand.Instance.Register(CommandReceiver.REWARD, HandleReward);
		
	}


    private void HandleReward(params object[] args)
    {
        if(args == null || args.Length != 1)
        {
            return;
        }

        if (count < 9) count++;
     }
	
	// Update is called once per frame
	void Update () {

        time += Time.deltaTime;

        if(time > 5.0f )
        {
            time = 0;
            if (count > 0) count--;
    
        }

        light.color = m_LightColorSet[count];


    }

}
