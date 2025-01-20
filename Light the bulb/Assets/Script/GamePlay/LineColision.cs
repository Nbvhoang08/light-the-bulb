using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineColision : MonoBehaviour
{
    [SerializeField] private bool _isColliding = true; // Private field với SerializeField để hiện trong Inspector.

    [SerializeField] private Material defaultMaterial; // Material khi không va chạm.
   
    [SerializeField] private Material collisionMaterial; // Material khi có va chạm.
    

    [SerializeField]  private Renderer targetRenderer; // Renderer của object để thay đổi Material.
   
   void Awake()
   {
       _isColliding = true;
   }
   void Start()
   {
        IsColliding = false;
   }

    // Property kiểm soát isColliding

    public bool IsColliding
    {
        get => _isColliding;
        set
        {

            if (_isColliding != value) // Chỉ thực hiện khi giá trị thay đổi
            {   
                _isColliding = value;
                OnCollisionStateChanged(); // Gọi hàm xử lý khi giá trị thay đổi
            }
        }
    }

    // Hàm xử lý logic khi trạng thái thay đổi
    private void OnCollisionStateChanged()
    {
        if (targetRenderer != null)
        {
            targetRenderer.material = _isColliding ? collisionMaterial : defaultMaterial;
        }
         // Gửi thông báo cho Observer
        if (_isColliding)
        {
            Subject.NotifyObservers("Add"); // Gửi thông báo "Add" khi isColliding = true
            SoundManager.Instance.PlayVFXSound(0);
        }
        else
        {
            Subject.NotifyObservers("Sub"); // Gửi thông báo "Sub" khi isColliding = false
            //SoundManager.Instance.PlayVFXSound(1);
        }   
    }

  
}
