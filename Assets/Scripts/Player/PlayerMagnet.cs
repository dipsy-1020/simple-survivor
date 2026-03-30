using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
    // 當有東西進入磁力場範圍時
    void OnTriggerEnter2D(Collider2D other)
    {
        // 如果進來的是寶石
        if (other.CompareTag("Gem"))
        {
            Gem gem = other.GetComponent<Gem>();
            if (gem != null)
            {
                // 告訴寶石：朝我的父物件（也就是主角）飛過去！
                gem.StartFlying(transform.parent);
            }
        }
    }
}