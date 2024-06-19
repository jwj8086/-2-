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

        // healthSlider ������ �Ҵ�Ǿ� �ִ��� Ȯ��
        if (staminaSlider != null)
        {
            // ü�� �� UI�� �ִ밪 ���� (�÷��̾��� �ִ� ü��)
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
