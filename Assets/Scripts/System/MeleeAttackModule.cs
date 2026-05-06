using UnityEngine;
using System.Collections;

/// <summary>
/// �U�Ϊ�ԽĨ�Ҳ� (�欰�h)�C
/// �t�d������V��A�z�L Rigidbody2D �I�[���������z���O�i���i�C
/// </summary>
[RequireComponent(typeof(Rigidbody2D))] // ���w�ݭn����Ӱ����z�첾
public class MeleeAttackModule : MonoBehaviour
{
    [Header("�Ĩ�]�w")]
    public float dashForce = 20f;       // �Ĩ몺�z�o�t��
    public float dashDuration = 0.3f;   // �Ĩ���򪺮ɶ�

    private Rigidbody2D rb;
    private bool isDashing = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// �����Ԭ�i (�Ѥj���I�s)
    /// </summary>
    /// <param name="direction">��i����V (�`�A�ƦV�q)</param>
    public void Attack(Vector2 direction)
    {
        // �T�O���|�b�Ĩ뤤����Ĳ�o
        if (!isDashing)
        {
            StartCoroutine(DashRoutine(direction.normalized));
        }
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        isDashing = true;

        // �л\���e�����z�t�סA���������ļ�
        rb.linearVelocity = direction * dashForce;

        // ���ݽĨ�ɶ�����
        yield return new WaitForSeconds(dashDuration);

        // �Ĩ뵲���A�j�O�٨� (�N�t���k�s)
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }
}