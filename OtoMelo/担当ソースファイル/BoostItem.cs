using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostItem : MonoBehaviour
{
    private enum BoostType
    {
        Accel,  // 加速
        Jump,   // ジャンプ
    }

    private enum AnimNum
    {
        State = 0,      // 普通
        Jump = 10,      // 動く
    }

    static readonly string _playerTag = "Player";
        
    [SerializeField] private ParticleSystem[] _particle;    // エフェクト
    [SerializeField] private float _AccelPower;             // 推進加速力
    [SerializeField] private float _jumpPower;              // 推進ジャンプ力
    [SerializeField] BoostType _boostType;                  // 加速アイテムの種類

    private PlayerController _player;   // プレイヤー情報
    private Animator _anim;             // アニメーション情報
    private float _fFreezeCount;        // ジャンプ台のアニメーション停止用
    private bool _isJump;               // ジャンプ中フラグ

    private void Start()
    {
        TryGetComponent(out _anim);
        _isJump = false;
    }

    // プレイヤ情報取得用
    public void InitSetPlayer(PlayerController player)
    {
        _player = player;
    }

    private void Update()
    {
        if (_isJump)
        {
            // 0.3秒でアニメーション停止、跳躍中フラグ解除
            _fFreezeCount += Time.deltaTime;
            if (_fFreezeCount >= 0.3f)
            {
                //anim.SetInteger("AnimNum", (int)AnimNum.State);
                _anim.speed = 0f;
                _isJump = false;
                _fFreezeCount = 0f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(_playerTag))
        {
            switch (_boostType)
            {
                // 加速装置
                case BoostType.Accel:
                    _isJump = false;

                    // SE
                    SoundManager.instance.StopSE();
                    SoundManager.instance.PlaySE(SoundManager.SE_Type.BoostAccel);

                    // プレイヤ内のブースト呼び出し
                    _player.MoveBoost(_AccelPower, jumpPower: 0, isJump : false);

                    break;

                // ジャンプ台
                case BoostType.Jump:
                    _isJump = true;

                    // エフェクト
                    if (_particle[(int)_boostType])
                    {
                        _particle[(int)BoostType.Jump].gameObject.SetActive(true);
                        _particle[(int)BoostType.Jump].Play();
                    }
                    else
                    {
                        // ジャンプエフェクト未設定
                    }
                    // SE
                    SoundManager.instance.StopSE();
                    SoundManager.instance.PlaySE(SoundManager.SE_Type.BoostJump);

                    // アニメーション
                    _anim.SetInteger("AnimNum", (int)AnimNum.Jump);
                    
                    // プレイヤ内のブースト呼び出し
                    _player.MoveBoost(accelPower: 0, _jumpPower, isJump: true);

                    break;
            }
        }
    }
}
