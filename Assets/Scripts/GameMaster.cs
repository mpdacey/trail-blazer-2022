using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    private const string TORCH_KEY = "TrailBlazerTorch";
    private const string SPAWN_KEY = "TrailBlazerSpawn";
    private const string MUSIC_KEY = "TrailBlazerMusic";
    private const string SOUND_KEY = "TrailBlazerSound";
    private const string TIMED_KEY = "TrailBlazerTime";
    private const string AWARD_KEY = "TrailBlazerReward";
    public GameObject torchesParent;
    public GameObject titleScreen;
    public GameObject pauseScreen;
    public GameObject resetButton;
    public GameObject countdownScreen;
    public GameObject player;
    public GameObject emeraldTime;
    public GameObject resetGraffiti;
    public GameObject mobileControls;
    public Image resetOverlay;
    public SunSetting sun;
    public AudioManager audioManager;
    public FinalTorch finalTorch;
    public Material[] torchMaterials;
    public int CurrentActiveTorches
    {
        get { return activatedTorches; }
        set { activatedTorches++; }
    }

    public bool SetPause
    {
        set { canPause = value; }
    }

    private int activatedTorches = 0;
    private List<Candle> unbankedCandles = new List<Candle>();
    private List<Candle> bankedCandles = new List<Candle>();
    private List<LockedTorch> lockedTorches = new List<LockedTorch>();
    private PlayerMovement movement;
    private PlayerLife life;
    private TimeTrialMaster trialMaster;
    private Vector3 start;
    private bool isPaused = false;
    private bool isTimedMode = false;
    private bool isResetting = false;
    private bool canPause = true;
    private float resetTimer = 0;

    public bool SetTimed { set { isTimedMode = value; } }

    // Start is called before the first frame update
    void Start()
    {
        movement = player.GetComponent<PlayerMovement>();
        life = player.GetComponent<PlayerLife>();
        trialMaster = GetComponent<TimeTrialMaster>();
        start = torchesParent.transform.GetChild(0).GetChild(2).position;

        movement.SetControl = false;

        if (PlayerPrefs.HasKey(TORCH_KEY))
            titleScreen.GetComponentsInChildren<Button>()[0].interactable = true;

        if (PlayerPrefs.HasKey(MUSIC_KEY))
            audioManager.MusicVolume = PlayerPrefs.GetFloat(MUSIC_KEY);
        else
            PlayerPrefs.SetFloat(MUSIC_KEY, 1);

        if (PlayerPrefs.HasKey(SOUND_KEY))
            audioManager.EffectVolumeNoSFX(PlayerPrefs.GetFloat(SOUND_KEY));
        else
            PlayerPrefs.SetFloat(SOUND_KEY, 1);

        if (!PlayerPrefs.HasKey(AWARD_KEY))
            PlayerPrefs.SetInt(AWARD_KEY, 0);

        trialMaster.unlockedTorches = ConvertIntToBoolArray(PlayerPrefs.GetInt(AWARD_KEY), 4);
        emeraldTime.SetActive(trialMaster.unlockedTorches[2]);

        titleScreen.GetComponentsInChildren<Button>()[0].interactable = PlayerPrefs.HasKey(TORCH_KEY);

        SetUIVolumeSliders(titleScreen);
    }

    void Update()
    {
        if (Input.GetButtonDown("Pause") && canPause)
        {
            TriggerPause();
        }

        if (Input.GetButtonDown("ResetTimeTrial"))
        {
            resetOverlay.gameObject.SetActive(true);
            isResetting = true;
        }
        else if (Input.GetButtonUp("ResetTimeTrial")) isResetting = false;

        if (isResetting && isTimedMode)
        {
            resetTimer += Time.deltaTime;
            resetOverlay.color = new Color(1, 1, 1, resetTimer/ 1.2f);

            if (resetTimer > 1.2f)
            {
                isResetting = false;
                ResetTimedGame();
            }
        }
        else if(resetTimer > 0)
        {
            resetTimer = Mathf.Clamp01(resetTimer - Time.deltaTime * 2);
            resetOverlay.color = new Color(1, 1, 1, resetTimer / 1.2f);

            if(Mathf.Approximately(resetTimer, 0))
            {
                resetOverlay.gameObject.SetActive(false);
            }
        }
    }

    public void StartNewGame()
    {
        PlayerPrefs.DeleteKey(TORCH_KEY);
        PlayerPrefs.SetInt(SPAWN_KEY, 0);

        StartCoroutine(DismissTitle());
        movement.SetControl = true;
        resetGraffiti.SetActive(false);
    }

    public void StartContinuedGame()
    {
        SetupContinuedGame();
        StartCoroutine(DismissTitle());
        movement.SetControl = true;
        resetGraffiti.SetActive(false);
    }

    public void StartTimedGame()
    {
        resetButton.SetActive(true);
        StartCoroutine(DismissTitle());
        StartCoroutine(FirstCountdownAnimation());
        resetGraffiti.SetActive(true);
    }

    public void ResetTimedGame()
    {
        player.GetComponent<PlayerLife>().StopAllCoroutines();
        ResetToTitleStatus();
        ClearAllProgress();

        StartCoroutine(CountdownAnimation());
    }

    public void SetTorchMaterials(int materialIndex)
    {
        Debug.Log(materialIndex);

        for(int i = 1; i<torchesParent.transform.childCount; i++)
        {
            Transform torch = torchesParent.transform.GetChild(i);
            torch.GetComponent<MeshRenderer>().material = torchMaterials[materialIndex];
        }

        Transform finalTorchObject = finalTorch.transform;

        finalTorch.GetComponent<MeshRenderer>().material = torchMaterials[materialIndex];

        for (int i = 1; i < finalTorchObject.childCount; i++)
        {
            Transform torch = finalTorchObject.GetChild(i);
            torch.GetComponent<MeshRenderer>().material = torchMaterials[materialIndex];
        }
    }
    IEnumerator FirstCountdownAnimation()
    {
        SetPause = false;
        countdownScreen.SetActive(true);
        countdownScreen.transform.GetChild(0).GetComponent<Image>().color = Color.white;
        yield return new WaitForSeconds(2.82f);
        StartCoroutine(CountdownAnimation());
    }

    IEnumerator CountdownAnimation()
    {
        movement.SetControl = false;
        countdownScreen.SetActive(true);
        trialMaster.ResetTimer();

        Animation countdownAnimation = countdownScreen.GetComponent<Animation>();
        countdownAnimation.Stop();
        countdownAnimation.Play();
        SetPause = false;

        audioManager.PlayObjectAudioClip(15);
        yield return new WaitForSeconds(0.75f);
        audioManager.PlayObjectAudioClip(16);
        yield return new WaitForSeconds(0.75f);
        audioManager.PlayObjectAudioClip(17);
        yield return new WaitForSeconds(0.75f);
        audioManager.PlayObjectAudioClip(18);

        SetTimed = true;
        movement.SetControl = true;
        trialMaster.SetTimerActive = true;
        while (countdownAnimation.isPlaying)
        {
            yield return null;
        }

        SetPause = true;
        countdownScreen.SetActive(false);
    }

    public void SaveCheckpoint(int siblingIndex)
    {
        if (isTimedMode)
        {
            trialMaster.MakeSplit();
        }
        else
        {
            int currentTorch = PlayerPrefs.GetInt(TORCH_KEY);
            PlayerPrefs.SetInt(TORCH_KEY, Mathf.Clamp(currentTorch + (int)Mathf.Pow(2, siblingIndex - 1), 0, 127));
            PlayerPrefs.SetInt(SPAWN_KEY, Mathf.Clamp(siblingIndex, 0, 7));
        }
    }

    public void AddCandle(GameObject candle)
    {
        unbankedCandles.Add(candle.GetComponent<Candle>());
    }

    public void AddLockedTorch(GameObject torch)
    {
        lockedTorches.Add(torch.GetComponent<LockedTorch>());
    }

    public void TriggerPause()
    {
        if (!titleScreen.activeSelf)
        {
            isPaused = !isPaused;

            mobileControls.SetActive(!isPaused);

            pauseScreen.SetActive(isPaused);
            if (isPaused) SetUIVolumeSliders(pauseScreen);

            Time.timeScale = isPaused ? 0 : 1;
        }
    }

    public void ReturnToTitle()
    {
        //General Reset.
        audioManager.StartTitleMusic();
        titleScreen.SetActive(true);
        SetUIVolumeSliders(titleScreen);
        ResetToTitleStatus();
        resetButton.SetActive(false);
        mobileControls.SetActive(false);

        titleScreen.GetComponentsInChildren<Button>()[0].interactable = PlayerPrefs.HasKey(TORCH_KEY);
        emeraldTime.SetActive(trialMaster.unlockedTorches[2]);
        trialMaster.SetTorchPanel();

        Animation dismissedAnimation = titleScreen.GetComponent<Animation>();
        dismissedAnimation["SubmitTitle"].speed = -1;
        dismissedAnimation["SubmitTitle"].time = dismissedAnimation["SubmitTitle"].length;

        dismissedAnimation.Play();

        ClearAllProgress();
    }

    public void BankCandles()
    {
        bankedCandles.AddRange(unbankedCandles);

        unbankedCandles.Clear();
        lockedTorches.Clear();
    }

    public void ClearCurrentCandles()
    {
        unbankedCandles.ForEach(delegate (Candle candle)
        {
            candle.ResetCandle();
        });

        lockedTorches.ForEach(delegate (LockedTorch torch)
        {
            torch.ClearLockedTorchProgress();
        });

        unbankedCandles.Clear();
        lockedTorches.Clear();
    }

    public void SetVolumePref(float value, bool isMusic)
    {
        if (isMusic)
            PlayerPrefs.SetFloat(MUSIC_KEY, value);
        else
            PlayerPrefs.SetFloat(SOUND_KEY, value);
    }

    public float[] GetTimeSplits(int length)
    {
        float[] splits = new float[length];

        for(int i = 0; i < length; i++)
        {
            splits[i] = PlayerPrefs.GetFloat(string.Format("{0}{1}", TIMED_KEY, i), -1);
        }

        return splits;
    }

    public void SetTimeSplits(float[] splits)
    {
        int index = 0;

        foreach(float split in splits)
        {
            PlayerPrefs.SetFloat(string.Format("{0}{1}", TIMED_KEY, index), split);
            index++;
        }
    }

    public void UnlockReward(int tier)
    {
        int current = PlayerPrefs.GetInt(AWARD_KEY);

        current += (int)Mathf.Pow(2, tier);

        PlayerPrefs.SetInt(AWARD_KEY, current);
    }

    private void ResetToTitleStatus()
    {
        //General Reset.
        sun.ResetDistance();
        SetTimed = false;

        //Time Trial Mode Reset
        trialMaster.SetTimerActive = false;
        movement.SetControl = false;
        StopAllCoroutines();
        countdownScreen.GetComponent<Animation>().Stop();
        countdownScreen.SetActive(false);
    }

    private void SetUIVolumeSliders(GameObject screen)
    {
        Slider[] sliders = screen.GetComponentsInChildren<Slider>();

        sliders[0].value = PlayerPrefs.GetFloat(SOUND_KEY);
        sliders[1].value = PlayerPrefs.GetFloat(MUSIC_KEY);
    }

    private void ClearAllProgress()
    {
        player.GetComponent<PlayerLife>().RespawnFlame(start);
        player.GetComponentInChildren<ParticleSystem>().Clear();

        for(int i = 1; i < torchesParent.transform.childCount; i++)
        {
            GameObject torch = torchesParent.transform.GetChild(i).gameObject;
            Torch torchComponent = torch.GetComponent<Torch>();
            torchComponent.ResetTorch();
        }

        bankedCandles.ForEach(delegate (Candle candle)
        {
            candle.ResetCandle();
        });
        bankedCandles.Clear();

        trialMaster.ResetTimer();

        ClearCurrentCandles();
        finalTorch.ResetFinalTorch();
    }

    IEnumerator DismissTitle()
    {
        Animation dismissedAnimation = titleScreen.GetComponent<Animation>();
        foreach (AnimationState state in dismissedAnimation)
        {
            state.speed = 1;
        }

        dismissedAnimation.Play();

        audioManager.SetOnTitle = false;

        while (dismissedAnimation.isPlaying)
        {
            yield return null;
        }

        titleScreen.SetActive(false);
        mobileControls.SetActive(true);
    }

    // Conversion Solution Reference: https://stackoverflow.com/a/6758238
    private bool[] ConvertIntToBoolArray(int input, int arraySize)
    {
        BitArray bitArray = new BitArray(new int[] { input });
        bool[] bools = new bool[bitArray.Count];
        bitArray.CopyTo(bools, 0);

        Array.Resize(ref bools, arraySize);

        return bools;
    }

    private void SetupContinuedGame()
    {
        int activeTorchesInt = PlayerPrefs.GetInt(TORCH_KEY);

        bool[] activeTorchesBools = ConvertIntToBoolArray(activeTorchesInt, torchesParent.transform.childCount - 1);

        for (int i = 0; i < activeTorchesBools.Length; i++)
        {
            if (activeTorchesBools[i])
            {
                Transform selectedTorch = torchesParent.transform.GetChild(i + 1);

                selectedTorch.GetComponent<Torch>().LoadTorch();
            }
        }

        int spawnPoint = PlayerPrefs.GetInt(SPAWN_KEY);
        Transform spawningTorch = torchesParent.transform.GetChild(spawnPoint);
        spawningTorch.GetComponent<Torch>().SetRespawnPoint();
        player.transform.position = player.GetComponent<PlayerLife>().RespawnPoint;
    }
}