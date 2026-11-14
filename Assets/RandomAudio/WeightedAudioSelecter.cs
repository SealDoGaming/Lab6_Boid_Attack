using System;
using UnityEngine;

public class WeightedAudioSelecter
{
    private AudioClip[] audioList;
    private float[] weightArray;
    private float totalWeight = 0f;
    public WeightedAudioSelecter(WeightedAudio[] list)
    {
        audioList = new AudioClip[list.Length];
        weightArray = new float[list.Length];



        for(int i=0; i<list.Length; i++)
        {
            audioList[i] = list[i].audioClip;
            weightArray[i] = totalWeight;
            totalWeight += list[i].weight;

        }

    }


    public AudioClip RandomClip()
    {

        float target = UnityEngine.Random.value * totalWeight;
         int length = audioList.Length;
        int start = 0;
        int end = length - 1;
        while (start<end)
        {
            int index = (end - start) / 2 + start;
            bool curr = target >= weightArray[index];
            bool next = target < weightArray[index + 1];
            if (curr && next)
            {
                break;
            }
            else if (curr)
            {
                start = index+1;
            } else{
                end = index - 1;
            }

        }
        Debug.Log("target: " + target);
        Debug.Log("Start: " + start + " End: " + end);
        string str = "[";
        for(int i = 0; i < weightArray.Length; i++)
        {
            str += weightArray[i] + ',';
        }
        str+= "]";  
        
        Debug.Log(str);
        Debug.Log((end - start) / 2 + start);

        return audioList[(end - start) / 2 + start];
    }


}
