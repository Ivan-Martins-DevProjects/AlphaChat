public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key)
        : base("NOT_FOUND", $"{entity} com o valor de '{key}' não foi encontrado.", 404)
    {
    }
}
