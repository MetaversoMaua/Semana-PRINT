using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Modo Precisão - Desafio de pontaria com sistema de combo.
/// Acertos consecutivos aumentam o multiplicador. Erros resetam o combo.
/// </summary>
public class ModoPrecisao : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Ativar este modo de jogo")]
    public bool ativo = false;
    
    [Header("Sistema de Combo")]
    [Tooltip("Multiplicador atual de pontos")]
    private int multiplicadorCombo = 1;
    
    [Tooltip("Acertos consecutivos")]
    private int acertosConsecutivos = 0;
    
    [Tooltip("Acertos necessários para aumentar combo")]
    public int acertosParaCombo = 3;
    
    [Tooltip("Multiplicador máximo")]
    public int multiplicadorMaximo = 5;
    
    [Tooltip("Tempo que o combo fica ativo sem acertar (segundos)")]
    public float tempoCombo = 5f;
    
    private float tempoUltimoAcerto = 0f;
    
    [Header("Penalidades")]
    [Tooltip("Resetar combo ao errar tiro")]
    public bool resetarComboAoErrar = true;
    
    [Tooltip("Raio para detectar 'erros' (tiros que não acertam nada)")]
    public float distanciaMinimaTiro = 5f;
    
    [Header("Spawn de Alvos")]
    [Tooltip("Prefabs de alvos para spawnar")]
    public GameObject[] prefabsAlvos;
    
    [Tooltip("Posições onde alvos podem spawnar")]
    public Transform[] pontosSpawn;
    
    [Tooltip("Rotação dos alvos ao spawnar (X, Y, Z em graus). Ajuste conforme os modelos 3D.")]
    public Vector3 rotacaoAlvos = new Vector3(0, 0, 0);
    
    [Tooltip("Número de alvos ativos simultaneamente")]
    public int alvosSimultaneos = 3;
    
    [Header("UI")]
    [Tooltip("Texto para mostrar combo")]
    public TMP_Text textoCombo;
    
    [Tooltip("Texto para mostrar informações do modo")]
    public TMP_Text textoModo;
    
    [Tooltip("Texto para mostrar dicas")]
    public TMP_Text textoDicas;

    private GerenciadorJogo gerenciador;
    private int pontosOriginais = 0;
    private bool jogoIniciado = false;

    private void Start()
    {
        if (!ativo) 
        {
            this.enabled = false;
            return;
        }

        gerenciador = FindFirstObjectByType<GerenciadorJogo>();
        
        if (gerenciador == null)
        {
            Debug.LogWarning("ModoPrecisao: GerenciadorJogo não encontrado na cena.");
        }
        
        if (textoModo != null)
        {
            textoModo.text = "MODO: PRECISÃO";
        }
        
        if (textoDicas != null)
        {
            textoDicas.text = "Aperte o botão do controle para começar!";
        }
        
        // Validar configurações
        if (prefabsAlvos == null || prefabsAlvos.Length == 0)
        {
            Debug.LogError("ModoPrecisao: Nenhum prefab de alvo configurado!");
            return;
        }
        
        if (pontosSpawn == null || pontosSpawn.Length == 0)
        {
            Debug.LogError("ModoPrecisao: Nenhum ponto de spawn configurado!");
            return;
        }

        Debug.Log("Modo Precisão pronto! Aperte o botão do controle para começar.");
    }

    private void Update()
    {
        if (!ativo) return;

        // Verificar input para iniciar jogo
        if (!jogoIniciado)
        {
            if (VerificarBotaoIniciar())
            {
                IniciarJogo();
            }
            return;
        }

        // Verificar timeout do combo
        if (acertosConsecutivos > 0 && Time.time - tempoUltimoAcerto > tempoCombo)
        {
            ResetarCombo();
            Debug.Log("Combo perdido por tempo!");
        }

        // Atualizar UI do combo
        AtualizarUICombo();

        // Manter número de alvos constante
        ManterAlvosAtivos();
    }

    private bool VerificarBotaoIniciar()
    {
        // Buscar todos os controllers e verificar se algum apertou o botão primary (A/X)
        var inputDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
            UnityEngine.XR.InputDeviceCharacteristics.Controller | 
            UnityEngine.XR.InputDeviceCharacteristics.HeldInHand,
            inputDevices
        );

        foreach (var device in inputDevices)
        {
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool buttonPressed) && buttonPressed)
            {
                return true;
            }
        }

        return false;
    }

    private void IniciarJogo()
    {
        jogoIniciado = true;
        multiplicadorCombo = 1;
        acertosConsecutivos = 0;
        tempoUltimoAcerto = Time.time;

        if (gerenciador != null)
        {
            gerenciador.ResetarJogo();
        }
        
        if (textoDicas != null)
        {
            textoDicas.text = "Acerte consecutivamente para aumentar o combo!\nErros resetam o multiplicador.";
        }

        // Spawnar alvos iniciais
        for (int i = 0; i < alvosSimultaneos && i < pontosSpawn.Length; i++)
        {
            SpawnarAlvoAleatorio();
        }

        // Interceptar sistema de pontuação
        InvokeRepeating(nameof(VerificarAcertos), 0.5f, 0.5f);

        Debug.Log("Modo Precisão iniciado! Faça combos para multiplicar seus pontos!");
    }

    private void VerificarAcertos()
    {
        if (gerenciador == null) return;

        // Detectar se houve acerto desde a última verificação
        if (gerenciador.pontos > pontosOriginais)
        {
            RegistrarAcerto();
            pontosOriginais = gerenciador.pontos;
        }
    }

    public void RegistrarAcerto()
    {
        acertosConsecutivos++;
        tempoUltimoAcerto = Time.time;

        // Aumentar multiplicador a cada X acertos
        if (acertosConsecutivos % acertosParaCombo == 0)
        {
            multiplicadorCombo = Mathf.Min(multiplicadorCombo + 1, multiplicadorMaximo);
            Debug.Log($"COMBO UP! Multiplicador: x{multiplicadorCombo}");
        }

        // Aplicar multiplicador aos pontos
        if (gerenciador != null && multiplicadorCombo > 1)
        {
            int bonus = (pontosOriginais - gerenciador.pontos) * (multiplicadorCombo - 1);
            gerenciador.AdicionarPontos(bonus);
        }
    }

    public void RegistrarErro()
    {
        if (!resetarComboAoErrar) return;

        if (acertosConsecutivos > 0)
        {
            Debug.Log("COMBO PERDIDO! Erro registrado.");
            ResetarCombo();
        }
    }

    private void ResetarCombo()
    {
        multiplicadorCombo = 1;
        acertosConsecutivos = 0;
    }

    private void AtualizarUICombo()
    {
        if (textoCombo == null) return;

        if (multiplicadorCombo > 1)
        {
            textoCombo.text = $"COMBO: x{multiplicadorCombo}\n{acertosConsecutivos} acertos";
            
            // Efeito visual - pulsar quando combo alto
            float escala = 1f + Mathf.Sin(Time.time * 5f) * 0.1f * multiplicadorCombo;
            textoCombo.transform.localScale = Vector3.one * escala;
            
            // Cor baseada no multiplicador
            textoCombo.color = Color.Lerp(Color.white, Color.yellow, (float)multiplicadorCombo / multiplicadorMaximo);
        }
        else
        {
            textoCombo.text = $"COMBO: x1\n{acertosConsecutivos}/{acertosParaCombo}";
            textoCombo.transform.localScale = Vector3.one;
            textoCombo.color = Color.white;
        }
    }

    private void ManterAlvosAtivos()
    {
        int alvosAtuais = ContarAlvosAtivos();

        while (alvosAtuais < alvosSimultaneos && prefabsAlvos.Length > 0 && pontosSpawn.Length > 0)
        {
            SpawnarAlvoAleatorio();
            alvosAtuais++;
        }
    }

    private int ContarAlvosAtivos()
    {
        int total = 0;
        total += FindObjectsByType<AlvoEstatico>(FindObjectsSortMode.None).Length;
        total += FindObjectsByType<AlvoMovel>(FindObjectsSortMode.None).Length;
        total += FindObjectsByType<AlvoResistente>(FindObjectsSortMode.None).Length;
        total += FindObjectsByType<Alvo>(FindObjectsSortMode.None).Length;
        return total;
    }

    private void SpawnarAlvoAleatorio()
    {
        if (prefabsAlvos.Length == 0 || pontosSpawn.Length == 0) return;

        // Tentar encontrar um ponto livre (máximo 10 tentativas)
        for (int tentativa = 0; tentativa < 10; tentativa++)
        {
            Transform pontoEscolhido = pontosSpawn[Random.Range(0, pontosSpawn.Length)];
            
            // Verificar se já tem alvo nesta posição
            Collider[] colisoes = Physics.OverlapSphere(pontoEscolhido.position, 0.5f);
            bool ocupado = false;
            
            foreach (var col in colisoes)
            {
                if (col.GetComponent<AlvoEstatico>() != null || 
                    col.GetComponent<AlvoMovel>() != null || 
                    col.GetComponent<AlvoResistente>() != null ||
                    col.GetComponent<Alvo>() != null)
                {
                    ocupado = true;
                    break;
                }
            }
            
            // Se o ponto está livre, spawnar aqui
            if (!ocupado)
            {
                GameObject prefabEscolhido = prefabsAlvos[Random.Range(0, prefabsAlvos.Length)];
                Instantiate(prefabEscolhido, pontoEscolhido.position, Quaternion.Euler(rotacaoAlvos));
                return;
            }
        }
    }

    /// <summary>
    /// Reseta o modo precisão
    /// </summary>
    public void Resetar()
    {
        jogoIniciado = false;
        ResetarCombo();
        pontosOriginais = 0;
        
        CancelInvoke();

        if (gerenciador != null)
        {
            gerenciador.ResetarJogo();
        }

        // Destruir todos os alvos
        foreach (var alvo in FindObjectsByType<AlvoEstatico>(FindObjectsSortMode.None))
            Destroy(alvo.gameObject);
        foreach (var alvo in FindObjectsByType<AlvoMovel>(FindObjectsSortMode.None))
            Destroy(alvo.gameObject);
        foreach (var alvo in FindObjectsByType<AlvoResistente>(FindObjectsSortMode.None))
            Destroy(alvo.gameObject);
        foreach (var alvo in FindObjectsByType<Alvo>(FindObjectsSortMode.None))
            Destroy(alvo.gameObject);

        if (textoDicas != null)
        {
            textoDicas.text = "Aperte o botão do controle para começar!";
        }

        Debug.Log("Modo Precisão resetado! Aperte o botão para recomeçar.");
    }
}
