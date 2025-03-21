using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem; // ������ϵͳ

public class ImageRaycastHandler : MonoBehaviour
{
    [Header("AR Components")]
    public ARRaycastManager arRaycastManager;        // ���� Inspector �а� XR Origin �ϵ� ARRaycastManager
    public ARTrackedImageManager arTrackedImageManager; // ���� Inspector �а� XR Origin �ϵ� ARTrackedImageManager

    [Header("Interaction Effects")]
    public GameObject placementIndicatorPrefab;      // ����ָʾ��⵽��Ŀ��λ�õ�Ԥ�Ƽ�
    public ParticleSystem hitEffect;                 // ����ʱ���ŵ�������Ч
    public AudioClip successSound;                   // ������Ч
    public GameObject objectToPlace;                 // ���к�Ҫ���õ�����Ԥ�Ƽ�

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject placementIndicator;
    private Touch touch1, touch2;
    private GameObject selectedObject;

    private void Start()
    {
        // ��� arTrackedImageManager �Ƿ����� Inspector �а�
        if (arTrackedImageManager == null)
        {
            Debug.LogError("ARTrackedImageManager is not assigned. Please assign it in the Inspector.");
            return;
        }

        // ����ͼƬĿ����¼�
        arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        // �����Ԥ�Ƶ� placementIndicator����ʵ��������ʼ����
        if (placementIndicatorPrefab != null)
        {
            placementIndicator = Instantiate(placementIndicatorPrefab);
            placementIndicator.SetActive(false);
        }

        // ��ʼUI��ʾ��ͨ�� UIManager ��ʾ��
        UIManager.Instance.ShowMessage("Please scan the image target and tap to interact!", 5f);
    }

    private void Update()
    {
        // ʹ��������ϵͳ��ⴥ������׿�豸��
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            ProcessRaycast(touchPos);
        }
        // �༭����ʹ�������е���
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            ProcessRaycast(mousePos);
        }

        // ˫ָ����
        if (Input.touchCount == 2)
        {
            touch1 = Input.GetTouch(0);
            touch2 = Input.GetTouch(1);
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
            Vector2 prevPos2 = touch2.position - touch2.deltaPosition;
            float prevDistance = (prevPos1 - prevPos2).magnitude;
            float currentDistance = (touch1.position - touch2.position).magnitude;
            float scaleFactor = currentDistance / prevDistance;
            if (selectedObject != null)
            {
                selectedObject.transform.localScale *= scaleFactor; // ����
            }
        }
    }

    // ��ͼƬĿ����״̬�����仯ʱ����
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // ����⵽��ͼƬĿ��ʱ����ʾ��ʾ��ָʾ��
        foreach (var trackedImage in eventArgs.added)
        {
            UIManager.Instance.ShowMessage("Image target detected: " + trackedImage.referenceImage.name + "You can now place objects.", 3f);
            // ��ѡ���� placementIndicator �ƶ���ͼƬĿ��λ��
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.position = trackedImage.transform.position;
                placementIndicator.transform.rotation = trackedImage.transform.rotation;
            }
        }
        // ����Ը�����Ҫ���� updated �� removed ״̬
    }

    void ProcessRaycast(Vector2 screenPos)
    {
        // ����һ������
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 1.0f); // �� Scene ��ͼ�п��ӻ�

        RaycastHit hit;
        // ʹ�� Physics.Raycast ��ⴥ���Ƿ�����˴� Collider �Ķ���
        if (Physics.Raycast(ray, out hit))
        {
            // �жϸö����Ƿ�����ͼƬĿ�꣨����ͼƬĿ��Ԥ�Ƽ��ϴ��� ARTrackedImage ����������丸�����У�
            ARTrackedImage trackedImage = hit.collider.GetComponentInParent<ARTrackedImage>();
            if (trackedImage != null)
            {
                // ��ȡ���߻��е�λ����Ϊ������
                Pose hitPose = new Pose(hit.point, hit.collider.transform.rotation);

                // ���� placementIndicator����������ˣ�
                if (placementIndicator != null)
                {
                    placementIndicator.SetActive(true);
                    placementIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                }

                // ����������Ч
                if (hitEffect != null)
                {
                    float distance = Vector3.Distance(Camera.main.transform.position, hitPose.position);
                    var mainModule = hitEffect.main;
                    mainModule.startSize = Mathf.Clamp(0.1f * distance, 0.5f, 2f);
                    ParticleSystem effect = Instantiate(hitEffect, hitPose.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, effect.main.duration); // ������Ϻ��Զ�����
                }

                // ������Ч
                if (successSound != null)
                {
                    AudioSource.PlayClipAtPoint(successSound, hitPose.position);
                }

                // �������壨����һ��������Ʒ��
                if (objectToPlace != null)
                {
                    Instantiate(objectToPlace, hitPose.position, Quaternion.identity);
                }

                // �𶯷��� 
                if (Application.platform == RuntimePlatform.Android)
                    Handheld.Vibrate(); // ��׿����

                // �ƶ�ѡ�е�����
                if (Physics.Raycast(ray, out hit))
                {
                    if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                    {
                        selectedObject = hit.collider.gameObject;
                        selectedObject.transform.position = hitPose.position; // ʵʱ����λ��
                    }
                    else
                    {
                        selectedObject = null;
                    }
                }

                // ���� UI ״̬
                UIManager.Instance.ShowMessage("Image target raycast hit: " + trackedImage.referenceImage.name);
                return;
            }
        }

        // �������û�л���ͼƬĿ��
        if (placementIndicator != null)
        {
            placementIndicator.SetActive(false);
        }
        UIManager.Instance.ShowMessage("No image target detected.");
    }

    private void OnDestroy()
    {
        if (arTrackedImageManager != null)
            arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
}