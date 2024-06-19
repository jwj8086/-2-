using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ACF.Tests
{
    [RequireComponent(typeof(Image))]
    public class HealthBarUI : MonoBehaviour
    {
        private const string STEP = "_Steps";
        private const string RATIO = "_HSRatio";
        private const string WIDTH = "_Width";
        private const string THICKNESS = "_Thickness";

        private static readonly int floatSteps = Shader.PropertyToID(STEP);
        private static readonly int floatRatio = Shader.PropertyToID(RATIO);
        private static readonly int floatWidth = Shader.PropertyToID(WIDTH);
        private static readonly int floatThickness = Shader.PropertyToID(THICKNESS);

        public float RectWidth = 1183f;
        [Range(0, 5f)] public float Thickness = 2f;
        [Range(0, 10f)] public float speed = 3f;

        public Image hp;
        public Image damaged;
        public Image separator;

        private float maxHp;

        [ContextMenu("Create Material")]
        private void CreateMaterial()
        {
            if (separator.material == null)
            {
                separator.material = new Material(Shader.Find("ABS/UI/Health Separator"));
            }
        }

        private void Start()
        {
            maxHp = PlayerInfo.Instance.Hp; // UI가 처음 생성될 때 현재 체력을 최대 체력으로 설정
        }

        private void Update()
        {
            float Hp = PlayerInfo.Instance.Hp;

            if (maxHp < Hp)
            {
                maxHp = Hp;
            }

            float step;
            float hpRatio = Hp/maxHp;

            step = maxHp/ hpRatio;
            hp.fillAmount = hpRatio;

            damaged.fillAmount = Mathf.Lerp(damaged.fillAmount, hp.fillAmount, Time.deltaTime * speed);

            separator.material.SetFloat(floatSteps, step);
            separator.material.SetFloat(floatRatio, hpRatio);
            separator.material.SetFloat(floatWidth, RectWidth);
            separator.material.SetFloat(floatThickness, Thickness);
        }
    }
}