using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Dardo lançável com física simplificada para facilitar o gameplay.
/// Voa sempre na direção que está apontando, independente do movimento da mão.
/// </summary>
public class Lancavel : MonoBehaviour
{
    [Header("Configurações de Arremesso")]
    [Tooltip("Velocidade base do dardo")]
    public float velocidadeBase = 10f;
    
    [Tooltip("Velocidade mínima garantida")]
    public float velocidadeMinima = 7f;
    
    [Tooltip("Multiplicador baseado na velocidade da mão (0 = ignora movimento da mão)")]
    [Range(0f, 2f)]
    public float influenciaVelocidadeMao = 0.1f;
    
    [Tooltip("Força o dardo a voar sempre na direção que está apontando")]
    public bool forcarDirecaoReta = true;
    
    [Header("Estabilização")]
    [Tooltip("Velocidade mínima na direção correta para receber assist (m/s)")]
    public float velocidadeMinimaParaAssist = 1f;
    
    [Tooltip("Cancela velocidades laterais indesejadas quando recebe assist")]
    [Range(0f, 1f)]
    public float cancelarVelocidadesLaterais = 0.9f;
    
    [Tooltip("Estabiliza a rotação durante o voo")]
    public bool estabilizarRotacao = true;
    
    [Tooltip("Força de estabilização (maior = mais estável)")]
    public float forcaEstabilizacao = 8f;
    
    [Header("Magnetismo (facilita acertar)")]
    [Tooltip("Ativa magnetismo em direção aos alvos")]
    public bool usarMagnetismo = true;
    
    [Tooltip("Distância máxima para magnetismo funcionar")]
    public float distanciaMagnetismo = 1.5f;
    
    [Tooltip("Força do magnetismo (maior = atrai mais forte)")]
    public float forcaMagnetismo = 5f;
    
    [Header("Geral")]
    [Tooltip("Tempo até o dardo ser destruído")]
    public float tempoDeVida = 10f;
    
    [Tooltip("Multiplica a gravidade durante o voo (0 = sem gravidade, 1 = gravidade normal)")]
    [Range(0f, 1f)]
    public float multiplicadorGravidade = 0.3f;

    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private bool jaColidiu = false;
    private bool foiLancado = false;

    // Para calcular velocidade manualmente
    private Vector3 posicaoAnterior;
    private Quaternion rotacaoAnterior;
    private bool estaSegurando = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (rb == null)
            Debug.LogError("Lancavel: Rigidbody não encontrado!");

        if (grab == null)
            Debug.LogError("Lancavel: XRGrabInteractable não encontrado!");
    }

    private void Start()
    {
        // Auto-destruir após tempo
        Destroy(gameObject, tempoDeVida);
    }

    private void OnEnable()
    {
        if (grab != null)
        {
            grab.selectEntered.AddListener(AoPegar);
            grab.selectExited.AddListener(AoSoltar);
        }
    }

    private void OnDisable()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(AoPegar);
            grab.selectExited.RemoveListener(AoSoltar);
        }
    }

    private void AoPegar(SelectEnterEventArgs args)
    {
        estaSegurando = true;
        posicaoAnterior = transform.position;
        rotacaoAnterior = transform.rotation;
    }

    private void Update()
    {
        // Atualizar posição anterior enquanto segura
        if (estaSegurando && !foiLancado)
        {
            posicaoAnterior = transform.position;
            rotacaoAnterior = transform.rotation;
        }
    }

    private void AoSoltar(SelectExitEventArgs args)
    {
        if (foiLancado) return;
        foiLancado = true;
        estaSegurando = false;

        // Desativar gravidade para o dardo voar mais reto
        if (rb != null && multiplicadorGravidade < 1f)
        {
            rb.useGravity = false;
        }

        // Registrar tiro no gerenciador
        var gerenciador = FindFirstObjectByType<GerenciadorJogo>();
        if (gerenciador != null)
        {
            gerenciador.RegistrarTiro();
        }

        // Calcular velocidade manualmente baseado no movimento do dardo
        Vector3 velocidadeVetorMao = (transform.position - posicaoAnterior) / Time.deltaTime;

        // Direção do arremesso (Down = -transform.up)
        Vector3 direcaoCorreta = -transform.up;

        if (rb != null)
        {
            if (forcarDirecaoReta)
            {
                // MODO ARCADE: Sempre voa na direção que está apontando
                // Velocidade baseada apenas na magnitude do movimento
                float velocidadeFinal = velocidadeBase + (velocidadeVetorMao.magnitude * influenciaVelocidadeMao);
                velocidadeFinal = Mathf.Max(velocidadeFinal, velocidadeMinima);
                
                // Aplica velocidade SEMPRE na direção que o dardo aponta
                rb.linearVelocity = direcaoCorreta * velocidadeFinal;
                
                // Sem rotação indesejada
                rb.angularVelocity = Vector3.zero;
                
                Debug.Log($"✓ Dardo arcade: direção fixa, vel: {velocidadeFinal:F2} m/s");
            }
            else
            {
                // MODO REALISTA (código antigo)
                // Calcular velocidade na direção do arremesso
                float velocidadeNaDirecaoCorreta = Vector3.Dot(velocidadeVetorMao, direcaoCorreta);

                // Verificar se recebe assist
                bool recebeAssist = velocidadeNaDirecaoCorreta >= velocidadeMinimaParaAssist;

                if (recebeAssist)
                {
                    // ASSIST: voa reto com velocidade garantida
                    float velocidadeFinal = velocidadeBase + (velocidadeVetorMao.magnitude * influenciaVelocidadeMao);
                    velocidadeFinal = Mathf.Max(velocidadeFinal, velocidadeMinima);
                    
                    // Calcular velocidade na direção desejada
                    Vector3 velocidadeNaDirecao = direcaoCorreta * velocidadeFinal;
                    
                    // Manter apenas uma pequena porcentagem das velocidades laterais
                    Vector3 velocidadeLateral = velocidadeVetorMao - (Vector3.Dot(velocidadeVetorMao, direcaoCorreta) * direcaoCorreta);
                    velocidadeLateral *= (1f - cancelarVelocidadesLaterais);
                    
                    // Aplicar velocidade final (direção correta + laterais reduzidas)
                    rb.linearVelocity = velocidadeNaDirecao + velocidadeLateral;
                    
                    // Rotação mínima
                    rb.angularVelocity = transform.right * Random.Range(-0.5f, 0.5f);
                    
                    Debug.Log($"✓ Dardo COM assist (vel: {velocidadeNaDirecaoCorreta:F2} m/s → final: {velocidadeFinal:F2} m/s)");
                }
                else
                {
                    // SEM ASSIST: usa velocidade real
                    if (velocidadeVetorMao.magnitude < 1f)
                    {
                        // Muito lento, aplica velocidade mínima
                        rb.linearVelocity = direcaoCorreta * velocidadeMinima;
                    }
                    else
                    {
                        rb.linearVelocity = velocidadeVetorMao;
                    }
                    
                    rb.angularVelocity = Vector3.Cross(velocidadeVetorMao, direcaoCorreta) * 0.5f;
                    
                    Debug.Log($"✗ Dardo SEM assist (vel: {velocidadeNaDirecaoCorreta:F2} m/s)");
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!foiLancado || jaColidiu || rb == null) return;
        
        // Aplicar gravidade customizada (reduzida)
        if (!rb.useGravity && multiplicadorGravidade > 0f)
        {
            rb.AddForce(Physics.gravity * multiplicadorGravidade, ForceMode.Acceleration);
        }
        
        // Continuar cancelando velocidades laterais durante o voo (assist contínuo)
        if (cancelarVelocidadesLaterais > 0f && rb.linearVelocity.magnitude > 1f)
        {
            Vector3 direcaoAtual = -transform.up;
            Vector3 velocidadeNaDirecao = Vector3.Project(rb.linearVelocity, direcaoAtual);
            Vector3 velocidadeLateral = rb.linearVelocity - velocidadeNaDirecao;
            
            // Reduzir velocidades laterais continuamente
            velocidadeLateral *= (1f - cancelarVelocidadesLaterais * Time.fixedDeltaTime * 2f);
            
            rb.linearVelocity = velocidadeNaDirecao + velocidadeLateral;
        }
        
        // Estabilizar rotação durante o voo
        if (estabilizarRotacao && rb.linearVelocity.magnitude > 1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * forcaEstabilizacao);
        }
        
        // Magnetismo: atrai levemente em direção aos alvos próximos
        if (usarMagnetismo)
        {
            AplicarMagnetismo();
        }
    }
    
    private void AplicarMagnetismo()
    {
        // Buscar todos os alvos na cena
        Alvo[] alvos = FindObjectsByType<Alvo>(FindObjectsSortMode.None);
        
        Alvo alvoMaisProximo = null;
        float menorDistancia = distanciaMagnetismo;
        
        // Encontrar alvo mais próximo dentro do raio
        foreach (var alvo in alvos)
        {
            float distancia = Vector3.Distance(transform.position, alvo.transform.position);
            
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                alvoMaisProximo = alvo;
            }
        }
        
        // Se encontrou alvo próximo, aplica força de atração
        if (alvoMaisProximo != null)
        {
            Vector3 direcaoParaAlvo = (alvoMaisProximo.transform.position - transform.position).normalized;
            
            // Força proporcional à proximidade (mais perto = mais forte)
            float percentualDistancia = 1f - (menorDistancia / distanciaMagnetismo);
            float forca = forcaMagnetismo * percentualDistancia;
            
            // Aplicar força suave em direção ao alvo
            rb.AddForce(direcaoParaAlvo * forca, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision colisao)
    {
        // Ignorar colisões se ainda estiver sendo segurado
        if (!foiLancado) return;
        
        if (jaColidiu) return;
        jaColidiu = true;

        // Detectar qualquer tipo de alvo
        Alvo alvo = colisao.collider.GetComponent<Alvo>();
        AlvoEstatico alvoEstatico = colisao.collider.GetComponent<AlvoEstatico>();
        AlvoMovel alvoMovel = colisao.collider.GetComponent<AlvoMovel>();
        AlvoResistente alvoResistente = colisao.collider.GetComponent<AlvoResistente>();

        // Chamar método Atingido() do tipo correto
        if (alvo != null)
        {
            alvo.Atingido();
        }
        else if (alvoEstatico != null)
        {
            alvoEstatico.Atingido();
        }
        else if (alvoMovel != null)
        {
            alvoMovel.Atingido();
        }
        else if (alvoResistente != null)
        {
            alvoResistente.Atingido();
        }
        
        // Destruir o dardo imediatamente após acertar qualquer coisa
        Destroy(gameObject);
    }
}


