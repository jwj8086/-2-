using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class Monster_Test2 : MonoBehaviour, IDamageAble
{
    #region �⺻ �Ҵ� ����
    [Header("Main Value")]
    public int HP = 600;
    public float speed = 4f;
    public int phyAtk = 35;
    public int phyDef = 100;
    public float engDef = 90f;
    public int detect_Range;
    [Range(0, 100)]
    [SerializeField] float chase_Gauge;
    #endregion

    #region ���� ����
    [Header("Detect List")]
    [SerializeField] private List<GameObject> detected_Objs = new List<GameObject>(); // �÷��̾� �� ���� ���� ����Ʈ
    #endregion

    #region �̵� ����
    [Header("Movement")]
    private float speed_backup;
    private Vector2 patrol_point_L;
    private Vector2 patrol_point_R;
    public float pp_L;
    public float pp_R;
    [SerializeField] private bool point_touch;
    [SerializeField] private int movement;
    [SerializeField] private int movementBackUp;
    [SerializeField] private bool makePP; // PatrolPoint ���� ����
    #endregion

    [Header("Charactor State")]
    #region ���� ����
    [SerializeField] public bool isChase;
    [SerializeField] public bool isAttack; //false�� �ʱ�ȭ �߰�
    [SerializeField] public bool isDie;
    [SerializeField] public bool isDetect;
    [SerializeField] private bool isPatrol;
    //private bool isRolling;
    public bool isHit; //Hit, Wait 
    public bool isWait;
    public bool iskb;
    #endregion

    #region �Լ� ���� ����
    bool patrolNow;
    [SerializeField] private bool dmgDelay;
    #endregion

    #region �Ҵ� ����
    [Header("Assignment")]
    Collider Detect_Area;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;
    GameObject player;
    public Gun gun;
    public GameObject M_HpBarPrefab;
    public GameObject canvas;
    public Transform firePoint;
    public GameObject damageTextPrefab;
    public GameObject laserPrefab;
    public GameObject WarningPrefab;
    public GameObject muzzlePrefab;
    [SerializeField] private Canvas DmgCanvas;
    #endregion

    public AttackType attackType;
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public float melee_Range = 7f;
    public float explode_Range = 5f;
    public float laser_Range = 12f;
    private Vector3 Shot_dir = Vector2.zero;
    public float cooldown = 4f;
    private float LastAttack = 0f;
    private float knockbackForce = 1.01f;
    public float height;
    private Slider healthSlider;
    private bool raycastEnabled = true;
    private Color originalColor;
    private GameObject WarningInstance;
    RectTransform M_hpBar;

    Transform Check;
    Transform Check1;
    Transform attack;


    public enum AttackType
    {
        Suicide,
        Roll,
        melee,
        Laser
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        DmgCanvas = GetComponentInChildren<Canvas>();
        Check = transform.GetChild(1);
        Check1 = transform.GetChild(2);
        attack = transform.GetChild(3);
        originalColor = sr.color;
        M_hpBar = Instantiate(M_HpBarPrefab, canvas.transform).GetComponent<RectTransform>();
        M_hpBar.transform.SetAsFirstSibling();
        healthSlider = M_hpBar.transform.GetComponent<Slider>();

        Make_PP();

        speed_backup = speed;
    }

    void FixedUpdate()
    {
        Direction();
        //rb.velocity = (Vector2.right * movement * speed);//movement���� ���� �̵�(x�� ��� �⺻��)
        
        if (sr.flipX)
        {
            attack.localPosition = new Vector3(Mathf.Abs(attack.localPosition.x), attack.localPosition.y, attack.localPosition.z);
        }
        else
        {
            attack.localPosition = new Vector3(-Mathf.Abs(attack.localPosition.x), attack.localPosition.y, attack.localPosition.z);
        }
        
        if (!isHit && !isWait)
        {
            rb.velocity = new Vector2(movement * speed, rb.velocity.y);
        }

        if(isWait)
        {
            rb.velocity = Vector2.zero;
        }

        if(isHit)
        {
            Knockback();
            Erase();
        }
    }
    private void Update()
    {
        Vector3 _hpBarPos = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x - 1, transform.position.y + height, 0));
        M_hpBar.position = _hpBarPos;

        if (HP <= 0)
        {
            isWait = true;
            anim.SetTrigger("isDie");
        }
        else if(HP > 0)
        {
            healthSlider.value = HP;
        }

        Think();
        
        //rb.velocity = Vector2.right * movement * speed;

        if(raycastEnabled)
        {
            RaycastHit2D hit = Physics2D.Raycast(Check.position, Vector2.down, 3f, groundLayer);
            if (hit.collider == null || hit.collider.CompareTag("Wall"))
            {
                StartCoroutine(DisableRaycastForTime(1f));
                movement = -movement;
            }

            RaycastHit2D hit1 = Physics2D.Raycast(Check1.position, Vector2.down, 3f, groundLayer);
            if (hit1.collider == null || hit1.collider.CompareTag("Wall"))
            {
                StartCoroutine(DisableRaycastForTime(1f));
                movement = -movement;
            }
        }
    }
    void Direction()
    {      //�̵� ���⿡ ���� ��Ʈ������ �¿���ȯ
        if (movement < 0)
            sr.flipX = false;
        else if (movement > 0 && !isAttack && !isWait)
            sr.flipX = true;
    }
    private void Think()
    {
        if (movement != 0 && !isAttack && !isDie)
            anim.SetBool("isRun", true);
        else if(movement == 0 && !isAttack)
            anim.SetBool("isRun", false);

        if (!isChase && !patrolNow &&!isAttack)
            StartCoroutine(Patrol_Move());                  //���� ���°� �ƴ� ��, ���� ��� ����
        if (!isChase)
            chase_Gauge = 100;
        else if (isChase)
        {
            Chase(player);
            isPatrol = false;
            makePP = false;
        }
    }
    IEnumerator Patrol_Move()
    {   //���� ���
        if (!makePP)
        {
            Make_PP();           //PP(���� ����Ʈ) ����
        }
        if (!isChase && !isAttack && makePP)
        {
            isPatrol = true;
            patrolNow = true;
            if ((((this.transform.position.x <= patrol_point_L.x) && !point_touch) || ((this.transform.position.x >= patrol_point_R.x) && !point_touch)))          //������ �� PP�� ��ġ���� �ʾ�����, ����Ʈ�� �������� ���(��ġ�ϱ� ������ ���)
            {
                point_touch = true;
                movementBackUp = movement;
                movement = 0;

                yield return new WaitForSeconds(2f);
                movement = -movementBackUp;
            }
            else if ((((this.transform.position.x > patrol_point_L.x) && !point_touch) || ((this.transform.position.x < patrol_point_R.x) && point_touch))) //������ �� PP�� ��ġ�Ͽ�����, ����Ʈ�� �������� �ʾ��� ���(��ġ �� �̵����� ���)
            {
                point_touch = false;
                yield return new WaitForSeconds(0.06f);
            }
            patrolNow = false;
        }
    }
    void Make_PP()
    {    //Patrol Point(PP) �����
        if (!isChase && !isAttack && !makePP && !isDetect)
        {
            makePP = true;

            patrol_point_L = ((Vector2)transform.position + Vector2.left * pp_L);
            patrol_point_R = ((Vector2)transform.position + Vector2.right * pp_R);

            print("PP�� �����߽��ϴ�. Left: " + patrol_point_L + " Right: " + patrol_point_R);

            movement = UnityEngine.Random.Range(-1, 1);
            if (movement == 0)
                movement = 1;
        }
    }
    IEnumerator Detect_State()
    { //�� ��� �� ���� ����
        print("�� ���� - ���");
        yield return new WaitForSeconds(2f);
        if (isDetect)
        {
            print("�� ���� - Ȯ��");
            print("���� ����");
            isPatrol = false;
            isChase = true;
        }
    }
    void Chase(GameObject player)
    { //�� ����
        Transform playerPos = PlayerInfo.Instance.playerPos;

        if (isChase && chase_Gauge > 0)
        {
            StartCoroutine(Chase_Gauge(playerPos));
            //print(chase_Gauge);
            if (transform.position.x <= playerPos.position.x - 1f && chase_Gauge != 0)
            {
                movement = 1;
            }
            else if (transform.position.x >= playerPos.position.x + 1f && chase_Gauge != 0)
            {
                movement = -1;
            }
            else
                movement = 0;
        }
    }
    IEnumerator Chase_Gauge(Transform playerPos)
    {
        chase_Gauge = Math.Clamp(chase_Gauge, 0, 100);
        yield return new WaitForSeconds(0.1f);
        float dis = Vector2.Distance(playerPos.position, transform.position);

        if (!isAttack && isDetect && !isHit)
        {
            StartCoroutine(Attack());
        }

        else if(!isDetect)
        {
            chase_Gauge -= 2f;

            if (chase_Gauge <= 0)
            {
                isChase = false;
                StopCoroutine("Chase_Gauge");
                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    IEnumerator DisableRaycastForTime(float duration)
    {
        raycastEnabled = false;

        yield return new WaitForSeconds(duration);

        raycastEnabled = true;
    }

    IEnumerator Attack()
    { 
        float dis = Vector2.Distance(player.transform.position, transform.position);
        int dmg = phyAtk;

        if (LastAttack > 0f)
        {
            LastAttack -= Time.deltaTime;
        }

        switch (attackType)
        {
            case AttackType.Suicide:
                /*if (dis <= explode_Range)
                {
                    Suicide(dmg);
                }
                break;*/
            /*case AttackType.Roll:
                    if (LastAttack <= 0f && !isRolling && dis <= 11f)
                    {
                        RolltoPlayer();
                        LastAttack = 7f;
                    }
                break;*/
            case AttackType.melee:
                if(dis <= melee_Range && LastAttack <= 0f && !isAttack)
                {
                    melee();
                }
                break;
            case AttackType.Laser:
                if (dis <= laser_Range && LastAttack <= 0f && !isAttack)
                {
                    FireLaser();
                }
                break;
        }
        yield return null;
    }

    public int CriticalDmg(int dmg)
    {
        int Cridmg;
        if ((int)UnityEngine.Random.Range(1, 101) % (100 / PlayerInfo.Instance.phyCri) == 0)
        {
            Cridmg = dmg * 2;
            return Cridmg;
        }
        else
        {
            Cridmg = dmg;
            return Cridmg;
        }
    }


    void Knockback()
    {
        Vector3 dir = (transform.position - PlayerInfo.Instance.playerPos.position).normalized;
        dir.y = 0;
        dir.z = 0;
        if(!iskb)
        {
            rb.velocity = Vector2.zero;
        }
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
    }

    public void ShowDamageText(int dmg)
    {
        GameObject damageUI = Instantiate(damageTextPrefab);
        damageUI.GetComponent<damageIndicator>().damage = dmg;
        damageUI.transform.SetParent(DmgCanvas.transform, false);
    }

    private GameObject GetObjTag(string tag)
    { // Ư�� �±� ������Ʈ(�÷��̾�) �˻��� �Լ�
        foreach (GameObject obj in detected_Objs)
        {
            if (obj.CompareTag(tag))
            {
                return obj;
            }
        }
        return null;
    }

    /*private void Suicide(int damage)
    {
        anim.SetTrigger("isBomb");
        Destroy(gameObject, 1.4f);
        if (!isAttack)
        {
            PlayerInfo.Instance.Hp -= damage;
            Debug.Log("������");
            isAttack = true;
        }
    }/*

     /*void RolltoPlayer()
    {
        isAttack = true;
        isRolling = true;
        anim.SetTrigger("isReady");
        Vector2 dir = (PlayerInfo.Instance.playerPos.position - transform.position).normalized;
        dir.y = 0;

        StartCoroutine(Roll(dir));
    }*/

    /*IEnumerator Roll(Vector3 dir)
    {
        float disMoved = 0f;
        yield return new WaitForSeconds(2.0f);
        while (disMoved < 10f && isAttack && isRolling)
        {
            gameObject.layer = 12;
            transform.position += dir * 15f * Time.deltaTime;
            disMoved += 15f * Time.deltaTime;
            yield return null;
        }
        if(disMoved >= 10f)
        {
            anim.SetTrigger("isEnd");
            isAttack = false;
            isRolling = false;
            disMoved = 0f;
            gameObject.layer = 6;
        }
    }*/

    void FireLaser()
    {
        isAttack = true;
        isWait = true;

        anim.SetTrigger("isShot");

        LastAttack = cooldown;
    }

    void Warning()
    {
        Vector3 shotDir = (PlayerInfo.Instance.playerPos.position - attack.position).normalized;
        shotDir.z = 0;

        Shot_dir = shotDir;

        gun.StartAiming(shotDir);

        WarningInstance = Instantiate(WarningPrefab, attack.position, Quaternion.identity);
        WarningInstance.transform.right = shotDir;

        WarningInstance.transform.localScale = new Vector3(50f, 0.05f);

        WarningInstance.transform.position = attack.position + shotDir * (25f);
    }

    void Erase()
    {
        Destroy(WarningInstance);
    }

    void Shoting()
    {
        GameObject LaserInstance = Instantiate(laserPrefab, attack.position, Quaternion.identity);
        GameObject MuzzleInstance = Instantiate(muzzlePrefab, attack.position, Quaternion.identity);

        LaserInstance.GetComponent<Rigidbody2D>().velocity = Shot_dir * speed * 8;

        float angle = Mathf.Atan2(Shot_dir.y, Shot_dir.x) * Mathf.Rad2Deg;

        LaserInstance.GetComponent<Rigidbody2D>().rotation = angle;

        gun.StopAiming();
    }

    private void melee()
    {
        isAttack = true;
        anim.SetTrigger("isMelee");
        isWait = true;
        LastAttack = cooldown;
    }

    void meleeAniReSet()
    {
        attack.gameObject.layer = 6;
        isAttack = false;
    }

    private void meleeAniSet()
    {
        attack.gameObject.layer = 12;
        isWait = false;
    }

    void laserAniSet()
    {
        isWait = false;
        isAttack = false;
    }

    void Destroy()
    {
        Destroy(gameObject);
        Destroy(M_hpBar.gameObject);
    }

    void isHitOn()
    {
        //anim.SetTrigger("ishit");
        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        for (int i = 0; i < 1; i++)
        {
            sr.color = new Color(1f, 1f, 1f, 0.8f);
            yield return new WaitForSeconds(0.2f);
            sr.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator Invincibility()
    {
        isHitOn();
        isAttack = false;
        PlayerInfo.Instance.Gauge += 2;
        anim.SetTrigger("ishit");
        isHit = true;
        iskb = true;
        yield return new WaitForSeconds(0.5f);
        iskb = false;
        isHit = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!detected_Objs.Contains(other.gameObject) && other.CompareTag("Monster"))
        {
            detected_Objs.Add(other.gameObject);
            print(other + "��Ÿ ������Ʈ Ʈ���� ����");                              // ��� ��� Ȱ��ȭ
        }
        if (!detected_Objs.Contains(other.gameObject) && (other.CompareTag("Player") || other.CompareTag("Monster")))
        {
            detected_Objs.Add(other.gameObject);
            print(other + "������Ʈ Ʈ���� ����");                              //��� ��� Ȱ��ȭ

            if (other.CompareTag("Player"))               //�˻��� �±װ� �÷��̾�� ���ٸ�
            {
                player = other.gameObject;
                StopCoroutine(Patrol_Move());    //��� ��� Ȱ��ȭ ������ �ڷ�ƾ�� �ѱ�
                isDetect = true;
                if (isDetect)
                    StartCoroutine(Detect_State());      //��� ��� Ȱ��ȭ ������ �ڷ�ƾ�� �ѱ�
                print(other + "�÷��̾� ������Ʈ Ʈ���� ����");
            }
        }

        if (other.CompareTag("Player") && attack.gameObject.layer == 12)
        {
            IDamageAble Dmg = other.GetComponent<IDamageAble>();
            if (Dmg != null)
            {
                Dmg.Damage(phyAtk, true);
            }
            isAttack = false;
            LastAttack = cooldown;
        }

        if(gameObject.layer == 13 && other.CompareTag("Player"))
        {
            isDetect = true;
        }

        /*if (isRolling && other.CompareTag("Wall"))
        {
            isRolling = false;
            isAttack = false;
            anim.SetTrigger("isEnd");

            gameObject.layer = 6;
            LastAttack = cooldown;
        }*/

        if (anim.GetBool("isMelee") && other.CompareTag("Player"))
        {
            gameObject.layer = 6;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (detected_Objs.Contains(other.gameObject))
        {
            detected_Objs.Remove(other.gameObject);
            print(other + "������Ʈ Ʈ���� ����");
            if (other.CompareTag("Player"))
            {
                print("�� ���� ����");
            }
        }

        if (gameObject.layer == 13 && other.CompareTag("Player"))
        {
            isDetect = false;
        }
    }


    public void Damage(int P_atk, bool isPhy)
    {
        if (isPhy)
        {
            int damage = Mathf.RoundToInt(P_atk * (1 - phyDef / (100 + phyDef)));
            HP -= PlayerInfo.Instance.CriticalDmg(damage);
            StartCoroutine(Invincibility());
        }
    }
}