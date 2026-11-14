using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ToGameTransitor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    RawImage titleThingy;
    [SerializeField]
    Texture2D title2;
    [SerializeField]
    RawImage overlay;
    private bool isTrans = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToGame1()
    {
        titleThingy.texture= title2;
        if (!isTrans)
        {
            isTrans = true;
            StartCoroutine(Transition1());

        }
    }

    public IEnumerator Transition1()
    {
        yield return new WaitForSeconds(1f);
        for(int i = 0; i < 50; i++)
        {
            yield return new WaitForSeconds(0.01f);
            overlay.color = new Color(0, 0, 0, Mathf.InverseLerp(0, 50, i));
        }

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("BoidScene3");
    }
}
