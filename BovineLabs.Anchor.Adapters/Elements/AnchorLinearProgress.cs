namespace BovineLabs.Anchor.Elements
{
    using Unity.AppUI.Core;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AnchorLinearProgress : LinearProgress
    {
        public const string UssClassName = "bl-anchor-linear-progress";

        public const string DirectionUssClassName = UssClassName + "--";

        public const string FillTextureUssPropertyName = "--bl-anchor-linear-progress-fill-texture";

        [NoAutoStaticsCleanup]
        private static readonly CustomStyleProperty<Texture2D> FillTextureStyleProperty = new(FillTextureUssPropertyName);

        private static readonly BindingId DirectionProperty = nameof(direction);
        private static readonly BindingId FillTextureProperty = nameof(fillTexture);
        private static readonly BindingId MaskTextureProperty = nameof(maskTexture);

        private static readonly int StartProperty = Shader.PropertyToID("_Start");
        private static readonly int EndProperty = Shader.PropertyToID("_End");
        private static readonly int RoundedProperty = Shader.PropertyToID("_Rounded");
        private static readonly int BufferStartProperty = Shader.PropertyToID("_BufferStart");
        private static readonly int BufferEndProperty = Shader.PropertyToID("_BufferEnd");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int AntiAliasingProperty = Shader.PropertyToID("_AA");
        private static readonly int RatioProperty = Shader.PropertyToID("_Ratio");
        private static readonly int PaddingProperty = Shader.PropertyToID("_Padding");
        private static readonly int BufferOpacityProperty = Shader.PropertyToID("_BufferOpacity");
        private static readonly int PhaseProperty = Shader.PropertyToID("_Phase");
        private static readonly int VerticalProperty = Shader.PropertyToID("_Vertical");
        private static readonly int ReverseProperty = Shader.PropertyToID("_Reverse");
        private static readonly int FillTextureShaderProperty = Shader.PropertyToID("_FillTexture");
        private static readonly int UseFillTextureProperty = Shader.PropertyToID("_UseFillTexture");
        private static readonly int MaskTextureShaderProperty = Shader.PropertyToID("_MaskTexture");

        [NoAutoStaticsCleanup]
        private static Material s_material;

        private Direction _direction;
        private Texture2D _fillTexture;
        private Texture2D _fillTextureFromStyle;
        private Texture2D _maskTexture;

        public AnchorLinearProgress()
        {
            RemoveFromClassList(LinearProgress.ussClassName);
            AddToClassList(UssClassName);
            AddToClassList(GetDirectionUssClassName(_direction));
            this.RegisterContextChangedCallback<DirContext>(OnLayoutDirectionChanged);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStylesResolved);
        }

        /// <summary>
        /// Vertical progress follows the active App UI layout direction.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Direction direction
        {
            get => _direction;
            set
            {
                if (_direction == value)
                {
                    return;
                }

                RemoveFromClassList(GetDirectionUssClassName(_direction));
                _direction = value;
                AddToClassList(GetDirectionUssClassName(_direction));
                m_Image.MarkDirtyRepaint();
                NotifyPropertyChanged(in DirectionProperty);
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public Texture2D fillTexture
        {
            get => _fillTexture ? _fillTexture : _fillTextureFromStyle;
            set
            {
                if (_fillTexture == value)
                {
                    return;
                }

                var previous = fillTexture;
                _fillTexture = value;
                if (previous == fillTexture)
                {
                    return;
                }

                m_Image.MarkDirtyRepaint();
                NotifyPropertyChanged(in FillTextureProperty);
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public Texture2D maskTexture
        {
            get => _maskTexture;
            set
            {
                if (_maskTexture == value)
                {
                    return;
                }

                _maskTexture = value;
                m_Image.MarkDirtyRepaint();
                NotifyPropertyChanged(in MaskTextureProperty);
            }
        }

        protected override void GenerateTextures()
        {
            if (direction == Direction.Horizontal && !fillTexture && !maskTexture)
            {
                base.GenerateTextures();
                return;
            }

            if (!EnsureMaterial())
            {
                ReleaseTextures();
                return;
            }

            var rect = contentRect;
            if (!rect.IsValid())
            {
                ReleaseTextures();
                return;
            }

            var dpi = Mathf.Max(Platform.scaleFactor, 1f);
            var rectSize = rect.size * dpi;
            if (!rectSize.IsValidForTextureSize())
            {
                ReleaseTextures();
                return;
            }

            if (m_RT && (Mathf.Abs(m_RT.width - rectSize.x) > 1 || Mathf.Abs(m_RT.height - rectSize.y) > 1))
            {
                ReleaseTextures();
            }

            if (!m_RT)
            {
                m_RT = RenderTexture.GetTemporary((int)rectSize.x, (int)rectSize.y, 24);
            }

            var vertical = direction == Direction.Vertical;
            var reverse = vertical && (this.GetContext<DirContext>()?.dir ?? Dir.Ltr) == Dir.Rtl;
            var axisLength = vertical ? rectSize.y : rectSize.x;
            var crossLength = vertical ? rectSize.x : rectSize.y;

            s_material.SetColor(ColorProperty, colorOverride);
            s_material.SetInt(RoundedProperty, roundedProgressCorners ? 1 : 0);
            s_material.SetFloat(StartProperty, 0);
            s_material.SetFloat(EndProperty, value);
            s_material.SetFloat(BufferStartProperty, 0);
            s_material.SetFloat(BufferEndProperty, bufferValue);
            s_material.SetFloat(BufferOpacityProperty, bufferOpacity);
            s_material.SetFloat(AntiAliasingProperty, 2f / axisLength);
            s_material.SetVector(PhaseProperty, GetCurrentTimeVector());
            s_material.SetFloat(RatioProperty, axisLength / crossLength);
            s_material.SetFloat(PaddingProperty, roundedProgressCorners ? crossLength * 0.5f / axisLength : 0);
            s_material.SetInt(VerticalProperty, vertical ? 1 : 0);
            s_material.SetInt(ReverseProperty, reverse ? 1 : 0);
            s_material.SetTexture(FillTextureShaderProperty, fillTexture ? fillTexture : Texture2D.whiteTexture);
            s_material.SetInt(UseFillTextureProperty, fillTexture ? 1 : 0);
            s_material.SetTexture(MaskTextureShaderProperty, maskTexture ? maskTexture : Texture2D.whiteTexture);

            if (variant == Variant.Indeterminate)
            {
                s_material.EnableKeyword("ANCHOR_PROGRESS_INDETERMINATE");
            }
            else
            {
                s_material.DisableKeyword("ANCHOR_PROGRESS_INDETERMINATE");
            }

            var previousRenderTexture = RenderTexture.active;
            Graphics.Blit(null, m_RT, s_material);
            RenderTexture.active = previousRenderTexture;
        }

        private static bool EnsureMaterial()
        {
            if (s_material)
            {
                return true;
            }

            var shader = AnchorSettings.I.LinearProgressShader;
            if (!shader)
            {
                return false;
            }

            s_material = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave,
            };

            return true;
        }

        private static Vector4 GetCurrentTimeVector()
        {
            var time = Time.realtimeSinceStartup;
            return new Vector4(time / 20f, time, time * 2f, time * 3f);
        }

        private static string GetDirectionUssClassName(Direction value)
        {
            return DirectionUssClassName + (value == Direction.Vertical ? "vertical" : "horizontal");
        }

        private void OnLayoutDirectionChanged(ContextChangedEvent<DirContext> _)
        {
            if (direction == Direction.Vertical)
            {
                m_Image.MarkDirtyRepaint();
            }
        }

        private void OnCustomStylesResolved(CustomStyleResolvedEvent evt)
        {
            evt.customStyle.TryGetValue(FillTextureStyleProperty, out var texture);
            if (_fillTextureFromStyle == texture)
            {
                return;
            }

            var previous = fillTexture;
            _fillTextureFromStyle = texture;
            if (previous == fillTexture)
            {
                return;
            }

            m_Image.MarkDirtyRepaint();
            NotifyPropertyChanged(in FillTextureProperty);
        }
    }
}
