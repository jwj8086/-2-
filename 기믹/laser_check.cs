using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class laser_check : MonoBehaviour, IDamageAble
{
    public Vector3 back;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            IDamageAble Dmg = other.GetComponent<IDamageAble>();
            if (Dmg != null)
            {
                Dmg.Damage(5, true);
            }
            StartCoroutine(Recall());
        }
    }

IEnumerator Recall()
    {
        yield return new WaitForSeconds(0.2f);
        PlayerInfo.Instance.playerPos.position = back;

    }

    public void Damage(int atk, bool type)
    {

    }
}
