using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem; // ������ϵͳ������ʹ�ã�

public class ARRaycastHandler : MonoBehaviour
{
    [Header("ARComponent")]
    public ARRaycastManager arRaycastManager; // ���� Inspector �а� AR Session Origin �ϵ� ARRaycastManager
    public ARPlaneManager arPlaneManager;        // �� AR Session Origin �ϵ� ARPlaneManager

    [Header("InteractionEffect")]
    public GameObject placementIndicatorPrefab;        // ƽ��ָʾ��Ԥ�Ƽ�
    public ParticleSystem hitEffect;             // ����ʱ��������Ч
    public AudioClip successSound;               // ������Ч
    public GameObject objectToPlace;             // �ɷ��õ�����Ԥ�Ƽ�

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool isPlaneDetected = false;  // ���ƽ���Ƿ��ѱ����
    private GameObject placementIndicator;
    private Touch touch1, touch2; 
    private GameObject selectedObject;

    private void Start()
    {
        UIManager.Instance.ShowMessage("Please scan the image and click on the screen interaction!", 5f);
        arPlaneManager.planesChanged += OnPlanesChanged;

        // ʵ���� placementIndicator ����ʼ��Ϊ����
        if (placementIndicatorPrefab != null)
        {
            placementIndicator = Instantiate(placementIndicatorPrefab);
            placementIndicator.SetActive(false);
        }
    }

    void Update()
    {
        // ʹ��������ϵͳ��ⴥ��
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

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // ���״μ�⵽ƽ��ʱ���� UI
        if (args.added.Count > 0 && !isPlaneDetected)
        {
            isPlaneDetected = true;
            UIManager.Instance.ShowMessage("Plane detected! You can now place objects.", 5f);

            // ���������ɵ�ƽ��
            foreach (var plane in arPlaneManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
        }
    }

    void ProcessRaycast(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        // ִ�� AR ���߼�⣬ֻ�����ʶ���ƽ��
        if (arRaycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // ��ѡ���� placementIndicator �ƶ�����⵽��ƽ����
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }

            //  ����������Ч����Ч
            if (hitEffect != null)
            {
                float distance = Vector3.Distance(Camera.main.transform.position, hitPose.position);
                var mainModule = hitEffect.main;
                mainModule.startSize = Mathf.Clamp(0.1f * distance, 0.5f, 2f);
                ParticleSystem effect = Instantiate(hitEffect, hitPose.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration); // ������Ϻ��Զ�����
            }

            if (successSound != null)
            {
                AudioSource.PlayClipAtPoint(successSound, hitPose.position);
            }

            // �������壨����չΪ���������ȣ�
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

            // ��ʾ��������
            UIManager.Instance.ShowMessage("The object has been placed! Location: " + hitPose.position.ToString("F2"));
        }
        else
        {
            // ���û�м�⵽ƽ�棬�������� placementIndicator ����ʾ��ʾ
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
            UIManager.Instance.ShowMessage("No plane detected, please move the device.");
        }
    }
    private void OnDestroy()
    {
        // ȡ���¼�����
        arPlaneManager.planesChanged -= OnPlanesChanged;
    }
}
