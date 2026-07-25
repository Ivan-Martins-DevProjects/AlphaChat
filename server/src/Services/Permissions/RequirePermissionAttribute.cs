using Microsoft.AspNetCore.Mvc;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string entity, string operation)
        : base(typeof(PermissionFilter))
    {
        Arguments = [new PermissionRequirement(entity, operation)];
    }
}

public class PermissionRequirement
{
    public string Entity { get; }
    public string Operation { get; }

    public PermissionRequirement(string entity, string operation)
    {
        Entity = entity;
        Operation = operation;
    }
}
