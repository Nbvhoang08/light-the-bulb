using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour ,IObserver
{
    // Start is called before the first frame update
    public int BubNum   
    {
        get => _bubNum;  // Trả về giá trị của biến private
        set
        {
            // Đảm bảo giá trị luôn >= 0
            _bubNum = Mathf.Max(0, value); // Nếu value nhỏ hơn 0, nó sẽ trở thành 0
        }
    }

    [SerializeField] private int _bubNum;
    void Awake()
    {
        Subject.RegisterObserver(this);
    }
    void OnDestroy()
    {
        Subject.UnregisterObserver(this);
    }

    void Start()
    {
        BubNum = 0;  // Giá trị bắt đầu là 0
        MaxBubNum = FindObjectsOfType<LineColision>().Length;
    }   
    public void OnNotify(string eventName, object eventData)
    {
        if(eventName == "Add")
        {
            BubNum ++;    
        }
        else if(eventName =="Sub")
        {
            BubNum --;
        }
    }
    

    // Update is called once per frame
    public int MaxBubNum = 10;  // Giới hạn số lượng BubNum
    private bool hasWon = false;  // Biến flag để kiểm tra đã thắng chưa

    void Update()
    {
        // Kiểm tra nếu BubNum đạt MaxBubNum và game chưa thắng
        CheckWin();
    }

    private void CheckWin() 
    {
        if (BubNum == MaxBubNum && !hasWon)  // Chỉ thực hiện nếu chưa thắng
        {
            // Đánh dấu là đã thắng để tránh thực thi lại
            hasWon = true;
            // Thực hiện sau 0.5 giây
            StartCoroutine(WinCoroutine());
        }
    }

    private IEnumerator WinCoroutine()
    {
        yield return new WaitForSeconds(0.5f);  // Chờ 0.5 giây
        // Thực hiện sự kiện win
        UIManager.Instance.OpenUI<Win>();
        LevelManager.Instance.SaveGame();
        // Dừng game
        Time.timeScale = 0;  // Dừng game
    }
}
