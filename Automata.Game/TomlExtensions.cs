using System;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

namespace Automata.Engine.Extensions
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TomlPropertyAttribute : Attribute
    {
        public string? Header { get; }
        public bool Required { get; }

        public TomlPropertyAttribute(string? header, bool required) => (Header, Required) = (header, required);
    }

    public static class TomlExtensions
    {
        public static T ToModel<T>(this DocumentSyntax documentSyntax) where T : new()
        {
            T instance = new T();
            TomlTable model = documentSyntax.ToModel();

            foreach (PropertyInfo property in instance.GetType().GetProperties())
            {
                TomlPropertyAttribute? attribute = property.GetCustomAttribute<TomlPropertyAttribute>();

                if (attribute is null) continue;

                if (attribute.Header is null)
                {
                    if (attribute.Required && !model.ContainsKey(property.Name))
                        throw new Exception($"Toml file does not have required property '{property.Name}'.");

                    property.SetValue(instance, model[property.Name]);
                }
                else
                {
                    if ((attribute.Required && !model.ContainsKey(attribute.Header)) || !((TomlTable)model[attribute.Header]).ContainsKey(property.Name))
                        throw new Exception($"Toml file does not have required property '{property.Name}'.");

                    property.SetValue(instance, ((TomlTable)model[attribute.Header])[property.Name]);
                }
            }

            return instance;
        }
    }
}
