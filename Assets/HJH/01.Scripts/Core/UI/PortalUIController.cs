using UnityEngine;

public class PortalUIController : MonoBehaviour
{
    [SerializeField] private PortalMapUI _portalMapUI;

    public void OpenPortalUI()
    {
        _portalMapUI.OpenUI();
    }
}