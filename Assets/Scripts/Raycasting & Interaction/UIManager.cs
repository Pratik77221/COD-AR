using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public TextMeshProUGUI messageText; // ���� Inspector �н����ֶΰ󶨵���� UI Text ����

    [Header("Animation Parameters")]
    public float fadeDuration = 0.5f;            // �ı�����ʱ��

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowMessage(string message, float duration = 2f)
    {
        if (messageText != null)
        {
            StopAllCoroutines();
            messageText.text = message;
            messageText.alpha = 1f;
            StartCoroutine(FadeText(duration));
        }
        else
        {
            Debug.LogWarning("messageText unbound��");
        }
    }

    private IEnumerator FadeText(float delay)
    {
        yield return new WaitForSeconds(delay);

        // �ı�����Ч��
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            messageText.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        messageText.text = "";
    }
}
