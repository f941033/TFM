using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NormalScale()
    {
        Time.timeScale = 1f;
    }

    public void DoubleScale()
    {
        Time.timeScale = 2f;
    }

    public void PauseScale()
    {
        Time.timeScale = 0f;
    }
}
