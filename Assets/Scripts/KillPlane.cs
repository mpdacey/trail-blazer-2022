using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayPlayerAudioClip(2);
            other.gameObject.GetComponent<PlayerLife>().StartCoroutine("Burnout");
        }
    }
}
