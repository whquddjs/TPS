using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : PlayerGoal
{
    /*
     * 차량 앞에서 E키를 누르게 될경우 코드
     * 현재 목표치와 필요 목표치가 같을 경우 
     * 현재 목표치를 0으로 바꾸고 필요 목표치량을 늘린다
     */
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (goalCur == goalMax)
                {
                    goalCur = 0;
                    goalMax += 2;

                    UpdateUI();
                }

                else
                {
                    return;
                }
            }

        }
    }
}
