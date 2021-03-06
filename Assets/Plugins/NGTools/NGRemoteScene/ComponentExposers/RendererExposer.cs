// File auto-generated by ExposerGenerator.
using System.Reflection;
using UnityEngine;

namespace NGTools.NGRemoteScene
{
	internal sealed class RendererExposer : ComponentExposer
	{
		public	RendererExposer() : base(typeof(Renderer))
		{
		}

		public override PropertyInfo[]	GetPropertyInfos()
		{
			return new PropertyInfo[] {
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("enabled"),
#endif
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7
				this.type.GetProperty("castShadows"),
#endif
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("receiveShadows"),
#endif
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("sharedMaterials"),
#endif
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("lightmapIndex"),
#endif
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7
				this.type.GetProperty("lightmapTilingOffset"),
#endif
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
				this.type.GetProperty("useLightProbes"),
#endif
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7
				this.type.GetProperty("lightProbeAnchor"),
#endif
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("sortingLayerName"),
#endif
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("sortingLayerID"),
#endif
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("sortingOrder"),
#endif
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("shadowCastingMode"),
#endif
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("lightmapScaleOffset"),
#endif
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("realtimeLightmapScaleOffset"),
#endif
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("probeAnchor"),
#endif
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("reflectionProbeUsage"),
#endif
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("realtimeLightmapIndex"),
#endif
#if UNITY_5_4
				this.type.GetProperty("motionVectors"),
#endif
#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("lightProbeUsage"),
#endif
#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("lightProbeProxyVolumeOverride"),
#endif
#if UNITY_5_5 || UNITY_5_6 || UNITY_5_6_OR_NEWER
				this.type.GetProperty("motionVectorGenerationMode"),
#endif
			};
		}
	}
}