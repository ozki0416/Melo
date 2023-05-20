using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    // �A�j���[�^�[�̏��
    private Animator anim = null;
    // �f�b�h�G���A�̏��
    private DeadAreaController dead;
    // �v���C���[�̏��
    private PlayerController player;
    // �ڒn�E�W�����v�E������
    private bool _isPlatform, _isJump, _isGlide;
    // �A�C�e���ɐG�ꂽ��
    private bool _isItemJump, _isItemBoost;
    // �ǂɏՓ˂�����
    private bool _isHit;
    // �X�^�[�g������
    private bool _isStart;
    // �����A�j���[�V�����̍Đ����x
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
        // �l�̍X�V
        _isJump = player._isJump;               // �W�����v
        _isGlide = player._isGlide;             // ����
        _isPlatform = player._isPlatform;       // �ڒn
        _isItemJump = player._isItemJump;       // �W�����v�A�C�e��
        _isItemBoost = player._isItemBoost;     // �����A�C�e��
        _isHit = player._isHitAnim;             // ����
        _isStart = player._isStart;             // �X�^�[�g

        // �ڒn
        if (_isPlatform)
        {
            // �A�j���[�V�����̍X�V
            if (!anim.GetBool("Run"))
            {
                anim.SetBool("Run", true);
                anim.SetBool("Jump", false);
            }
            anim.SetBool("DoubleJump", false);
            anim.SetBool("Glide", false);
        }
    
        // �ǂɏՓ�
        if (_isHit) {
            anim.SetBool("Hit", true);  // �A�j���[�V�����X�V
            GameObject obj = Instantiate(player._damagedeffect, player.transform);  // �G�t�F�N�g
            Destroy(obj,2.0f);  // �j��
        }

        if (!_isStart)
        {
            return;
        }

        // �A�C�e���ł̃A�j���[�V�����X�V
        ItemAnim();
        // �v���b�g�t�H�[���ʂ̃��C���A�j���[�V�����X�V
#if UNITY_EDITOR_WIN
        PCAnim();
#elif UNITY_ANDROID
        SmartphoneAnim();
#endif

        // ���f����~���ɃA�j���[�V������~
        if (player._rbodyPlayer.bodyType == RigidbodyType2D.Static) {
            anim.speed = 0f;
        }
        else if (player._rbodyPlayer.bodyType == RigidbodyType2D.Dynamic && anim.speed <= 1f) {
            anim.speed = 1f;
        }
    }

    // �X�}�[�g�t�H��
    private void SmartphoneAnim()
    {
        #region �X�}�[�g�t�H���̃A�j���[�V�����Ǘ�
        if (Input.touchCount > 0)
        {
            if (Input.GetMouseButton(0) && _isGlide)
            {
                // ����
                anim.SetBool("Glide", true);
            }
            if (Input.GetMouseButtonDown(0) && !_isGlide)
            {
                // �W�����v
                anim.SetBool("Jump", true);
                anim.SetBool("Run", false);
                // �Q�i�W�����v
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
        #region PC�p�̃A�j���[�V�����Ǘ�
        if (Input.GetMouseButton(1) && _isGlide)
        {
            // ����
            anim.SetBool("Glide", true);            
        }
        if (Input.GetMouseButtonDown(1) && !_isGlide)
        {
            // �W�����v
            anim.SetBool("Jump", true);
            anim.SetBool("Run", false);
            // �Q�i�W�����v
            if (!_isPlatform)
            {
                anim.SetBool("Run", false);
                anim.SetBool("DoubleJump", true);
            }            
        }
        #endregion
    }

    // �A�C�e���𓥂񂾍ۂ̃A�j���[�V����
    private void ItemAnim()
    {
        // �W�����v��
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
        // �����A�C�e��
        if (_isItemBoost) {
            anim.speed = _fSpeed;   // �A�j���[�V�����Đ����x3�{
        }
        else {
            anim.speed = 1f;        // ���ɖ߂�
        }
    }
}
