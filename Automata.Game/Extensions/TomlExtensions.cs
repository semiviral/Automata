using System;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

namespace Automata.Game.Extensions
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

                if (attribute is null)
                {
                    continue;
                }

                if (attribute.Header is null)
                {
                    if (!model.TryGetValue(property.Name, out object? value) && attribute.Required)
                    {
                        throw new Exception($"Toml file does not have required property '{property.Name}'.");
                    }

                    property.SetValue(instance, Convert.ChangeType(value, property.PropertyType));
                }
                else
                {
                    if ((attribute.Required && !model.ContainsKey(attribute.Header)) || !((TomlTable)model[attribute.Header]).ContainsKey(property.Name))
                    {
                        throw new Exception($"Toml file does not have required property '{property.Name}'.");
                    }

                    property.SetValue(instance, Convert.ChangeType(((TomlTable)model[attribute.Header])[property.Name], property.PropertyType));
                }
            }

            return instance;
        }
    }
}
