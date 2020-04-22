using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("怪物資料")]
    public EnemyData data;

    private Animator ani;
    private Rigidbody rig;
    private NavMeshAgent nma;
    private Transform player;
    private Transform can;
    private int randomAttack;
    private float timer;
    private float hp;
    private float hpMax;
    private bool attacking;
    private Image hpBar;
    private Text textDamage;
    private RaycastHit hit;

    private void Start()
    {
        ani = GetComponent<Animator>();
        nma = GetComponent<NavMeshAgent>();
        rig = GetComponent<Rigidbody>();
        nma.speed = data.speed;

        can = transform.Find("血條");
        hpBar = can.Find("血條").GetComponent<Image>();
        textDamage = can.Find("傷害值").GetComponent<Text>();
        player = FindObjectOfType<Player>().transform;

        hp = data.hp;
        hpMax = hp;
    }

    private void Update()
    {
        Move();
        FixedCanvas();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, data.rangeTrack);

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, data.rangeAttack);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.forward * 1.5f + transform.up, transform.forward * 2.5f);
        Gizmos.DrawRay(transform.position + transform.forward * 1.5f + transform.up + transform.right * 0.5f, transform.forward * 2.5f);
        Gizmos.DrawRay(transform.position + transform.forward * 1.5f + transform.up - transform.right * 0.5f, transform.forward * 2.5f);
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        if (ani.GetBool("死亡開關")) return;
        if (ani.GetCurrentAnimatorStateInfo(0).IsName("攻擊1") || ani.GetCurrentAnimatorStateInfo(0).IsName("攻擊2") || ani.GetCurrentAnimatorStateInfo(0).IsName("受傷"))
        {
            nma.isStopped = true;
            return;
        }

        float dis = Vector3.Distance(transform.position, player.position);

        if (dis < data.rangeAttack)
        {
            Attack();
        }
        else if (dis < data.rangeTrack)
        {
            nma.isStopped = false;
            nma.SetDestination(player.position);
        }
        else
        {
            nma.isStopped = true;
        }

        ani.SetBool("走路開關", !nma.isStopped);
    }

    /// <summary>
    /// 攻擊
    /// </summary>
    private void Attack()
    {
        Vector3 pos = player.position - transform.position;
        Quaternion toRot = Quaternion.LookRotation(pos);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRot, 0.3f * Time.deltaTime * 10);

        nma.isStopped = true;
        nma.velocity = Vector3.zero;

        timer += Time.deltaTime;

        if (timer >= data.attackCD)
        {
            timer = 0;
            randomAttack = Random.Range(1, 3);
            ani.SetInteger("攻擊", randomAttack);
            ani.SetBool("攻擊中", true);
            Invoke("AttackStop", 1f);
            attacking = true;

            if (randomAttack == 1) StartCoroutine(DelayDamageToPlayer(1.8f));
            else if (randomAttack == 2) StartCoroutine(DelayDamageToPlayer(1f));
        }
    }

    private IEnumerator DelayDamageToPlayer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (Physics.Raycast(transform.position + transform.forward * 1.5f + transform.up, transform.forward, out hit, 2.5f) ||
                Physics.Raycast(transform.position + transform.forward * 1.5f + transform.up + transform.right * 0.5f, transform.forward, out hit, 2.5f) ||
                Physics.Raycast(transform.position + transform.forward * 1.5f + transform.up - transform.right * 0.5f, transform.forward, out hit, 2.5f))
        {
            float damage = data.attack + Random.Range(0, 100);
            hit.collider.GetComponent<Player>().Damage(damage);
        }
    }

    /// <summary>
    /// 攻擊後攻擊數值歸零
    /// </summary>
    private void AttackStop()
    {
        ani.SetInteger("攻擊", 0);
        ani.SetBool("攻擊中", false);
        attacking = false;
    }

    /// <summary>
    /// 固定畫布
    /// </summary>
    private void FixedCanvas()
    {
        can.eulerAngles = new Vector3(45, 0, 0);
    }

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="damage">接收到的傷害值</param>
    public void Damage(float damage, Color color)
    {
        hp -= damage;
        hpBar.fillAmount = hp / hpMax;
        textDamage.color = color;
        StopAllCoroutines();
        StartCoroutine(ShowDamage(damage));

        if (hp <= 0) Dead();

        if (ani.GetCurrentAnimatorStateInfo(0).IsName("攻擊1") || ani.GetCurrentAnimatorStateInfo(0).IsName("攻擊2") || attacking) return;
        ani.SetTrigger("受傷觸發");
    }

    private IEnumerator ShowDamage(float value)
    {
        textDamage.rectTransform.anchoredPosition = new Vector2(0, 50);
        textDamage.text = value.ToString();

        for (int i = 0; i < 20; i++)
        {
            textDamage.rectTransform.anchoredPosition += Vector2.up * 8;
            yield return null;
        }

        textDamage.text = "";
    }

    private void Dead()
    {
        rig.Sleep();
        ani.SetBool("死亡開關", true);
        nma.isStopped = true;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 2.5f);
        Invoke("DelayCloseCanvas", 1.5f);
    }

    private void DelayCloseCanvas()
    {
        can.gameObject.SetActive(false);
    }
}
