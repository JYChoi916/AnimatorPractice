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

    [Range(0, 1)]
    public float distanceToGround;



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

        WaveAction();
    }

    void WaveAction()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
        if (stateInfo.normalizedTime >= 1)
        {
            animator.StopPlayback();
            animator.Play("Waving", 1, 0);
        }

        if (stateInfo.normalizedTime > 0.7)
        {
            animator.SetLayerWeight(1, 1 - stateInfo.normalizedTime + 0.1f);
        }
        else
        {
            animator.SetLayerWeight(1, 1);
        }
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
                Debug.Log("Change to falling");
                // 애니메이션을 낙하 상태로 전환
                animator.SetBool("IsFalling", true);
            }
        }

        // 중력을 적용해 최종 수직 이동 속도를 계산
        verticalVelocity += gravity * Time.deltaTime;
    }

    void GroundedCheck()
    {
        // 지면 체크 오브젝트의 포지션에서 groundDistance만큼 구체를 구성해 배경 오브젝트가 있는지 확인
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

        // currentMagnitude가 0 ~ Max 일때, 0~1까지로 압축
        float blendSpeed = currentMagnitude / maxInputMagnitude;

        // 압축된 값을 애니메이션 Blend 수치로 파라미터 전달
        animator.SetFloat("Speed", blendSpeed);

        // 모든 이동 처리가 끝났기 때문에, 현재 높이를 다음 프레임에서 활요하기 위해 저장
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

    Vector3 leftFootPosition, rightFootPosition, leftFootIKPosition, rightFootIKPosition;
    Quaternion leftFootIKRotation, rightFootIKRotation;
    float lastPelvisPositionY, lastLeftFootPositionY, lastRightFootPositionY;

    [Header("Feet Grounder")]
    public bool enableFeetIK = true;

    [Range(0, 2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;
    [Range(0, 2)] [SerializeField] private float raycastDownDistance = 1.5f;
    [SerializeField] float pelvisOffset = 0f;
    [Range(0, 1)] [SerializeField] float pelvisUpAndDownSpeed = 0.28f;
    [Range(0, 1)] [SerializeField] float feetToIKPositionSpeed = 0.5f;

    public string leftFootAnimVariableName = "LeftFootCurve";
    public string rightFootAnimVariableName = "RightFootCurve";

    public bool useProIKFeature = false;
    public bool showSolverDebug = true;

    private void FixedUpdate()
    {
        if (enableFeetIK == false) return;
        if (animator == null) return;

        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);
        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);

        FeetPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation);
        FeetPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (enableFeetIK == false) return;
        if (animator == null) return;

        MovePelvisHeight();

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        if (useProIKFeature)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat(leftFootAnimVariableName));
        }

        MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);

        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        if (useProIKFeature)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat(rightFootAnimVariableName));
        }

        MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY);
    }

    void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
    {
        Vector3 targetIKPosition = animator.GetIKPosition(foot);

        if (positionIKHolder != Vector3.zero)
        {
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
            positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed);
            targetIKPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIKPosition = transform.TransformPoint(targetIKPosition);

            animator.SetIKPosition(foot, targetIKPosition);
        }
    }

    void MovePelvisHeight()
    {
        if (rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = animator.bodyPosition.y;
            return;
        }

        float lOffsetPosition = leftFootIKPosition.y - transform.position.y;
        float rOffsetPosition = rightFootIKPosition.y - transform.position.y;

        float totalOffset = lOffsetPosition < rOffsetPosition ? lOffsetPosition : rOffsetPosition;

        Vector3 newPelvisPosition = animator.bodyPosition + Vector3.up * totalOffset;
        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);

        animator.bodyPosition = newPelvisPosition;

        lastPelvisPositionY = animator.bodyPosition.y;
    }

    void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotation)
    {
        // raycast handling section
        RaycastHit feetOutHit;

        if (showSolverDebug)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow);

        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, staticObjectLayerMask))
        {
            feetIKPositions = fromSkyPosition;
            feetIKPositions.y = feetOutHit.point.y + pelvisOffset;
            feetIKRotation = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

            return;
        }

        feetIKPositions = Vector3.zero;

    }

    void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = animator.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + heightFromGroundRaycast;
    }

    private void OnDrawGizmosSelected()
    {
        if(UnityEngine.Application.isPlaying)
        {
            animator = GetComponent<Animator>();
            RaycastHit hit;
            Ray ray = new Ray(leftFootPosition + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, distanceToGround + 1f, staticObjectLayerMask))
            {
                if (hit.transform.tag == "Walkable")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += distanceToGround;

                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(hit.point, 0.03f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(hit.point, hit.point + hit.normal * 2f);
                }
            }

            ray = new Ray(rightFootPosition + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, distanceToGround + 1f, staticObjectLayerMask))
            {
                if (hit.transform.tag == "Walkable")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += distanceToGround;

                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(hit.point, 0.03f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(hit.point, hit.point + hit.normal * 2f);
                }
            }
        }
    }

    
}
