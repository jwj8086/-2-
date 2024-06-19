using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform gun;
    public SpriteRenderer M_sr;
    public Transform F_point;

    private Vector3 Pos;
    private Quaternion Rot;
    private Vector3 Dir;
    private bool isAiming;

    private void Start()
    {
        Pos = gun.localPosition;
        Rot = gun.localRotation;
    }


    private void Update()
    {
        
        Vector3 dir = (PlayerInfo.Instance.playerPos.position - gun.position).normalized;
        dir.y = 0;
        dir.z = 0;

        if(!M_sr.flipX)
        {
            dir *= -1;
        }

        if(isAiming && -Dir != (F_point.position - transform.position).normalized)
        {
            Quaternion rotation = Quaternion.LookRotation(Dir);
            Vector3 euler = rotation.eulerAngles;
            euler.y = 0;
            gun.rotation = Quaternion.Lerp(gun.rotation, rotation, Time.deltaTime * 1f);
        }
        else
        {
            gun.localPosition = Vector3.Lerp(gun.localPosition, Pos, Time.deltaTime * 5f);
            gun.localRotation = Quaternion.Lerp(gun.localRotation, Rot, Time.deltaTime * 5f);
        }
    }

    public void StartAiming(Vector3 shot)
    {
        Dir = shot;
        isAiming = true;
    }

    public void StopAiming()
    {
        isAiming = false;
    }
}
