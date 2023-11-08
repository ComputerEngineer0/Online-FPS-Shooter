using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints;

    public static SpawnManager instance;

    private void Awake()
    {

        instance = this;

    }

    void Start()
    {

        foreach(Transform spawn in spawnPoints)
        {

            // Spawn bolgelerine yerlestirdigimiz nesneleri gormek istemiyoruz. Oyun baslarken onlar aktif olmayacak.
            spawn.gameObject.SetActive(false); 
        }


        
    }

    
    void Update()
    {
        
    }

    public Transform GetSpawnPoint()
    {

        return spawnPoints[Random.Range(0, spawnPoints.Length - 1)];

    }

}
