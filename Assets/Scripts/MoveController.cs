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
    // 걷기 스피드
    public float moveSpeed = 2.0f;
    // 회전속도
    public float rotationFactorPerFrame = 0.1f;
    // 지상의 좌표
    public Transform groundCheck;
    // 지상과의 거리
    public float groundDistance = 0.4f;
    // 판정확인 레이어마스크
    public LayerMask staticObjectLayerMask;
    // 카메라 Transform
    public Transform cameraTransform;

    // 이동 방향
    Vector3 moveDirection;
    // 세로축 이동 속력
    float verticalVelocity;
    // 이전 프레임 y위치
    float previousHeight;
    // 땅을 밟고 있는지 체크
    bool isGrounded;
    // Animator 컴포넌트 인스턴스
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        isGrounded = true;
    }

    // Update is called once per frame
    void Update()
    {
        // 1. 점프와 중력처리
        JumpAndGravity();
        // 2. 지면 체크 처리
        GroundedCheck();
        // 3. 이동
        Move();
    }

    void JumpAndGravity()
    {
        if (isGrounded)
        {
            // 애니메이터 파라미터 초기화 (지상에 있으므로 점프와 낙하 상태는 아니게 바꾼다)
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);

            // 낙하속도를 초기화 한다.
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            // 점프 키 입력 체크
            if (Input.GetAxis("Jump") > Mathf.Epsilon)
            {
                // 점프 속도를 수직 이동속도에 계산
                verticalVelocity = Mathf.Sqrt(jumpPower * -2 * gravity);
                animator.SetBool("IsJumping", true);
            }
        }
        else
        {
            // 점프 정점에 도달하는 것을 체크
            float diffHeight = previousHeight - transform.position.y;
            if (diffHeight > 0f)
            {
                // 애니메이션을 낙하 상태로 전환
                animator.SetBool("IsFalling", true);
            }
        }

        // 중력을 적용해 최종 수직 이동 속도를 계산
        verticalVelocity += gravity * Time.deltaTime;
    }

    void GroundedCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, staticObjectLayerMask);

        animator.SetBool("IsGrounded", isGrounded);
    }

    void Move()
    {
        // 이동 벡터 초기화
        moveDirection = Vector3.zero;

        // 대쉬를 눌렀는가?
        float dash = Input.GetAxis("Fire3");

        float speedScale = 1.0f;
        if (dash > Mathf.Epsilon)
        {
            speedScale += dash;
        }

        // 현재 수평 이동 속도 계산
        float currentSpeed = moveSpeed * speedScale;

        // 입력값
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 입력값으로 입력 벡터 만들고
        Vector3 inputVector = new Vector3(horizontal, 0, vertical);

        // 입력벡터의 양을 저장
        float inputMagnitude = inputVector.magnitude;

        // 카메라가 바라보는 방향과 카메라의 오른쪽 방향으로 입력값의 각 축의 값을 곱해 이동 방향 벡터를 구함
        moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;

        // 그 이동 방향의 수직 방향은 0로 고정
        moveDirection.y = 0f;

        // 정규화 하여 크기를 1로 만든다.
        moveDirection.Normalize();

        // 현재 수평 이동 속도에 DeltaTime을 곱하고
        moveDirection *= Time.deltaTime * currentSpeed;

        // 수평 이동벡터 + (Y값만 수직 이동 속도를 적용한)수직 이동벡터에 deltaTime을 곱한 벡터만큼 움직인다.
        controller.Move(moveDirection + new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        // 이동 벡터의 크기가 0보다 크다면 걷기 상태가 된다.
        bool isWalking = moveDirection.magnitude > Mathf.Epsilon;

        // 걷기 상태라면
        if (isWalking)
        {
            // 캐릭터가 바라보는 방향도 이동 방향으로 돌려준다.
            //transform.forward = moveDirection;
            HandleRotation();
        }

        // 최대 수평 이동 속도
        float maxInputMagnitude = 4.0f;

        // 현재 수평 이동 속도
        float currentMagnitude = inputMagnitude * currentSpeed;

        // currentMagnitu
        float blendSpeed = currentMagnitude / maxInputMagnitude;

        animator.SetFloat("Speed", blendSpeed);
        previousHeight = transform.position.y;
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
