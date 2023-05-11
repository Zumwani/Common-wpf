using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Common.Utility;

/// <summary>Specifies the visual state of an element.</summary>
public enum VisualState
{
    /// <summary>Specifies that an element is in its normal state.</summary>
    Normal,
    /// <summary>Specifies that an element is in the hover state.</summary>
    Hover,
    /// <summary>Specifies that an element is in the click state.</summary>
    Click,
    /// <summary>Specifies that an element is in the checked state.</summary>
    Checked
}

/// <summary>Provides utility functions relating to the visual state of ui elements.</summary>
public static class VisualStateUtility
{

    #region Events

    /// <summary>Adds a event handler to <see cref="StateChangedEvent"/>.</summary>
    public static void AddStateChangedHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.AddHandler(StateChangedEvent, e);
    /// <summary>Removes a event handler to <see cref="StateChangedEvent"/>.</summary>
    public static void RemoveStateChangedHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.RemoveHandler(StateChangedEvent, e);

    /// <summary>Adds a event handler to <see cref="LeftClickStartEvent"/>.</summary>
    public static void AddLeftClickStartHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.AddHandler(LeftClickStartEvent, e);
    /// <summary>Adds a event handler to <see cref="RightClickStartEvent"/>.</summary>
    public static void AddRightClickStartHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.AddHandler(RightClickStartEvent, e);
    /// <summary>Removes a event handler to <see cref="LeftClickStartEvent"/>.</summary>
    public static void RemoveLeftClickStartHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.RemoveHandler(LeftClickStartEvent, e);
    /// <summary>Removes a event handler to <see cref="RightClickStartEvent"/>.</summary>
    public static void RemoveRightClickStartHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.RemoveHandler(RightClickStartEvent, e);

    /// <summary>Adds a event handler to <see cref="LeftClickEvent"/>.</summary>
    public static void AddLeftClickHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.AddHandler(LeftClickEvent, e);
    /// <summary>Adds a event handler to <see cref="RightClickEvent"/>.</summary>
    public static void AddRightClickHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.AddHandler(RightClickEvent, e);
    /// <summary>Removes a event handler to <see cref="LeftClickEvent"/>.</summary>
    public static void RemoveLeftClickHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.RemoveHandler(LeftClickEvent, e);
    /// <summary>Removes a event handler to <see cref="RightClickEvent"/>.</summary>
    public static void RemoveRightClickHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.RemoveHandler(RightClickEvent, e);

    /// <summary>An event that represents that an elements visual state has changed.</summary>
    public static readonly RoutedEvent StateChangedEvent = EventManager.RegisterRoutedEvent(
        "StateChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VisualStateUtility));

    /// <summary>An event that represents that the left mouse button has clicked on the element.</summary>
    public static readonly RoutedEvent LeftClickEvent = EventManager.RegisterRoutedEvent(
        "LeftClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(VisualStateUtility));

    /// <summary>An event that represents that the right mouse button has clicked on the element.</summary>
    public static readonly RoutedEvent RightClickEvent = EventManager.RegisterRoutedEvent(
        "RightClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(VisualStateUtility));

    /// <summary>An event that represents that the left mouse button has clicked on the element.</summary>
    public static readonly RoutedEvent LeftClickStartEvent = EventManager.RegisterRoutedEvent(
        "LeftClickStart", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(VisualStateUtility));

    /// <summary>An event that represents that the right mouse button has clicked on the element.</summary>
    public static readonly RoutedEvent RightClickStartEvent = EventManager.RegisterRoutedEvent(
        "RightClickStart", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(VisualStateUtility));

    #endregion
    #region Commands

    static readonly Dictionary<string, DependencyProperty> commandProperties = new();
    static DependencyProperty RegisterProperty<T>(string name)
    {
        var property = DependencyProperty.RegisterAttached(name, typeof(T), typeof(VisualStateUtility), new PropertyMetadata(null));
        commandProperties.Add(name, property);
        return property;
    }

    /// <summary>Gets the left click command.</summary>
    public static ICommand GetLeftClickCommand(DependencyObject obj) => (ICommand)obj.GetValue(LeftClickCommandProperty);
    /// <summary>Gets the right click command.</summary>
    public static ICommand GetRightClickCommand(DependencyObject obj) => (ICommand)obj.GetValue(RightClickCommandProperty);
    /// <summary>Gets the click command parameter.</summary>
    public static object GetClickCommandParameter(DependencyObject obj) => obj.GetValue(ClickCommandParameterProperty);
    /// <summary>Gets the left click command parameter.</summary>
    public static object GetLeftClickCommandParameter(DependencyObject obj) => obj.GetValue(LeftClickCommandParameterProperty);
    /// <summary>Gets the right click command parameter.</summary>
    public static object GetRightClickCommandParameter(DependencyObject obj) => obj.GetValue(RightClickCommandParameterProperty);

    /// <summary>Gets the left click command.</summary>
    public static void SetLeftClickCommand(DependencyObject obj, ICommand value) => obj.SetValue(LeftClickCommandProperty, value);
    /// <summary>Gets the right click command.</summary>
    public static void SetRightClickCommand(DependencyObject obj, ICommand value) => obj.SetValue(RightClickCommandProperty, value);
    /// <summary>Gets the left click command parameter.</summary>
    public static void SetClickCommandParameter(DependencyObject obj, object value) => obj.SetValue(ClickCommandParameterProperty, value);
    /// <summary>Gets the left click command parameter.</summary>
    public static void SetLeftClickCommandParameter(DependencyObject obj, object value) => obj.SetValue(LeftClickCommandParameterProperty, value);
    /// <summary>Gets the right click command parameter.</summary>
    public static void SetRightClickCommandParameter(DependencyObject obj, object value) => obj.SetValue(RightClickCommandParameterProperty, value);

    /// <summary>The click command parameter property.</summary>
    public static readonly DependencyProperty ClickCommandParameterProperty = RegisterProperty<ICommand>("ClickCommandParameter");
    /// <summary>The left click command property.</summary>
    public static readonly DependencyProperty LeftClickCommandProperty = RegisterProperty<ICommand>("LeftClickCommand");
    /// <summary>The right click command property.</summary>
    public static readonly DependencyProperty RightClickCommandProperty = RegisterProperty<ICommand>("RightClickCommand");
    /// <summary>The left click command parameter property.</summary>
    public static readonly DependencyProperty LeftClickCommandParameterProperty = RegisterProperty<ICommand>("LeftClickCommandParameter");
    /// <summary>The right click command parameter property.</summary>
    public static readonly DependencyProperty RightClickCommandParameterProperty = RegisterProperty<ICommand>("RightClickCommandParameter");

    #endregion
    #region Properties

    /// <summary>Gets whatever this element is managed by <see cref="VisualStateUtility"/>.</summary>
    public static bool GetIsEnabled(FrameworkElement obj) => (bool)obj.GetValue(IsEnabledProperty);
    /// <summary>Sets whatever this element is managed by <see cref="VisualStateUtility"/>.</summary>
    public static void SetIsEnabled(FrameworkElement obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    /// <summary>Gets the <see cref="VisualState"/> of this element.</summary>
    public static VisualState GetVisualState(FrameworkElement obj) => (VisualState)obj.GetValue(VisualStateProperty);
    /// <summary>Sets the <see cref="VisualState"/> of this element.</summary>
    public static void SetVisualState(FrameworkElement obj, VisualState value) => obj.SetValue(VisualStateProperty, value);

    /// <summary>Gets whatever this element should be animated on right click.</summary>
    public static bool? GetAnimateRightClick(DependencyObject obj) => (bool?)obj.GetValue(AnimateRightClickProperty);
    /// <summary>Sets whatever this element should be animated on right click.</summary>
    public static void SetAnimateRightClick(DependencyObject obj, bool? value) => obj.SetValue(AnimateRightClickProperty, value);

    /// <summary>Gets whatever this element should show its <see cref="FrameworkElement.ContextMenu"/> on left click.</summary>
    public static bool GetShowContextMenuOnLeftClick(DependencyObject obj) => (bool)obj.GetValue(ShowContextMenuOnLeftClickProperty);
    /// <summary>Sets whatever this element should show its <see cref="FrameworkElement.ContextMenu"/> on left click.</summary>
    public static void SetShowContextMenuOnLeftClick(DependencyObject obj, bool value) => obj.SetValue(ShowContextMenuOnLeftClickProperty, value);

    /// <summary>Gets whatever this element is checked.</summary>
    public static bool GetIsChecked(DependencyObject obj) => (bool)obj.GetValue(IsCheckedProperty);
    /// <summary>Sets whatever this element is checked.</summary>
    public static void SetIsChecked(DependencyObject obj, bool value) => obj.SetValue(IsCheckedProperty, value);

    /// <summary>The dependency property that determines if the visual state of an element should be managed by <see cref="VisualStateUtility"/>.</summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(VisualStateUtility), new PropertyMetadata(false, OnIsEnabledChanged));

    /// <summary>The dependency property that specifies the <see cref="VisualState"/> of an element.</summary>
    public static readonly DependencyProperty VisualStateProperty =
        DependencyProperty.RegisterAttached("VisualState", typeof(VisualState), typeof(VisualStateUtility), new PropertyMetadata(VisualState.Normal));

    /// <summary>The dependency property that specifies whatever right click should be animated on an element.</summary>
    public static readonly DependencyProperty AnimateRightClickProperty =
        DependencyProperty.RegisterAttached("AnimateRightClick", typeof(bool?), typeof(VisualStateUtility), new PropertyMetadata(null));

    /// <summary>The dependency property that specifies whatever <see cref="FrameworkElement.ContextMenu"/> should open on left click.</summary>
    public static readonly DependencyProperty ShowContextMenuOnLeftClickProperty =
        DependencyProperty.RegisterAttached("ShowContextMenuOnLeftClick", typeof(bool), typeof(VisualStateUtility), new PropertyMetadata(false));

    /// <summary>The dependency property that specifies whatever an element is checked.</summary>
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.RegisterAttached("IsChecked", typeof(bool), typeof(VisualStateUtility), new PropertyMetadata(false, OnCheckedChanged));

    #endregion
    #region Handlers

    static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not FrameworkElement element)
            return;

        element.MouseEnter -= Element_MouseEnter;
        element.MouseLeave -= Element_MouseLeave;
        element.PreviewMouseLeftButtonDown -= Element_PreviewMouseLeftButtonDown;
        element.PreviewMouseLeftButtonUp -= Element_PreviewMouseLeftButtonUp;
        element.PreviewMouseRightButtonDown -= Element_PreviewMouseRightButtonDown;
        element.PreviewMouseRightButtonUp -= Element_PreviewMouseRightButtonUp;

        if ((bool)e.NewValue)
        {
            element.MouseEnter += Element_MouseEnter;
            element.MouseLeave += Element_MouseLeave;
            element.PreviewMouseLeftButtonDown += Element_PreviewMouseLeftButtonDown;
            element.PreviewMouseLeftButtonUp += Element_PreviewMouseLeftButtonUp;
            element.PreviewMouseRightButtonDown += Element_PreviewMouseRightButtonDown;
            element.PreviewMouseRightButtonUp += Element_PreviewMouseRightButtonUp;
        }

    }

    #region Hover

    static void Element_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
            UpdateVisualState(element, isMouseOver: true);
    }

    static void Element_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
            UpdateVisualState(element, isMouseOver: false);
    }

    #endregion
    #region Left button

    static void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
            UpdateVisualState(element, isPressed: true);
    }

    static void Element_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
            UpdateVisualState(element, isPressed: false);
    }

    #endregion
    #region Right button

    static void Element_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
            UpdateVisualState(element, isRight: true, isPressed: true);
    }

    static void Element_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
            UpdateVisualState(element, isRight: true, isPressed: false);
    }

    #endregion
    #region Checked

    static void OnCheckedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            UpdateVisualState(element, isChecked: (bool)e.NewValue);
    }

    #endregion

    #endregion
    #region Block events

    /// <summary>A helper class that blocks an element from updating its <see cref="VisualState"/> while instanciated. <see cref="Dispose"/> to release block.</summary>
    public sealed class BlockedEventHelper : IDisposable
    {

        internal BlockedEventHelper(FrameworkElement element, RoutedEvent @event)
        {
            Element = element;
            Event = @event;
        }

        /// <summary>The element to block visual state updates for.</summary>
        public FrameworkElement Element { get; }
        /// <summary>The event to block.</summary>
        public RoutedEvent @Event { get; }

        /// <summary>Gets whatever the block is enabled.</summary>
        public bool IsEnabled { get; private set; } = true;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (blockedEvents.ContainsKey(Element))
            {
                _ = blockedEvents[Element].Remove(this);
                if (blockedEvents[Element].Count == 0)
                    _ = blockedEvents.Remove(Element);
            }
            IsEnabled = false;
        }

    }

    static readonly Dictionary<FrameworkElement, List<BlockedEventHelper>> blockedEvents = new();

    /// <summary>
    /// <para>Blocks the event. Block is automatically removed using <see cref="IDisposable.Dispose"/>.</para>
    /// <para>Usage:</para>
    /// <example>
    /// <code>
    /// <para/>using (<see cref="BlockEvent(FrameworkElement, RoutedEvent)"/>)
    /// <para/>{
    /// <para/>⠀⠀Your code here...
    /// <para/>}
    /// </code>
    /// </example>
    /// </summary>
    public static BlockedEventHelper? BlockEvent(FrameworkElement element, RoutedEvent e)
    {

        if (element is null)
            return null;

        if (!blockedEvents.ContainsKey(element))
            blockedEvents.Add(element, new());

        var helper = new BlockedEventHelper(element, e);
        blockedEvents[element].Add(helper);

        return helper;

    }

    static bool IsBlocked(FrameworkElement element, RoutedEvent e) =>
        blockedEvents.TryGetValue(element, out var l) && l.Any(h => h.Event == e);

    #endregion
    #region State switching

    static void UpdateVisualState(FrameworkElement element, bool? isMouseOver = null, bool? isPressed = null, bool? isChecked = null, bool? isRight = null, bool isContextMenuClose = false)
    {

        if (element.ContextMenu?.IsOpen ?? false)
        {
            element.ContextMenu.Closed -= ContextMenu_Closed;
            element.ContextMenu.Closed += ContextMenu_Closed;
            element.IsHitTestVisible = false;
            isPressed = true;
        }

        var animateClick = !(isRight ?? false) || (GetAnimateRightClick(element) ?? element.ContextMenu is not null);

        var state = VisualState.Normal;

        if (isChecked ?? false)
            state = VisualState.Checked;

        if (isMouseOver ?? false)
            state = VisualState.Hover;

        if ((isPressed ?? false) && animateClick)
            state = VisualState.Click;

        if (state == VisualState.Normal && element.IsMouseOver)
            state = VisualState.Hover;

        if (state == VisualState.Normal && GetIsChecked(element))
            state = VisualState.Checked;

        var prevState = GetVisualState(element);

        SetVisualState(element, state);
        _ = RaiseEvent(StateChangedEvent, element);

        if (isPressed.HasValue && !isContextMenuClose)
            RaiseClickEvents(element, isRight ?? false, isUp: prevState == VisualState.Click);

        void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            element.IsHitTestVisible = true;
            element.ContextMenu.Closed -= ContextMenu_Closed;
            UpdateVisualState(element, element.IsMouseOver, isPressed: false, isRight, isContextMenuClose: true);
        }

    }

    static void RaiseClickEvents(FrameworkElement element, bool isRight, bool isUp)
    {

        if (!isRight)
        {

            if (!isUp)
                _ = RaiseEvent(LeftClickStartEvent, element);
            else
            {

                if (GetShowContextMenuOnLeftClick(element) && element.ContextMenu is not null)
                {
                    element.ContextMenu.DataContext = element.DataContext;
                    element.ContextMenu.PlacementTarget = element;
                    element.ContextMenu.IsOpen = true;
                }

                if (RaiseEvent(LeftClickEvent, element))
                    InvokeCommand(LeftClickCommandProperty.Name);

            }

        }
        else if (isRight)
        {
            if (!isUp)
                _ = RaiseEvent(RightClickStartEvent, element);
            else if (RaiseEvent(RightClickEvent, element))
                InvokeCommand(RightClickCommandProperty.Name);
        }

        void InvokeCommand(string name)
        {
            var command = element.GetValue(commandProperties[name]) as ICommand;
            var parameter = element.GetValue(commandProperties[name + "Parameter"]);
            if (command?.CanExecute(parameter) ?? false)
                command.Execute(parameter);
        }

    }

    static bool RaiseEvent(RoutedEvent @event, FrameworkElement element)
    {

        if (IsBlocked(element, @event))
            return false;

        var e = new RoutedEventArgs(@event, element);
        element.RaiseEvent(e);
        return !e.Handled;

    }

    static void Element_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
            SetIsEnabled(element, false);
    }

    #endregion

}

