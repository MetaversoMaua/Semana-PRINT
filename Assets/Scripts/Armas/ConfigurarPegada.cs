using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Configura automaticamente o ponto de pegada do objeto.
/// Adicione este script em objetos que devem ser segurados em uma posição específica.
/// </summary>
public class ConfigurarPegada : MonoBehaviour
{
    [Header("Configuração da Pegada")]
    [Tooltip("Offset da posição em relação ao centro do objeto")]
    public Vector3 offsetPosicao = Vector3.zero;
    
    [Tooltip("Offset da rotação em relação ao objeto")]
    public Vector3 offsetRotacao = Vector3.zero;

    private void Awake()
    {
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (grab == null)
        {
            Debug.LogError("ConfigurarPegada: XRGrabInteractable não encontrado!");
            return;
        }

        // Criar attach transform se não existir
        if (grab.attachTransform == null)
        {
            GameObject attachPoint = new GameObject("AttachPoint");
            attachPoint.transform.SetParent(transform);
            attachPoint.transform.localPosition = offsetPosicao;
            attachPoint.transform.localRotation = Quaternion.Euler(offsetRotacao);
            
            grab.attachTransform = attachPoint.transform;
            
            Debug.Log($"ConfigurarPegada: Attach Transform criado para {gameObject.name}");
        }
        else
        {
            // Ajustar attach transform existente
            grab.attachTransform.localPosition = offsetPosicao;
            grab.attachTransform.localRotation = Quaternion.Euler(offsetRotacao);
        }

        // FORÇAR as configurações corretas
        grab.matchAttachPosition = true;
        grab.matchAttachRotation = true;
        grab.snapToColliderVolume = false;
        grab.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous;
        grab.attachEaseInTime = 0f;

        Debug.Log($"ConfigurarPegada: Configurações aplicadas para {gameObject.name}");
    }
}

