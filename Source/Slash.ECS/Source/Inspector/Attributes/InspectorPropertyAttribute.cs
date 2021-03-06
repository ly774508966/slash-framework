﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InspectorPropertyAttribute.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.ECS.Inspector.Attributes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    using Slash.ECS.Components;
    using Slash.ECS.Inspector.Validation;
    using Slash.Reflection.Utils;

#if WINDOWS_STORE
    using Slash.Reflection.Extensions;
#endif

    /// <summary>
    ///   Exposes the property to the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [Serializable]
    public class InspectorPropertyAttribute : Attribute
    {
        #region Constants

        /// <summary>
        ///   Delimiter of list elements in strings.
        /// </summary>
        public const char ListDelimiter = ',';

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Exposes the property to the inspector.
        /// </summary>
        /// <param name="name">Property name to be shown in the inspector.</param>
        public InspectorPropertyAttribute(string name)
        {
            this.Name = name;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Type of value in attribute table. If null, the property type is used.
        /// </summary>
        public Type AttributeType { get; set; }

        /// <summary>
        ///   Default property value.
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        ///   Default list item.
        ///   Null for reference types, default value for value types.
        /// </summary>
        public object DefaultListItem
        {
            get
            {
                return this.ItemType.IsValueType ? Activator.CreateInstance(this.ItemType) : null;
            }
        }

        /// <summary>
        ///   A user-friendly description of the property.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///   Whether the property is of a list type, or not.
        /// </summary>
        public bool IsList
        {
            get
            {
                return typeof(IList).IsAssignableFrom(this.PropertyType);
            }
        }

        /// <summary>
        ///   Item type. Equals property type if no generic type, otherwise the type of the generic.
        /// </summary>
        public Type ItemType
        {
            get
            {
                var itemType = this.PropertyType;

                // If this property is of list type, get generic type argument for creating new list.
                if (ReflectionUtils.IsGenericType(itemType))
                {
                    itemType = itemType.GetGenericArguments()[0];
                }

                return itemType;
            }
        }

        /// <summary>
        ///   Whether the property is of a list type, or not.
        /// </summary>
        [Obsolete("Not required any more, determined by property type. Use IsList to check if property is a list.")]
        public bool List { get; set; }

        /// <summary>
        ///   Property name to be shown in the inspector.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   Returns a collection of values if the property has a defined set of possible values.
        ///   Otherwise <c>null</c> is returned.
        /// </summary>
        public virtual IEnumerable<object> PossibleValues
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        ///   Name of property this attribute belongs to.
        ///   Only set manually when collecting inspector type information.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        ///   Type of property this attribute belongs to.
        ///   Only set manually when collecting inspector type information.
        /// </summary>
        public Type PropertyType { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///   Converts the passed text to a value of the correct type for this property.
        /// </summary>
        /// <param name="text">Text to convert.</param>
        /// <returns>
        ///   Value of the correct type for this property, if the conversion was successful, and <c>null</c> otherwise.
        /// </returns>
        public virtual object ConvertStringToValue(string text)
        {
            object value;
            if (!this.TryConvertStringToValue(text, out value))
            {
                throw new ArgumentException("Can't convert string to value", "text");
            }
            return value;
        }

        /// <summary>
        ///   Converts the passed value or list to a string that can be converted back to a value or list of the correct type for
        ///   this property.
        /// </summary>
        /// <param name="value">Value or list to convert.</param>
        /// <returns>String that can be converted back to a value or list of the correct type for this property.</returns>
        /// <see cref="ConvertStringToValue" />
        public string ConvertValueOrListToString(object value)
        {
            if (this.IsList)
            {
                var list = (IEnumerable)value;
                var stringBuilder = new StringBuilder();

                foreach (var item in list)
                {
                    stringBuilder.Append(this.ConvertValueToString(item));
                    stringBuilder.Append(ListDelimiter);
                }

                return stringBuilder.Length > 0
                    ? stringBuilder.ToString().Substring(0, stringBuilder.Length - 1)
                    : string.Empty;
            }

            return value.ToString();
        }

        /// <summary>
        ///   Converts the passed value to a string that can be converted back to a value of the correct type for this property.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>String that can be converted back to a value of the correct type for this property.</returns>
        /// <see cref="ConvertStringToValue" />
        public virtual string ConvertValueToString(object value)
        {
            string text;
            if (!this.TryConvertValueToString(value, out text))
            {
                throw new ArgumentException("Can't convert value to string", "value");
            }
            return text;
        }

        /// <summary>
        ///   Performs required deinitialization steps for this property of the specified object.
        /// </summary>
        /// <param name="entityManager">Entity manager the object belongs to.</param>
        /// <param name="obj">Object the property belongs to.</param>
        public virtual void Deinit(EntityManager entityManager, object obj)
        {
        }

        /// <summary>
        ///   Gets an empty list for elements of the type of the property the attribute is attached to.
        /// </summary>
        /// <returns>Empty list of matching type.</returns>
        public virtual IList GetEmptyList()
        {
            var itemType = this.ItemType;
            return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
        }

        /// <summary>
        ///   Returns the value of the property this inspeoctor property is for.
        /// </summary>
        /// <param name="entityManager">Entity manager.</param>
        /// <param name="obj">Object to get property value for.</param>
        /// <returns>Property value.</returns>
        public virtual object GetPropertyValue(EntityManager entityManager, object obj)
        {
            return obj.GetType().GetProperty(this.PropertyName).GetValue(obj, null);
        }

        /// <summary>
        ///   Indicates if the specified value is allowed for the property.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>True if the specified value is allowed; otherwise, false.</returns>
        public virtual bool IsAllowed(object value)
        {
            return true;
        }

        /// <summary>
        ///   Initializes the specified object via reflection with the specified property value.
        /// </summary>
        /// <param name="entityManager">Entity manager.</param>
        /// <param name="obj">Object to set property value for.</param>
        /// <param name="propertyValue">Property value to set.</param>
        public virtual void SetPropertyValue(EntityManager entityManager, object obj, object propertyValue)
        {
            obj.GetType().GetProperty(this.PropertyName).SetValue(obj, propertyValue, null);
        }

        /// <summary>
        ///   Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///   A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("Name: {0}, Default: {1}", this.Name, this.Default);
        }

        /// <summary>
        ///   Tries to convert the specified text to a value or list of the correct type for this property.
        /// </summary>
        /// <param name="text">Text to convert.</param>
        /// <param name="value">Value or list of the correct type for this property, if the conversion was successful.</param>
        /// <returns>
        ///   True if the conversion was successful; otherwise, false.
        /// </returns>
        public bool TryConvertStringToListOrValue(string text, out object value)
        {
            var success = true;

            if (this.IsList)
            {
                // Split string into list items.
                var array = text.Split(ListDelimiter);
                var list = this.GetEmptyList();

                // Convert all items.
                foreach (var item in array)
                {
                    object convertedItem;

                    if (this.TryConvertStringToValue(item, out convertedItem))
                    {
                        list.Add(convertedItem);
                    }
                    else
                    {
                        success = false;
                    }
                }

                value = list;
                return success;
            }

            return this.TryConvertStringToValue(text, out value);
        }

        /// <summary>
        ///   Tries to convert the specified text to a value of the correct type for this property.
        /// </summary>
        /// <param name="text">Text to convert.</param>
        /// <param name="value">Value of the correct type for this property, if the conversion was successful.</param>
        /// <returns>
        ///   True if the conversion was successful; otherwise, false.
        /// </returns>
        public virtual bool TryConvertStringToValue(string text, out object value)
        {
            value = null;
            return false;
        }

        /// <summary>
        ///   Tries to convert the specified value to a string that can be converted back to a value of the correct type for this
        ///   property.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="text">String that can be converted back to a value of the correct type for this property.</param>
        /// <see cref="TryConvertStringToValue" />
        /// <returns>
        ///   True if the conversion was successful; otherwise, false.
        /// </returns>
        public virtual bool TryConvertValueToString(object value, out string text)
        {
            text = value != null ? value.ToString() : string.Empty;
            return true;
        }

        /// <summary>
        ///   Checks whether the passed value is valid for this property.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>
        ///   <c>null</c>, if the passed value is valid for this property,
        ///   and <see cref="ValidationError" /> which contains information about the error otherwise.
        /// </returns>
        public virtual ValidationError Validate(object value)
        {
            if (value == null)
            {
                return ValidationError.Null;
            }

            if (value.GetType() != this.PropertyType)
            {
                return ValidationError.WrongType;
            }

            return null;
        }

        #endregion
    }
}