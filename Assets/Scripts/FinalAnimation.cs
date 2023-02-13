using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalAnimation : MonoBehaviour
{
    public GameObject finalTorch;
    public CinemachineVirtualCamera virCamera;
    private GameObject player;
    private bool hasTriggered = false;

    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !hasTriggered)
        {
            int triggeredTorches = finalTorch.GetComponent<FinalTorch>().FinalLitTorches;

            if(triggeredTorches == 7)
            {
                hasTriggered = true;
                player = other.gameObject;
                player.GetComponent<PlayerMovement>().SetControl = false;
                GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayFinalle();
                StartCoroutine("FinalTorchAnimation");
            }
        }
    }

    IEnumerator FinalTorchAnimation()
    {
        yield return null;

        virCamera.Follow = finalTorch.transform;

        for(int i = 1; i < 8; i++)
        {
            yield return new WaitForSeconds(1.2f- i/8.0f);
            finalTorch.transform.GetChild(i).GetComponent<Torch>().FinalleActivateTorch();
        }
        yield return new WaitForSeconds(1);

        virCamera.Follow = player.transform;
        finalTorch.GetComponent<SphereCollider>().enabled = true;
        player.GetComponent<PlayerMovement>().SetControl = true;
    }
}
