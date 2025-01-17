using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine3D : MonoBehaviour
{
     public LineRenderer3D lineRenderer3D; // Tham chiếu tới LineRenderer3D
    public Transform target; // Object mà đường sẽ theo dõi
    public float updateInterval = 0.1f; // Thời gian cập nhật để thêm điểm mới (giây)
    public float pointSpacing = 0.1f; // Khoảng cách tối thiểu giữa các điểm
    public float thickness = 0.1f; // Độ dày của đường vẽ

    private float lastUpdateTime;
    private Vector3 lastPoint;

    void Start()
    {
        if (lineRenderer3D == null)
        {
            Debug.LogError("LineRenderer3D chưa được gán!");
            return;
        }

        // Khởi tạo LineRenderer3D với một số lượng điểm ban đầu (tối thiểu là 1 điểm)
        lineRenderer3D.SetPositions(1);
        if (target != null)
        {
            AddPoint(target.position);
        }
    }

    void Update()
    {
        if (target == null || lineRenderer3D == null) return;

        // Thêm điểm mới khi target di chuyển đủ khoảng cách hoặc sau một khoảng thời gian
        if (Time.time - lastUpdateTime >= updateInterval && Vector3.Distance(target.position, lastPoint) >= pointSpacing)
        {
            AddPoint(target.position);
        }

        // Cập nhật đường vẽ
        lineRenderer3D.BeginGenerationAutoComplete();
    }

    private void AddPoint(Vector3 position)
    {
        lastUpdateTime = Time.time;
        lastPoint = position;

        // Thêm điểm mới vào LineRenderer3D
        lineRenderer3D.AddPoint(position, thickness);
    }
}
