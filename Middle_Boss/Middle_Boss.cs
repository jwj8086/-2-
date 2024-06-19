using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Middle_Boss : MonoBehaviour
{
    #region 기본 할당 변수
    [Header("Main Value")]
    public int HP = 800;
    public float speed = 3f;
    public int phyAtk = 50;
    public int phyDef = 100;
    public float meleeRange = 5f;
    public float laserRange = 8f;
    public float attackCooldown = 2f;
    private float lastAttackTime;
    private bool isAttack = false;
    private bool isChase = false;
    private bool isHit = false;
    #endregion

    #region 이동 및 추적 관련
    [Header("Movement")]
    public float detectionRange = 15f;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    #endregion

    #region 공격 관련
    [Header("Attack")]
    public GameObject laserPrefab;
    public Transform firePoint;
    public float lineDuration = 1.5f;
    private LineRenderer lineRenderer;
    #endregion


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (HP <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if(isHit)
        {
            anim.SetTrigger("ishit");
        }

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerInfo.Instance.playerPos.position);

        if (distanceToPlayer <= detectionRange)
        {
            isChase = true;
        }
        else
        {
            isChase = false;
        }

        if (isChase)
        {
            ChasePlayer();
            if(Time.time >= lastAttackTime + attackCooldown)
            {
                RandomAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (PlayerInfo.Instance.playerPos.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

        sr.flipX = direction.x > 0;
    }

    void RandomAttack()
    {
        int attackType = Random.Range(0, 3);
        switch (attackType)
        {
            case 0:
                MeleeAttack();
                break;
            case 1:
                StartCoroutine(FireLaser());
                break;
            case 2:
                RollAttack();
                break;
        }
    }

    void MeleeAttack()
    {
        if(Vector2.Distance(PlayerInfo.Instance.playerPos.position, transform.position) <= meleeRange)
        {
            PlayerInfo.Instance.Hp -= Mathf.RoundToInt(phyAtk * (1 - PlayerInfo.Instance.Def / (100 + PlayerInfo.Instance.Def)));
        }
    }

    IEnumerator FireLaser()
    {
        isAttack = true;
        rb.velocity = Vector2.zero;

        Vector2 fPoint = firePoint.position;
        Vector2 fireDir = (PlayerInfo.Instance.playerPos.position - firePoint.position).normalized;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, fPoint);
        lineRenderer.SetPosition(1, fPoint + fireDir * laserRange);
        lineRenderer.enabled = true;

        float elapsedTime = 0f;

        while (elapsedTime < lineDuration)
        {
            if (isHit)
            {
                lineRenderer.enabled = false;
            }
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / lineDuration;
            yield return null;
        }

        lineRenderer.enabled = false;

        if (!isHit && isAttack && elapsedTime > lineDuration)
        {
            GameObject laser = Instantiate(laserPrefab, fPoint, Quaternion.identity);
            Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
            laserRb.velocity = fireDir * speed * 6;

            Destroy(laser, laserRange / (speed * 6));
        }
    }

    void RollAttack()
    {
        anim.SetTrigger("isReady");
    }

    void TakeDamage(int amount) 
    {
        HP -= amount;
    }

    int CriticalDmg(int dmg)
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
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("P_atk"))
        {
            anim.SetTrigger("ishit");
            int dmg = Mathf.RoundToInt(PlayerInfo.Instance.phyAtk * (1 - phyDef / (100 + phyDef)));
            TakeDamage(CriticalDmg(dmg));
            PlayerInfo.Instance.Gauge += 2;
        }
    }
}