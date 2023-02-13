using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MonoBehaviour
{
    public bool lit = false;
    public GameObject finalTorch;
    private ParticleSystem particles;
    private GameObject player;
    private GameMaster gameMaster;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(!lit)ActivateTorch();

            player.GetComponent<PlayerLife>().ChangeFlame = 40;
            player.GetComponent<PlayerLife>().pauseBurn = true;
            player.GetComponentInChildren<ParticleSystem>().Stop();
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            player.GetComponentInChildren<ParticleSystem>().Play();
            player.GetComponent<PlayerLife>().pauseBurn = false;
        }
    }

    // Used when loading save data
    public void LoadTorch()
    {
        LightTorch();
        if(transform.childCount > 3)
        {
            GetComponent<SphereCollider>().enabled = true;
            Candle[] candles = GetComponentsInChildren<Candle>();

            foreach(Candle candle in candles) candle.LoadCandle();
        }
    }

    public void SetRespawnPoint()
    {
        player.GetComponent<PlayerLife>().RespawnPoint = transform.position + Vector3.up * 0.5f;
    }

    public void ResetTorch()
    {
        if (TryGetComponent(out LockedTorch component))
            component.ClearLockedTorchProgress();

        DeactivateTorch();
    }

    private void LightTorch()
    {
        particles.Play();
        GetComponentInChildren<Light>().enabled = true;
        gameMaster.CurrentActiveTorches++;
        if (finalTorch) finalTorch.GetComponent<FinalTorch>().LightLock();
        lit = true;
    }

    // Used when player enters torch.
    private void ActivateTorch()
    {
        LightTorch();
        SetRespawnPoint();
        gameMaster.BankCandles();
        gameMaster.SaveCheckpoint(transform.GetSiblingIndex());
        GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayObjectAudioClip(6);
    }

    private void DeactivateTorch()
    {
        particles.Stop();
        GetComponentInChildren<Light>().enabled = false;
        lit = false;
    }

    public void FinalleActivateTorch()
    {
        particles.Play();
        GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayObjectAudioClip(6);
        GetComponentInChildren<Light>().enabled = true;
        lit = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        particles = gameObject.GetComponentInChildren<ParticleSystem>();
        player = GameObject.FindWithTag("Player");
        gameMaster = GameObject.Find("GameMaster").GetComponent<GameMaster>();
    }
}
