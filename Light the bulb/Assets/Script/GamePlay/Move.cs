using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float moveSpeed = 5f; // Tốc độ di chuyển
    public Joystick joystick; // Joystick được tham chiếu từ Canvas

    private Rigidbody sphereRigidbody;

    void Start()
    {
        // Lấy Rigidbody của hình cầu
        sphereRigidbody = GetComponent<Rigidbody>();
        sphereRigidbody.constraints = RigidbodyConstraints.FreezePositionZ; // Chỉ di chuyển trong mặt phẳng XY
        sphereRigidbody.constraints = RigidbodyConstraints.FreezeRotation; // Chỉ di chuyển trong mặt phẳng XY
        sphereRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // Giảm lỗi xuyên qua vật thể

        if (joystick == null)
        {
            joystick = FindObjectOfType<Joystick>();
        }
    }

    void Update()
    {
        if (joystick == null)
        {
            joystick = FindObjectOfType<Joystick>();
        }
    }

    void FixedUpdate()
    {
        if (joystick == null) return;

        // Lấy giá trị từ joystick
        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;

        // Tạo vector di chuyển trong mặt phẳng XY
        Vector3 moveDirection = new Vector3(horizontal, vertical, 0f);
        // Áp dụng vận tốc để di chuyển hình cầu
        sphereRigidbody.velocity = moveDirection * moveSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Kiểm tra va chạm với object có tag là "Bub"
        if (collision.gameObject.CompareTag("Bub"))
        {
            // Dừng chuyển động nếu va chạm
            sphereRigidbody.velocity = Vector3.zero;
        }
    }
}
