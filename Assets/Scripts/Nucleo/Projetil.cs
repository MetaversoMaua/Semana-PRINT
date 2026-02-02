using UnityEngine;

public class Projetil : MonoBehaviour
{
    public float tempoDeVida = 5f;

    void Start()
    {
        // Garantir detecção de colisão contínua
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // Registrar tiro no gerenciador
        var gerenciador = FindFirstObjectByType<GerenciadorJogo>();
        if (gerenciador != null)
        {
            gerenciador.RegistrarTiro();
        }
        
        Destroy(gameObject, tempoDeVida);
    }

    private void OnCollisionEnter(Collision colisao)
    {
        // Tentar detectar qualquer tipo de alvo
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

        Destroy(gameObject);
    }
}
