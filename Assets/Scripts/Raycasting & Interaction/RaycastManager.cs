using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // ����������ϵͳ

public class RaycastManager : MonoBehaviour
{
    void Update()
    {
        // ��ⴥ�����루��׿�豸��
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            ProcessRaycast(touchPos);
        }
        // �༭����ʹ��������
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            ProcessRaycast(mousePos);
        }
    }

    void ProcessRaycast(Vector2 screenPos)
    {
        // �����������������
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 1.0f); // ���Կ��ӻ�
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Object Detected: " + hit.collider.gameObject.name);
            // ʾ�����ı䱻����������ɫ
            Renderer rend = hit.collider.gameObject.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = Color.red;
            }
            // ���ý����߼������紥�������������ȣ�
            HandleInteraction(hit.collider.gameObject);
        }
    }

    void HandleInteraction(GameObject target)
    {
        // �ж�Ŀ���Ƿ����ض���ǩ������"Interactable"������ִ�ж����߼�
        if (target.CompareTag("Interactable"))
        {
            // ����Ŀ���ϵĶ�����ȷ��Ŀ������Animator�������Ӧ������
            Animator animator = target.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("OnInteract");
            }
            // ���Ž���������ȷ��������AudioSource�������Ƶ������
            AudioSource audio = target.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }
            // ���Ե�����Ч������׿֧�֣�
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
            // ���������߼����������UI�������ӳɡ�������
            UIManager.Instance.ShowMessage("Object clicked: " + target.name);
        }
    }
}
