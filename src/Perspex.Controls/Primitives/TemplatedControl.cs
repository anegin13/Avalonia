﻿// Copyright (c) The Perspex Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Linq;
using System.Reactive.Linq;
using Perspex.Controls.Templates;
using Perspex.Data;
using Perspex.Interactivity;
using Perspex.LogicalTree;
using Perspex.Media;
using Perspex.Styling;
using Perspex.VisualTree;
using Serilog;
using Serilog.Core.Enrichers;

namespace Perspex.Controls.Primitives
{
    /// <summary>
    /// A lookless control whose visual appearance is defined by its <see cref="Template"/>.
    /// </summary>
    public class TemplatedControl : Control, ITemplatedControl
    {
        /// <summary>
        /// Defines the <see cref="Background"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> BackgroundProperty =
            Border.BackgroundProperty.AddOwner<TemplatedControl>();

        /// <summary>
        /// Defines the <see cref="BorderBrush"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> BorderBrushProperty =
            Border.BorderBrushProperty.AddOwner<TemplatedControl>();

        /// <summary>
        /// Defines the <see cref="BorderThickness"/> property.
        /// </summary>
        public static readonly StyledProperty<double> BorderThicknessProperty =
            Border.BorderThicknessProperty.AddOwner<TemplatedControl>();

        /// <summary>
        /// Defines the <see cref="FontFamily"/> property.
        /// </summary>
        public static readonly StyledProperty<string> FontFamilyProperty =
            TextBlock.FontFamilyProperty.AddOwner<TemplatedControl>();

        /// <summary>
        /// Defines the <see cref="FontSize"/> property.
        /// </summary>
        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<TemplatedControl>();

        /// <summary>
        /// Defines the <see cref="FontStyle"/> property.
        /// </summary>
        public static readonly StyledProperty<FontStyle> FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner<TemplatedControl>();

        /// <summary>
        /// Defines the <see cref="FontWeight"/> property.
        /// </summary>
        public static readonly StyledProperty<FontWeight> FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner<TemplatedControl>();

        /// <summary>
        /// Defines the <see cref="Foreground"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> ForegroundProperty =
            TextBlock.ForegroundProperty.AddOwner<TemplatedControl>();

        /// <summary>
        /// Defines the <see cref="Padding"/> property.
        /// </summary>
        public static readonly StyledProperty<Thickness> PaddingProperty =
            Decorator.PaddingProperty.AddOwner<TemplatedControl>();

        /// <summary>
        /// Defines the <see cref="Template"/> property.
        /// </summary>
        public static readonly StyledProperty<IControlTemplate> TemplateProperty =
            PerspexProperty.Register<TemplatedControl, IControlTemplate>("Template");

        /// <summary>
        /// Defines the IsTemplateFocusTarget attached property.
        /// </summary>
        public static readonly AttachedProperty<bool> IsTemplateFocusTargetProperty =
            PerspexProperty.RegisterAttached<TemplatedControl, Control, bool>("IsTemplateFocusTarget");

        /// <summary>
        /// Defines the <see cref="TemplateApplied"/> routed event.
        /// </summary>
        public static readonly RoutedEvent<TemplateAppliedEventArgs> TemplateAppliedEvent =
            RoutedEvent.Register<TemplatedControl, TemplateAppliedEventArgs>(
                "TemplateApplied", 
                RoutingStrategies.Direct);

        private bool _templateApplied;

        private readonly ILogger _templateLog;

        /// <summary>
        /// Initializes static members of the <see cref="TemplatedControl"/> class.
        /// </summary>
        static TemplatedControl()
        {
            ClipToBoundsProperty.OverrideDefaultValue<TemplatedControl>(true);
            TemplateProperty.Changed.AddClassHandler<TemplatedControl>(x => x.OnTemplateChanged);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatedControl"/> class.
        /// </summary>
        public TemplatedControl()
        {
            _templateLog = Log.ForContext(new[]
            {
                new PropertyEnricher("Area", "Template"),
                new PropertyEnricher("SourceContext", GetType()),
                new PropertyEnricher("Id", GetHashCode()),
            });
        }

        /// <summary>
        /// Raised when the control's template is applied.
        /// </summary>
        public event EventHandler<TemplateAppliedEventArgs> TemplateApplied
        {
            add { AddHandler(TemplateAppliedEvent, value); }
            remove { RemoveHandler(TemplateAppliedEvent, value); }
        }

        /// <summary>
        /// Gets or sets the brush used to draw the control's background.
        /// </summary>
        public Brush Background
        {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the brush used to draw the control's border.
        /// </summary>
        public Brush BorderBrush
        {
            get { return GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the thickness of the control's border.
        /// </summary>
        public double BorderThickness
        {
            get { return GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        /// <summary>
        /// Gets or sets the font family used to draw the control's text.
        /// </summary>
        public string FontFamily
        {
            get { return GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        /// <summary>
        /// Gets or sets the size of the control's text in points.
        /// </summary>
        public double FontSize
        {
            get { return GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the font style used to draw the control's text.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the font weight used to draw the control's text.
        /// </summary>
        public FontWeight FontWeight
        {
            get { return GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the brush used to draw the control's text and other foreground elements.
        /// </summary>
        public Brush Foreground
        {
            get { return GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the padding placed between the border of the control and its content.
        /// </summary>
        public Thickness Padding
        {
            get { return GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the template that defines the control's appearance.
        /// </summary>
        public IControlTemplate Template
        {
            get { return GetValue(TemplateProperty); }
            set { SetValue(TemplateProperty, value); }
        }

        /// <summary>
        /// Gets the value of the IsTemplateFocusTargetProperty attached property on a control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The property value.</returns>
        /// <see cref="SetIsTemplateFocusTarget(Control, bool)"/>
        public bool GetIsTemplateFocusTarget(Control control)
        {
            return control.GetValue(IsTemplateFocusTargetProperty);
        }

        /// <summary>
        /// Sets the value of the IsTemplateFocusTargetProperty attached property on a control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The property value.</param>
        /// <remarks>
        /// When a control is navigated to using the keyboard, a focus adorner is shown - usually
        /// around the control itself. However if the TemplatedControl.IsTemplateFocusTarget 
        /// attached property is set to true on an element in the control template, then the focus
        /// adorner will be shown around that control instead.
        /// </remarks>
        public void SetIsTemplateFocusTarget(Control control, bool value)
        {
            control.SetValue(IsTemplateFocusTargetProperty, value);
        }

        /// <inheritdoc/>
        public sealed override void ApplyTemplate()
        {
            if (!_templateApplied)
            {
                if (VisualChildren.Count > 0)
                {
                    foreach (var child in this.GetTemplateChildren())
                    {
                        child.SetValue(TemplatedParentProperty, null);
                    }

                    VisualChildren.Clear();
                }

                if (Template != null)
                {
                    _templateLog.Verbose("Creating control template");

                    var child = Template.Build(this);
                    var nameScope = new NameScope();
                    NameScope.SetNameScope((Control)child, nameScope);
                    child.SetValue(TemplatedParentProperty, this);
                    RegisterNames(child, nameScope);
                    ((ISetLogicalParent)child).SetParent(this);
                    VisualChildren.Add(child);

                    OnTemplateApplied(new TemplateAppliedEventArgs(nameScope));
                }

                _templateApplied = true;
            }
        }

        /// <inheritdoc/>
        protected sealed override IndexerDescriptor CreateBindingDescriptor(IndexerDescriptor source)
        {
            var result = base.CreateBindingDescriptor(source);

            // If the binding is a template binding, then complete when the Template changes.
            if (source.Priority == BindingPriority.TemplatedParent)
            {
                var templateChanged = this.GetObservable(TemplateProperty).Skip(1);

                result.SourceObservable = result.Source.GetObservable(result.Property)
                    .TakeUntil(templateChanged);
            }

            return result;
        }

        /// <inheritdoc/>
        protected override IControl GetTemplateFocusTarget()
        {
            foreach (Control child in this.GetTemplateChildren())
            {
                if (GetIsTemplateFocusTarget(child))
                {
                    return child;
                }
            }

            return this;
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            if (VisualChildren.Count > 0)
            {
                ((ILogical)VisualChildren[0]).NotifyDetachedFromLogicalTree(e);
            }

            base.OnDetachedFromLogicalTree(e);
        }

        /// <summary>
        /// Called when the control's template is applied.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// Called when the <see cref="Template"/> property changes.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnTemplateChanged(PerspexPropertyChangedEventArgs e)
        {
            if (_templateApplied && VisualChildren.Count > 0)
            {
                _templateApplied = false;
            }

            _templateApplied = false;
            InvalidateMeasure();
        }

        /// <summary>
        /// Registers each control with its name scope.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="nameScope">The name scope.</param>
        private void RegisterNames(IControl control, INameScope nameScope)
        {
            if (control.Name != null)
            {
                nameScope.Register(control.Name, control);
            }

            if (control.TemplatedParent == this)
            {
                foreach (IControl child in control.GetVisualChildren())
                {
                    RegisterNames(child, nameScope);
                }
            }
        }
    }
}
