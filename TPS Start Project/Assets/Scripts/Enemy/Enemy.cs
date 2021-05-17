using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;//Navigation 이용

//좀비가 플레이어를 인식하는 영역을 시각적으로 표시할 예정
//이 기능은 유니티 에디터를 사용하게 되며 이 기능들을 빌드할 때에는 빠지고 실행하게 설정
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Enemy : LivingEntity
{
    private enum State
    {
        Patrol,//평소 상태
        Tracking,//추적
        AttackBegin,//공격 시작
        Attacking//공격 중
    }

    public GameObject[] items;//좀비가 죽었을 때 생성될 아이템 배열(회복약, 총알, 탈출 재료)

    private State state;

    private NavMeshAgent agent;
    private Animator animator;

    public Transform attackRoot;//attackRoot를 중심으로 플레이어가 공격을 당하도록 설정
    public Transform eyeTransform;//좀비 시야의 기준점

    private AudioSource audioPlayer;
    public AudioClip hitClip;
    public AudioClip deathClip;

    public float runSpeed = 10f;//좀비 이동속도
    [Range(0.01f, 2f)] public float turnSmoothTime = 0.1f;//좀비가 방향 회전시 지연시간
    private float turnSmoothVelocity;//현재 회전실시간 변화량 저장 변수

    public float damage = 30f;//데미지
    public float attackRadius = 2f;//범위
    private float attackDistance;//사정거리

    public float fieldOfView = 160f;//좀비 시야각 수평방향 160도
    public float viewDistance = 20f;//좀비가 볼수있는 거리 20f
    public float patrolSpeed = 3f;//비추적시 스피드

    [HideInInspector] public LivingEntity targetEntity;
    public LayerMask whatIsTarget;//적인지를 인식할 때 쓰이는 레이어마스크


    private RaycastHit[] hits = new RaycastHit[10];//범위기반의 공격
    private List<LivingEntity> lastAttackedTargets = new List<LivingEntity>();//공격을 새로 할 때마다 초기화되는 리스트

    private bool hasTarget => targetEntity != null && !targetEntity.dead;//추적할 대상의 존재여부 판단

#if UNITY_EDITOR//전처리기 실제 빌드에선 제외

    private void OnDrawGizmosSelected()//Scene 창에서 시각적으로 잘 보일 수 있도록 함
    {
        if (attackRoot != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(attackRoot.position, attackRadius);
        }

        if (eyeTransform != null)
        {
            var leftEyeRotation = Quaternion.AngleAxis(-fieldOfView * 0.5f, Vector3.up);
            var leftRayDirection = leftEyeRotation * transform.forward;
            Handles.color = new Color(1f, 1f, 1f, 0.2f);
            Handles.DrawSolidArc(eyeTransform.position, Vector3.up, leftRayDirection, fieldOfView, viewDistance);
        }
    }

#endif

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioPlayer = GetComponent<AudioSource>();

        //자신의 위치와 플레이어와의 사이가 일정 사거리 안에 있을 경우 공격을 실행할 것임
        var attackPivot = attackRoot.position;
        attackPivot.y = transform.position.y;
        attackDistance = Vector3.Distance(transform.position, attackPivot) + attackRadius;

        agent.stoppingDistance = attackDistance;//공격사거리내 들어오면 움직임을 멈춤
        agent.speed = patrolSpeed;
    }

    public void Setup(float health, float damage, float runSpeed, float patrolSpeed)
    {
        //초기 체력, 현재 체력, 데미지, 스피드를 새로 지정함
        this.startingHealth = health;
        this.health = health;

        this.damage = damage;

        this.runSpeed = runSpeed;
        this.patrolSpeed = patrolSpeed;

        agent.speed = patrolSpeed;
    }

    private void Start()
    {
        StartCoroutine(UpdatePath());//코루틴 실행
    }

    private void Update()
    {
        if (dead)//죽었을 경우
            return;//Update 메소드 종료

        if (state == State.Tracking)//추적 상태일 경우
        {
            //플레이어와 좀비사이의 거리 계산
            var distance = Vector3.Distance(targetEntity.transform.position, transform.position);

            //사거리내 플레이어가 있을 경우
            if (distance <= attackDistance)
            {
                BeginAttack();//공격 시작
                //좀비의 공격은 팔이 휘두르는 순간 공격이 시작되며 휘두르는 것이 끝나면 공격이 끝남
            }
        }

        animator.SetFloat("Speed", agent.desiredVelocity.magnitude);
    }

    private void FixedUpdate()
    {
        if (dead) return;//죽었을 경우 FixedUpdate 메소드 종료

        if (state == State.AttackBegin || state == State.Attacking)//공격을 시작했거나 공격 중일 경우
        {
            //좀비의 방향을 플레이어를 향해 설정
            var lookRotation = Quaternion.LookRotation(targetEntity.transform.position - transform.position);
            var targetAngleY = lookRotation.eulerAngles.y;

            targetAngleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY,
                ref turnSmoothVelocity, turnSmoothTime);
            transform.eulerAngles = Vector3.up * targetAngleY;
        }

        //FixedUpdate는 0.02초마다 실행됨
        //0.02초 사이내로 공격을 실행하는 좀비 혹은 공격당할 플레이어가 재빨리 이동하여 감지가 안될경우를 대비
        //Physics.CastSphere를 사용하여 오브젝트가 움직였을때 움직인 괘적사이에 겹치는 물체를 감지할 것
        if (state == State.Attacking)
        {
            //좀비의 방향
            var direction = transform.forward;
            //이동하는 거리계산
            //이 거리내의 오브젝트 감지 가능
            var deltaDistance = agent.velocity.magnitude * Time.deltaTime;

            //deltaDistance거리내 포착된 hits들의 개수를 설정함
            var size = Physics.SphereCastNonAlloc(attackRoot.position, attackRadius, direction, hits,
            deltaDistance, whatIsTarget);

            for (var i = 0; i < size; i++)
            {
                //포착된 hits들이 LivingEntity를 가지고 있는 지를 확인함
                var attackTargetEntity = hits[i].collider.GetComponent<LivingEntity>();

                //attackTargetEntity가 null이 아니고 직전까지 공격을 가한 오브젝트가 아닐 경우
                if (attackTargetEntity != null && !lastAttackedTargets.Contains(attackTargetEntity))
                {
                    var message = new DamageMessage();//새로운 공격 메세지 실행
                    message.amount = damage;//데미지량 설정
                    message.damager = gameObject;//공격을 가하는 오브젝트 = 좀비
                    if (hits[i].distance <= 0f)//Physics.SphereCastNonAlloc가 실행되자마자 포착된 hit이 있을 경우
                        message.hitPoint = attackRoot.position;
                    else//아닐 경우(휘두르는 도중)
                        message.hitPoint = hits[i].point;

                    message.hitNormal = hits[i].normal;

                    attackTargetEntity.ApplyDamage(message);//공격받는 오브젝트에 데미지 입힘
                    lastAttackedTargets.Add(attackTargetEntity);//최근에 공격받은 타겟으로 지정하여 짧은 시간내 여러 공격 당하는 것 방지
                    break;
                }
            }
        }
    }

    //Coroutine으로 실행
    private IEnumerator UpdatePath()
    {
        while (!dead)//좀비가 죽지 않은 경우
        {
            if (hasTarget)//타겟이 있을 경우(플레이어 발견)
            {
                if (state == State.Patrol)//평소 상태였다면
                {
                    state = State.Tracking;//추적상태로 변환
                    agent.speed = runSpeed;//달릴 때 속도로 변환
                }
                agent.SetDestination(targetEntity.transform.position);//좀비가 목표로하는 위치를 플레이어의 위치로 지정함(지속적인 추적)
            }
            else//타겟이 없을 경우
            {
                if (targetEntity != null) targetEntity = null;//기존에 타겟이 있엇을 경우 타겟을 지움

                if (state != State.Patrol)//추적 상태였을 경우(플레이어를 놓침)
                {
                    state = State.Patrol;//평소 상태로 변환
                    agent.speed = patrolSpeed;//평소 스피드로 변환
                }

                //0.05초 마다 코루틴이 실행되는데 0.05초마다 계속해서 위치를 새로 지정할 경우 움직임이 부자연스러움
                //좀비가 지정된 목적지와의 거리가 1f이하일 경우에만 새로 이동할 위치를 지정하게끔 설정함
                if (agent.remainingDistance <= 1f)
                {
                    //자신을 기준으로 20f만큼의 범위 내의 NavMesh의 랜덤한 위치를 지정하여 그곳으로 좀비가 이동함
                    var patrolTargetPostion
                        = Utility.GetRandomPointOnNavMesh(transform.position, 20f, NavMesh.AllAreas);

                    //좀비의 목적지를 설정
                    agent.SetDestination(patrolTargetPostion);
                }

                //좀비의 시야각, 시야 사거리내 지정된 적이 있는 지를 판별함
                var colliders = Physics.OverlapSphere(eyeTransform.position, viewDistance, whatIsTarget);

                foreach (var collider in colliders)
                {
                    if (!IsTargetOnSight(collider.transform))//지정된 적이 아닌 것은 무시함
                    {
                        continue;
                    }

                    var livingEntity = collider.GetComponent<LivingEntity>();//상대방이 살아있는 생명체(플레이어)인지 확인

                    if (livingEntity != null && !livingEntity.dead)//플레이어가 현재 생존해있는 경우
                    {
                        targetEntity = livingEntity;//타겟을 플레이어로 설정함
                        break;
                    }
                }
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    //LivingEntity의 ApplyDamage를 overide
    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if (!base.ApplyDamage(damageMessage)) return false;//데미지를 받지 않았다면 false

        if (targetEntity == null)//플레이어를 인식하지 못햇을 경우 데미지를 받앗다면
        {
            targetEntity = damageMessage.damager.GetComponent<LivingEntity>();//즉시 플레이어를 타겟으로 설정
        }

        //좀비에게 설정된 이펙트 실행
        EffectManager.Instance.PlayHitEffect(damageMessage.hitPoint, damageMessage.hitNormal, transform,
            EffectManager.EffectType.Flesh);

        audioPlayer.PlayOneShot(hitClip);

        return true;
    }

    public void BeginAttack()
    {
        state = State.AttackBegin;//공격시작인 상태로 변경

        agent.isStopped = true;//좀비의 이동을 멈춤
        animator.SetTrigger("Attack");//공격 animation 실행
    }


    //애니메이션에서 실행되는 코드
    public void EnableAttack()
    {
        state = State.Attacking;

        //최근에 공격한 상대방의 정보를 지움
        lastAttackedTargets.Clear();
    }

    //애니메이션에서 실행되는 코드
    public void DisableAttack()
    {
        if (hasTarget)
            state = State.Tracking;
        else
            state = State.Patrol;

        agent.isStopped = false;
    }

    private bool IsTargetOnSight(Transform target)
    {
        //좀비가 바라보는 방향설정
        var direction = target.position - eyeTransform.position;
        direction.y = eyeTransform.forward.y;

        if (Vector3.Angle(direction, eyeTransform.forward) > fieldOfView * 0.5f)
        {
            return false;
        }

        //초기화
        direction = target.position - eyeTransform.position;

        RaycastHit hit;

        //시야각 내 플레이어가 있으나 벽으로 막혀있어 보이지 않을 경우를 대비한 코드
        if (Physics.Raycast(eyeTransform.position, direction, out hit, viewDistance, whatIsTarget))
        {
            //Raycast에 부딪친 오브젝트가 플레이어일 경우
            //플레이어가 아닐 경우 좀비는 플레이어를 인식하지 못함
            if (hit.transform == target)
            {
                return true;
            }
        }

        return false;
    }

    //좀비가 죽었을 경우 아이템 생성
    public void ItemSpawn()
    {
        //좀비가 죽은 위치 그 자리에서 생성됨
        var SpawnPosition
            = Utility.GetRandomPointOnNavMesh(transform.position, 0, NavMesh.AllAreas);

        //아이템 생성 시 높이 설정
        SpawnPosition += Vector3.up * 0.7f;

        //아이템은 지정된 아이템 배열 내 랜덤으로 나온다.
        var item = Instantiate(items[Random.Range(0, items.Length)], SpawnPosition, Quaternion.identity);
        Destroy(item, 7f);//5초 뒤 자동삭제
    }

    //LivingEntity의 Die를 override
    public override void Die()
    {
        base.Die();

        GetComponent<Collider>().enabled = false;//좀비가 죽으면서 길을 막음을 방지

        agent.enabled = false;//좀비의 navigation 무효화

        animator.applyRootMotion = true;
        animator.SetTrigger("Die");//죽음 animation 실행
        audioPlayer.PlayOneShot(deathClip);//죽는 소리 실행
        ItemSpawn();//좀비가 죽었으므로 아이템 생성
    }
}