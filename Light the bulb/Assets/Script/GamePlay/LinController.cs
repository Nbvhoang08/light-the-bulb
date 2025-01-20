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

    public List<Vector3> ropePositions  = new List<Vector3>();
    public List<LineColision> collidingBubs = new List<LineColision>(); // Danh sách các bub đang va chạm

    private void Awake() => AddPosToRope(fixedPoint.position);
     void Start()
     {
          UpdateRopePositions();
     }
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

    private HashSet<LineColision> currentFrameCollisions = new HashSet<LineColision>();
    private void CheckBubCollisions()
    {
       // Reset tham chiếu nếu mất điểm bắt đầu hoặc kết thúc
        currentFrameCollisions.Clear();
        if (IsMissingReference())
        {
            ResetCollisionState();
            return;
        }

        // Kiểm tra nếu ropePositions chỉ có 2 điểm
        if (ropePositions.Count == 2)
        {
            // Nếu chỉ có 2 điểm, bắn raycast từ điểm đầu đến điểm cuối
            Vector3 start = ropePositions[0];
            Vector3 end = player.position;

            Vector3 direction = (end - start).normalized;
            float segmentDistance = Vector3.Distance(start, end);

            // Raycast để kiểm tra va chạm
            RaycastHit[] hits = Physics.BoxCastAll(
                start,
                new Vector3(lineWidth, lineWidth, lineWidth) * 0.5f, // Sử dụng chiều rộng dây line
                direction,
                Quaternion.FromToRotation(Vector3.forward, direction),
                segmentDistance,
                LayerMask.GetMask("Bub")
            );

            // Xử lý va chạm nếu có
            foreach (RaycastHit hit in hits)
            {
                if (hit.point != null)
                {
                    HandleCollision(hit, start, segmentDistance);
                }
            }
            UpdateCollisionState();
            return; // Khi chỉ có 2 điểm, không cần kiểm tra thêm
        }

        // Kiểm tra va chạm cho các đoạn dây có nhiều hơn 2 điểm
        for (int i = 0; i < ropePositions.Count - 1; i++)
        {
            Vector3 start = ropePositions[i];
            Vector3 end = ropePositions[i + 1];

            lastStart = start;
            lastEnd = end;

            Vector3 direction = (end - start).normalized;
            float segmentDistance = Vector3.Distance(start, end);

            // Raycast để kiểm tra va chạm
            RaycastHit[] hits = Physics.BoxCastAll(
                start,
                new Vector3(lineWidth, lineWidth, lineWidth) * 0.5f, 
                direction,
                Quaternion.FromToRotation(Vector3.forward, direction),
                segmentDistance,
                LayerMask.GetMask("Bub")
            );

            // Xử lý va chạm nếu có
            foreach (RaycastHit hit in hits)
            {
                if (hit.point != null)
                {
                    HandleCollision(hit, start, segmentDistance);
                }
            }
        }

            // Cập nhật trạng thái va chạm cho các Bub
            UpdateCollisionState();
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
                bub.IsColliding = false; // Thay đổi trạng thái khi va chạm kết thúc
                
            }
        }
        // Cập nhật danh sách collidingBubs cho frame tiếp theo
        collidingBubs.Clear();
        collidingBubs.AddRange(currentFrameCollisions);
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
        bub.IsColliding = true;
        currentFrameCollisions.Add(bub);
    }

    private bool IsMissingReference()
    {
        if (lastStart.HasValue && lastEnd.HasValue)
        {
            bool isStartMissing = !ropePositions.Contains(lastStart.Value);
            bool isEndMissing = !ropePositions.Contains(lastEnd.Value);
            return isStartMissing || isEndMissing;
        }

        // Nếu một trong hai giá trị không có thì không thể kiểm tra

        return false;
    }

    // Các phương thức khác giữ nguyên
    private void DetectCollisionEnter()
    {
        RaycastHit hit;
        // Lấy vị trí gần cuối của dây (điểm thứ hai từ cuối)
        Vector3 lastPosition = ropePositions[ropePositions.Count - 2];

        // Kiểm tra va chạm bằng Linecast
        if (Physics.Linecast(player.position, lastPosition, out hit, collMask))
        {
            Vector3 hitPoint = hit.point;

            // Nếu điểm va chạm khác với điểm cuối hiện tại
            if (ropePositions[ropePositions.Count - 1] != hitPoint)
            {
                // Xóa điểm cuối hiện tại
                ropePositions.RemoveAt(ropePositions.Count - 1);

                // Thêm điểm mới vào danh sách
                AddPosToRope(hitPoint);
            }
        }
    }
    /// <summary>
    /// Kiểm tra nếu tham chiếu dây đã mất
    /// </summary>
   
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
         // Số lượng điểm trong LineRenderer
        int pointCount = rope.positionCount;

        if (pointCount < 3) return; // Không đủ điểm để kiểm tra

        // Lấy toàn bộ vị trí từ LineRenderer
        Vector3[] positions = new Vector3[pointCount];
        rope.GetPositions(positions);

        // Chuyển mảng thành danh sách để dễ thao tác
        List<Vector3> positionList = new List<Vector3>(positions);

        // Duyệt qua các điểm từ gần cuối đến đầu
        for (int i = positionList.Count - 2; i > 0; i--)
        {
            Vector3 dir1 = positionList[i] - positionList[i - 1];
            Vector3 dir2 = positionList[i + 1] - positionList[i];

            float angle = Vector3.Angle(dir1, dir2);

            if (angle > maxAngle)
            {
                // Xóa điểm tại vị trí i
                positionList.RemoveAt(i);
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
     private void ResetCollisionState()
    {
        lastStart = null;
        lastEnd = null;
    }
    private void LastSegmentGoToPlayerPos() => rope.SetPosition(rope.positionCount - 1, player.position);
}
