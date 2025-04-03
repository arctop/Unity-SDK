using System;
using UnityEngine;
using UnityEngine.Events;

namespace com.arctop.qa
{
    public class GameQAReceiver : MonoBehaviour
    {
        [SerializeField] private UnityEvent m_onQaColorsReceived;
        [SerializeField] private UnityEvent<string> m_onHSILineReceived;

        public static Color[] QAColors { get; private set; } = null;

        private void CreateHsiValueArray(string colorsValues)
        {
            var colors = colorsValues.Split(',');
            QAColors = new Color[colors.Length];
            for (int i = 0; i < QAColors.Length; i++)
            {
                if (!ColorUtility.TryParseHtmlString(colors[i], out QAColors[i]))
                {
                    Debug.LogError($"Couldn't convert color {colors[i]}");
                }
            }
            m_onQaColorsReceived?.Invoke();
        }
        public void SetHsiValue(string hsiLine)
        {
            m_onHSILineReceived?.Invoke(hsiLine);
        }

        private void Start()
        {
            //TODO: Replace with new QA
            CreateHsiValueArray(
                "#6fe100,#71e400,#73e800,#75eb00,#77ee00,#79f100,#7bf500,#7df803,#7ffb07,#84fe0b,#88ff0f,#8dff13,#91ff17,#96ff1b,#9aff1f,#9fff23,#a4ff27,#a8ff2b,#adff2f,#b1ff33,#b6ff37,#baff3b,#bfff37,#c3ff33,#c8ff2f,#ccff2b,#cfff29,#d1ff27,#d3ff25,#d6ff23,#d8ff21,#daff1f,#dcff1d,#dfff1b,#e1ff19,#e3ff17,#e6ff15,#e8ff13,#eaff11,#ecff0f,#efff0d,#f1ff0b,#f3fd09,#f5fc07,#f8fb05,#fafa03,#fcf801,#fff700,#fff600,#fff500,#fff300,#fff200,#fff100,#fff000,#ffee00,#ffed00,#ffec00,#ffeb00,#ffea00,#ffe800,#ffe700,#ffe600,#ffe500,#ffe300,#ffe200,#ffe100,#ffe000,#ffde00,#ffdd00,#ffdc00,#ffdb00,#ffda00,#ffd800,#ffd701,#ffd502,#ffd203,#ffd004,#ffcd05,#ffcb06,#ffc807,#ffc608,#ffc309,#ffc10a,#ffbe0b,#ffbc0c,#ffb90d,#ffb10d,#ffa90c,#ffa10b,#ff990a,#ff9109,#ff8808,#ff8007,#ff7806,#ff7005,#ff6804,#ff5f03,#ff5702,#ff4f01,#ff4700,#ff4200,#ff3d00,#ff3900,#ff3400,#ff2f00,#ff2a00,#ff2600,#ff2100,#ff1c00,#ff1700,#ff1300,#ff0e00,#ff0900,#ff0411");
        }
    }
}