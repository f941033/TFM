using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomDissolution : MonoBehaviour
{
    [Header("Dissolution Settings")]
    public Material dissolutionMaterial;
    public float dissolutionDuration = 2f;
    public AnimationCurve dissolutionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip dissolutionSound;

    private TilemapRenderer tilemapRenderer;
    private bool isDissolving = false;

    void Start()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();

        // Asignar el material de disolución
        if (dissolutionMaterial != null)
        {
            tilemapRenderer.material = dissolutionMaterial;
            dissolutionMaterial.SetFloat("_DissolveAmount", 0f);
        }
    }

    public void StartDissolution()
    {
        if (!isDissolving && dissolutionMaterial != null)
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
            dissolutionMaterial.SetFloat("_DissolveAmount", dissolveValue);

            yield return null;
        }

        // Asegurar que termine completamente
        dissolutionMaterial.SetFloat("_DissolveAmount", 1f);

        // Desactivar el GameObject después de la disolución
        gameObject.SetActive(false);

        isDissolving = false;
    }

    [ContextMenu("Test Dissolution")]
    public void TestDissolution()
    {
        StartDissolution();
    }
}
