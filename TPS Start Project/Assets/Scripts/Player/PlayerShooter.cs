using UnityEngine;

// 주어진 Gun 오브젝트를 쏘거나 재장전
// 알맞은 애니메이션을 재생하고 IK를 사용해 캐릭터 양손이 총에 위치하도록 조정
public class PlayerShooter : MonoBehaviour
{
    public enum AimState
    {
        Idle,//가만히 있음
        HipFire//총 쏠 준비가 되었음
    }

    public AimState aimState { get; private set; }

    public Gun gun; // 사용할 총
    public Goal goal;//목표치
    public GameObject GunPoint;//플레이어가 총을 잡는 위치(방아쇠 위치)
    public LayerMask excludeTarget;//혹여나 총알에 플레이어가 맞는 것을 방지하기 위해 excludeTarget을 플레이어로 설정

    private PlayerInput playerInput;
    private Animator playerAnimator; // 애니메이터 컴포넌트
    private Camera playerCamera;

    //TPS게임 특성상 메인카메라가 바라보는 곳과 캐릭터가 바라보는 곳이 서로 다르기 때문에
    //"실제로" 총알이 맞아야 할 곳을 따로 저장해둠
    private Vector3 aimPoint;

    //플레이어가 바라보는 방향과 카메라가 바라보는 방향 사이의 각도를 계산
    //서로간의 각도가 너무 벌어진 채로 총을 쏘게 되면 움직임이 이상하기 때문에 총을 쏘기 전에 카메라가 바라보는 방향으로 돌아서게 하는 작업을 함
    private bool linedUp => !(Mathf.Abs(playerCamera.transform.eulerAngles.y - transform.eulerAngles.y) > 1f);

    //gun.fireTransform.position = 총구의 위치
    //총구의 위치부터 방아쇠 위치 사이까지 가상의 선을 연결하여 선이 끊어지지 않을 경우 true을 준다.
    private bool hasEnoughDistance => !Physics.Linecast(GunPoint.transform.position, gun.fireTransform.position, ~excludeTarget);

    //만일 excludeTarget을 설정하지 않을 경우 gameObject의 layer로 excludeTarget을 설정해줌
    void Awake()
    {
        if (excludeTarget != (excludeTarget | (1 << gameObject.layer)))
        {
            excludeTarget |= 1 << gameObject.layer;
        }
    }

    private void Start()
    {
        playerCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
    }

    //playerShooter가 활성화 될 때마다 gun의 게임오브젝트를 활성화하며
    //Setup()메소드의 자기자신을 넣어 실행
    private void OnEnable()
    {
        gun.gameObject.SetActive(true);
        gun.Setup(this);
    }

    //playerShooter가 비활성화 될 때마다 gun의 게임오브젝트를 비활성화
    private void OnDisable()
    {
        gun.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        //플레이어가 좌클릭을 할 경우
        if (playerInput.fire)
        {
            Shoot();
        }
        //플레이어가 R키를 누를 경우
        else if (playerInput.reload)
        {
            Reload();
        }
    }

    private void Update()
    {
        UpdateAimTarget();

        var angle = playerCamera.transform.eulerAngles.x;//플레이어의 시선
        if (angle > 270f) angle -= 360f;

        angle = angle / 180f * -1f + 0.5f;

        playerAnimator.SetFloat("Angle", angle);//angle의 수치만큼 플레이어의 시선을 이동

        UpdateUI();
    }

    public void Shoot()
    {
        if (aimState == AimState.Idle)//가만히 있는 경우
        {
            if (linedUp) //카메라와 플레이어가 바라보는 각도의 차이가 1f을 넘지 않을 경우 
                aimState = AimState.HipFire;//바로 총을 쏠 준비를 함
        }
        else if (aimState == AimState.HipFire)//총을 쏠 준비가 되었다면
        {
            if (hasEnoughDistance)//거리가 확보가 되었으면
            {
                if (gun.Fire(aimPoint))//aimPoint로 총을 쏠 경우
                    playerAnimator.SetTrigger("Shoot");//Shoot animation 실행
            }
            else//거리 확보가 안되어있으면
            {
                aimState = AimState.Idle;//Idle상태로 변경
            }
        }
    }

    public void Reload()
    {
        // 재장전 입력 감지시 재장전
        if (gun.Reload()) playerAnimator.SetTrigger("Reload");
    }

    //메인카메라가 1차적으로 aimPoint를 설정
    //실제 플레이어가 총을 쏠 때 그 위치에 총알이 닿는 지를 파악
    //실제로 총알이 맞는 위치를 최종적인 aimPoint로 설정
    private void UpdateAimTarget()
    {
        RaycastHit hit;

        //화면상의 정중앙의 위치에 ViewPoint로 Ray를 보냄
        var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));

        //gun.fireDistance = 사정거리
        if (Physics.Raycast(ray, out hit, gun.fireDistance, ~excludeTarget))
        {
            aimPoint = hit.point;

            //플레이어가 실제로 총을 쏠때 어떠한 물체가 끼워드려있는 경우
            if (Physics.Linecast(gun.fireTransform.position, hit.point, out hit, ~excludeTarget))
            {
                aimPoint = hit.point;
            }
        }
        //사정거리까지 감지되는 것이 없을 경우
        else
        {
            //카메라가 바라보는 방향으로 총을 쏨
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * gun.fireDistance;
        }
    }

    // 탄약 UI 갱신
    private void UpdateUI()
    {
        if (gun == null || UIManager.Instance == null) return;

        // UI 매니저의 탄약 텍스트에 탄창의 탄약과 남은 전체 탄약을 표시
        UIManager.Instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain);

        UIManager.Instance.SetActiveCrosshair(hasEnoughDistance);
        UIManager.Instance.UpdateCrossHairPosition(aimPoint);
    }

    // 애니메이터의 IK 갱신
    private void OnAnimatorIK(int layerIndex)
    {
        if (gun == null || gun.state == Gun.State.Reloading) return;

        // IK를 사용하여 왼손의 위치와 회전을 총의 오른쪽 손잡이에 맞춘다
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand,
            gun.leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand,
            gun.leftHandMount.rotation);
    }
}