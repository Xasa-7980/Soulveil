//////////////////////////////////////////////////////
// MK Install Wizard Configuration            		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
using Configuration = MK.Glow.Editor.InstallWizard.Configuration;
namespace MK.Glow.Editor.InstallWizard
{
    //[CreateAssetMenu(fileName = "Configuration", menuName = "MK/Install Wizard/Create Configuration Asset")]
    #if UNITY_6000_5_OR_NEWER
    public sealed partial class Configuration : ScriptableObject
    #else
    public sealed class Configuration : ScriptableObject
    #endif
    {
        #pragma warning disable CS0414

        internal static bool isReady 
        { 
            get
            { 
                if(_instance == null)
                    TryGetInstance();
                return _instance != null; 
            } 
        }
        
        [SerializeField]
        private RenderPipeline _renderPipeline = RenderPipeline.Built_in_PostProcessingStack;

        [SerializeField]
        internal bool showInstallerOnReload = true;

        [SerializeField][Space]
        private Texture2D _titleImage = null;

        [SerializeField][Space]
        private Object _readMe = null;

        [SerializeField][Space]
        private Object _basePackageBuiltin = null;
        [SerializeField]
        private Object _basePackageLWRP = null;
        [SerializeField]
        private Object _basePackageURP = null;
        [SerializeField]
        private Object _basePackageHDRP = null;

        [SerializeField][Space]
        private Object _examplesPackageInc = null;
        [SerializeField]
        private Object _examplesPackageBuiltin = null;
        [SerializeField]
        private Object _examplesPackageLWRP = null;
        [SerializeField]
        private Object _examplesPackageURP = null;
        [SerializeField]
        private Object _examplesPackageURP2D = null;
        [SerializeField]
        private Object _examplesPackageHDRP = null;
        [SerializeField]
        private Object _examplesSkyHDRP = null;

        [SerializeField][Space]
        private ExampleContainer[] _examples = null;

        private static void LogAssetNotFoundError()
        {
            //Debug.LogError("Could not find Install Wizard Configuration Asset, please try to import the package again.");
        }

        #if UNITY_6000_5_OR_NEWER
        [Unity.Scripting.LifecycleManagement.AutoStaticsCleanup]
        #endif
        private static MK.Glow.Editor.InstallWizard.Configuration _instance = null;
        
        [InitializeOnLoadMethod]
        internal static MK.Glow.Editor.InstallWizard.Configuration TryGetInstance()
        {
            if(_instance == null)
            {
                string[] _guids = AssetDatabase.FindAssets("t:" + typeof(MK.Glow.Editor.InstallWizard.Configuration).Namespace + ".Configuration", null);
                if(_guids.Length > 0)
                {
                    _instance = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_guids[0]), typeof(MK.Glow.Editor.InstallWizard.Configuration)) as Configuration;
                    if(_instance != null)
                        return _instance;
                    else
                    {
                        LogAssetNotFoundError();
                        return null;
                    }
                }
                else
                {
                    LogAssetNotFoundError();
                    return null;
                }
            }
            else
                return _instance;
        }

        internal static string TryGetPath()
        {
            if(isReady)
            {
                return AssetDatabase.GetAssetPath(_instance);
            }
            else
            {
                return string.Empty;
            }
        }

        internal static Texture2D TryGetTitleImage()
        {
            if(isReady)
            {
                return _instance._titleImage;
            }
            else
            {
                return null;
            }
        }

        internal static ExampleContainer[] TryGetExamples()
        {
            if(isReady)
            {
                return _instance._examples;
            }
            else
            {
                return null;
            }
        }

        internal static bool TryGetShowInstallerOnReload()
        {
            if(isReady)
            {
                return _instance.showInstallerOnReload;
            }
            else
            {
                return false;
            }
        }
        internal static void TrySetShowInstallerOnReload(bool v)
        {
            if(isReady)
            {
                if(_instance.showInstallerOnReload == v)
                    return;

                _instance.showInstallerOnReload = v;
                SaveInstance();
            }
        }

        internal static RenderPipeline TryGetRenderPipeline()
        {
            if(isReady)
            {
                return _instance._renderPipeline;
            }
            else
            {
                return RenderPipeline.Built_in_PostProcessingStack;
            }
        }
        internal static void TrySetRenderPipeline(RenderPipeline v)
        {
            if(isReady)
            {
                if(_instance._renderPipeline == v)
                    return;

                _instance._renderPipeline = v;

                SaveInstance();
            }
        }

        internal static void SaveInstance()
        {
            if(isReady)
            {
                EditorUtility.SetDirty(_instance);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        internal static void ImportShaders(RenderPipeline renderPipeline)
        {
            if(isReady)
            {
                switch(renderPipeline)
                {
                    case RenderPipeline.Built_in_Legacy:
                        #if UNITY_6000_6_OR_NEWER
                        UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._basePackageBuiltin), false);
                        #else
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._basePackageBuiltin), false);
                        #endif
                    break;
                    case RenderPipeline.Built_in_PostProcessingStack:
                        #if UNITY_6000_6_OR_NEWER
                        UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._basePackageLWRP), false);
                        #else
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._basePackageLWRP), false);
                        #endif
                    break;
                    //case RenderPipeline.Lightweight:
                    //    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._basePackageLWRP), false);
                    //break;
                    case RenderPipeline.Universal3D:
                    #if UNITY_2021_2_OR_NEWER
                    case RenderPipeline.Universal2D:
                    #endif
                        #if UNITY_6000_6_OR_NEWER
                        UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._basePackageURP), false);
                        #else
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._basePackageURP), false);
                        #endif
                    break;
                    case RenderPipeline.High_Definition:
                        #if UNITY_6000_6_OR_NEWER
                        UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._basePackageHDRP), false);
                        #else
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._basePackageHDRP), false);
                        #endif
                    break;
                    default:
                    //All cases should be handled
                    break;
                }
                TrySetShowInstallerOnReload(false);
            }
        }

        internal static void ImportExamples(RenderPipeline renderPipeline)
        {
            if(isReady)
            {
                #if UNITY_6000_6_OR_NEWER
                UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._examplesPackageInc), false);
                #else
                AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageInc), false);
                #endif
                switch(renderPipeline)
                {
                    case RenderPipeline.Built_in_Legacy:
                        #if UNITY_6000_6_OR_NEWER
                        UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._examplesPackageBuiltin), false);
                        #else
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageBuiltin), false);
                        #endif
                    break;
                    case RenderPipeline.Built_in_PostProcessingStack:
                        #if UNITY_6000_6_OR_NEWER
                        UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._examplesPackageLWRP), false);
                        #else
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageLWRP), false);
                        #endif
                    break;
                    //case RenderPipeline.Lightweight:
                    //    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageLWRP), false);
                    //break;
                    case RenderPipeline.Universal3D:
                        #if UNITY_6000_6_OR_NEWER
                        UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._examplesPackageURP), false);
                        #else
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageURP), false);
                        #endif
                    break;
                    #if UNITY_2021_2_OR_NEWER
                    case RenderPipeline.Universal2D:
                        #if UNITY_6000_6_OR_NEWER
                        UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._examplesPackageURP2D), false);
                        #else
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageURP2D), false);
                        #endif
                    break;
                    #endif
                    case RenderPipeline.High_Definition:
                        #if UNITY_6000_6_OR_NEWER
                        UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._examplesSkyHDRP), false);
                        UnityEditor.AssetPackage.Package.Import(AssetDatabase.GetAssetPath(_instance._examplesPackageHDRP), false);
                        #else
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesSkyHDRP), false);
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageHDRP), false);
                        #endif
                    break;
                }
            }
        }

        internal static void OpenReadMe()
        {
            if(isReady)
            {
                AssetDatabase.OpenAsset(_instance._readMe);
            }
        }
        #pragma warning restore CS0414
    }
}
#endif