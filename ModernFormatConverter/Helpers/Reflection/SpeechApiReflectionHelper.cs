using ModernFormatConverter.Services.Root;
using System;
using System.Collections;
using System.Reflection;
using System.Speech.Synthesis;

namespace ModernFormatConverter.Helpers.Reflection
{
    /// <summary>
    /// 语音辅助类，添加 Speech OneCore 语音路径用以扫描
    /// </summary>
    public static class SpeechApiReflectionHelper
    {
        private const string PROP_VOICE_SYNTHESIZER = "VoiceSynthesizer";
        private const string FIELD_INSTALLED_VOICES = "_installedVoices";
        private const string ONE_CORE_VOICES_REGISTRY = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech_OneCore\Voices";
        private static readonly Type ObjectTokenCategoryType = typeof(SpeechSynthesizer).Assembly.GetType("System.Speech.Internal.ObjectTokens.ObjectTokenCategory")!;
        private static readonly Type VoiceInfoType = typeof(SpeechSynthesizer).Assembly.GetType("System.Speech.Synthesis.VoiceInfo")!;
        private static readonly Type InstalledVoiceType = typeof(SpeechSynthesizer).Assembly.GetType("System.Speech.Synthesis.InstalledVoice")!;

        /// <summary>
        /// 注入 Speech OneCore 存储路径
        /// </summary>
        public static bool InjectOneCoreVoices(this SpeechSynthesizer synthesizer)
        {
            try
            {
                object voiceSynthesizer = GetProperty(synthesizer, PROP_VOICE_SYNTHESIZER) ?? throw new NotSupportedException($"Property not found: {PROP_VOICE_SYNTHESIZER}");
                if (GetField(voiceSynthesizer, FIELD_INSTALLED_VOICES) is not IList installedVoices)
                {
                    throw new NotSupportedException($"Field not found or null: {FIELD_INSTALLED_VOICES}");
                }

                if (ObjectTokenCategoryType.GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, [ONE_CORE_VOICES_REGISTRY]) is not IDisposable otc)
                {
                    throw new NotSupportedException($"Failed to call Create on {ObjectTokenCategoryType} instance");
                }

                using (otc)
                {
                    if (ObjectTokenCategoryType.GetMethod("FindMatchingTokens", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(otc, [null, null]) is not IList tokens)
                    {
                        throw new NotSupportedException($"Failed to list matching tokens");
                    }

                    foreach (object token in tokens)
                    {
                        if (token is null || GetProperty(token, "Attributes") is null)
                        {
                            continue;
                        }

                        object voiceInfo = typeof(SpeechSynthesizer).Assembly.CreateInstance(VoiceInfoType.FullName!, true, BindingFlags.Instance | BindingFlags.NonPublic, null, [token], null, null) ?? throw new NotSupportedException($"Failed to instantiate {VoiceInfoType}");
                        object installedVoice = typeof(SpeechSynthesizer).Assembly.CreateInstance(InstalledVoiceType.FullName!, true, BindingFlags.Instance | BindingFlags.NonPublic, null, [voiceSynthesizer, voiceInfo], null, null) ?? throw new NotSupportedException($"Failed to instantiate {InstalledVoiceType}");
                        installedVoices.Add(installedVoice);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                LogService.WriteLog(System.Diagnostics.TraceEventType.Error, nameof(ModernFormatConverter), nameof(SpeechApiReflectionHelper), nameof(InjectOneCoreVoices), 1, e);
                return false;
            }
        }

        private static object GetProperty(object target, string propName)
        {
            return target.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target);
        }

        private static object GetField(object target, string propName)
        {
            return target.GetType().GetField(propName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target);
        }
    }
}
