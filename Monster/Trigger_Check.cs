using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Check : MonoBehaviour
{
    public Monster_Test2 Mon;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameObject.layer == 13 && collision.CompareTag("Player"))
        {
            Mon.isDetect = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (gameObject.layer == 13 && collision.CompareTag("Player"))
        {
            Mon.isDetect = false;
        }
    }
}
