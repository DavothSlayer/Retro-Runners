using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

//  VhsFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/Vol/Vhs")]
    public class VhsPass : VolFx.Pass
    {
		private static readonly int s_VhsTex      = Shader.PropertyToID("_VhsTex");
		private static readonly int s_NoiseTex    = Shader.PropertyToID("_NoiseTex");
		private static readonly int s_InputA      = Shader.PropertyToID("_InputA");
		private static readonly int s_InputB      = Shader.PropertyToID("_InputB");
		private static readonly int s_Glitch      = Shader.PropertyToID("_Glitch");
		private static readonly int s_Noise       = Shader.PropertyToID("_Noise");
		private static readonly int s_NoiseOffset = Shader.PropertyToID("_NoiseOffset");

		public override string ShaderName => string.Empty;

        [Tooltip("Use single tape type to smaller build size")]
		[HideInInspector]
        public Optional<Mode> _singleTape = new Optional<Mode>(Mode.Tape, false);
		[Tooltip("Default Glitch color")]
		public Color _colorDefault = Color.red;
		
        public  NoiseSettings _noiseSettings;
		private float         _flicker;
		private Texture2D     _noiseTex;

		[HideInInspector]
		public  float       _frameRate = 20f;
		[HideInInspector]
        public  Texture2D[] _tape;
		[HideInInspector]
        public  Texture2D[] _noise;
		[HideInInspector]
        public  Texture2D[] _shades;
		[HideInInspector]
        public  Texture2D[] _clip;
		
		private                 float _playTime;
		private                 float _yScanline;
		private                 float _xScanline;

		protected override bool   Invert     => true;

		// =======================================================================
		public enum Mode
		{
			Tape,
			Noise,
			Shades
		}
		
		[Serializable]
        public class NoiseSettings
        {
            public int        _height = 180;
            [Range(0, 1)]
            public float      _aspect = .3f;
            public bool       _point = true;
            [CurveRange]
            public AnimationCurve _intencityToHardness = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<VhsVol>();

            var isActive = settings.IsActive();
			if (isActive == false)
                return false;
			
			_validateNoise();
			
			if (_singleTape.Enabled)
			{
				_clip = _singleTape.Value switch
				{
					Mode.Tape   => _tape,
					Mode.Noise  => _noise,
					Mode.Shades => _shades,
					_           => throw new ArgumentOutOfRangeException()
				};
			}

            // scale line
			_yScanline += Time.deltaTime * 0.01f * settings._flow.value;
			_xScanline -= Time.deltaTime * 0.1f  * settings._pulsation.value;
            
			var glitch = settings._color.overrideState ? settings._color.value : _colorDefault;
			
			if (_yScanline >= 1)
				_yScanline = Random.value;
            
			if (_xScanline <= 0 || Random.value < 0.05)
				_xScanline = Random.value;
            
			mat.SetColor(s_Glitch, glitch);

			// float4 _inputA;	// _yScanline, _xScanline, _Intensity, _Rocking
			mat.SetVector(s_InputA, new Vector4(_yScanline,
												_xScanline,
												settings._weight.value,
												settings._rocking.value * settings._weight.value));
			
			// float4 _inputB;	// _Tape, _Pow, _Flickering, _Bleed
			mat.SetVector(s_InputB, new Vector4(settings._tape.value,
												settings._squeeze.value == 0f ? 0f : 1f / Mathf.Lerp(1000, 2, settings._squeeze.value),
												settings._flickering.value,
												settings._bleed.value));

			var noise   = settings._density.value;
            if (noise == 0)
                noise = -1;
			
            mat.SetVector(s_NoiseOffset, new Vector4(Random.value, Random.value, Random.value, Random.value));
			mat.SetVector(s_Noise, new Vector4(Mathf.Clamp01(settings._intensity.value + _noiseSettings._intencityToHardness.Evaluate(settings._density.value)),
											   noise,
											   settings._scale.value,
											   0));
			
			
            // params
			_playTime = (_playTime + Time.unscaledDeltaTime) % (_clip.Length / _frameRate);
			mat.SetTexture(s_VhsTex, _clip[Mathf.FloorToInt(_playTime * _frameRate)]);
			mat.SetTexture(s_NoiseTex, _noiseTex);
            
            return true;
        }
		
        private void _validateNoise()
        {
            var aspect   = Screen.width / (float)Screen.height;
            var noiseRes = new Vector2Int((int)(_noiseSettings._height * aspect * _noiseSettings._aspect), _noiseSettings._height);
            if (noiseRes.x < 4)
                noiseRes.x = 4;
            if (noiseRes.y < 4)
                noiseRes.y = 4;

			
			if (_noiseTex == null || _noiseTex.width != noiseRes.x || _noiseTex.height != noiseRes.y)
			{
				_noiseTex = new Texture2D(noiseRes.x, noiseRes.y, TextureFormat.RGBA32, false);

				_noiseTex.filterMode = _noiseSettings._point ? FilterMode.Point : FilterMode.Bilinear;
				_noiseTex.wrapMode   = TextureWrapMode.Repeat;
				for (var x = 0; x < _noiseTex.width; x++)
				for (var y = 0; y < _noiseTex.height; y++)
					_noiseTex.SetPixel(x, y, new Color(Random.value, Random.value, Random.value, Random.value));

				_noiseTex.Apply();
			}
        }

        protected override bool _editorValidate => _clip == null || _clip.Length == 0 || (Application.isPlaying == false && _clip.Any(n => n == null))
												   || ((_singleTape.Enabled && _tape != null && _noise != null && _shades != null) || (_singleTape.Enabled == false && (_tape == null || _noise == null || _shades == null)));
        protected override void _editorSetup(string folder, string asset)
        {
#if UNITY_EDITOR
			var sep = Path.DirectorySeparatorChar;
			
			_clip = UnityEditor.AssetDatabase.FindAssets("t:texture", new string[] {$"{folder}{sep}Vhs{sep}Tape"})
							   .Select(n => UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(n)))
							   .Where(n => n != null)
							   .ToArray();
			
			_playTime = 0f;
#endif
        }
    }
}