using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class PrepareDungeon : RoomFirstDungeonGenerator
{
    public GameObject Boss;
    public GameObject Player;
    private string[] Bosses = {"Boss1", "Boss2"};

    public void PlayerToStart(Vector2Int roomCenter)
    {  
        Player = GameObject.FindWithTag("Player");
        Player.transform.position = new Vector2(roomCenter.x, roomCenter.y);
    }

    // public void SpawnEnteties()
    // {

    // }

    // public void SpawnEnemies()
    // {

    // }

    public void SpawnBoss()
    {
         string Boss = Bosses[Random.Range(0, Bosses.Length)];
         //Debug.Log(Boss);
    }
        
    // public void FillBossRoom()
    // {

    // }

    // public void FillMerchentRoom()
    // {

    // }

}