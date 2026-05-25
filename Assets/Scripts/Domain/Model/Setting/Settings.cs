using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Domain.Model.Setting
{
    public class WorldSettings
    {
        public readonly CheckBox EnableCheat = new("チートを有効にする", false);
        public readonly CheckBox RetryOnDead;
        public readonly CheckBox AutoIdentify;
        public readonly CheckBox IgnoreWall;
        public WorldSettings()
        {
            RetryOnDead = new("死亡時にリトライ可能", false, EnableCheat.Value);
            AutoIdentify = new("全てのアイテムが自動で識別される", false, EnableCheat.Value);
            IgnoreWall = new("壁を無視する", false, EnableCheat.Value);
        }

        public List<IOptionInput> GetOptions()
        {
            List<IOptionInput> setters = new();
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(this);
                if (typeof(IOptionInput).IsAssignableFrom(field.FieldType))
                {
                    setters.Add((IOptionInput)value);
                }
            }

            return setters;
        }

        public void Reset()
        {
            foreach (var option in GetOptions())
                option.Reset();
        }

        public void SetValues(Dictionary<string, int> values)
        {
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(this);
                if (values.TryGetValue(field.Name, out var intValue))
                {

                    switch (value)
                    {
                        case Slider slider:
                            slider.Value.Value = intValue;
                            break;
                        case LabeledSlider labeledSlider:
                            labeledSlider.Index.Value = intValue;
                            break;
                        case CheckBox checkBox:
                            checkBox.Value.Value = intValue == 1;
                            break;
                    }
                }
            }
        }

        public Dictionary<string, int> GetValues()
        {
            var settings = new Dictionary<string, int>();
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(this);
                if (typeof(IOptionInput).IsAssignableFrom(field.FieldType))
                {
                    var option = (IOptionInput)value switch
                    {
                        Slider slider => slider.CurrentValue,
                        LabeledSlider labeledSlider => labeledSlider.CurrentIndex,
                        CheckBox checkBox => checkBox.CurrentValue ? 1 : 0,
                        _ => throw new InvalidOperationException(""),
                    };
                    settings.Add(field.Name, option);
                }
            }
            return settings;
        }
    }
    public class GlobalSettings
    {
        public readonly Slider BGMVolume = new("BGM音量", 0, 100, 50);
        public readonly Slider SEVolume = new("SE音量", 0, 100, 50);
        public readonly Slider MoveMilliseconds = new("移動時間[ms]", 1, 1000, 150);
        public readonly Slider DashMilliseconds = new("ダッシュ時移動時間[ms]", 1, 1000, 20);
        public readonly Slider ThrowMilliseconds = new("吹き飛ばし時間[ms]", 1, 1000, 50);
        public readonly Slider EffectDisplayTime = new("エフェクト表示時間[ms]", 10, 1000, 100);
        public readonly Slider LogShownMilliSeconds = new("ログ表示時間[ms]", 10, 10000, 5000);
        public readonly Slider DamageTextDisplayTime = new("ダメージテキスト表示時間[ms]", 10, 3000, 500);
        public readonly Slider FlushDuration = new("フラッシュ時間[ms]", 10, 5000, 1000);
        public readonly Slider SignificantDamageThresholdPercentage = new("大ダメージ閾値[%]", 1, 100, 25);
        public readonly Slider LowHpThresholdPercentage = new("低HP警告閾値[%]", 1, 100, 25);
        public readonly CheckBox IntelligentDash = new("スマートダッシュ", true);
        public readonly CheckBox AutoPickUpShopItem = new("店のアイテムを自動で拾う", false);
        public readonly Slider DashPauseMilliseconds = new("分岐一時停止時間[ms]", 100, 1000, 250);
        public readonly CheckBox SwapABXY = new("A/B・X/Yボタンを入れ替える", false);
        public readonly CheckBox AutoSave = new("自動でセーブする", false);
        public GlobalSettings() { }

        public List<IOptionInput> GetOptions()
        {
            List<IOptionInput> setters = new();
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(this);
                if (typeof(IOptionInput).IsAssignableFrom(field.FieldType))
                {
                    setters.Add((IOptionInput)value);
                }
            }

            return setters;
        }

        public void Reset()
        {
            foreach (var option in GetOptions())
                option.Reset();
        }

        public void SetValues(Dictionary<string, int> values)
        {
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(this);
                if (values.TryGetValue(field.Name, out var intValue))
                {
                    switch (value)
                    {
                        case Slider slider:
                            slider.Value.Value = intValue;
                            break;
                        case LabeledSlider labeledSlider:
                            labeledSlider.Index.Value = intValue;
                            break;
                        case CheckBox checkBox:
                            checkBox.Value.Value = intValue == 1;
                            break;
                    }
                }
            }
        }

        public Dictionary<string, int> GetValues()
        {
            var settings = new Dictionary<string, int>();
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(this);
                if (typeof(IOptionInput).IsAssignableFrom(field.FieldType))
                {
                    var option = (IOptionInput)value switch
                    {
                        Slider slider => slider.CurrentValue,
                        LabeledSlider labeledSlider => labeledSlider.CurrentIndex,
                        CheckBox checkBox => checkBox.CurrentValue ? 1 : 0,
                        _ => throw new InvalidOperationException(""),
                    };
                    settings.Add(field.Name, option);
                }
            }
            return settings;
        }
    }
    public static class Settings
    {
        public static GlobalSettings GlobalSettings = new();
        public static WorldSettings WorldSettings = new();

        public static List<IOptionInput> GetOptions()
        {
            List<IOptionInput> options = new();
            foreach (var option in GlobalSettings.GetOptions())
                options.Add(option);

            foreach (var option in WorldSettings.GetOptions())
                options.Add(option);

            return options;
        }

        public static void Reset()
        {
            GlobalSettings.Reset();
            WorldSettings.Reset();
        }

        public static void SetValues(Dictionary<string, int> values)
        {
            GlobalSettings.SetValues(values);
            WorldSettings.SetValues(values);
        }

        public static Dictionary<string, int> GetValues()
        {
            var settings = new Dictionary<string, int>();
            foreach (var setting in GlobalSettings.GetValues())
                settings.Add(setting.Key, setting.Value);

            foreach (var setting in WorldSettings.GetValues())
                settings.Add(setting.Key, setting.Value);
            return settings;
        }
    }
}