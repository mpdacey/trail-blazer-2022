using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candle : MonoBehaviour
{
    public bool hasActivated = false;

    public void ActivateFromTorch(GameObject player) { ActivateCandle(player); }

    public void LoadCandle()
    {
        hasActivated = true;
        GetComponentInChildren<ParticleSystem>().Play();
        GetComponentInChildren<Light>().enabled = true;
    }

    public void ResetCandle()
    {
        hasActivated = false;
        GetComponentInChildren<ParticleSystem>().Stop();
        GetComponentInChildren<ParticleSystem>().Clear();
        GetComponentInChildren<Light>().enabled = false;
    }

    void ActivateCandle(GameObject player)
    {
        LoadCandle();
        player.GetComponent<PlayerLife>().ChangeFlame = 8;
        GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayObjectAudioClip(5);
        GameObject.Find("GameMaster").GetComponent<GameMaster>().AddCandle(gameObject);

        if(TryGetComponent<KeyCandle>(out KeyCandle component))
        {
            component.TriggerTorchLock(player);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!hasActivated) ActivateCandle(other.gameObject);
            other.gameObject.GetComponent<PlayerLife>().pauseBurn = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerLife>().pauseBurn = false;
        }
    }
}
