using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oil : MonoBehaviour, IItem
{
    public int cnt = 1;

    public void Use(GameObject target)
    {
        var Goal = target.GetComponent<PlayerGoal>();

        if (Goal != null)
        {
            Goal.UpdateOilScore(cnt);
        }

        Destroy(gameObject);
    }
}
