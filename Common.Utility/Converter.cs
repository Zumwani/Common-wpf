using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Common.Utility
{

    static class ConverterUtility
    {

        public static bool Convert<T>(object value, out T outValue, CultureInfo culture = null) =>
            Convert<T, object>(value, null, out outValue, out _, culture, allowParamNull: true);

        public static bool Convert<T, TParam>(object value, object param, out T outValue, out TParam outParam, CultureInfo culture = null, bool allowParamNull = true)
        {

            culture ??= CultureInfo.InvariantCulture;

            var valueIsNull = value is null;
            var paramIsNull = param is null;

            outValue = value is IConvertible
                ? (T)System.Convert.ChangeType(value, typeof(T), culture)
                : (typeof(T).IsAssignableFrom(value?.GetType())
                    ? (T)value
                    : default);

            outParam = param is IConvertible
                ? (TParam)System.Convert.ChangeType(param, typeof(TParam), culture)
                : (typeof(TParam).IsAssignableFrom(param?.GetType())
                ? (TParam)param
                : default);

            return
                (valueIsNull || outValue is not null) &&
                (paramIsNull || outParam is not null || allowParamNull);

        }

        public static bool Convert<T>(object[] value, out T[] outValue, CultureInfo culture = null) =>
            Convert<T, object>(value, null, out outValue, out _, culture, allowParamNull: true);

        public static bool Convert<T, TParam>(object[] values, object param, out T[] outValue, out TParam outParam, CultureInfo culture = null, bool allowParamNull = true)
        {

            outValue = null;
            outParam = default;

            culture ??= CultureInfo.InvariantCulture;

            var v = values.Select(v => { var couldConvert = Convert<T>(v, out var newValue, culture); return (couldConvert, newValue); }).ToArray();
            if (v.Any(v1 => !v1.couldConvert))
                return false;

            outValue = v.Select(v => v.newValue).ToArray();

            outParam = (TParam)System.Convert.ChangeType(param, typeof(TParam), culture);
            return outParam is not null || allowParamNull;

        }

    }

    /// <inheritdoc cref="IValueConverter"/>
    public abstract class BetterConverter : MarkupExtension, IValueConverter
    {

        /// <inheritdoc cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/>/>
        public virtual object Convert(object value) =>
            throw new NotImplementedException();

        /// <inheritdoc cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/>/>
        public virtual object ConvertBack(object value) =>
            throw new NotImplementedException();

        /// <inheritdoc cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/>/>
        public virtual object Convert(object value, object parameter) =>
            Convert(value);

        /// <inheritdoc cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/>/>
        public virtual object ConvertBack(object value, object parameter) =>
            ConvertBack(value);

        object IValueConverter.Convert(object value, Type _, object parameter, CultureInfo _1) =>
            Convert(value, parameter);

        object IValueConverter.ConvertBack(object value, Type _, object parameter, CultureInfo _1) =>
            ConvertBack(value, parameter);

    }

    /// <inheritdoc cref="IValueConverter"/>
    public abstract class BetterConverter<TOut> : MarkupExtension, IValueConverter
    {

        /// <inheritdoc cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/>/>
        public abstract TOut Convert(object value);

        /// <inheritdoc cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/>/>
        public virtual object ConvertBack(TOut value) =>
            throw new NotImplementedException();

        object IValueConverter.Convert(object value, Type _, object _1, CultureInfo _2) =>
            Convert(value);

        object IValueConverter.ConvertBack(object value, Type _, object _1, CultureInfo culture) =>
            ConverterUtility.Convert<TOut>(value, out var _value, culture)
                ? ConvertBack(_value)
                : value;

    }

    /// <inheritdoc cref="IValueConverter"/>
    public abstract class BetterConverter<TIn, TOut> : MarkupExtension, IValueConverter
    {

        /// <inheritdoc cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/>/>
        public abstract TOut Convert(TIn value);

        /// <inheritdoc cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/>/>
        public virtual TIn ConvertBack(TOut value) =>
            throw new NotImplementedException();

        object IValueConverter.Convert(object value, Type _, object _1, CultureInfo culture) =>
            ConverterUtility.Convert<TIn>(value, out var _value, culture)
                ? Convert(_value ?? default)
                : value;

        object IValueConverter.ConvertBack(object value, Type _, object _1, CultureInfo culture) =>
           ConverterUtility.Convert<TOut>(value, out var _value, culture)
                ? ConvertBack(_value)
                : value;

    }

    /// <inheritdoc cref="IValueConverter"/>
    public abstract class BetterConverter<TIn, TOut, TParameter> : MarkupExtension, IValueConverter
    {

        /// <inheritdoc cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/>/>
        public abstract TOut Convert(TIn value, TParameter parameter);

        /// <inheritdoc cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/>/>
        public virtual TIn ConvertBack(TOut value, TParameter parameter) =>
            throw new NotImplementedException();

        object IValueConverter.Convert(object value, Type _, object parameter, CultureInfo culture) =>
            ConverterUtility.Convert<TIn, TParameter>(value, parameter, out var _value, out var _parameter, culture)
                ? Convert(_value, _parameter)
                : value;

        object IValueConverter.ConvertBack(object value, Type _, object parameter, CultureInfo culture) =>
            ConverterUtility.Convert<TOut, TParameter>(value, parameter, out var _value, out var _parameter, culture)
                ? ConvertBack(_value, _parameter)
                : value;

    }

    /// <inheritdoc cref="IMultiValueConverter"/>
    public abstract class BetterMultiConverter : MarkupExtension, IMultiValueConverter
    {

        public virtual object Convert(object[] values) =>
            throw new NotImplementedException();

        public virtual object[] ConvertBack(object value) =>
            throw new NotImplementedException();

        public virtual object Convert(object[] values, object parameter) =>
            Convert(values);

        public virtual object[] ConvertBack(object values, object parameter) =>
            ConvertBack(values);

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            Convert(values, parameter);

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            ConvertBack(value, parameter);

    }

    /// <inheritdoc cref="IMultiValueConverter"/>
    public abstract class BetterMultiConverter<TIn, TOut> : MarkupExtension, IMultiValueConverter
    {

        public abstract TOut Convert(TIn[] values);
        public virtual TIn[] ConvertBack(TOut value) =>
            throw new NotImplementedException();

        object IMultiValueConverter.Convert(object[] values, Type _, object _1, CultureInfo culture) =>
            ConverterUtility.Convert<TIn>(values, out var _values, culture)
                ? Convert(_values)
                : throw new ArgumentException("One or more variables in 'values' was not of correct type.");

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            ConverterUtility.Convert<TOut>(value, out var _value, culture)
                ? ConvertBack(_value).Cast<object>().ToArray()
                : throw new ArgumentException("Variable 'value' was not of correct type.", nameof(value));

    }

    /// <inheritdoc cref="IMultiValueConverter"/>
    public abstract class BetterMultiConverter<TIn, TOut, TParameter> : MarkupExtension, IMultiValueConverter
    {

        public abstract TOut Convert(TIn[] values, TParameter parameter);
        public virtual TIn[] ConvertBack(TOut value, TParameter parameter) =>
            throw new NotImplementedException();

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            ConverterUtility.Convert<TIn, TParameter>(values, parameter, out var _values, out var _param, culture: culture)
                ? Convert(_values, _param)
                : throw new ArgumentException("One or more variables in 'values' was not of correct type.");

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            ConverterUtility.Convert<TOut, TParameter>(value, parameter, out var _values, out var _param, culture: culture)
                ? ConvertBack(_values, _param).Cast<object>().ToArray()
                : throw new ArgumentException("Variable 'value' was not of correct type.", nameof(value));

    }

}
