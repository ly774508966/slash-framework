﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InspectorControl.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BlueprintEditor.Inspectors
{
    using System.Windows.Controls;

    using BlueprintEditor.ViewModels;

    using Slash.ECS.Inspector.Attributes;

    public class InspectorControl : UserControl, IInspectorControl
    {
        #region Fields

        /// <summary>
        ///   Data context of inspector.
        /// </summary>
        private InspectorPropertyData dataContext;

        #endregion

        #region Public Events

        /// <summary>
        ///   Raised when value of the inspector control changed.
        /// </summary>
        public event InspectorControlValueChangedDelegate ValueChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///   Current value of inspector control.
        /// </summary>
        public object Value
        {
            get
            {
                return this.dataContext.Value;
            }
            set
            {
                this.dataContext.Value = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///   Initializes the control with the inspector property it is for and the current value.
        /// </summary>
        /// <param name="inspectorProperty">Inspector property the control is for.</param>
        /// <param name="editorContext">Editor context this control lives in.</param>
        /// <param name="localizationContext">Context used for showing and changing localized attribute values.</param>
        /// <param name="currentValue">Current value.</param>
        /// <param name="valueInherited">Indicates if the current value was inherited.</param>
        public virtual void Init(InspectorPropertyAttribute inspectorProperty, EditorContext editorContext, LocalizationContext localizationContext, object currentValue, bool valueInherited)
        {
            // Setup data context of control.
            this.dataContext = new InspectorPropertyData(inspectorProperty, localizationContext) { EditorContext = editorContext, Value = currentValue, ValueInherited = valueInherited };
            this.dataContext.ValueChanged += this.OnValueChanged;
            this.DataContext = this.dataContext;
        }

        #endregion

        #region Methods

        protected void OnValueChanged()
        {
            this.OnValueChanged(this.dataContext.InspectorProperty, this.dataContext.Value);
        }

        protected void OnValueChanged(InspectorPropertyAttribute inspectorProperty, object newValue)
        {
            InspectorControlValueChangedDelegate handler = this.ValueChanged;
            if (handler != null)
            {
                handler(inspectorProperty, newValue);
            }
        }

        #endregion
    }
}