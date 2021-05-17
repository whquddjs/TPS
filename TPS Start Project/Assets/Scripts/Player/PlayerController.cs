using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    public AudioClip itemPickupClip;
    public int lifeRemains = 0;//죽는 즉시 게임이 종료 될 수 있도록 남은 목숨을 0으로 지정
    private AudioSource playerAudioPlayer;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerShooter playerShooter;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAudioPlayer = GetComponent<AudioSource>();

        playerHealth.OnDeath += HandleDeath;

        Cursor.visible = false;
    }
    
    private void HandleDeath()//플레이어가 죽었을 경우
    {
        //플레이어는 움직이거나 총을 쏠 수가 없다
        playerMovement.enabled = false;
        playerShooter.enabled = false;

        if(lifeRemains <= 0)//사망하여 0이하가 될 경우
        {
            GameManager.Instance.EndGame();//GameManager의 EndGame 실행(게임 종료 UI실행)
        }

        Cursor.visible = true;//마우스 커서가 보이며 재시작 버튼을 누를수 있다.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerHealth.dead)//캐릭터가 죽었을 경우
            return;//메소드 종료

        var item = other.GetComponent<IItem>();//드랍된 아이템을 먹기 위해서

        if(item != null)
        {
            item.Use(gameObject);//아이템의 Use()를 사용
            playerAudioPlayer.PlayOneShot(itemPickupClip);//아이템 먹는 오디오 재생
        }
    }
}