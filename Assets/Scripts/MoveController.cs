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
    // �ȱ� ���ǵ�
    public float moveSpeed = 2.0f;
    // ȸ���ӵ�
    public float rotationFactorPerFrame = 0.1f;
    // ������ ��ǥ
    public Transform groundCheck;
    // ������� �Ÿ�
    public float groundDistance = 0.4f;
    // ����Ȯ�� ���̾��ũ
    public LayerMask staticObjectLayerMask;
    // ī�޶� Transform
    public Transform cameraTransform;

    // �̵� ����
    Vector3 moveDirection;
    // ������ �̵� �ӷ�
    float verticalVelocity;
    // ���� ������ y��ġ
    float previousHeight;
    // ���� ��� �ִ��� üũ
    bool isGrounded;
    // Animator ������Ʈ �ν��Ͻ�
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
        // 1. ������ �߷�ó��
        JumpAndGravity();
        // 2. ���� üũ ó��
        GroundedCheck();
        // 3. �̵�
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
            // �ִϸ����� �Ķ���� �ʱ�ȭ (���� �����Ƿ� ������ ���� ���´� �ƴϰ� �ٲ۴�)
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);

            // ���ϼӵ��� �ʱ�ȭ �Ѵ�.
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            // ���� Ű �Է� üũ
            if (Input.GetAxis("Jump") > Mathf.Epsilon)
            {
                // ���� �ӵ��� ���� �̵��ӵ��� ���
                verticalVelocity = Mathf.Sqrt(jumpPower * -2 * gravity);
                animator.SetBool("IsJumping", true);
            }
        }
        else
        {
            // ���� ������ �����ϴ� ���� üũ
            float diffHeight = previousHeight - transform.position.y;
            if (diffHeight > 0f)
            {
                Debug.Log("Change to falling");
                // �ִϸ��̼��� ���� ���·� ��ȯ
                animator.SetBool("IsFalling", true);
            }
        }

        // �߷��� ������ ���� ���� �̵� �ӵ��� ���
        verticalVelocity += gravity * Time.deltaTime;
    }

    void GroundedCheck()
    {
        // ���� üũ ������Ʈ�� �����ǿ��� groundDistance��ŭ ��ü�� ������ ��� ������Ʈ�� �ִ��� Ȯ��
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, staticObjectLayerMask);

        animator.SetBool("IsGrounded", isGrounded);
    }

    void Move()
    {
        // �̵� ���� �ʱ�ȭ
        moveDirection = Vector3.zero;

        // �뽬�� �����°�?
        float dash = Input.GetAxis("Fire3");

        float speedScale = 1.0f;
        if (dash > Mathf.Epsilon)
        {
            speedScale += dash;
        }

        // ���� ���� �̵� �ӵ� ���
        float currentSpeed = moveSpeed * speedScale;

        // �Է°�
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // �Է°����� �Է� ���� �����
        Vector3 inputVector = new Vector3(horizontal, 0, vertical);

        // �Էº����� ���� ����
        float inputMagnitude = inputVector.magnitude;

        // ī�޶� �ٶ󺸴� ����� ī�޶��� ������ �������� �Է°��� �� ���� ���� ���� �̵� ���� ���͸� ����
        moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;

        // �� �̵� ������ ���� ������ 0�� ����
        moveDirection.y = 0f;

        // ����ȭ �Ͽ� ũ�⸦ 1�� �����.
        moveDirection.Normalize();

        // ���� ���� �̵� �ӵ��� DeltaTime�� ���ϰ�
        moveDirection *= Time.deltaTime * currentSpeed;

        // ���� �̵����� + (Y���� ���� �̵� �ӵ��� ������)���� �̵����Ϳ� deltaTime�� ���� ���͸�ŭ �����δ�.
        controller.Move(moveDirection + new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        // �̵� ������ ũ�Ⱑ 0���� ũ�ٸ� �ȱ� ���°� �ȴ�.
        bool isWalking = moveDirection.magnitude > Mathf.Epsilon;

        // �ȱ� ���¶��
        if (isWalking)
        {
            // ĳ���Ͱ� �ٶ󺸴� ���⵵ �̵� �������� �����ش�.
            //transform.forward = moveDirection;
            HandleRotation();
        }

        // �ִ� ���� �̵� �ӵ�
        float maxInputMagnitude = 4.0f;

        // ���� ���� �̵� �ӵ�
        float currentMagnitude = inputMagnitude * currentSpeed;

        // currentMagnitude�� 0 ~ Max �϶�, 0~1������ ����
        float blendSpeed = currentMagnitude / maxInputMagnitude;

        // ����� ���� �ִϸ��̼� Blend ��ġ�� �Ķ���� ����
        animator.SetFloat("Speed", blendSpeed);

        // ��� �̵� ó���� ������ ������, ���� ���̸� ���� �����ӿ��� Ȱ���ϱ� ���� ����
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
