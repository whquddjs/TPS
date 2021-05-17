using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public int goalCur { get; protected set; }
    public int goalMax = 1;

    public int OilCur { get; protected set; }
    public int OilMax = 1;

    public virtual void UpdateGoalScore(int cnt)
    {
        goalCur += cnt;
    }

    public virtual void UpdateOilScore(int cnt)
    {
        OilCur += cnt;
    }

}
