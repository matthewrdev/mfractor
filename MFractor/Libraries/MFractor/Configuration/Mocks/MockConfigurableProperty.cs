using System;
using System.Reflection;

namespace MFractor.Configuration.Mocks
{
    public class MockConfigurableProperty : IConfigurableProperty
    {
        public object OwnerInstance { get; }

        public PropertyInfo Property { get; }

        public string Name { get; }

        public string Description { get; }

        public object DefaultValue { get; }

        public Type PropertyType { get; }

        public object Value 
        {
            get
            {
                return default(object);
            }
            set
            {
                // Do nothing;
            }
        }

        public MockConfigurableProperty(object ownerInstance, 
                                        System.Reflection.PropertyInfo property, 
                                        string name, 
                                        string description, 
                                        object defaultValue, 
                                        System.Type type)
        {
            OwnerInstance = ownerInstance;
            Property = property;
            Name = name;
            Description = description;
            DefaultValue = defaultValue;
            PropertyType = type;
        }

        public static MockConfigurableProperty From(IConfigurableProperty property)
        {
            return new MockConfigurableProperty(property.OwnerInstance, property.Property, property.Name, property.Description, property.DefaultValue, property.PropertyType);
        }
    }
}
