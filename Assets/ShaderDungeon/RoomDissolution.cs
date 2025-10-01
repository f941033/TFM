using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomDissolution : MonoBehaviour
{
    [Header("Dissolution Settings")]
    public Material dissolutionMaterial; // Material base (shared)
    public float dissolutionDuration = 2f;
    public AnimationCurve dissolutionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip dissolutionSound;

    private TilemapRenderer tilemapRenderer;
    private Material materialInstance; // NUEVA VARIABLE: instancia única
    private bool isDissolving = false;

    void Start()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();

        // ESTO ES LO CLAVE: Crear una instancia única del material para esta sala
        if (dissolutionMaterial != null)
        {
            materialInstance = new Material(dissolutionMaterial); // Crear copia
            tilemapRenderer.material = materialInstance; // Usar la copia
            materialInstance.SetFloat("_DissolveAmount", 0f);
        }
    }

    public void StartDissolution()
    {
        if (!isDissolving && materialInstance != null)
        {
            StartCoroutine(DissolveCoroutine());
        }
    }

    IEnumerator DissolveCoroutine()
    {
        isDissolving = true;

        // Reproducir sonido
        if (audioSource && dissolutionSound)
        {
            audioSource.PlayOneShot(dissolutionSound);
        }

        float elapsedTime = 0f;
        while (elapsedTime < dissolutionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / dissolutionDuration;

            // Usar curva de animación para suavizar
            float dissolveValue = dissolutionCurve.Evaluate(progress);

            // USAR LA INSTANCIA ÚNICA, NO EL MATERIAL COMPARTIDO
            materialInstance.SetFloat("_DissolveAmount", dissolveValue);

            yield return null;
        }

        // Asegurar que termine completamente
        materialInstance.SetFloat("_DissolveAmount", 1f);

        // Desactivar el GameObject después de la disolución
        gameObject.SetActive(false);
        isDissolving = false;
    }

    [ContextMenu("Test Dissolution")]
    public void TestDissolution()
    {
        StartDissolution();
    }

    // IMPORTANTE: Limpiar la instancia cuando se destruya el objeto
    void OnDestroy()
    {
        if (materialInstance != null)
        {
            DestroyImmediate(materialInstance);
        }
    }
}
