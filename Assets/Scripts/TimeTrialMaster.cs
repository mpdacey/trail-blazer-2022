using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeTrialMaster : MonoBehaviour
{
    //Inspector visable multidimentional arrays: http://answers.unity.com/comments/816654/view.html
    [System.Serializable]
    public class TrophySplits
    {
        public float[] splits;
    }

    public GameObject timerPanel;
    public GameObject rewardPanel;
    public GameObject recordPanel;
    public Transform torchPanel;
    public TrophySplits[] trophySplits;
    public bool[] unlockedTorches;
    private Animator recordPanelAnimator;
    private Animator rewardPanelAnimator;
    private GameMaster master;
    private bool timerOn = false;
    private bool showTimer = true;
    private bool isDisplayingReward = false;
    private float timerValue = 0;
    private Text screenText;
    private Text splitText;
    private Dictionary<string, Color> colors;
    private float[] recordedSplits = new float[8];
    private float[] currentSplits = new float[8];
    private int currentSplitIndex = 0;
    private int timeGoal = 0;

    public bool SetTimerActive
    {
        set
        {
            timerPanel.SetActive(value);
            timerOn = value;
            screenText.enabled = value;
            splitText.enabled = false;
            recordedSplits = SetTimeSplits();
        }
    }

    public void DismissRecordPanel()
    {
        recordPanelAnimator.SetBool("isDisplayed", false);
        StartCoroutine(WaitForAnimation());
    }

    public void DismissRewardPanel()
    {
        rewardPanelAnimator.SetBool("isDisplayed", false);
        StartCoroutine(WaitForAnimation());
    }

    IEnumerator WaitForAnimation()
    {
        yield return new WaitForSeconds(20f / 60);

        isDisplayingReward = false;
    }

    private float[] SetTimeSplits()
    {
        float[] splits;

        if(timeGoal == 0)
        {
            splits = master.GetTimeSplits(recordedSplits.Length);
        }
        else
        {
            splits = trophySplits[timeGoal-1].splits;
        }

        return splits;
    }
    

    public void ResetTimer()
    {
        timerOn = false;
        showTimer = true;
        timerValue = 0;
        currentSplitIndex = 0;
        currentSplits.Initialize();
    }

    public float TimerValue
    {
        get { return timerValue; }
    }

    public int TimeGoal
    {
        set
        {
            timeGoal = Mathf.Clamp(value, 0, 4);
            Debug.Log("timeGoal: " + timeGoal);
        }
    }

    public void MakeSplit()
    {
        if(currentSplitIndex < currentSplits.Length)
        {
            currentSplits[currentSplitIndex] = timerValue;
            currentSplitIndex++;
            StartCoroutine(HoldSplit());
        }
    }

    IEnumerator HoldSplit()
    {
        float currentSplitValue = currentSplits[currentSplitIndex - 1];
        float recordedSplitValue = recordedSplits[currentSplitIndex - 1];
        Shadow shadow = splitText.gameObject.GetComponent<Shadow>();

        showTimer = false;
        screenText.text = FormatTime(currentSplitValue);
        if (recordedSplitValue != -1)
        {
            float splitValue = Mathf.Clamp(currentSplitValue - recordedSplitValue, -599.999f, 599.999f);
            string splitString = FormatTime(Mathf.Abs(splitValue)).Remove(0, 1);

            if(splitValue > 0)
            {
                splitText.text = '+' + splitString;
                splitText.color = colors["pos"];
                shadow.effectColor = colors["posShadow"];
            }
            else
            {
                splitText.text = '-' + splitString;
                splitText.color = colors["neg"];
                shadow.effectColor = colors["negShadow"];
            }

            splitText.enabled = true;
        }

        yield return new WaitForSeconds(2);
        splitText.enabled = false;

        Debug.Log(string.Format("currentSplitValue: {0}, recordedSplitValue: {1}", currentSplitValue, recordedSplitValue));

        if (currentSplitIndex != currentSplits.Length) showTimer = true;
        else
        {
            if (currentSplitValue < recordedSplitValue || recordedSplitValue == -1)
            {
                master.SetPause = false;
                master.SetTimeSplits(currentSplits);
                master.audioManager.PlayPlayerAudioClip(19);
                recordPanel.SetActive(true);
                recordPanelAnimator.SetBool("isDisplayed", true);
                yield return new WaitForSeconds(1.5f);
                recordPanelAnimator.SetBool("isDisplayed", false);
                yield return new WaitForSeconds(1);
                recordPanel.SetActive(false);
                master.SetPause = true;
            }

            for (int i = 0; i < trophySplits.Length; i++)
            {
                if (currentSplitValue < trophySplits[i].splits[7] && !unlockedTorches[i])
                {
                    unlockedTorches[i] = true;
                    master.UnlockReward(i);
                    master.SetPause = false;
                    rewardPanel.SetActive(true);
                    rewardPanel.GetComponentInChildren<SwitchUISprite>().SetSprite(i);
                    rewardPanelAnimator.SetBool("isDisplayed", true);
                    master.audioManager.PlayPlayerAudioClip(7 + i);
                    isDisplayingReward = true;
                    while (isDisplayingReward) yield return new WaitForEndOfFrame();
                    rewardPanel.SetActive(false);
                    master.SetPause = true;
                }
            }
        }
    }

    public void SetTorchPanel()
    {
        for(int i = 0; i < unlockedTorches.Length; i++)
        {
            torchPanel.GetChild(i + 2).gameObject.SetActive(unlockedTorches[i]);
            if (i == 0)
                torchPanel.GetChild(0).gameObject.SetActive(unlockedTorches[0]);
                torchPanel.GetChild(1).gameObject.SetActive(unlockedTorches[0]);
        }
    }

    // Based on method created by IvanTroho: https://www.codegrepper.com/code-examples/csharp/unity+string+format+time
    private string FormatTime(float input)
    {
        int time = Mathf.FloorToInt(input);
        int minutes = time / 60;
        int seconds = time % 60;
        int milliseconds = (int)(input % 1 * 100);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    private void Start()
    {
        Text[] texts = timerPanel.GetComponentsInChildren<Text>();
        rewardPanelAnimator = rewardPanel.GetComponent<Animator>();
        recordPanelAnimator = recordPanel.GetComponent<Animator>();
        SetTorchPanel();
        screenText = texts[0];
        splitText = texts[1];
        splitText.enabled = false;
        master = GetComponent<GameMaster>();
        AssignColours();
    }

    //Convert hex into color by comphonia: http://answers.unity.com/comments/1490787/view.html
    private void AssignColours()
    {
        colors = new Dictionary<string, Color>();

        if (ColorUtility.TryParseHtmlString("#FF5659", out Color positive))
            colors.Add("pos", positive);

        if (ColorUtility.TryParseHtmlString("#AB1C25", out Color positiveShadow))
            colors.Add("posShadow", positiveShadow);

        if (ColorUtility.TryParseHtmlString("#5BFF55", out Color negative))
            colors.Add("neg", negative);

        if (ColorUtility.TryParseHtmlString("#1CAB4B", out Color negativeShadow))
            colors.Add("negShadow", negativeShadow);
    }

    private void Update()
    {
        if (timerOn)
        {
            timerValue += Time.deltaTime;
            if(showTimer)screenText.text = FormatTime(timerValue);
        }
    }
}