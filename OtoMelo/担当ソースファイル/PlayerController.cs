using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // �v���C���[�̏��
    private enum ParticleType
    {
        Landing,    // �ڒn
        Gliding,    // ����
        Running,    // ���s
    }

    // ���E�A�C�e��
    static readonly string _szPlatformTag = "Platform";
    static readonly string _szJumpPadTag = "JumpPad";
    static readonly string _szAccelPadTag = "AccelPad";

    // �v���C���[
    [System.NonSerialized] public Rigidbody2D _rbodyPlayer;         // Rigidbody
    private Transform _tranPlayer;
    private ParticleSystem.EmissionModule _emissionRunning;         // ���s�G�t�F�N�g
    private ParticleSystem.EmissionModule _emissionGliding;         // ����G�t�F�N�g
    private bool _isTouch;                                          // ��ʂ��^�b�`���ꂽ���ǂ���
    private bool _isBoost;                                          // �����������ǂ���
    private float _jumpTime = 0.5f;                                 // �W�����v���x
    private float _fTime, _fAnimTime = 0;                           // ����

    // �v���C���[�̏��
    [System.NonSerialized] public bool _isPlatform;                 // �ڒn��
    [System.NonSerialized] public bool _isJump;                     // �W�����v��
    [System.NonSerialized] public bool _isGlide;                    // ����
    [System.NonSerialized] public bool _isHitAnim;                  // �Փ˒�
    [System.NonSerialized] public bool _isItemJump, _isItemBoost;   // ���i��

    // �G�t�F�N�g��
    [SerializeField] private AnimationCurve jumpCurve;              // �W�����v�̃A�j���[�V�����J�[�u
    [SerializeField] private ParticleSystem[] _particle;            // ����������p�[�e�B�N��
    [SerializeField] private GameObject _jumpeffect;                // �W�����v�G�t�F�N�g
    [SerializeField] public GameObject _damagedeffect;              // ��e�G�t�F�N�g
    [SerializeField] private TrailRenderer _dashtrail;              // �O�ՃG�t�F�N�g

    [SerializeField] private LayerMask _platformLayer;              // ���̃��C���[
    [SerializeField, Min(0)] private float _maxJumpTime = 1f;       // �ő�W�����v����
    [SerializeField, Min(0)] private float _fJumpPower = 12f;       // �W�����v��
    [SerializeField, Min(0)] private float _fMoveSpeed = 4f;        // ������
    [SerializeField] private float _fMaxStayCnt = 120;

    public float _jumpItemPower { get; set; } = 24f;
    public float _accelItemPower { get; set; } = 8f;

    public bool _isStart { get; set; } = false;

    public bool _isControl { get; set; } = true;

    public float _fReturnSpeed { get; set; } = 0;


    void Start()
    {
        TryGetComponent(out _tranPlayer);
        TryGetComponent(out _rbodyPlayer);

        _emissionRunning = _particle[(int)ParticleType.Running].emission;
        _emissionGliding = _particle[(int)ParticleType.Gliding].emission;

        _isJump = false;
        _isTouch = false;
        _isGlide = false;
        _isControl = true;
        _isItemJump = false;
        _isItemBoost = false;
    }

    private void Update()
    {
        // �ڒn����
        _isPlatform = JudgementPlatform();
        _emissionRunning.enabled = _isPlatform;
        _emissionGliding.enabled = false;

        if (_isPlatform)
        {
            _isJump = false;
            _isGlide = false;
            _isItemJump = false;
        }

        if (!_isStart)
        {
            return;
        }

        // ������Ԃ��ƃ_�b�V���G�t�F�N�g���o��
        if (_rbodyPlayer.velocity.x >= 12)
        {
            _dashtrail.gameObject.SetActive(true);
        }
        else
        {
            _dashtrail.gameObject.SetActive(false);
        }

        #region �W�����v
        _isTouch = false;

        // uGUI�̃{�^����������Ă��邩���`�F�b�N
        if (!_isControl)
        {
            // ������Ă����珈�������Ȃ��B
            return;
        }

#if UNITY_EDITOR_WIN
        if (Input.GetMouseButtonDown(1) && !_isGlide)
        {
            //_boxCollider.sharedMaterial.bounciness = 0.2f;
            // ��i�W�����v��
            if (_isJump)
            {
                SoundManager.instance.PlaySE(SoundManager.SE_Type.Gliding);
                _isGlide = true;
                _isJump = false;
            }
            // ��i�W�����v��
            else if (!_isPlatform)
            {
                if(_rbodyPlayer.bodyType==RigidbodyType2D.Dynamic)
                {
                    GameObject eff = Instantiate(_jumpeffect, transform.position - new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
                    Destroy(eff, 2.0f);
                }
               
                SoundManager.instance.PlaySE(SoundManager.SE_Type.Jump);
                _isJump = true;
            }
            if (_isPlatform)
            {
                if (_rbodyPlayer.bodyType == RigidbodyType2D.Dynamic)
                {
                    GameObject eff = Instantiate(_jumpeffect, transform.position - new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
                    Destroy(eff, 2.0f);
                }
                
                SoundManager.instance.PlaySE(SoundManager.SE_Type.Jump);
                _isTouch = true;
            }
            if (_isJump)
            {
                _isTouch = true;
            }
        }
        // �W�����v���ɒ�����
        else if (Input.GetMouseButton(1) && _isGlide)
        {
            // ����
            if (_rbodyPlayer.bodyType == RigidbodyType2D.Dynamic)
                _emissionGliding.enabled = true;
            _rbodyPlayer.velocity = new Vector2(_rbodyPlayer.velocity.x, -1.5f);
        }

#elif UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            if (Input.GetMouseButtonDown(0) && !_isGlide)
            {
                //_boxCollider.sharedMaterial.bounciness = 0.2f;
                // ��i�W�����v��
                if (_isJump)
                {
                    SoundManager.instance.PlaySE(SoundManager.SE_Type.Gliding);
                    _isGlide = true;
                    _isJump = false;
                }
                // ��i�W�����v��
                else if (!_isPlatform)
                {
                    if (_rbodyPlayer.bodyType == RigidbodyType2D.Dynamic)
                    {
                        GameObject eff = Instantiate(_jumpeffect, transform.position - new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
                        Destroy(eff, 2.0f);
                    }
                    SoundManager.instance.PlaySE(SoundManager.SE_Type.Jump);
                    _isJump = true;
                }
                if (_isPlatform)
                {
                    if (_rbodyPlayer.bodyType == RigidbodyType2D.Dynamic)
                    {
                        GameObject eff = Instantiate(_jumpeffect, transform.position - new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
                        Destroy(eff, 2.0f);
                    }
                    SoundManager.instance.PlaySE(SoundManager.SE_Type.Jump);
                    _isTouch = true;
                }
                if (_isJump)
                {
                    _isTouch = true;
                }
            }
            // �W�����v���ɒ�����
            else if (Input.GetMouseButton(0) && _isGlide)
            {
                // ����
                _emissionGliding.enabled = true;
                _rbodyPlayer.velocity = new Vector2(_rbodyPlayer.velocity.x, -1.5f);
            }
        }
#endif
        #endregion
    }

    private void FixedUpdate()
    {
        _fTime += Time.deltaTime;
        if (_fTime >= 3f)
        {
            //3�b���Ƃɍs����������
            //Debug.Log(_rbodyPlayer.velocity);

            _fTime = 0f;
        }
        // �������̃A�j���[�V�����I��
        if (_isItemBoost)
        {
            _fAnimTime += Time.deltaTime;
            if (_fAnimTime >= 1f)
            {
                _isItemBoost = false;
                _fAnimTime = 0f;
            }
        }

        JumpPlayer();

        MovePlayer();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �W�����v�A�C�e���ɐi��������
        if (other.CompareTag(_szJumpPadTag))
        {
            _isItemJump = true;
        }
        // �����A�C�e���ɐi��������
        if (other.CompareTag(_szAccelPadTag))
        {          
            _isItemBoost = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag(_szPlatformTag))
        {
            _particle[(int)ParticleType.Landing].gameObject.SetActive(true);
            _particle[(int)ParticleType.Landing].Play();
        }
    }

    private void MovePlayer()
    {
        Vector2 moveVector = Vector2.zero;
        moveVector = new Vector2(_fMoveSpeed - _rbodyPlayer.velocity.x, y: 0f);
        _rbodyPlayer.AddForce(moveVector, ForceMode2D.Force);

        // ��莞�ԕ��A�G���A�ɗ��܂�����
        if (_isBoost)
        {
            _rbodyPlayer.AddForce(new Vector2(_fMoveSpeed, 0f), ForceMode2D.Force);
        }
    }

    private void JumpPlayer()
    {
        if (!_isTouch)
        {
            return;
        }

        _rbodyPlayer.velocity = new Vector2(_rbodyPlayer.velocity.x, 0f);

        // �W�����v�̑��x���A�j���[�V�����J�[�u����擾
        float t = _jumpTime / _maxJumpTime;
        float power = _fJumpPower * jumpCurve.Evaluate(t);
        if (t >= 1)
        {
            _jumpTime = 0;
        }

        _rbodyPlayer.AddForce(Vector2.up * _fJumpPower, ForceMode2D.Impulse);
    }

    public void MoveBoost(float accelPower,float jumpPower, bool isJump)
    {
        if(isJump)
        {
            _rbodyPlayer.velocity = new Vector2(_rbodyPlayer.velocity.x, 0);
            _isJump = false;
            _isGlide = false;
            _isStart = false;
            StartCoroutine(WaitTach(jumpPower / 10));
        }
        _rbodyPlayer.AddForce(new Vector2(accelPower, jumpPower), ForceMode2D.Impulse);
    }

    private IEnumerator WaitTach(float waitTime)
    {
        yield return new WaitForSeconds(0.4f);

        _isStart = true;

        yield return null;
    }


    private RaycastHit2D JudgementPlatform()
    {
        RaycastHit2D hit;

        hit = Physics2D.CircleCast(_tranPlayer.position, radius: _tranPlayer.lossyScale.x * 0.5f, Vector2.down, distance: _tranPlayer.lossyScale.y * 0.5f, _platformLayer);

        return hit;
    }

    public void SetBodyType(RigidbodyType2D rigidbodyType)
    {
        _rbodyPlayer.bodyType = rigidbodyType;
    }

}

