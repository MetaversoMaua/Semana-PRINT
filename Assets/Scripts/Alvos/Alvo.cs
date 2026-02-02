using UnityEngine;

public class Alvo : MonoBehaviour
{
    [Header("Configurações")]
    public bool destruirAoAtingir = true;
    
    [Header("Feedback (Opcional)")]
    public GameObject efeitoAcerto;
    public AudioClip somAcerto;

    public void Atingido()
    {
        Debug.Log("Alvo atingido!");

        // Efeito visual
        if (efeitoAcerto != null)
            Instantiate(efeitoAcerto, transform.position, Quaternion.identity);

        // Som
        if (somAcerto != null)
            AudioSource.PlayClipAtPoint(somAcerto, transform.position);

        if (destruirAoAtingir)
            Destroy(gameObject);
    }
}
