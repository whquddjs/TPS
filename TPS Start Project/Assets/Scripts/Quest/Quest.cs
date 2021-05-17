using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : PlayerGoal
{

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
