﻿/****************************************************************************
 * Copyright (c) 2017 magicbell
 * Copyright (c) 2017 liangxie
 * Copyright (c) 2018.3 liangxie
 * 
 * http://qframework.io
 * https://github.com/liangxiegame/QFramework
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 ****************************************************************************/


namespace QFramework
{
	using UnityEngine;
	using System.Collections.Generic;
	using UnityEngine.UI;
	using System;

#if SLUA_SUPPORT
	using SLua;
#endif

	public class UILevel
	{
		public const int Bg                 = -2;  //背景层UI
		public const int AnimationUnderPage = -1; //动画层
		public const int Common             = 0; //普通层UI
		public const int AnimationOnPage    = 1; // 动画层
		public const int PopUI              = 2; //弹出层UI
		public const int Guide              = 3; //新手引导层
		public const int Const              = 4; //持续存在层UI
		public const int Toast              = 5; //对话框层UI
		public const int Forward            = 6; //最高UI层用来放置UI特效和模型
	}

#if SLUA_SUPPORT
	[CustomLuaClass]
#endif
	//// <summary>
	/// UGUI UI界面管理器
	/// </summary>
	public class QUIManager : QMgrBehaviour, ISingleton
	{
		Dictionary<string, IUIBehaviour> mAllUI = new Dictionary<string, IUIBehaviour>();

		[SerializeField] Transform mBgTrans;
		[SerializeField] Transform mAnimationUnderPageTrans;
		[SerializeField] Transform mCommonTrans;
		[SerializeField] Transform mAnimationOnPageTrans;
		[SerializeField] Transform mPopUITrans;
		[SerializeField] Transform mConstTrans;
		[SerializeField] Transform mToastTrans;
		[SerializeField] Transform mForwardTrans;
		[SerializeField] Camera mUICamera;
		[SerializeField] Canvas mCanvas;
		[SerializeField] CanvasScaler mCanvasScaler;
		[SerializeField] GraphicRaycaster mGraphicRaycaster;

		public void OnSingletonInit()
		{
			Log.I("QUIManager Init");
		}

		private static QUIManager mInstance;

		public static QUIManager Instance
		{
			get
			{
				if (null == mInstance)
				{
					mInstance = FindObjectOfType<QUIManager>();
				}

				if (null == mInstance)
				{
					Instantiate(Resources.Load<GameObject>("QUIManager"));
					mInstance = QMonoSingletonProperty<QUIManager>.Instance;
					mInstance.name = "QUIManager";
					DontDestroyOnLoad(mInstance);
				}

				return mInstance;
			}
		}

		public Canvas RootCanvas
		{
			get { return mCanvas; }
		}

		public Camera UICamera
		{
			get { return mUICamera; }
		}

		// TODO: 全局唯一事件管理
		public GraphicRaycaster GlobalGraphicRaycaster
		{
			get { return mGraphicRaycaster; }
		}

		public void SetResolution(int width, int height)
		{
			mCanvasScaler.referenceResolution = new UnityEngine.Vector2(width, height);
		}

		public void SetMatchOnWidthOrHeight(float heightPercent)
		{
			mCanvasScaler.matchWidthOrHeight = heightPercent;
		}

		public IUIBehaviour OpenUI(string uiBehaviourName, int canvasLevel)
		{
			if (!mAllUI.ContainsKey(uiBehaviourName))
			{
				CreateUI(uiBehaviourName, canvasLevel);
			}

			mAllUI[uiBehaviourName].Show();
			return mAllUI[uiBehaviourName];
		}


		/// <summary>
		/// 创建UIPanel
		/// </summary>
		/// <param name="uiBehaviourName"></param>
		/// <param name="uiLevel"></param>
		/// <param name="initData"></param>
		/// <returns></returns>
		public GameObject CreateUIObj(string uiBehaviourName, int uiLevel,string assetBundleName = null)
		{
			IUIBehaviour ui;
			if (mAllUI.TryGetValue(uiBehaviourName, out ui))
			{
				Log.W("{0}: already exist", uiBehaviourName);
				// 直接返回,不要再调一次Init(),Init()应该只能调用一次
				return ui.Transform.gameObject;
			}

			ui = QUIBehaviour.Load(uiBehaviourName,assetBundleName);

			switch (uiLevel)
			{
				case UILevel.Bg:
					ui.Transform.SetParent(mBgTrans);
					break;
				case UILevel.AnimationUnderPage:
					ui.Transform.SetParent(mAnimationUnderPageTrans);
					break;
				case UILevel.Common:
					ui.Transform.SetParent(mCommonTrans);
					break;
				case UILevel.AnimationOnPage:
					ui.Transform.SetParent(mAnimationOnPageTrans);
					break;
				case UILevel.PopUI:
					ui.Transform.SetParent(mPopUITrans);
					break;
				case UILevel.Const:
					ui.Transform.SetParent(mConstTrans);
					break;
				case UILevel.Toast:
					ui.Transform.SetParent(mToastTrans);
					break;
				case UILevel.Forward:
					ui.Transform.SetParent(mForwardTrans);
					break;
			}

			var uiGoRectTrans = ui.Transform as RectTransform;

			uiGoRectTrans.offsetMin = Vector2.zero;
			uiGoRectTrans.offsetMax = Vector2.zero;
			uiGoRectTrans.anchoredPosition3D = Vector3.zero;
			uiGoRectTrans.anchorMin = Vector2.zero;
			uiGoRectTrans.anchorMax = Vector2.one;

			ui.Transform.LocalScaleIdentity();
			ui.Transform.gameObject.name = uiBehaviourName;

			return ui.Transform.gameObject;
		}

		/// <summary>
		/// 显示UIBehaiviour
		/// </summary>
		/// <param name="uiBehaviourName"></param>
		public void ShowUI(string uiBehaviourName)
		{
			IUIBehaviour uiBehaviour = null;
			if (mAllUI.TryGetValue(uiBehaviourName, out uiBehaviour))
			{
				uiBehaviour.Show();
			}
		}

		/// <summary>
		/// 隐藏UI
		/// </summary>
		/// <param name="uiBehaviourName"></param>
		public void HideUI(string uiBehaviourName)
		{
			IUIBehaviour uiBehaviour = null;
			if (mAllUI.TryGetValue(uiBehaviourName, out uiBehaviour))
			{
				uiBehaviour.Hide();
			}
		}

		/// <summary>
		/// 删除所有UI层
		/// </summary>
		public void CloseAllUI()
		{
			foreach (var layer in mAllUI)
			{
				Destroy(layer.Value.Transform.gameObject);
			}

			mAllUI.Clear();
		}

		/// <summary>
		/// 关闭并卸载UI
		/// </summary>
		/// <param name="behaviourName"></param>
		public void CloseUI(string behaviourName)
		{
			IUIBehaviour behaviour = null;

			mAllUI.TryGetValue(behaviourName, out behaviour);

			if (null != behaviour)
			{
				behaviour.Close();
				mAllUI.Remove(behaviourName);
			}
		}

		/// <summary>
		/// 获取UIBehaviour
		/// </summary>
		/// <param name="uiBehaviourName"></param>
		/// <returns></returns>
		public QUIBehaviour GetUI(string uiBehaviourName)
		{
			IUIBehaviour retUiBehaviour = null;
			if (mAllUI.TryGetValue(uiBehaviourName, out retUiBehaviour))
			{
				return retUiBehaviour as QUIBehaviour;
			}
			return null;
		}

		/// <summary>
		/// 获取UI相机
		/// </summary>
		/// <returns></returns>
		public Camera GetUICamera()
		{
			return mUICamera;
		}

		protected override void SetupMgrId()
		{
			mMgrId = QMgrID.UI;
		}

		/// <summary>
		/// 命名空间对应名字的缓存
		/// </summary>
		private Dictionary<string, string> mFullname4UIBehaviourName = new Dictionary<string, string>();

		private string GetUIBehaviourName<T>()
		{
			string fullBehaviourName = typeof(T).ToString();
			string retValue = null;

			if (mFullname4UIBehaviourName.ContainsKey(fullBehaviourName))
			{
				retValue = mFullname4UIBehaviourName[fullBehaviourName];
			}
			else
			{
				string[] nameSplits = fullBehaviourName.Split(new char[] {'.'});
				retValue = nameSplits[nameSplits.Length - 1];
				mFullname4UIBehaviourName.Add(fullBehaviourName, retValue);
			}

			return retValue;
		}

		public IUIBehaviour CreateUI(string uiBehaviourName, int level, IUIData uiData = null,string assetBundleName = null)
		{
			IUIBehaviour ui;
			if (mAllUI.TryGetValue(uiBehaviourName, out ui))
			{
				return ui;
			}
			var uiObj = CreateUIObj(uiBehaviourName, level, assetBundleName);

			ui = uiObj.GetComponent<IUIBehaviour>();

			mAllUI.Add(uiBehaviourName, ui);

			ui.Init(uiData);

			return ui;
		}

		#region UnityCSharp Generic Support

		/// <summary>
		/// Create&ShowUI
		/// </summary>
		public T OpenUI<T>(int canvasLevel = UILevel.Common, IUIData uiData = null,string assetBundleName = null,string prefabName = null) where T : QUIBehaviour
		{
			var behaviourName = prefabName ?? GetUIBehaviourName<T>();

			if (!mAllUI.ContainsKey(behaviourName))
			{
				CreateUI(behaviourName, canvasLevel, uiData,assetBundleName);
			}

			mAllUI[behaviourName].Show();
			return mAllUI[behaviourName] as T;
		}


		public void ShowUI<T>() where T : QUIBehaviour
		{
			ShowUI(GetUIBehaviourName<T>());
		}

		public void HideUI<T>() where T : QUIBehaviour
		{
			HideUI(GetUIBehaviourName<T>());
		}

		public void CloseUI<T>() where T : QUIBehaviour
		{
			CloseUI(GetUIBehaviourName<T>());
		}

		public T GetUI<T>() where T : QUIBehaviour
		{
			return GetUI(GetUIBehaviourName<T>()) as T;
		}

		#endregion

		#region LuaSupport

		#endregion
	}

	public static class UIMgr
	{

		internal static T OpenPanel<T>(int canvasLevel = UILevel.Common, IUIData uiData = null, string assetBundleName = null,
			string prefabName = null) where T : QUIBehaviour
		{
			return QUIManager.Instance.OpenUI<T>(canvasLevel, uiData, assetBundleName,prefabName);
		}

		internal static void ClosePanel<T>() where T : QUIBehaviour
		{
			QUIManager.Instance.CloseUI<T>();
		}

		internal static T GetPanel<T>() where T : QUIBehaviour
		{
			return QUIManager.Instance.GetUI<T>();
		}


		#region 给脚本层用的api
		public static QUIBehaviour GetPanel(string panelName)
		{
			return QUIManager.Instance.GetUI(panelName);
		}
		
//		public static QUIBehaviour OpenPanel(string panelName,int level = 0,string assetBundleName = null)
//		{
//			return QUIManager.Instance.OpenUI(panelName,level,assetBundleName) as QUIBehaviour;
//		}

		public static void ClosePanel(string panelName)
		{
			QUIManager.Instance.CloseUI(panelName);
		}
		#endregion
	}
}