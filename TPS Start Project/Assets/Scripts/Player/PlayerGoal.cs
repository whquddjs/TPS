using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGoal : Goal
{
    [SerializeField]
    GameObject fisrt_goal;
    [SerializeField]
    GameObject second_goal;

    bool FirstComplete = false;
    bool SecondComplete = false;

    int cnt = 0;
    string misson = "기름을 구해\n차량에 주입하세요";

    public override void UpdateGoalScore(int cnt)
    {
        base.UpdateGoalScore(cnt);

        if (goalCur > goalMax)
        {
            goalCur = goalMax;
        }

        UpdateUI();
    }

    public override void UpdateOilScore(int cnt)
    {
        base.UpdateOilScore(cnt);

        if (OilCur > OilMax)
        {
            OilCur = OilMax;
        }

        UpdateUI();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Goal")
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (goalCur == goalMax)
                {
                    goalCur = 0;
                    goalMax += 1;

                    if (FirstComplete == false && SecondComplete == false)
                    {
                        fisrt_goal.SetActive(true);
                        FirstComplete = true;
                    }

                    else if (FirstComplete == true && SecondComplete == false)
                    {
                        second_goal.SetActive(true);
                        SecondComplete = true;
                        OilSpawner.Instance.spawn();
                    }

                    else
                        
                        return;
                    cnt++;

                    UpdateUI();

                }

                if(OilCur == OilMax)
                {
                    UIManager.Instance.UpdateMissonText("축하합니다.");
                    Clear();
                }
                else
                    return;
            }

        }
    }

    public void UpdateUI()
    {
        if (cnt < 2)
            UIManager.Instance.UpdateGoalText(goalCur, goalMax);

        else
        {
            UIManager.Instance.UpdateMissonText(misson);
            UIManager.Instance.UpdateOilText(OilCur, OilMax);
        }

    }

    public void Clear()
    {
        GameManager.Instance.Clear();
        Cursor.visible = true;
    }
}
