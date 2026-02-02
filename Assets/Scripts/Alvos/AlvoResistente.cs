using UnityEngine;

/// <summary>
/// Alvo que precisa ser acertado múltiplas vezes para ser destruído.
/// Muda de cor conforme perde vida. Ótimo para desafios de precisão.
/// </summary>
public class AlvoResistente : MonoBehaviour
{
    [Header("Pontuação")]
    [Tooltip("Pontos por cada acerto")]
    public int pontosPorAcerto = 5;
    
    [Tooltip("Bônus ao destruir completamente")]
    public int bonusDestruicao = 20;
    
    [Header("Vida")]
    [Tooltip("Número de acertos necessários para destruir")]
    public int vidaMaxima = 3;
    
    private int vidaAtual;
    
    [Header("Feedback Visual")]
    [Tooltip("Cores conforme a vida (do máximo ao mínimo)")]
    public Color[] coresPorVida = new Color[] { Color.green, Color.yellow, Color.red };
    
    [Tooltip("Efeito ao receber dano")]
    public GameObject efeitoDano;
    
    [Tooltip("Efeito ao ser destruído")]
    public GameObject efeitoDestruicao;
    
    [Header("Feedback Sonoro")]
    [Tooltip("Som ao receber dano")]
    public AudioClip somDano;
    
    [Tooltip("Som ao ser destruído")]
    public AudioClip somDestruicao;
    
    [Header("Comportamento")]
    [Tooltip("Escala um pouco menor a cada acerto")]
    public bool diminuirTamanho = true;
    
    [Tooltip("Empurrar ao receber dano (precisa de Rigidbody)")]
    public bool aplicarForca = false;
    
    [Tooltip("Força do empurrão")]
    public float forcaEmpurrao = 5f;
    
    [Tooltip("Congelar posição para não sair voando (recomendado quando aplicarForca = false)")]
    public bool congelarPosicao = true;

    private Renderer rend;
    private Vector3 escalaInicial;
    private Rigidbody rb;

    private void Awake()
    {
        vidaAtual = vidaMaxima;
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        escalaInicial = transform.localScale;
        
        // Configurar Rigidbody
        if (rb != null)
        {
            if (aplicarForca)
            {
                // Se vai aplicar força, mantém dinâmico mas congela posição inicialmente
                rb.isKinematic = false;
                if (congelarPosicao)
                {
                    rb.constraints = RigidbodyConstraints.FreezePosition;
                }
            }
            else
            {
                // Se não vai aplicar força, deixa kinematic (não sai voando)
                rb.isKinematic = true;
            }
        }
        
        // Aplicar cor inicial
        AtualizarCor();
    }

    public void Atingido()
    {
        // Receber dano
        vidaAtual--;
        
        Debug.Log($"Alvo Resistente atingido! Vida: {vidaAtual}/{vidaMaxima} (+{pontosPorAcerto} pontos)");

        // Feedback visual - efeito de dano
        if (efeitoDano != null)
        {
            Instantiate(efeitoDano, transform.position, Quaternion.identity);
        }

        // Feedback sonoro - som de dano
        if (somDano != null)
        {
            AudioSource.PlayClipAtPoint(somDano, transform.position);
        }

        // Atualizar cor baseado na vida
        AtualizarCor();

        // Diminuir tamanho
        if (diminuirTamanho)
        {
            float percentualVida = (float)vidaAtual / vidaMaxima;
            transform.localScale = escalaInicial * Mathf.Max(0.5f, percentualVida);
        }

        // Aplicar força (se tiver Rigidbody e aplicarForca estiver ativo)
        if (aplicarForca && rb != null)
        {
            // Descongelar temporariamente para aplicar força
            if (congelarPosicao)
            {
                rb.constraints = RigidbodyConstraints.None;
            }
            
            Vector3 direcaoAleatoria = Random.insideUnitSphere;
            direcaoAleatoria.y = Mathf.Abs(direcaoAleatoria.y); // Empurra pra cima
            rb.AddForce(direcaoAleatoria * forcaEmpurrao, ForceMode.Impulse);
        }

        // Notificar pontuação
        var gerenciador = FindFirstObjectByType<GerenciadorJogo>();
        if (gerenciador != null)
        {
            gerenciador.AdicionarPontos(pontosPorAcerto);
        }

        // Verificar se foi destruído
        if (vidaAtual <= 0)
        {
            Destruir();
        }
    }

    private void AtualizarCor()
    {
        if (rend == null || coresPorVida.Length == 0) return;

        // Calcular índice da cor baseado na vida
        int indice = vidaMaxima - vidaAtual;
        indice = Mathf.Clamp(indice, 0, coresPorVida.Length - 1);

        rend.material.color = coresPorVida[indice];
    }

    private void Destruir()
    {
        Debug.Log($"Alvo Resistente destruído! Bônus: +{bonusDestruicao} pontos");

        // Feedback visual - efeito de destruição
        if (efeitoDestruicao != null)
        {
            Instantiate(efeitoDestruicao, transform.position, Quaternion.identity);
        }

        // Feedback sonoro - som de destruição
        if (somDestruicao != null)
        {
            AudioSource.PlayClipAtPoint(somDestruicao, transform.position);
        }

        // Notificar bônus de destruição
        var gerenciador = FindFirstObjectByType<GerenciadorJogo>();
        if (gerenciador != null)
        {
            gerenciador.AdicionarPontos(bonusDestruicao);
            gerenciador.RegistrarAlvoDestruido();
        }

        Destroy(gameObject);
    }
}
