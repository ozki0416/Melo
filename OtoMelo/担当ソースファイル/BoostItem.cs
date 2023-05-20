using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostItem : MonoBehaviour
{
    private enum BoostType
    {
        Accel,  // ����
        Jump,   // �W�����v
    }

    private enum AnimNum
    {
        State = 0,      // ����
        Jump = 10,      // ����
    }

    static readonly string _playerTag = "Player";
        
    [SerializeField] private ParticleSystem[] _particle;    // �G�t�F�N�g
    [SerializeField] private float _AccelPower;             // ���i������
    [SerializeField] private float _jumpPower;              // ���i�W�����v��
    [SerializeField] BoostType _boostType;                  // �����A�C�e���̎��

    private PlayerController _player;   // �v���C���[���
    private Animator _anim;             // �A�j���[�V�������
    private float _fFreezeCount;        // �W�����v��̃A�j���[�V������~�p
    private bool _isJump;               // �W�����v���t���O

    private void Start()
    {
        TryGetComponent(out _anim);
        _isJump = false;
    }

    // �v���C�����擾�p
    public void InitSetPlayer(PlayerController player)
    {
        _player = player;
    }

    private void Update()
    {
        if (_isJump)
        {
            // 0.3�b�ŃA�j���[�V������~�A�������t���O����
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
                // �������u
                case BoostType.Accel:
                    _isJump = false;

                    // SE
                    SoundManager.instance.StopSE();
                    SoundManager.instance.PlaySE(SoundManager.SE_Type.BoostAccel);

                    // �v���C�����̃u�[�X�g�Ăяo��
                    _player.MoveBoost(_AccelPower, jumpPower: 0, isJump : false);

                    break;

                // �W�����v��
                case BoostType.Jump:
                    _isJump = true;

                    // �G�t�F�N�g
                    if (_particle[(int)_boostType])
                    {
                        _particle[(int)BoostType.Jump].gameObject.SetActive(true);
                        _particle[(int)BoostType.Jump].Play();
                    }
                    else
                    {
                        // �W�����v�G�t�F�N�g���ݒ�
                    }
                    // SE
                    SoundManager.instance.StopSE();
                    SoundManager.instance.PlaySE(SoundManager.SE_Type.BoostJump);

                    // �A�j���[�V����
                    _anim.SetInteger("AnimNum", (int)AnimNum.Jump);
                    
                    // �v���C�����̃u�[�X�g�Ăяo��
                    _player.MoveBoost(accelPower: 0, _jumpPower, isJump: true);

                    break;
            }
        }
    }
}
