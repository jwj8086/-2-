using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

public class Mid_Boss_Stats : MonoBehaviour
{
    #region 기본 할당 변수
    public int Hp = 800;
    public float speed = 5f;
    public int Atk = 30;
    public int PhyDef = 100;
    public int EngDef = 100;
    public float DetectRange = 30f;
    public float meleeRange = 10f;
    public float laserRange = 20f;
    public float middleRange = 15f;
    //public float rollTime = 3f;
    public Transform fireP;
    public GameObject laserPrefab;
    public GameObject WarningPrefab;
    public GameObject muzzlePrefab;
    public float attackColldown = 5f;
    public float BlinkDur = 0.2f;
    public int blinkCount = 1;
    #endregion

    public Rigidbody2D rb;
    public SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }
}
