using UnityEngine;

/// <summary>
/// Alvo estático simples - a forma mais básica de alvo.
/// Bom para iniciantes aprenderem a mirar e disparar.
/// </summary>
public class AlvoEstatico : MonoBehaviour
{
    [Header("Pontuação")]
    [Tooltip("Pontos dados ao acertar este alvo")]
    public int pontos = 10;
    
    [Header("Feedback Visual")]
    [Tooltip("Efeito de partículas ao acertar (explosão, faísca, etc)")]
    public GameObject efeitoAcerto;
    
    [Tooltip("Material ou cor ao ser atingido (opcional)")]
    public Material materialAcerto;
    
    [Header("Feedback Sonoro")]
    [Tooltip("Som ao acertar")]
    public AudioClip somAcerto;
    
    [Header("Comportamento")]
    [Tooltip("Destruir alvo ao ser atingido")]
    public bool destruirAoAtingir = true;
    
    [Tooltip("Tempo antes de destruir (delay para ver efeito)")]
    public float tempoAntesDestruir = 0.1f;

    private Renderer rend;
    private Material materialOriginal;
    private bool foiAtingido = false;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            materialOriginal = rend.material;
        }
        
        // Garantir que o alvo não sai voando ao ser atingido
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Sem física, não se move
        }
    }

    public void Atingido()
    {
        if (foiAtingido) return;
        foiAtingido = true;

        Debug.Log($"Alvo Estático atingido! +{pontos} pontos");

        // Feedback visual - efeito de partículas
        if (efeitoAcerto != null)
        {
            Instantiate(efeitoAcerto, transform.position, Quaternion.identity);
        }

        // Feedback visual - mudar material
        if (materialAcerto != null && rend != null)
        {
            rend.material = materialAcerto;
        }

        // Feedback sonoro
        if (somAcerto != null)
        {
            AudioSource.PlayClipAtPoint(somAcerto, transform.position);
        }

        // Notificar sistema de pontuação (se existir)
        var gerenciador = FindFirstObjectByType<GerenciadorJogo>();
        if (gerenciador != null)
        {
            gerenciador.AdicionarPontos(pontos);
            gerenciador.RegistrarAlvoDestruido();
        }

        // Destruir após delay
        if (destruirAoAtingir)
        {
            Destroy(gameObject, tempoAntesDestruir);
        }
    }
}
