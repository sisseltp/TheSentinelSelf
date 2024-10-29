using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 2;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale = 5;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Time.timeScale = 10;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Time.timeScale = 0;
        }
    }
}
