using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gerenciador central do jogo - controla pontuação, alvos destruídos, etc.
/// Permite aos estudantes visualizar o progresso.
/// </summary>
public class GerenciadorJogo : MonoBehaviour
{
    [Header("Pontuação")]
    [Tooltip("Pontos atuais do jogador")]
    public int pontos = 0;
    
    [Tooltip("Texto UI para mostrar pontos - TextMeshPro (recomendado)")]
    public TMP_Text textoPontosTMP;
    
    [Tooltip("Texto UI para mostrar pontos - Text antigo (legado)")]
    public Text textoPontos;
    
    [Header("Estatísticas")]
    [Tooltip("Número de alvos destruídos")]
    public int alvosDestruidos = 0;
    
    [Tooltip("Número de tiros disparados")]
    public int tirosDisparados = 0;
    
    [Tooltip("Texto UI para mostrar estatísticas - TextMeshPro")]
    public TMP_Text textoEstatisticasTMP;
    
    [Tooltip("Texto UI para mostrar estatísticas - Text antigo")]
    public Text textoEstatisticas;

    private void Start()
    {
        // Atualizar UI imediatamente
        AtualizarUI();
        
        // Atualizar UI periodicamente para garantir que sempre está sincronizado
        InvokeRepeating(nameof(AtualizarUI), 0.5f, 0.5f);
    }

    /// <summary>
    /// Adiciona pontos à pontuação total
    /// </summary>
    public void AdicionarPontos(int valor)
    {
        pontos += valor;
        AtualizarUI();
        Debug.Log($"Pontos: {pontos} (+{valor})");
    }

    /// <summary>
    /// Registra que um alvo foi destruído
    /// </summary>
    public void RegistrarAlvoDestruido()
    {
        alvosDestruidos++;
        AtualizarUI();
    }

    /// <summary>
    /// Registra que um tiro foi disparado
    /// </summary>
    public void RegistrarTiro()
    {
        tirosDisparados++;
        AtualizarUI();
    }

    /// <summary>
    /// Calcula a precisão do jogador
    /// </summary>
    public float CalcularPrecisao()
    {
        if (tirosDisparados == 0) return 0f;
        return (float)alvosDestruidos / tirosDisparados * 100f;
    }

    /// <summary>
    /// Reseta todas as estatísticas
    /// </summary>
    public void ResetarJogo()
    {
        pontos = 0;
        alvosDestruidos = 0;
        tirosDisparados = 0;
        AtualizarUI();
        Debug.Log("Jogo resetado!");
    }

    private void AtualizarUI()
    {
        // Atualizar texto de pontos (TextMeshPro)
        if (textoPontosTMP != null)
        {
            textoPontosTMP.text = $"PONTOS: {pontos}";
        }
        
        // Atualizar texto de pontos (Text antigo)
        if (textoPontos != null)
        {
            textoPontos.text = $"PONTOS: {pontos}";
        }

        // Atualizar texto de estatísticas (TextMeshPro)
        if (textoEstatisticasTMP != null)
        {
            float precisao = CalcularPrecisao();
            textoEstatisticasTMP.text = $"Alvos: {alvosDestruidos}\nTiros: {tirosDisparados}\nPrecisão: {precisao:F1}%";
        }
        
        // Atualizar texto de estatísticas (Text antigo)
        if (textoEstatisticas != null)
        {
            float precisao = CalcularPrecisao();
            textoEstatisticas.text = $"Alvos: {alvosDestruidos}\nTiros: {tirosDisparados}\nPrecisão: {precisao:F1}%";
        }
    }
}
