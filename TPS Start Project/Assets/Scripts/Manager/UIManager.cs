using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    private static UIManager instance;
    
    public static UIManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<UIManager>();

            return instance;
        }
    }

    [SerializeField] private GameObject gameoverUI;//게임오버UI
    [SerializeField] private GameObject gameClearUI;//게임승리UI
    [SerializeField] private Crosshair crosshair;//십자선

    [SerializeField] private Text healthText;//체력
    [SerializeField] private Text ammoText;//총알
    [SerializeField] private Text OilText;//기름
    [SerializeField] private Text goalText;//목표치
    [SerializeField] private Text MissonText;//미션

    public void UpdateMissonText(string misson)
    {
        MissonText.text = misson;//현재 미션의 Text
    }

    public void UpdateOilText(int OilCur, int OilMax)
    {
        OilText.text = OilCur + "/" + OilMax;//현재 오일 갯수와 필요한 오일 갯수
    }

    public void UpdateAmmoText(int magAmmo, int remainAmmo)
    {
        ammoText.text = magAmmo + "/" + remainAmmo;//현재 총알 갯수와 전체 총알 갯수
    } 

    public void UpdateGoalText(int goalCur, int goalMax)
    {
        goalText.text = goalCur + "/" + goalMax;// 현재 탈출재료 갯수와 필요한 탈출 재료 갯수
    }    

    public void UpdateCrossHairPosition(Vector3 worldPosition)
    {
        crosshair.UpdatePosition(worldPosition);//십자선의 위치 이동
    }
    
    public void UpdateHealthText(float health)
    {
        healthText.text = Mathf.Floor(health).ToString();//체력수치를 지속적으로 표현
    }

    public void SetActiveCrosshair(bool active)
    {
        crosshair.SetActiveCrosshair(active);//플레이어가 총을 쏘기에 충분한 거리가 유지되어있지 않을 경우 십자선 표시x
    }
    
    public void SetActiveGameoverUI(bool active)
    {
        gameoverUI.SetActive(active);//사망하였을 경우
    }

    public void SetActiveGameClearUI(bool active)
    {
        gameClearUI.SetActive(active);//게임에 승리하였을 경우
    }

    public void GameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}