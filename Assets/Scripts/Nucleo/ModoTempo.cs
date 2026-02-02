using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Modo Tempo - Desafio cronometrado para destruir o máximo de alvos possível.
/// Quanto mais alvos destruídos no tempo, maior a pontuação.
/// </summary>
public class ModoTempo : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Ativar este modo de jogo")]
    public bool ativo = false;
    
    [Header("Tempo")]
    [Tooltip("Duração do desafio em segundos")]
    public float duracaoTotal = 60f;
    
    private float tempoRestante;
    private bool jogoAtivo = false;
    private bool jogoTerminado = false;
    
    [Header("Spawn de Alvos")]
    [Tooltip("Prefabs de alvos para spawnar")]
    public GameObject[] prefabsAlvos;
    
    [Tooltip("Posições onde alvos podem spawnar")]
    public Transform[] pontosSpawn;
    
    [Tooltip("Rotação dos alvos ao spawnar (X, Y, Z em graus). Ajuste conforme os modelos 3D.")]
    public Vector3 rotacaoAlvos = new Vector3(0, 0, 0);
    
    [Tooltip("Intervalo entre spawns (segundos)")]
    public float intervaloSpawn = 2f;
    
    [Tooltip("Número máximo de alvos na cena ao mesmo tempo")]
    public int maxAlvosSimultaneos = 5;
    
    private float proximoSpawn = 0f;
    
    [Header("UI")]
    [Tooltip("Texto para mostrar o cronômetro")]
    public TMP_Text textoCronometro;
    
    [Tooltip("Texto para mostrar informações do modo")]
    public TMP_Text textoModo;
    
    [Tooltip("Texto para mostrar resultado final")]
    public TMP_Text textoResultado;
    
    [Tooltip("Painel de resultado final")]
    public GameObject painelResultado;

    private GerenciadorJogo gerenciador;

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
            Debug.LogWarning("ModoTempo: GerenciadorJogo não encontrado na cena.");
        }
        
        if (textoModo != null)
        {
            textoModo.text = "MODO: CONTRA O TEMPO";
        }
        
        if (painelResultado != null)
        {
            painelResultado.SetActive(false);
        }
        
        // Validar configurações
        if (prefabsAlvos == null || prefabsAlvos.Length == 0)
        {
            Debug.LogError("ModoTempo: Nenhum prefab de alvo configurado!");
            return;
        }
        
        if (pontosSpawn == null || pontosSpawn.Length == 0)
        {
            Debug.LogError("ModoTempo: Nenhum ponto de spawn configurado!");
            return;
        }

        Debug.Log("Modo Tempo pronto! Aperte o botão do controle para começar.");
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

    private bool VerificarBotaoReiniciar()
    {
        return VerificarBotaoIniciar(); // Mesmo botão para reiniciar
    }

    private void Update()
    {
        // Verificar input para iniciar/reiniciar jogo
        if (!jogoAtivo && jogoTerminado)
        {
            if (VerificarBotaoReiniciar())
            {
                Reiniciar();
            }
            return;
        }
        
        if (!jogoAtivo && !jogoTerminado)
        {
            if (VerificarBotaoIniciar())
            {
                IniciarJogo();
            }
            return;
        }

        if (!ativo || !jogoAtivo) return;

        // Atualizar cronômetro
        tempoRestante -= Time.deltaTime;
        
        if (textoCronometro != null)
        {
            int minutos = Mathf.FloorToInt(tempoRestante / 60f);
            int segundos = Mathf.FloorToInt(tempoRestante % 60f);
            textoCronometro.text = $"TEMPO: {minutos:00}:{segundos:00}";
            
            // Mudar cor quando tempo está acabando
            if (tempoRestante <= 10f)
            {
                textoCronometro.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 2f, 1f));
            }
        }

        // Verificar se acabou o tempo
        if (tempoRestante <= 0f)
        {
            TerminarJogo();
            return;
        }

        // Spawnar alvos periodicamente
        if (Time.time >= proximoSpawn)
        {
            int alvosAtivos = ContarAlvosAtivos();
            
            if (alvosAtivos < maxAlvosSimultaneos && prefabsAlvos.Length > 0 && pontosSpawn.Length > 0)
            {
                SpawnarAlvoAleatorio();
                proximoSpawn = Time.time + intervaloSpawn;
            }
        }
    }

    private void IniciarJogo()
    {
        tempoRestante = duracaoTotal;
        jogoAtivo = true;
        jogoTerminado = false;
        proximoSpawn = Time.time + 1f; // Primeiro spawn após 1 segundo

        if (gerenciador != null)
        {
            gerenciador.ResetarJogo();
        }
        
        if (textoCronometro != null)
        {
            textoCronometro.color = Color.white;
        }

        Debug.Log($"Modo Tempo iniciado! Destrua o máximo de alvos em {duracaoTotal} segundos!");
    }

    private void TerminarJogo()
    {
        if (jogoTerminado) return;
        
        jogoAtivo = false;
        jogoTerminado = true;

        // Parar spawns
        CancelInvoke();
        
        // Destruir todos os alvos
        DestruirTodosAlvos();

        // Mostrar resultado
        if (painelResultado != null)
        {
            painelResultado.SetActive(true);
        }

        if (textoResultado != null && gerenciador != null)
        {
            float precisao = gerenciador.CalcularPrecisao();
            textoResultado.text = $"TEMPO ESGOTADO!\n\n" +
                                 $"Pontos: {gerenciador.pontos}\n" +
                                 $"Alvos Destruídos: {gerenciador.alvosDestruidos}\n" +
                                 $"Precisão: {precisao:F1}%\n\n" +
                                 $"Aperte o botão para jogar novamente";
        }

        if (textoCronometro != null)
        {
            textoCronometro.text = "TEMPO: 00:00";
            textoCronometro.color = Color.red;
        }

        Debug.Log("Modo Tempo terminado! Aperte o botão para jogar novamente.");
    }

    private void DestruirTodosAlvos()
    {
        foreach (var alvo in FindObjectsByType<AlvoEstatico>(FindObjectsSortMode.None))
            Destroy(alvo.gameObject);
        foreach (var alvo in FindObjectsByType<AlvoMovel>(FindObjectsSortMode.None))
            Destroy(alvo.gameObject);
        foreach (var alvo in FindObjectsByType<AlvoResistente>(FindObjectsSortMode.None))
            Destroy(alvo.gameObject);
        foreach (var alvo in FindObjectsByType<Alvo>(FindObjectsSortMode.None))
            Destroy(alvo.gameObject);
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
            // Escolher ponto de spawn aleatório
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
        
        // Se chegou aqui, todos os pontos estão ocupados
        // Não spawna nada
    }

    /// <summary>
    /// Reinicia o modo tempo
    /// </summary>
    public void Reiniciar()
    {
        if (painelResultado != null)
        {
            painelResultado.SetActive(false);
        }

        // Destruir todos os alvos
        DestruirTodosAlvos();

        IniciarJogo();
    }

    /// <summary>
    /// Adiciona tempo extra (pode ser usado como power-up)
    /// </summary>
    public void AdicionarTempo(float segundos)
    {
        tempoRestante += segundos;
        Debug.Log($"+{segundos} segundos!");
    }
}
