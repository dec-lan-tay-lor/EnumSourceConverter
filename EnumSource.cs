using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Tonic.UI
{
    /// <summary>
    /// Return all enums value for a given enum type or a binding to a property with an enum Type
    /// Ussage: 
    /// </summary>
    public class EnumSource : MarkupExtension
    {
        private readonly Type _type;
        private readonly Binding _binding;

        [ThreadStatic]
        static Dictionary<Type, object[]> cache = new Dictionary<Type, object[]>();

        /// <summary>
        /// True if friendly names should be used. If used, the EnumBinding should be used instead of the WPF binding in order to convert back from the internal representation
        /// </summary>
        public bool Friendly { get; set; } = true;

        public EnumSource(object value)
        {
            if (value is Type)
            {
                var type = (Type)value;
                if (!type.IsEnum)
                    throw new ArgumentException("EnumToItemsSource requieres that the given type is a non-nullable enum");
                _type = type;
            }
            else if (value is Binding)
            {
                _binding = (Binding)value;
            }
            else if (value is string)
            {
                _binding = new Binding((string)value);
                _binding.Mode = BindingMode.OneWay;
            }
            else
                throw new ArgumentException("Value must be an enum type, a Binding or an string");
        }


        private Type GetPropertyType(IServiceProvider serviceProvider)
        {
            //provider of target object and it's property
            var targetProvider = (IProvideValueTarget)serviceProvider
                .GetService(typeof(IProvideValueTarget));
            if (targetProvider.TargetProperty is DependencyProperty)
            {
                return ((DependencyProperty)targetProvider.TargetProperty).PropertyType;
            }

            return targetProvider.TargetProperty.GetType();
        }

        static readonly DependencyProperty BindingProperty =
     DependencyProperty.RegisterAttached(
         "Binding",
         typeof(Type),
         typeof(EnumSource),
         new PropertyMetadata());

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            //If the type is unknown;
            if (_type == null)
            {
                //provider of target object and it's property
                var targetProvider = (IProvideValueTarget)serviceProvider
                    .GetService(typeof(IProvideValueTarget));
                var target = (FrameworkElement)targetProvider.TargetObject;

                //Extract indirectly the property type and the enum info using the GetTypeEnumConverter:
                var B = new Binding(_binding.Path.Path);
                B.Mode = BindingMode.TwoWay;
                var conv = new GetTypeEnumConverter();
                conv.UseDescription = Friendly;
                B.Converter = conv;

                var r = (BindingExpression)B.ProvideValue(serviceProvider);
                conv.expr = r;
                return r;
            }
            else
            {
                //The type is already known, extract enum info:
                if (Friendly)
                    return GetEnumValues(_type).Select(x => EnumValue.Create(x));
                else
                    return GetEnumValues(_type);
            }
        }

        /// <summary>
        /// Get the enum values for a given enum type
        /// </summary>
        /// <param name="Type">Enum or nullable enum type</param>
        /// <returns></returns>
        public static IEnumerable<object> GetEnumValues(Type Type)
        {
            //Extract nullable argument:
            if(Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type = Type.GetGenericArguments()[0];
            }

            object[] result;
            if (!cache.TryGetValue(Type, out result))
            {
                result = Enum.GetValues(Type).Cast<object>().ToArray();
                cache.Add(Type, result);
            }

            return result;
        }
    }
}
