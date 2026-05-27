using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace SPTarkov.Reflection.Patching;

public abstract class AbstractPrepatch(ModuleDefinition serverCoreModule) : IPrepatch
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public abstract string ModGuid { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public abstract bool IsActive { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public abstract void Patch();

    /// <summary>
    ///     Gets a type definition from the server core module
    /// </summary>
    /// <typeparam name="T">Type to get the typedef for</typeparam>
    /// <returns>The typedef for the provided runtime type</returns>
    /// <exception cref="PatchException">Thrown if the type does not exist in the module definition</exception>
    protected TypeDefinition GetTypeDefinition<T>()
    {
        var result = serverCoreModule.GetAllTypes().FirstOrDefault(t => t.FullName == typeof(T).FullName);
        return result ?? throw new PatchException($"Could not a TypeDefinition for type: {typeof(T).FullName}");
    }

    /// <summary>
    ///     Gets a field definition from the server core module
    /// </summary>
    /// <param name="name">Name of the field</param>
    /// <typeparam name="T">Declaring type of the field</typeparam>
    /// <returns>The field definition for the provided type and name</returns>
    /// <exception cref="PatchException">Thrown if the field does not exist in the type definition</exception>
    protected FieldDefinition GetField<T>(string name)
    {
        var typeDef = GetTypeDefinition<T>();
        return typeDef.Fields.FirstOrDefault(f => f.Name == name)
            ?? throw new PatchException($"Could not locate a FieldDefinition for type: `{typeof(T).FullName}` and name `{name}`");
    }

    /// <summary>
    ///     Gets a property definition from the server core module
    /// </summary>
    /// <param name="name">Name of the property</param>
    /// <typeparam name="T">Declaring type of the property</typeparam>
    /// <returns>The property definition for the provided type and name</returns>
    /// <exception cref="PatchException">Thrown if the property does not exist in the type definition</exception>
    protected PropertyDefinition GetProperty<T>(string name)
    {
        var typeDef = GetTypeDefinition<T>();
        return typeDef.Properties.FirstOrDefault(p => p.Name == name)
            ?? throw new PatchException($"Could not locate a PropertyDefinition for type: `{typeof(T).FullName}` and name `{name}`");
    }

    /// <summary>
    ///     Gets a method definition from the server core module
    /// </summary>
    /// <param name="name">Name of the method</param>
    /// <typeparam name="T">Declaring type of the method</typeparam>
    /// <returns>The method definition for the provided type and name</returns>
    /// <exception cref="PatchException">Thrown if the method does not exist in the type definition</exception>
    protected MethodDefinition GetMethod<T>(string name)
    {
        var typeDef = GetTypeDefinition<T>();
        return typeDef.Methods.FirstOrDefault(m => m.Name == name)
            ?? throw new PatchException($"Could not locate a MethodDefinition for type: `{typeof(T).FullName}` and name `{name}`");
    }

    /// <summary>
    ///     Adds a new constant to an enum
    /// </summary>
    /// <param name="name">Name of the constant field, must be unique</param>
    /// <param name="constant">Number of the constant, must be unique</param>
    /// <typeparam name="T">Type of the enum to add the constant to</typeparam>
    protected void AddNewEnumConstant<T>(string name, int constant)
        where T : Enum
    {
        var typeDef = GetTypeDefinition<T>();

        var newEnum = new FieldDefinition(
            name,
            Mono.Cecil.FieldAttributes.Public
                | Mono.Cecil.FieldAttributes.Static
                | Mono.Cecil.FieldAttributes.Literal
                | Mono.Cecil.FieldAttributes.HasDefault,
            typeDef
        )
        {
            Constant = constant,
        };

        typeDef.Fields.Add(newEnum);
    }
}
