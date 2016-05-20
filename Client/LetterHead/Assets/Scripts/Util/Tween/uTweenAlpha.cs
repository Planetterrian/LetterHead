using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace uTools {
	[AddComponentMenu("uTools/Tween/Tween Alpha(uTools)")]
	public class uTweenAlpha : uTweenValue {

		public bool includeChilds = false;

		private Text mText;
	    private TextMeshProUGUI mTextMesh;
		private Light mLight;
		private Material mMat;
        private Image mImage;
        private CanvasGroup mCanvasGroup;
		private SpriteRenderer mSpriteRender;

		float mAlpha = 0f;

		public float alpha {
			get {
				return mAlpha;
			}
			set {
				SetAlpha(transform, value);
				mAlpha = value;
			}
		}

		protected override void ValueUpdate (float value, bool isFinished)
		{
			alpha = value;
		}

		void SetAlpha(Transform _transform, float _alpha)
		{

		    if (mText == null &&
		        mLight == null &&
		        mImage == null &&
		        mCanvasGroup == null &&
		        mSpriteRender == null &&
		        mMat == null &&
                mTextMesh == null)
		    {
                mText = _transform.GetComponent<Text>();
                mLight = _transform.GetComponent<Light>();
                mImage = _transform.GetComponent<Image>();
                mCanvasGroup = _transform.GetComponent<CanvasGroup>();
                mSpriteRender = _transform.GetComponent<SpriteRenderer>();
		        mTextMesh = _transform.GetComponent<TextMeshProUGUI>();

                if (_transform.GetComponent<Renderer>() != null)
                    mMat = _transform.GetComponent<Renderer>().material;
            }


            Color c = Color.white;
			if (mText != null){
				c = mText.color;
				c.a = _alpha;
				mText.color = c;
			}
			if (mLight != null){ 
				c = mLight.color;
				c.a = _alpha;
				mLight.color = c;
			}
            if (mImage != null)
            {
                c = mImage.color;
                c.a = _alpha;
                mImage.color = c;
            }
            if (mCanvasGroup != null)
            {
                mCanvasGroup.alpha = _alpha;
            }
			if (mSpriteRender != null) {
				c = mSpriteRender.color;
				c.a = _alpha;
				mSpriteRender.color = c;
			}
			if (mMat != null) {
				c = mMat.color;
				c.a = _alpha;
				mMat.color = c;
			}
		    if (mTextMesh != null)
		    {
                c = mTextMesh.color;
                c.a = _alpha;
                mTextMesh.color = c;
            }

			if (includeChilds) {
				for (int i = 0; i < _transform.childCount; ++i) {
					Transform child = _transform.GetChild(i);
					SetAlpha(child, _alpha);
				}
			}
		}



	}

}