using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameHandler : MonoBehaviour
{
    public Image grappleCrosshair;
    [HideInInspector] public Image grappleImage;
    [HideInInspector] public TextMeshProUGUI slowTimeText;

    public bool timeSlowed;

    // Start is called before the first frame update
    void Start()
    {
        slowTimeText = GameObject.Find("Slow Time Text").GetComponent<TextMeshProUGUI>();
        grappleCrosshair = GameObject.Find("Grapple Crosshair").GetComponent<Image>();
        grappleImage = GameObject.Find("Grapple Image").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Slow Time"))
        {
            timeSlowed = true;
        }

        if (timeSlowed)
            StartSlowTime();
        else
            StopSlowTime();
    }

    IEnumerator SlowTimeCoroutine(int seconds)
    {
        coroutineStarted = true;
        int counter = seconds;
        while (counter > 0)
        {
            slowTimeText.text = Mathf.Round(counter).ToString();
            yield return new WaitForSecondsRealtime(1);
            counter--;
        }
        
        StopSlowTime();
    }

    private bool coroutineStarted = false;
    void StartSlowTime()
    {
        Time.timeScale = 0.4f;
        if(!coroutineStarted)
            StartCoroutine(SlowTimeCoroutine(3));
    }

    void StopSlowTime()
    {
        Time.timeScale = 1;
        timeSlowed = false;
        coroutineStarted = false;
        slowTimeText.text = "Q";
    }
}