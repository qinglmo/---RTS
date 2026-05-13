using Unity.VisualScripting;
using UnityEngine;

public class InitializationManager : MonoBehaviour
{

    void Start()
    {
        GridWorldService.Instance.Initialize(GridManager.Instance, GridManager.Instance, GridManager.Instance);
    }
}