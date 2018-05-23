using System.Collections.Generic;
using UnityEngine;

public class ColorStrandManager : Singleton<ColorStrandManager>
{
    public Dictionary<int, Color> m_FrequencyColorSet = new Dictionary<int, Color>()
    {
        {0,  Utils.HexToRGB("ff00ff") },            // Purple 
        {1,  Utils.HexToRGB("f602fd") },            
        {2,  Utils.HexToRGB("ed05fc") },
        {3,  Utils.HexToRGB("e508fa") },
        {4,  Utils.HexToRGB("dc0bf9") },
        {5,  Utils.HexToRGB("d30ef8") },
        {6,  Utils.HexToRGB("cb11f6") },
        {7,  Utils.HexToRGB("c214f5") },
        {8,  Utils.HexToRGB("b917f4") },
        {9,  Utils.HexToRGB("b11af2") },
        {10, Utils.HexToRGB("a81df1") },
        {11, Utils.HexToRGB("7e15f4") },            //Purple-blue
        {12, Utils.HexToRGB("540ef8") },
        {13, Utils.HexToRGB("2a07fb") },
        {14, Utils.HexToRGB("1f45bc") },            //Dark Blue
        {15, Utils.HexToRGB("15837d") },
        {16, Utils.HexToRGB("0ac13e") },
        {17, Utils.HexToRGB("47d02f") },
        {18, Utils.HexToRGB("84e01f") },
        {19, Utils.HexToRGB("c1ef0f") },
        {20, Utils.HexToRGB("c3ec0f") },
        {21, Utils.HexToRGB("c5e80e") },            //Yellow-green
        {22, Utils.HexToRGB("c7e50e") },
        {23, Utils.HexToRGB("c9e10d") },
        {24, Utils.HexToRGB("cbde0d") },
        {25, Utils.HexToRGB("cdda0c") },
        {26, Utils.HexToRGB("cfd70c") },
        {27, Utils.HexToRGB("d1d30b") },
        {28, Utils.HexToRGB("d3d00b") },
        {29, Utils.HexToRGB("d4cc0a") },
        {30, Utils.HexToRGB("d6c90a") },
        {31, Utils.HexToRGB("d8c609") },
        {32, Utils.HexToRGB("dac209") },
        {33, Utils.HexToRGB("dcbf08") },
        {34, Utils.HexToRGB("debb08") },
        {35, Utils.HexToRGB("e0b807") },
        {36, Utils.HexToRGB("e2b407") },
        {37, Utils.HexToRGB("e4b106") },
        {38, Utils.HexToRGB("e6ad06") },
        {39, Utils.HexToRGB("e8aa05") },
        {40, Utils.HexToRGB("e9a605") },
        {41, Utils.HexToRGB("eba304") },            //Orange
        {42, Utils.HexToRGB("ed9f04") },
        {43, Utils.HexToRGB("ef9c03") },
        {44, Utils.HexToRGB("f19903") },
        {45, Utils.HexToRGB("f39502") },
        {46, Utils.HexToRGB("f59202") },
        {47, Utils.HexToRGB("f78e01") },
        {48, Utils.HexToRGB("f98b01") },
        {49, Utils.HexToRGB("fb8700") },
        {50, Utils.HexToRGB("fd8400") },            //Orange-red
    };

    public Color GetColorForFrequency(int frequency)
    {
        Color value;
        if (!m_FrequencyColorSet.TryGetValue(frequency, out value))
        {
            value = new Color(1f, 1f, 1f, 1f);
        }
        return value;
    }
}
