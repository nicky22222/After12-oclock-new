using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    public static bool IsStart { get; set; }
    public static bool IsHoldingWand { get; set; }

    [SerializeField] private List<GameProgressData> progressList;
    [SerializeField] private int currentIndex = -1;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            Debug.LogError("GameManager already exists!");
            return;
        }
        Instance = this;
    }
    
    private void Update()
    {
        if ((currentIndex < 0 && IsStart) ||
            (currentIndex >= 0 && progressList[currentIndex].IsComplete()) ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            if (currentIndex < progressList.Count - 1)
            {
                currentIndex++;
                Debug.Log($"Execute progress {currentIndex}");
                progressList[currentIndex].Execute();
            }
        }
    }

    public void BroadcastMsg(string msg)
    {
        progressList[currentIndex].BroadcastMsg(msg);
    }

    public void SetSkybox(Material newMat)
    {
        if (newMat == null) return;
        RenderSettings.skybox = newMat;
        DynamicGI.UpdateEnvironment();
    }
}
