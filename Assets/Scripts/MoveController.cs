using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    // 캐릭터 컨트롤러
    CharacterController controller;
    // 점프에 작용되는 힘
    public float jumpPower = 1.2f;
    // 중력 가속도
    public float gravity = -15f;
    // 스피드
    public float speed = 2.0f;
    // 회전속도
    public float rotationFactorPerFrame = 0.1f;
    // 지상의 좌표
    public Transform groundCheck;
    // 지상과의 거리
    public float groundDistance = 0.4f;
    // 판정확인 레이어마스크
    public LayerMask staticObjectLayerMask;
    public Transform cameraTransform;
    // 이동 방향
    Vector3 moveDirection;
    // 이동
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
