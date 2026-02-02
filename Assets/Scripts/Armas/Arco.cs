using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR;

/// <summary>
/// Sistema de arco e flecha para VR - versão simplificada.
/// Aproxima da corda, aperta trigger, puxa, solta trigger = dispara.
/// </summary>
public class Arco : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Ponto onde a flecha spawna")]
    public Transform pontoFlecha;
    
    [Tooltip("Transform da corda (será puxado)")]
    public Transform corda;
    
    [Tooltip("Prefab da flecha")]
    public GameObject prefabFlecha;
    
    [Tooltip("Transform da zona de puxar (onde a mão precisa estar)")]
    public Transform zonaPuxar;

    [Header("Configurações de Puxar")]
    [Tooltip("Distância máxima da zona para poder puxar (m)")]
    public float distanciaMaximaZona = 0.15f;
    
    [Tooltip("Distância máxima que pode puxar (m)")]
    public float distanciaMaximaPuxar = 0.5f;
    
    [Tooltip("Força mínima do disparo")]
    public float forcaMinima = 10f;
    
    [Tooltip("Força máxima do disparo")]
    public float forcaMaxima = 30f;
    
    [Tooltip("Rotação adicional da flecha (X, Y, Z em graus). Ajuste conforme o modelo 3D.")]
    public Vector3 rotacaoFlecha = new Vector3(0, 0, 0);

    private Vector3 posicaoInicialCorda;
    private bool estaPuxando = false;
    private float distanciaPuxada = 0f;
    private bool arcoSegurado = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactorPuxando = null;

    private void Awake()
    {
        // Validações
        if (pontoFlecha == null)
            Debug.LogError("Arco: pontoFlecha não atribuído!");
        
        if (corda == null)
            Debug.LogError("Arco: corda não atribuído!");
        
        if (prefabFlecha == null)
            Debug.LogError("Arco: prefabFlecha não atribuído!");
        
        if (zonaPuxar == null)
        {
            Debug.LogWarning("Arco: zonaPuxar não atribuído, usando posição da corda");
            zonaPuxar = corda;
        }

        // Salvar posição inicial da corda
        if (corda != null)
            posicaoInicialCorda = corda.localPosition;
    }

    private void OnEnable()
    {
        var grabArco = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabArco != null)
        {
            grabArco.selectEntered.AddListener(AoPegarArco);
            grabArco.selectExited.AddListener(AoSoltarArco);
        }
    }

    private void OnDisable()
    {
        var grabArco = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabArco != null)
        {
            grabArco.selectEntered.RemoveListener(AoPegarArco);
            grabArco.selectExited.RemoveListener(AoSoltarArco);
        }
    }

    private void AoPegarArco(SelectEnterEventArgs args)
    {
        arcoSegurado = true;
    }

    private void AoSoltarArco(SelectExitEventArgs args)
    {
        arcoSegurado = false;
        estaPuxando = false;
        
        if (corda != null)
            corda.localPosition = posicaoInicialCorda;
        
        distanciaPuxada = 0f;
    }

    private void Update()
    {
        if (!arcoSegurado) return;

        // Buscar qualquer controller apertando o trigger
        var todosInteractors = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>(FindObjectsSortMode.None);
        
        foreach (var interactor in todosInteractors)
        {
            bool triggerApertado = VerificarTrigger(interactor);
            float distancia = Vector3.Distance(interactor.transform.position, zonaPuxar.position);
            
            // Se está dentro da zona E apertando trigger = começar a puxar
            if (distancia < distanciaMaximaZona && triggerApertado)
            {
                if (!estaPuxando)
                {
                    estaPuxando = true;
                    interactorPuxando = interactor;
                }
                
                // Calcular distância puxada
                Vector3 posicaoControllerLocal = transform.InverseTransformPoint(interactor.transform.position);
                float distanciaZ = Mathf.Clamp(-posicaoControllerLocal.z, 0f, distanciaMaximaPuxar);
                distanciaPuxada = distanciaZ;
                
                // Mover corda
                Vector3 novaPosicao = posicaoInicialCorda;
                novaPosicao.z -= distanciaZ;
                corda.localPosition = novaPosicao;
            }
            // Se está puxando E o trigger ainda está apertado E saiu da zona = DISPARAR!
            else if (estaPuxando && interactor == interactorPuxando && triggerApertado && distancia >= distanciaMaximaZona)
            {
                // Saiu da zona enquanto segura trigger = disparar
                Disparar();
                
                // Reset
                estaPuxando = false;
                interactorPuxando = null;
                corda.localPosition = posicaoInicialCorda;
                distanciaPuxada = 0f;
                return; // Importante: não processar mais
            }
        }
        
        // Se soltou o trigger antes de sair da zona = cancelar
        if (estaPuxando && interactorPuxando != null)
        {
            bool triggerAindaApertado = VerificarTrigger(interactorPuxando);
            if (!triggerAindaApertado)
            {
                // Cancelar - soltou trigger antes de puxar para fora
                estaPuxando = false;
                interactorPuxando = null;
                corda.localPosition = posicaoInicialCorda;
                distanciaPuxada = 0f;
            }
        }
    }

    private bool VerificarTrigger(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        // Método 1: Tentar pegar input do XR através do device
        var inputDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
            UnityEngine.XR.InputDeviceCharacteristics.Controller | 
            UnityEngine.XR.InputDeviceCharacteristics.HeldInHand,
            inputDevices
        );

        foreach (var device in inputDevices)
        {
            // Verificar se o device está na mesma posição do interactor (mesmo controller)
            if (Vector3.Distance(device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 pos) ? pos : Vector3.zero, 
                                 interactor.transform.position) < 0.1f)
            {
                // Tentar ler o valor do trigger
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue))
                {
                    return triggerValue > 0.5f;
                }
            }
        }

        // Método 2: Buscar todos os devices e pegar qualquer trigger apertado
        foreach (var device in inputDevices)
        {
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue))
            {
                if (triggerValue > 0.5f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void Disparar()
    {
        if (pontoFlecha == null || prefabFlecha == null) return;

        // Calcular força baseada em quanto puxou
        float percentualPuxado = Mathf.Clamp01(distanciaPuxada / distanciaMaximaPuxar);
        float forca = Mathf.Lerp(forcaMinima, forcaMaxima, percentualPuxado);

        // Registrar tiro no gerenciador
        var gerenciador = FindFirstObjectByType<GerenciadorJogo>();
        if (gerenciador != null)
        {
            gerenciador.RegistrarTiro();
        }

        // Instanciar flecha com rotação configurável
        Quaternion rotacaoCorrigida = pontoFlecha.rotation * Quaternion.Euler(rotacaoFlecha);
        GameObject flecha = Instantiate(prefabFlecha, pontoFlecha.position, rotacaoCorrigida);

        // Aplicar força
        Rigidbody rbFlecha = flecha.GetComponent<Rigidbody>();
        if (rbFlecha != null)
        {
            rbFlecha.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rbFlecha.linearVelocity = pontoFlecha.forward * forca;
        }

        Debug.Log($"Flecha disparada! Força: {forca:F1} (puxou {percentualPuxado * 100:F0}%)");
    }
}
