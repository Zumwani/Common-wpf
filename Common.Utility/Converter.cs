using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Common.Utility
{

    /// <inheritdoc cref="IValueConverter"/>/>
    public abstract class Converter : MarkupExtension, IValueConverter
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

    /// <inheritdoc cref="IValueConverter"/>/>
    public abstract class Converter<TOut> : MarkupExtension, IValueConverter
    {

        /// <inheritdoc cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/>/>
        public abstract TOut Convert(object value);

        /// <inheritdoc cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/>/>
        public virtual object ConvertBack(TOut value) =>
            throw new NotImplementedException();

        object IValueConverter.Convert(object value, Type _, object _1, CultureInfo _2) =>
            Convert(value);

        object IValueConverter.ConvertBack(object value, Type _, object _1, CultureInfo _2)
        {
            if (value is null || typeof(TOut).IsAssignableFrom(value.GetType()))
                return ConvertBack((TOut)value);
            throw new ArgumentException("Variable 'value' was not of correct type.", nameof(value));
        }

    }

    /// <inheritdoc cref="IValueConverter"/>/>
    public abstract class Converter<TIn, TOut> : MarkupExtension, IValueConverter
    {

        /// <inheritdoc cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/>/>
        public abstract TOut Convert(TIn value);

        /// <inheritdoc cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/>/>
        public virtual TIn ConvertBack(TOut value) =>
            throw new NotImplementedException();

        object IValueConverter.Convert(object value, Type _, object _1, CultureInfo _2)
        {
            if (value is null || typeof(TIn).IsAssignableFrom(value.GetType()))
                return Convert((TIn)value);
            throw new ArgumentException("Variable 'value' was not of correct type.", nameof(value));
        }

        object IValueConverter.ConvertBack(object value, Type _, object _1, CultureInfo _2)
        {
            if (value is null || typeof(TOut).IsAssignableFrom(value.GetType()))
                return ConvertBack((TOut)value);
            throw new ArgumentException("Variable 'value' was not of correct type.", nameof(value));
        }

    }

    /// <inheritdoc cref="IValueConverter"/>/>
    public abstract class Converter<TIn, TOut, TParameter> : MarkupExtension, IValueConverter
    {

        /// <inheritdoc cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/>/>
        public abstract TOut Convert(TIn value, TParameter parameter);

        /// <inheritdoc cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/>/>
        public virtual TIn ConvertBack(TOut value, TParameter parameter) =>
            throw new NotImplementedException();

        object IValueConverter.Convert(object value, Type _, object parameter, CultureInfo _1)
        {
            var param = (parameter is null || typeof(TParameter).IsAssignableFrom(parameter.GetType())) ? (TParameter)parameter : default;
            if (value is null || typeof(TIn).IsAssignableFrom(value.GetType()))
                return Convert((TIn)value, param);
            throw new ArgumentException("Variable 'value' was not of correct type.", nameof(value));
        }

        object IValueConverter.ConvertBack(object value, Type _, object parameter, CultureInfo _1)
        {
            var param = (parameter is null || typeof(TParameter).IsAssignableFrom(parameter.GetType())) ? (TParameter)parameter : default;
            if (value is null || typeof(TOut).IsAssignableFrom(value.GetType()))
                return ConvertBack((TOut)value, param);
            throw new ArgumentException("Variable 'value' was not of correct type.", nameof(value));
        }

    }

    public abstract class MultiConverter : MarkupExtension, IMultiValueConverter
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

    public abstract class MultiConverter<TIn, TOut> : MarkupExtension, IMultiValueConverter
    {

        public abstract TOut Convert(TIn[] values);
        public virtual TIn[] ConvertBack(TOut value) =>
            throw new NotImplementedException();

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (!values.All(v => v is null || typeof(TIn).IsAssignableFrom(v.GetType())))
                throw new ArgumentException("One or more variables in 'values' was not of correct type.");

            return Convert(values.Select(v => (TIn)v).ToArray());

        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (value is null || typeof(TOut).IsAssignableFrom(value.GetType()))
                return ConvertBack((TOut)value).Cast<object>().ToArray();
            throw new ArgumentException("Variable 'value' was not of correct type.", nameof(value));
        }

    }

    public abstract class MultiConverter<TIn, TOut, TParameter> : MarkupExtension, IMultiValueConverter
    {

        public abstract TOut Convert(TIn[] values, TParameter parameter);
        public virtual TIn[] ConvertBack(TOut value, TParameter parameter) =>
            throw new NotImplementedException();

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (!values.All(v => v is null || typeof(TIn).IsAssignableFrom(v.GetType())))
                throw new ArgumentException("One or more variables in 'values' was not of correct type.");

            var param = (parameter is null || typeof(TParameter).IsAssignableFrom(parameter.GetType())) ? (TParameter)parameter : default;
            return Convert(values.Select(v => (TIn)v).ToArray(), param);

        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var param = (parameter is null || typeof(TParameter).IsAssignableFrom(parameter.GetType())) ? (TParameter)parameter : default;
            if (value is null || typeof(TOut).IsAssignableFrom(value.GetType()))
                return ConvertBack((TOut)value, param).Cast<object>().ToArray();
            throw new ArgumentException("Variable 'value' was not of correct type.", nameof(value));
        }

    }

}
