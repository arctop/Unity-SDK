using System;
using System.Linq;
using UnityEngine;
namespace com.arctop.qa
{
    public class QASensorArray : MonoBehaviour
    {
        private static readonly int sr_objectOffset = Shader.PropertyToID("_objectOffset");
        private static readonly int sr_shaderColorsArray = Shader.PropertyToID("CurrentColorsArray");
        private static readonly int sr_ringNumber = Shader.PropertyToID("_ringNumber");
        private static readonly int sr_dotNumber = Shader.PropertyToID("_dotNumber");
        

        [SerializeField]
        private Material m_qaMaterial;
        [SerializeField]
        private Renderer[] m_qaDotsRenderers;
        [SerializeField, Range(0, 113)]
        private int m_initialColorIndex = 113;
        private Color m_initialColor;

        private Color[] m_currentColors;
        private Color[] m_newColors;

        public void Init()
        {
            var dotNum = m_qaDotsRenderers.Length;
            if (dotNum == 0)
            {
                return;
            }

            m_initialColor = GameQAReceiver.QAColors[m_initialColorIndex];
            var rNum = m_qaMaterial.GetInt(sr_ringNumber);
            m_qaMaterial.SetInt(sr_dotNumber, dotNum);
            m_newColors = new Color[dotNum];
            m_currentColors = Enumerable.Repeat(m_initialColor, dotNum * rNum).ToArray();
            m_qaMaterial.SetColorArray(sr_shaderColorsArray, m_currentColors);
            for (var i = 0; i < m_qaDotsRenderers.Length; i++)
            {
                var dot = m_qaDotsRenderers[i];
                MaterialPropertyBlock props = new MaterialPropertyBlock();
                props.SetFloat(sr_objectOffset, i);
                dot.SetPropertyBlock(props);
            }
        }

        private void PushNewColorLine(Color[] value)
        {
            var length = value.Length;
            Array.Copy(m_currentColors, 0, m_currentColors, length, m_currentColors.Length - length);
            Array.Copy(value, m_currentColors, length);
            m_qaMaterial.SetColorArray(sr_shaderColorsArray, m_currentColors);
        }

        public void SetHsiValue(string hsiLine)
        {
            #if UNITY_EDITOR
            if (!gameObject.activeInHierarchy) return;
            if (m_newColors == null) return;
            #endif
            // comes in as a val,val,val,val, and not encoded as JSON
            var hsiValues = hsiLine.Split(',');
            for (int i = 0; i < hsiValues.Length && i < m_newColors.Length; i++)
            {
                if (float.TryParse(hsiValues[i], out float index))
                {
                    m_newColors[i] = GameQAReceiver.QAColors[Mathf.FloorToInt(index)];
                }
                else
                {
                    Debug.LogError($"Couldn't parse value {hsiValues[i]}");
                }
            }
            PushNewColorLine(m_newColors);
        }
    }
}