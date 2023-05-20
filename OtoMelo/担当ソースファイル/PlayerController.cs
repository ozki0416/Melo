using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // プレイヤーの状態
    private enum ParticleType
    {
        Landing,    // 接地
        Gliding,    // 滑空
        Running,    // 走行
    }

    // 床・アイテム
    static readonly string _szPlatformTag = "Platform";
    static readonly string _szJumpPadTag = "JumpPad";
    static readonly string _szAccelPadTag = "AccelPad";

    // プレイヤー
    [System.NonSerialized] public Rigidbody2D _rbodyPlayer;         // Rigidbody
    private Transform _tranPlayer;
    private ParticleSystem.EmissionModule _emissionRunning;         // 走行エフェクト
    private ParticleSystem.EmissionModule _emissionGliding;         // 滑空エフェクト
    private bool _isTouch;                                          // 画面がタッチされたかどうか
    private bool _isBoost;                                          // 加速したかどうか
    private float _jumpTime = 0.5f;                                 // ジャンプ速度
    private float _fTime, _fAnimTime = 0;                           // 時間

    // プレイヤーの状態
    [System.NonSerialized] public bool _isPlatform;                 // 接地中
    [System.NonSerialized] public bool _isJump;                     // ジャンプ中
    [System.NonSerialized] public bool _isGlide;                    // 滑空中
    [System.NonSerialized] public bool _isHitAnim;                  // 衝突中
    [System.NonSerialized] public bool _isItemJump, _isItemBoost;   // 推進中

    // エフェクト等
    [SerializeField] private AnimationCurve jumpCurve;              // ジャンプのアニメーションカーブ
    [SerializeField] private ParticleSystem[] _particle;            // 発生させるパーティクル
    [SerializeField] private GameObject _jumpeffect;                // ジャンプエフェクト
    [SerializeField] public GameObject _damagedeffect;              // 被弾エフェクト
    [SerializeField] private TrailRenderer _dashtrail;              // 軌跡エフェクト

    [SerializeField] private LayerMask _platformLayer;              // 床のレイヤー
    [SerializeField, Min(0)] private float _maxJumpTime = 1f;       // 最大ジャンプ時間
    [SerializeField, Min(0)] private float _fJumpPower = 12f;       // ジャンプ力
    [SerializeField, Min(0)] private float _fMoveSpeed = 4f;        // 加速力
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
        // 接地判定
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

        // 加速状態だとダッシュエフェクトが出る
        if (_rbodyPlayer.velocity.x >= 12)
        {
            _dashtrail.gameObject.SetActive(true);
        }
        else
        {
            _dashtrail.gameObject.SetActive(false);
        }

        #region ジャンプ
        _isTouch = false;

        // uGUIのボタンが押されているかをチェック
        if (!_isControl)
        {
            // 押されていたら処理をしない。
            return;
        }

#if UNITY_EDITOR_WIN
        if (Input.GetMouseButtonDown(1) && !_isGlide)
        {
            //_boxCollider.sharedMaterial.bounciness = 0.2f;
            // 二段ジャンプ後
            if (_isJump)
            {
                SoundManager.instance.PlaySE(SoundManager.SE_Type.Gliding);
                _isGlide = true;
                _isJump = false;
            }
            // 一段ジャンプ後
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
        // ジャンプ時に長押し
        else if (Input.GetMouseButton(1) && _isGlide)
        {
            // 滑空
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
                // 二段ジャンプ後
                if (_isJump)
                {
                    SoundManager.instance.PlaySE(SoundManager.SE_Type.Gliding);
                    _isGlide = true;
                    _isJump = false;
                }
                // 一段ジャンプ後
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
            // ジャンプ時に長押し
            else if (Input.GetMouseButton(0) && _isGlide)
            {
                // 滑空
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
            //3秒ごとに行いたい処理
            //Debug.Log(_rbodyPlayer.velocity);

            _fTime = 0f;
        }
        // 加速時のアニメーション終了
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
        // ジャンプアイテムに進入したら
        if (other.CompareTag(_szJumpPadTag))
        {
            _isItemJump = true;
        }
        // 加速アイテムに進入したら
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

        // 一定時間復帰エリアに留まった時
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

        // ジャンプの速度をアニメーションカーブから取得
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

