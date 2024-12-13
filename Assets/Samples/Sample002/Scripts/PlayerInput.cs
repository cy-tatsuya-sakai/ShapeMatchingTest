using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Samples.Sample002
{
    /// <summary>
    /// 入力情報
    /// </summary>
    public class PlayerInputInfo
    {
        public bool changeRectangle;
        public bool changeTriangle;
        public bool changeCircle;
        public int  roll;
    }

    /// <summary>
    /// 入力
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        private PlayerControls  _controls;
        private PlayerInputInfo _inputInfo;

        private bool  _changeCirc, _changeRect, _changeTri;
        private bool  _rollL, _rollR;
        private float _rollStick;

        void Awake()
        {
            _controls = new();
            _inputInfo = new();

            void OnRollLeft(InputAction.CallbackContext context)        => _rollL = true;
            void OnRollRight(InputAction.CallbackContext context)       => _rollR = true;
            void OnRollCancelLeft(InputAction.CallbackContext context)  => _rollL = false;
            void OnRollCancelRight(InputAction.CallbackContext context) => _rollR = false;
            void OnRollStick(InputAction.CallbackContext context)
            {
                var v = context.ReadValue<Vector2>();
                _rollStick = v.x;
            }
            void OnRollStickCancel(InputAction.CallbackContext context)
            {
                _rollStick = 0.0f;
            }

            _controls.Player.Rectangle.started  += (_) => _changeRect = true;
            _controls.Player.Triangle.started   += (_) => _changeTri  = true;
            _controls.Player.Circle.started     += (_) => _changeCirc = true;

            _controls.Player.RollLeft.started   += OnRollLeft;
            _controls.Player.RollLeft.canceled  += OnRollCancelLeft;
            _controls.Player.RollRight.started  += OnRollRight;
            _controls.Player.RollRight.canceled += OnRollCancelRight;
            _controls.Player.Roll.performed     += OnRollStick;
            _controls.Player.Roll.canceled      += OnRollStickCancel;

            _controls.Enable();
        }

        /// <summary>
        /// 入力情報を取得
        /// </summary>
        public PlayerInputInfo GetInputInfo()
        {
            // 形状
            _inputInfo.changeRectangle  = _changeRect;
            _inputInfo.changeTriangle   = _changeTri;
            _inputInfo.changeCircle     = _changeCirc;
            _changeRect = _changeTri = _changeCirc = false;

            // 回転
            const float t = 0.5f;
            _inputInfo.roll = 0;
            if(_rollStick <= -t)
            {
                _inputInfo.roll = -1;
            }
            else if(_rollStick >= t)
            {
                _inputInfo.roll = 1;
            }
            else
            {
                if(_rollL)
                {
                    _inputInfo.roll = -1;
                }
                else if(_rollR)
                {
                    _inputInfo.roll = 1;
                }
            }

            return _inputInfo;
        }
    }
}