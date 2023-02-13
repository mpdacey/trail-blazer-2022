using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    public bool pauseBurn = false;
    public bool died = false;
    private List<Coroutine> currentCoroutines = new List<Coroutine>();
    public int ChangeFlame
    {
        set
        {
            remainingFlame = Mathf.Clamp(remainingFlame + value, 0, 40);
            UpdateFlame();
        }
    }
    private int remainingFlame = 40;
    private Vector3 lastRespawn;
    public Vector3 RespawnPoint
    {
        set
        {
            lastRespawn = value;
        }
        get
        {
            return lastRespawn;
        }
    }
    private Transform flame;
    private ParticleSystem flameParticles;
    private GameMaster gameMaster;
    private AudioManager audioManager;
    private CinemachineBrain cinemachine;

    // Start is called before the first frame update
    void Start()
    {
        flame = transform.GetChild(0);
        flameParticles = GetComponentInChildren<ParticleSystem>();
        gameMaster = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        cinemachine = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineBrain>();
        lastRespawn = transform.position;
        currentCoroutines.Add(StartCoroutine("Burn"));
    }

    private void UpdateFlame()
    {
        //flame.localScale = Vector3.one + Vector3.one * ((float)remainingFlame / 40 * 3);

        ParticleSystem.MainModule main = flameParticles.main;
        main.startSizeMultiplier = 0.05f + 0.23f * ((float)remainingFlame / 40);
        ParticleSystem.ShapeModule shape = flameParticles.shape;
        shape.radius = 0.02f + 0.03f * ((float)remainingFlame / 40);
    }

    public void RespawnFlame(Vector3 resetPoint)
    {
        transform.position = resetPoint;
        GetComponent<SphereCollider>().enabled = true;
        flame.GetComponent<ParticleSystem>().Play();
        flame.GetComponent<Light>().enabled = true;
        cinemachine.enabled = true;
        ChangeFlame = 60;
        died = false;
        currentCoroutines.Add(StartCoroutine(Burn()));
    }

    IEnumerator Burnout()
    {
        died = true;
        GetComponent<PlayerMovement>().SetControl = false;
        GetComponent<SphereCollider>().enabled = false;
        cinemachine.enabled = false;
        gameMaster.ClearCurrentCandles();

        foreach (Coroutine routine in currentCoroutines)
            StopCoroutine(routine);
        currentCoroutines.Clear();

        yield return new WaitForSeconds(2);

        RespawnFlame(RespawnPoint);
        GetComponent<PlayerMovement>().SetControl = true;
        audioManager.PlayPlayerAudioClip(4);
    }

    IEnumerator Burn()
    {
        while (true)
        {
            while (remainingFlame > 0)
            {
                if (!pauseBurn)
                {
                    UpdateFlame();
                    ChangeFlame = -1;
                }

                yield return new WaitForSeconds(0.33f);
            }

            //Warn of impending doom
            for (int i = 0; i< 5; i++)
            {
                if(remainingFlame <= 0)
                {
                    flame.GetComponent<ParticleSystem>().Play();
                    yield return new WaitForSeconds(0.5f);
                    audioManager.PlayPlayerAudioClip(1);
                    flame.GetComponent<ParticleSystem>().Stop();
                    yield return new WaitForSeconds(0.1f * (i + 1));
                }
                else
                {
                    flame.GetComponent<ParticleSystem>().Play();
                    break;
                }
            }

            //Die
            if (remainingFlame <= 0)
            {
                StartCoroutine("Burnout");
                flame.GetComponent<ParticleSystem>().Stop();
                audioManager.PlayPlayerAudioClip(3);
                yield return new WaitForSeconds(2);
            }
        }
    }
}
