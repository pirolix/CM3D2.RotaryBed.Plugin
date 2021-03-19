using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;
using System;
using System.Collections.Generic;

namespace CM3D2.RotaryBed.Plugin
{
    [   PluginName( "RotaryBed" ),
        PluginVersion( "0.1.0.319" )]

    public class RotaryBed : PluginBase
    {
        #region メンバ変数定義：動作設定値
        private struct _CONFIG {
            public string       KeyTogglePower;
            public float        PitchAmp;
            public float        PitchFreq;
            public float        RollAmp;
            public float        RollFreq;
        }
        private _CONFIG         m_cfg;
        #endregion

        #region メンバ変数定義：状態管理関係
        private bool    m_bActive;
        private float   m_fTimeOrigin;
        #endregion

        /// <summary>プラグインが初期化されたタイミングで呼ばれるコンストラクタ</summary>
        public void Awake()
        {
            // 動作設定値の初期値
            m_cfg.KeyTogglePower    = "K";
            m_cfg.PitchAmp          = 0.5f;
            m_cfg.PitchFreq         = 0.5f;
            m_cfg.RollAmp           = 0.2f;
            m_cfg.RollFreq          = 0.3f;

            GameObject.DontDestroyOnLoad( this );
        }

        /// <summary>ゲームレベルが変化した際に呼ばれる</summary>
        public void OnLevelWasLoaded(int level)
        {
            // 勝手に起動しない
            m_bActive = false;
            // シーン切り替わり毎に設定ファイルの内容を反映
            GetPluginPreferences();
        }

        /// <summary>画面を更新する</summary>
        public void LateUpdate()
        {
            // キー入力で切替える：オン/オフ
            if( Input.GetKeyDown( m_cfg.KeyTogglePower.ToLower())) {
                if( m_bActive )
                    m_bActive = false; // 終了
                else {
                    m_bActive = true; // 開始
                    m_fTimeOrigin      = Time.time;
                }
            }
            if( !m_bActive )
                return; // 非アクティブ時は何もしない

            CameraMain mainCamera = GameMain.Instance.MainCamera;
            Transform cameraT = mainCamera.camera.transform;

            // 移動量を計算
            float dT = Time.time - m_fTimeOrigin;
            float pitch = m_cfg.PitchAmp * Mathf.Sin( m_cfg.PitchFreq * dT );
            float roll  = m_cfg.RollAmp  * Mathf.Sin( m_cfg.RollFreq  * dT );

            // 注視点を中心にカメラ移動
            Vector3 target = mainCamera.GetTargetPos();
            cameraT.RotateAround( target, Vector3.up, pitch );
            cameraT.RotateAround( target, Vector3.right, roll );
            // 微妙にバンク？するので正立させる
            cameraT.LookAt( target, Vector3.up );
        }

        #region .ini ファイルの読み込み関係
        /// <summary>.ini ファイルからプラグイン設定を読み込む</summary>
        private void GetPluginPreferences()
        {
            m_cfg.KeyTogglePower = GetPreferences( "Key", "TogglePower", m_cfg.KeyTogglePower ).ToUpper();
            m_cfg.PitchAmp  = GetPreferences( "Config", "PitchAmp",  m_cfg.PitchAmp );
            m_cfg.PitchFreq = GetPreferences( "Config", "PitchFreq", m_cfg.PitchFreq );
            m_cfg.RollAmp   = GetPreferences( "Config", "RollAmp",   m_cfg.RollAmp );
            m_cfg.RollFreq  = GetPreferences( "Config", "RollFreq",  m_cfg.RollFreq );
        }

        /// <summary>設定ファイルから string データを読む</summary>
        private string GetPreferences( string section, string key, string defaultValue )
        {
            if (!Preferences.HasSection(section) || !Preferences[section].HasKey(key) || string.IsNullOrEmpty(Preferences[section][key].Value))
            {
                Preferences[section][key].Value = defaultValue;
                SaveConfig();
            }
            return Preferences[section][key].Value;
        }

        /// <summary>設定ファイルから bool データを読む</summary>
        private bool GetPreferences( string section, string key, bool defaultValue )
        {
            if( !Preferences.HasSection( section ) || !Preferences[section].HasKey( key ) || string.IsNullOrEmpty( Preferences[section][key].Value ))
            {
                Preferences[section][key].Value = defaultValue.ToString();
                SaveConfig();
            }
            bool b = defaultValue;
            bool.TryParse( Preferences[section][key].Value, out b );
            return b;
        }

        /// <summary>設定ファイルから int データを読む</summary>
        private int GetPreferences( string section, string key, int defaultValue )
        {
            if( !Preferences.HasSection( section ) || !Preferences[section].HasKey( key ) || string.IsNullOrEmpty( Preferences[section][key].Value ))
            {
                Preferences[section][key].Value = defaultValue.ToString();
                SaveConfig();
            }
            int i = defaultValue;
            int.TryParse( Preferences[section][key].Value, out i );
            return i;
        }

        /// <summary>設定ファイルから float データを読む</summary>
        private float GetPreferences( string section, string key, float defaultValue )
        {
            if( !Preferences.HasSection( section ) || !Preferences[section].HasKey( key ) || string.IsNullOrEmpty( Preferences[section][key].Value ))
            {
                Preferences[section][key].Value = defaultValue.ToString();
                SaveConfig();
            }
            float f = defaultValue;
            float.TryParse( Preferences[section][key].Value, out f );
            return f;
        }
        #endregion ファイルの読み込み関係

        #region 汎用メソッド
        /// <summary>プラグイン名を取得する</summary>
        private String GetPluginName()
        {
            String name = String.Empty;
            try {
                // 属性クラスからプラグイン名取得
                PluginNameAttribute att = Attribute.GetCustomAttribute( typeof( RotaryBed ), typeof( PluginNameAttribute )) as PluginNameAttribute;
                if( att != null )
                    name = att.Name;
            }
            catch( Exception e ) {
                Console.WriteLine( "{0}::GetPluginName: Exception: {1}", GetPluginName(), e.Message );
            }
            return name;
        }
        #endregion 汎用メソッド
    }
}
