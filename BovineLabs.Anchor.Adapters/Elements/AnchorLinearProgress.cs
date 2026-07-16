// <copyright file="AnchorLinearProgress.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using Unity.AppUI.Core;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// App UI linear progress that supports horizontal or vertical fill and an optional alpha mask.
    /// </summary>
    [UxmlElement]
    public partial class AnchorLinearProgress : Progress
    {
        /// <summary>Anchor styling class.</summary>
        public const string UssClassName = "bl-anchor-linear-progress";

        /// <summary>Direction styling class prefix.</summary>
        public const string DirectionUssClassName = UssClassName + "--";

        /// <summary>USS custom property for the fill texture.</summary>
        public const string FillTextureUssPropertyName = "--bl-anchor-linear-progress-fill-texture";

        private const string ShaderResourcePath = "BovineLabs.Anchor/AnchorLinearProgress";

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

        private static Material s_material;

        private Direction m_direction;
        private Texture2D m_fillTexture;
        private Texture2D m_fillTextureFromStyle;
        private Texture2D m_maskTexture;

        /// <summary>Initializes a new instance of the <see cref="AnchorLinearProgress"/> class.</summary>
        public AnchorLinearProgress()
        {
            this.AddToClassList(UssClassName);
            this.AddToClassList(GetDirectionUssClassName(this.m_direction));
            this.RegisterContextChangedCallback<DirContext>(this.OnLayoutDirectionChanged);
            this.RegisterCallback<CustomStyleResolvedEvent>(this.OnCustomStylesResolved);
        }

        /// <summary>Gets or sets the progress axis. Vertical progress follows the active App UI layout direction.</summary>
        [CreateProperty]
        [UxmlAttribute]
        public Direction direction
        {
            get => this.m_direction;
            set
            {
                if (this.m_direction == value)
                {
                    return;
                }

                this.RemoveFromClassList(GetDirectionUssClassName(this.m_direction));
                this.m_direction = value;
                this.AddToClassList(GetDirectionUssClassName(this.m_direction));
                this.m_Image.MarkDirtyRepaint();
                this.NotifyPropertyChanged(in DirectionProperty);
            }
        }

        /// <summary>Gets or sets the texture revealed by the progress and buffer values.</summary>
        [CreateProperty]
        [UxmlAttribute]
        public Texture2D fillTexture
        {
            get => this.m_fillTexture ? this.m_fillTexture : this.m_fillTextureFromStyle;
            set
            {
                if (this.m_fillTexture == value)
                {
                    return;
                }

                var previous = this.fillTexture;
                this.m_fillTexture = value;
                if (previous == this.fillTexture)
                {
                    return;
                }

                this.m_Image.MarkDirtyRepaint();
                this.NotifyPropertyChanged(in FillTextureProperty);
            }
        }

        /// <summary>Gets or sets the texture whose alpha clips the complete progress render.</summary>
        [CreateProperty]
        [UxmlAttribute]
        public Texture2D maskTexture
        {
            get => this.m_maskTexture;
            set
            {
                if (this.m_maskTexture == value)
                {
                    return;
                }

                this.m_maskTexture = value;
                this.m_Image.MarkDirtyRepaint();
                this.NotifyPropertyChanged(in MaskTextureProperty);
            }
        }

        /// <inheritdoc />
        protected override void GenerateTextures()
        {
            if (!EnsureMaterial())
            {
                this.ReleaseTextures();
                return;
            }

            var rect = this.contentRect;
            if (!rect.IsValid())
            {
                this.ReleaseTextures();
                return;
            }

            var dpi = Mathf.Max(Platform.scaleFactor, 1f);
            var rectSize = rect.size * dpi;
            if (!rectSize.IsValidForTextureSize())
            {
                this.ReleaseTextures();
                return;
            }

            if (this.m_RT && (Mathf.Abs(this.m_RT.width - rectSize.x) > 1 || Mathf.Abs(this.m_RT.height - rectSize.y) > 1))
            {
                this.ReleaseTextures();
            }

            if (!this.m_RT)
            {
                this.m_RT = RenderTexture.GetTemporary((int)rectSize.x, (int)rectSize.y, 24);
            }

            var vertical = this.direction == Direction.Vertical;
            var reverse = vertical && (this.GetContext<DirContext>()?.dir ?? Dir.Ltr) == Dir.Rtl;
            var axisLength = vertical ? rectSize.y : rectSize.x;
            var crossLength = vertical ? rectSize.x : rectSize.y;

            s_material.SetColor(ColorProperty, this.colorOverride);
            s_material.SetInt(RoundedProperty, this.roundedProgressCorners ? 1 : 0);
            s_material.SetFloat(StartProperty, 0);
            s_material.SetFloat(EndProperty, this.value);
            s_material.SetFloat(BufferStartProperty, 0);
            s_material.SetFloat(BufferEndProperty, this.bufferValue);
            s_material.SetFloat(BufferOpacityProperty, this.bufferOpacity);
            s_material.SetFloat(AntiAliasingProperty, 2f / axisLength);
            s_material.SetVector(PhaseProperty, GetCurrentTimeVector());
            s_material.SetFloat(RatioProperty, axisLength / crossLength);
            s_material.SetFloat(PaddingProperty, this.roundedProgressCorners ? crossLength * 0.5f / axisLength : 0);
            s_material.SetInt(VerticalProperty, vertical ? 1 : 0);
            s_material.SetInt(ReverseProperty, reverse ? 1 : 0);
            s_material.SetTexture(FillTextureShaderProperty, this.fillTexture ? this.fillTexture : Texture2D.whiteTexture);
            s_material.SetInt(UseFillTextureProperty, this.fillTexture ? 1 : 0);
            s_material.SetTexture(MaskTextureShaderProperty, this.maskTexture ? this.maskTexture : Texture2D.whiteTexture);

            if (this.variant == Variant.Indeterminate)
            {
                s_material.EnableKeyword("ANCHOR_PROGRESS_INDETERMINATE");
            }
            else
            {
                s_material.DisableKeyword("ANCHOR_PROGRESS_INDETERMINATE");
            }

            var previousRenderTexture = RenderTexture.active;
            Graphics.Blit(null, this.m_RT, s_material);
            RenderTexture.active = previousRenderTexture;
        }

        private static bool EnsureMaterial()
        {
            if (s_material)
            {
                return true;
            }

            var shader = Resources.Load<Shader>(ShaderResourcePath);
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
            if (this.direction == Direction.Vertical)
            {
                this.m_Image.MarkDirtyRepaint();
            }
        }

        private void OnCustomStylesResolved(CustomStyleResolvedEvent evt)
        {
            evt.customStyle.TryGetValue(FillTextureStyleProperty, out var texture);
            if (this.m_fillTextureFromStyle == texture)
            {
                return;
            }

            var previous = this.fillTexture;
            this.m_fillTextureFromStyle = texture;
            if (previous == this.fillTexture)
            {
                return;
            }

            this.m_Image.MarkDirtyRepaint();
            this.NotifyPropertyChanged(in FillTextureProperty);
        }
    }
}
