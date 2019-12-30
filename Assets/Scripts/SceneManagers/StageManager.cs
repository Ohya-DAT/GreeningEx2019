﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GreeningEx2019
{
    public class StageManager : SceneManagerBase
    {
        public static StageManager instance = null;

        [Tooltip("ゲーム用プレイヤープレハブ"), SerializeField]
        GameObject stellaPrefab = null;
        [Tooltip("クリアフェードの色"), SerializeField]
        Color clearFadeColor = Color.green;
        [Tooltip("クリア用マテリアル"), SerializeField]
        Material[] clearBGMaterials = new Material[3];
            

        const float RollingSeconds = 0.8f;
        const float ClearFadeSeconds = 0.24f;

        /// <summary>
        /// クリア処理中の時、true
        /// </summary>
        public static bool IsClearPlaying { get; private set; }

        /// <summary>
        /// 操作可能な状態かどうかを返します。
        /// </summary>
        public static bool CanMove
        {
            get
            {
                return !Fade.IsFading && !IsClearPlaying;
            }
        }

        private new void Awake()
        {
            instance = this;
            IsClearPlaying = false;
            base.Awake();
        }

        public override void OnFadeOutDone()
        {
            SoundController.PlayBGM(SoundController.BgmType.Game0, true);
            SceneManager.SetActiveScene(gameObject.scene);

            // プレイヤーを入れ替える
            GameObject stabPlayer = GameObject.FindGameObjectWithTag("Player");
            GameObject myp = Instantiate(stellaPrefab, stabPlayer.transform.position, stabPlayer.transform.rotation);
            Destroy(stabPlayer);

            // カメラにターゲットを設定
            FollowCamera fcam = Camera.main.gameObject.GetComponent<FollowCamera>();
            fcam.SetTarget(myp.transform);
        }

        /// <summary>
        /// クリア処理を開始
        /// </summary>
        public static void StartClear()
        {
            IsClearPlaying = true;
            instance.StartCoroutine(instance.ClearSequence());
            SoundController.PlayBGM(SoundController.BgmType.Clear);
        }

        IEnumerator ClearSequence()
        {
            // 星を回転させる
            Goal.ClearAnim();
            yield return new WaitForSeconds(RollingSeconds);

            // フェードアウト
            Color lastColor = Fade.NowColor;
            yield return Fade.StartFade(Fade.FadeStateType.Out, clearFadeColor, ClearFadeSeconds);

            // 背景を切り替える
            BGScroller.instance.ChangeMaterials(clearBGMaterials);

            // フェードイン
            yield return Fade.StartFade(Fade.FadeStateType.In, clearFadeColor, ClearFadeSeconds);
            Fade.SetFadeColor(lastColor);

            // ステラが星の所定の位置に移動して捕まる
            // ステラを星の子供にする
            // 星が飛び立つ
            Goal.FlyAnim();

            // これ以降は、Goal側に実装
        }
    }
}
