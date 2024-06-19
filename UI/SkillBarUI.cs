using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarUI : MonoBehaviour
{
    private Slider skillSlider;

    // Start is called before the first frame update
    void Start()
    {
        skillSlider = GetComponent<Slider>();

        skillSlider.value = 0f;
        skillSlider.maxValue = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
