using NGTools.NGRemoteScene;
using System;
using UnityEditor;
using UnityEngine;

namespace NGToolsEditor.NGRemoteScene
{
	[TypeHandlerDrawerFor(typeof(UInt16Handler))]
	internal sealed class UInt16Drawer : TypeHandlerDrawer
	{
		private BgColorContentAnimator	anim;
		private ValueMemorizer<UInt16>	drag;

		public	UInt16Drawer(TypeHandler typeHandler) : base(typeHandler)
		{
			this.drag = new ValueMemorizer<UInt16>();
		}

		public override void Draw(Rect r, DataDrawer data)
		{
			if (this.anim == null)
				this.anim = new BgColorContentAnimator(data.inspector.Repaint, 1F, 0F);

			if (data.inspector.Hierarchy.GetUpdateNotification(data.GetPath()) == true)
			{
				this.drag.NewValue((UInt16)data.value);
				this.anim.Start();
			}

			using (this.anim.Restorer(0F, .8F + this.anim.Value, 0F, 1F))
			{
				EditorGUI.BeginChangeCheck();
				UInt16	newValue = Convert.ToUInt16(EditorGUI.IntField(r, Utility.NicifyVariableName(data.name), Convert.ToInt32(this.drag.Get((UInt16)data.value))));
				if (EditorGUI.EndChangeCheck() == true)
				{
					this.drag.Set(newValue);
					this.AsyncUpdateCommand(data.unityData, data.GetPath(), newValue, typeof(UInt16));
				}

				this.drag.Draw(r);
			}
		}
	}
}