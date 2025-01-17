
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollisionData 
{
     // Danh sách các vị trí va chạm
 // Danh sách các vị trí va chạm
    [SerializeField]
    private List<Vector3> collisionPositions = new List<Vector3>();
    public List<Vector3> CollisionPositions => collisionPositions;

    // Thông tin về đối tượng bị va chạm
    [SerializeField]
    private GameObject bub;
    public GameObject Bub => bub;
    // Constructor khởi tạo với danh sách vị trí và đối tượng bị va chạm
    public CollisionData(GameObject bub)
    {
        this.bub = bub;
    }

    // Thêm vị trí va chạm
    public void AddCollisionPosition(Vector3 position)
    {
        if (!CollisionPositions.Contains(position)) // Đảm bảo vị trí không bị trùng lặp
        {
            CollisionPositions.Add(position);
        }
    }

    // Xóa vị trí va chạm
    public void RemoveCollisionPosition(Vector3 position)
    {
        if (CollisionPositions.Contains(position))
        {
            CollisionPositions.Remove(position);
        }
    }

    // Xóa tất cả dữ liệu va chạm
    public void ClearAllCollisions()
    {
        CollisionPositions.Clear();
        bub = null;
    }

    // Kiểm tra xem đối tượng này có chứa va chạm tại một vị trí cụ thể không
    public bool ContainsCollision(Vector3 position)
    {
        return CollisionPositions.Contains(position);
    }
}

[System.Serializable]
public class LineCollisionSet
{
    public List<LineColision> collisions = new List<LineColision>();

    public void UpdateHashSet(HashSet<LineColision> hashSet)
    {
        collisions.Clear();
        collisions.AddRange(hashSet);
    }

    public void UpdateList(HashSet<LineColision> hashSet)
    {
        hashSet.Clear();
        foreach (var item in collisions)
        {
            hashSet.Add(item);
        }
    }
}
