using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{

    public static PlayerSpawner instance;

    private GameObject player;

    public GameObject deathEffect;

    public float respawnTime = 5f;

    private void Awake()
    {
        instance = this;
    }

    public GameObject playerPrefab;

    private void Start()
    {

        if(PhotonNetwork.IsConnected)
        {

          SpawnPlayer();

        }

        
    }

    public void SpawnPlayer()
    {

        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);

    }
   
    public void Die(string damager)
    {

        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);

        UIController.instance.deathText.text = "You were killed by " + damager;

        //PhotonNetwork.Destroy(player);

        //SpawnPlayer();

        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber,1,1);

        if(player != null)
        {

            StartCoroutine(DieCo());


        }

    }
    
    public IEnumerator DieCo() //Co = Coroutine
    {

        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(player);

        player = null;

        UIController.instance.deathScreen.SetActive(true);

        yield return new WaitForSeconds(respawnTime);

        UIController.instance.deathScreen.SetActive(false);
         
        if(MatchManager.instance.state == MatchManager.GameState.Playing && player == null)
        { 
        SpawnPlayer();
        }


    } 


}
