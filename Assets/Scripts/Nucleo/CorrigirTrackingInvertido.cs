using UnityEngine;

/// <summary>
/// Corrige tracking invertido/espelhado.
/// Adicione este script na Main Camera (dentro do XR Origin).
/// </summary>
public class CorrigirTrackingInvertido : MonoBehaviour
{
    [Header("Inversão de Eixos")]
    public bool inverterHorizontal = true;
    public bool inverterVertical = true;

    private Quaternion rotacaoOriginal;

    private void LateUpdate()
    {
        // Captura a rotação atual
        Quaternion rotacaoAtual = transform.localRotation;

        // Converte para Euler
        Vector3 euler = rotacaoAtual.eulerAngles;

        // Inverte os eixos conforme necessário
        if (inverterHorizontal)
            euler.y = -euler.y;

        if (inverterVertical)
            euler.x = -euler.x;

        // Aplica a rotação corrigida
        transform.localRotation = Quaternion.Euler(euler);
    }
}
