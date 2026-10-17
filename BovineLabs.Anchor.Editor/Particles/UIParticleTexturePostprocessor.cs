namespace BovineLabs.Anchor.Editor.Particles
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Particles;
    using UnityEditor;
    using UnityEngine;

    internal sealed class UIParticleTexturePostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            var changed = new HashSet<string>(imported, StringComparer.Ordinal);
            changed.UnionWith(deleted);
            changed.UnionWith(moved);
            changed.UnionWith(movedFrom);
            foreach (var effect in Resources.FindObjectsOfTypeAll<UIParticleEffect>())
            {
                foreach (var emitter in effect.Emitters)
                {
                    if (emitter == null)
                    {
                        continue;
                    }

                    UnityEngine.Object source = emitter.TextureSource == UIParticleTextureSource.Sprite ? emitter.Sprite : emitter.Texture;
                    if (source != null && changed.Contains(AssetDatabase.GetAssetPath(source)))
                    {
                        effect.Invalidate();
                        break;
                    }
                }
            }
        }
    }
}
