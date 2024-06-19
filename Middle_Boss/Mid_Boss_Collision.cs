using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Mid_Boss_Collision : MonoBehaviour
{
    private Mid_Boss_Stats stats;
    private Mid_Boss_Action action;

    private void Awake()
    {
        stats = GetComponent<Mid_Boss_Stats>();
        action = GetComponentInParent<Mid_Boss_Action>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageAble Dmg = collision.GetComponent<IDamageAble>();
        if(Dmg != null)
        {
            Dmg.Damage(stats.Atk, true);
        }

        if (action.isDash && !action.isWait && (collision.CompareTag("Player") || collision.gameObject.layer == 10))
        {
            action.DashEnd();
        }
    }
}