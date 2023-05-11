using System;
using System.Windows.Data;

namespace Common.Utility.xaml.NoNamespace;

/// <summary>A <see cref="ObjectDataProvider"/> preconfigured to provde enums. Just specify <see cref="EnumType"/>.</summary>
public class EnumProvider : ObjectDataProvider
{

    private Type? type;
    /// <summary>Gets or sets the enum type to represent.</summary>
    public Type? EnumType
    {
        get => type;
        set => SetType(value);
    }

    void SetType(Type? type)
    {

        this.type = type;

        MethodParameters.Clear();
        MethodName = "";
        ObjectType = null;

        if (type is not null)
        {
            MethodName = nameof(Enum.GetValues);
            ObjectType = typeof(Enum);
            _ = MethodParameters.Add(type);
        }

    }

}
