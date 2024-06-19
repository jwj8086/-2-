using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    private Slider staminaSlider;

    // Start is called before the first frame update
    void Start()
    {
        staminaSlider = GetComponent<Slider>();

        // healthSlider 변수가 할당되어 있는지 확인
        if (staminaSlider != null)
        {
            // 체력 바 UI의 최대값 설정 (플레이어의 최대 체력)
            staminaSlider.maxValue = 100f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInfo.Instance.Stamina += 1 * Time.deltaTime;
        staminaSlider.value = PlayerInfo.Instance.Stamina;

        if(staminaSlider.value < 100)
        {
            staminaSlider.value += Mathf.RoundToInt(Time.deltaTime);
        }
    }
}
