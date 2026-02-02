using UnityEngine;

/// <summary>
/// Alvo que se move - oferece mais desafio que alvos estáticos.
/// Pode se mover em linha, circular, ou girar no próprio eixo.
/// </summary>
public class AlvoMovel : MonoBehaviour
{
    [Header("Pontuação")]
    [Tooltip("Pontos dados ao acertar este alvo")]
    public int pontos = 20;
    
    [Header("Tipo de Movimento")]
    public TipoMovimento tipoMovimento = TipoMovimento.PingPong;
    
    [Header("Configurações de Movimento")]
    [Tooltip("Velocidade do movimento")]
    public float velocidade = 2f;
    
    [Tooltip("Distância do movimento (para PingPong e Circular)")]
    public float distanciaMovimento = 3f;
    
    [Tooltip("Eixo de rotação (para Girar)")]
    public Vector3 eixoRotacao = Vector3.up;
    
    [Header("Feedback Visual")]
    [Tooltip("Efeito de partículas ao acertar")]
    public GameObject efeitoAcerto;
    
    [Header("Feedback Sonoro")]
    [Tooltip("Som ao acertar")]
    public AudioClip somAcerto;
    
    [Header("Comportamento")]
    [Tooltip("Destruir alvo ao ser atingido")]
    public bool destruirAoAtingir = true;
    
    [Tooltip("Tempo antes de destruir")]
    public float tempoAntesDestruir = 0.1f;

    private Vector3 posicaoInicial;
    private float tempoDecorrido = 0f;
    private bool foiAtingido = false;

    public enum TipoMovimento
    {
        PingPong,      // Vai e volta em linha reta
        Circular,      // Movimento circular
        Girar          // Gira no próprio eixo
    }

    private void Awake()
    {
        // Garantir que o alvo não sai voando ao ser atingido
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Sem física, só movimento controlado
        }
    }

    private void Start()
    {
        posicaoInicial = transform.position;
    }

    private void Update()
    {
        if (foiAtingido) return;

        tempoDecorrido += Time.deltaTime;

        switch (tipoMovimento)
        {
            case TipoMovimento.PingPong:
                MovimentoPingPong();
                break;
            case TipoMovimento.Circular:
                MovimentoCircular();
                break;
            case TipoMovimento.Girar:
                MovimentoGirar();
                break;
        }
    }

    private void MovimentoPingPong()
    {
        // Movimento em vai-e-volta no eixo X
        float offset = Mathf.PingPong(tempoDecorrido * velocidade, distanciaMovimento);
        transform.position = posicaoInicial + Vector3.right * (offset - distanciaMovimento / 2f);
    }

    private void MovimentoCircular()
    {
        // Movimento circular no plano XZ
        float x = Mathf.Cos(tempoDecorrido * velocidade) * distanciaMovimento;
        float z = Mathf.Sin(tempoDecorrido * velocidade) * distanciaMovimento;
        transform.position = posicaoInicial + new Vector3(x, 0, z);
    }

    private void MovimentoGirar()
    {
        // Rotação no próprio eixo
        transform.Rotate(eixoRotacao, velocidade * 50f * Time.deltaTime);
    }

    public void Atingido()
    {
        if (foiAtingido) return;
        foiAtingido = true;

        Debug.Log($"Alvo Móvel atingido! +{pontos} pontos");

        // Feedback visual
        if (efeitoAcerto != null)
        {
            Instantiate(efeitoAcerto, transform.position, Quaternion.identity);
        }

        // Feedback sonoro
        if (somAcerto != null)
        {
            AudioSource.PlayClipAtPoint(somAcerto, transform.position);
        }

        // Notificar sistema de pontuação
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
