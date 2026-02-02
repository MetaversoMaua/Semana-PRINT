using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Força o tracking mode para Floor (chão) ao iniciar.
/// Adicione este script no XR Origin.
/// </summary>
public class ConfigurarTrackingVR : MonoBehaviour
{
    private void Start()
    {
        // Define o tracking origin como Floor (altura do chão real)
        var xrInputSubsystems = new System.Collections.Generic.List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(xrInputSubsystems);

        foreach (var subsystem in xrInputSubsystems)
        {
            if (subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor))
            {
                Debug.Log("Tracking origin configurado para FLOOR com sucesso!");
                
                // Recentrar usando o método atualizado
                subsystem.TryRecenter();
            }
            else
            {
                Debug.LogWarning("Não foi possível definir tracking origin como Floor. Tentando Device...");
                subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
            }
        }
    }
}
