using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Modo Livre - Treino infinito sem limites de tempo ou munição.
/// Perfeito para iniciantes praticarem e aprenderem os controles.
/// </summary>
public class ModoLivre : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Ativar este modo de jogo")]
    public bool ativo = true;
    
    [Tooltip("Iniciar automaticamente ou aguardar botão")]
    public bool iniciarAutomaticamente = false;
    
    [Header("Spawn de Alvos")]
    [Tooltip("Respawnar alvos automaticamente quando destruídos")]
    public bool respawnarAlvos = true;
    
    [Tooltip("Tempo para respawnar (segundos)")]
    public float tempoRespawn = 3f;
    
    [Tooltip("Lista de prefabs de alvos para spawnar")]
    public GameObject[] prefabsAlvos;
    
    [Tooltip("Posições onde alvos podem spawnar")]
    public Transform[] pontosSpawn;
    
    [Tooltip("Rotação dos alvos ao spawnar (X, Y, Z em graus). Ajuste conforme os modelos 3D.")]
    public Vector3 rotacaoAlvos = new Vector3(0, 0, 0);
    
    [Header("UI")]
    [Tooltip("Texto para mostrar informações do modo")]
    public TMP_Text textoModo;
    
    [Tooltip("Texto para mostrar dicas")]
    public TMP_Text textoDicas;

    private GerenciadorJogo gerenciador;
    private int alvosAtivos = 0;
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
            Debug.LogWarning("ModoLivre: GerenciadorJogo não encontrado na cena. Crie um GameObject com o script GerenciadorJogo.");
        }
        
        if (textoModo != null)
        {
            textoModo.text = "MODO: LIVRE";
        }
        
        if (textoDicas != null)
        {
            if (iniciarAutomaticamente)
            {
                textoDicas.text = "Pratique à vontade! Sem limites de tempo ou munição.";
            }
            else
            {
                textoDicas.text = "Aperte o botão do controle para começar!";
            }
        }

        // Validar configurações
        if (prefabsAlvos == null || prefabsAlvos.Length == 0)
        {
            Debug.LogError("ModoLivre: Nenhum prefab de alvo configurado! Adicione prefabs no array 'prefabsAlvos'.");
            return;
        }
        
        if (pontosSpawn == null || pontosSpawn.Length == 0)
        {
            Debug.LogError("ModoLivre: Nenhum ponto de spawn configurado! Adicione Transforms no array 'pontosSpawn'.");
            return;
        }

        // Iniciar automaticamente ou aguardar botão
        if (iniciarAutomaticamente)
        {
            IniciarJogo();
        }

        Debug.Log("Modo Livre pronto! " + (iniciarAutomaticamente ? "Jogo iniciado." : "Aperte o botão para começar."));
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

        // Verificar se precisa spawnar mais alvos
        if (respawnarAlvos)
        {
            alvosAtivos = ContarAlvosAtivos();
            
            // Se tiver menos alvos que pontos de spawn, spawnar mais
            if (alvosAtivos < pontosSpawn.Length)
            {
                SpawnarAlvosNecessarios();
            }
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

        if (gerenciador != null)
        {
            gerenciador.ResetarJogo();
        }

        if (textoDicas != null)
        {
            textoDicas.text = "Pratique à vontade! Sem limites de tempo ou munição.";
        }

        // Spawnar alvos iniciais
        if (respawnarAlvos)
        {
            foreach (Transform ponto in pontosSpawn)
            {
                if (ponto != null)
                {
                    SpawnarAlvoAleatorio(ponto.position);
                }
            }
        }

        Debug.Log("Modo Livre iniciado!");
    }

    private void SpawnarAlvosNecessarios()
    {
        foreach (Transform ponto in pontosSpawn)
        {
            // Verificar se já tem alvo próximo deste ponto
            bool temAlvoProximo = false;
            Collider[] colisoes = Physics.OverlapSphere(ponto.position, 0.5f);
            
            foreach (var col in colisoes)
            {
                if (col.GetComponent<AlvoEstatico>() != null || 
                    col.GetComponent<AlvoMovel>() != null || 
                    col.GetComponent<AlvoResistente>() != null ||
                    col.GetComponent<Alvo>() != null)
                {
                    temAlvoProximo = true;
                    break;
                }
            }

            // Se não tem alvo, spawnar um novo após delay
            if (!temAlvoProximo)
            {
                Invoke(nameof(SpawnarNoProximoPonto), tempoRespawn);
                return; // Spawna um por vez
            }
        }
    }

    private void SpawnarNoProximoPonto()
    {
        foreach (Transform ponto in pontosSpawn)
        {
            Collider[] colisoes = Physics.OverlapSphere(ponto.position, 0.5f);
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

            if (!ocupado)
            {
                SpawnarAlvoAleatorio(ponto.position);
                return;
            }
        }
    }

    private void SpawnarAlvoAleatorio(Vector3 posicao)
    {
        if (prefabsAlvos.Length == 0) return;

        // Verificar se já tem alvo nesta posição
        Collider[] colisoes = Physics.OverlapSphere(posicao, 0.5f);
        foreach (var col in colisoes)
        {
            if (col.GetComponent<AlvoEstatico>() != null || 
                col.GetComponent<AlvoMovel>() != null || 
                col.GetComponent<AlvoResistente>() != null ||
                col.GetComponent<Alvo>() != null)
            {
                // Já tem alvo aqui, não spawnar
                return;
            }
        }

        GameObject prefabEscolhido = prefabsAlvos[Random.Range(0, prefabsAlvos.Length)];
        Instantiate(prefabEscolhido, posicao, Quaternion.Euler(rotacaoAlvos));
    }

    /// <summary>
    /// Reseta o modo livre
    /// </summary>
    public void Resetar()
    {
        jogoIniciado = false;

        if (gerenciador != null)
        {
            gerenciador.ResetarJogo();
        }

        // Destruir todos os alvos
        DestruirTodosAlvos();

        if (textoDicas != null)
        {
            textoDicas.text = "Aperte o botão do controle para começar!";
        }

        Debug.Log("Modo Livre resetado! Aperte o botão para recomeçar.");
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
}
