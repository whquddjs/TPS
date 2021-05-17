using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();
            
            return instance;
        }
    }

    public bool isGameover { get; private set; }

    private void Awake()
    {
        if (Instance != this) Destroy(gameObject);
    }
    
    //게임 패배시 GameOverUI Active(true)
    public void EndGame()
    {
        isGameover = true;
        UIManager.Instance.SetActiveGameoverUI(true);
    }

    //게임 승리시 GameClearUI Active(true)
    public void Clear()
    {
        isGameover = true;
        UIManager.Instance.SetActiveGameClearUI(true);
    }
}