using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camfollow : MonoBehaviour
{
   public Transform target; // Đối tượng mà camera sẽ theo dõi
    public Vector3 offset; // Khoảng cách giữa camera và đối tượng
    public float smoothSpeed = 0.125f; // Tốc độ mượt mà khi theo dõi

    void LateUpdate()
    {
        if (target == null) return;

        // Vị trí mong muốn của camera
        Vector3 desiredPosition = target.position + offset;
        // Vị trí mượt mà giữa vị trí hiện tại và vị trí mong muốn
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // Đặt vị trí của camera
        transform.position = smoothedPosition;

        // Đảm bảo camera luôn nhìn vào đối tượng
        transform.LookAt(target);
    }
}
