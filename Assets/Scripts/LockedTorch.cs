using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedTorch : MonoBehaviour
{
    private int currentStage = -1;
    private GameMaster gameMaster;
    private Candle[] candles;

    private void Start()
    {
        candles = GetComponentsInChildren<Candle>();
        gameMaster = GameObject.Find("GameMaster").GetComponent<GameMaster>();
    }

    public void LightLock(GameObject player)
    {
        currentStage++;

        candles[currentStage].ActivateFromTorch(player);
        GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayObjectAudioClip(currentStage + 7);

        if (currentStage == 0) gameMaster.AddLockedTorch(gameObject);

        if (currentStage == 3) GetComponent<SphereCollider>().enabled = true;
    }

    public void ClearLockedTorchProgress()
    {
        currentStage = -1;
        GetComponent<SphereCollider>().enabled = false;
    }
}
