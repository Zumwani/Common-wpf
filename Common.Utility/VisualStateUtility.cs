using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Common.Utility
{

    public enum VisualState
    {
        Normal, Hover, Click
    }

    public static class VisualStateUtility
    {

        #region Events

        public static void AddStateChangedHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.AddHandler(StateChangedEvent, e);
        public static void RemoveStateChangedHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.RemoveHandler(StateChangedEvent, e);

        public static void AddLeftClickStartHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.AddHandler(LeftClickStartEvent, e);
        public static void AddRightClickStartHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.AddHandler(RightClickStartEvent, e);
        public static void RemoveLeftClickStartHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.RemoveHandler(LeftClickStartEvent, e);
        public static void RemoveRightClickStartHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.RemoveHandler(RightClickStartEvent, e);

        public static void AddLeftClickHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.AddHandler(LeftClickEvent, e);
        public static void AddRightClickHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.AddHandler(RightClickEvent, e);
        public static void RemoveLeftClickHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.RemoveHandler(LeftClickEvent, e);
        public static void RemoveRightClickHandler(FrameworkElement sender, RoutedEventHandler e) => sender?.RemoveHandler(RightClickEvent, e);

        public static readonly RoutedEvent StateChangedEvent = EventManager.RegisterRoutedEvent(
            "StateChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VisualStateUtility));

        public static readonly RoutedEvent LeftClickEvent = EventManager.RegisterRoutedEvent(
            "LeftClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(VisualStateUtility));

        public static readonly RoutedEvent RightClickEvent = EventManager.RegisterRoutedEvent(
            "RightClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(VisualStateUtility));

        public static readonly RoutedEvent LeftClickStartEvent = EventManager.RegisterRoutedEvent(
            "LeftClickStart", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(VisualStateUtility));

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

        public static ICommand GetLeftClickCommand(DependencyObject obj) => (ICommand)obj.GetValue(LeftClickCommandProperty);
        public static ICommand GetRightClickCommand(DependencyObject obj) => (ICommand)obj.GetValue(RightClickCommandProperty);
        public static object GetClickCommandParameter(DependencyObject obj) => (object)obj.GetValue(ClickCommandParameterProperty);
        public static object GetLeftClickCommandParameter(DependencyObject obj) => obj.GetValue(LeftClickCommandParameterProperty);
        public static object GetRightClickCommandParameter(DependencyObject obj) => obj.GetValue(RightClickCommandParameterProperty);

        public static void SetLeftClickCommand(DependencyObject obj, ICommand value) => obj.SetValue(LeftClickCommandProperty, value);
        public static void SetRightClickCommand(DependencyObject obj, ICommand value) => obj.SetValue(RightClickCommandProperty, value);
        public static void SetClickCommandParameter(DependencyObject obj, object value) => obj.SetValue(ClickCommandParameterProperty, value);
        public static void SetLeftClickCommandParameter(DependencyObject obj, object value) => obj.SetValue(LeftClickCommandParameterProperty, value);
        public static void SetRightClickCommandParameter(DependencyObject obj, object value) => obj.SetValue(RightClickCommandParameterProperty, value);

        public static readonly DependencyProperty ClickCommandParameterProperty = RegisterProperty<ICommand>("ClickCommandParameter");
        public static readonly DependencyProperty LeftClickCommandProperty = RegisterProperty<ICommand>("LeftClickCommand");
        public static readonly DependencyProperty RightClickCommandProperty = RegisterProperty<ICommand>("RightClickCommand");
        public static readonly DependencyProperty LeftClickCommandParameterProperty = RegisterProperty<ICommand>("LeftClickCommandParameter");
        public static readonly DependencyProperty RightClickCommandParameterProperty = RegisterProperty<ICommand>("RightClickCommandParameter");

        #endregion
        #region Properties

        public static bool GetIsEnabled(FrameworkElement obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(FrameworkElement obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        public static VisualState GetVisualState(FrameworkElement obj) => (VisualState)obj.GetValue(VisualStateProperty);
        public static void SetVisualState(FrameworkElement obj, VisualState value) => obj.SetValue(VisualStateProperty, value);

        public static bool? GetAnimateRightClick(DependencyObject obj) => (bool?)obj.GetValue(AnimateRightClickProperty);
        public static void SetAnimateRightClick(DependencyObject obj, bool? value) => obj.SetValue(AnimateRightClickProperty, value);

        public static bool GetShowContextMenuOnLeftClick(DependencyObject obj) => (bool)obj.GetValue(ShowContextMenuOnLeftClickProperty);
        public static void SetShowContextMenuOnLeftClick(DependencyObject obj, bool value) => obj.SetValue(ShowContextMenuOnLeftClickProperty, value);

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(VisualStateUtility), new PropertyMetadata(false, OnIsEnabledChanged));

        public static readonly DependencyProperty VisualStateProperty =
            DependencyProperty.RegisterAttached("VisualState", typeof(VisualState), typeof(VisualStateUtility), new PropertyMetadata(VisualState.Normal));

        public static readonly DependencyProperty AnimateRightClickProperty =
            DependencyProperty.RegisterAttached("AnimateRightClick", typeof(bool?), typeof(VisualStateUtility), new PropertyMetadata(null));

        public static readonly DependencyProperty ShowContextMenuOnLeftClickProperty =
            DependencyProperty.RegisterAttached("ShowContextMenuOnLeftClick", typeof(bool), typeof(VisualStateUtility), new PropertyMetadata(false));

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


        #endregion
        #region Block events

        public sealed class BlockedEventHelper : IDisposable
        {

            internal BlockedEventHelper(FrameworkElement element, RoutedEvent @event)
            {
                Element = element;
                Event = @event;
            }

            public FrameworkElement Element { get; }
            public RoutedEvent @Event { get; }
            public bool IsEnabled { get; private set; } = true;

            public void Dispose()
            {
                if (blockedEvents.ContainsKey(Element))
                {
                    blockedEvents[Element].Remove(this);
                    if (blockedEvents[Element].Count == 0)
                        blockedEvents.Remove(Element);
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
        public static BlockedEventHelper BlockEvent(FrameworkElement element, RoutedEvent e)
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

        static void UpdateVisualState(FrameworkElement element, bool? isMouseOver = null, bool? isPressed = null, bool? isRight = null, bool isContextMenuClose = false)
        {

            isMouseOver ??= GetVisualState(element) > VisualState.Normal;
            if (element.ContextMenu?.IsOpen ?? false)
            {
                element.ContextMenu.Closed -= ContextMenu_Closed;
                element.ContextMenu.Closed += ContextMenu_Closed;
                element.IsHitTestVisible = false;
                isPressed = true;
            }

            var animateClick = !(isRight ?? false) || (GetAnimateRightClick(element) ?? element.ContextMenu is not null);

            var state = VisualState.Normal;
            if (isMouseOver ?? false)
                state = VisualState.Hover;
            if (isPressed ?? false && animateClick)
                state = VisualState.Click;

            var prevState = GetVisualState(element);

            SetVisualState(element, state);
            RaiseEvent(StateChangedEvent, element);

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
                    RaiseEvent(LeftClickStartEvent, element);
                else
                {

                    if (GetShowContextMenuOnLeftClick(element))
                        element.ContextMenu?.SetValue(ContextMenu.IsOpenProperty, true);

                    if (RaiseEvent(LeftClickEvent, element))
                        InvokeCommand(LeftClickCommandProperty.Name);

                }

            }
            else if (isRight)
            {
                if (!isUp)
                    RaiseEvent(RightClickStartEvent, element);
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

}
