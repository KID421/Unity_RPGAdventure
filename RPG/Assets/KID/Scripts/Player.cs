using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour
{
    [Header("移動速度"), Range(1, 2500)]
    public float speed = 1600;
    [Header("移動阻力"), Range(1, 10)]
    public float drag = 3.5f;
    [Header("旋轉速度"), Range(1, 30)]
    public float turn = 10;
    [Header("攝影機追蹤速度"), Range(1, 150)]
    public float camSpeed = 10;
    [Header("音效")]
    public AudioClip[] soundAtk;
    [Header("攻擊"), Range(1, 100)]
    public float attack1 = 50;
    [Range(100, 500)]
    public float attack2 = 100;
    [Header("爆擊機率"), Range(0, 1)]
    public float attackDouble = 0.3f;

    private Animator ani;
    private Rigidbody rig;
    private AudioSource aud;
    private Vector3 ang = Vector3.zero;
    private float attackTimer;
    private float attackInterval = 0.5f;
    private int attackCount;
    private Transform cam;
    private Image imgHp;
    private float hp = 500;
    private float hpMax;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;

        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();

        hpMax = hp;
        rig.drag = drag;

        imgHp = transform.Find("畫布").Find("血條").GetComponent<Image>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        Rotate();
        Attack();
    }

    private void LateUpdate()
    {
        CameraControl();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.forward * 0.5f + transform.up, transform.forward);
    }

    /// <summary>
    /// 旋轉
    /// </summary>
    private void Rotate()
    {
        if (attackCount > 0) return;

        if (Input.GetKey(KeyCode.W)) ang.y = 0;
        else if (Input.GetKey(KeyCode.S)) ang.y = 180;
        else if (Input.GetKey(KeyCode.A)) ang.y = -90;
        else if (Input.GetKey(KeyCode.D)) ang.y = 90;

        Quaternion qua = Quaternion.Euler(ang);
        transform.rotation = Quaternion.Lerp(transform.rotation, qua, 0.9f * Time.deltaTime * turn);
        ani.SetFloat("走路跑步", rig.velocity.magnitude);
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        if (attackCount > 0) return;
        if (rig.velocity.magnitude > 10) return;
        if (ani.GetCurrentAnimatorStateInfo(0).IsName("受傷")) return;

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");
        
        if (Mathf.Abs(v) > 0) rig.AddForce(transform.forward * speed * Time.fixedDeltaTime * Mathf.Abs(v));
        else if (Mathf.Abs(h) > 0) rig.AddForce(transform.forward * speed * Time.fixedDeltaTime * Mathf.Abs(h));
    }

    /// <summary>
    /// 攻擊
    /// </summary>
    private void Attack()
    {
        if (attackCount == 1)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackInterval)
            {
                attackTimer = 0;
                attackCount = 0;
            }
        }
        else if (attackCount == 2 && ani.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
        {
            attackTimer = 0;
            attackCount = 0;
        }

        if (attackCount < 2 && Input.GetKeyDown(KeyCode.Mouse0) && !ani.GetCurrentAnimatorStateInfo(0).IsName("攻擊2"))
        {
            aud.PlayOneShot(soundAtk[attackCount], 10);
            attackCount++;

            RaycastHit hit;
            if (Physics.Raycast(transform.position + transform.forward * 0.5f + transform.up, transform.forward, out hit, 1))
            {
                Color color = Color.white;
                float atk = attackCount == 1 ? attack1 + AttackRandom() : attack2 + AttackRandom();
                if (Random.Range(0f, 1f) < attackDouble)
                {
                    atk *= 2;
                    color = Color.red;
                    StartCoroutine(ShakeCamera());
                }
                hit.collider.GetComponent<Enemy>().Damage(atk, color);
            }
        }

        ani.SetInteger("攻擊", attackCount);
    }

    /// <summary>
    /// 攻擊隨機傷害值
    /// </summary>
    /// <returns>傳回隨機 0 - 10 傷害值</returns>
    private float AttackRandom()
    {
        return Random.Range(0, 10);
    }

    public void Damage(float damage)
    {
        hp -= damage;
        imgHp.fillAmount = hp / hpMax;
        ani.SetTrigger("受傷觸發");
    }

    /// <summary>
    /// 攝影機追蹤
    /// </summary>
    private void CameraControl()
    {
        Vector3 pos = transform.position;
        pos.y = 15;
        pos.z -= 9;

        cam.position = Vector3.Lerp(cam.position, pos, 0.3f * Time.deltaTime * camSpeed);
    }

    /// <summary>
    /// 晃動攝影機效果
    /// </summary>
    private IEnumerator ShakeCamera()
    {
        for (int i = 0; i < 5; i++)
        {
            cam.position += i % 2 == 1 ? Vector3.right * 0.2f : -Vector3.right * 0.2f;
            yield return new WaitForSeconds(0.05f);
        }
    }
}

