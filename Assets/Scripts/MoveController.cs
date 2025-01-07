using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    // ĳ���� ��Ʈ�ѷ�
    CharacterController controller;
    // ������ �ۿ�Ǵ� ��
    public float jumpPower = 1.2f;
    // �߷� ���ӵ�
    public float gravity = -15f;
    // ���ǵ�
    public float speed = 2.0f;
    // ȸ���ӵ�
    public float rotationFactorPerFrame = 0.1f;
    // ������ ��ǥ
    public Transform groundCheck;
    // ������� �Ÿ�
    public float groundDistance = 0.4f;
    // ����Ȯ�� ���̾��ũ
    public LayerMask staticObjectLayerMask;
    public Transform cameraTransform;
    // �̵� ����
    Vector3 moveDirection;
    // �̵�
    Vector3 velocity;
    bool isGrounded;
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = Vector3.zero;
        float dash = Input.GetAxis("Fire3");
        bool isRunning = dash > Mathf.Epsilon;
        float speedScale = 1.0f;
        if (isRunning)
        {
            speedScale += dash;

        }
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, staticObjectLayerMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        if (isGrounded && Input.GetAxis("Jump")> Mathf.Epsilon)
        {
            velocity.y = Mathf.Sqrt(jumpPower * -2 * gravity);
        }
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0f;
        moveDirection.Normalize();
        moveDirection *= Time.deltaTime * speed;
        controller.Move(moveDirection);

        
        bool isWalking = moveDirection.magnitude > Mathf.Epsilon;
        if(isWalking)
        {
            //transform.forward = moveDirection;
            HandleRotation();
        }
        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning", isRunning);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }
    void HandleRotation()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame);
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision!!");
    }
}
