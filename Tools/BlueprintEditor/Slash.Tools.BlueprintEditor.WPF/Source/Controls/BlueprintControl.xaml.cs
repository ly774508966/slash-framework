﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlueprintControl.xaml.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BlueprintEditor.Controls
{
    using System;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Input;

    using BlueprintEditor.Commands;
    using BlueprintEditor.Inspectors;
    using BlueprintEditor.ViewModels;

    using Slash.ECS.Inspector.Attributes;

    /// <summary>
    ///   Interaction logic for BlueprintControl.xaml
    /// </summary>
    public sealed partial class BlueprintControl
    {
        #region Fields

        private InspectorFactory inspectorFactory;
        
        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Constructor.
        /// </summary>
        public BlueprintControl()
        {
            this.InitializeComponent();

            this.DataContextChanged += this.OnDataContextChanged;
        }

        #endregion

        #region Delegates

        public delegate void SelectedBlueprintChangedDelegate(
            BlueprintViewModel newBlueprint, BlueprintViewModel oldBlueprint);

        #endregion

        #region Public Events

        public event SelectedBlueprintChangedDelegate SelectedBlueprintChaged;

        #endregion

        #region Public Properties

        public EditorContext EditorContext
        {
            set
            {
                this.inspectorFactory = new InspectorFactory(value, value != null ? value.LocalizationContext : null);
            }
        }

        #endregion

        #region Methods

        private void CanExecuteAddComponent(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.LbComponentsAvailable != null && this.LbComponentsAvailable.SelectedItems != null
                           && this.LbComponentsAvailable.SelectedItems.Count > 0;
        }

        private void CanExecuteRemoveComponent(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.LbComponentsAdded != null && this.LbComponentsAdded.SelectedItems != null
                           && this.LbComponentsAdded.SelectedItems.Count > 0;
        }

        private void ExecutedAddComponent(object sender, ExecutedRoutedEventArgs e)
        {
            // Get selected component type of available component types.
            Type componentType = (Type)this.LbComponentsAvailable.SelectedItem;
            if (componentType == null)
            {
                MessageBox.Show("No component type selected.");
                return;
            }

            BlueprintViewModel viewModel = (BlueprintViewModel)this.DataContext;
            viewModel.AddComponent(componentType);

            // Select component type.
            this.LbComponentsAdded.SelectedItem = componentType;
        }

        private void ExecutedRemoveComponent(object sender, ExecutedRoutedEventArgs e)
        {
            // Get selected component type of added component types.
            Type componentType = (Type)this.LbComponentsAdded.SelectedItem;
            if (componentType == null)
            {
                MessageBox.Show("No component type selected.");
                return;
            }

            BlueprintViewModel viewModel = (BlueprintViewModel)this.DataContext;

            if (!viewModel.RemoveComponent(componentType))
            {
                return;
            }

            // Select component type.
            this.LbComponentsAvailable.SelectedItem = componentType;
        }

        /// <summary>
        ///   Gets the current value of the specified property for the passed blueprint,
        ///   taking into account, in order: Blueprint attribute table, parents, default value.
        /// </summary>
        /// <param name="property">Property to get the current value of.</param>
        /// <returns>Current value of the specified property for the passed blueprint.</returns>
        private object GetCurrentAttributeValue(InspectorPropertyAttribute property, out bool inherited)
        {
            BlueprintViewModel viewModel = (BlueprintViewModel)this.DataContext;
            return viewModel.GetCurrentAttributeValue(property.Name, out inherited);
        }

        private void LbComponentsAdded_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Send command.
            BlueprintCommands.RemoveComponentCommand.Execute(null);
        }

        private void LbComponentsAvailable_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Send command.
            BlueprintCommands.AddComponentCommand.Execute(null);
        }

        private void OnBlueprintComponentsChanged(
            object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.UpdateInspectors();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            BlueprintViewModel oldViewModel = (BlueprintViewModel)e.OldValue;
            if (oldViewModel != null)
            {
                oldViewModel.AddedComponents.CollectionChanged -= this.OnBlueprintComponentsChanged;
            }

            BlueprintViewModel newViewModel = (BlueprintViewModel)e.NewValue;
            if (newViewModel != null)
            {
                newViewModel.AddedComponents.CollectionChanged += this.OnBlueprintComponentsChanged;
            }

            this.UpdateInspectors();

            this.OnSelectedBlueprintChanged(newViewModel, oldViewModel);
        }

        private void OnPropertyControlValueChanged(InspectorPropertyAttribute inspectorProperty, object newValue)
        {
            BlueprintViewModel viewModel = (BlueprintViewModel)this.DataContext;
            viewModel.SetAttributeValue(inspectorProperty.Name, newValue);
        }

        private void OnSelectedBlueprintChanged(BlueprintViewModel newBlueprint, BlueprintViewModel oldBlueprint)
        {
            var handler = this.SelectedBlueprintChaged;
            if (handler != null)
            {
                handler(newBlueprint, oldBlueprint);
            }
        }

        private void UpdateInspectors()
        {
            // Clear inspectors.
            this.AttributesPanel.Children.Clear();

            BlueprintViewModel viewModel = (BlueprintViewModel)this.DataContext;
            if (viewModel == null || viewModel.AddedComponents == null)
            {
                return;
            }

            // Add inspectors for blueprint components.
            this.inspectorFactory.AddComponentInspectorsRecursively(
                viewModel, this.AttributesPanel, this.GetCurrentAttributeValue, this.OnPropertyControlValueChanged);
        }

        #endregion
    }
}