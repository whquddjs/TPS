using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilSpawner : MonoBehaviour
{
    private readonly List<Oil> oils = new List<Oil>();
    private static OilSpawner instance;

    public static OilSpawner Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<OilSpawner>();

            return instance;
        }
    }


    public Transform[] spawnPoints;
    public Oil Oil;

    public void spawn()
    {
        int spawnCount = 3;

        for (var i = 0; i < spawnCount; i++)
        {
            create();
        }
    }

    public void create()
    {
        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        var oil = Instantiate(Oil, spawnPoint.position, spawnPoint.rotation);

        oils.Add(oil);
    }


}
