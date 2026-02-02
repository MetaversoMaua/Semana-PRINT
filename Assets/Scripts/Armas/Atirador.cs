using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Atirador : MonoBehaviour
{
    [Header("Referências")]
    public Transform pontoDisparo;
    public GameObject prefabProjetil;

    [Header("Configurações")]
    public float forcaDisparo = 25f;
    public float intervaloDisparo = 0.3f;
    [Tooltip("Rotação adicional do projétil (X, Y, Z em graus). Ajuste conforme o modelo 3D.")]
    public Vector3 rotacaoProjetil = new Vector3(0, 180, 0);

    private float ultimoDisparo;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (grab == null)
            Debug.LogError("Atirador: XRGrabInteractable não encontrado neste GameObject!");
        
        if (pontoDisparo == null)
            Debug.LogError("Atirador: pontoDisparo não atribuído no Inspector!");
        
        if (prefabProjetil == null)
            Debug.LogError("Atirador: prefabProjetil não atribuído no Inspector!");
    }

    private void OnEnable()
    {
        grab.activated.AddListener(Disparar);
    }

    private void OnDisable()
    {
        grab.activated.RemoveListener(Disparar);
    }

    public void Disparar(ActivateEventArgs args)
    {
        if (Time.time < ultimoDisparo + intervaloDisparo) return;
        
        if (pontoDisparo == null || prefabProjetil == null)
        {
            Debug.LogWarning("Atirador: Não é possível disparar. Verifique pontoDisparo e prefabProjetil.");
            return;
        }

        ultimoDisparo = Time.time;

        // Aplica rotação configurável para cada modelo de projétil
        Quaternion rotacaoCorrigida = pontoDisparo.rotation * Quaternion.Euler(rotacaoProjetil);
        
        GameObject proj = Instantiate(
            prefabProjetil,
            pontoDisparo.position,
            rotacaoCorrigida
        );

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.AddForce(pontoDisparo.forward * forcaDisparo, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("Atirador: Projétil não tem Rigidbody!");
        }
    }
}
