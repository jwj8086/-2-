using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Mid_Boss_Action : MonoBehaviour, IDamageAble
{
    private Mid_Boss_Stats stats;
    private float Timer = 0f;
    private Animator anim;
    private Transform attack;
    private Transform attack1;
    private Transform attack2;
    //private Transform Check;
    //private Transform Check1;
    private GameObject WarningInstance;
    private Vector2 dir = Vector2.zero;
    private Vector3 Shot_dir = Vector2.zero;
    private Color originColor;
    //private bool raycastEnabled = true;

    public LayerMask groundLayer;

    #region 상태변수
    public bool isAttack;
    public bool isWait;
    public bool isDetect;
    public bool isRoll;
    public bool isRolling;
    public bool isDash;
    public bool isD_Ready;
    public bool isHit;
    public bool isdie;
    public bool isDying;
    #endregion

    private void Awake()
    {
        stats = GetComponent<Mid_Boss_Stats>();
        anim = GetComponent<Animator>();
        //Check = transform.GetChild(0);
        //Check1 = transform.GetChild(1);
        attack = transform.GetChild(2);
        attack1 = transform.GetChild(3);
        attack2 = transform.GetChild(4);
        originColor = stats.sr.color;
    }

    private void Update()
    {
        if(!isDash)
        {
            dir = (PlayerInfo.Instance.playerPos.position - transform.position).normalized;
            dir.y = 0;
        }

        float dis = Mathf.Abs(PlayerInfo.Instance.playerPos.position.x - transform.position.x);

        if (stats.Hp < 0 && !isDying)
        {
            isdie = true;
        }

        if(isdie)
        {
            Die();
            isDying = true;
            isdie = false;
        }

        if (Timer > 0f)
        {
            Timer -= Time.deltaTime;
        }

        if (!isWait && dis <= stats.DetectRange)
        {
            if(!isAttack)
            {
                anim.SetBool("isRun", true);
            }
            stats.rb.velocity = new Vector2(dir.x * stats.speed, stats.rb.velocity.y);
            stats.sr.flipX = dir.x > 0;
        }


        if (isWait || dis >= stats.DetectRange)
        {
            if (!isAttack)
            {
                anim.SetBool("isRun", false);
            }
            stats.rb.velocity = Vector2.zero;
        }

        if(isDash && !isD_Ready && !isWait)
        {
            stats.rb.velocity = new Vector2(dir.x * stats.speed * 3, stats.rb.velocity.y);
        }

        Vector3 scale1 = attack1.localScale;
        scale1.x = stats.sr.flipX ? -1 : 1;
        attack1.localScale = scale1;
        
        if(stats.sr.flipX)
        {
            attack.localPosition = new Vector3(Mathf.Abs(stats.fireP.localPosition.x), stats.fireP.localPosition.y, stats.fireP.localPosition.z);
        }
        else
        {
            attack.localPosition = new Vector3(-Mathf.Abs(stats.fireP.localPosition.x), stats.fireP.localPosition.y, stats.fireP.localPosition.z);
        }

        if (Timer <= 0f && !isAttack)
        {
            RangeCheck(dis);
        }

        /*if(isRolling && isAttack) 구르기를 시작한 상태고 공격중이라면 구르기 시간이 0이 될때까지 구름
        {
            stats.rollTime -= Time.deltaTime;

            if(stats.rollTime <= 0f)
            {
                stats.rollTime = 3f;
                isAttack = false;
                isRolling = false;
            }
        }*/

        /*if(isRoll && !isAttack)
        {
            EndAttack_R();
        }*/

        /*if (raycastEnabled)
        {
            RaycastHit2D hit = Physics2D.Raycast(Check.position, Vector2.down, 3f, groundLayer);
            if (hit.collider == null || hit.collider.CompareTag("Wall"))
            {
                StartCoroutine(DisableRaycastForTime(1f));
            }

            RaycastHit2D hit1 = Physics2D.Raycast(Check1.position, Vector2.down, 3f, groundLayer);
            if (hit1.collider == null || hit1.collider.CompareTag("Wall"))
            {
                StartCoroutine(DisableRaycastForTime(1f));
            }
        }*/
    }

    /*IEnumerator DisableRaycastForTime(float duration)
    {
        raycastEnabled = false;

        yield return new WaitForSeconds(duration);

        raycastEnabled = true;
    }*/

    void isHitOn()
    {
        //anim.SetTrigger("ishit");
        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        for(int i = 0;i < stats.blinkCount;i++)
        {
            stats.sr.color = new Color(1f,1f, 1f, 0.8f);
            yield return new WaitForSeconds(stats.BlinkDur);
            stats.sr.color = originColor;
            yield return new WaitForSeconds(stats.BlinkDur);
        }
    }

    void Die()
    {
        stats.Hp += 1;
        isWait = true;
        anim.SetTrigger("isdie");
        isdie = false;
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
        isDying = false;
    }

    void RangeCheck(float dis)
    {
        if (stats.meleeRange >= dis && Timer <= 0f)
        {
            isAttack = true;
            Timer = stats.attackColldown;
            MeleeAttack();
        }
        
        /*else if (stats.middleRange >= dis && dis > stats.meleeRange && Timer <= 0f)
        {
            isRoll = true;
            isAttack = true;
            RollAttack();
        }*/

        else if (stats.middleRange >= dis && dis > stats.meleeRange && Timer <= 0f)
        {
            isDash = true;
            isD_Ready = true;
            isAttack = true;
            DashReady();
        }
        else if (stats.laserRange >= dis && dis > stats.middleRange && Timer <= 0f)
        {
            isAttack = true;
            LaserAttack();
        }
    }

    private void MeleeAttack()
    {
        Timer = stats.attackColldown;
        anim.SetTrigger("isMelee");
        isWait = true;
    }

    void DashReady()
    {
        isWait = true;
        anim.SetTrigger("isDReady");
    }

    void Dash_Ready_Off()
    {
        isWait = false;
        isD_Ready = false;
        attack2.gameObject.layer = 12;
    }

    public void DashEnd()
    {
        if(attack2.gameObject.layer != 7)
        {
            attack2.gameObject.layer = 7;
        }
        isWait = true;
        anim.SetTrigger("isDEnd");
    }
    
    private void LaserAttack()
    {
        Timer = stats.attackColldown;
        anim.SetTrigger("isShot");

        //Vector3 dir = (PlayerInfo.Instance.playerPos.position - attack.position).normalized;
        //var laserEndpoints = LaserLineEndpoints.Create(attack.position, attack.position + dir * 20f);
        //L_Controll.DrawLaserBeam(laserEndpoints, 0);
        isWait = true;
    }

    void ShotReady()
    {
        Vector3 shotDir = (PlayerInfo.Instance.playerPos.position - stats.fireP.position).normalized;
        shotDir.z = 0;

        Shot_dir = shotDir;

        WarningInstance = Instantiate(stats.WarningPrefab, stats.fireP.position, Quaternion.identity);
        WarningInstance.transform.right = shotDir;

        WarningInstance.transform.localScale = new Vector3(50f, 0.05f);

        WarningInstance.transform.position = stats.fireP.position + shotDir * (25f);
    }

    /*private void RollAttack()
    {
        anim.SetTrigger("isReady");
        isWait = true;
    }*/

    /*void RollStart()
    {
        isRolling = true;
        attack2.gameObject.layer = 12;
        stats.speed = 15f;
        isWait = false;
    }*/

    void Erase()
    {
        Destroy(WarningInstance);
    }

    void Shoting()
    {
        GameObject LaserInstance = Instantiate(stats.laserPrefab, attack.position, Quaternion.identity);
        GameObject MuzzleInstance = Instantiate(stats.muzzlePrefab, attack.position, Quaternion.identity);

        LaserInstance.GetComponent<Rigidbody2D>().velocity = Shot_dir * stats.speed * 8;

        float angle = Mathf.Atan2(Shot_dir.y, Shot_dir.x) * Mathf.Rad2Deg;

        LaserInstance.GetComponent<Rigidbody2D>().rotation = angle;
    }

    void MoveStart()
    {
        if(isDash)
        {
            isDash = false;
        }
        isWait = false;
        isAttack = false;
        if(attack1.gameObject.layer != 7)
        {
            attack1.gameObject.layer = 7;
        }
    }

    void CanAttack_M() 
    {
        attack1.gameObject.layer = 12;
    }

    /*public void EndAttack_R()
    {
        Timer = stats.attackColldown;
        attack2.gameObject.layer = 6;
        stats.speed = 5f;
        anim.SetTrigger("isEnd");
        isWait = true;
        isRoll = false;
    }*/

    IEnumerator Invincibility()
    {
        isHitOn();
        isHit = true;
        yield return new WaitForSeconds(0.5f);
        isHit = false;
    }

    public void InvincibilityOn()
    {
        StartCoroutine(Invincibility());
    }

    public void Damage(int P_atk, bool isPhy)
    {
        if(isPhy)
        {
            int damage = Mathf.RoundToInt(P_atk * (1 - stats.PhyDef / (100 + stats.PhyDef)));
            stats.Hp -= PlayerInfo.Instance.CriticalDmg(damage);
            StartCoroutine(Invincibility());
        }
    }
}
