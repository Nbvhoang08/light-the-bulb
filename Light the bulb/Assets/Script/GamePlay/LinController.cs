using System.Collections.Generic;
using UnityEngine;


public class LinController : MonoBehaviour
{    
     public Transform player;
    public Transform fixedPoint;
    public LineRenderer rope;
    public LayerMask collMask;
    public float maxAngle = 60f;
    public float lineWidth = 0.1f; // Độ rộng của line để check va chạm

    public List<Vector3> ropePositions { get; set; } = new List<Vector3>();
    public List<LineColision> collidingBubs = new List<LineColision>(); // Danh sách các bub đang va chạm

    private void Awake() => AddPosToRope(fixedPoint.position);

    private void Update()
    {
        UpdateRopePositions();
        LastSegmentGoToPlayerPos();

        DetectCollisionEnter();
        if (ropePositions.Count > 2) DetectCollisionExits();
        if (ropePositions.Count > 3) RemoveSharpAngles();
        
        CheckBubCollisions(); // Thêm phương thức kiểm tra va chạm với Bub
    }

private Vector3? lastStart = null;  // Lưu điểm bắt đầu của lần check trước
private Vector3? lastEnd = null;    // Lưu điểm kết thúc của lần check trước
[SerializeField]
private LineCollisionSet currentFrameCollisionsSet = new LineCollisionSet();
private HashSet<LineColision> currentFrameCollisions = new HashSet<LineColision>();
private void CheckBubCollisions()
{
    // Reset danh sách collision của frame hiện tại
    currentFrameCollisions.Clear();

    // Reset tham chiếu nếu mất điểm bắt đầu hoặc kết thúc
    if (IsMissingReference())
    {
        ResetCollisionState();
        return;
    }

    // Kiểm tra va chạm từng đoạn dây
    for (int i = 0; i < ropePositions.Count - 1; i++)
    {
        Vector3 start = ropePositions[i];
        Vector3 end = ropePositions[i + 1];

        lastStart = start;
        lastEnd = end;

        Vector3 direction = (end - start).normalized;
        float segmentDistance = Vector3.Distance(start, end);

        // Raycast để tìm các va chạm
        RaycastHit[] hits = Physics.BoxCastAll(
            start,
            new Vector3(lineWidth, lineWidth, lineWidth) * 0.5f,
            direction,
            Quaternion.FromToRotation(Vector3.forward, direction),
            segmentDistance,
            LayerMask.GetMask("Bub")
        );

        // Debug vẽ BoxCast
        DrawDebugBox(
            start,
            new Vector3(lineWidth, lineWidth, segmentDistance),
            Quaternion.FromToRotation(Vector3.forward, direction),
            Color.yellow
        );

        // Xử lý từng va chạm
        foreach (RaycastHit hit in hits)
        {
            HandleCollision(hit, start, segmentDistance);
        }
    }

    // Cập nhật trạng thái va chạm cho các Bub
    UpdateCollisionState();
}

/// <summary>
/// Xử lý một va chạm
/// </summary>
private void HandleCollision(RaycastHit hit, Vector3 start, float segmentDistance)
{
    float distanceToHit = Vector3.Distance(start, hit.point);
    if (distanceToHit > segmentDistance) return;

    LineColision bub = hit.collider.GetComponent<LineColision>();
    if (bub == null) return;

    // Đánh dấu trạng thái va chạm
    bub.isColliding = true;
    currentFrameCollisions.Add(bub);

    // Debug điểm va chạm
    Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.2f, Color.green);
    if (bub.transform != null)
    {
        Debug.DrawLine(hit.point, bub.transform.position, Color.blue);
    }
}

/// <summary>
/// Cập nhật trạng thái va chạm
/// </summary>
private void UpdateCollisionState()
{
    // Đặt isColliding = false cho các Bub không còn va chạm
    foreach (var bub in collidingBubs)
    {
        if (bub != null && !currentFrameCollisions.Contains(bub))
        {
            bub.isColliding = false; // Thay đổi trạng thái khi va chạm kết thúc
            Debug.Log($"Collision exited for: {bub.name}");
        }
    }

    // Cập nhật danh sách collidingBubs cho frame tiếp theo
    collidingBubs.Clear();
    collidingBubs.AddRange(currentFrameCollisions);
}

/// <summary>
/// Kiểm tra nếu tham chiếu dây đã mất
/// </summary>
private bool IsMissingReference()
{
    return lastStart.HasValue && lastEnd.HasValue &&
           (!ropePositions.Contains(lastStart.Value) || !ropePositions.Contains(lastEnd.Value));
}

/// <summary>
/// Reset trạng thái va chạm khi dây không còn hợp lệ
/// </summary>
private void ResetCollisionState()
{
    foreach (var bub in collidingBubs)
    {
        if (bub != null) bub.isColliding = false;
    }
    collidingBubs.Clear();
    lastStart = null;
    lastEnd = null;
    Debug.Log("Reset collision state.");
}
// Hàm hỗ trợ vẽ debug box
private void DrawDebugBox(Vector3 center, Vector3 size, Quaternion rotation, Color color)
{
    // Lấy các vector hướng của box
    Vector3 forward = rotation * Vector3.forward * size.z/2;
    Vector3 up = rotation * Vector3.up * size.y/2;
    Vector3 right = rotation * Vector3.right * size.x/2;

    Vector3[] points = new Vector3[8];
    points[0] = center + forward + up + right;
    points[1] = center + forward + up - right;
    points[2] = center + forward - up + right;
    points[3] = center + forward - up - right;
    points[4] = center - forward + up + right;
    points[5] = center - forward + up - right;
    points[6] = center - forward - up + right;
    points[7] = center - forward - up - right;

    // Vẽ các cạnh của box
    for (int i = 0; i < 4; i++)
    {
        Debug.DrawLine(points[i], points[(i + 1) % 4], color);
        Debug.DrawLine(points[i + 4], points[((i + 1) % 4) + 4], color);
        Debug.DrawLine(points[i], points[i + 4], color);
    }
}

    // Các phương thức khác giữ nguyên
    private void DetectCollisionEnter()
    {
        RaycastHit hit;
        if (Physics.Linecast(player.position, rope.GetPosition(ropePositions.Count - 2), out hit, collMask))
        {
            ropePositions.RemoveAt(ropePositions.Count - 1);
            AddPosToRope(hit.point);
        }
    }

    private void DetectCollisionExits()
    {
        if (ropePositions.Count < 3)
        {
            return;
        }

        RaycastHit hit;
        if (!Physics.Linecast(player.position, rope.GetPosition(ropePositions.Count - 3), out hit, collMask))
        {
            ropePositions.RemoveAt(ropePositions.Count - 2);
        }
    }

    private void RemoveSharpAngles()
    {
        for (int i = ropePositions.Count - 3; i >= 1; i--)
        {
            Vector3 dir1 = ropePositions[i] - ropePositions[i - 1];
            Vector3 dir2 = ropePositions[i + 1] - ropePositions[i];

            float angle = Vector3.Angle(dir1, dir2);
            if (angle > maxAngle)
            {
                ropePositions.RemoveAt(i);
            }
        }
    }

    private void AddPosToRope(Vector3 _pos)
    {
        if (ropePositions.Contains(_pos))
        {
            return;
        }

        ropePositions.Add(_pos);
        ropePositions.Add(player.position);
    }

    private void UpdateRopePositions()
    {
        rope.positionCount = ropePositions.Count;
        rope.SetPositions(ropePositions.ToArray());
    }

    private void LastSegmentGoToPlayerPos() => rope.SetPosition(rope.positionCount - 1, player.position);
}
