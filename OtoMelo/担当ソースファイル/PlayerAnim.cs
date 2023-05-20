using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    // アニメーターの情報
    private Animator anim = null;
    // デッドエリアの情報
    private DeadAreaController dead;
    // プレイヤーの情報
    private PlayerController player;
    // 接地・ジャンプ・滑空状態
    private bool _isPlatform, _isJump, _isGlide;
    // アイテムに触れたか
    private bool _isItemJump, _isItemBoost;
    // 壁に衝突したか
    private bool _isHit;
    // スタートしたか
    private bool _isStart;
    // 加速アニメーションの再生速度
    [SerializeField] private float _fSpeed = 3f;

    // Start is called before the first frame update
    void Start()
    {
        TryGetComponent(out anim);
        TryGetComponent(out player);
    }

    // Update is called once per frame
    void Update()
    {
        // 値の更新
        _isJump = player._isJump;               // ジャンプ
        _isGlide = player._isGlide;             // 滑空
        _isPlatform = player._isPlatform;       // 接地
        _isItemJump = player._isItemJump;       // ジャンプアイテム
        _isItemBoost = player._isItemBoost;     // 加速アイテム
        _isHit = player._isHitAnim;             // 激突
        _isStart = player._isStart;             // スタート

        // 接地
        if (_isPlatform)
        {
            // アニメーションの更新
            if (!anim.GetBool("Run"))
            {
                anim.SetBool("Run", true);
                anim.SetBool("Jump", false);
            }
            anim.SetBool("DoubleJump", false);
            anim.SetBool("Glide", false);
        }
    
        // 壁に衝突
        if (_isHit) {
            anim.SetBool("Hit", true);  // アニメーション更新
            GameObject obj = Instantiate(player._damagedeffect, player.transform);  // エフェクト
            Destroy(obj,2.0f);  // 破棄
        }

        if (!_isStart)
        {
            return;
        }

        // アイテムでのアニメーション更新
        ItemAnim();
        // プラットフォーム別のメインアニメーション更新
#if UNITY_EDITOR_WIN
        PCAnim();
#elif UNITY_ANDROID
        SmartphoneAnim();
#endif

        // モデル停止時にアニメーション停止
        if (player._rbodyPlayer.bodyType == RigidbodyType2D.Static) {
            anim.speed = 0f;
        }
        else if (player._rbodyPlayer.bodyType == RigidbodyType2D.Dynamic && anim.speed <= 1f) {
            anim.speed = 1f;
        }
    }

    // スマートフォン
    private void SmartphoneAnim()
    {
        #region スマートフォンのアニメーション管理
        if (Input.touchCount > 0)
        {
            if (Input.GetMouseButton(0) && _isGlide)
            {
                // 滑空
                anim.SetBool("Glide", true);
            }
            if (Input.GetMouseButtonDown(0) && !_isGlide)
            {
                // ジャンプ
                anim.SetBool("Jump", true);
                anim.SetBool("Run", false);
                // ２段ジャンプ
                if (!_isPlatform)
                {
                    anim.SetBool("Run", false);
                    anim.SetBool("DoubleJump", true);
                }
            }
        }
        #endregion
    }

    // PC
    private void PCAnim()
    {
        #region PC用のアニメーション管理
        if (Input.GetMouseButton(1) && _isGlide)
        {
            // 滑空
            anim.SetBool("Glide", true);            
        }
        if (Input.GetMouseButtonDown(1) && !_isGlide)
        {
            // ジャンプ
            anim.SetBool("Jump", true);
            anim.SetBool("Run", false);
            // ２段ジャンプ
            if (!_isPlatform)
            {
                anim.SetBool("Run", false);
                anim.SetBool("DoubleJump", true);
            }            
        }
        #endregion
    }

    // アイテムを踏んだ際のアニメーション
    private void ItemAnim()
    {
        // ジャンプ台
        if (_isItemJump) {
            if (anim.GetBool("DoubleJump"))
            {
                anim.SetBool("DoubleJump", false);
            }
            if (anim.GetBool("Glide"))
            {
                anim.SetBool("Glide", false);
            }
            anim.SetBool("Jump", true);
        }
        else {
            anim.SetBool("Jump", false);
        }
        // 加速アイテム
        if (_isItemBoost) {
            anim.speed = _fSpeed;   // アニメーション再生速度3倍
        }
        else {
            anim.speed = 1f;        // 元に戻す
        }
    }
}
