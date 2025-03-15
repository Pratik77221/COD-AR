using Photon.Pun.Demo.SlotRacer.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawnAnimation : MonoBehaviour
{
    public Transform Grid;  // ����
    public float dropHeight = 2.0f; // ��ʼ�߶ȣ����������
    public float targetZ = 0.5f; // �����һ�۵ľ��루�������ʺϵľ��룩
    public float dropSpeed = 2.0f;
    public float resetDistance = 0.1f;

    private Vector3 lastCameraPosition;
    private bool isDropping = false;
    bool gridLocked = false;

    void Start()
    {
        lastCameraPosition = Camera.main.transform.position;
        // ��ʼ��������
        PlaceGridInAbove();
    }
    void Update()
    {
        if (!Grid.gameObject.activeSelf) return;
        if (gridLocked) return;

        Camera cam = Camera.main;
        Vector3 camPosition = cam.transform.position;

        // ����ˮƽλ�� (����Y�ᣬֻ����X/Z�ƶ�)
        float horizontalDistance = Vector2.Distance(
            new Vector2(camPosition.x, camPosition.z),
            new Vector2(lastCameraPosition.x, lastCameraPosition.z)
        );

        if (horizontalDistance > resetDistance)
        {
            PlaceGridInAbove();
            lastCameraPosition = camPosition;
        }

        if (isDropping)
        {
            // �½�����
            Grid.position = Vector3.Lerp(Grid.position, new Vector3(Grid.position.x, 0, Grid.position.z), Time.deltaTime * dropSpeed);
            if (Grid.position.y < 0.01f) isDropping = false;
        }

        PlaceGridInFront();

    }



    public void PlaceGrid(Vector3 planePosition)
    {
        // ȷ�����̴������
        Grid.position = new Vector3(planePosition.x, dropHeight, planePosition.z);
        Grid.gameObject.SetActive(true);
        isDropping = true;
    }

    void PlaceGridInAbove()
    {
        Camera mainCamera = Camera.main;
        Vector3 forward = mainCamera.transform.forward;  // ��ȡ�ֻ���ǰ����
        Vector3 armDistance = mainCamera.transform.position + forward * targetZ; // һ�۾��루0.5�ף�

        PlaceGrid(armDistance);
    }

    void PlaceGridInFront()
    {
        Camera mainCamera = Camera.main;
        Vector3 forward = mainCamera.transform.forward;  // ��ȡ�ֻ���ǰ����
        Vector3 armDistance = mainCamera.transform.position + forward * targetZ; // һ�۾��루0.5�ף�

        Grid.position = new Vector3(armDistance.x, Grid.position.y, armDistance.z);
    }

    void ConfirmGridPlacement()
    {
        gridLocked = true;
    }
}
