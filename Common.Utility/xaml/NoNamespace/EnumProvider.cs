using System;
using System.Windows.Data;

namespace Common.Utility.xaml.NoNamespace;

public class EnumProvider : ObjectDataProvider
{

    private Type? type;
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
