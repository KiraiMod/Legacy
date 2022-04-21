using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KiraiMod
{
    public static class ModuleLoader
    {
        private static List<(object, Attribute)> elements = new List<(object, Attribute)>();

        public static void Initialize()
        {
            Type[] modules = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace.StartsWith("KiraiMod.Modules"))
                .Where(t => t.IsAbstract && t.IsSealed)
                .ToArray();

            KiraiLib.Logger.Trace($"Found {modules.Length} modules");

            foreach (Type module in modules)
            {
                foreach (MethodInfo method in module.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    elements.Add((method, method.GetCustomAttribute<UIButtonAttribute>()));
                foreach (FieldInfo field in module.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    elements.Add((field, field.GetCustomAttribute<UIToggleAttribute>()));
                    elements.Add((field, field.GetCustomAttribute<UISliderAttribute>()));
                }
                foreach (PropertyInfo prop in module.GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    elements.Add((prop, prop.GetCustomAttribute<UIToggleAttribute>()));
                    elements.Add((prop, prop.GetCustomAttribute<UISliderAttribute>()));
                }
            }
        }

        public static void CreateUIElements()
        {
            foreach ((object, Attribute) element in elements)
            {
                if (element.Item2 is UIToggleAttribute)
                {
                    UIToggleAttribute attrib = element.Item2 as UIToggleAttribute;

                    Action<bool> action;
                    bool cval;

                    if (element.Item1 is FieldInfo)
                    {
                        FieldInfo info = element.Item1 as FieldInfo;
                        cval = (bool)info.GetValue(null);

                        action = (bool state) =>
                        {
                            if (state == cval) return;

                            cval = !cval;

                            KiraiLib.Logger.Debug($"{info.DeclaringType.Name}.{attrib.reference} {(cval ? "On" : "Off")}");

                            info.SetValue(null, state);
                        };
                    }
                    else
                    {
                        PropertyInfo info = element.Item1 as PropertyInfo;
                        cval = (bool)info.GetValue(null);

                        action = (bool state) =>
                        {
                            if (state == cval) return;

                            cval = !cval;

                            KiraiLib.Logger.Debug($"{info.DeclaringType.Name}.{attrib.reference} {(cval ? "On" : "Off")}");

                            info.SetValue(null, state);
                        };
                    }   

                    KiraiLib.UI.Toggle.Create(
                        Utils.CreateID(attrib.label, (int)attrib.page),
                        attrib.label,
                        attrib.description,
                        attrib.x,
                        attrib.y,
                        cval,
                        KiraiLib.UI.pages[Shared.PageRemap[(int)attrib.page]].transform,
                        action);
                }
                else if (element.Item2 is UIButtonAttribute)
                {
                    MethodInfo info = element.Item1 as MethodInfo;
                    UIButtonAttribute attrib = element.Item2 as UIButtonAttribute;
                    KiraiLib.UI.Button.Create(
                        Utils.CreateID(attrib.label, (int)attrib.page),
                        attrib.label,
                        attrib.description,
                        attrib.x,
                        attrib.y,
                        KiraiLib.UI.pages[Shared.PageRemap[(int)attrib.page]].transform,
                        () => info.Invoke(null, null));
                }
                else if (element.Item2 is UISliderAttribute)
                {
                    UISliderAttribute attrib = element.Item2 as UISliderAttribute;

                    Action<float> action;

                    float cval;
                    
                    if (element.Item1 is FieldInfo)
                    {
                        FieldInfo info = element.Item1 as FieldInfo;
                        cval = (float)info.GetValue(null);
                        action = (float value) => info.SetValue(null, value);
                    }
                    else
                    {
                        PropertyInfo info = element.Item1 as PropertyInfo;
                        cval = (float)info.GetValue(null);
                        action = (float value) => info.SetValue(null, value);
                    }

                    KiraiLib.UI.Slider.Create(
                        Utils.CreateID(attrib.label, (int)attrib.page),
                        attrib.label,
                        attrib.x,
                        attrib.y,
                        attrib.min,
                        attrib.max,
                        cval,
                        KiraiLib.UI.pages[Shared.PageRemap[(int)attrib.page]].transform,
                        action);
                }
            }
        }

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class UIToggleAttribute : Attribute
        {
            public string label;
            public string description;
            public Shared.PageIndex page;
            public float x;
            public float y;
            public string reference;

            public UIToggleAttribute(string label, string description, Shared.PageIndex page, float x, float y, [CallerMemberName] string reference = null)
            {
                this.label = label;
                this.description = description;
                this.page = page;
                this.x = x;
                this.y = y;
                this.reference = reference;
            }

            public UIToggleAttribute(string label, string description, Shared.PageIndex page, int index, [CallerMemberName] string reference = null)
            {
                this.label = label;
                this.description = description;
                this.page = page;

                Utils.GetGenericLayout(index, out int x, out int y);
                this.x = x;
                this.y = y;

                this.reference = reference;
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class UIButtonAttribute : Attribute
        {
            public string label;
            public string description;
            public Shared.PageIndex page;
            public float x;
            public float y;
            public string reference;

            public UIButtonAttribute(string label, string description, Shared.PageIndex page, float x, float y, [CallerMemberName] string reference = null)
            {
                this.label = label;
                this.description = description;
                this.page = page;
                this.x = x;
                this.y = y;
                this.reference = reference;
            }

            public UIButtonAttribute(string label, string description, Shared.PageIndex page, int index, [CallerMemberName] string reference = null)
            {
                this.label = label;
                this.description = description;
                this.page = page;

                Utils.GetGenericLayout(index, out int x, out int y);
                this.x = x;
                this.y = y;

                this.reference = reference;
            }
        }

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class UISliderAttribute : Attribute
        {
            public string label;
            public Shared.PageIndex page;
            public float x;
            public float y;
            public string reference;
            public float min;
            public float max;

            public UISliderAttribute(string label, Shared.PageIndex page, float x, float y, float min, float max, [CallerMemberName] string reference = null)
            {
                this.label = label;
                this.page = page;
                this.x = x;
                this.y = y;
                this.min = min;
                this.max = max;
                this.reference = reference;
            }

            public UISliderAttribute(string label, Shared.PageIndex page, int index, float min, float max, [CallerMemberName] string reference = null)
            {
                this.label = label;
                this.page = page;

                Utils.GetSliderLayout(index, out float x, out float y);
                this.x = x;
                this.y = y;

                this.min = min;
                this.max = max;
                this.reference = reference;
            }
        }
    }
}
